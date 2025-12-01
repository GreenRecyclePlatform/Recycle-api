using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using recycle.Domain.Entities;
using recycle.Infrastructure;

namespace recycle.Infrastructure.Seeders
{
    public static class SettingSeeder
    {
        public static async Task SeedSettings(AppDbContext context)
        {
            // Check if settings already exist
            if (await context.Settings.AnyAsync())
            {
                return; // Already seeded
            }

            var settings = new List<Setting>
            {
                // Platform Settings
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "platformName", Value = "RecycleHub", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "platformEmail", Value = "support@recyclehub.com", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "platformPhone", Value = "+91 22 1234 5678", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "timezone", Value = "Asia/Kolkata", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "currency", Value = "INR", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "platform", Key = "language", Value = "en", CreatedAt = DateTime.UtcNow },

                // Pricing Settings
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "plasticPrice", Value = "25", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "paperPrice", Value = "18", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "cardboardPrice", Value = "22", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "electronicsPrice", Value = "48", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "metalsPrice", Value = "40", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "glassPrice", Value = "15", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "commissionRate", Value = "10", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "pricing", Key = "minimumOrderValue", Value = "500", CreatedAt = DateTime.UtcNow },

                // Notification Settings
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "emailNotifications", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "smsNotifications", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "pushNotifications", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "newRequestAlert", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "pickupCompleteAlert", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "lowInventoryAlert", Value = "true", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "notification", Key = "newOrderAlert", Value = "true", CreatedAt = DateTime.UtcNow },

                // Operational Settings
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "autoApproveRequests", Value = "false", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "autoAssignDrivers", Value = "false", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "workingHoursStart", Value = "08:00", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "workingHoursEnd", Value = "20:00", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "maxPickupsPerDriver", Value = "10", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "pickupSlotDuration", Value = "120", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "operational", Key = "minimumPickupWeight", Value = "5", CreatedAt = DateTime.UtcNow },

                // Payment Settings
                new Setting { Id = Guid.NewGuid(), Category = "payment", Key = "paymentCycle", Value = "weekly", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "payment", Key = "paymentMethod", Value = "bank-transfer", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "payment", Key = "paymentGatewayFee", Value = "2.5", CreatedAt = DateTime.UtcNow },
                new Setting { Id = Guid.NewGuid(), Category = "payment", Key = "processingTime", Value = "3", CreatedAt = DateTime.UtcNow },
            };

            await context.Settings.AddRangeAsync(settings);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Settings seeded successfully!");
        }
    }
}