using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class MarkAsReadDto
    {
        [Required(ErrorMessage = "At least one notification ID is required")]
        [MinLength(1, ErrorMessage = "At least one notification ID is required")]
        public Guid[] NotificationIds { get; set; }
    }
}
