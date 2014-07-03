using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database
{
    public class ServiceTimeDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ServiceCompanyId { get; set; }
        public TimeSpan FromHour { get; set; }
        public TimeSpan ToHour { get; set; }
        public DateTime FromDateTime { get; set; }
        public DateTime ToDateTime { get; set; }
        public int DateOfWeek { get; set; }

        public ServiceTimeDTO UpdateDateTime(DateTime date)
        {
            if (DateOfWeek == (int)date.DayOfWeek)
            {
                var result = new ServiceTimeDTO
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
