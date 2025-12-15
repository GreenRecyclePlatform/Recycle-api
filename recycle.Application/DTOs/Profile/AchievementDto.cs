using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Profile
{
    public class AchievementDto
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? EarnedDate { get; set; }
        public bool Unlocked { get; set; }
    }
}
