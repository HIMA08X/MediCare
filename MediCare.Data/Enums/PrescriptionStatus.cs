using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Enums
{
    public enum PrescriptionStatus : byte
    {
        Active = 0,
        Superseded = 1,
        Cancelled = 2
    
    }
}
