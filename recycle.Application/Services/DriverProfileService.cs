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

        public async Task<DriverProfileResponseDto> GetDriverProfileById(Guid id)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetByIdAsync(
                id,
                includes: query => query
                    .Include(dp => dp.User)          
         .Include(dp => dp.User.Addresses) 
            );

            if (driverProfile == null)
                return null;

            var result = new DriverProfileResponseDto
            {
                Id = driverProfile.Id,
                UserId = driverProfile.UserId,
                FirstName = driverProfile.User.FirstName,
                LastName = driverProfile.User.LastName,
                ProfileImageUrl = driverProfile.profileImageUrl,
                IdNumber = driverProfile.idNumber,
                Rating = driverProfile.Rating,
                RatingCount = driverProfile.ratingCount,
                IsAvailable = driverProfile.IsAvailable,
                TotalTrips = driverProfile.TotalTrips,
                CreatedAt = driverProfile.CreatedAt,
                phonenumber=driverProfile.User.PhoneNumber,
                Email=driverProfile.User.Email,
                Address = driverProfile.User.Addresses?.FirstOrDefault() != null
                    ? new AddressDto
                    {
                        Street = driverProfile.User.Addresses.First().Street,
                        City = driverProfile.User.Addresses.First().City,
                        Governorate = driverProfile.User.Addresses.First().Governorate,
                        PostalCode = driverProfile.User.Addresses.First().PostalCode
                    }
                    : null
            };

            return result;
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
         .Include(dp => dp.User.Addresses) 
 );


            // Map إلى DTO
            var result = driverProfiles.Select(dp => new DriverProfileResponseDto
            {
                Id = dp.Id,
                UserId = dp.UserId,
                FirstName = dp.User.FirstName,
                LastName = dp.User.LastName,
                Email = dp.User.Email!,
                ProfileImageUrl = dp.profileImageUrl,
                IdNumber = dp.idNumber,
                Rating = dp.Rating,
                RatingCount = dp.ratingCount,
                IsAvailable = dp.IsAvailable,
                TotalTrips = dp.TotalTrips,
                CreatedAt = dp.CreatedAt,
                phonenumber = dp.User.PhoneNumber,



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
        public async Task<bool> DeleteDriverProfile(Guid id)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetByIdAsync(id);
            if (driverProfile == null)
            {
                return false;
            }
            await _unitOfWork.DriverProfiles.RemoveAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<DriverProfileResponseDto> UpdateDriverProfile(Guid userId, UpdateDriverProfileDto updateDto)
        {
            // 1. get Driver Profile
            var driverProfile = await _unitOfWork.DriverProfiles.GetByIdAsync(
                userId,
                includes: query => query
                    .Include(dp => dp.User)
                    .Include(dp => dp.User.Addresses)
            );

            if (driverProfile == null)
                return null;

            // 2. Update  User
            driverProfile.User.FirstName = updateDto.FirstName;
            driverProfile.User.LastName = updateDto.LastName;
            driverProfile.User.PhoneNumber = updateDto.PhoneNumber;
            driverProfile.User.Email = updateDto.Email;

            // 3. Update  Driver Profile
            driverProfile.profileImageUrl = updateDto.ProfileImageUrl ?? driverProfile.profileImageUrl;

            // 4. Update الـ Address
            if (updateDto.Address != null)
            {
                var existingAddress = driverProfile.User.Addresses?.FirstOrDefault();

                if (existingAddress != null)
                {
                    // Address ، update 
                    existingAddress.Street = updateDto.Address.Street;
                    existingAddress.City = updateDto.Address.City;
                    existingAddress.Governorate = updateDto.Address.Governorate;
                    existingAddress.PostalCode = updateDto.Address.PostalCode;
                }
               
            }

            // 5. Save 
            await _unitOfWork.DriverProfiles.UpdateAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();

            // 6. Return  Updated Profile
            return await GetDriverProfileById(driverProfile.Id);
        }

    }
}
