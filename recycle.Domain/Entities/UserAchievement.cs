using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    public class UserAchievement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid AchievementId { get; set; }
        public Achievement Achievement { get; set; }
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
