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
   // [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private Guid GetCurrentUserId()
        {
            // للتجربة فقط - احذفيني بعدين!
            return Guid.Parse("8B2A4D3A-B286-4031-D7DE-08DE2457CB91");

            // كومنت مؤقت
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (string.IsNullOrEmpty(userIdClaim))
            //     throw new UnauthorizedAccessException("User ID not found in token");
            // return Guid.Parse(userIdClaim);
        }

       

        [HttpPost]
        //[Authorize(Roles = "User")]  // ❌ اعملي comment
        [AllowAnonymous]  // ✅ ضيفي ده مؤقتاً
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            // للتجربة استخدمي userId ثابت
            var userId = Guid.Parse("8b2a4d3a-b286-4031-d7de-08de2457cb91");

            try
            {
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
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        //    /// <summary>
        //    /// Update an existing review (within 7 days)
        //    /// </summary>
        //    /// <param name="reviewId">Review ID</param>
        //    /// <param name="dto">Updated review details</param>
        //    /// <returns>Updated review</returns>
        [HttpPut("{reviewId}")]
       // [Authorize(Roles = "User")]
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
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        [HttpDelete("{reviewId}")]
        //[Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            var userId = Guid.Parse("8b2a4d3a-b286-4031-d7de-08de2457cb91");
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

        /// <summary>
        /// Get all reviews for a specific driver
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        [HttpGet("driver/{driverId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetDriverReviews(
            Guid driverId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
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

        /// <summary>
        /// Get driver rating statistics
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        [HttpGet("driver/{driverId}/stats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DriverRatingDto>), 200)]
        public async Task<IActionResult> GetDriverRatingStats(Guid driverId)
        {
            var stats = await _reviewService.GetDriverRatingStatsAsync(driverId);

            if (stats == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Driver not found"
                });

            return Ok(new ApiResponse<DriverRatingDto>
            {
                Success = true,
                Data = stats
            });
        }

        /// <summary>
        /// Get current user's pending reviews (completed pickups without reviews)
        /// </summary>
        [HttpGet("pending")]
        //[Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ApiResponse<PendingReviewDto[]>), 200)]
        public async Task<IActionResult> GetPendingReviews()
        {
            var userId = Guid.Parse("8b2a4d3a-b286-4031-d7de-08de2457cb91"); // ✅

         //   var userId = GetCurrentUserId();
            var pendingReviews = await _reviewService.GetPendingReviewsForUserAsync(userId);

            return Ok(new ApiResponse<PendingReviewDto[]>
            {
                Success = true,
                Data = pendingReviews.ToArray(),
                Message = $"You have {pendingReviews.Count} pending review(s)"
            });
        }

        /// <summary>
        /// Check if user can review a specific request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        [HttpGet("request/{requestId}/can-review")]
        //[Authorize(Roles = "User")]
        [ProducesResponseType(typeof(CanReviewResponse), 200)]
        public async Task<IActionResult> CanReviewRequest(Guid requestId)
        {
            var userId = Guid.Parse("8b2a4d3a-b286-4031-d7de-08de2457cb91"); // ✅

           // var userId = GetCurrentUserId();
            var validation = await _reviewService.CanUserReviewRequestAsync(userId, requestId);

            return Ok(new CanReviewResponse
            {
                CanReview = validation.IsValid,
                Message = validation.ErrorMessage ?? "You can review this pickup"
            });
        }

        /// <summary>
        /// Get current user's review history
        /// </summary>
        [HttpGet("my-reviews")]
        //[Authorize(Roles = "User")]
        [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = Guid.Parse("8b2a4d3a-b286-4031-d7de-08de2457cb91"); // ✅

            //var userId = GetCurrentUserId();
            var reviews = await _reviewService.GetReviewsByUserAsync(userId, page, pageSize);

            return Ok(new PaginatedResponse<ReviewDto>
            {
                Success = true,
                Data = reviews,
                Page = page,
                PageSize = pageSize,
                TotalCount = reviews.Count  // ✅ كان 0، صلحيه

            });
        }

        /// <summary>
        /// Flag a review as inappropriate (Admin only)
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        /// <param name="request">Flag reason</param>
        [HttpPost("{reviewId}/flag")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> FlagReview(Guid reviewId, [FromBody] FlagReviewRequest request)
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

        /// <summary>
        /// Get review by ID
        /// </summary>
        [HttpGet("{reviewId}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
        public async Task<IActionResult> GetReviewById(Guid reviewId)
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
