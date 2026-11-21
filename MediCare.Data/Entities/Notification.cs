using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Enums;

namespace MediCare.Data.Entities
{
    public sealed class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AppointmentId { get; set; }
        public NotificationChannel Channel { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public string DedupKey { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LockedUntilUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }
        public DateTime? ReadAtUtc { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }
}
