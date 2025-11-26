using Microsoft.AspNetCore.Http;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<DriverProfile> GetDriverProfileById(Guid Id)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(dp => dp.Id == Id);
            return driverProfile;
        }

        public async Task<DriverProfile> UpdateDriverProfileImage(UpdateDriverProfileImageDto imageDto,Guid userId)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(p=>p.UserId == userId);

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

        public async Task<List<DriverProfile>> GetDriverProfiles()
        {
            var driverProfiles = await _unitOfWork.DriverProfiles.GetAll();
            return driverProfiles.ToList();
        }

        public async Task<bool> DeleteDriverProfile(Guid userId)
        {
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(dp => dp.UserId == userId);
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
    }
}
