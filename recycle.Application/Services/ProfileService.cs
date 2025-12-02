// recycle.Application/Services/ProfileService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs;
using recycle.Application.DTOs.Notifications;
using recycle.Application.DTOs.Profile;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Addresses)
                .Include(u => u.pickupRequests)
                    .ThenInclude(pr => pr.RequestMaterials)
                .Include(u => u.Payments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var primaryAddress = user.Addresses?.FirstOrDefault();

            // Calculate stats
            var totalRequests = user.pickupRequests?.Count ?? 0;
            var completedPickups = user.pickupRequests?.Count(p => p.Status == "Completed") ?? 0;
            var totalEarnings = user.Payments?
                .Where(p => p.PaymentStatus == "Completed")
                .Sum(p => p.Amount) ?? 0;

            // Calculate environmental impact
            var materialsRecycled = user.pickupRequests?
                .Where(p => p.Status == "Completed")
                .Sum(p => p.RequestMaterials?.Sum(rm => rm.EstimatedWeight) ?? 0) ?? 0;

            // Get achievements
            var achievements = GetUserAchievements(
                user.CreatedAt,
                totalRequests,
                completedPickups,
                materialsRecycled
            );

            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                PrimaryAddress = primaryAddress != null ? new AddressDto
                {
                    Id = primaryAddress.Id,
                    Street = primaryAddress.Street,
                    City = primaryAddress.City,
                    Governorate = primaryAddress.Governorate,
                    PostalCode = primaryAddress.PostalCode
                } : null,
                Stats = new ProfileStatsDto
                {
                    TotalRequests = totalRequests,
                    CompletedPickups = completedPickups,
                    TotalEarnings = totalEarnings,
                    ImpactScore = completedPickups * 10
                },
                EnvironmentalImpact = new EnvironmentalImpactDto
                {
                    MaterialsRecycled = materialsRecycled,
                    Co2Saved = materialsRecycled * 0.5m,
                    TreesEquivalent = (int)(materialsRecycled / 10)
                },
                NotificationPreferences = new NotificationPreferencesDto
                {
                    EmailNotifications = user.EmailNotifications,
                    SmsNotifications = user.SmsNotifications,
                    PickupReminders = user.PickupReminders,
                    MarketingEmails = user.MarketingEmails
                },
                Achievements = achievements
            };
        }

        /// <summary>
        /// Calculate user achievements based on activity
        /// </summary>
        private List<AchievementDto> GetUserAchievements(
            DateTime accountCreatedAt,
            int totalRequests,
            int completedPickups,
            decimal materialsRecycled)
        {
            var achievements = new List<AchievementDto>();
            var now = DateTime.UtcNow;

            // ✅ Achievement 1: First Pickup
            var hasFirstPickup = completedPickups >= 1;
            achievements.Add(new AchievementDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Icon = "🎉",
                Title = "First Pickup",
                Description = "Completed your first recycling pickup",
                EarnedDate = hasFirstPickup ? accountCreatedAt.AddDays(7) : null, // Assume earned 7 days after signup
                Unlocked = hasFirstPickup
            });

            // ✅ Achievement 2: Top Contributor
            var isTopContributor = completedPickups >= 10;
            achievements.Add(new AchievementDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Icon = "🌟",
                Title = "Top Contributor",
                Description = "Completed 10+ pickups",
                EarnedDate = isTopContributor ? now.AddDays(-30) : null,
                Unlocked = isTopContributor
            });

            // ✅ Achievement 3: Green Warrior
            var isGreenWarrior = materialsRecycled >= 100;
            achievements.Add(new AchievementDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Icon = "🏆",
                Title = "Green Warrior",
                Description = "Recycled over 100kg of materials",
                EarnedDate = isGreenWarrior ? now.AddDays(-60) : null,
                Unlocked = isGreenWarrior
            });

            // ✅ Achievement 4: Eco Champion - FIXED LOGIC
            var accountAgeDays = (now - accountCreatedAt).TotalDays;
            var isEcoChampion = accountAgeDays >= 180; // 6 months = 180 days
            var ecoChampionEarnedDate = isEcoChampion ? accountCreatedAt.AddDays(180) : (DateTime?)null;

            achievements.Add(new AchievementDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Icon = "🎖️",
                Title = "Eco Champion",
                Description = "6 months of active recycling",
                EarnedDate = ecoChampionEarnedDate,
                Unlocked = isEcoChampion
            });

            return achievements;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.DateOfBirth = dto.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return await GetProfileAsync(userId);
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid userId, UpdateAddressDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var primaryAddress = user.Addresses?.FirstOrDefault();

            if (primaryAddress == null)
            {
                primaryAddress = new Address
                {
                    UserId = userId,
                    Street = dto.Street,
                    City = dto.City,
                    Governorate = dto.Governorate,
                    PostalCode = dto.PostalCode
                };
                await _unitOfWork.Addresses.AddAsync(primaryAddress);
            }
            else
            {
                primaryAddress.Street = dto.Street;
                primaryAddress.City = dto.City;
                primaryAddress.Governorate = dto.Governorate;
                primaryAddress.PostalCode = dto.PostalCode;
                await _unitOfWork.Addresses.UpdateAsync(primaryAddress);
            }

            await _unitOfWork.SaveChangesAsync();

            return new AddressDto
            {
                Id = primaryAddress.Id,
                Street = primaryAddress.Street,
                City = primaryAddress.City,
                Governorate = primaryAddress.Governorate,
                PostalCode = primaryAddress.PostalCode
            };
        }

        public async Task<NotificationPreferencesDto> UpdateNotificationPreferencesAsync(
            Guid userId,
            NotificationPreferencesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            user.EmailNotifications = dto.EmailNotifications;
            user.SmsNotifications = dto.SmsNotifications;
            user.PickupReminders = dto.PickupReminders;
            user.MarketingEmails = dto.MarketingEmails;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return dto;
        }
    }
}