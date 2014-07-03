using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.AppointmentModel
{
    public enum CompanyTimeSlotStatus
    {
        UnAvailable = 0,
        Available = 1,
        Pending = 2,
        Busy = 3
    }

    public class CompanyTimeSlot
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Status { get; set; }

        public int EmployeeAvailableId { get; set; }
    }
}
