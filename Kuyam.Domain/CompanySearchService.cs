using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Kuyam.Database;
using Kuyam.Domain.AppointmentModel;
using Kuyam.Domain.Company;
using Kuyam.Utility;

namespace Kuyam.Domain
{
    public static class CompanySearchService
    {
        public static List<CompanyProfileSearch> GetCompaniesFromTypeIDWithDistance(int custID, int serviceId, double distance, decimal priceFrom, decimal priceTo, DateTime hourFrom, DateTime hourTo, bool isToday, int page, int sortBy, out int totalRecord, string key)
        {
            var take = 10;
            var skip = (page - 1)*take;
            
            var totalItems = 0;
            var curLat = ConfigManager.DefaultLatitude;
            var curLong = ConfigManager.Defaultlongitude;
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                Cust cust = DAL.xGetCust(HttpContext.Current.User.Identity.Name);
                curLat = cust.Latitude;
                curLong = cust.Longitude;
            }

            var pcList = CompanySearchDAL.GetProfileCompaniesWebSite(serviceId, priceFrom, priceTo, hourFrom, hourTo,
                    isToday, key, curLat, curLong, distance, custID, skip, take, out totalItems);
            totalRecord = totalItems;

            foreach (var companyProfileSearch in pcList)
            {
                TransformEmployeeHours(companyProfileSearch);
                TransformInstructorClassSchedulerHours(companyProfileSearch);
                TransformCompanyHours(companyProfileSearch);
                TransformEvents(companyProfileSearch);
                companyProfileSearch.CompanyAvailableTimeSlots = GetCompanyAvailableTimeSlots(companyProfileSearch);
            }

            return pcList;
        }

        private static void TransformEmployeeHours(CompanyProfileSearch companyProfile)
        {
            var employeeHoursXml = UtiHelper.Deserialize<EmployeeHoursXml>("<EmployeeHoursXml>" + companyProfile.EmployeeHoursStr + "</EmployeeHoursXml>");
            companyProfile.EmployeeHours = employeeHoursXml != null
                ? employeeHoursXml.EmployeeHours.Select(o => new EmployeeHour
                {
                    DayOfWeek = o.DayOfWeek,
                    FromHour = o.FromHour,
                    IsPreview = o.IsPreview,
                    ToHour = o.ToHour,
                    CompanyEmployee = new CompanyEmployee { EmployeeID = o.EmployeeID, EmployeeName = o.EmployeeName }
                }).ToList()
                : new List<EmployeeHour>();
        }

        private static void TransformInstructorClassSchedulerHours(CompanyProfileSearch companyProfile)
        {
            var employeeHoursXml = UtiHelper.Deserialize<EmployeeHoursXml>("<EmployeeHoursXml>" + companyProfile.InstructorClassSchedulerStr + "</EmployeeHoursXml>");
            companyProfile.InstructorClassSchedulerHours = employeeHoursXml != null
                ? employeeHoursXml.EmployeeHours.Select(o => new EmployeeHour
                {
                    ID = o.ID,
                    DayOfWeek = o.DayOfWeek,
                    FromHour = o.FromHour,
                    IsPreview = o.IsPreview,
                    ToHour = o.ToHour,
                    CompanyEmployee = new CompanyEmployee { EmployeeID = o.EmployeeID, EmployeeName = o.EmployeeName }
                }).ToList()
                : new List<EmployeeHour>();
        }

        private static void TransformCompanyHours(CompanyProfileSearch companyProfile)
        {
            var companyHoursXml = UtiHelper.Deserialize<CompanyHoursXml>("<CompanyHoursXml>" + companyProfile.CompanyHoursStr + "</CompanyHoursXml>");
            companyProfile.CompanyHours = companyHoursXml != null
                    ? companyHoursXml.CompanyHours.Select(o => new CompanyHour
                    {
                        DayOfWeek = o.DayOfWeek,
                        FromHour = o.FromHour,
                        IsDaily = o.IsDaily,
                        ToHour = o.ToHour
                    }).ToList()
                    : new List<CompanyHour>();
        }

        private static void TransformEvents(CompanyProfileSearch companyProfile)
        {
            var eventsXML = UtiHelper.Deserialize<EventsXml>("<EventsXml>" + companyProfile.CompanyEventsStr + "</EventsXml>");
            companyProfile.CompanyEvents = eventsXML != null
                ? eventsXML.Events.Select(o => new EventDTO
                {
                    EventID = o.EventID,
                    Name = o.Name,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    CompanyEventID = o.CompanyEventID
                }).ToList()
                : new List<EventDTO>();
        }

