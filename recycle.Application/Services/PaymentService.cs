using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;


namespace recycle.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentDto dto)
        {
            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = dto.RequestId,
                RecipientUserID = dto.RecipientUserId,
                RecipientType = dto.RecipientType,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentDto
            {
                PaymentId = payment.ID,
                RequestId = payment.RequestId,
                RecipientUserId = payment.RecipientUserID,
                RecipientType = payment.RecipientType,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference,
                PaymentStatus = payment.PaymentStatus
            };
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string? status)
        {
            var payments = await _unitOfWork.Payments.GetAllAsync(status);

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.ID,
                RequestId = p.RequestId,
                RecipientUserId = p.RecipientUserID,
                RecipientType = p.RecipientType,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                PaymentStatus = p.PaymentStatus,
                AdminNotes = p.AdminNotes,
                FailureReason = p.FailureReason
            });
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
        {
            var p = await _unitOfWork.Payments.GetByIdAsync(id);
            if (p == null) return null;

            return new PaymentDto
            {
                PaymentId = p.ID,
                RequestId = p.RequestId,
                RecipientUserId = p.RecipientUserID,
                RecipientType = p.RecipientType,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                PaymentStatus = p.PaymentStatus,
                AdminNotes = p.AdminNotes,
                FailureReason = p.FailureReason
            };
        }

        public async Task<bool> UpdatePaymentStatusAsync(Guid paymentId, string newStatus, Guid adminId, string? adminNotes = null, string? failureReason = null)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null) return false;

            payment.PaymentStatus = newStatus;
            payment.ApprovedByAdminID = adminId;
            payment.ApprovedAt = DateTime.UtcNow;
            payment.AdminNotes = adminNotes;
            payment.FailureReason = failureReason;

            if (newStatus == "Paid")
                payment.PaidAt = DateTime.UtcNow;

            if (newStatus == "Failed")
                payment.FailedAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}

