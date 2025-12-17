using recycle.Application.DTOs.supplier;

namespace recycle.Application.Interfaces.IService
{
    public interface ISupplierOrderService
    {
        
        Task<List<AvailableMaterialDto>> GetAvailableMaterialsAsync();

        
        Task<SupplierOrderResponseDto> CreateOrderAsync(Guid supplierId, CreateSupplierOrderDto dto);

        Task<PaymentIntentDto> CreatePaymentIntentAsync(Guid orderId);

        
        Task<bool> ConfirmPaymentAsync(Guid orderId, string paymentIntentId);

        Task<List<SupplierOrderResponseDto>> GetMyOrdersAsync(Guid supplierId);

        
        Task<SupplierOrderResponseDto?> GetOrderByIdAsync(Guid orderId);
    }
}