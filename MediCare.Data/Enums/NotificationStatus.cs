using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Enums
{
    public enum NotificationStatus : byte
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3,
        Cancelled = 4
    }
}
