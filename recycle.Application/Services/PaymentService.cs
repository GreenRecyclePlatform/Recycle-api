using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;


namespace recycle.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentDto dto)
        {
            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = dto.PaymentId,
                RecipientUserID = dto.RecipientUserId,
                RecipientType = dto.RecipientType,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

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
            var query = _context.Payments.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.PaymentStatus == status);

            var payments = await query.ToListAsync();

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                RequestId = p.RequestId,
                RecipientUserId = p.RecipientUserId,
                RecipientType = p.RecipientType,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                PaymentStatus = p.PaymentStatus,
                AdminNotes = p.AdminNotes,
                FailureReason = p.FailureReason
            });
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
        {
            var p = await _context.Payments.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
            if (p == null) return null;

            return new PaymentDto
            {
                PaymentId = p.PaymentId,
                RequestId = p.RequestId,
                RecipientUserId = p.RecipientUserId,
                RecipientType = p.RecipientType,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                PaymentStatus = p.PaymentStatus,
                AdminNotes = p.AdminNotes,
                FailureReason = p.FailureReason
            };
        }

        public async Task<bool> UpdatePaymentStatusAsync(Guid paymentId, string newStatus, int adminId, string? adminNotes = null, string? failureReason = null)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
                return false;

            payment.PaymentStatus = newStatus;
            payment.ApprovedByAdminId = adminId;
            payment.ApprovedAt = DateTime.UtcNow;

            if (newStatus == "Paid")
                payment.PaidAt = DateTime.UtcNow;

            if (newStatus == "Failed")
            {
                payment.FailedAt = DateTime.UtcNow;
                payment.FailureReason = failureReason;
            }

            payment.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

