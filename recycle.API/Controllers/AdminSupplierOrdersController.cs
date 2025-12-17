namespace recycle.API.Controllers
{
    using global::recycle.Application.DTOs.supplier;
    using global::recycle.Application.Interfaces.IRepository;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;


    namespace recycle.API.Controllers
    {
        [Route("api/admin/supplier-orders")]
        [ApiController]
        [Authorize(Roles = "Admin")]
        public class AdminSupplierOrdersController : ControllerBase
        {
            private readonly ISupplierOrderRepository _orderRepo;

            public AdminSupplierOrdersController(ISupplierOrderRepository orderRepo)
            {
                _orderRepo = orderRepo;
            }

            [HttpGet]
            [ProducesResponseType(StatusCodes.Status200OK)]
            public async Task<ActionResult> GetAllOrders()
            {
                var orders = await _orderRepo.GetAllOrdersAsync();

                var result = orders.Select(o => new
                {
                    o.OrderId,
                    SupplierName = !string.IsNullOrEmpty(o.Supplier?.CompanyName)
                        ? o.Supplier.CompanyName
                        : $"{o.Supplier?.FirstName} {o.Supplier?.LastName}",
                    o.TotalAmount,
                    o.PaymentStatus,
                    o.OrderDate,
                    o.PaidAt,
                    ItemsCount = o.OrderItems?.Count ?? 0,
                    Items = o.OrderItems?.Select(i => new
                    {
                        MaterialName = i.Material?.Name ?? "Unknown",
                        i.Quantity,
                        i.PricePerKg,
                        Subtotal = i.TotalPrice
                    }).ToList()
                });

                return Ok(result);
            }

            [HttpGet("{orderId}")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<ActionResult> GetOrderById(Guid orderId)
            {
                var order = await _orderRepo.GetOrderByIdAsync(orderId);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                var result = new
                {
                    order.OrderId,
                    SupplierName = !string.IsNullOrEmpty(order.Supplier?.CompanyName)
                        ? order.Supplier.CompanyName
                        : $"{order.Supplier?.FirstName} {order.Supplier?.LastName}",
                    SupplierEmail = order.Supplier?.Email,
                    SupplierPhone = order.Supplier?.PhoneNumber,
                    order.TotalAmount,
                    order.PaymentStatus,
                    order.OrderDate,
                    order.PaidAt,
                    order.StripePaymentIntentId,
                    Items = order.OrderItems?.Select(i => new
                    {
                        MaterialName = i.Material?.Name ?? "Unknown",
                        i.Quantity,
                        i.PricePerKg,
                        Subtotal = i.TotalPrice
                    }).ToList()
                };

                return Ok(result);
            }

            [HttpGet("payment-stats")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            public async Task<ActionResult<SupplierPaymentStatsDto>> GetPaymentStatistics()
            {
                var (totalRevenue, completedOrders, pendingPayments) =
                    await _orderRepo.GetPaymentStatisticsAsync();

                var recentPayments = await _orderRepo.GetRecentPaymentsAsync(10);

                var stats = new SupplierPaymentStatsDto
                {
                    TotalRevenue = totalRevenue,
                    CompletedOrdersCount = completedOrders,
                    PendingPayments = pendingPayments,
                    RecentPayments = recentPayments.Select(o => new SupplierPaymentDetailDto
                    {
                        OrderId = o.OrderId,
                        SupplierName = !string.IsNullOrEmpty(o.Supplier?.CompanyName)
                            ? o.Supplier.CompanyName
                            : $"{o.Supplier?.FirstName} {o.Supplier?.LastName}",
                        Amount = o.TotalAmount,
                        PaymentStatus = o.PaymentStatus,
                        PaidAt = o.PaidAt,
                        OrderDate = o.OrderDate
                    }).ToList()
                };

                return Ok(stats);
            }
        }
    }
}
