using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.supplier;
using recycle.Application.Interfaces.IService;
using System.Security.Claims;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Supplier")]
    public class SupplierOrdersController : ControllerBase
    {
        private readonly ISupplierOrderService _orderService;

        public SupplierOrdersController(ISupplierOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated");
            return Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// GET: api/supplierorders/available-materials
        /// جلب المواد المتاحة للشراء
        /// </summary>
        [HttpGet("available-materials")]
        [AllowAnonymous] // عشان الـ Supplier يقدر يشوفها قبل ما يسجل دخول
        public async Task<IActionResult> GetAvailableMaterials()
        {
            try
            {
                var materials = await _orderService.GetAvailableMaterialsAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/supplierorders
        /// إنشاء طلب جديد
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateSupplierOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var supplierId = GetCurrentUserId();
                var order = await _orderService.CreateOrderAsync(supplierId, dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/supplierorders/{orderId}/create-payment-intent
        /// إنشاء PaymentIntent للدفع
        /// </summary>
        [HttpPost("{orderId}/create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent(Guid orderId)
        {
            try
            {
                var paymentIntent = await _orderService.CreatePaymentIntentAsync(orderId);
                return Ok(paymentIntent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/supplierorders/confirm-payment
        /// تأكيد الدفع بعد نجاح Stripe
        /// </summary>
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _orderService.ConfirmPaymentAsync(dto.OrderId, dto.PaymentIntentId);

                if (result)
                    return Ok(new { message = "Payment confirmed successfully" });

                return BadRequest(new { message = "Payment confirmation failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/supplierorders/my-orders
        /// جلب طلبات الـ Supplier الحالي
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var supplierId = GetCurrentUserId();
                var orders = await _orderService.GetMyOrdersAsync(supplierId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/supplierorders/{orderId}
        /// جلب تفاصيل طلب معين
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                // ✅ تأكد إن الـ Supplier هو صاحب الطلب
                var supplierId = GetCurrentUserId();
                if (order.SupplierId != supplierId)
                    return Forbid();

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}