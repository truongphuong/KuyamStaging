using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;
using Kuyam.Domain.AppointmentModel;
using Kuyam.Domain.Company;

namespace Kuyam.WebUI.Models.CompanyAppointment
{
    public class CompanyAvailableTimeSlots
    {
        public static int NumberTimeSlots = 3;

        public CompanyAvailableTimeSlots()
        {
            TimeSlots = new List<CompanyTimeSlot>();
            DayAvaiable = "today";
        }

        public List<CompanyTimeSlot> TimeSlots { get; set; } 
        public bool IsShowMore { get; set; }
        public int CompanyProfileId { get; set; }
        public bool IsAvailableToday { get; private set; }
        public string DayAvaiable { get; set; }
        public List<CompanyHour> CompanyHours { get; set; }
        public ProfileCompany ProfileCompany { get; set; }
        public List<ServiceTime> CompanyGenreralTimes { get; set; }
        public bool IsRederect { get; set; }
        public void SetTimeSlots(List<CompanyTimeSlot> timeSlots, DateTime currentTime)
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
                        DayAvaiable = "available tomorrow";
                    }
                    else
                    {
                        DayAvaiable = "available " + groupByDate.Key.ToString("ddd, MMM dd");
                    }

                    if (groupByDate.Count() > NumberTimeSlots)
                    {
                        TimeSlots = groupByDate.Take(NumberTimeSlots).ToList();
                        IsShowMore = true;
                    }
                    else
                    {
                        TimeSlots = groupByDate.ToList();
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

        public void SetCompanyGenreralTimes(List<ServiceTime> companyGenreralHours, DateTime ofDate, DateTime starTime)
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