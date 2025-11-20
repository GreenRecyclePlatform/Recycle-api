using recycle.Application.DTOs.Payment;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IStripeAdapter
    {
        Task<string> CreateExpressAccountAsync(Guid userId, string email);
        Task<string> CreateAccountLinkAsync(string stripeAccountId, string refreshUrl, string returnUrl);
        Task<StripeTransferResult> CreateTransferToConnectedAccountAsync(Guid paymentId, string stripeAccountId, long amountInCents, string currency = "usd", string? description = null);
        Event VerifyAndConstructEvent(string jsonPayload, string signatureHeader);
        Task<Charge> CreateTestChargeAsync(long amountInCents, string currency = "usd");
    }
}
