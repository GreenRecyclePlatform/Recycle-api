using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class BulkNotificationDto
    {
        [Required(ErrorMessage = "User IDs are required")]
        [MinLength(1, ErrorMessage = "At least one user ID is required")]
        public Guid[] UserIds { get; set; }

        [Required(ErrorMessage = "Notification type is required")]
        [MaxLength(50)]
        public string NotificationType { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(500)]
        public string Message { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";
    }
}
