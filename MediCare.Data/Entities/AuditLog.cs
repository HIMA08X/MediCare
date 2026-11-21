using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class AuditLog
    {
        public long Id { get; set; }
        public Guid ActorUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public Guid? PatientProfileId { get; set; }
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public ApplicationUser ActorUser { get; set; } = null!;
    }
}
