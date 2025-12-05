//using Microsoft.Extensions.Options;
//using recycle.Application.DTOs.Payment;
//using recycle.Application.Interfaces;
//using recycle.Application.Options;
//using Stripe;

//namespace recycle.Infrastructure.ExternalServices
//{
//    public class StripeAdapter : IStripeAdapter
//    {
//        private readonly StripeOptions _options;

//        public StripeAdapter(IOptions<StripeOptions> options)
//        {
//            _options = options.Value;
//            if (!string.IsNullOrWhiteSpace(_options.SecretKey))
//            {
//                StripeConfiguration.ApiKey = _options.SecretKey;
//            }
//        }

//        public async Task<string> CreateExpressAccountAsync(Guid userId, string email)
//        {
//            var accountService = new AccountService();

//            var account = await accountService.CreateAsync(new AccountCreateOptions
//            {
//                Type = "express",
//                Email = email,
//                Capabilities = new AccountCapabilitiesOptions
//                {
//                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
//                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
//                }
//            });

//            return account.Id;
//        }

//        public async Task<string> CreateAccountLinkAsync(string stripeAccountId, string refreshUrl, string returnUrl)
//        {
//            var linkService = new AccountLinkService();

//            var link = await linkService.CreateAsync(new AccountLinkCreateOptions
//            {
//                Account = stripeAccountId,
//                RefreshUrl = refreshUrl,
//                ReturnUrl = returnUrl,
//                Type = "account_onboarding"
//            });

//            return link.Url!;
//        }

//        public async Task<StripeTransferResult> CreateTransferToConnectedAccountAsync(Guid paymentId, string stripeAccountId, long amountInCents, string currency = "usd", string? description = null)
//        {
//            var transferService = new TransferService();

//            var transfer = await transferService.CreateAsync(new TransferCreateOptions
//            {
//                Amount = amountInCents,
//                Currency = currency,
//                Destination = stripeAccountId,
//                Description = description,
//                Metadata = new Dictionary<string, string>{{ "paymentId", paymentId.ToString() }}
//            });

//            return new StripeTransferResult
//            {
//                TransferId = transfer.Id,
//                Status = "paid", // Stripe provides "paid", "pending", etc.
//                PayoutId = transfer.DestinationPayment?.ToString()
//            };
//        }

//        public Event VerifyAndConstructEvent(string jsonPayload, string signatureHeader)
//        {
//            return EventUtility.ConstructEvent(
//                jsonPayload,
//                signatureHeader,
//                _options.WebhookSecret
//            );
//        }

//        public async Task<Charge> CreateTestChargeAsync(long amountInCents, string currency = "usd")
//        {
//            var chargeService = new ChargeService();
//            var charge = await chargeService.CreateAsync(new ChargeCreateOptions
//            {
//                Amount = amountInCents,
//                Currency = currency,
//                Source = "tok_visa", // generic test token; for available balance test use special card as guidance
//                Description = "Test charge for adding balance to platform"
//            });

//            return charge;
//        }
//    }
//}
