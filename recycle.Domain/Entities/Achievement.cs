using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    public class Achievement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; } // "Pickup", "Environmental", "Milestone"
        public int RequiredCount { get; set; } // e.g., 1 for "First Pickup", 100 for "100kg Recycled"
    }
}
