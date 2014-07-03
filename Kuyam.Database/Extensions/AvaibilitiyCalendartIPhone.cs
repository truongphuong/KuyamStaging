using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class AvaibilitiyCalendartIPhone
    {
        public int TimeSlotNumber { get; set; }        
        public string Start { get; set; }
        public string End { get; set; }
        public int Status { get; set; }
        public int AppointmentID { get; set; }
        public List<AppointmentIPhone> Appointment { get; set; }
    }
}
