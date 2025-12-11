using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Repositories
{
    public class SupplierOrderRepository : ISupplierOrderRepository
    {
        private readonly AppDbContext _context;

        public SupplierOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// إنشاء Order جديد مع الـ Items
        /// </summary>
        public async Task<SupplierOrder> CreateOrderAsync(SupplierOrder order)
        {
            await _context.SupplierOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Reload مع الـ Navigation Properties
            return await GetOrderByIdAsync(order.OrderId)
                   ?? throw new Exception("Failed to retrieve created order");
        }

        /// <summary>
        /// جلب Order بالـ ID مع كل التفاصيل
        /// </summary>
        public async Task<SupplierOrder?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.SupplierOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        /// <summary>
        /// جلب كل Orders لـ Supplier معين
        /// </summary>
        public async Task<IEnumerable<SupplierOrder>> GetOrdersBySupplierIdAsync(Guid supplierId)
        {
            return await _context.SupplierOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Material)
                .Where(o => o.SupplierId == supplierId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// تحديث حالة الدفع بعد نجاح Stripe
        /// </summary>
        public async Task<bool> UpdatePaymentStatusAsync(
            Guid orderId,
            string status,
            string paymentIntentId)
        {
            var order = await _context.SupplierOrders.FindAsync(orderId);

            if (order == null)
                return false;

            order.PaymentStatus = status;
            order.StripePaymentIntentId = paymentIntentId;

            if (status == "Completed")
            {
                order.PaidAt = DateTime.UtcNow;
            }

            order.UpdatedAt = DateTime.UtcNow;

            _context.SupplierOrders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// حفظ التغييرات
        /// </summary>
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// ✅ الـ Method الجديدة - بتحسب Available Quantities لكل Material
        /// </summary>
        public async Task<Dictionary<Guid, decimal>> GetAvailableQuantitiesAsync()
        {
            var result = await _context.RequestMaterials
                .Include(rm => rm.PickupRequest)
                .Where(rm =>
                    rm.PickupRequest.Status == "Completed" &&
                    rm.ActualWeight.HasValue
                )
                .GroupBy(rm => rm.MaterialId)
                .Select(g => new
                {
                    MaterialId = g.Key,
                    TotalQuantity = g.Sum(rm => rm.ActualWeight!.Value)
                })
                .ToDictionaryAsync(x => x.MaterialId, x => x.TotalQuantity);

            return result;
        }
    }
}