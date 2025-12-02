// recycle.Application/Interfaces/IProfileService.cs
using recycle.Application.DTOs;
using recycle.Application.DTOs.Notifications;
using recycle.Application.DTOs.Profile;
using System;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<AddressDto> UpdateAddressAsync(Guid userId, UpdateAddressDto dto);
        Task<NotificationPreferencesDto> UpdateNotificationPreferencesAsync(Guid userId, NotificationPreferencesDto dto);

    }
}