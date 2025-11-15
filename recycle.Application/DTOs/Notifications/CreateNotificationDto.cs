using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Notification type is required")]
        [MaxLength(50, ErrorMessage = "Notification type cannot exceed 50 characters")]
        public string NotificationType { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; }

        [MaxLength(50, ErrorMessage = "Entity type cannot exceed 50 characters")]
        public string RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        [MaxLength(20)]
        [RegularExpression("^(Low|Normal|High|Urgent)$", ErrorMessage = "Priority must be Low, Normal, High, or Urgent")]
        public string Priority { get; set; } = "Normal";
    }
}
