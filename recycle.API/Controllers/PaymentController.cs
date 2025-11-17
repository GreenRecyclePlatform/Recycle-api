using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Enums;
using Stripe;

namespace Recycle.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IStripeAdapter _stripeAdapter;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IStripeAdapter stripeAdapter, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _stripeAdapter = stripeAdapter;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(PaymentDto dto)
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] string? status)
        {
            var result = await _paymentService.GetPaymentsAsync(status);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status, [FromQuery] Guid adminId,
                                                      [FromQuery] string? notes, [FromQuery] string? failureReason)
        {
            var updated = await _paymentService.UpdatePaymentStatusAsync(id, status, adminId, notes, failureReason);
            if (!updated) return NotFound("Payment not found");

            return Ok("Status updated successfully");
        }

        [HttpPost("onboard-driver")]
        public async Task<IActionResult> OnboardDriver([FromQuery] Guid userId, [FromQuery] string email, [FromQuery] string refreshUrl, [FromQuery] string returnUrl)
        {
            // 1. Create Express account
            var stripeAccountId = await _stripeAdapter.CreateExpressAccountAsync(userId, email);

            // You must save stripeAccountId to your Users table (via UnitOfWork user repo). Example:
            // var user = await _unitOfWork.Users.GetByIdAsync(userId);
            // user.StripeAccountId = stripeAccountId; _unitOfWork.Users.Update(user); await _unitOfWork.SaveAsync();
            // For now just return account id and account link

            var accountLink = await _stripeAdapter.CreateAccountLinkAsync(stripeAccountId, refreshUrl, returnUrl);

            return Ok(new { stripeAccountId, accountLink });
        }

        [HttpPost("request-payout")]
        public async Task<IActionResult> RequestPayout([FromBody] PayoutRequestDto dto)
        {
            // dto: RecipientUserId, Amount, RequestId, Currency (optional)
            var paymentDto = await _paymentService.RequestPayoutAsync(dto.RecipientUserId, dto.Amount, dto.RequestId, dto.Currency ?? "usd");
            if (paymentDto == null) return BadRequest("Unable to create payout");

            return Ok(paymentDto);
        }

        // Webhook endpoint. Stripe sends events here.
        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];

            Event stripeEvent;

            try
            {
                stripeEvent = _stripeAdapter.VerifyAndConstructEvent(json, signature);
            }
            catch
            {
                return BadRequest("Invalid Stripe signature");
            }

            switch (stripeEvent.Type)
            {
                case "transfer.paid":
                    var transfer = stripeEvent.Data.Object as Transfer;
                    if (transfer?.Metadata != null && transfer.Metadata.ContainsKey("ID"))
                    {
                        Guid pid = Guid.Parse(transfer.Metadata["ID"]);
                        await _paymentService.UpdatePaymentStatusAsync(pid, PaymentStatuses.Paid, pid, "Stripe transfer.paid webhook");
                    }
                    break;

                case "transfer.failed":
                    var t2 = stripeEvent.Data.Object as Transfer;
                    if (t2?.Metadata != null && t2.Metadata.ContainsKey("ID"))
                    {
                        Guid pid = Guid.Parse(t2.Metadata["ID"]);
                        await _paymentService.UpdatePaymentStatusAsync(pid, PaymentStatuses.Failed, pid, "Stripe transfer.failed webhook", "Stripe failure");
                    }
                    break;
            }

            return Ok();
        }
    }
}
