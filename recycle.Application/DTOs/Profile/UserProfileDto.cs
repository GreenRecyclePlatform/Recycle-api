using recycle.Application.DTOs.Notifications;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Profile
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }

        public Address? PrimaryAddress { get; set; }
        public ProfileStatsDto Stats { get; set; }
        public EnvironmentalImpactDto EnvironmentalImpact { get; set; }
        public NotificationPreferencesDto NotificationPreferences { get; set; }
        public List<AchievementDto> Achievements { get; internal set; }
    }
}
