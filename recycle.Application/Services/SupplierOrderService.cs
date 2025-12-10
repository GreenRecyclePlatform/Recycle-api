using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs.supplier;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Domain.Entities;
using recycle.Domain.Entities.recycle.Domain.Entities;

namespace recycle.Application.Services
{
    public class SupplierOrderService : ISupplierOrderService
    {
        private readonly ISupplierOrderRepository _orderRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IUserRepository _userRepository;
        private readonly StripeService _stripeService;

        public SupplierOrderService(
            ISupplierOrderRepository orderRepository,
            IMaterialRepository materialRepository,
            IUserRepository userRepository,
            StripeService stripeService)
        {
            _orderRepository = orderRepository;
            _materialRepository = materialRepository;
            _userRepository = userRepository;
            _stripeService = stripeService;
        }

        /// <summary>
        /// ✅ 1️⃣ جلب المواد المتاحة للشراء - بدون DbContext
        /// </summary>
        public async Task<List<AvailableMaterialDto>> GetAvailableMaterialsAsync()
        {
            // 1. جيب كل المواد النشطة
            var materials = await _materialRepository.GetAllAsync(includeInactive: false);

            // 2. جيب الكميات المتاحة من Repository
            var availableQuantities = await _orderRepository.GetAvailableQuantitiesAsync();

            // 3. اعمل Mapping
            var result = materials.Select(m => new AvailableMaterialDto
            {
                MaterialId = m.Id,
                Name = m.Name,
                Description = m.Description,
                Icon = m.Icon,
                ImageUrl = m.ImageUrl,
                SellingPrice = m.SellingPrice,
                Unit = m.Unit ?? "kg",
                IsActive = m.IsActive,
                // ✅ لو المادة موجودة في Dictionary، خد الكمية، لو لأ = 0
                AvailableQuantity = availableQuantities.ContainsKey(m.Id)
                    ? availableQuantities[m.Id]
                    : 0
            }).ToList();

            return result;
        }

        /// <summary>
        /// 2️⃣ إنشاء Order جديد (بدون دفع)
        /// </summary>
        public async Task<SupplierOrderResponseDto> CreateOrderAsync(
            Guid supplierId,
            CreateSupplierOrderDto dto)
        {
            // 1. Validate Supplier exists
            var supplier = await _userRepository.GetByIdAsync(supplierId);
            if (supplier == null)
                throw new Exception("Supplier not found");

            // 2. Validate Materials and Calculate Total
            var orderItems = new List<SupplierOrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var material = await _materialRepository.GetByIdAsync(item.MaterialId);

                if (material == null)
                    throw new Exception($"Material with ID {item.MaterialId} not found");

                if (!material.IsActive)
                    throw new Exception($"Material '{material.Name}' is not active");

                if (item.Quantity <= 0)
                    throw new Exception("Quantity must be greater than zero");

                var itemTotal = item.Quantity * material.SellingPrice;
                totalAmount += itemTotal;

                orderItems.Add(new SupplierOrderItem
                {
                    Id = Guid.NewGuid(),
                    MaterialId = material.Id,
                    Quantity = item.Quantity,
                    PricePerKg = material.SellingPrice,
                    TotalPrice = itemTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 3. Create Order Entity
            var order = new SupplierOrder
            {
                OrderId = Guid.NewGuid(),
                SupplierId = supplierId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            // 4. Save to Database
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // 5. Return Response
            return MapToResponseDto(createdOrder);
        }

        /// <summary>
        /// 3️⃣ إنشاء PaymentIntent في Stripe
        /// </summary>
        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.PaymentStatus != "Pending")
                throw new Exception($"Cannot create payment for order with status '{order.PaymentStatus}'");

            var paymentIntent = await _stripeService.CreatePaymentIntentAsync(
                amount: order.TotalAmount,
                currency: "usd"
            );

            order.StripePaymentIntentId = paymentIntent.Id;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdatePaymentStatusAsync(
                order.OrderId,
                "Pending",
                paymentIntent.Id
            );

            return new PaymentIntentDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Amount = order.TotalAmount,
                OrderId = order.OrderId
            };
        }

        /// <summary>
        /// 4️⃣ تأكيد الدفع بعد نجاح Stripe
        /// </summary>
        public async Task<bool> ConfirmPaymentAsync(Guid orderId, string paymentIntentId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.StripePaymentIntentId != paymentIntentId)
                throw new Exception("Payment Intent ID mismatch");

            var paymentIntent = await _stripeService.GetPaymentIntentAsync(paymentIntentId);

            if (paymentIntent.Status != "succeeded")
                throw new Exception($"Payment not successful. Status: {paymentIntent.Status}");

            return await _orderRepository.UpdatePaymentStatusAsync(
                orderId,
                "Completed",
                paymentIntentId
            );
        }

        /// <summary>
        /// 5️⃣ جلب Order History للـ Supplier
        /// </summary>
        public async Task<List<SupplierOrderResponseDto>> GetMyOrdersAsync(Guid supplierId)
        {
            var orders = await _orderRepository.GetOrdersBySupplierIdAsync(supplierId);
            return orders.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// 6️⃣ جلب Order واحد
        /// </summary>
        public async Task<SupplierOrderResponseDto?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order == null ? null : MapToResponseDto(order);
        }

        /// <summary>
        /// Helper: Mapping من Entity لـ DTO
        /// </summary>
        private SupplierOrderResponseDto MapToResponseDto(SupplierOrder order)
        {
            return new SupplierOrderResponseDto
            {
                OrderId = order.OrderId,
                SupplierId = order.SupplierId,
                SupplierCompanyName = order.Supplier?.CompanyName ?? "Unknown",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus,
                StripePaymentIntentId = order.StripePaymentIntentId,
                PaidAt = order.PaidAt,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems?.Select(item => new OrderItemResponseDto
                {
                    MaterialId = item.MaterialId,
                    MaterialName = item.Material?.Name ?? "Unknown",
                    MaterialIcon = item.Material?.Icon ?? "♻️",
                    Quantity = item.Quantity,
                    PricePerKg = item.PricePerKg,
                    TotalPrice = item.TotalPrice
                }).ToList() ?? new List<OrderItemResponseDto>()
            };
        }
    }
}