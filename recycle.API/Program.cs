using Microsoft.AspNetCore.Identity;
using recycle.Application;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Application.Services;
using recycle.Domain.Entities;
using recycle.Infrastructure;
using recycle.Infrastructure.Hubs;
using recycle.Infrastructure.Repositories;
using recycle.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//// Register Specific Repositories (for UnitOfWork)
builder.Services.AddScoped < IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IRepository<Notification>, Repository<Notification>>();
//builder.Services.AddScoped<IRepository<ApplicationUser>, Repository<ApplicationUser>>(); 
//builder.Services.AddScoped<IRepository<PickupRequest>, Repository<PickupRequest>>();
//builder.Services.AddScoped<IRepository<DriverAssignment>, Repository<DriverAssignment>>();

builder.Services.AddControllers();
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
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
//builder.Services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
//builder.Services.AddScoped<IPickupRequestService, PickupRequestService>();
//// ================================================================================

//// Reviews
builder.Services.AddScoped<IReviewService, ReviewService>();

//// Notifications
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");


// Add a root endpoint
//app.MapGet("/", () => Results.Ok(new
//{
//    message = "Recycle API is running!",
//    swagger = "/swagger",
//    signalrHub = "/notificationHub"
//}));

app.Run();
