using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs;
using recycle.Application.DTOs.Notifications;
using recycle.Application.DTOs.Profile;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            Debug.WriteLine($"=== FETCHING PROFILE FOR USER ID: {userId} ===");

            var user = await _userManager.Users
                .Include(u => u.Addresses)
                .Include(u => u.pickupRequests)
                    .ThenInclude(pr => pr.RequestMaterials)
                .Include(u => u.Payments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                Debug.WriteLine($"❌ USER NOT FOUND: {userId}");
                throw new Exception("User not found");
            }

            Debug.WriteLine($"✅ USER FOUND: {user.FirstName} {user.LastName}");

            var primaryAddress = user.Addresses?.FirstOrDefault();

            // Calculate stats
            var totalRequests = user.pickupRequests?.Count ?? 0;
            var completedPickups = user.pickupRequests?.Count(p =>
                p.Status != null && (
                    p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    p.Status.Equals("Complete", StringComparison.OrdinalIgnoreCase)
                )) ?? 0;

            var totalEarnings = user.Payments?
                .Where(p => p.PaymentStatus == "Completed")
                .Sum(p => p.Amount) ?? 0;

            // Calculate environmental impact
            var materialsRecycled = user.pickupRequests?
                .Where(p => p.Status != null &&
                    p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.RequestMaterials?.Sum(rm => rm.EstimatedWeight) ?? 0) ?? 0;

            
            Debug.WriteLine($"Stats: Requests={totalRequests}, Completed={completedPickups}, Materials={materialsRecycled}kg");


            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                PrimaryAddress = primaryAddress != null ? new Address
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
                    
                },

                NotificationPreferences = new NotificationPreferencesDto
                {
                    EmailNotifications = user.EmailNotifications,
                    SmsNotifications = user.SmsNotifications,
                    PickupReminders = user.PickupReminders,
                    MarketingEmails = user.MarketingEmails
                },
            };
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
        public async Task<Address> UpdateAddressAsync(Guid userId, UpdateAddressDto dto)
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

            // ✅ RETURN THE EXISTING primaryAddress OBJECT
            return primaryAddress;
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