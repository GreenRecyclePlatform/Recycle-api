using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
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
      
            //services.AddScoped<IPaymentService, PaymentService>();
            //services.AddScoped<ITokenService, TokenService>();
            //services.AddScoped<IAuthService, AuthService>();
            //services.AddTransient<IEmailService, EmailService>();

            return services;
        }

        private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Database")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<DbInitializer>();

            services.AddScoped<IDriverAssignmentRepository, DriverAssignmentRepository>();

            //for example
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IProductRepository, ProductRepository>();
            //services.AddScoped<ICartRepository, CartRepository>();
            //services.AddScoped<IReviewRepository, ReviewRepository>();
            //services.AddScoped<IWishlistRepository, WishlistRepository>();


        }
    }
}
