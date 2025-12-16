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

       //calculate  avalible quantity after buying
        public async Task<Dictionary<Guid, decimal>> GetAvailableQuantitiesAsync()
        {
      
            var incomingQuantities = await _context.RequestMaterials
                .Include(rm => rm.PickupRequest)
                .Where(rm =>
                    rm.PickupRequest != null &&
                    rm.PickupRequest.Status == "Completed" &&
                    rm.EstimatedWeight!=0 &&
                    rm.EstimatedWeight> 0
                )
                .GroupBy(rm => rm.MaterialId)
                .Select(g => new
                {
                    MaterialId = g.Key,
                    TotalIncoming = g.Sum(rm => rm.EstimatedWeight!)
                })
                .ToDictionaryAsync(x => x.MaterialId, x => x.TotalIncoming);

           
            var soldQuantities = await _context.SupplierOrderItems
                .Include(item => item.Order)
                .Where(item => item.Order.PaymentStatus == "Completed")
                .GroupBy(item => item.MaterialId)
                .Select(g => new
                {
                    MaterialId = g.Key,
                    TotalSold = g.Sum(item => item.Quantity)
                })
                .ToDictionaryAsync(x => x.MaterialId, x => x.TotalSold);

            // 3️⃣ حساب Available = Incoming - Sold
            var result = new Dictionary<Guid, decimal>();

            foreach (var incoming in incomingQuantities)
            {
                var sold = soldQuantities.ContainsKey(incoming.Key)
                    ? soldQuantities[incoming.Key]
                    : 0;

                var available = incoming.Value - sold;

                // ✅ نضيف فقط المواد اللي عندها كمية متاحة
                if (available > 0)
                {
                    result[incoming.Key] = available;
                }
            }

            return result;
        }





       //info of payment to admin
        public async Task<(decimal totalRevenue, int completedOrders, decimal pendingPayments)> GetPaymentStatisticsAsync()
        {
            // all Revenue Completed
            var totalRevenue = await _context.SupplierOrders
                .Where(o => o.PaymentStatus == "Completed")
                .SumAsync(o => o.TotalAmount);

            // num of Completed
            var completedOrders = await _context.SupplierOrders
                .CountAsync(o => o.PaymentStatus == "Completed");

            // num of pending 
            var pendingPayments = await _context.SupplierOrders
                .Where(o => o.PaymentStatus == "Pending")
                .SumAsync(o => o.TotalAmount);

            return (totalRevenue, completedOrders, pendingPayments);
        }

        // Admin Dashboard
        public async Task<IEnumerable<SupplierOrder>> GetRecentPaymentsAsync(int count = 20)
        {
            return await _context.SupplierOrders
                .Include(o => o.Supplier)
                .Where(o => o.PaymentStatus == "Completed")
                .OrderByDescending(o => o.PaidAt)
                .Take(count)
                .ToListAsync();
        }

      //order with deteails 
        public async Task<IEnumerable<SupplierOrder>> GetAllOrdersAsync()
        {
            return await _context.SupplierOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Material)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}