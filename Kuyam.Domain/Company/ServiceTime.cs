using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Domain.Company
{
    public class ServiceTime
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ServiceCompanyId { get; set; }
        public TimeSpan FromHour { get; set; }
        public TimeSpan ToHour { get; set; }
        public DateTime FromDateTime { get; set; }
        public DateTime ToDateTime { get; set; }
        public int DateOfWeek { get; set; }

        public ServiceTime UpdateDateTime(DateTime date)
        {
            if (DateOfWeek == (int) date.DayOfWeek)
            {
                var result = new ServiceTime
                {
                    EmployeeId = this.EmployeeId,
                    ServiceCompanyId = this.ServiceCompanyId,
                    FromHour = this.FromHour,
                    ToHour = this.ToHour,
                    DateOfWeek = this.DateOfWeek
                };
                result.FromDateTime = date.Date.AddTicks(FromHour.Ticks);
                result.ToDateTime = date.Date.AddTicks(ToHour.Ticks);
                return result;
            }
            return null;
        }
    }
}
