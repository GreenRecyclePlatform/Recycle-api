// File: recycle.Application/Services/PaymentService.cs
// FINAL CORRECTED VERSION -
using Microsoft.Extensions.Logging;
using recycle.Application.DTOs.Payment;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayPalPayoutService _payPalService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPayPalPayoutService payPalService,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _payPalService = payPalService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new payment record in Pending status
        /// </summary>
        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate request exists
            var request = await _unitOfWork.PickupRequests.GetByIdAsync(dto.RequestId);
            if (request == null)
                throw new InvalidOperationException($"Pickup request {dto.RequestId} not found");

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(dto.RecipientUserId);
            if (user == null)
                throw new InvalidOperationException($"User {dto.RecipientUserId} not found");

            // Create payment record
            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = dto.RequestId,
                RecipientUserID = dto.RecipientUserId,
                RecipientType = dto.RecipientType,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod ?? PaymentMethods.Wallet,
                PaymentStatus = PaymentStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
                AdminNotes = dto.AdminNotes
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} created for request {RequestId}",
                payment.ID, dto.RequestId);

            return MapToDto(payment);
        }

        /// <summary>
        /// Get all payments with optional status filter
        /// </summary>
        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string? status)
        {
            // FIXED: Use the correct signature from IPaymentRepository
            var payments = await _unitOfWork.Payments.GetAllAsync(status);
            return payments.Select(MapToDto);
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            return payment != null ? MapToDto(payment) : null;
        }

        /// <summary>
        /// Update payment status - When admin approves, money is sent via PayPal
        /// </summary>
        public async Task<bool> UpdatePaymentStatusAsync(
            Guid paymentId,
            string newStatus,
            Guid adminId,
            string? adminNotes = null,
            string? failureReason = null)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return false;
            }

            // Get user email for PayPal
            var user = await _unitOfWork.Users.GetByIdAsync(payment.RecipientUserID);
            if (user == null)
            {
                _logger.LogError("Recipient user {UserId} not found for payment {PaymentId}",
                    payment.RecipientUserID, paymentId);
                return false;
            }

            // Validate user has email
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogError("User {UserId} has no email address for PayPal payout", user.Id);
                payment.PaymentStatus = PaymentStatuses.Failed;
                payment.FailureReason = "User email address is required for PayPal payouts";
                payment.FailedAt = DateTime.UtcNow;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                return false;
            }

            // Update basic fields
            payment.PaymentStatus = newStatus;
            if (adminId != Guid.Empty)
                payment.ApprovedByAdminID = adminId;
            if (!string.IsNullOrWhiteSpace(adminNotes))
                payment.AdminNotes = adminNotes;

            // Handle status-specific logic
            if (newStatus == PaymentStatuses.Approved)
            {
                payment.ApprovedAt = DateTime.UtcNow;

                try
                {
                    _logger.LogInformation("Sending PayPal payout for payment {PaymentId} to {Email}",
                        paymentId, user.Email);

                    // Send money via PayPal
                    var payoutResult = await _payPalService.SendPayoutAsync(
                        recipientEmail: user.Email!,
                        amount: payment.Amount,
                        currency: "EUR", // Change to "EGP" if PayPal supports it
                        note: $"Payment for recyclable materials - Request #{payment.RequestId}"
                    );

                    if (payoutResult.Success)
                    {
                        // PayPal payout succeeded
                        payment.TransactionReference = payoutResult.PayoutBatchId;
                        payment.PaymentStatus = PaymentStatuses.Paid;
                        payment.PaidAt = DateTime.UtcNow;

                        _logger.LogInformation("PayPal payout successful. BatchId: {BatchId}",
                            payoutResult.PayoutBatchId);
                    }
                    else
                    {
                        // PayPal payout failed
                        payment.PaymentStatus = PaymentStatuses.Failed;
                        payment.FailureReason = payoutResult.Message;
                        payment.FailedAt = DateTime.UtcNow;

                        _logger.LogError("PayPal payout failed: {Error}", payoutResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending PayPal payout for payment {PaymentId}", paymentId);

                    payment.PaymentStatus = PaymentStatuses.Failed;
                    payment.FailureReason = $"PayPal error: {ex.Message}";
                    payment.FailedAt = DateTime.UtcNow;
                }
            }
            else if (newStatus == PaymentStatuses.Paid)
            {
                payment.PaidAt = DateTime.UtcNow;
            }
            else if (newStatus == PaymentStatuses.Failed)
            {
                payment.FailedAt = DateTime.UtcNow;
                payment.FailureReason = failureReason;
            }

            // FIXED: Use Update() instead of UpdateAsync()
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Request a payout for a completed pickup (creates payment record)
        /// </summary>
        public async Task<PaymentDto?> RequestPayoutAsync(
            Guid recipientUserId,
            decimal amount,
            Guid requestId,
            string currency = "eur")
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(recipientUserId);
            if (user == null)
            {
                _logger.LogWarning("RequestPayoutAsync: User {UserId} not found", recipientUserId);
                return null;
            }

            // Validate email exists (required for PayPal)
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("User must have a valid email for PayPal payouts");
            }

            // Create payment record
            var createDto = new CreatePaymentDto
            {
                RecipientUserId = recipientUserId,
                RequestId = requestId,
                Amount = amount,
                RecipientType = "User",
                PaymentMethod = PaymentMethods.Wallet, // PayPal
                AdminNotes = $"Payout requested for {currency.ToUpper()} {amount:F2}"
            };

            return await CreatePaymentAsync(createDto);
        }

        #region Helper Methods

        /// <summary>
        /// Map Payment entity to DTO
        /// </summary>
        private PaymentDto MapToDto(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            return new PaymentDto
            {
                ID = payment.ID,
                RequestId = payment.RequestId,
                RecipientUserID = payment.RecipientUserID,
                RecipientType = payment.RecipientType,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference,
                PaymentStatus = payment.PaymentStatus,
                ApprovedByAdminID = payment.ApprovedByAdminID,
                ApprovedAt = payment.ApprovedAt,
                PaidAt = payment.PaidAt,
                FailedAt = payment.FailedAt,
                CreatedAt = payment.CreatedAt,
                AdminNotes = payment.AdminNotes,
                FailureReason = payment.FailureReason
            };
        }

        #endregion
    }
}