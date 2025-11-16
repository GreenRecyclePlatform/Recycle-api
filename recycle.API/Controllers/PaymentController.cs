using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;

namespace Recycle.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status, [FromQuery] int adminId,
                                                      [FromQuery] string? notes, [FromQuery] string? failureReason)
        {
            var updated = await _paymentService.UpdatePaymentStatusAsync(id, status, adminId, notes, failureReason);
            if (!updated) return NotFound("Payment not found");

            return Ok("Status updated successfully");
        }
    }
}
