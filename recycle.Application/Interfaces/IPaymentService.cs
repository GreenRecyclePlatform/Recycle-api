// recycle.Application/Interfaces/IPaymentService.cs

using recycle.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IPaymentService
    {
        // Existing methods
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
        Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string? status);
        Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
        Task<bool> UpdatePaymentStatusAsync(Guid paymentId, string newStatus, Guid adminId, string? adminNotes = null, string? failureReason = null);

        // Payout
        Task<PaymentDto?> RequestPayoutAsync(Guid recipientUserId, decimal amount, Guid requestId, string currency = "eur");

        //  NEW methods to add
        Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(Guid userId);
        Task<PaymentSummaryDto> GetSummaryAsync(Guid userId);
        Task<PagedResultDto<PaymentDto>> FilterPaymentsAsync(PaymentFilterDto filter);
        Task<bool> CompletePaymentAsync(Guid id, string transactionId);
    }
}