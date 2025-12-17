using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IRepository
{

  
        public interface ISupplierOrderRepository
        {
            
            Task<SupplierOrder> CreateOrderAsync(SupplierOrder order);

            Task<SupplierOrder?> GetOrderByIdAsync(Guid orderId);

           
            Task<IEnumerable<SupplierOrder>> GetOrdersBySupplierIdAsync(Guid supplierId);

            
            Task<bool> UpdatePaymentStatusAsync(Guid orderId, string status, string paymentIntentId);

           
            Task<bool> SaveChangesAsync();




        Task<Dictionary<Guid, decimal>> GetAvailableQuantitiesAsync();

        Task<(decimal totalRevenue, int completedOrders, decimal pendingPayments)> GetPaymentStatisticsAsync();
        Task<IEnumerable<SupplierOrder>> GetRecentPaymentsAsync(int count = 20);
        Task<IEnumerable<SupplierOrder>> GetAllOrdersAsync();

    }

}
