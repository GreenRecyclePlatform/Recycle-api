using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recycle.Domain
{
    public class Review
    {
        public Guid ReviewId { get; set; } = Guid.NewGuid();

        public Guid RequestId { get; set; }

        public string ReviewerId { get; set; }

        public string RevieweeId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; } = false;

        public bool IsFlagged { get; set; } = false;

        public string FlagReason { get; set; }

        public DateTime? FlaggedAt { get; set; }

        public bool IsHidden { get; set; } = false;

        // Navigation Properties
        public PickupRequest PickupRequest { get; set; }

        public ApplicationUser Reviewer { get; set; }

        public ApplicationUser Reviewee { get; set; }
    }
}
