using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Enums
{
    public enum ServiceError
    {
        None = 0,
        NotFound = 1,
        Forbidden = 2,
        ValidationFailed = 3,
        Conflict = 4
    }
}
