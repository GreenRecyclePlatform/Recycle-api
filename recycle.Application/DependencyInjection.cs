using Microsoft.Extensions.DependencyInjection;
﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using recycle.Application.Services;
using recycle.Application.Interfaces;
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
           
            services.AddScoped<IDriverAssignmentService, DriverAssignmentService>();
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            services.AddScoped<AddressService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<IRequestMaterialService, RequestMaterialService>();
            services.AddScoped<DriverProfileService>();


            ///
            

            //services.AddScoped<IPickupRequestService, PickupRequestService>();

            return services;
        }
    }
}
