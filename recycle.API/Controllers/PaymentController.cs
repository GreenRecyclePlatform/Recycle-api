using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using System.Security.Claims;

namespace Recycle.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IPaymentService paymentService, IUnitOfWork unitOfWork)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create a new payment record (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            if (result == null)
                return BadRequest("Unable to create payment. Check request and user IDs.");

            return Ok(result);
        }

        /// <summary>
        /// Get all payments with optional status filter (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPayments([FromQuery] string? status)
        {
            var list = await _paymentService.GetPaymentsAsync(status);
            return Ok(list);
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Admin approves payment and sends money via PayPal
        /// </summary>
        [HttpPut("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePayment(
            Guid id,
            [FromQuery] Guid adminId,
            [FromQuery] string? notes)
        {
            var success = await _paymentService.UpdatePaymentStatusAsync(
                id,
                recycle.Domain.Enums.PaymentStatuses.Approved,
                adminId,
                notes);

            if (!success)
                return NotFound("Payment not found");

            return Ok(new { message = "Payment approved and processed via PayPal" });
        }

        /// <summary>
        /// Admin rejects payment
        /// </summary>
        [HttpPut("{id:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectPayment(
            Guid id,
            [FromQuery] Guid adminId,
            [FromQuery] string reason)
        {
            var success = await _paymentService.UpdatePaymentStatusAsync(
                id,
                recycle.Domain.Enums.PaymentStatuses.Failed,
                adminId,
                failureReason: reason);

            if (!success)
                return NotFound("Payment not found");

            return Ok(new { message = "Payment rejected" });
        }

        /// <summary>
        /// Request payout after pickup completion (automatically creates payment)
        /// </summary>
        [HttpPost("request-payout")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RequestPayout([FromBody] PayoutRequestDto dto)
        {
            var payment = await _paymentService.RequestPayoutAsync(
                dto.RecipientUserId,
                dto.Amount,
                dto.RequestId,
                dto.Currency ?? "EUR");

            if (payment == null)
                return BadRequest("Unable to create payout request");

            return Ok(new
            {
                message = "Payout request created. Admin needs to approve it.",
                payment
            });
        }

        /// <summary>
        /// Get user's payment history
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserPayments(Guid userId)
        {
            var payments = await _paymentService.GetPaymentsAsync(null);
            //var userPayments = payments.Where(p => p.RecipientUserID == userId).ToList();
            // ✅ FIXED: Changed RecipientUserID to userId (new DTO property name)
            var userPayments = payments.Where(p => p.userId == userId).ToList();
            return Ok(userPayments);
        }

        /// <summary>
        /// Check PayPal payout status by payment ID
        /// </summary>
        [HttpGet("{id:guid}/paypal-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckPayPalStatus(Guid id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound("Payment not found");

            //if (string.IsNullOrEmpty(payment.TransactionReference))
            if (string.IsNullOrEmpty(payment.transactionId))
                return BadRequest("No PayPal transaction reference found");

            try
            {
                var paypalService = HttpContext.RequestServices.GetRequiredService<IPayPalPayoutService>();
                var status = await paypalService.GetPayoutStatusAsync(payment.transactionId);

                return Ok(new
                {
                    paymentId = id,
                    paypalBatchId = payment.transactionId,
                    paypalStatus = status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //=====================================================
        //  NEW ENDPOINT 1: Get current user's payments
        [HttpGet("my-payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetMyPayments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var payments = await _paymentService.GetUserPaymentsAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payments", error = ex.Message });
            }
        }

        //  NEW ENDPOINT 2: Get payment summary
        [HttpGet("summary")]
        public async Task<ActionResult<PaymentSummaryDto>> GetSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                var summary = await _paymentService.GetSummaryAsync(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving summary", error = ex.Message });
            }
        }

        //  NEW ENDPOINT 3: Filter payments with pagination
        [HttpPost("filter")]
        public async Task<ActionResult<PagedResultDto<PaymentDto>>> FilterPayments(
            [FromBody] PaymentFilterDto filter)
        {
            try
            {
                // If not admin, force filter to current user
                var userRole = GetCurrentUserRole();
                if (userRole != "Admin")
                {
                    filter.UserId = GetCurrentUserId();
                }

                var result = await _paymentService.FilterPaymentsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error filtering payments", error = ex.Message });
            }
        }

        //  NEW ENDPOINT 4: Complete payment (Admin only)
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CompletePayment(Guid id, [FromBody] CompletePaymentDto dto)
        {
            try
            {
                var result = await _paymentService.CompletePaymentAsync(id, dto.TransactionId);

                if (!result)
                {
                    return NotFound(new { message = "Payment not found" });
                }

                return Ok(new { message = "Payment marked as completed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error completing payment", error = ex.Message });
            }
        }

        //  Helper methods (add if not already present)
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(userIdClaim);
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }


    }
    
}