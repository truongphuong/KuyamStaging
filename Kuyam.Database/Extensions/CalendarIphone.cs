using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class CalendarIphone
    {
        public int CalendarID { get; set; }
        public Nullable<int> ProfileID { get; set; }
        public string Name { get; set; }
        public string BackColor { get; set; }
        public string ForeColor { get; set; }
        public bool IsDefault { get; set; }
        public int CalendarDisplayTypeID { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
    }
}
