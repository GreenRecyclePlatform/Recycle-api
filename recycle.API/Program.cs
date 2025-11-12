using recycle.Application.Interfaces;
using recycle.Application.Services;
using recycle.Domain;
using recycle.Domain.Entities;
using recycle.Infrastructure.Repositories;

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Reviews
builder.Services.AddScoped<IReviewService, ReviewService>();

// Notifications
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
