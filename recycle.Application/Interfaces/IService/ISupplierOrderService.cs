using recycle.Application.DTOs.supplier;

namespace recycle.Application.Interfaces.IService
{
    public interface ISupplierOrderService
    {
        /// <summary>
        /// جلب كل المواد المتاحة للشراء
        /// </summary>
        Task<List<AvailableMaterialDto>> GetAvailableMaterialsAsync();

        /// <summary>
        /// إنشاء Order جديد (قبل الدفع)
        /// </summary>
        Task<SupplierOrderResponseDto> CreateOrderAsync(Guid supplierId, CreateSupplierOrderDto dto);

        /// <summary>
        /// إنشاء PaymentIntent في Stripe
        /// </summary>
        Task<PaymentIntentDto> CreatePaymentIntentAsync(Guid orderId);

        /// <summary>
        /// تأكيد الدفع بعد نجاح Stripe
        /// </summary>
        Task<bool> ConfirmPaymentAsync(Guid orderId, string paymentIntentId);

        /// <summary>
        /// جلب Order History للـ Supplier
        /// </summary>
        Task<List<SupplierOrderResponseDto>> GetMyOrdersAsync(Guid supplierId);

        /// <summary>
        /// جلب Order واحد بالتفاصيل
        /// </summary>
        Task<SupplierOrderResponseDto?> GetOrderByIdAsync(Guid orderId);
    }
}