//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using recycle.Application.Interfaces;
//using recycle.Application.Interfaces.IRepository;
//using recycle.Application.Interfaces.IService;
//using recycle.Application.Services;
//using recycle.Infrastructure.ExternalServices;
//using recycle.Infrastructure.Repositories;
//using recycle.Infrastructure.Services;
//using recycle.Application.Options;
//using Stripe;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Configuration;

//namespace recycle.Infrastructure
//{
//    public static class DependencyInjection
//    {
//        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
//        {
//            AddPersistence(services, configuration);


//            //for external services
//            // services.AddScoped<IPaymentRepository, PaymentRepository>();
//            //==================================================================================
//            // Payment Services - UPDATED FOR PAYPAL
//            services.AddScoped<IPaymentRepository, PaymentRepository>();
//            services.AddScoped<IPaymentService, PaymentService>();
//            services.AddScoped<IPayPalPayoutService, PayPalPayoutService>();

//            // Configure PayPal Options
//            services.Configure<PayPalOptions>(options =>
//            {
//                options.ClientId = configuration["PayPal:ClientId"] ?? string.Empty;
//                options.ClientSecret = configuration["PayPal:ClientSecret"] ?? string.Empty;
//                options.Mode = configuration["PayPal:Mode"] ?? "sandbox"; // sandbox or live
//            });
//            //==================================================================================

//            services.AddScoped<IUnitOfWork, UnitOfWork>();
//            services.AddScoped<IPaymentService, PaymentService>();
//            services.AddScoped<ITokenService, ExternalServices.TokenService>();
//            services.AddScoped<IAuthService, AuthService>();
//            services.AddScoped<INotificationHubService, NotificationHubService>();
//            services.AddScoped<IEmailService, EmailService>();
//            services.AddScoped<IStripeAdapter, StripeAdapter>();
//            services.AddSignalR();
//            services.Configure<StripeOptions>(options =>
//            {
//                options.SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
//                options.WebhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
//            });
//            var stripeSecretKey = configuration["Stripe:SecretKey"];
//            if (!string.IsNullOrEmpty(stripeSecretKey))
//            {
//                StripeConfiguration.ApiKey = stripeSecretKey;
//            }

//            return services;
//        }

//        private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
//        {
//            services.AddDbContext<AppDbContext>(options =>
//                options.UseSqlServer(configuration.GetConnectionString("Database")));

//            services.AddScoped<IUnitOfWork, UnitOfWork>();
//            services.AddScoped<IUserRepository, UserRepository>();
//            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//            services.AddScoped<INotificationRepository, NotificationRepository>();
//            services.AddScoped<IReviewRepository, ReviewRepository>();
//            services.AddScoped<IMaterialRepository, MaterialRepository>();
//            services.AddScoped<IRequestMaterialRepository, RequestMaterialRepository>();

//            //=====================Abdelrahman Service injections============
//            services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
//            services.AddScoped<IPickupRequestService, PickupRequestService>();

//            services.AddScoped<DbInitializer>();

//            services.AddScoped<IDriverAssignmentRepository, DriverAssignmentRepository>();



//        }
//    }
//}
// File: recycle.Infrastructure/DependencyInjection.cs
// REPLACE with this updated version

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Application.Interfaces.IService.recycle.Application.Interfaces;
//ing recycle.Application.Options;

using recycle.Application.Services;
using recycle.Infrastructure.ExternalServices;
using recycle.Infrastructure.Repositories;
using recycle.Infrastructure.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddPersistence(services, configuration);

            // ===== PAYMENT SERVICES - PAYPAL =====
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPayPalPayoutService, PayPalPayoutService>();

            // FIXED: Proper configuration binding
            services.Configure<PayPalOptions>(options =>
            {
                var paypalSection = configuration.GetSection("PayPal");
                options.ClientId = paypalSection["ClientId"] ?? string.Empty;
                options.ClientSecret = paypalSection["ClientSecret"] ?? string.Empty;
                options.Mode = paypalSection["Mode"] ?? "sandbox";
            });


            // ===== OTHER SERVICES (Keep as is) =====
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, ExternalServices.TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INotificationHubService, NotificationHubService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddSignalR();

            // ===== REMOVE/COMMENT OUT STRIPE CONFIGURATION =====
            // services.AddScoped<IStripeAdapter, StripeAdapter>(); // DELETE THIS
            // services.Configure<StripeOptions>(...); // DELETE THIS
            // StripeConfiguration.ApiKey = ...; // DELETE THIS

            return services;
        }

        private static void AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Database")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<IRequestMaterialRepository, RequestMaterialRepository>();

            // Pickup request services
            services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
            services.AddScoped<IPickupRequestService, PickupRequestService>();

            services.AddScoped<DbInitializer>();
            services.AddScoped<IDriverAssignmentRepository, DriverAssignmentRepository>();


            //

            services.AddScoped<IKnowledgeService, RecycleKnowledgeService>();

        }
    }
}