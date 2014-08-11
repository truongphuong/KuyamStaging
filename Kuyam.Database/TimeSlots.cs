using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuyam.Database
{
    public class TimeSlots
    {
        public int NumberTimeSlots
        {
            get
            {
                if (IsClass)
                {
                    return 1;
                }
                else
                {
                    return 3;
                }
            }

        }

        public TimeSlots()
        {
            CompanyTimeSlots = new List<TimeSlot>();
            DayAvaiable = "today";
        }

        public List<TimeSlot> CompanyTimeSlots { get; set; }
        public bool IsShowMore { get; set; }
        public int CompanyProfileId { get; set; }
        public int CompanyTypeID { get; set; }
        public bool IsAvailableToday { get; private set; }
        public string DayAvaiable { get; set; }
        public List<CompanyHour> CompanyHours { get; set; }
        public List<ServiceTimeDTO> CompanyGenreralTimes { get; set; }
        public bool IsRederect { get; set; }

        public bool IsClass { get; set; }

        public void SetTimeSlots(List<TimeSlot> timeSlots, DateTime currentTime)
        {
            if (timeSlots != null && timeSlots.Any())
            {
                var groupByDate = timeSlots.GroupBy(t => t.StartTime.Date).FirstOrDefault();
                if (groupByDate != null)
                {
                    if (groupByDate.Key == currentTime.Date)
                    {
                        DayAvaiable = "available today";
                        IsAvailableToday = true;
                    }
                    else if (groupByDate.Key == currentTime.Date.AddDays(1))
                    {
                        DayAvaiable = "available " + groupByDate.Key.ToString("ddd, MMM dd");// "available tomorrow";
                    }
                    else
                    {
                        DayAvaiable = "available " + groupByDate.Key.ToString("ddd, MMM dd");
                    }

                    if (groupByDate.Count() > NumberTimeSlots)
                    {
                        CompanyTimeSlots = groupByDate.Take(NumberTimeSlots).ToList();
                        IsShowMore = true;
                    }
                    else
                    {
                        CompanyTimeSlots = groupByDate.ToList();
                    }
                }
            }
        }

        public void SetCompanyHours(List<CompanyHour> companyHours, DateTime ofDate, DateTime starTime)
        {
            if (companyHours != null && companyHours.Any())
            {
                if (ofDate.Date == starTime.Date)
                {
                    DayAvaiable = "today's hours";
                    IsAvailableToday = true;
                }
                else if (ofDate.Date == starTime.Date.AddDays(1))
                {
                    DayAvaiable = "tomorrow's hours";
                }
                else
                {
                    DayAvaiable = ofDate.ToString("ddd, MMM dd") + " hrs";
                }

                CompanyHours = companyHours;
            }
        }

        public void SetCompanyGenreralTimes(List<ServiceTimeDTO> companyGenreralHours, DateTime ofDate, DateTime starTime)
        {
            if (companyGenreralHours != null && companyGenreralHours.Any())
            {
                if (ofDate.Date == starTime.Date)
                {
                    DayAvaiable = "today's hours";
                    IsAvailableToday = true;
                }
                else if (ofDate.Date == starTime.Date.AddDays(1))
                {
                    DayAvaiable = "tomorrow's hours";
                }
                else
                {
                    DayAvaiable = ofDate.ToString("ddd, MMM dd") + " hrs";
                }

                CompanyGenreralTimes = companyGenreralHours;
            }
        }
    }
}