        private static TimeSlots GetCompanyAvailableTimeSlots(CompanyProfileSearch profileCompany)
        {
            if (profileCompany == null)
                return null;

            var startTime = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            var result = new TimeSlots() { CompanyProfileId = profileCompany.ProfileID };
            result.ProfileCompany = profileCompany;

            result.IsClass = profileCompany.IsClass.HasValue ? profileCompany.IsClass.Value : false;

            var companyhours = profileCompany.CompanyHours;
            if (companyhours.Any())
            {
                for (int i = 0; i < 7; i++)
                {
                    var date = startTime.Date.AddDays(i);
                    var hoursOfDate = companyhours.Where(c =>
                        (c.IsDaily != null && c.IsDaily.Value) ||
                        c.DayOfWeek.ToString().Contains(((int)date.DayOfWeek).ToString())).Take(2)
                        .ToList();

                    if (hoursOfDate.Any())
                    {
                        result.SetCompanyHours(hoursOfDate, date, startTime);
                        break;
                    }
                }
            }

            if (profileCompany.CompanyTypeID != (int)Types.CompanyType.NonKuyamBookIt
                && profileCompany.CompanyTypeID != (int)Types.CompanyType.GeneralAvailability)
            {
                if (startTime.Minute > 30)
                    startTime = startTime.AddMinutes(60 - startTime.Minute);
                else
                    startTime = startTime.AddMinutes(30 - startTime.Minute);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour,
                    startTime.Minute, 0);
                var endTime = startTime.AddDays(8).Date;
                var companyTimeslots = GetTimeSlots(profileCompany, startTime, endTime,
                    TimeSlots.NumberTimeSlots + 1);

                result.SetTimeSlots(companyTimeslots, startTime);
            }

