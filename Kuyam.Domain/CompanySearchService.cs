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
using Kuyam.Repository.Interface;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;

namespace Kuyam.Domain
{
    public class CompanySearchService
    {
        #region Fields

        private readonly DbContext _dbContext;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<EmployeeService> _employeeServiceRepository;
        private readonly IRepository<InstructorClassScheduler> _instructorClassSchedulerRepository;

        #endregion

        #region Ctor

        public CompanySearchService(DbContext dbContext,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<Appointment> appointmentRepository,
            IRepository<InstructorClassScheduler> instructorClassSchedulerRepository,
            IRepository<EmployeeService> employeeServiceRepository
            )
        {
            this._dbContext = dbContext;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._appointmentRepository = appointmentRepository;
            this._instructorClassSchedulerRepository = instructorClassSchedulerRepository;
            this._employeeServiceRepository = employeeServiceRepository;


        }
        #endregion


        public List<CompanyProfileSearch> GetProfileCompaniesByEventId(int eventId, int categoryId, string cityName = "")
        {
            var data = _dbContext.SqlQuery<CompanyProfileSearch>("GetProfileCompaniesByEventID @EventID, @CategoryID, @CityName",
                new SqlParameter("EventID", eventId), new SqlParameter("CategoryID", categoryId), new SqlParameter("CityName", cityName));
            var result = data.ToList();

            var appointments = this.GetAppoinmentsByProfileIds(result.Select(a => a.ProfileID).ToList());

            foreach (var companyProfileSearch in result)
            {
                TransformEmployeeHours(companyProfileSearch);
                TransformInstructorClassSchedulerHours(companyProfileSearch);
                TransformCompanyHours(companyProfileSearch);
                TransformEvents(companyProfileSearch);
                companyProfileSearch.Appointments = appointments.Where(m => m.ProfileId == companyProfileSearch.ProfileID).ToList();
                companyProfileSearch.CompanyAvailableTimeSlots = GetCompanyAvailableTimeSlots(companyProfileSearch);
            }
            return result;
        }

        public List<CompanyProfileSearch> GetProfileCompaniesWebSite(int serviceID, decimal? fromPrice, decimal? toPrice
            , DateTime? fromDate, DateTime? toDate, bool isToday, string keySearch, double currentLat, double currentLong
            , double distance, int custID, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                if (keySearch == null) keySearch = string.Empty;

                var totalItemParam = new SqlParameter("TotalItems", 0);
                totalItemParam.Direction = ParameterDirection.Output;
                var data = _dbContext.SqlQuery<CompanyProfileSearch>(
                    "GetProfileCompaniesWebSite @ServiceID, @FromPrice, @ToPrice, @FromDate, @ToDate, @IsToDay, @KeySearch, @CurrentLat, @CurrentLong, @Distance, @CustID, @Skip, @Take, @TotalItems out",
                    new SqlParameter("ServiceID", serviceID),
                    new SqlParameter("FromPrice", fromPrice),
                    new SqlParameter("ToPrice", fromPrice),
                    new SqlParameter("FromDate", fromDate),
                    new SqlParameter("ToDate", toDate),
                    new SqlParameter("IsToday", isToday),
                    new SqlParameter("KeySearch", keySearch),
                    new SqlParameter("CurrentLat", Convert.ToSingle(currentLat)),
                    new SqlParameter("CurrentLong", Convert.ToSingle(currentLong)),
                    new SqlParameter("Distance", Convert.ToSingle(distance)),
                    new SqlParameter("CustID", custID),
                    new SqlParameter("Skip", skip),
                    new SqlParameter("Take", take),
                    totalItemParam
                    );

                var result = data.ToList();
                totalItems = Convert.ToInt32(totalItemParam.Value);

                return result;
            }
            catch (Exception ex)
            {
                return new List<CompanyProfileSearch>();
            }
        }

