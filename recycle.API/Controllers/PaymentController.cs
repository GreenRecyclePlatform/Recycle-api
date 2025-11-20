using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Enums;
using recycle.Infrastructure.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IPaymentService paymentService, IStripeAdapter stripeAdapter, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _paymentService = paymentService;
            _stripeAdapter = stripeAdapter;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] string? status)
        {
            var list = await _paymentService.GetPaymentsAsync(status);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var p = await _paymentService.GetPaymentByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status, [FromQuery] Guid adminId, [FromQuery] string? notes, [FromQuery] string? failureReason)
        {
            var ok = await _paymentService.UpdatePaymentStatusAsync(id, status, adminId, notes, failureReason);
            if (!ok) return NotFound();
            return Ok("Status updated");
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

            // 2. SAVE StripeAccountId to the user in DB (REQUIRED)
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.StripeAccountId = stripeAccountId;
            _unitOfWork.Users?.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // 3. Generate Stripe onboarding link
            var accountLink = await _stripeAdapter.CreateAccountLinkAsync(stripeAccountId, refreshUrl, returnUrl);

            // 4. Return info to frontend
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
                stripeEvent = _stripeAdapter.VerifyAndConstructEvent(json, signature!);
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

        // DEV only: create test charge in platform to add funds (remove in production)
        [HttpPost("stripe/test-charge")]
        public async Task<IActionResult> CreateTestCharge([FromQuery] long amountInCents, [FromQuery] string currency = "usd")
        {
            try
            {
                var charge = await _stripeAdapter.CreateTestChargeAsync(amountInCents, currency);
                return Ok(new { success = true, chargeId = charge.Id, status = charge.Status });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }



        //Will be added later with the front
        [HttpGet("stripe/login")]
        public async Task<IActionResult> GetStripeDashboardLoginLink([FromQuery] Guid userId)
        {
            return Ok();
        }

        [HttpGet("account/status")]
        public async Task<IActionResult> GetStripeAccountStatus([FromQuery] Guid userId)
        {
            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPayments(Guid userId)
        {
            return Ok();
        }
    }
}
