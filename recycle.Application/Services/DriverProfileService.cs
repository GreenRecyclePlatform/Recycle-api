using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycle.Application.DTOs.DriverAssignments;
using recycle.Application.DTOs;



namespace recycle.Application.Services
{
    public class DriverProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DriverProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DriverProfile> GetDriverProfileById(Guid Id)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(dp => dp.Id == Id);
            return driverProfile;
        }

        public async Task<DriverProfile> CreateDriverProfile(DriverProfileDto driverProfileDto, Guid userId)
        {
            var driverProfile = new DriverProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                profileImageUrl = driverProfileDto.Image,
                idNumber = driverProfileDto.IdNumber,
                Rating = 0,
                ratingCount = 0,
                IsAvailable = true,
                TotalTrips = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DriverProfiles.AddAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();
            return driverProfile;
        }

        public async Task<bool> UpdateDriverAvailability(Guid userId, bool isAvailable)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(dp => dp.UserId == userId);
            if (driverProfile == null)
            {
                return false;
            }
            driverProfile.IsAvailable = isAvailable;
            await _unitOfWork.DriverProfiles.UpdateAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<DriverProfileResponseDto>> GetDriverProfiles()
        {
            var driverProfiles = await _unitOfWork.DriverProfiles.GetAll(
                filter: null,
                includes: query => query
                    .Include(dp => dp.User)
                        .ThenInclude(u => u.Addresses)
            );

            
            // Map إلى DTO
            var result = driverProfiles.Select(dp => new DriverProfileResponseDto
            {
                Id = dp.Id,
                UserId = dp.UserId,
                FirstName = dp.User.FirstName,
                LastName = dp.User.LastName,
                ProfileImageUrl = dp.profileImageUrl,
                IdNumber = dp.idNumber,
                Rating = dp.Rating,
                RatingCount = dp.ratingCount,
                IsAvailable = dp.IsAvailable,
                TotalTrips = dp.TotalTrips,
                CreatedAt = dp.CreatedAt,

                // first Address if more 1
                Address = dp.User.Addresses?.FirstOrDefault() != null
                    ? new AddressDto
                    {
                        Street = dp.User.Addresses.First().Street,
                        City = dp.User.Addresses.First().City,
                        Governorate = dp.User.Addresses.First().Governorate,
                        PostalCode = dp.User.Addresses.First().PostalCode
                    }
                    : null
            }).ToList();

            return result;
        }
        public async Task<bool> DeleteDriverProfile(Guid userId)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(dp => dp.UserId == userId);
            if (driverProfile == null)
            {
                return false;
            }
            await _unitOfWork.DriverProfiles.RemoveAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
