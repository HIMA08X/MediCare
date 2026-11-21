using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Enums
{
    public enum AppointmentStatus : byte
    {
        Pending = 0,
        Confirmed = 1,
        Rejected = 2,
        Cancelled = 3,
        Rescheduled = 4,
        Completed = 5,
        NoShow = 6
    }
}
