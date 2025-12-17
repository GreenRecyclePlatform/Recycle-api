using Microsoft.Extensions.Configuration;
using Stripe;

namespace recycle.Application.Services
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency = "usd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), 
                Currency = currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },

                Metadata = new Dictionary<string, string>
                {
                    { "integration_check", "accept_a_payment" }
                }
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }

       
        public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.CancelAsync(paymentIntentId);
        }
    }
}