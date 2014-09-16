using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Domain.AppointmentModel;
using Kuyam.Domain.Company;
using Kuyam.Domain.Models;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using Kuyam.Repository.Infrastructure;
using Kuyam.Utility;
using System.Transactions;
using System.Data.Entity;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.ClassModel;
using System.Data.SqlClient;

namespace Kuyam.Domain
{
    public class AppointmentService : IAppointmentService
    {
        #region Fields

        private readonly DbContext _dbContext;
        private readonly CompanyProfileService _companyProfileService;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<AppointmentTemp> _appointmentTempRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<AppointmentLog> _appointmentLogRepository;
        private readonly IRepository<AppointmentNotify> _appointmentNotifyRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<EmployeeService> _employeeServiceRepository;
        private readonly IRepository<Rating> _ratingServiceRepository;
        private readonly IRepository<KuyamEvent> _kuyameventRepository;
        private readonly IRepository<AppointmentParticipant> _appointmentParticipantRepository;
        private readonly IRepository<Calendar> _calendarRepository;
        private readonly IRepository<CalendarShare> _calendarShareRepository;
        private readonly IRepository<EmployeeHour> _employeeHourRepository;
        private readonly IRepository<NonKuyamAppointment> _nonKuyamAppointmentRepository;
        private readonly IRepository<RequestAppointment> _requestAppointmentRepository;
        private readonly IRepository<ProposedAppointment> _proposedAppoinmentRepository;
        private readonly IRepository<InstructorClassScheduler> _instructorClassSchedulerRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IRepository<Cust> _custRepository;
        #endregion

        #region Ctor

        public AppointmentService(DbContext dbContext,
            CompanyProfileService companyProfileService,
            IRepository<Appointment> appointmentrRepository,
            IRepository<Service> serviceRepository,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<AppointmentLog> appointmentLogRepository,
            IRepository<CompanyEmployee> companyEmployeeRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<EmployeeService> employeeServiceRepository,
            IRepository<Rating> ratingServiceRepository,
            IRepository<KuyamEvent> kuyameventRepository,
            IRepository<AppointmentParticipant> appointmentParticipantRepository,
            IRepository<Calendar> calendarRepository,
            IRepository<CalendarShare> calendarShareRepository,
            IRepository<EmployeeHour> _employeeHourRepository,
            IRepository<AppointmentTemp> appointmentTempRepository,
            IRepository<AppointmentNotify> appointmentNotifyRepository,
            IRepository<NonKuyamAppointment> nonKuyamAppointmentRepository,
            IRepository<RequestAppointment> requestAppointmentRepository,
            IRepository<ProposedAppointment> proposedAppoinmentRepository,
            IRepository<InstructorClassScheduler> instructorClassSchedulerRepository,
            IRepository<PaymentMethod> paymentMethodRepository,
            IRepository<Cust> custRepository)
        {
            this._dbContext = dbContext;
            this._companyProfileService = companyProfileService;
            this._appointmentRepository = appointmentrRepository;
            this._serviceRepository = serviceRepository;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._appointmentLogRepository = appointmentLogRepository;
            this._companyEmployeeRepository = companyEmployeeRepository;
            this._profileCompanyRepository = profileCompanyRepository;
            this._employeeServiceRepository = employeeServiceRepository;
            this._ratingServiceRepository = ratingServiceRepository;
            this._kuyameventRepository = kuyameventRepository;
            this._appointmentParticipantRepository = appointmentParticipantRepository;
            this._calendarRepository = calendarRepository;
            this._calendarShareRepository = calendarShareRepository;
            this._employeeHourRepository = _employeeHourRepository;
            this._appointmentTempRepository = appointmentTempRepository;
            this._appointmentNotifyRepository = appointmentNotifyRepository;
            this._nonKuyamAppointmentRepository = nonKuyamAppointmentRepository;
            this._requestAppointmentRepository = requestAppointmentRepository;
            this._proposedAppoinmentRepository = proposedAppoinmentRepository;
            this._instructorClassSchedulerRepository = instructorClassSchedulerRepository;
            this._paymentMethodRepository = paymentMethodRepository;
            this._custRepository = custRepository;
        }

        #endregion


        public void AddAppointment(Appointment appointment)
        {
            appointment.Created = DateTime.UtcNow;
            appointment.Modified = DateTime.UtcNow;
            appointment.StatusChangeDate = DateTime.UtcNow;
            _appointmentRepository.Insert(appointment);
            AppointmentParticipant appointmentParticipant = new AppointmentParticipant
            {
                AppointmentID = appointment.AppointmentID,
                CalendarID = appointment.CalendarId,
                ParticipantStatusID = 0,
                ParticipantTypeID = 0
            };
            _appointmentParticipantRepository.Insert(appointmentParticipant);
        }
        public void InsertAppointment(Appointment appointment)
        {
            _appointmentRepository.Insert(appointment);
        }
        public void UpdateAppointment(Appointment appointment)
        {
            _appointmentRepository.Update(appointment);
        }

