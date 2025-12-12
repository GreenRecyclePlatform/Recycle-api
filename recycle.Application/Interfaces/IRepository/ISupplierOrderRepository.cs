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
            /// <summary>
            /// إنشاء Order جديد
            /// </summary>
            Task<SupplierOrder> CreateOrderAsync(SupplierOrder order);

            /// <summary>
            /// جلب Order بالـ ID مع الـ Items
            /// </summary>
            Task<SupplierOrder?> GetOrderByIdAsync(Guid orderId);

            /// <summary>
            /// جلب كل Orders لـ Supplier معين (للـ Order History)
            /// </summary>
            Task<IEnumerable<SupplierOrder>> GetOrdersBySupplierIdAsync(Guid supplierId);

            /// <summary>
            /// تحديث حالة الدفع بعد نجاح Stripe
            /// </summary>
            Task<bool> UpdatePaymentStatusAsync(Guid orderId, string status, string paymentIntentId);

            /// <summary>
            /// حفظ التغييرات
            /// </summary>
            Task<bool> SaveChangesAsync();


        Task<Dictionary<Guid, decimal>> GetAvailableQuantitiesAsync();

    }

}
