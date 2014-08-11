using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class EmployeeHourDTO
    {        
        public int ID { get; set; }
        public TimeSpan FromHour { get; set; }
        public TimeSpan ToHour { get; set; }
        public int CompanyEmployeeID { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsPreview { get; set; }
        public int ServiceCompanyID { get; set; }
        public string ServiceName { get; set; }
        public int AttendeesNumber { get; set; }
    }
}