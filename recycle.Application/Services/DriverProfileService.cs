using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DriverProfileService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅✅✅ Method جديدة: جيب Driver Profile بالـ User ID ✅✅✅
        public async Task<DriverProfileResponseDto> GetDriverProfileByUserId(Guid userId)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                dp => dp.UserId == userId,
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
                phonenumber = driverProfile.User.PhoneNumber,
                Email = driverProfile.User.Email,
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

        // ✅ Method موجودة: جيب Driver Profile بالـ Profile ID (للاستخدام الداخلي)
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
                phonenumber = driverProfile.User.PhoneNumber,
                Email = driverProfile.User.Email,
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

        public async Task<DriverProfile> UpdateDriverProfileImage(UpdateDriverProfileImageDto imageDto, Guid userId)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(p => p.UserId == userId);

            if (imageDto.Image != null)
            {
                if (!string.IsNullOrEmpty(driverProfile.profileImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), driverProfile.profileImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(imageDto.Image.FileName);
                string imagepath = @"wwwroot\images\" + filename;
                var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), imagepath);

                using (var filestream = new FileStream(directoryLocation, FileMode.Create))
                {
                    await imageDto.Image.CopyToAsync(filestream);
                }

                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}";
                driverProfile.profileImageUrl = baseUrl + "/images/" + filename;
                driverProfile.profileImageLocalPath = imagepath;

                await _unitOfWork.DriverProfiles.UpdateAsync(driverProfile);
                await _unitOfWork.SaveChangesAsync();

                return driverProfile;
            }
            else
            {
                return driverProfile;
            }
        }

        public async Task<DriverProfile> CreateDriverProfile(DriverProfileDto driverProfileDto, Guid userId)
        {
            if (driverProfileDto.Image != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(driverProfileDto.Image.FileName);
                string imagepath = @"wwwroot\images\" + filename;
                var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), imagepath);

                FileInfo file = new FileInfo(directoryLocation);

                if (file.Exists)
                {
                    file.Delete();
                }
                using (var filestream = new FileStream(directoryLocation, FileMode.Create))
                {
                    await driverProfileDto.Image.CopyToAsync(filestream);
                }
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}";
                driverProfileDto.ImageUrl = baseUrl + "/images/" + filename;
                driverProfileDto.ImageLocalPath = imagepath;
            }

            var driverProfile = new DriverProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                profileImageUrl = driverProfileDto.ImageUrl,
                profileImageLocalPath = driverProfileDto.ImageLocalPath,
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
            if (!string.IsNullOrEmpty(driverProfile.profileImageLocalPath))
            {
                var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), driverProfile.profileImageLocalPath);
                FileInfo file = new FileInfo(oldFilePathDirectory);

                if (file.Exists)
                {
                    file.Delete();
                }
            }
            await _unitOfWork.DriverProfiles.RemoveAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ✅ Update Driver Profile بالـ Driver Profile ID
        public async Task<DriverProfileResponseDto> UpdateDriverProfile(Guid driverProfileId, UpdateDriverProfileDto updateDto)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetByIdAsync(
                driverProfileId,
                includes: query => query
                    .Include(dp => dp.User)
                    .Include(dp => dp.User.Addresses)
            );

            if (driverProfile == null)
                return null;

            driverProfile.User.FirstName = updateDto.FirstName;
            driverProfile.User.LastName = updateDto.LastName;
            driverProfile.User.PhoneNumber = updateDto.PhoneNumber;
            driverProfile.User.Email = updateDto.Email;

            driverProfile.profileImageUrl = updateDto.ProfileImageUrl ?? driverProfile.profileImageUrl;

            if (updateDto.Address != null)
            {
                var existingAddress = driverProfile.User.Addresses?.FirstOrDefault();

                if (existingAddress != null)
                {
                    existingAddress.Street = updateDto.Address.Street;
                    existingAddress.City = updateDto.Address.City;
                    existingAddress.Governorate = updateDto.Address.Governorate;
                    existingAddress.PostalCode = updateDto.Address.PostalCode;
                }
            }

            await _unitOfWork.DriverProfiles.UpdateAsync(driverProfile);
            await _unitOfWork.SaveChangesAsync();

            return await GetDriverProfileById(driverProfile.Id);
        }
    }
}