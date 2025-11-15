using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs.Reviews;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto)
        {
            var userGuid = userId;
            var requestGuid = dto.RequestId;

            // Validate
            var validation = await CanUserReviewRequestAsync(userId, dto.RequestId);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Get request with driver
            var request = await _unitOfWork.PickupRequests.GetAsync(
                filter: r => r.RequestId == requestGuid,
                includes: q => q.Include(r => r.DriverAssignments)
                                   .ThenInclude(da => da.Driver));
                                      

            var activeAssignment = request.DriverAssignments.FirstOrDefault(da => da.IsActive);
            if (activeAssignment == null)
                throw new InvalidOperationException("No driver assigned to this request");

            // Create review
            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                RequestId = requestGuid,
                ReviewerId = userGuid,
                RevieweeId = activeAssignment.DriverId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsEdited = false
            };

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Update driver rating
            await UpdateDriverRatingInternalAsync(activeAssignment.DriverId);

            // Send notification
            await _notificationService.SendNotificationAsync(
                activeAssignment.DriverId,
                "NewReview",
                "New Review Received",
                $"You received a {dto.Rating}-star review from a customer",
                "Review",
                review.ReviewId,
                "Normal"
            );

            return await MapToDto(review);
        }

        public async Task<ReviewDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto)
        {
            var userGuid = userId;
            var reviewGuid = reviewId;

            var review = await _reviewRepository.GetByIdAsync(reviewGuid);

            if (review == null || review.ReviewerId != userGuid)
                throw new InvalidOperationException("Review not found or you don't have permission");

            var daysSinceCreation = (DateTime.UtcNow - review.CreatedAt).TotalDays;
            if (daysSinceCreation > 7)
                throw new InvalidOperationException("Reviews can only be edited within 7 days");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();
            review.UpdatedAt = DateTime.UtcNow;
            review.IsEdited = true;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateDriverRatingInternalAsync(review.RevieweeId);

            return await MapToDto(review);
        }

        public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
        {
            var userGuid = userId;
            var reviewGuid = reviewId;

            var review = await _reviewRepository.GetByIdAsync(reviewGuid);

            if (review == null || review.ReviewerId != userGuid)
                return false;

            var driverId = review.RevieweeId;

            await _reviewRepository.RemoveAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateDriverRatingInternalAsync(driverId);

            return true;
        }

        public async Task<ReviewDto> GetReviewByIdAsync(Guid reviewId)
        {
            var reviewGuid = reviewId;

            var review = await _reviewRepository.GetAsync(
                filter: r => r.ReviewId == reviewGuid,
                includes: q => q.Include(r => r.Reviewer)
                               .Include(r => r.Reviewee)
                                   .ThenInclude(u => u.DriverProfile)
                               .Include(r => r.PickupRequest));

            return review != null ? await MapToDto(review) : null;
        }

        public async Task<ReviewDto> GetReviewByRequestIdAsync(Guid requestId)
        {
            var requestGuid = requestId;
            var review = await _reviewRepository.GetReviewByRequestIdAsync(requestGuid);

            return review != null ? await MapToDto(review) : null;
        }

        public async Task<List<ReviewDto>> GetReviewsForDriverAsync(Guid driverId, int page = 1, int pageSize = 20)
        {
            var driverGuid = driverId;
            var reviews = await _reviewRepository.GetReviewsByDriverIdAsync(driverGuid, page, pageSize);

            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(await MapToDto(review));
            }
            return reviewDtos;
        }

        public async Task<List<ReviewDto>> GetReviewsByUserAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var userGuid = userId;
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userGuid, page, pageSize);

            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(await MapToDto(review));
            }
            return reviewDtos;
        }

        public async Task<List<PendingReviewDto>> GetPendingReviewsForUserAsync(Guid userId)
        {
            var userGuid = userId;

            var completedRequests = await _unitOfWork.PickupRequests.GetAll(
                filter: pr => pr.UserId == userGuid && pr.Status == "Completed",
                includes: q => q.Include(pr => pr.DriverAssignments)
                               .ThenInclude(da => da.Driver)
                               .Include(pr => pr.Review));

            var pendingReviews = new List<PendingReviewDto>();

            foreach (var pr in completedRequests)
            {
                if (pr.Review != null) continue;

                var activeDriver = pr.DriverAssignments.FirstOrDefault(da => da.IsActive);
                if (activeDriver?.Driver == null) continue;

                pendingReviews.Add(new PendingReviewDto
                {
                    RequestId = pr.RequestId,
                    PickupAddress = GetAddressString(pr.Address),
                    CompletedAt = pr.CompletedAt ?? DateTime.UtcNow,
                    DaysSinceCompletion = (int)(DateTime.UtcNow - (pr.CompletedAt ?? DateTime.UtcNow)).TotalDays,
                    DriverId = activeDriver.DriverId,
                    DriverName = GetDriverName(activeDriver.Driver.User),
                    DriverRating = GetDriverRating(activeDriver.Driver.User)

                });
            }

            return pendingReviews;
        }

        public async Task<ValidationResult> CanUserReviewRequestAsync(Guid userId, Guid requestId)
        {
            var userGuid = userId;
            var requestGuid = requestId;

            var request = await _unitOfWork.PickupRequests.GetAsync(
                filter: r => r.RequestId == requestGuid,
                includes: q => q.Include(r => r.DriverAssignments));

            if (request == null)
                return ValidationResult.Fail("Request not found");

            if (request.UserId != userGuid)
                return ValidationResult.Fail("You can only review your own pickup requests");

            if (request.Status != "Completed")
                return ValidationResult.Fail("Request must be completed before reviewing");

            if (request.CompletedAt.HasValue)
            {
                var hoursSince = (DateTime.UtcNow - request.CompletedAt.Value).TotalHours;
                if (hoursSince < 1)
                    return ValidationResult.Fail("Please wait at least 1 hour after pickup");
            }

            if (request.CompletedAt.HasValue)
            {
                var daysSince = (DateTime.UtcNow - request.CompletedAt.Value).TotalDays;
                if (daysSince > 30)
                    return ValidationResult.Fail("Review period expired (30 days maximum)");
            }

            var exists = await _reviewRepository.AnyAsync(r => r.RequestId == requestGuid);
            if (exists)
                return ValidationResult.Fail("You have already reviewed this pickup");

            return ValidationResult.Success();
        }

        public async Task UpdateDriverRatingAsync(Guid driverId)
        {
            var driverGuid = driverId;
            await UpdateDriverRatingInternalAsync(driverGuid);
        }

        private async Task UpdateDriverRatingInternalAsync(Guid driverGuid)
        {
            var driver = await _unitOfWork.Users.GetAsync(
                filter: u => u.Id == driverGuid,
                includes: q => q.Include(u => u.DriverProfile));

            if (driver?.DriverProfile == null) return;

            var reviews = await _reviewRepository.GetAll(
                filter: r => r.RevieweeId == driverGuid);

            var reviewsList = reviews.ToList();

            if (reviewsList.Any())
            {
                var avgRating = (decimal)reviewsList.Average(r => r.Rating);
                var totalReviews = reviewsList.Count;

                SetPropertyIfExists(driver.DriverProfile, "AverageRating", avgRating);
                SetPropertyIfExists(driver.DriverProfile, "TotalReviews", totalReviews);
                SetPropertyIfExists(driver.DriverProfile, "Rating", avgRating);
                SetPropertyIfExists(driver.DriverProfile, "ReviewCount", totalReviews);
            }
            else
            {
                SetPropertyIfExists(driver.DriverProfile, "AverageRating", 0m);
                SetPropertyIfExists(driver.DriverProfile, "TotalReviews", 0);
                SetPropertyIfExists(driver.DriverProfile, "Rating", 0m);
                SetPropertyIfExists(driver.DriverProfile, "ReviewCount", 0);
            }

            await _unitOfWork.Users.UpdateAsync(driver);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DriverRatingDto> GetDriverRatingStatsAsync(Guid driverId)
        {
            var driverGuid = driverId;

            var driver = await _unitOfWork.Users.GetAsync(
                filter: u => u.Id == driverGuid,
                includes: q => q.Include(u => u.DriverProfile));

            if (driver == null) return null;

            var reviews = await _reviewRepository.GetAll(
                filter: r => r.RevieweeId == driverGuid);

            var reviewsList = reviews.ToList();

            var totalPickups = await _unitOfWork.DriverAssignments.CountAsync(
                da => da.DriverId == driverGuid && da.Status == AssignmentStatus.Completed);

            var totalAssignments = await _unitOfWork.DriverAssignments.CountAsync(
                da => da.DriverId == driverGuid);

            var completionRate = totalAssignments > 0
                ? (double)totalPickups / totalAssignments * 100
                : 0;

            return new DriverRatingDto
            {
                DriverId = driverId,
                DriverName = GetDriverName(driver),
                AverageRating = GetDriverRating(driver),
                TotalReviews = GetDriverTotalReviews(driver),
                FiveStars = reviewsList.Count(r => r.Rating == 5),
                FourStars = reviewsList.Count(r => r.Rating == 4),
                ThreeStars = reviewsList.Count(r => r.Rating == 3),
                TwoStars = reviewsList.Count(r => r.Rating == 2),
                OneStar = reviewsList.Count(r => r.Rating == 1),
                TotalPickups = totalPickups,
                CompletionRate = Math.Round(completionRate, 2)
            };
        }

        public async Task<bool> FlagReviewAsync(Guid reviewId, string reason)
        {
            var reviewGuid = reviewId;
            var review = await _reviewRepository.GetByIdAsync(reviewGuid);
            if (review == null) return false;

            review.IsFlagged = true;
            review.FlagReason = reason;
            review.FlaggedAt = DateTime.UtcNow;
            review.IsHidden = true;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HideReviewAsync(Guid reviewId, bool hide)
        {
            var reviewGuid = reviewId;
            var review = await _reviewRepository.GetByIdAsync(reviewGuid);
            if (review == null) return false;

            review.IsHidden = hide;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<ReviewDto>> GetFlaggedReviewsAsync()
        {
            var reviews = await _reviewRepository.GetFlaggedReviewsAsync();

            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(await MapToDto(review));
            }
            return reviewDtos;
        }

        // Helper methods
        private async Task<ReviewDto> MapToDto(Review review)
        {
            if (review.Reviewer == null)
                review.Reviewer = await _unitOfWork.Users.GetByIdAsync(review.ReviewerId);

            if (review.Reviewee == null)
            {
                review.Reviewee = await _unitOfWork.Users.GetAsync(
                    filter: u => u.Id == review.RevieweeId,
                    includes: q => q.Include(u => u.DriverProfile));
            }

            if (review.PickupRequest == null)
                review.PickupRequest = await _unitOfWork.PickupRequests.GetByIdAsync(review.RequestId);

            var reviewerName = review.Reviewer != null
                ? $"{review.Reviewer.FirstName} {review.Reviewer.LastName}"
                : "Unknown";

            var revieweeName = GetDriverName(review.Reviewee);

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                RequestId = review.RequestId,
                ReviewerId = review.ReviewerId,
                ReviewerName = reviewerName,
                RevieweeId = review.RevieweeId,
                RevieweeName = revieweeName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                IsEdited = review.IsEdited,
                PickupAddress = GetAddressString(review.PickupRequest?.Address)
            };
        }

        private Guid IntToGuid(int id)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(id).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        private int GuidToInt(Guid guid)
        {
            return BitConverter.ToInt32(guid.ToByteArray(), 0);
        }

        private string GetAddressString(Address address)
        {
            if (address == null) return "N/A";

            var street = GetPropertyValue<string>(address, "Street") ?? "";
            var city = GetPropertyValue<string>(address, "City") ?? "";

            if (!string.IsNullOrEmpty(street) && !string.IsNullOrEmpty(city))
                return $"{street}, {city}";

            return !string.IsNullOrEmpty(street) ? street : (!string.IsNullOrEmpty(city) ? city : "N/A");
        }

        private string GetDriverName(ApplicationUser user)
        {
            if (user == null) return "Unknown";

            if (user.DriverProfile != null)
            {
                var firstName = GetPropertyValue<string>(user.DriverProfile, "FirstName") ?? "";
                var lastName = GetPropertyValue<string>(user.DriverProfile, "LastName") ?? "";

                if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                    return $"{firstName} {lastName}".Trim();
            }

            return $"{user.FirstName} {user.LastName}".Trim();
        }

        private decimal GetDriverRating(ApplicationUser user)
        {
            if (user?.DriverProfile == null) return 0;

            var rating = GetPropertyValue<decimal?>(user.DriverProfile, "AverageRating");
            if (rating.HasValue) return rating.Value;

            rating = GetPropertyValue<decimal?>(user.DriverProfile, "Rating");
            if (rating.HasValue) return rating.Value;

            return 0m;
        }

        private int GetDriverTotalReviews(ApplicationUser user)
        {
            if (user?.DriverProfile == null) return 0;

            var total = GetPropertyValue<int?>(user.DriverProfile, "TotalReviews");
            if (total.HasValue) return total.Value;

            total = GetPropertyValue<int?>(user.DriverProfile, "ReviewCount");
            if (total.HasValue) return total.Value;

            return 0;
        }

        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null) return default;

            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                var value = property.GetValue(obj);
                if (value is T typedValue)
                    return typedValue;
            }
            return default;
        }

        private void SetPropertyIfExists(object obj, string propertyName, object value)
        {
            if (obj == null) return;

            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }
    }
}