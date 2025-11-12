using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recycle.Domain
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int ReviewerId { get; set; }

        [Required]
        public int RevieweeId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; } = false;

        public bool IsFlagged { get; set; } = false;

        [MaxLength(200)]
        public string FlagReason { get; set; }

        public DateTime? FlaggedAt { get; set; }

        public bool IsHidden { get; set; } = false;

        //Navigation Properties
       [ForeignKey("RequestId")]
        public  PickupRequest PickupRequest { get; set; }

        [ForeignKey("ReviewerId")]
        public ApplicationUser Reviewer { get; set; }

        [ForeignKey("RevieweeId")]
        public ApplicationUser Reviewee { get; set; }
    }
}