            return result;
        }

        private static List<TimeSlot> GetTimeSlots(CompanyProfileSearch profileCompany, DateTime startTime, DateTime endTime, int take = 0, int timeSlot = 30)
        {
            List<TimeSlot> results = new List<TimeSlot>();

            var profileId = profileCompany.ProfileID;
            var isClass = profileCompany.IsClass.HasValue ? profileCompany.IsClass.Value : false;
            var companyAppointments = CompanySearchDAL.GetAppoinmentsByProfileId(profileId, startTime, endTime);

            var serviceHours = GetCompanyServiceTimes(profileCompany, startTime).ToList();
            for (DateTime time = startTime; time < endTime; time = time.AddMinutes(timeSlot))
            {
                var end = time.AddMinutes(timeSlot);
                var employeeAvaiable = 0;
                var serviceName = string.Empty;
                int status =
                    (int)GetCompanyTimeslotStatus(time, end, companyAppointments, serviceHours, out employeeAvaiable, out serviceName);
               
                if (status == (int)CompanyTimeSlotStatus.Available)
                    results.Add(new TimeSlot()
                    {
                        Title = time.ToString("hh:mmtt") + (isClass ? " " + UtilityHelper.TruncateAtWord(serviceName, 12) : string.Empty),
                        Time = time.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = status,
                        StartTime = time,
                        EndTime = end,
                        EmployeeAvailableId = employeeAvaiable
                    });
                if (take > 0 && results.Count == take)
                    break;
            }
            return results;
        }

        private static CompanyTimeSlotStatus GetCompanyTimeslotStatus(DateTime start, DateTime end, List<Appointment> companyAppointments, List<ServiceTime> companyServiceHours, out int employeeAvaiable, out string serviceName)
        {
            employeeAvaiable = 0;
            serviceName = string.Empty;
            if (companyAppointments.Any(
                    x =>
                    ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                     (start < x.End && x.End <= end)) &&
                    x.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed))
            {
                return CompanyTimeSlotStatus.Busy;
            }

            if (companyAppointments.Any(
                    x =>
                    ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                     (start < x.End && x.End <= end)) &&
                    (x.AppointmentStatusID == (int)Types.AppointmentStatus.Pending ||
                     x.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending ||
                     x.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified ||
                     x.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)))
            {
                return CompanyTimeSlotStatus.Pending;
            }


            var endTime = end.TimeOfDay;
            if (endTime.Hours == 0 && endTime.Minutes == 0 && endTime.Seconds == 0)
                endTime = end.AddTicks(-1).TimeOfDay;

            var companyServiceHoursAvailable = companyServiceHours.Where(
                x => (x.DateOfWeek == (int)start.DayOfWeek) &&
                    ((x.FromHour < start.TimeOfDay && x.ToHour > endTime) ||
                     (start.TimeOfDay <= x.FromHour && x.FromHour < endTime) ||
                     (start.TimeOfDay < x.ToHour && x.ToHour <= endTime))).ToList();
            if (companyServiceHoursAvailable.Any())
            {
                if (
                    !companyAppointments.Any(
                        x =>
                        ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                         (start < x.End && x.End <= end)) &&
                        x.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled &&
                        x.AppointmentStatusID != (int)Types.AppointmentStatus.Delete &&
                        x.AppointmentStatusID != (int)Types.AppointmentStatus.Unknown))
                {
                    employeeAvaiable = companyServiceHoursAvailable.First().EmployeeId;
                    serviceName = DAL.GetServiceNameFromServiceCompanyId(companyServiceHoursAvailable.First().ServiceCompanyId);
                    return CompanyTimeSlotStatus.Available;
                }

                var appointmentExits = companyAppointments.Where(x => ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                                                    (start < x.End && x.End <= end))).ToList();

                if (companyServiceHoursAvailable.Count > appointmentExits.Count())
                {
                    var employeeBusy = appointmentExits.Select(a => a.EmployeeID).Distinct();
                    var serviceAvailable = companyServiceHoursAvailable.FirstOrDefault(s => !employeeBusy.Contains(s.EmployeeId));
                    employeeAvaiable = serviceAvailable != null ? serviceAvailable.EmployeeId : 0;
                    return CompanyTimeSlotStatus.Available;
                }
            }

            return CompanyTimeSlotStatus.UnAvailable;
        }

        private static List<ServiceTime> GetCompanyServiceTimes(CompanyProfileSearch profileCompany, DateTime dtnow)
        {
            var results = new List<ServiceTime>();
            var lstEmployeeHour = new List<ServiceTime>();
            if (profileCompany.IsClass.HasValue && profileCompany.IsClass.Value)
            {
                lstEmployeeHour = profileCompany.InstructorClassSchedulerHours
               .Select(d => new ServiceTime()
               {
                   EmployeeId = d.CompanyEmployeeID,
                   DateOfWeek = d.DayOfWeek,
                   FromHour = d.FromHour,
                   ToHour = d.ToHour,
                   ServiceCompanyId = d.ID //  get serviceCompanyID in store procedure
               }).ToList();
            }
            else
            {
                lstEmployeeHour = profileCompany.EmployeeHours
               .Select(d => new ServiceTime()
               {
                   EmployeeId = d.CompanyEmployeeID,
                   DateOfWeek = d.DayOfWeek,
                   FromHour = d.FromHour,
                   ToHour = d.ToHour,
                   ServiceCompanyId = d.ID //  get serviceCompanyID in store procedure
               }).ToList();
            }
           


            int dayOfWeek = (int)dtnow.DayOfWeek;
            int detDay = 7 - dayOfWeek;
            int day = 0;

            var companyHours = profileCompany.CompanyHours;

            foreach (ServiceTime item in lstEmployeeHour)
            {
                DateTime dt = dtnow;

                if (item.DateOfWeek >= dayOfWeek)
                {
                    day = item.DateOfWeek - dayOfWeek;
                }
                else
                {
                    day = item.DateOfWeek + detDay;
                }

                dt = dt.AddDays(day);
                dt = dt.Date;

                var eventCustoms = GetIntersectWithCompanyHours(companyHours, dt, item.FromHour, item.ToHour);
                foreach (ServiceTime ec in eventCustoms)
                {
                    ec.EmployeeId = item.EmployeeId;
                    ec.ServiceCompanyId = item.ServiceCompanyId;
                    results.Add(ec);
                }

            }

            return results;
        }

        private static List<ServiceTime> GetIntersectWithCompanyHours(List<CompanyHour> companyHours, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            List<ServiceTime> results = new List<ServiceTime>();
            int dateOfWeek = (int)date.DayOfWeek;

            // Get company hours of day
            var companyHoursOfDay = companyHours.Where(h => h.DayOfWeek == dateOfWeek || h.IsDaily == true);

            while (true)
            {

                // get company hours intersec
                companyHoursOfDay = companyHoursOfDay.Where(h => h.ToHour > startTime && h.FromHour < endTime);

                // if not exist any company hour, return false
                if (companyHoursOfDay == null || companyHoursOfDay.Count() == 0)
                {
                    return results;
                }

                if (companyHoursOfDay.Any(h => h.FromHour <= startTime && h.ToHour >= endTime))
                {
                    results.Add(new ServiceTime
                    {
                        DateOfWeek = dateOfWeek,
                        FromDateTime = date.Date.AddMinutes(startTime.TotalMinutes),
                        ToDateTime = date.Date.AddMinutes(endTime.TotalMinutes),
                        FromHour = startTime,
                        ToHour = endTime
                    });
                    return results;
                }

                // get company hour cover start time
                var companyHourCoverStart = companyHoursOfDay.Where(h => h.FromHour <= startTime).OrderByDescending(h => h.ToHour).FirstOrDefault();

                // if not cover, truncate start time
                if (companyHourCoverStart == null)
                {
                    startTime = companyHoursOfDay.Where(h => h.FromHour > startTime).OrderBy(h => h.FromHour).First().FromHour;
                }
                // if cover, get time cover
                else
                {
                    var newEndTime = companyHourCoverStart.ToHour;
                    results.Add(new ServiceTime
                    {
                        DateOfWeek = dateOfWeek,
                        FromDateTime = date.Date.AddMinutes(startTime.TotalMinutes),
                        ToDateTime = date.Date.AddMinutes(newEndTime.TotalMinutes),
                        FromHour = startTime,
                        ToHour = newEndTime
                    });

                    startTime = newEndTime;
                }
            }
        }


    
    }
}
