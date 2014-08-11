using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class TimeSlotsDTO
    {
        public List<TimeSlot> CompanyTimeSlots { get; set; }
        public List<CompanyHourDTO> CompanyHours { get; set; }
        public bool IsShowMore { get; set; }        
        public int CompanyProfileId { get; set; }
        public int CompanyTypeID { get; set; }
        public bool IsAvailableToday { get; set; }
        public string DayAvaiable { get; set; }
        public bool IsRederect { get; set; }
        public bool IsClass { get; set; }
    }
}