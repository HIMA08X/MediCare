using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Enums
{
    public enum DoctorApprovalStatus : byte
    {
        PendingApproval = 0,
        Approved = 1,
        Rejected = 2,
        Suspended = 3
    }
}
