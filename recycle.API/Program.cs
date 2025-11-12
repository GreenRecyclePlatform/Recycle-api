using recycle.Application.Interfaces;
using recycle.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//=======================Abdelrahman Services======================
// ===== ADD YOUR SERVICE REGISTRATIONS HERE (BEFORE var app = builder.Build()) =====
builder.Services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
builder.Services.AddScoped<IPickupRequestService, PickupRequestService>();
// ================================================================================


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
