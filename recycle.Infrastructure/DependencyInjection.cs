using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Application.Interfaces.IService.recycle.Application.Interfaces;
using recycle.Application.Options;
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
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddPersistence(services, configuration);


            //for external services
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ITokenService, ExternalServices.TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INotificationHubService, NotificationHubService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStripeAdapter, StripeAdapter>();
            services.AddSignalR();
            services.Configure<StripeOptions>(options =>
            {
                options.SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
                options.WebhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
            });
            var stripeSecretKey = configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(stripeSecretKey))
            {
                StripeConfiguration.ApiKey = stripeSecretKey;
            }

            return services;
        }

        private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
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

            //=====================Abdelrahman Service injections============
            services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
            services.AddScoped<IPickupRequestService, PickupRequestService>();

            services.AddScoped<DbInitializer>();

            services.AddScoped<IDriverAssignmentRepository, DriverAssignmentRepository>();


            //

            services.AddScoped<IKnowledgeService, RecycleKnowledgeService>();

        }
    }
}
