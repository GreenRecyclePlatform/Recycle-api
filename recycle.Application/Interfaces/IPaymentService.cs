using recycle.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentAsync(PaymentDto dto);
        Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string? status);
        Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
        Task<bool> UpdatePaymentStatusAsync(Guid paymentId, string newStatus, Guid adminId, string? adminNotes = null, string? failureReason = null);
        
    }
}
