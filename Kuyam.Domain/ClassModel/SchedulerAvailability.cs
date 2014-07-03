using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.ClassModel
{
    public class SchedulerAvailability
    {
        public int ServiceCompanyID { get; set; }
        public int ProfileID { get; set; }
        public int ServiceID { get; set; }
        public int ClassSchedulerID { get; set; }
        public DateTime? FromDateTime { get; set; }
        public DateTime? ToDateTime { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int AttendeesNumber { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }
        public bool IsPerDay { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan FromHour { get; set; }
        public TimeSpan ToHour { get; set; }
        public int? EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int? PaymentMethod { get; set; }
        public bool IsAvailability { get; set; }
    }
}
