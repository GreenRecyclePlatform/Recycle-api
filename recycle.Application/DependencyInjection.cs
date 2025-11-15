using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using recycle.Application.Services;
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
            //In your application layer DI setup(e.g., AddApplication method)
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            services.AddScoped<AddressService>();

            return services;
        }
    }
}
