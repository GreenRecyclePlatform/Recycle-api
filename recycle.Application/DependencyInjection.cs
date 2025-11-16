<<<<<<< HEAD
﻿using Microsoft.Extensions.DependencyInjection;
=======
﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using recycle.Application.Services;
using recycle.Application.Interfaces;
>>>>>>> origin/dev
using recycle.Application.Interfaces.IService;
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
<<<<<<< HEAD
            // In your application layer DI setup (e.g., AddApplication method)
            //services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
            services.AddScoped<IDriverAssignmentService, DriverAssignmentService>();
=======
            //In your application layer DI setup(e.g., AddApplication method)
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
>>>>>>> origin/dev

            services.AddScoped<AddressService>();
            services.AddScoped<IReviewService, ReviewService>();

            
            //services.AddScoped<IPickupRequestService, PickupRequestService>();

            return services;
        }
    }
}
