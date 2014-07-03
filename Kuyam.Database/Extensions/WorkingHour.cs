using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class WorkingHour
    {
        public int FromHour { get; set; }
        public int FromMinute { get; set; }

        public int ToHour { get; set; }
        public int ToMinute { get; set; }

        public string DayRange { get; set; }

        public int FirstDay { get; set; }
        public int LastDay { get; set; }
    }

    public class RangeEmployeeHour
    {
        public System.TimeSpan Start { get; set; }
        public System.TimeSpan End { get; set; }
    }
}
