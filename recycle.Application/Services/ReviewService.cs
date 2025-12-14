// ============================================
// ReviewService.cs - FINAL CORRECT VERSION
// Using existing repository includes functionality
// ============================================

using recycle.Application.DTOs.Reviews;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace recycle.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRepository<PickupRequest> _pickupRequestRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _notificationService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRepository<PickupRequest> pickupRequestRepository,
        IRepository<ApplicationUser> userRepository,
        IUnitOfWork unitOfWork,
        INotificationHubService notificationService)
    {
        _reviewRepository = reviewRepository;
        _pickupRequestRepository = pickupRequestRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    // CREATE
    public async Task<Review> CreateReview(Guid userId, CreateReviewDto dto)
    {
        Console.WriteLine($"🔍 CreateReview called - UserId: {userId}, RequestId: {dto.RequestId}");

        // 1️⃣ Get the pickup request WITH driver assignments using includes
        var request = await _pickupRequestRepository.GetByIdAsync(
            dto.RequestId,
            includes: query => query
                .Include(r => r.DriverAssignments)
                .ThenInclude(da => da.Driver)
        );

        if (request == null)
        {
            Console.WriteLine($"❌ Pickup request {dto.RequestId} not found");
            throw new InvalidOperationException("Pickup request not found");
        }

        Console.WriteLine($"✅ Found request: Status={request.Status}, UserId={request.UserId}");

        // 2️⃣ Verify it's the user's request
        if (request.UserId != userId)
        {
            Console.WriteLine($"❌ Request belongs to user {request.UserId}, not {userId}");
            throw new UnauthorizedAccessException("You can only review your own pickups");
        }

        // 3️⃣ Verify request is completed
        if (request.Status != "Completed")
        {
            Console.WriteLine($"❌ Request status is {request.Status}, not Completed");
            throw new InvalidOperationException("Can only review completed pickup requests");
        }

        // 4️⃣ Check if review already exists
        var existingReview = await _reviewRepository.GetByRequestIdAsync(dto.RequestId);
        if (existingReview != null)
        {
            Console.WriteLine($"❌ Review already exists for request {dto.RequestId}");
            throw new InvalidOperationException("You have already reviewed this pickup");
        }

        // 5️⃣ ✅ GET DRIVER ID FROM REQUEST
        var driverAssignment = request.DriverAssignments?
            .Where(da => da.Status == AssignmentStatus.Completed ||
                        da.Status == AssignmentStatus.Assigned)
            .OrderByDescending(da => da.AssignedAt)
            .FirstOrDefault();

        if (driverAssignment == null)
        {
            Console.WriteLine($"❌ No driver assignment found for request {dto.RequestId}");
            throw new InvalidOperationException("This pickup request has no assigned driver");
        }

        var driverId = driverAssignment.DriverId;
        Console.WriteLine($"✅ Found driver ID from assignment: {driverId}");

        // 6️⃣ Create review
        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            RequestId = dto.RequestId,
            RevieweeId = driverId,  // ✅ From driver assignment
            ReviewerId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            IsFlagged = false,
            IsHidden = false
        };

        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"✅ Review created successfully: {review.ReviewId}");

        // 7️⃣ Update driver rating
        await UpdateDriverRatingAsync(driverId);

        // 8️⃣ Send notification
        await _notificationService.SendToUser(driverId, new NotificationDto
        {
            Title = "New Review Received",
            Message = $"You received a {dto.Rating}-star review from a customer.",
            Type = NotificationTypes.NewReview,
            RelatedEntityType = NotificationEntityTypes.Review,
            RelatedEntityId = review.ReviewId
        });

        return review;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto)
    {
        var review = await CreateReview(userId, dto);
        return await MapToDtoAsync(review);
    }

    // READ
    public async Task<ReviewDto> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return null;

        return await MapToDtoAsync(review);
    }

    public async Task<ReviewDto> GetReviewByRequestIdAsync(Guid requestId)
    {
        var review = await _reviewRepository.GetByRequestIdAsync(requestId);
        if (review == null)
            return null;

        return await MapToDtoAsync(review);
    }

    public async Task<List<ReviewDto>> GetReviewsForDriverAsync(Guid driverId, int page = 1, int pageSize = 20)
    {
        var allReviews = await _reviewRepository.GetAllAsync();
        var driverReviews = allReviews
            .Where(r => r.RevieweeId == driverId && !r.IsHidden)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in driverReviews)
        {
            reviewDtos.Add(await MapToDtoAsync(review));
        }

        return reviewDtos;
    }

    public async Task<List<ReviewDto>> GetReviewsByUserAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var allReviews = await _reviewRepository.GetAllAsync();
        var userReviews = allReviews
            .Where(r => r.ReviewerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in userReviews)
        {
            reviewDtos.Add(await MapToDtoAsync(review));
        }

        return reviewDtos;
    }

    // GetPendingReviewsForUserAsync - Using includes
    public async Task<List<PendingReviewDto>> GetPendingReviewsForUserAsync(Guid userId)
    {
        Console.WriteLine($"📋 GetPendingReviewsForUserAsync - UserId: {userId}");

        // ✅ Get completed requests WITH driver assignments included
        var completedRequests = await _pickupRequestRepository.GetAll(
            filter: r => r.UserId == userId && r.Status == "Completed",
            includes: query => query
                .Include(r => r.DriverAssignments)
                .ThenInclude(da => da.Driver)
        );

        Console.WriteLine($"✅ Found {completedRequests.Count()} completed requests");

        // Get reviewed request IDs
        var allReviews = await _reviewRepository.GetAllAsync();
        var reviewedRequestIds = allReviews
            .Where(r => r.ReviewerId == userId)
            .Select(r => r.RequestId)
            .ToHashSet();

        Console.WriteLine($"✅ User has already reviewed {reviewedRequestIds.Count} requests");

        var pendingReviews = new List<PendingReviewDto>();

        foreach (var request in completedRequests.Where(r => !reviewedRequestIds.Contains(r.RequestId)))
        {
            Console.WriteLine($"🔍 Processing request: {request.RequestId}");

            // Get driver from assignments (already included)
            var driverAssignment = request.DriverAssignments?
                .Where(da => da.Status == AssignmentStatus.Completed ||
                            da.Status == AssignmentStatus.Assigned)
                .OrderByDescending(da => da.AssignedAt)
                .FirstOrDefault();

            if (driverAssignment == null)
            {
                Console.WriteLine($"⚠️ No driver assignment found for request {request.RequestId}");
                continue;
            }

            var driverId = driverAssignment.DriverId;
            var driver = driverAssignment.Driver; // Already included!

            var driverName = driver != null
                ? $"{driver.User.FirstName} {driver.User.LastName}".Trim()
                : "Unknown Driver";

            Console.WriteLine($"✅ Driver: {driverId} - {driverName}");

            pendingReviews.Add(new PendingReviewDto
            {
                RequestId = request.RequestId,
                DriverId = driverId,
                CompletedAt = request.CompletedAt ?? DateTime.UtcNow,
                DriverName = driverName
            });
        }

        Console.WriteLine($"✅ Returning {pendingReviews.Count} pending reviews");

        // Send reminder notification
        if (pendingReviews.Any())
        {
            await _notificationService.SendToUser(userId, new NotificationDto
            {
                Title = "Review Reminder",
                Message = $"You have {pendingReviews.Count} completed pickup(s) waiting for your review.",
                Type = NotificationTypes.ReviewReminder,
                RelatedEntityType = NotificationEntityTypes.Review,
                RelatedEntityId = null
            });
        }

        return pendingReviews;
    }

    // UPDATE
    public async Task<ReviewDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
            throw new InvalidOperationException("Review not found");

        if (review.ReviewerId != userId)
            throw new UnauthorizedAccessException("You can only update your own reviews");

        if ((DateTime.UtcNow - review.CreatedAt).TotalDays > 7)
            throw new InvalidOperationException("Reviews can only be updated within 7 days");

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await UpdateDriverRatingAsync(review.RevieweeId);

        return await MapToDtoAsync(review);
    }

    // DELETE
    public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
            return false;
        if (review.ReviewerId != userId)
            return false;

        var driverId = review.RevieweeId;

        await _reviewRepository.RemoveAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await UpdateDriverRatingAsync(driverId);

        return true;
    }

    // VALIDATION
    public async Task<ValidationResult> CanUserReviewRequestAsync(Guid userId, Guid requestId)
    {
        var request = await _pickupRequestRepository.GetByIdAsync(requestId);

        if (request == null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Pickup request not found" };

        if (request.UserId != userId)
            return new ValidationResult { IsValid = false, ErrorMessage = "This is not your pickup request" };

        if (request.Status != "Completed")
            return new ValidationResult { IsValid = false, ErrorMessage = "Can only review completed pickups" };

        var existingReview = await _reviewRepository.GetByRequestIdAsync(requestId);
        if (existingReview != null)
            return new ValidationResult { IsValid = false, ErrorMessage = "You have already reviewed this pickup" };

        return new ValidationResult { IsValid = true, ErrorMessage = null };
    }

    // DRIVER RATING
    public async Task UpdateDriverRatingAsync(Guid driverId)
    {
        var allReviews = await _reviewRepository.GetAllAsync();
        var driverReviews = allReviews
            .Where(r => r.RevieweeId == driverId && !r.IsHidden)
            .ToList();

        if (driverReviews.Any())
        {
            var averageRating = driverReviews.Average(r => r.Rating);

            var driver = await _unitOfWork.DriverProfiles.GetAsync(p => p.UserId == driverId);
            if (driver != null)
            {
                driver.Rating = Math.Round((decimal)averageRating, 2);
                driver.ratingCount = driverReviews.Count;
                await _unitOfWork.DriverProfiles.UpdateAsync(driver);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }

    public async Task<DriverRatingDto> GetDriverRatingStatsAsync(Guid driverId)
    {
        var allReviews = await _reviewRepository.GetAllAsync();
        var driverReviews = allReviews
            .Where(r => r.RevieweeId == driverId && !r.IsHidden)
            .ToList();

        if (!driverReviews.Any())
            return null;

        var averageRating = driverReviews.Average(r => r.Rating);
        var ratingDistribution = driverReviews
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        return new DriverRatingDto
        {
            DriverId = driverId,
            AverageRating = Math.Round(averageRating, 2),
            TotalReviews = driverReviews.Count,
            FiveStarCount = ratingDistribution.GetValueOrDefault(5, 0),
            FourStarCount = ratingDistribution.GetValueOrDefault(4, 0),
            ThreeStarCount = ratingDistribution.GetValueOrDefault(3, 0),
            TwoStarCount = ratingDistribution.GetValueOrDefault(2, 0),
            OneStarCount = ratingDistribution.GetValueOrDefault(1, 0)
        };
    }

    // ADMIN OPERATIONS
    public async Task<bool> FlagReviewAsync(Guid reviewId, string reason)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
            return false;

        review.IsFlagged = true;
        review.FlagReason = reason;
        review.FlaggedAt = DateTime.UtcNow;
        review.IsHidden = true;

        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendToRole("Admin", new NotificationDto
        {
            Title = "Review Flagged",
            Message = $"A review has been flagged for inappropriate content. Reason: {reason}",
            Type = NotificationTypes.ReviewFlagged,
            RelatedEntityType = NotificationEntityTypes.Review,
            RelatedEntityId = reviewId
        });

        return true;
    }

    public async Task<bool> HideReviewAsync(Guid reviewId, bool hide)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
            return false;

        review.IsHidden = hide;

        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<ReviewDto>> GetFlaggedReviewsAsync()
    {
        var allReviews = await _reviewRepository.GetAllAsync();
        var flaggedReviews = allReviews
            .Where(r => r.IsFlagged)
            .OrderByDescending(r => r.FlaggedAt)
            .ToList();

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in flaggedReviews)
        {
            reviewDtos.Add(await MapToDtoAsync(review));
        }

        return reviewDtos;
    }

    // HELPER METHODS
    private async Task<ReviewDto> MapToDtoAsync(Review review)
    {
        var customer = await _userRepository.GetByIdAsync(review.ReviewerId);
        var driver = await _userRepository.GetByIdAsync(review.RevieweeId);

        return new ReviewDto
        {
            ReviewId = review.ReviewId,
            RequestId = review.RequestId,
            DriverId = review.RevieweeId,
            CustomerId = review.ReviewerId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            IsFlagged = review.IsFlagged,
            IsHidden = review.IsHidden,
            FlagReason = review.FlagReason,
            CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer",
            DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : "Unknown Driver"
        };
    }
}