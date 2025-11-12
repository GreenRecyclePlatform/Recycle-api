using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // In your application layer DI setup (e.g., AddApplication method)
            //services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            //services.AddScoped<AddressService>();
            //services.AddScoped<OrderService>();
            //services.AddScoped<ReviewService>();

            return services;
        }
    }
}
