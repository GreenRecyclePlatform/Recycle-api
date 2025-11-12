using recycle.Application.Interfaces;
using recycle.Application.Services;
using recycle.Domain;
using recycle.Domain.Entities;
using recycle.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using recycle.Application;
using recycle.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register Specific Repositories (for UnitOfWork)
builder.Services.AddScoped<IRepository<Review>, Repository<Review>>();
builder.Services.AddScoped<IRepository<Notification>, Repository<Notification>>();
builder.Services.AddScoped<IRepository<ApplicationUser>, Repository<ApplicationUser>>(); 
builder.Services.AddScoped<IRepository<PickupRequest>, Repository<PickupRequest>>();
builder.Services.AddScoped<IRepository<DriverAssignment>, Repository<DriverAssignment>>();

builder.Services.AddControllers();
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//=======================Abdelrahman Services======================
// ===== ADD YOUR SERVICE REGISTRATIONS HERE (BEFORE var app = builder.Build()) =====
builder.Services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
builder.Services.AddScoped<IPickupRequestService, PickupRequestService>();
// ================================================================================

// Reviews
builder.Services.AddScoped<IReviewService, ReviewService>();

// Notifications
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