        public List<CompanyProfileSearch> GetCompaniesFromTypeIDWithDistance(Cust cust, int serviceId, double distance, decimal priceFrom, decimal priceTo, DateTime hourFrom, DateTime hourTo, bool isToday, int page, int sortBy, out int totalRecord, string key)
        {
            var take = 10;
            var skip = (page - 1) * take;

            var totalItems = 0;
            var curLat = ConfigManager.DefaultLatitude;
            var curLong = ConfigManager.Defaultlongitude;
            int custID = 0;
            if (cust != null)
            {
                custID = cust.CustID;
                curLat = cust.Latitude;
                curLong = cust.Longitude;
            }

            var pcList = this.GetProfileCompaniesWebSite(serviceId, priceFrom, priceTo, hourFrom, hourTo,
                    isToday, key, curLat, curLong, distance, custID, skip, take, out totalItems);

            totalRecord = totalItems;

            var appointments = this.GetAppoinmentsByProfileIds(pcList.Select(a => a.ProfileID).ToList());

            foreach (var companyProfileSearch in pcList)
            {
                TransformEmployeeHours(companyProfileSearch);
                TransformInstructorClassSchedulerHours(companyProfileSearch);
                TransformCompanyHours(companyProfileSearch);
                TransformEvents(companyProfileSearch);
                companyProfileSearch.Appointments = appointments.Where(m => m.ProfileId == companyProfileSearch.ProfileID).ToList();
                companyProfileSearch.CompanyAvailableTimeSlots = GetCompanyAvailableTimeSlots(companyProfileSearch);
            }

            return pcList;
        }

        private void TransformEmployeeHours(CompanyProfileSearch companyProfile)
        {
            var employeeHoursXml = UtiHelper.Deserialize<EmployeeHoursXml>("<EmployeeHoursXml>" + companyProfile.EmployeeHoursStr + "</EmployeeHoursXml>");
            companyProfile.EmployeeHours = employeeHoursXml != null
                ? employeeHoursXml.EmployeeHours.Select(o => new EmployeeHour
                {
                    ID = o.ID,
                    ServiceCompanyID = o.ServiceCompanyID,
                    DayOfWeek = o.DayOfWeek,
                    FromHour = o.FromHour,
                    IsPreview = o.IsPreview,
                    ToHour = o.ToHour,
                    CompanyEmployee = new CompanyEmployee { EmployeeID = o.EmployeeID, EmployeeName = o.EmployeeName }
                }).ToList()
                : new List<EmployeeHour>();
        }

        private void TransformInstructorClassSchedulerHours(CompanyProfileSearch companyProfile)
        {
            var employeeHoursXml = UtiHelper.Deserialize<EmployeeHoursXml>("<EmployeeHoursXml>" + companyProfile.InstructorClassSchedulerStr + "</EmployeeHoursXml>");
            companyProfile.InstructorClassSchedulerHours = employeeHoursXml != null
                ? employeeHoursXml.EmployeeHours.Select(o => new EmployeeHour
                {
                    ID = o.ID,
                    ServiceCompanyID = o.ServiceCompanyID,
                    ServiceName = o.ServiceName,
                    AttendeesNumber = o.AttendeesNumber,
                    DayOfWeek = o.DayOfWeek,
                    FromHour = o.FromHour,
                    IsPreview = o.IsPreview,
                    ToHour = o.ToHour,
                    CompanyEmployee = new CompanyEmployee { EmployeeID = o.EmployeeID, EmployeeName = o.EmployeeName },
                    StartDate = o.StartDate,
                    EndDate = o.EndDate
                }).ToList()
                : new List<EmployeeHour>();
        }

        private void TransformCompanyHours(CompanyProfileSearch companyProfile)
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

        private void TransformEvents(CompanyProfileSearch companyProfile)
        {
            var eventsXML = UtiHelper.Deserialize<EventsXml>("<EventsXml>" + companyProfile.CompanyEventsStr + "</EventsXml>");
            companyProfile.CompanyEvents = eventsXML != null
                ? eventsXML.Events.Select(o => new EventDTO
                {
                    EventID = o.EventID,
                    Name = o.Name,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    CompanyEventID = o.CompanyEventID,
                    CompanyServiceEventsNumber = o.CompanyServiceEventsNumber,
                    ClassEventsNumber = o.ClassEventsNumber
                }).ToList()
                : new List<EventDTO>();
        }

