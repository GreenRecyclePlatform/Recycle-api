using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using recycle.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// CRITICAL: Use fully qualified names to avoid ambiguity
using PayPalEnvironment = PayPalCheckoutSdk.Core.PayPalEnvironment;
using SandboxEnvironment = PayPalCheckoutSdk.Core.SandboxEnvironment;
using LiveEnvironment = PayPalCheckoutSdk.Core.LiveEnvironment;
using PayPalHttpClient = PayPalCheckoutSdk.Core.PayPalHttpClient;
using PayPalHttp;
using PayoutsSdk.Payouts;

namespace recycle.Infrastructure.ExternalServices
{
    public class PayPalPayoutService : IPayPalPayoutService
    {
        private readonly PayPalHttpClient _client;
        private readonly PayPalOptions _options;
        private readonly ILogger<PayPalPayoutService> _logger;

        public PayPalPayoutService(
            IOptions<PayPalOptions> options,
            ILogger<PayPalPayoutService> logger)
        {
            _options = options.Value;
            _logger = logger;

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_options.ClientId))
                throw new InvalidOperationException("PayPal ClientId is not configured");
            if (string.IsNullOrWhiteSpace(_options.ClientSecret))
                throw new InvalidOperationException("PayPal ClientSecret is not configured");

            // Create PayPal environment
            PayPalEnvironment environment;
            if (_options.Mode?.ToLower() == "live")
            {
                environment = new LiveEnvironment(_options.ClientId, _options.ClientSecret);
                _logger.LogWarning("PayPal running in LIVE mode");
            }
            else
            {
                environment = new SandboxEnvironment(_options.ClientId, _options.ClientSecret);
                _logger.LogInformation("PayPal running in SANDBOX mode");
            }

            _client = new PayPalHttpClient(environment);
        }

        /// <summary>
        /// Send money to a user's PayPal account
        /// </summary>
        public async Task<PayoutResponse> SendPayoutAsync(
            string recipientEmail,
            decimal amount,
            string currency,
            string note)
        {
            try
            {
                _logger.LogInformation("Sending PayPal payout: {Amount} {Currency} to {Email}",
                    amount, currency, recipientEmail);

                // Create unique IDs
                var senderBatchId = $"batch_{Guid.NewGuid():N}";
                var senderItemId = $"item_{Guid.NewGuid():N}";

                // Create payout request - using PayoutsSdk types
                var body = new CreatePayoutRequest
                {
                    SenderBatchHeader = new SenderBatchHeader
                    {
                        SenderBatchId = senderBatchId,
                        EmailSubject = "Payment from Recycle Platform",
                        EmailMessage = "You received payment for recyclable materials. Thank you!"
                    },
                    Items = new List<PayoutItem>
                    {
                        new PayoutItem
                        {
                            RecipientType = "EMAIL",
                            Receiver = recipientEmail,
                            Amount = new Currency
                            {
                                CurrencyCode = currency.ToUpper(),
                                Value = amount.ToString("F2")
                            },
                            Note = note,
                            SenderItemId = senderItemId,
                            RecipientWallet = "PAYPAL"
                        }
                    }
                };

                // Create and execute request
                var request = new PayoutsPostRequest();
                request.RequestBody(body);

                var response = await _client.Execute(request);
                var result = response.Result<CreatePayoutResponse>();

                _logger.LogInformation("PayPal payout created. BatchId: {BatchId}, Status: {Status}",
                    result.BatchHeader.PayoutBatchId, result.BatchHeader.BatchStatus);

                return new PayoutResponse
                {
                    Success = true,
                    PayoutBatchId = result.BatchHeader.PayoutBatchId,
                    Status = result.BatchHeader.BatchStatus,
                    Message = "Payout created successfully"
                };
            }
            catch (HttpException httpEx)
            {
                var errorMessage = $"PayPal HTTP error: {httpEx.Message}";
                _logger.LogError(httpEx, "PayPal payout failed: {Error}", errorMessage);

                return new PayoutResponse
                {
                    Success = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, "PayPal payout exception: {Error}", errorMessage);

                return new PayoutResponse
                {
                    Success = false,
                    Message = errorMessage
                };
            }
        }

        /// <summary>
        /// Check the status of a payout batch
        /// </summary>
        public async Task<string> GetPayoutStatusAsync(string payoutBatchId)
        {
            try
            {
                _logger.LogInformation("Checking PayPal payout status: {BatchId}", payoutBatchId);

                var request = new PayoutsGetRequest(payoutBatchId);
                var response = await _client.Execute(request);

                // Explicitly use PayoutsSdk.Payouts.PayoutBatch
                var result = response.Result<PayoutsSdk.Payouts.PayoutBatch>();

                var status = result.BatchHeader.BatchStatus;
                _logger.LogInformation("Payout {BatchId} status: {Status}", payoutBatchId, status);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout status for {BatchId}", payoutBatchId);
                throw new Exception($"Error getting payout status: {ex.Message}", ex);
            }
        }
    }
}