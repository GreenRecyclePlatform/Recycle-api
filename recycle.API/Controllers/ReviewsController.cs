using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Reviews;
using recycle.Application.Interfaces.IService;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ Enabled authorization
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                Console.WriteLine("❌ No user ID found in token claims");
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            Console.WriteLine($"✅ User ID from token: {userIdClaim}");
            return Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// Create a new review
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                Console.WriteLine($"Creating review for user: {userId}");

                var reviewDto = await _reviewService.CreateReviewAsync(userId, dto);

                return Ok(new ApiResponse<ReviewDto>
                {
                    Success = true,
                    Data = reviewDto,
                    Message = "Review created successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating review: {ex.Message}");
                Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"{ex.Message} | Inner: {ex.InnerException?.Message}"
                });
            }
        }

        /// <summary>
        /// Update an existing review (within 7 days)
        /// </summary>
        [HttpPut("{reviewId}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var review = await _reviewService.UpdateReviewAsync(userId, reviewId, dto);

                return Ok(new ApiResponse<ReviewDto>
                {
                    Success = true,
                    Data = review,
                    Message = "Review updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating review: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the review"
                });
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{reviewId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _reviewService.DeleteReviewAsync(userId, reviewId);

                if (!result)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Review not found or you don't have permission to delete it"
                    });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Review deleted successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting review: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the review"
                });
            }
        }

        /// <summary>
        /// Get all reviews for a specific driver
        /// </summary>
        [HttpGet("driver/{driverId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetDriverReviews(
            Guid driverId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsForDriverAsync(driverId, page, pageSize);

                return Ok(new PaginatedResponse<ReviewDto>
                {
                    Success = true,
                    Data = reviews,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = reviews.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting driver reviews: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching reviews"
                });
            }
        }

        /// <summary>
        /// Get driver rating statistics
        /// </summary>
        [HttpGet("driver/{driverId}/stats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DriverRatingDto>), 200)]
        public async Task<IActionResult> GetDriverRatingStats(Guid driverId)
        {
            try
            {
                var stats = await _reviewService.GetDriverRatingStatsAsync(driverId);

                if (stats == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No reviews found for this driver"
                    });

                return Ok(new ApiResponse<DriverRatingDto>
                {
                    Success = true,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting driver stats: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching statistics"
                });
            }
        }

        /// <summary>
        /// Get current user's pending reviews (completed pickups without reviews)
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<PendingReviewDto[]>), 200)]
        public async Task<IActionResult> GetPendingReviews()
        {
            try
            {
                var userId = GetCurrentUserId();
                Console.WriteLine($"📋 Getting pending reviews for user: {userId}");

                var pendingReviews = await _reviewService.GetPendingReviewsForUserAsync(userId);

                Console.WriteLine($"✅ Found {pendingReviews.Count} pending reviews");

                return Ok(new ApiResponse<List<PendingReviewDto>>
                {
                    Success = true,
                    Data = pendingReviews,
                    Message = $"You have {pendingReviews.Count} pending review(s)"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetPendingReviews: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Check if user can review a specific request
        /// </summary>
        [HttpGet("request/{requestId}/can-review")]
        [ProducesResponseType(typeof(CanReviewResponse), 200)]
        public async Task<IActionResult> CanReviewRequest(Guid requestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var validation = await _reviewService.CanUserReviewRequestAsync(userId, requestId);

                return Ok(new CanReviewResponse
                {
                    CanReview = validation.IsValid,
                    Message = validation.ErrorMessage ?? "You can review this pickup"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking review eligibility: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Get current user's review history
        /// </summary>
        [HttpGet("my-reviews")]
        [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reviews = await _reviewService.GetReviewsByUserAsync(userId, page, pageSize);

                return Ok(new PaginatedResponse<ReviewDto>
                {
                    Success = true,
                    Data = reviews,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = reviews.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting user reviews: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching reviews"
                });
            }
        }

        /// <summary>
        /// Flag a review as inappropriate (Admin only)
        /// </summary>
        [HttpPost("{reviewId}/flag")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> FlagReview(Guid reviewId, [FromBody] FlagReviewRequest request)
        {
            try
            {
                var result = await _reviewService.FlagReviewAsync(reviewId, request.Reason);

                if (!result)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Review not found"
                    });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Review flagged and hidden successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error flagging review: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        [HttpGet("{reviewId}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetReviewById(Guid reviewId)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(reviewId);

                if (review == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Review not found"
                    });

                return Ok(new ApiResponse<ReviewDto>
                {
                    Success = true,
                    Data = review
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting review: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }
    }

    // Response Models
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public bool Success { get; set; }
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public class CanReviewResponse
    {
        public bool CanReview { get; set; }
        public string Message { get; set; }
    }

    public class FlagReviewRequest
    {
        public string Reason { get; set; }
    }
}