        private TimeSlots GetCompanyAvailableTimeSlots(CompanyProfileSearch profileCompany)
        {
            if (profileCompany == null)
                return null;

            var startTime = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            var result = new TimeSlots() { CompanyProfileId = profileCompany.ProfileID };

            result.IsClass = profileCompany.HasClassBooking;

            var companyhours = profileCompany.CompanyHours;
            if (companyhours.Any())
            {
                for (int i = 0; i < 7; i++)
                {
                    var date = startTime.Date.AddDays(i);
                    var hoursOfDate = companyhours.Where(c =>
                        (c.IsDaily != null && c.IsDaily.Value) ||
                        c.DayOfWeek == (int)date.DayOfWeek).Take(1)
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
                var numberofTimeSlot = 3;
                if (result.IsClass)
                    numberofTimeSlot = 1;
                var companyTimeslots = GetTimeSlots(profileCompany, startTime, endTime, numberofTimeSlot);

                result.SetTimeSlots(companyTimeslots, startTime);
            }

            return result;
        }

        private List<Appointment> GetAppoinmentsByProfileIds(List<int> profileIds)
        {
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            return _appointmentRepository.Table.Where(a => profileIds.Contains(a.ProfileId ?? 0)
                && a.Start >= dtNow
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Unknown
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && (a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= a.Created)
                ).ToList();
        }

        private List<TimeSlot> GetTimeSlots(CompanyProfileSearch profileCompany, DateTime startTime, DateTime endTime, int take = 0, int timeSlot = 30)
        {
            List<TimeSlot> results = new List<TimeSlot>();

            var profileId = profileCompany.ProfileID;

            var isClass = profileCompany.HasClassBooking;

            var companyAppointments = profileCompany.Appointments.Where(a => ((a.Start >= startTime && a.Start < endTime) || (a.End > startTime && a.End <= endTime))).ToList();

            var serviceHours = GetCompanyServiceTimes(profileCompany, startTime).ToList();

            for (DateTime time = startTime; time < endTime; time = time.AddMinutes(timeSlot))
            {
                var end = time.AddMinutes(timeSlot);
                var employeeAvaiableId = 0;
                var serviceName = string.Empty;
                int status = 0;
                int InstructorClassSchedulerId = 0;
                if (!isClass)
                {
                    status = (int)GetCompanyTimeslotStatus(time, end, companyAppointments, serviceHours, out employeeAvaiableId, out serviceName);
                }
                else
                {
                    status = (int)GetClassSchedulerslotStatus(time, end, companyAppointments, serviceHours, out employeeAvaiableId, out InstructorClassSchedulerId, out serviceName);
                }


                if (status == (int)CompanyTimeSlotStatus.Available)
                    results.Add(new TimeSlot()
                    {
                        Title = time.ToString("h:mmtt") + (isClass ? " " + UtilityHelper.TruncateAtWord(serviceName, 12) : string.Empty),
                        Time = time.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = status,
                        StartTime = time,
                        EndTime = end,
                        EmployeeAvailableId = employeeAvaiableId,
                        InstructorClassSchedulerId = InstructorClassSchedulerId
                    });

                if (results.Count > take)
                    break;
            }
            return results;
        }

        private CompanyTimeSlotStatus GetCompanyTimeslotStatus(DateTime start, DateTime end, List<Appointment> companyAppointments, List<ServiceTime> companyServiceHours, out int employeeAvaiable, out string serviceName)
        {
            employeeAvaiable = 0;
            serviceName = string.Empty;
            if (companyAppointments.Any(x =>
                    ((x.Start < start && x.End > end)
                    || (start <= x.Start && x.Start < end)
                    || (start < x.End && x.End <= end)
                    ) && x.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed))
            {

                return CompanyTimeSlotStatus.Busy;
            }

            if (companyAppointments.Any(x =>
                    ((x.Start < start && x.End > end)
                    || (start <= x.Start && x.Start < end)
                    || (start < x.End && x.End <= end)
                    ) && (x.AppointmentStatusID == (int)Types.AppointmentStatus.Pending ||
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
                    var employeeHour = companyServiceHoursAvailable.First();

                    employeeAvaiable = employeeHour.Id;
                    serviceName = employeeHour.ServiceName; //DAL.GetServiceNameFromServiceCompanyId(companyServiceHoursAvailable.First().ServiceCompanyId);
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

        private CompanyTimeSlotStatus GetClassSchedulerslotStatus(DateTime start, DateTime end, List<Appointment> companyAppointments, List<ServiceTime> companyServiceHours, out int instructorId, out int InstructorClassSchedulerId, out string serviceName)
        {
            instructorId = 0;
            InstructorClassSchedulerId = 0;
            serviceName = string.Empty;

            var endTime = end.TimeOfDay;
            if (endTime.Hours == 0 && endTime.Minutes == 0 && endTime.Seconds == 0)
                endTime = end.AddTicks(-1).TimeOfDay;

            var companyServiceHoursAvailable = companyServiceHours.Where(
                x => (x.DateOfWeek == (int)start.DayOfWeek) &&
                    ((x.FromHour < start.TimeOfDay && x.ToHour > endTime) ||
                     (start.TimeOfDay <= x.FromHour && x.FromHour < endTime) ||
                     (start.TimeOfDay < x.ToHour && x.ToHour <= endTime))).ToList();

            if (!companyServiceHoursAvailable.Any())
            {
                return CompanyTimeSlotStatus.UnAvailable;
            }

            foreach (var companyServiceHour in companyServiceHoursAvailable)
            {
                var numberAppoitment = companyAppointments.Where(x =>
                     ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                      (start < x.End && x.End <= end)) &&
                      (x.ClassSchedulerID.HasValue && x.ClassSchedulerID.Value == companyServiceHour.Id) &&
                     (x.AppointmentStatusID == (int)Types.AppointmentStatus.Pending ||
                      x.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending ||
                      x.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified ||
                      x.AppointmentStatusID == (int)Types.AppointmentStatus.Modified ||
                       x.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                      )).ToList().Count();

                var maxSlot = companyServiceHour.AttendeesNumber;


                //var sc = _serviceCompanyRepository.Table.Where(x => x.ServiceCompanyID == companyServiceHour.ServiceCompanyId).FirstOrDefault();

                //if (sc != null)
                //{
                //    maxSlot = sc.AttendeesNumber.HasValue ? sc.AttendeesNumber.Value : 0;
                //}

                if (numberAppoitment < maxSlot)
                {
                    instructorId = companyServiceHour.EmployeeId;
                    InstructorClassSchedulerId = companyServiceHour.Id;
                    serviceName = companyServiceHour.ServiceName;//DAL.GetServiceNameFromServiceCompanyId(companyServiceHour.ServiceCompanyId);
                    return CompanyTimeSlotStatus.Available;
                }
            }
            return CompanyTimeSlotStatus.UnAvailable;
        }

        private List<ServiceTime> GetCompanyServiceTimes(CompanyProfileSearch profileCompany, DateTime dtnow)
        {
            var results = new List<ServiceTime>();
            var lstEmployeeHour = new List<ServiceTime>();
            if (profileCompany.HasClassBooking)
            {
                lstEmployeeHour = profileCompany.InstructorClassSchedulerHours.Select(d => new ServiceTime()
                             {
                                 Id = d.ID,
                                 EmployeeId = d.CompanyEmployeeID,
                                 DateOfWeek = d.DayOfWeek,
                                 FromHour = d.FromHour,
                                 ToHour = d.ToHour,
                                 ServiceCompanyId = d.ServiceCompanyID,//  get serviceCompanyID in store procedure
                                 ServiceName = d.ServiceName,
                                 AttendeesNumber = d.AttendeesNumber,
                                 FromDateTime = d.StartDate.HasValue ? d.StartDate.Value : DateTime.MinValue,
                                 ToDateTime = d.EndDate.HasValue ? d.EndDate.Value : DateTime.MinValue
                             }).ToList();
            }
            else
            {
                lstEmployeeHour = profileCompany.EmployeeHours
               .Select(d => new ServiceTime()
               {
                   Id = d.ID,
                   EmployeeId = d.CompanyEmployeeID,
                   DateOfWeek = d.DayOfWeek,
                   FromHour = d.FromHour,
                   ToHour = d.ToHour,
                   FromDateTime = d.StartDate.HasValue ? d.StartDate.Value : DateTime.MinValue,
                   ToDateTime = d.EndDate.HasValue ? d.EndDate.Value : DateTime.MinValue
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

                if (item.FromDateTime > DateTime.MinValue && item.ToDateTime > DateTime.MinValue &&
                    (item.FromDateTime > dt || item.ToDateTime < dt))
                {
                    continue;
                }

                var eventCustoms = GetIntersectWithCompanyHours(companyHours, dt, item.FromHour, item.ToHour);
                foreach (ServiceTime ec in eventCustoms)
                {
                    ec.EmployeeId = item.EmployeeId;
                    ec.ServiceCompanyId = item.ServiceCompanyId;
                    ec.ServiceName = item.ServiceName;
                    ec.AttendeesNumber = item.AttendeesNumber;
                    ec.Id = item.Id;
                    results.Add(ec);
                }


            }

            return results;
        }

        private bool IsClassExpire(int serviceCompanyId, DateTime date)
        {
            var isExpire = false;
            isExpire = _serviceCompanyRepository.Table.Any(x => x.ServiceCompanyID == serviceCompanyId
                && x.ServiceTypeId == (int)Types.ServiceType.ClassType && (x.FromDateTime.Value > date || x.ToDateTime.Value < date));
            return isExpire;
        }

        private List<ServiceTime> GetIntersectWithCompanyHours(List<CompanyHour> companyHours, DateTime date, TimeSpan startTime, TimeSpan endTime)
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