        public int UpdateAppointment(int appointmentID, int rating)
        {
            Appointment apt = _appointmentRepository.Table.First(x => x.AppointmentID == appointmentID);
            if (apt == null)
                return 0;
            apt.Rating = rating;
            apt.Modified = DateTime.UtcNow;
            _appointmentRepository.Update(apt);
            return apt.AppointmentStatusID;
        }
        public void DeleteAppointment(Appointment appointment)
        {
            _appointmentRepository.Delete(appointment);
        }

        public void InsertAppointmentTemp(AppointmentTemp appointmentTemp)
        {
            _appointmentTempRepository.Insert(appointmentTemp);
        }
        public void UpdateAppointmentTemp(AppointmentTemp appointmentTemp)
        {
            _appointmentTempRepository.Update(appointmentTemp);
        }
        public void DeleteAppointmentTemp(AppointmentTemp appointmentTemp)
        {
            _appointmentTempRepository.Delete(appointmentTemp);
        }
        public enum CalendarTimeSlotStatus
        {
            UnAvailable = 0,
            Available = 1,
            Pending = 2,
            Busy = 3,

        }

        public CalendarTimeSlotStatus GetCalendarTimeslotStatus(DateTime start, DateTime end, List<Appointment> calendarAppointments, List<Appointment> companyAppointments, List<ServiceTime> companyServiceHours)
        {

            if (companyAppointments.Any(
                    x =>
                    ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                     (start < x.End && x.End <= end)) &&
                    x.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed))
            {
                return CalendarTimeSlotStatus.Busy;
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
                return CalendarTimeSlotStatus.Pending;
            }


            var endTime = end.TimeOfDay;
            if (endTime.Hours == 0 && endTime.Minutes == 0 && endTime.Seconds == 0)
                endTime = end.AddTicks(-1).TimeOfDay;
            if (
                companyServiceHours.Any(
                    x =>
                    (x.DateOfWeek == (int)start.DayOfWeek) &&
                    ((x.FromHour < start.TimeOfDay && x.ToHour > endTime) ||
                     (start.TimeOfDay <= x.FromHour && x.FromHour < endTime) ||
                     (start.TimeOfDay < x.ToHour && x.ToHour <= endTime))))
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
                    return CalendarTimeSlotStatus.Available;
                }

