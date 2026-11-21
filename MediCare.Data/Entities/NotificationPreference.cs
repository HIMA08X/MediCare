using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MediCare.Data.Entities
{
    public sealed class NotificationPreference
    {
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; }
        public bool InAppEnabled { get; set; } = true;
        public DateTime? SmsConsentAtUtc { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
