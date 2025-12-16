// File: recycle.Application/Services/PaymentService.cs
// FINAL CORRECTED VERSION -
using Microsoft.EntityFrameworkCore;
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
        /* public async Task<bool> UpdatePaymentStatusAsync(
         //    Guid paymentId,
         //    string newStatus,
         //    Guid adminId,
         //    string? adminNotes = null,
         //    string? failureReason = null)
         //{
         //    var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
         //    if (payment == null)
         //    {
         //        _logger.LogWarning("Payment {PaymentId} not found", paymentId);
         //        return false;
         //    }

         //    // Get user email for PayPal
         //    var user = await _unitOfWork.Users.GetByIdAsync(payment.RecipientUserID);
         //    if (user == null)
         //    {
         //        _logger.LogError("Recipient user {UserId} not found for payment {PaymentId}",
         //            payment.RecipientUserID, paymentId);
         //        return false;
         //    }

         //    // Validate user has email
         //    if (string.IsNullOrWhiteSpace(user.Email))
         //    {
         //        _logger.LogError("User {UserId} has no email address for PayPal payout", user.Id);
         //        payment.PaymentStatus = PaymentStatuses.Failed;
         //        payment.FailureReason = "User email address is required for PayPal payouts";
         //        payment.FailedAt = DateTime.UtcNow;
         //        _unitOfWork.Payments.Update(payment);
         //        await _unitOfWork.SaveChangesAsync();
         //        return false;
         //    }

         //    // Update basic fields
         //    payment.PaymentStatus = newStatus;
         //    if (adminId != Guid.Empty)
         //        payment.ApprovedByAdminID = adminId;
         //    if (!string.IsNullOrWhiteSpace(adminNotes))
         //        payment.AdminNotes = adminNotes;

         //    // Handle status-specific logic
         //    //if (newStatus == PaymentStatuses.Approved)
         //    //{
         //    //    payment.ApprovedAt = DateTime.UtcNow;

         //    //    try
         //    //    {
         //    //        _logger.LogInformation("Sending PayPal payout for payment {PaymentId} to {Email}",
         //    //            paymentId, user.Email);

         //    //        // Send money via PayPal
         //    //        var payoutResult = await _payPalService.SendPayoutAsync(
         //    //            recipientEmail: user.Email!,
         //    //            amount: payment.Amount,
         //    //            currency: "EUR", // Change to "EGP" if PayPal supports it
         //    //            note: $"Payment for recyclable materials - Request #{payment.RequestId}"
         //    //        );

         //    //        if (payoutResult.Success)
         //    //        {
         //    //            // PayPal payout succeeded
         //    //            payment.TransactionReference = payoutResult.PayoutBatchId;
         //    //            payment.PaymentStatus = PaymentStatuses.Paid;
         //    //            payment.PaidAt = DateTime.UtcNow;

         //    //            _logger.LogInformation("PayPal payout successful. BatchId: {BatchId}",
         //    //                payoutResult.PayoutBatchId);
         //    //        }
         //    //        else
         //    //        {
         //    //            // PayPal payout failed
         //    //            payment.PaymentStatus = PaymentStatuses.Failed;
         //    //            payment.FailureReason = payoutResult.Message;
         //    //            payment.FailedAt = DateTime.UtcNow;

         //    //            _logger.LogError("PayPal payout failed: {Error}", payoutResult.Message);
         //    //        }
         //    //    }
         //    //    catch (Exception ex)
         //    //    {
         //    //        _logger.LogError(ex, "Error sending PayPal payout for payment {PaymentId}", paymentId);

         //    //        payment.PaymentStatus = PaymentStatuses.Failed;
         //    //        payment.FailureReason = $"PayPal error: {ex.Message}";
         //    //        payment.FailedAt = DateTime.UtcNow;
         //    //    }
         //    //}

         //    else if (newStatus == PaymentStatuses.Paid)
         //    {
         //        payment.PaidAt = DateTime.UtcNow;
         //    }
         //    else if (newStatus == PaymentStatuses.Failed)
         //    {
         //        payment.FailedAt = DateTime.UtcNow;
         //        payment.FailureReason = failureReason;
         //    }

         //    // FIXED: Use Update() instead of UpdateAsync()
         //    _unitOfWork.Payments.Update(payment);
         //    await _unitOfWork.SaveChangesAsync();

         //    return true;
         //}*/
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

            // ✅ MODIFIED: Validate PayPal email instead of regular email
            if (string.IsNullOrWhiteSpace(user.PayPalEmail))
            {
                _logger.LogError("User {UserId} has no PayPal email address configured", user.Id);
                payment.PaymentStatus = PaymentStatuses.Failed;
                payment.FailureReason = "User PayPal email address is required for PayPal payouts";
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
                    // ✅ MODIFIED: Log with PayPal email
                    _logger.LogInformation("Sending PayPal payout for payment {PaymentId} to {PayPalEmail}",
                        paymentId, user.PayPalEmail);

                    // ✅ MODIFIED: Use PayPalEmail instead of Email
                    var payoutResult = await _payPalService.SendPayoutAsync(
                        recipientEmail: user.PayPalEmail,  // ✅ Changed from user.Email to user.PayPalEmail
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
            if (string.IsNullOrWhiteSpace(user.PayPalEmail))
            {
                throw new InvalidOperationException("User must have a valid email for PayPal payouts");
            }

            //==========================================

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

        /// <summary>
        /// ✨ NEW: Get payments for a specific user
        /// </summary>
        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetByUserIdAsync(userId);
            return payments.Select(MapToDto);
        }

        /// <summary>
        /// ✨ NEW: Get payment summary for a user
        /// </summary>
        public async Task<PaymentSummaryDto> GetSummaryAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetByUserIdAsync(userId);
            var paymentList = payments.ToList();

            var completedPayments = paymentList
                .Where(p => p.PaymentStatus == PaymentStatuses.Paid)
                .ToList();

            // Calculate earnings (payments received)
            var totalEarnings = completedPayments
                .Where(p => p.RecipientType == "User" || p.RecipientType == "Driver")
                .Sum(p => p.Amount);

            // Calculate withdrawals (for now, assume all payments are earnings)
            // You may need to add a Type field to Payment entity later
            var totalWithdrawals = 0m; // Will be implemented when you add payment types

            var pendingAmount = paymentList
                .Where(p => p.PaymentStatus == PaymentStatuses.Pending ||
                           p.PaymentStatus == PaymentStatuses.Approved)
                .Sum(p => p.Amount);

            var lastPayment = paymentList
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            return new PaymentSummaryDto
            {
                TotalEarnings = totalEarnings,
                TotalWithdrawals = totalWithdrawals,
                PendingAmount = pendingAmount,
                AvailableBalance = totalEarnings - totalWithdrawals,
                LastPaymentDate = lastPayment?.CreatedAt
            };
        }

        /// <summary>
        /// ✨ NEW: Filter payments with pagination
        /// </summary>
        public async Task<PagedResultDto<PaymentDto>> FilterPaymentsAsync(PaymentFilterDto filter)
        {
            // Get queryable
            var query = _unitOfWork.Payments.GetQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
            {
                query = query.Where(p => p.RecipientUserID == filter.UserId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(p => p.PaymentStatus == filter.Status);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to DTOs
            var paymentDtos = payments.Select(MapToDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return new PagedResultDto<PaymentDto>
            {
                Data = paymentDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// ✨ NEW: Complete a payment (mark as paid with transaction ID)
        /// </summary>
        public async Task<bool> CompletePaymentAsync(Guid id, string transactionId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", id);
                return false;
            }

            payment.PaymentStatus = PaymentStatuses.Paid;
            payment.TransactionReference = transactionId;
            payment.PaidAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} marked as completed with transaction {TransactionId}",
                id, transactionId);

            return true;
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
                // ✅ Map backend ID to frontend paymentId
                paymentId = payment.ID,

                // ✅ Map backend RecipientUserID to frontend userId
                userId = payment.RecipientUserID,

                // Get user name if available
                userName = payment.RecipientUser != null
            ? $"{payment.RecipientUser.FirstName} {payment.RecipientUser.LastName}"
            : "Unknown",

                // ✅ Map backend RequestId to frontend pickupRequestId
                pickupRequestId = payment.RequestId,

                amount = payment.Amount,

                // ✅ Set type based on RecipientType or default to "Earning"
                type = "Earning",  // You can make this dynamic if needed

                // ✅ Map backend PaymentStatus to frontend status
                status = payment.PaymentStatus,

                // ✅ Map backend PaymentMethod to frontend paymentMethod
                paymentMethod = payment.PaymentMethod,

                // ✅ Map backend TransactionReference to frontend transactionId
                transactionId = payment.TransactionReference,

                // Build description
                description = $"Payment for pickup request {payment.RequestId}",

                createdAt = payment.CreatedAt,

                // ✅ Map backend ApprovedAt to frontend processedAt
                processedAt = payment.ApprovedAt != default(DateTime)
            ? payment.ApprovedAt
            : null,

                // ✅ Map backend PaidAt to frontend completedAt
                completedAt = payment.PaidAt != default(DateTime)
            ? payment.PaidAt
            : null,

                // ✅ Map backend FailureReason to frontend failureReason
                failureReason = payment.FailureReason
            };
        }

        #endregion
    }
}