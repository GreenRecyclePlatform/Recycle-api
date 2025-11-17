using Microsoft.Extensions.Logging;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using recycle.Domain.Enums;


namespace recycle.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeAdapter _stripeAdapter;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IStripeAdapter stripeAdapter, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _stripeAdapter = stripeAdapter;
            _logger = logger;
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = dto.RequestId,
                RecipientUserID = dto.RecipientUserId,
                RecipientType = string.IsNullOrWhiteSpace(dto.RecipientType) ? "User" : dto.RecipientType,
                Amount = dto.Amount,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? PaymentMethods.Cash : dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                PaymentStatus = PaymentStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
                AdminNotes = dto.AdminNotes,
                FailureReason = dto.FailureReason
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string? status)
        {
            var payments = await _unitOfWork.Payments.GetAllAsync(status);
            return payments.Select(MapToDto);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
        {
            var p = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (p == null) return null;
            return MapToDto(p);
        }

        public async Task<bool> UpdatePaymentStatusAsync(Guid paymentId, string newStatus, int adminId, string? adminNotes = null, string? failureReason = null)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null) return false;

            payment.PaymentStatus = newStatus;
            payment.ApprovedByAdminID = adminId;
            payment.ApprovedAt = DateTime.UtcNow;
            payment.AdminNotes = adminNotes;
            payment.FailureReason = failureReason;

            if (newStatus == PaymentStatuses.Paid)
                payment.PaidAt = DateTime.UtcNow;

            if (newStatus == PaymentStatuses.Failed)
                payment.FailedAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Request a payout (platform -> connected account).
        /// Creates a Payment record (Pending), calls Stripe transfer, then updates the record with the transfer id and marks as Approved.
        /// Final Paid/Failed states are expected to be set by webhook events.
        /// </summary>
        public async Task<PaymentDto?> RequestPayoutAsync(Guid recipientUserId, decimal amount, Guid requestId, string currency = "usd")
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            // 1. Find recipient (user or driver). We try Users first.
            // This assumes IUnitOfWork exposes Users repository with GetByIdAsync(Guid).
            var recipient = await TryGetUserOrDriverAsync(recipientUserId);
            if (recipient == null)
            {
                _logger.LogWarning("RequestPayoutAsync: recipient not found: {RecipientId}", recipientUserId);
                return null;
            }

            // 2. Check stripe account id on recipient
            var stripe = recipient.Value;
            var stripeAccountId = stripe.StripeAccountId;
            var isDriver = stripe.IsDriver;
            if (string.IsNullOrWhiteSpace(stripeAccountId))
            {
                throw new InvalidOperationException("Recipient is not onboarded to Stripe.");
            }

            // 3. Create payment record (Pending)
            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = requestId,
                RecipientUserID = recipientUserId,
                RecipientType = isDriver ? "Driver" : "User",
                Amount = amount,
                PaymentMethod = PaymentMethods.BankTransfer,
                PaymentStatus = PaymentStatuses.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // 4. Call Stripe to create transfer
            try
            {
                var amountInCents = (long)Math.Round(amount * 100m);
                var stripeResult = await _stripeAdapter.CreateTransferToConnectedAccountAsync(payment.ID, stripeAccountId, amountInCents, currency, $"Payout for payment {payment.ID}");

                // 5. Update payment with transaction reference and provisional status
                payment.TransactionReference = stripeResult.TransferId;
                // mark as approved/processing; final "Paid" will be set by webhook
                payment.PaymentStatus = PaymentStatuses.Approved;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe transfer failed for paymentId {PaymentId}", payment.ID);

                // Update payment as failed
                payment.PaymentStatus = PaymentStatuses.Failed;
                payment.FailureReason = ex.Message;
                payment.FailedAt = DateTime.UtcNow;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                throw; // bubble for controller to return 500 or handle as needed
            }
        }



        #region Helpers & Mapping

        /// <summary>
        /// Map Payment domain entity to DTO
        /// </summary>
        private PaymentDto MapToDto(Payment p)
        {
            if (p == null) return null!;

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

        /// <summary>
        /// Try to get user or driver information (including StripeAccountId).
        /// This method depends on your UnitOfWork exposing a Users repo and/or DriverProfile repo.
        /// The returned dynamic object contains minimal members used by this service:
        ///   - string StripeAccountId
        ///   - bool IsDriver
        /// If your project has concrete types, replace this with strongly-typed repos and types.
        /// </summary>
        private async Task<(string? StripeAccountId, bool IsDriver)?> TryGetUserOrDriverAsync(Guid userId)
        {
            // try Users repo
            try
            {
                var usersRepo = _unitOfWork as dynamic;
                if (usersRepo?.Users != null)
                {
                    var user = await usersRepo.Users.GetByIdAsync(userId);
                    if (user != null)
                    {
                        // user may have StripeAccountId property
                        var stripeAccountId = (string?)(user.StripeAccountId ?? user?.StripeAccountId);
                        return (stripeAccountId, IsDriver: false);
                    }
                }
            }
            catch
            {
                // ignore and continue to driver repo
            }

            // try DriverProfile repo if exists
            try
            {
                var unit = _unitOfWork as dynamic;
                if (unit?.DriverProfiles != null)
                {
                    var driver = await unit.DriverProfiles.GetByIdAsync(userId);
                    if (driver != null)
                    {
                        var stripeAccountId = (string?)(driver.StripeAccountId ?? driver?.StripeAccountId);
                        return (stripeAccountId, IsDriver: true);
                    }
                }
            }
            catch
            {
                // ignore if not present
            }

            // If neither repository exists, try a general Users repository name used in many projects
            try
            {
                var unit = _unitOfWork as dynamic;
                if (unit?.UserRepository != null)
                {
                    var user = await unit.UserRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        var stripeAccountId = (string?)(user.StripeAccountId ?? user?.StripeAccountId);
                        return (stripeAccountId, IsDriver: false);
                    }
                }
            }
            catch
            {
                // final fallback
            }

            return null;
        }

        #endregion


    }
}

