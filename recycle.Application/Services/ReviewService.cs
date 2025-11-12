using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs.Reviews;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    //public class ReviewService : IReviewService
    //{
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly INotificationService _notificationService;

    //    public ReviewService(IUnitOfWork unitOfWork, INotificationService notificationService)
    //    {
    //        _unitOfWork = unitOfWork;
    //        _notificationService = notificationService;
    //    }

    //    public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto dto)
    //    {
    //        // Validate user can review
    //        var validation = await CanUserReviewRequestAsync(userId, dto.RequestId);
    //        if (!validation.IsValid)
    //            throw new InvalidOperationException(validation.ErrorMessage);

    //        // Get request with driver assignment
    //        var request = await _unitOfWork.PickupRequests.GetAsync(
    //            filter: r => r.RequestId == dto.RequestId,
    //            includes: q => q.Include(r => r.DriverAssignments));

    //        var activeAssignment = request.DriverAssignments
    //            .FirstOrDefault(da => da.IsActive);

    //        if (activeAssignment == null)
    //            throw new InvalidOperationException("No driver assigned to this request");

    //        // Create review
    //        var review = new Review
    //        {
    //            RequestId = dto.RequestId,
    //            ReviewerId = userId,
    //            RevieweeId = activeAssignment.DriverId,
    //            Rating = dto.Rating,
    //            Comment = dto.Comment?.Trim(),
    //            CreatedAt = DateTime.UtcNow,
    //            IsEdited = false
    //        };

    //        await _unitOfWork.Reviews.AddAsync(review);
    //        await _unitOfWork.SaveChangesAsync();

    //        // Update driver rating
    //        await UpdateDriverRatingAsync(activeAssignment.DriverId);

    //        // Send notification to driver
    //        await _notificationService.SendNotificationAsync(
    //            activeAssignment.DriverId,
    //            "NewReview",
    //            "New Review Received",
    //            $"You received a {dto.Rating}-star review from a customer",
    //            "Review",
    //            review.ReviewId,
    //            "Normal"
    //        );

    //        return await MapToDto(review);
    //    }

    //    public async Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
    //    {
    //        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

    //        if (review == null || review.ReviewerId != userId)
    //            throw new InvalidOperationException("Review not found or you don't have permission to update it");

    //        // Can only edit within 7 days
    //        var daysSinceCreation = (DateTime.UtcNow - review.CreatedAt).TotalDays;
    //        if (daysSinceCreation > 7)
    //            throw new InvalidOperationException("Reviews can only be edited within 7 days of creation");

    //        review.Rating = dto.Rating;
    //        review.Comment = dto.Comment?.Trim();
    //        review.UpdatedAt = DateTime.UtcNow;
    //        review.IsEdited = true;

    //        await _unitOfWork.Reviews.UpdateAsync(review);
    //        await _unitOfWork.SaveChangesAsync();

    //        // Recalculate driver rating
    //        await UpdateDriverRatingAsync(review.RevieweeId);

    //        return await MapToDto(review);
    //    }

    //    public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
    //    {
    //        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

    //        if (review == null || review.ReviewerId != userId)
    //            return false;

    //        var driverId = review.RevieweeId;

    //        await _unitOfWork.Reviews.RemoveAsync(review);
    //        await _unitOfWork.SaveChangesAsync();

    //        // Recalculate driver rating
    //        await UpdateDriverRatingAsync(driverId);

    //        return true;
    //    }

    //    public async Task<ReviewDto> GetReviewByIdAsync(int reviewId)
    //    {
    //        var review = await _unitOfWork.Reviews.GetAsync(
    //            filter: r => r.ReviewId == reviewId,
    //            includes: q => q.Include(r => r.Reviewer)
    //                           .Include(r => r.Reviewee)
    //                           .Include(r => r.PickupRequest));

    //        if (review == null)
    //            return null;

    //        return await MapToDto(review);
    //    }

    //    public async Task<ReviewDto> GetReviewByRequestIdAsync(int requestId)
    //    {
    //        var review = await _unitOfWork.Reviews.GetAsync(
    //            filter: r => r.RequestId == requestId,
    //            includes: q => q.Include(r => r.Reviewer)
    //                           .Include(r => r.Reviewee)
    //                           .Include(r => r.PickupRequest));

    //        return review != null ? await MapToDto(review) : null;
    //    }

    //    public async Task<List<ReviewDto>> GetReviewsForDriverAsync(int driverId, int page = 1, int pageSize = 20)
    //    {
    //        var reviews = await _unitOfWork.Reviews.GetAll(
    //            filter: r => r.RevieweeId == driverId && !r.IsHidden,
    //            includes: q => q.Include(r => r.Reviewer)
    //                           .Include(r => r.Reviewee)
    //                           .Include(r => r.PickupRequest),
    //            orderBy: q => q.OrderByDescending(r => r.CreatedAt),
    //            pageSize: pageSize,
    //            pageNumber: page);

    //        var reviewDtos = new List<ReviewDto>();
    //        foreach (var review in reviews)
    //        {
    //            reviewDtos.Add(await MapToDto(review));
    //        }
    //        return reviewDtos;
    //    }

    //    public async Task<List<ReviewDto>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20)
    //    {
    //        var reviews = await _unitOfWork.Reviews.GetAll(
    //            filter: r => r.ReviewerId == userId,
    //            includes: q => q.Include(r => r.Reviewer)
    //                           .Include(r => r.Reviewee)
    //                           .Include(r => r.PickupRequest),
    //            orderBy: q => q.OrderByDescending(r => r.CreatedAt),
    //            pageSize: pageSize,
    //            pageNumber: page);

    //        var reviewDtos = new List<ReviewDto>();
    //        foreach (var review in reviews)
    //        {
    //            reviewDtos.Add(await MapToDto(review));
    //        }
    //        return reviewDtos;
    //    }

    //    public async Task<List<PendingReviewDto>> GetPendingReviewsForUserAsync(int userId)
    //    {
    //        var completedRequests = await _unitOfWork.PickupRequests.GetAll(
    //            filter: pr => pr.UserId == userId && pr.Status == "Completed",
    //            includes: q => q.Include(pr => pr.DriverAssignments)
    //                           .ThenInclude(da => da.Driver)
    //                           .Include(pr => pr.Review));

    //        var pendingReviews = completedRequests
    //            .Where(pr => pr.Review == null) // No review yet
    //            .Select(pr =>
    //            {
    //                var activeDriver = pr.DriverAssignments.FirstOrDefault(da => da.IsActive);
    //                return new PendingReviewDto
    //                {
    //                    RequestId = pr.RequestId,
    //                    PickupAddress = pr.PickupAddress,
    //                    CompletedAt = pr.CompletedAt ?? DateTime.UtcNow,
    //                    DaysSinceCompletion = (int)(DateTime.UtcNow - (pr.CompletedAt ?? DateTime.UtcNow)).TotalDays,
    //                    DriverId = activeDriver?.DriverId ?? 0,
    //                    DriverName = activeDriver != null
    //                        ? $"{activeDriver.Driver.FirstName} {activeDriver.Driver.LastName}"
    //                        : "Unknown",
    //                    DriverRating = activeDriver?.Driver.AverageRating ?? 0
    //                };
    //            })
    //            .ToList();

    //        return pendingReviews;
    //    }

    //    public async Task<ValidationResult> CanUserReviewRequestAsync(int userId, int requestId)
    //    {
    //        var request = await _unitOfWork.PickupRequests.GetAsync(
    //            filter: r => r.RequestId == requestId,
    //            includes: q => q.Include(r => r.DriverAssignments));

    //        if (request == null)
    //            return ValidationResult.Fail("Request not found");

    //        if (request.UserId != userId)
    //            return ValidationResult.Fail("You can only review your own pickup requests");

    //        if (request.Status != "Completed")
    //            return ValidationResult.Fail("Request must be completed before you can leave a review");

    //        // Wait at least 1 hour after completion
    //        if (request.CompletedAt.HasValue)
    //        {
    //            var hoursSinceCompletion = (DateTime.UtcNow - request.CompletedAt.Value).TotalHours;
    //            if (hoursSinceCompletion < 1)
    //                return ValidationResult.Fail("Please wait at least 1 hour after pickup completion before reviewing");
    //        }

    //        // Cannot review after 30 days
    //        if (request.CompletedAt.HasValue)
    //        {
    //            var daysSinceCompletion = (DateTime.UtcNow - request.CompletedAt.Value).TotalDays;
    //            if (daysSinceCompletion > 30)
    //                return ValidationResult.Fail("Review period has expired (30 days maximum)");
    //        }

    //        // Check if already reviewed
    //        var existingReview = await _unitOfWork.Reviews.AnyAsync(r => r.RequestId == requestId);

    //        if (existingReview)
    //            return ValidationResult.Fail("You have already reviewed this pickup");

    //        return ValidationResult.Success();
    //    }

    //    public async Task UpdateDriverRatingAsync(int driverId)
    //    {
    //        var driver = await _unitOfWork.Users.GetByIdAsync(driverId);
    //        if (driver == null) return;

    //        var reviews = await _unitOfWork.Reviews.GetAll(
    //            filter: r => r.RevieweeId == driverId);

    //        if (reviews.Any())
    //        {
    //            driver.AverageRating = (decimal)reviews.Average(r => r.Rating);
    //            driver.TotalReviews = reviews.Count();
    //        }
    //        else
    //        {
    //            driver.AverageRating = 0;
    //            driver.TotalReviews = 0;
    //        }

    //        await _unitOfWork.Users.UpdateAsync(driver);
    //        await _unitOfWork.SaveChangesAsync();
    //    }

    //    public async Task<DriverRatingDto> GetDriverRatingStatsAsync(int driverId)
    //    {
    //        var driver = await _unitOfWork.Users.GetByIdAsync(driverId);
    //        if (driver == null) return null;

    //        var reviews = await _unitOfWork.Reviews.GetAll(
    //            filter: r => r.RevieweeId == driverId);

    //        var totalPickups = await _unitOfWork.DriverAssignments.CountAsync(
    //            da => da.DriverId == driverId && da.Status == "Completed");

    //        var completedAssignments = await _unitOfWork.DriverAssignments.CountAsync(
    //            da => da.DriverId == driverId && da.Status == "Completed");

    //        var totalAssignments = await _unitOfWork.DriverAssignments.CountAsync(
    //            da => da.DriverId == driverId);

    //        var completionRate = totalAssignments > 0
    //            ? (double)completedAssignments / totalAssignments * 100
    //            : 0;

    //        var reviewsList = reviews.ToList();

    //        return new DriverRatingDto
    //        {
    //            DriverId = driverId,
    //            DriverName = $"{driver.FirstName} {driver.LastName}",
    //            AverageRating = driver.AverageRating,
    //            TotalReviews = driver.TotalReviews,
    //            FiveStars = reviewsList.Count(r => r.Rating == 5),
    //            FourStars = reviewsList.Count(r => r.Rating == 4),
    //            ThreeStars = reviewsList.Count(r => r.Rating == 3),
    //            TwoStars = reviewsList.Count(r => r.Rating == 2),
    //            OneStar = reviewsList.Count(r => r.Rating == 1),
    //            TotalPickups = totalPickups,
    //            CompletionRate = Math.Round(completionRate, 2)
    //        };
    //    }

    //    public async Task<bool> FlagReviewAsync(int reviewId, string reason)
    //    {
    //        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
    //        if (review == null) return false;

    //        review.IsFlagged = true;
    //        review.FlagReason = reason;
    //        review.FlaggedAt = DateTime.UtcNow;
    //        review.IsHidden = true;

    //        await _unitOfWork.Reviews.UpdateAsync(review);
    //        await _unitOfWork.SaveChangesAsync();

    //        return true;
    //    }

    //    public async Task<bool> HideReviewAsync(int reviewId, bool hide)
    //    {
    //        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
    //        if (review == null) return false;

    //        review.IsHidden = hide;

    //        await _unitOfWork.Reviews.UpdateAsync(review);
    //        await _unitOfWork.SaveChangesAsync();

    //        return true;
    //    }

    //    public async Task<List<ReviewDto>> GetFlaggedReviewsAsync()
    //    {
    //        var reviews = await _unitOfWork.Reviews.GetAll(
    //            filter: r => r.IsFlagged,
    //            includes: q => q.Include(r => r.Reviewer)
    //                           .Include(r => r.Reviewee)
    //                           .Include(r => r.PickupRequest));

    //        var reviewDtos = new List<ReviewDto>();
    //        foreach (var review in reviews)
    //        {
    //            reviewDtos.Add(await MapToDto(review));
    //        }
    //        return reviewDtos;
    //    }

    //    private async Task<ReviewDto> MapToDto(Review review)
    //    {
    //        // Load related entities if not already loaded
    //        if (review.Reviewer == null)
    //            review.Reviewer = await _unitOfWork.Users.GetByIdAsync(review.ReviewerId);

    //        if (review.Reviewee == null)
    //            review.Reviewee = await _unitOfWork.Users.GetByIdAsync(review.RevieweeId);

    //        if (review.PickupRequest == null)
    //            review.PickupRequest = await _unitOfWork.PickupRequests.GetByIdAsync(review.RequestId);

    //        return new ReviewDto
    //        {
    //            ReviewId = review.ReviewId,
    //            RequestId = review.RequestId,
    //            ReviewerId = review.ReviewerId,
    //            ReviewerName = $"{review.Reviewer.FirstName} {review.Reviewer.LastName}",
    //            RevieweeId = review.RevieweeId,
    //            RevieweeName = $"{review.Reviewee.FirstName} {review.Reviewee.LastName}",
    //            Rating = review.Rating,
    //            Comment = review.Comment,
    //            CreatedAt = review.CreatedAt,
    //            UpdatedAt = review.UpdatedAt,
    //            IsEdited = review.IsEdited,
    //            PickupAddress = review.PickupRequest?.PickupAddress ?? "N/A"
    //        };
    //    }
    //}
}