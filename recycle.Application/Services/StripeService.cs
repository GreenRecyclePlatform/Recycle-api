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

            // تعيين الـ Secret Key من appsettings.json
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        /// <summary>
        /// إنشاء PaymentIntent في Stripe
        /// </summary>
        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency = "usd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe بيستخدم cents (مثلاً: $10.50 = 1050)
                Currency = currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },

                // Optional: Metadata
                Metadata = new Dictionary<string, string>
                {
                    { "integration_check", "accept_a_payment" }
                }
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        /// <summary>
        /// التحقق من حالة PaymentIntent
        /// </summary>
        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }

        /// <summary>
        /// إلغاء PaymentIntent
        /// </summary>
        public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.CancelAsync(paymentIntentId);
        }
    }
}