                int sCount =
                    companyServiceHours.Count(x => (x.DateOfWeek == (int)start.DayOfWeek) &&
                                                   ((x.FromHour < start.TimeOfDay && x.ToHour > endTime) ||
                                                    (start.TimeOfDay <= x.FromHour && x.FromHour < endTime) ||
                                                    (start.TimeOfDay < x.ToHour && x.ToHour <= endTime)));
                int aCount =
                    companyAppointments.Count(x => ((x.Start < start && x.End > end) || (start <= x.Start && x.Start < end) ||
                                                    (start < x.End && x.End <= end)));
                if (sCount > aCount)
                {
                    return CalendarTimeSlotStatus.Available;
                }
            }

            return CalendarTimeSlotStatus.UnAvailable;
        }

        public List<Service> GetListService()
        {
            List<Service> list = _serviceRepository.Table.Where(m => m.ParentServiceID == null).OrderBy(m => m.ServiceName).ToList();
            return list;
        }

        public Appointment GetAppointmentByID(int appoimentID)
        {
            return _appointmentRepository.Table.Single(m => m.AppointmentID == appoimentID);
        }

        public Calendar GetCalendarIdById(int calendarId)
        {
            Calendar calendar = _calendarRepository.Table.FirstOrDefault(m => m.CalendarID == calendarId);
            return calendar;
        }

        public List<CompanyEmployee> GetCompanyEmployeeByCompanyID(int companyId)
        {
            var query = from cpne in _companyEmployeeRepository.Table
                        where cpne.ProfileCompanyID == companyId
                        select cpne;

            return query.ToList();
        }

        public List<AppointmentLog> GetAppointmentNoteByID(int aptId)
        {
            return _appointmentLogRepository.Table.Where(m => m.AppointmentID == aptId).ToList();
        }

        public List<ServiceCompany> GetListEmployeeServiceByEmployeeID(int companyEmployeeID)
        {

            var query = from eplsv in _employeeServiceRepository.Table
                        join svcp in _serviceCompanyRepository.Table on eplsv.ServiceCompanyID equals svcp.ServiceCompanyID
                        where eplsv.CompanyEmployeeID == companyEmployeeID
                        select svcp;

            return query.ToList();
        }

        public List<Appointment> GetListAppointmentByCustID(int custID)
        {
            if (custID == 0)
                return null;

            DateTime dtstart = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            DateTime dtEnd = dtstart.AddDays(7);
            dtEnd = dtEnd.Date.AddMinutes(59);

            var query = from aptp in _appointmentParticipantRepository.Table
                        join apt in _appointmentRepository.Table on aptp.AppointmentID equals apt.AppointmentID
                        join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID
                        where apt.CustID == custID
                              && apt.Start >= dtstart
                              && apt.Start <= dtEnd
                              && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                        orderby apt.Created ascending
                        select apt;
            var result = query.ToList();
            return result;
        }

        public List<Appointment> GetleftAppointmentByCustID(int custID)
        {
            if (custID == 0)
                return null;

            DateTime dtstart = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);

            var query = from aptp in _appointmentParticipantRepository.Table
                        join apt in _appointmentRepository.Table on aptp.AppointmentID equals apt.AppointmentID
                        join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID
                        where apt.CustID == custID
                              && apt.Start >= dtstart
                              && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Modified
                              || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                        orderby apt.Created ascending
                        select apt;
            var result = query.ToList();
            return result;
        }

        private Service GetRootService(int serviceId)
        {
            var service = _serviceRepository.Table.Where(t => t.ServiceID == serviceId).FirstOrDefault();
            if (service != null)
            {
                if (service.ParentServiceID.HasValue && service.ParentServiceID.Value != 0)
                {
                    return GetRootService(service.ParentServiceID.Value);
                }
                else
                {
                    return service;
                }
            }
            return null;
        }

        private List<int> GetChildrenService(int serviceId)
        {
            var service = _serviceRepository.Table.Where(t => t.ParentServiceID == serviceId);
            var result = new List<int>();
            if (service.Any())
            {
                var listServiceId = service.Select(t => t.ServiceID).ToList();
                foreach (var id in listServiceId)
                {
                    result.AddRange(GetChildrenService(id));
                }
            }
            result.Add(serviceId);
            return result;
        }

        public List<AppointmentParticipant> GetListAppointmentParticipantCustID(int custID, DateTime? createdDate, bool? isloadMore, int calendarId = 0, int status = 0, int seviceid = 0)
        {
            if (custID == 0)
                return null;

            List<int> listService = new List<int>();

            if (seviceid != 0)
            {
                listService = GetChildrenService(seviceid);
            }

            DateTime dtstart = createdDate.HasValue ? createdDate.Value.Date : DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            DateTime dtEnd = dtstart.AddDays(7);
            dtEnd = dtEnd.Date.AddMinutes(59);
            var query = from aptp in _appointmentParticipantRepository.Table
                        join apt in _appointmentRepository.Table on aptp.AppointmentID equals apt.AppointmentID
                        join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID
                        where apt.CustID == custID
                              && (calendarId == 0 || cals.CalendarID == calendarId)
                              && (status == 0 || apt.AppointmentStatusID == status)
                              && (apt.Start >= dtstart && apt.Start < dtEnd)
                              && (seviceid == 0 || (apt.ServiceCompany != null && listService.Contains(apt.ServiceCompany.ServiceID)))
                              && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                              && apt.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                        orderby apt.Start ascending
                        select aptp;
            List<AppointmentParticipant> result = null;
            if (isloadMore.HasValue && isloadMore.Value)
            {
                result = query.ToList();
            }
            else
            {
                result = query.ToList();
            }
            return result;
        }

        public List<AppointmentParticipant> GetListAppointmentParticipantCustID2(int custID, DateTime? startDate, bool? isloadMore, int calendarId = 0, int status = 0, int seviceid = 0)
        {
            if (custID == 0)
                return null;

            List<int> listService = new List<int>();
            if (seviceid != 0)
            {
                listService = GetChildrenService(seviceid);
            }

            DateTime dtstart = startDate.HasValue ? startDate.Value.Date : DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            DateTime dtEnd = dtstart.AddDays(7);
            dtEnd = dtEnd.Date.AddMinutes(59);
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);

            var query = from aptp in _appointmentParticipantRepository.Table
                        join apt in _appointmentRepository.Table on aptp.AppointmentID equals apt.AppointmentID
                        join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID
                        where apt.CustID == custID
                              && (calendarId == 0 || cals.CalendarID == calendarId)
                              && (status == 0 || apt.AppointmentStatusID == status)
                              && (apt.Start >= dtstart && apt.Start <= dtEnd)
                              && (seviceid == 0 || (apt.ServiceCompany != null && listService.Contains(apt.ServiceCompany.ServiceID)))
                              && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                                 || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
                                 || apt.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified
                                 || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                              && apt.Start <= dtNow
                        orderby apt.Created ascending
                        select aptp;
            List<AppointmentParticipant> result = null;
            if (isloadMore.HasValue && isloadMore.Value)
            {
                result = query.ToList();
            }
            else
            {
                result = query.Skip(0).Take(10).ToList();
            }
            return result;
        }

        public void RequestModification(Appointment appointment)
        {
            try
            {
                _appointmentRepository.Update(appointment);
            }
            catch (Exception ex)
            {
                LogHelper.Error("RequestModification:" + ex.Source.ToString());
            }

        }

        public void RequestModification(AppointmentParticipant appointmentPart)
        {

            KuyamEvent kuyamEvent = _kuyameventRepository.Table.FirstOrDefault(k => k.AppointmentID == appointmentPart.AppointmentID);
            if (kuyamEvent != null)
            {
                kuyamEvent.StartDate = appointmentPart.Appointment.Start;
                kuyamEvent.EndDate = appointmentPart.Appointment.End;
                _kuyameventRepository.Update(kuyamEvent);
            }
        }

        public void AddNote(AppointmentLog aptlg)
        {
            _appointmentLogRepository.Insert(aptlg);
        }

        public void AddRating(Rating rating)
        {
            _ratingServiceRepository.Insert(rating);
        }

        public void ViewedNote(List<AppointmentLog> aptlgs)
        {
            _appointmentLogRepository.Update(new AppointmentLog());
        }

        public Appointment ChangeStatus(int appointmentID, int AppointmentStatusID, string reason = "")
        {
            Appointment apt = _appointmentRepository.Table.First(x => x.AppointmentID == appointmentID);
            if (apt == null)
                return null;
            string subject = string.Empty;
            apt.AppointmentStatusID = AppointmentStatusID;
            apt.Desc = reason;
            apt.StatusChangeDate = DateTime.UtcNow;
            apt.Modified = DateTime.UtcNow;
            _appointmentRepository.Update(apt);
            return apt;
        }

        public List<Appointment> GetAppoinmentsByEmployeeId(int employeeId, int serviceId, DateTime startDate, DateTime endDate)
        {
            return _appointmentRepository.Table.Where(p => (employeeId <= 0 || p.EmployeeID == employeeId) &&
                (serviceId <= 0 || p.ServiceCompanyID == serviceId) &&
                ((p.Start >= startDate && p.End <= endDate) ||
                (p.Start < startDate) ||
                (p.End > endDate))
                ).ToList();
        }

        public List<Appointment> GetAppoinmentsByEmployeeId(int employeeId, DateTime startDate, DateTime endDate)
        {
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            return _appointmentRepository.Table.Where(p =>
                (employeeId <= 0 || p.EmployeeID == employeeId)
                && p.Start >= startDate && p.Start < endDate
                && p.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled // valid appointment does not have cancel , delete and temporary pending which have  the created time more than 10 mins 
                && p.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && (p.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= p.Created)
                ).ToList();
        }

        public List<AppointmentTemp> GetAppoinmentTempsByEmployeeId(int employeeId, int calendarId, DateTime startDate, DateTime endDate)
        {
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            return _appointmentTempRepository.Table.Where(p =>
                (employeeId == 0 || p.EmployeeID == employeeId)
                && p.CalendarId != calendarId
                && p.Start >= startDate
                && p.Start < endDate
                && (p.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending || temp <= p.Created)).ToList();
        }

        public List<AppointmentBusyTime> GetAppoinmentBusyTimByEmployeeId(int profileId)
        {
            var _class = from cmp in _companyEmployeeRepository.Table
                         join emps in _employeeServiceRepository.Table on cmp.EmployeeID equals emps.CompanyEmployeeID
                         join inst in _instructorClassSchedulerRepository.Table on emps.ID equals inst.InstructorClassID
                         where cmp.ProfileCompanyID == profileId
                         select new AppointmentBusyTime { EmployeeID = emps.CompanyEmployeeID, DayOfWeek = inst.DayOfWeek, Start = inst.FromHour, End = inst.ToHour };
            return _class.ToList();
        }

        public AppointmentTemp GetAppoinmentTempsById(int id)
        {
            return _appointmentTempRepository.Table.Where(p => p.AppointmentID == id).First();
        }

        public List<Appointment> GetAppoinmentsByEmployeeIdForIP(int employeeId, DateTime startDate, DateTime endDate)
        {
            //startDate = startDate.AddHours(-1);
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            return _appointmentRepository.Table.Where(p => (employeeId <= 0 || p.EmployeeID == employeeId) && ((p.Start >= startDate && p.Start < endDate) || (p.End > startDate && p.End <= endDate))
                && p.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled // valid appointment does not have cancel , delete and temporary pending which have  the created time more than 10 mins 
                && p.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && (p.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= p.Created)
                ).ToList();
        }

        /// <summary>
        /// Gets the appoinments by profile id.
        /// </summary>
        /// <param name="profileId">The profile id.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public List<Appointment> GetAppoinmentsByProfileId(int profileId, DateTime startTime, DateTime endTime)
        {
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            return _appointmentRepository.Table.Where(a => a.CompanyEmployee.ProfileCompanyID == profileId
                && ((a.Start >= startTime && a.Start < endTime) || (a.End > startTime && a.End <= endTime))
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Unknown
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled // valid appointment does not have cancel , delete and temporary pending which have  the created time more than 10 mins 
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && (a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= a.Created)
                ).ToList();
        }

        public List<Rating> GetRatingListByProfileID(int profileID, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            var result = _ratingServiceRepository.Table.Where(r => r.ServiceCompany.ProfileID == profileID).ToList();

            totalRecord = result.Count();

            return result.OrderByDescending(r => r.CreateDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// Get all ratings for a company.
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public List<Rating> GetRatingsByProfileId(int profileId)
        {
            return _ratingServiceRepository.Table.Where(t=>t.ServiceCompany.ProfileID == profileId).OrderByDescending(t=>t.CreateDate).ToList();
        }

        public List<RatingModel> GetRatingByCompanyProfile(int companyProfileId)
        {            
            var ratings = from r in _ratingServiceRepository.Table
                join c in _custRepository.Table
                    on r.CustID equals c.CustID
                select new {Customer = c, Rating = r};
            var ratingModels = new List<RatingModel>();
            foreach (var rating in ratings)
            {
                var ratingModel = new RatingModel(rating.Rating)
                {
                    Customer = new CustomerModel(rating.Customer)
                };
                ratingModels.Add(ratingModel);
            }            
            return ratingModels;
        }


        public List<Appointment> GetAppoinmentsByCustId(int custID, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.AddHours(-1);
            return _appointmentRepository.Table.Where(p => (custID <= 0 || p.CustID == custID) && p.Start >= startDate && p.Start <= endDate).ToList();

        }


        //Add for company invoices feature
        public List<PaymentMethod> GetPaymentMethodByProfileId(int profileId)
        {
            return _paymentMethodRepository.Table.Where(pm => pm.ProfileCompany.ProfileID == profileId).ToList();
        }


        public List<Appointment> GetAppointmentByCalendarId(int calendaId, DateTime startDate, DateTime endDate)
        {

            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        where (calendaId == 0 || aptp.CalendarID == calendaId)
                        && apt.Start >= startDate && apt.Start <= endDate
                        && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                            //&& apt.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                        && (apt.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= apt.Created) //for temporary pending on IP
                        select apt;
            return query.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calendaId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public List<Calendar> GetAppointmentByCalendarId(int calendaId, DateTime startDate)
        {
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        where (calendaId == 0 || aptp.CalendarID == calendaId)
                        && (apt.Start < startDate && startDate < apt.End || apt.Start == startDate || apt.End == startDate)
                        && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                            //&& apt.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                        && (apt.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= apt.Created) //for temporary pending on IP
                        select aptp.Calendar;
            return query.ToList();
        }

        /// <summary>
        /// Gets the active appointments by calendar id.
        /// </summary>
        /// <param name="calendaId">The calenda id.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<Appointment> GetActiveAppointmentsByCalendarId(int calendaId, DateTime startDate, DateTime endDate)
        {
            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        where (calendaId == 0 || aptp.CalendarID == calendaId)
                        && (apt.Start >= startDate && apt.Start <= endDate)
                        && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                            || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
                            || apt.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified
                            || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)

                        select apt;
            return query.ToList();
        }


        /// <summary>
        /// Gets the active appointments by calendar id.
        /// </summary>
        /// <param name="calendaId">The calenda id.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<RequestAppointment> GetRequestAppointmentsByCalendarId(int calendaId, DateTime startDate, DateTime endDate)
        {
            var query = from rapt in this._requestAppointmentRepository.Table
                        where (calendaId == 0 || rapt.CalendarId == calendaId)
                        && rapt.Start >= startDate && rapt.Start <= endDate
                        select rapt;
            return query.ToList();
        }

        /// <summary>
        /// GetRequestAppointmentsByCalendarId
        /// </summary>
        /// <param name="calendaId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public List<RequestAppointment> GetRequestAppointmentsByCalendarId(int calendaId, List<DateTime> statDate)
        {
            var query = from rapt in this._requestAppointmentRepository.Table
                        where (calendaId == 0 || rapt.CalendarId == calendaId)
                        && statDate.Contains(rapt.Start)
                        select rapt;
            return query.ToList();
        }


        //public Appointment GetAppointmentByAppointmentId(int appointmentId)
        //{
        //    if (appointmentId == 0)
        //        return null;
        //    var query = from apt in _appointmentRepository.Table
        //                where apt.AppointmentID == appointmentId
        //                select apt;
        //    return query.FirstOrDefault();
        //}

        public AppointmentParticipant GetAppointmentParticipantByAppointmentId(int appointmentId)
        {
            if (appointmentId == 0)
                return null;
            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        where apt.AppointmentID == appointmentId
                        select aptp;
            return query.FirstOrDefault();
        }

        public List<EmployeeHour> GetListEmployeeHour(int employeeId)
        {
            var query = from emph in _employeeHourRepository.Table
                        where emph.CompanyEmployeeID == employeeId && !emph.IsPreview
                        select emph;
            return query.ToList();
        }


        #region Appointment History
        public List<Service> GetServicesByCustomerId(int custId)
        {
            var lstService = (from s in _appointmentRepository.Table
                              where s.CustID == custId
                              select s.ServiceCompanyID).Distinct().ToList();
            var services = from sc in _serviceCompanyRepository.Table
                           join s in _serviceRepository.Table on sc.ServiceID equals s.ServiceID
                           where lstService.Contains(sc.ServiceCompanyID)
                           select s;
            return services.Distinct().ToList();

        }

        public List<Appointment> GetAppointmentHistoryNotReview(int custID, DateTime? day, int calendarId = 0, DateTime currentDate = default(DateTime))
        {
            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        //join svcp in _serviceCompanyRepository.Table on apt.ServiceCompanyID equals svcp.ServiceCompanyID
                        //join sv in _serviceRepository.Table on svcp.ServiceID equals sv.ServiceID
                        //join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        //join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID

                        where apt.CustID == custID
                              && (calendarId == 0 || aptp.CalendarID == calendarId)
                              && (day == null || (apt.End.Month == day.Value.Month && apt.End.Year == day.Value.Year) || (apt.Start.Year == day.Value.Year && apt.Start.Month == day.Value.Month))
                              && apt.Rating == null
                              && apt.End < currentDate
                              && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed
                                  || apt.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)

                        select apt;
            var result = query.OrderByDescending(m => m.Created).ToList();
            return result;
        }

        public List<Appointment> GetAppointmentHistoryReview(int custID, DateTime day, int calendarId = 0, DateTime currentDate = default(DateTime))
        {
            DateTime start = day.Date;// new DateTime(day.Year, day.Month, 1);
            DateTime end = start.AddMonths(1);


            var query = from apt in _appointmentRepository.Table
                        join aptp in _appointmentParticipantRepository.Table on apt.AppointmentID equals aptp.AppointmentID
                        //join svcp in _serviceCompanyRepository.Table on apt.ServiceCompanyID equals svcp.ServiceCompanyID
                        //join sv in _serviceRepository.Table on svcp.ServiceID equals sv.ServiceID
                        //join cal in _calendarRepository.Table on aptp.CalendarID equals cal.CalendarID
                        //join cals in _calendarShareRepository.Table on aptp.CalendarID equals cals.CalendarID
                        where apt.CustID == custID
                              && (calendarId == 0 || aptp.CalendarID == calendarId)
                              && ((apt.End.Month == day.Month && apt.End.Year == day.Year) || (apt.Start.Year == day.Year && apt.Start.Month == day.Month))
                              && apt.Rating != null
                              && apt.End < currentDate
                              && (apt.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed ||
                                apt.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                        select apt;
            var result = query.OrderByDescending(m => m.Created).ToList();
            return result;
        }

        public List<Rating> GetRatingByServiceCompanyId(int id)
        {
            return _ratingServiceRepository.Table.Where(r => r.ServiceCompanyID == id).ToList();
        }

        #endregion Appointment History


        /// <summary>
        /// Gets the appointments have noto unread.
        /// </summary>
        /// <param name="cust">The cust.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="fromDate">From date.</param>
        /// <returns></returns>
        public List<Appointment> GetAppointmentsHaveNote(Cust cust, ProfileCompany profile, DateTime fromDate)
        {
            var companyServiceIds = profile.ServiceCompanies.Select(c => c.ServiceCompanyID);
            var appointments =
                _appointmentRepository.Table.Where(a => a.CustID == cust.CustID
                    && (companyServiceIds.Contains(a.ServiceCompanyID.Value) || a.ProfileId == profile.ProfileID)
                    && a.AppointmentLogs.Any(l => l.LogDT >= fromDate)).Distinct();
            return appointments.ToList();
        }

        /// <summary>
        /// Gets the last non kuyam appointment.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public NonKuyamAppointment GetLastNonKuyamAppointment(int custId, int profileId = 0)
        {
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            return
                _nonKuyamAppointmentRepository.Table.Where(
                    a => a.CustID == custId && a.AppointmentStatusID == 0 && (profileId == 0 || a.ProfileId == profileId) && a.Start > today)
                                              .OrderByDescending(a => a.AppointmentID)
                                              .FirstOrDefault();
        }


        /// <summary>
        /// Gets the last non kuyam appointment.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public NonKuyamAppointment GetLastNonKuyamAppointmentById(int apptId)
        {
            return
                _nonKuyamAppointmentRepository.Table.Where(
                    a => a.AppointmentStatusID == 0 && a.AppointmentID == apptId)
                                              .OrderByDescending(a => a.AppointmentID)
                                              .FirstOrDefault();
        }

        /// <summary>
        /// Gets the non kuyam appointments.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public IQueryable<NonKuyamAppointment> GetNonKuyamAppointments(int custId)
        {
            return _nonKuyamAppointmentRepository.Table.Where(a => a.CustID == custId);
        }

        /// <summary>
        /// Adds the non kuyam appointment.
        /// </summary>
        /// <param name="nonKuyamAppointment">The non kuyam appointment.</param>
        /// <returns></returns>
        public bool AddNonKuyamAppointment(NonKuyamAppointment nonKuyamAppointment)
        {
            try
            {
                nonKuyamAppointment.StatusChangeDate = DateTime.UtcNow;
                nonKuyamAppointment.Created = DateTime.UtcNow;
                _nonKuyamAppointmentRepository.Insert(nonKuyamAppointment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the non kuyam appointment.
        /// </summary>
        /// <param name="nonKuyamAppointment">The non kuyam appointment.</param>
        /// <returns></returns>
        public bool UpdateNonKuyamAppointment(NonKuyamAppointment nonKuyamAppointment)
        {
            try
            {
                nonKuyamAppointment.StatusChangeDate = DateTime.UtcNow;
                _nonKuyamAppointmentRepository.Update(nonKuyamAppointment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateProposedKuyamAppointment(ProposedAppointment proposedAppointment)
        {
            try
            {
                proposedAppointment.StatusChangeDate = DateTime.UtcNow;
                _proposedAppoinmentRepository.Update(proposedAppointment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateRequestAppointment(RequestAppointment requestAppointment)
        {
            try
            {
                requestAppointment.Modified = DateTime.UtcNow;
                _requestAppointmentRepository.Update(requestAppointment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Only get appointments which don't have cancel and delete status
        /// </summary>
        /// <returns>IQueryable&lt;Appointment></returns>
        public IQueryable<Appointment> Get()
        {
            var cancelStatus = (int)Types.AppointmentStatus.Cancelled;
            var deleteStatus = (int)Types.AppointmentStatus.Delete;
            return _appointmentRepository.Table.Where(t => t.AppointmentStatusID != cancelStatus && t.AppointmentStatusID != deleteStatus);
        }

        /// <summary>
        /// Only get one appointment which start date &lt; now
        /// </summary>
        /// <param name="custId"></param>
        /// <returns></returns>
        public Appointment GetAppointmentReview(int custId)
        {
            var now = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            return Get().FirstOrDefault(apt => apt.CustID == custId && apt.Start < now && apt.Rating == null);
        }

        public bool AddRequestAppointment(List<RequestAppointment> items)
        {
            try
            {
                foreach (var item in items)
                {
                    _requestAppointmentRepository.Insert(item);
                }
            }
            catch (Exception)
            {

                return false;
            }

            return true;

        }

        public List<RequestAppointment> GetRequestAppointments(int companyId, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize, out int totalRecord)
        {
            var query = from rapt in this._requestAppointmentRepository.Table
                        where rapt.Status == (int)Types.RequestAppoitmentStatus.Default
                        select rapt;
            if (companyId != 0)
                query = query.Where(a => a.ProfileId == companyId);
            if (startDate.HasValue && endDate.HasValue && startDate != DateTime.MinValue && endDate != DateTime.MinValue)
            {
                query = from rapt in query
                        where DbFunctions.TruncateTime(rapt.Start) >= startDate && DbFunctions.TruncateTime(rapt.End) <= endDate
                        select rapt;
            }
            else
            {
                if (startDate.HasValue && startDate != DateTime.MinValue)
                {
                    query = from rapt in query
                            where DbFunctions.TruncateTime(rapt.Start) >= startDate
                            select rapt;
                }
                else
                {
                    if (endDate.HasValue && endDate != DateTime.MinValue)
                        query = from rapt in query
                                where DbFunctions.TruncateTime(rapt.End) <= endDate
                                select rapt;
                }
            }
            totalRecord = query.Count();
            return query.OrderByDescending(u => u.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public RequestAppointment GetRequestAppointmentsById(int Id)
        {
            var query = from rapt in this._requestAppointmentRepository.Table
                        where rapt.Id == Id
                        select rapt;
            return query.SingleOrDefault();
        }

        public ProposedAppointment InsertProposedAppointment(ProposedAppointment proposed)
        {
            try
            {
                proposed.Modified = DateTime.UtcNow;
                proposed.Created = DateTime.UtcNow;
                _proposedAppoinmentRepository.Insert(proposed);
                return proposed;
            }
            catch (Exception)
            {

                return null;
            }
            return null;
        }

        public ProposedAppointment GetProposedAppointmentById(int apptId)
        {
            var query = from rpapt in _proposedAppoinmentRepository.Table
                        where rpapt.AppointmentID == apptId
                        select rpapt;
            return query.FirstOrDefault();
        }

        public List<ProposedAppointment> GetProposedAppointment(int companyId, int pageIndex, int pageSize, out int totalRecord)
        {
            var query = from rapt in _proposedAppoinmentRepository.Table
                        where rapt.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.Default
                        select rapt;
            if (companyId != 0)
                query = query.Where(a => a.ProfileId == companyId);

            totalRecord = query.Count();
            return query.OrderByDescending(u => u.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<ProposedAppointment> GetProposedAppointmentsByCustId(int custId, int? calendarId, DateTime currentDate)
        {
            var query = from rapt in _proposedAppoinmentRepository.Table
                        where rapt.CustID == custId
                        && (!calendarId.HasValue || calendarId == 0 || rapt.CalendarId == calendarId)
                        && rapt.Start >= currentDate
                        && rapt.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.Default
                        select rapt;
            return query.OrderBy(m => m.Start).ToList();
        }

        public List<ProposedAppointment> GetProposedAppointmentsByCustId(int custId, int? calendarId, DateTime currentDate, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            var query = from rapt in _proposedAppoinmentRepository.Table
                        where rapt.CustID == custId
                        && (!calendarId.HasValue || calendarId == 0 || rapt.CalendarId == calendarId)
                        && rapt.Start > currentDate
                        && rapt.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.Default
                        select rapt;
            totalRecord = query.Count();

            return query.OrderBy(m => m.Start).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<ProposedAppointment> GetProposedAppointmentsByHotelId(int hotelId, DateTime currentDate, int pageIndex, int pageSize, string key, out int totalRecord)
        {
            totalRecord = 0;

            var query = from rapt in _proposedAppoinmentRepository.Table
                        where (hotelId == -1 || rapt.HotelID == hotelId)
                         && rapt.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.Default
                        select rapt;
            if (!string.IsNullOrEmpty(key))
                query = from rapt in query
                        where rapt.Cust != null &&
                        (rapt.Cust.FirstName.Contains(key)
                        || rapt.Cust.LastName.Contains(key)
                        || (rapt.Cust.FirstName.Contains(key) && rapt.Cust.LastName.Contains(key))
                        || (rapt.ProfileCompany != null && rapt.ProfileCompany.Name.Contains(key)))
                        select rapt;
            totalRecord = query.Count();

            return query.OrderBy(m => m.Start).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public Service GetServiceById(int Id)
        {
            Service service = _serviceRepository.Table.SingleOrDefault(a => a.ServiceID == Id);
            return service;
        }

        /// <summary>
        /// Gets the company time slots available.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="take">The number of items to get. 0 is get all .</param>
        /// <param name="timeSlot">The time slot.</param>
        /// <returns></returns>
        public List<CompanyTimeSlot> GetCompanyTimeSlotsAvailable(int profileId, DateTime startTime, DateTime endTime, int take = 0, int timeSlot = 30)
        {
            List<CompanyTimeSlot> results = new List<CompanyTimeSlot>();

            var companyAppointments = GetAppoinmentsByProfileId(profileId, startTime, endTime);

            var serviceHours = _companyProfileService.GetCompanyServiceTimes(profileId, startTime).ToList();

            for (DateTime time = startTime; time < endTime; time = time.AddMinutes(timeSlot))
            {
                var end = time.AddMinutes(timeSlot);
                var employeeAvaiable = 0;
                int status = (int)GetCompanyTimeslotStatus(time, end, companyAppointments, serviceHours, out employeeAvaiable);
                if (status == (int)CompanyTimeSlotStatus.Available)
                    results.Add(new CompanyTimeSlot()
                    {
                        Title = time.ToString("hh:mmtt"),
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

        /// <summary>
        /// Gets the company timeslot status.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="companyAppointments">The company appointments.</param>
        /// <param name="companyServiceHours">The company service hours.</param>
        /// <returns></returns>
        private CompanyTimeSlotStatus GetCompanyTimeslotStatus(DateTime start, DateTime end, List<Appointment> companyAppointments, List<ServiceTime> companyServiceHours, out int employeeAvaiable)
        {
            employeeAvaiable = 0;
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


        public SchedulerAvailability GetDataCheckOutOfClass(int classSchedulerId)
        {
            return _dbContext.SqlQuery<SchedulerAvailability>("GetDataCheckOutOfClass @ClassSchedulerID", new SqlParameter("ClassSchedulerID", classSchedulerId)).FirstOrDefault();
        }

        public SchedulerAvailability GetDataCheckOutForServiceIdAndEmployeeId(int serviceId, int employeeId)
        {
            return _dbContext.SqlQuery<SchedulerAvailability>("GetDataCheckOutForServiceIdAndEmployeeId @ServiceId, @EmployeeId", new SqlParameter("ServiceId", serviceId), new SqlParameter("EmployeeId", employeeId)).FirstOrDefault();
        }

        public InstructorClassScheduler GetInstructorClassSchedulerById(int classSchedulerById)
        {
            return _instructorClassSchedulerRepository.Table.Where(m => m.ID == classSchedulerById).FirstOrDefault();
        }
        public void UpdateInstructorClassScheduler(InstructorClassScheduler classScheduler)
        {
            _instructorClassSchedulerRepository.Update(classScheduler);
        }
    }
}
