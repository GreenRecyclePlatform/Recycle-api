using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recycle.Domain.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string NotificationType { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string RelatedEntityType { get; set; }

        public Guid? RelatedEntityId { get; set; }

        public bool IsRead { get; set; } = false;

        public string Priority { get; set; } = "Normal";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation Property
        public ApplicationUser User { get; set; }
    }


}
