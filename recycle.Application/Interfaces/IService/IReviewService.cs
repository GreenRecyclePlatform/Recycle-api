using System.Collections.Generic;
using System.Threading.Tasks;
using recycle.Application.DTOs.Reviews;

namespace recycle.Application.Interfaces.IService
{
    public interface IReviewService
    {
        // Create
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto dto);

        // Read
        Task<ReviewDto> GetReviewByIdAsync(int reviewId);
        Task<ReviewDto> GetReviewByRequestIdAsync(int requestId);
        Task<List<ReviewDto>> GetReviewsForDriverAsync(int driverId, int page = 1, int pageSize = 20);
        Task<List<ReviewDto>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20);
        Task<List<PendingReviewDto>> GetPendingReviewsForUserAsync(int userId);

        // Update
        Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto);

        // Delete
        Task<bool> DeleteReviewAsync(int userId, int reviewId);

        // Validation
        Task<ValidationResult> CanUserReviewRequestAsync(int userId, int requestId);

        // Driver Rating
        Task UpdateDriverRatingAsync(int driverId);
        Task<DriverRatingDto> GetDriverRatingStatsAsync(int driverId);

        // Admin
        Task<bool> FlagReviewAsync(int reviewId, string reason);
        Task<bool> HideReviewAsync(int reviewId, bool hide);
        Task<List<ReviewDto>> GetFlaggedReviewsAsync();
    }
}