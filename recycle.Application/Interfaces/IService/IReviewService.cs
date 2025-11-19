using System.Collections.Generic;
using System.Threading.Tasks;
using recycle.Application.DTOs.Reviews;

using recycle.Domain.Entities;

namespace recycle.Application.Interfaces.IService
{
    /// <summary>
    /// Service interface for Review business logic operations
    /// </summary>
    public interface IReviewService
    {
        Task<Review> CreateReview(Guid userId, CreateReviewDto dto);

        //// ==================== CREATE ====================

        ///// <summary>
        ///// Create a new review for a completed pickup request
        ///// </summary>
        Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto);

        //// ==================== READ ====================

        ///// <summary>
        ///// Get review by ID
        ///// </summary>
        Task<ReviewDto> GetReviewByIdAsync(Guid reviewId);

        ///// <summary>
        ///// Get review by request ID
        ///// </summary>
        Task<ReviewDto> GetReviewByRequestIdAsync(Guid requestId);

        ///// <summary>
        ///// Get all reviews for a specific driver (paginated)
        ///// </summary>
        Task<List<ReviewDto>> GetReviewsForDriverAsync(Guid driverId, int page = 1, int pageSize = 20);

        ///// <summary>
        ///// Get all reviews by a specific user (paginated)
        ///// </summary>
        Task<List<ReviewDto>> GetReviewsByUserAsync(Guid userId, int page = 1, int pageSize = 20);

        ///// <summary>
        ///// Get pending reviews for a user (completed pickups without reviews)
        ///// </summary>
        Task<List<PendingReviewDto>> GetPendingReviewsForUserAsync(Guid userId);

        //// ==================== UPDATE ====================

        ///// <summary>
        ///// Update an existing review (within 7 days)
        ///// </summary>
        Task<ReviewDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto);

        //// ==================== DELETE ====================

        ///// <summary>
        ///// Delete a review
        ///// </summary>
        Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);

        //// ==================== VALIDATION ====================

        ///// <summary>
        ///// Check if user can review a specific request
        ///// </summary>
       Task<ValidationResult> CanUserReviewRequestAsync(Guid userId, Guid requestId);

        //// ==================== DRIVER RATING ====================

        ///// <summary>
        ///// Update driver's average rating based on reviews
        ///// </summary>
        Task UpdateDriverRatingAsync(Guid driverId);

        ///// <summary>
        ///// Get driver rating statistics
        ///// </summary>
        Task<DriverRatingDto> GetDriverRatingStatsAsync(Guid driverId);

        //// ==================== ADMIN OPERATIONS ====================

        ///// <summary>
        ///// Flag a review as inappropriate (Admin only)
        ///// </summary>
        Task<bool> FlagReviewAsync(Guid reviewId, string reason);

        ///// <summary>
        ///// Hide or unhide a review (Admin only)
        ///// </summary>
        Task<bool> HideReviewAsync(Guid reviewId, bool hide);

        ///// <summary>
        ///// Get all flagged reviews (Admin only)
        ///// </summary>
        Task<List<ReviewDto>> GetFlaggedReviewsAsync();
    }
}