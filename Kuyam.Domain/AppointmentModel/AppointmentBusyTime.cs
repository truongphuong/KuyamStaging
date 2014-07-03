using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.AppointmentModel
{
    public class AppointmentBusyTime
    {
        public int EmployeeID { get; set; }

        public int DayOfWeek { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }     
       
    }
}
