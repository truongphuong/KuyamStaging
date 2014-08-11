using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class CompanyHourDTO
    {       
        public int CompanyHourID { get; set; }
        public TimeSpan FromHour { get; set; }
        public TimeSpan ToHour { get; set; }
        public bool? IsDaily { get; set; }
        public int? ProfileCompanyID { get; set; }
        public int DayOfWeek { get; set; }
    }
}