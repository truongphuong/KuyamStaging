using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kuyam.Domain.Company;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.Domain;
using Kuyam.Utility;
using Kaltura;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;
using System.Configuration;
using Kuyam.WebUI.Helpers;
using System.Threading;
using System.Threading.Tasks;
using Kuyam.Mobile.Models;
using Kuyam.WebUI.Extension;
using Kuyam.Domain.Seo;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.ClassModel;

namespace Kuyam.WebUI.Controllers
{
    //[Authorize(Roles = "personal, admin, support, god")]
    public class CompanyProfileController : KuyamBaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly OrderService _orderService;
        private readonly NotificationService _notificationService;
        private readonly AdminService _adminService;
        private const string YellowAvailable = "yellowavailable";
        private const string RedAvailable = "redavailable";
        private const string OrrangeAvailable = "orangeavailable";
        private const string LightyellowAvailable = "lightyellowavailable";

        public CompanyProfileController(IAppointmentService appointmentService,
                                        CompanyProfileService companyProfileService,
                                        OrderService orderService,
                                        NotificationService notificationService,
                                        AdminService adminService)
        {
            _appointmentService = appointmentService;
            _companyProfileService = companyProfileService;
            _orderService = orderService;
            _notificationService = notificationService;
            _adminService = adminService;
        }

        #region Utility and private method

        /// <summary>
        ///  count day
        /// </summary>
        /// <param name="type"></param>
        /// <returns> minutes of day </returns>
        private int GetMinutesByType(int type)
        {
            //const int SECOND = 1;
            const int MINUTE = 1;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            switch (type)
            {
                case (int)Types.AlertTime.none:
                    return -1;
                case (int)Types.AlertTime.AtTimeOfEvent:
                    return 1;
                case (int)Types.AlertTime.FifteenMinBefore:
                    return 15;
                case (int)Types.AlertTime.ThirtyMinBefore:
                    return 30;
                case (int)Types.AlertTime.OneHourBefore:
                    return HOUR;
                case (int)Types.AlertTime.TwoHoursBefore:
                    return 2 * HOUR;
                case (int)Types.AlertTime.OneDayBefore:
                    return DAY;
                case (int)Types.AlertTime.TwoDaysBefore:
                    return 2 * DAY;

            }

            return 1;
        }

        private string CaculatorExpiresDate(int duration)
        {
            if (duration < 12)
                return duration + " month(s) after purchase";
            return duration + " year after purchase";
        }

        private string CaculatorExpiresDate(DateTime startdate, DateTime endDate)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;
            var ts = new TimeSpan(endDate.Ticks - startdate.Ticks);
            double delta = ts.TotalSeconds;// Math.Abs(ts.TotalSeconds);
            if (delta <= 0)
                return " never";
            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months + " month(s) after purchase";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years + " year after purchase";
            }
        }

        public static string GetCompanyPackages(List<CompanyPackage> lstPackage)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            string str = string.Empty;


            foreach (var companyPackage in lstPackage)
            {
                string unitPrice = string.Empty;
                if (companyPackage.DurationInMonth.HasValue && companyPackage.DurationInMonth.Value > 0)
                {
                    str = string.Format("expires {0}  month(s) after purchase.", companyPackage.DurationInMonth);
                }
                else
                {
                    str = string.Format("with unlimited booking(s).");
                }

                if (companyPackage.NumberOfBooking > 0)
                {
                    int tmp = (int)companyPackage.Price / companyPackage.NumberOfBooking;
                    if (tmp > 0)
                        unitPrice = string.Format("${0}/{1}", tmp, companyPackage.PackageName);
                }

                var obj = companyPackage.CompanyPackageServices.ToList();
                int numberservice = obj != null ? obj.Count : 0;
                stringBuilder.Append("<div class=\"packageitem\">");
                stringBuilder.Append("<div class=\"pkgimage\">");
                stringBuilder.AppendFormat("<img width=\"212px\" height=\"133px\" imageid={0} src=\'{1}/p/811441/thumbnail/entry_id/{0}/width/212/height/133\' class=\"fakepackageimage\"/>", companyPackage.KalturaImageId, Types.KaturaDoman);
                stringBuilder.Append("</div>");
                stringBuilder.Append("<div class=\"clear8\"></div>");
                stringBuilder.AppendFormat("<div class=\"pkgtitle\">{0}</div>", companyPackage.PackageName);
                stringBuilder.Append("<div class=\"clear\"></div>");
                stringBuilder.Append("<div class=\"pkgprice\">");
                stringBuilder.AppendFormat("<div class=\"txtprice\" name=\"{0}\" value=\"{1}\" quantity=\"{2}\" duration=\"{3}\" servicenumber=\"{4}\" packageid=\"{5}\" >${1}</div>", companyPackage.PackageName, companyPackage.Price, companyPackage.NumberOfBooking, companyPackage.DurationInMonth, numberservice, companyPackage.PackageId);
                stringBuilder.Append("<div class=\"divpurchase\">");
                stringBuilder.Append("<a href=\"javascript:void(0);\"  id=\"purchase90\" class=\"btnpurchase\" style=\"\" title=\"purchase\">purchase</a>");
                stringBuilder.Append("</div>");
                stringBuilder.Append("</div>");
                stringBuilder.Append("<div class=\"clear7\"></div>");
                stringBuilder.AppendFormat("<div class=\"pkgdescript\"></br>{0}</br>applies to: <a href=\"javascript:loadDetailService({3});\" title=\"package details\" class=\"apkgservice\" id=\"pkgdetail\">{1} services</a></br>{2}</div>", unitPrice, numberservice, str, companyPackage.PackageId);
                stringBuilder.Append("<div class=\"clear\"></div></div>");
            }
            return stringBuilder.ToString();
        }

        private string GetClassName(string color)
        {
            foreach (var pair in Constants.classNames)
            {
                if (pair.Value == color)
                {
                    return string.Format("fc-{0}", pair.Key);
                }
            }
            return string.Empty;
        }

        private List<WeekObject> GetData(DateTime start, DateTime end, int employeeId, int serviceId, int calendarId, int profileId)
        {

            List<WeekObject> weekObject = new List<WeekObject>();

            string resutl = string.Empty;
            try
            {
                //DateTime dtNow = start;
                //DateTime beginDay = start.Date;//new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

                Cust user = MySession.Cust;
                //var syncCalendar = _appointmentService.GetCalendarIdById(calendarId);
                //try
                //{
                //    if (syncCalendar != null)
                //    {
                //        InfoConnSoapClient service = new InfoConnSoapClient();
                //        SearchOption searchOption = new SearchOption
                //        {
                //            CalendarId = syncCalendar.SyncCalendarId,
                //            StartDate = DateTimeUltility.ConvertToUtcTime(dtNow, DateTimeUltility.CurrentTimeZone),
                //            EndDate = DateTimeUltility.ConvertToUtcTime(end, DateTimeUltility.CurrentTimeZone),
                //            ConnectorSourceType = ConnectorSourceType.Google
                //        };
                //        string className = GetClassName(syncCalendar.BackColor);

                //        if (syncCalendar.CalendarDisplayTypeID == (int)ConnectorSourceType.Google)
                //        {
                //            var eventsGoogle = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.Google);

                //            foreach (var evt in eventsGoogle)
                //            {
                //                var item = new CalendarObject
                //                {
                //                    id = evt.Id.ToString(),
                //                    title = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("h:mm"),
                //                    start = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    end = DateTimeUltility.ConvertToUserTime(evt.EndDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    className = "fc-orange",
                //                    eventType = (int)ConnectorSourceType.Google
                //                };
                //                calendarObject.Add(item);
                //            }

                //        }
                //        else if (syncCalendar.CalendarDisplayTypeID == (int)ConnectorSourceType.Facebook)
                //        {
                //            searchOption.ConnectorSourceType = ConnectorSourceType.Facebook;
                //            var eventsFacebook = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.Facebook);
                //            foreach (var evt in eventsFacebook)
                //            {
                //                var item = new CalendarObject
                //                {
                //                    id = evt.Id.ToString(),
                //                    title = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("h:mm"),
                //                    start = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    end = DateTimeUltility.ConvertToUserTime(evt.EndDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    className = "fc-orange",
                //                    eventType = (int)ConnectorSourceType.Google
                //                };
                //                calendarObject.Add(item);
                //            }

                //        }
                //        else if (syncCalendar.CalendarDisplayTypeID == (int)ConnectorSourceType.iCalendar)
                //        {
                //            searchOption.ConnectorSourceType = ConnectorSourceType.iCalendar;
                //            var eventsIcal = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.iCalendar);
                //            foreach (var evt in eventsIcal)
                //            {
                //                var item = new CalendarObject
                //                {
                //                    id = evt.Id.ToString(),
                //                    title = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("h:mm"),
                //                    start = DateTimeUltility.ConvertToUserTime(evt.StartDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    end = DateTimeUltility.ConvertToUserTime(evt.EndDate, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm"),
                //                    className = "fc-orange",
                //                    eventType = (int)ConnectorSourceType.Google
                //                };
                //                calendarObject.Add(item);
                //            }
                //        }
                //    }

                //}
                //catch (Exception)
                //{

                //}

                var profileCompany = _companyProfileService.GetProfileCompanyByID(profileId);
                var empAppointments = _appointmentService.GetAppoinmentTempsByEmployeeId(employeeId, calendarId, start, end);

                var instructorHours = _appointmentService.GetAppoinmentBusyTimByEmployeeId(profileId);

                var appoinmentsOfEmployee = _appointmentService.GetAppoinmentsByEmployeeId(employeeId, start, end);

                appoinmentsOfEmployee.ForEach(a => empAppointments.Add(a.ToAppointmentTemp()));

                var serviceHour = GetEventListByEmployeeId(profileId, employeeId, serviceId, start);

                //if (calendarId != 0)
                //{
                //    var calAppointment = _appointmentService.GetAppointmentByCalendarId(calendarId, dtNow, end);

                //    if (calAppointment != null && calAppointment.Count > 0)
                //    {
                //        foreach (var apt in calAppointment)
                //        {
                //            CalendarObject item = new CalendarObject
                //            {
                //                id = apt.CustID.ToString(),
                //                title = apt.Start.ToString("h:mm"),
                //                start = apt.Start.ToString("yyyy-MM-dd HH:mm"),
                //                end = apt.End.ToString("yyyy-MM-dd HH:mm"),
                //                className = "fc-orange"
                //            };
                //            calendarObject.Add(item);

                //        }
                //    }
                //}


                if (serviceHour != null && serviceHour.Count > 0)
                {
                    List<int> lstEmployee = _companyProfileService.GetEmployeeByProfileId(profileId);

                    // Get service of employee
                    List<CompanyService> lstService = _companyProfileService.GetServiceCompanybyEmployeeId(profileId, employeeId, serviceId);

                    var diffResult = end.Subtract(start);
                    int countDay = diffResult.Days;
                    for (var index = 0; index < countDay; index++)
                    {
                        DateTime currentDate = start.Date.AddDays(index);

                        int dayOfWeek = (int)currentDate.DayOfWeek;

                        string dayInWeek = currentDate.AddDays(index).ToString("dddd");
                        if (index == 0 && currentDate.Date == DateTime.Now.Date)
                        {
                            dayInWeek = "Today";
                        }

                        WeekObject timslotPerDay = new WeekObject();
                        timslotPerDay.DayOfWeek = dayOfWeek;
                        timslotPerDay.Day = dayInWeek;
                        timslotPerDay.DateTime = start.AddDays(index).ToString("MMM d, yyyy");

                        if (!serviceHour.Any(m => m.DayOfWeek == dayOfWeek))
                        {
                            weekObject.Add(timslotPerDay);
                            continue;
                        }

                        int stepHour = 30;

                        for (int i = 0; i < 48; i++)
                        {

                            string cssClass = string.Empty;
                            EventCustom eventCustom = serviceHour.Where(x => x.DayOfWeek == dayOfWeek
                                    && x.Start <= currentDate
                                    && x.End > currentDate).OrderBy(m => m.Start).FirstOrDefault();

                            DateTime beginDate = currentDate;
                            DateTime endDate = currentDate.AddMinutes(stepHour);

                            if (eventCustom != null && (beginDate > start))
                            {

                                if (eventCustom.Start <= beginDate && endDate <= eventCustom.End || (endDate > eventCustom.End && beginDate < eventCustom.End))
                                {
                                    DateTime enddt = (eventCustom.End <= endDate) ? eventCustom.End : endDate;

                                    TimeSpan tsp = enddt - beginDate;
                                    int duration = tsp.Minutes;
                                    CalendarObject item = new CalendarObject
                                    {
                                        id = eventCustom.EmployeeID.ToString(),
                                        title = beginDate.ToString("h:mm"),
                                        start = beginDate.ToString("yyyy-MM-dd HH:mm"),
                                        end = enddt.ToString("yyyy-MM-dd HH:mm"),
                                        duration = 0,
                                        className = "fc-black"
                                    };

                                    if (profileCompany.HasClassBooking)
                                    {
                                        if (instructorHours.Any(y => eventCustom.EmployeeID == y.EmployeeID && y.DayOfWeek == (int)beginDate.DayOfWeek
                                            && ((beginDate < beginDate.Date.Add(y.Start) && beginDate.Date.Add(y.End) < enddt)
                                               || (beginDate <= beginDate.Date.Add(y.Start) && beginDate.Date.Add(y.Start) < enddt)
                                               || (beginDate < beginDate.Date.Add(y.End) && beginDate.Date.Add(y.End) <= enddt)))
                                        )
                                        {
                                            currentDate = currentDate.AddMinutes(stepHour);
                                            continue;
                                        }
                                    }

                                    var aptNext = empAppointments.Where(m => m.Start == endDate).OrderBy(m => m.Start).FirstOrDefault();

                                    var isAvailability = lstService.Any(s => ((employeeId == 0 && s.EmployeeID == eventCustom.EmployeeID) || s.EmployeeID == employeeId) && serviceHour.Any(m => (m.Start <= beginDate && m.End >= beginDate.AddMinutes(s.Duration)) && (aptNext == null || beginDate.AddMinutes(s.Duration) <= aptNext.Start) && m.EmployeeID == s.EmployeeID));

                                    bool existTimeslot = false;

                                    if (empAppointments != null)
                                    {
                                        var aptend = empAppointments.Where(m => (m.Start <= beginDate && beginDate < m.End) || (m.Start < enddt && enddt <= m.End));

                                        if (!aptend.Any() && duration >= 30 && isAvailability)
                                        {
                                            timslotPerDay.TimeSlot.Add(item);
                                            existTimeslot = true;
                                        }
                                    }
                                    else
                                    {
                                        if (duration >= 30 && isAvailability)
                                        {
                                            timslotPerDay.TimeSlot.Add(item);
                                            existTimeslot = true;
                                        }
                                    }

                                    if (employeeId == 0 && !existTimeslot)
                                    {
                                        foreach (int id in lstEmployee)
                                        {
                                            var tmpHour = serviceHour.Where(x => x.DayOfWeek == index && x.EmployeeID == id && x.Start <= currentDate && currentDate <= x.End && x.End >= enddt);
                                            var aptend = empAppointments.Where(m => (m.Start <= beginDate && beginDate < m.End) || (m.Start < enddt && enddt <= m.End));
                                            var isAvailabilityNew = lstService.Any(s => (s.EmployeeID == id) && serviceHour.Any(m => (m.Start <= beginDate && m.End >= beginDate.AddMinutes(s.Duration)) && (aptNext == null || beginDate.AddMinutes(s.Duration) <= aptNext.Start) && m.EmployeeID == s.EmployeeID));
                                            if (tmpHour.Any() && !aptend.Any(m => m.EmployeeID == id) && duration >= 30 && isAvailabilityNew)
                                            {
                                                item.id = id.ToString();
                                                timslotPerDay.TimeSlot.Add(item);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            currentDate = currentDate.AddMinutes(stepHour);
                        }

                        weekObject.Add(timslotPerDay);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return weekObject;
        }

        private List<CalendarObject> GetGeneralAvailability(DateTime start, DateTime end, int employeeId, int calendarId, int profileId)
        {

            List<CalendarObject> calendarObject = new List<CalendarObject>();

            string resutl = string.Empty;
            try
            {
                DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
                DateTime beginDay = dtNow.Date;

                //var calAppointment = _appointmentService.GetAppointmentByCalendarId(calendarId, dtNow, end);

                //var empAppointments = _appointmentService.GetAppoinmentTempsByEmployeeId(employeeId, calendarId, dtNow, end);

                //_appointmentService.GetAppoinmentsByEmployeeId(employeeId, dtNow, end).ForEach(a => empAppointments.Add(a.ToAppointmentTemp()));

                var serviceHours = _companyProfileService.GetCompanyGeneralServiceTimes(profileId, beginDay).ToList();

                if (serviceHours != null && serviceHours.Count > 0)
                {
                    List<int> lstEmployee = _companyProfileService.GetEmployeeByProfileId(profileId);

                    var calRequestAppointments = _appointmentService.GetRequestAppointmentsByCalendarId(calendarId, beginDay, end);

                    if (calRequestAppointments.Any() && calendarId != 0)
                    {
                        foreach (var apt in calRequestAppointments)
                        {
                            CalendarObject item = new CalendarObject
                            {
                                id = apt.CustID.ToString(),
                                title = string.Format("{0} to {1}", apt.Start.ToString("h:mmtt").ToLower(), apt.End.ToString("h:mmtt").ToLower()),
                                start = apt.Start.ToString("yyyy-MM-dd HH:mm"),
                                end = apt.End.ToString("yyyy-MM-dd HH:mm"),
                                className = "fc-blue"
                            };
                            calendarObject.Add(item);

                        }
                    }


                    for (DateTime date = beginDay; date < end; date = date.AddDays(1))
                    {
                        var availability = serviceHours.Where(x => x.DateOfWeek == (int)date.DayOfWeek && (calendarId == 0 || calRequestAppointments == null || calRequestAppointments.Count() == 0 || !calRequestAppointments.Any(a => (a.Start < date.Add(x.FromHour) && a.End > date.Add(x.ToHour)) || (date.Add(x.FromHour) <= a.Start && a.Start < date.Add(x.ToHour)) || (date.Add(x.FromHour) < a.End && a.End <= date.Add(x.ToHour)))))
                            .Select(m => new CalendarObject
                            {
                                id = m.Id.ToString(),
                                title = string.Format("{0} to {1}", date.Add(m.FromHour).ToString("hh:mmtt").StartsWith("0") ? string.Format("&nbsp;&nbsp;{0}", date.Add(m.FromHour).ToString("h:mmtt").ToLower()) : date.Add(m.FromHour).ToString("h:mmtt").ToLower(), date.Add(m.ToHour).ToString("hh:mmtt").StartsWith("0") ? string.Format("&nbsp;&nbsp;{0}", date.Add(m.ToHour).ToString("h:mmtt").ToLower()) : date.Add(m.ToHour).ToString("h:mmtt").ToLower()),
                                start = date.Add(m.FromHour).ToString("yyyy-MM-dd HH:mm"),
                                end = date.Add(m.ToHour).ToString("yyyy-MM-dd HH:mm"),
                                duration = 0,
                                className = "fc-black"
                            }).ToList();
                        if (availability.Any())
                        {
                            calendarObject.AddRange(availability);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }

            return calendarObject;
        }
        private List<EventCustom> GetEventListByEmployeeId(int profileId, int employeeId, int serviceId, DateTime dtnow)
        {
            var eventCustom = new List<EventCustom>();
            List<EmployeeHour> lstEmployeeHour = _companyProfileService.GetEmployeeHour(profileId, employeeId, serviceId);

            if (lstEmployeeHour == null && lstEmployeeHour.Count <= 0)
                return null;

            int dayOfWeek = (int)dtnow.DayOfWeek;
            int detDay = 7 - dayOfWeek;
            int day = 0;

            var companyHours = _companyProfileService.GetCompanyHourList(profileId);

            foreach (var item in lstEmployeeHour)
            {
                DateTime dt = dtnow;

                if (item.DayOfWeek >= dayOfWeek)
                {
                    day = item.DayOfWeek - dayOfWeek;
                }
                else
                {
                    day = item.DayOfWeek + detDay;
                }

                dt = dt.AddDays(day);
                dt = dt.Date;

                var eventCustoms = _companyProfileService.GetIntersectWithCompanyHours(companyHours, item.CompanyEmployeeID, dt, item.FromHour, item.ToHour);

                foreach (ServiceTime serviceTime in eventCustoms)
                {
                    EventCustom ec = new EventCustom();
                    ec.DayOfWeek = serviceTime.DateOfWeek;
                    ec.Start = serviceTime.FromDateTime;
                    ec.End = serviceTime.ToDateTime;
                    ec.EmployeeID = item.CompanyEmployee.EmployeeID;
                    ec.ProfileID = item.CompanyEmployee.ProfileCompanyID;
                    ec.EmployeeName = item.CompanyEmployee.EmployeeName;
                    ec.ClassCustom = string.Empty;
                    eventCustom.Add(ec);
                }

            }

            return eventCustom;
        }

        private List<Service> GetServiceListByProfileCompanyId(int profileCompanyId)
        {
            return ProfileCompany.GetServiceListByProfileCompanyId(profileCompanyId);
        }

        private List<CompanyEmployee> GetEmployeeListByProfileCompanyId(int profileCompanyId)
        {
            return ProfileCompany.GetActiveEmployeeListByProfileCompanyId(profileCompanyId);
        }

        #endregion End Utility

        public ActionResult Index(int id)
        {
            return RedirectToAction("availability", new { id = id });
        }

        #region Favorite

        [HttpPost]
        public ActionResult AddToFavorite(string profileID)
        {
            Favorite fav = new Favorite();
            fav.ProfileID = int.Parse(profileID);
            fav.CustID = MySession.CustID;
            fav.CreatedTime = DateTime.Now;

            _companyProfileService.AddFavorite(fav);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFavorite()
        {
            List<cFavorite> result = new List<cFavorite>();
            var favoriteResult = _companyProfileService.GetFavoriteListByCustID(MySession.CustID);

            foreach (var fav in favoriteResult)
            {
                cFavorite f = new cFavorite();
                f.ProfileID = fav.ProfileID;
                f.Name = fav.Name;
                f.Slug = Url.RouteUrl("availability", new { sename = fav.GetSeName(fav.ProfileID) });
                result.Add(f);

            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveFromFavorite(string profileID)
        {
            int companyProfileId = 0;
            if (Int32.TryParse(profileID, out companyProfileId))
            {
                var favorite = _companyProfileService.GetFavoriteByProfileIdAndCustID(companyProfileId, MySession.CustID);
                _companyProfileService.RemoveFromFavorite(favorite);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion Favorite

        public ActionResult GetServiceByEmployee(int employeeId, DateTime expectedDate, int? serviceId)
        {
            string data = "<option value='0' selected='selected' code='0'>select a service</option>";
            var lstService = _companyProfileService.GetServiceCompanybyEmployeeId(0, employeeId, serviceId ?? 0);
            Cust user = MySession.Cust;
            foreach (var companyService in lstService)
            {
                string plainText = string.Format("{0}@{1}@{2}@{3}", expectedDate.ToString(), companyService.ID, employeeId,
                                             user.CustID);
                string ciperText = Kuyam.Utility.SecurityHelper.EncryptStringToBytesAes(plainText);
                //string html =
                //    string.Format(
                //        "<a href=\"\\AppointmentPreview\\Index?genre={2}\" ><div class=\"tblitem {0}\" val=\"{1}\"></div></a>"
                //        ,
                //        active, duration, ciperText);
                data +=
                    String.Format(
                        "<option txt='{5}' duration='{6}' code='{7}' desc='{8}'  value='{0}'>{1}, {2}, {3}, {4}</option>",
                        companyService.ID,
                        companyService.ServiceName, companyService.Duration + "min",
                        "$" + companyService.Price, "1 person", companyService.ServiceName, companyService.Duration,
                        ciperText, companyService.Description);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the service by employee id.
        /// </summary>
        /// <param name="employeeId">The employee id.</param>
        /// <param name="packageId">The package id.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns></returns>
        public ActionResult GetServiceByEmployeeId(int profileId, int employeeId, int? serviceId, int? packageId, string startDate, int? categoryId, int? calendarId)
        {
            //Parse start time
            DateTime startTime = DateTime.ParseExact(startDate, "yy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);

            var profileCompany = _companyProfileService.GetProfileCompanyByID(profileId);


            // Get service of employee
            List<CompanyService> lstService = packageId.HasValue
                                                  ? _companyProfileService.GetServiceEmployeeByPackage(employeeId, packageId.Value, profileId, categoryId ?? 0)
                                                  : _companyProfileService.GetServiceCompanybyEmployeeId(profileId, employeeId, serviceId ?? 0, categoryId ?? 0);

            var serviceHour = GetEventListByEmployeeId(profileId, employeeId, serviceId ?? 0, startTime);

            var empAppointments = _appointmentService.GetAppoinmentTempsByEmployeeId(employeeId, calendarId ?? 0, startTime, DateTime.MaxValue);

            _appointmentService.GetAppoinmentsByEmployeeId(employeeId, startTime, DateTime.MaxValue).ForEach(a => empAppointments.Add(a.ToAppointmentTemp()));

            var aptNext = empAppointments.Where(m => m.Start == startTime.AddMinutes(30)).OrderBy(m => m.Start).FirstOrDefault();

            var listServiceAvailability = lstService.Where(s => (!profileCompany.HasClassBooking || s.ServiceTypeId == (int)Types.ServiceType.ServiceType)
                && (employeeId == 0 || s.EmployeeID == employeeId)
                && serviceHour.Any(m => (m.Start <= startTime && m.End >= startTime.AddMinutes(s.Duration)) && (aptNext == null || startTime.AddMinutes(s.Duration) <= aptNext.Start) && m.EmployeeID == s.EmployeeID)).ToList();

            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (startTime < DateTime.Now)
            {
                dict.Add("validTime", false);
            }


            if (profileCompany != null && profileCompany.IsShowCatagory.HasValue && profileCompany.IsShowCatagory.Value && !categoryId.HasValue)
            {
                var categorys = _companyProfileService.GetCategoryByServiceAvailability(listServiceAvailability.Select(m => m.ServiceID).Distinct().ToList(), profileId);
                var model = categorys.Select(m => new CategoryModel(m)).ToList();
                dict.Add("filltercatagory", true);
                dict.Add("category", model);
                return Json(dict, JsonRequestBehavior.AllowGet);

            }
            else
            {
                dict.Add("filltercatagory", false);
                var result = listServiceAvailability.Distinct(new CompanyServiceComparer());
                // build options
                StringBuilder htmlSevice = new StringBuilder();
                htmlSevice.Append("<option value='0' selected='selected' >select a service</option>");
                if (result.Any())
                {
                    foreach (var companyService in result)
                    {
                        CompanyServiceEvent cseItem = _adminService.GetAvalableCompanyServiceEventFromCompanyServiceId(companyService.ID, startTime);
                        var percentstring = "0";
                        if (cseItem != null)
                        {
                            double percent = (double)(((cseItem.ServiceCompany.Price.Value - cseItem.NewPrice.Value) * 100) / cseItem.ServiceCompany.Price.Value);
                            if (percent > 0)
                            {
                                percentstring = Math.Round(percent, 0).ToString();
                            }

                        }

                        htmlSevice.AppendFormat("<option txt=\"{5}\" duration=\"{6}\" desc=\"{7}\" employeeid =\"{8}\"  employeename=\"{9}\" offerPercent=\"{10}\" value=\"{0}\">{1}, {2} min, ${3}, {4} person</option>", companyService.ID, UtilityHelper.TruncateText(companyService.ServiceName, 36), companyService.Duration,
                            cseItem != null ? cseItem.NewPrice.Value : companyService.Price, companyService.AttendeesNumber, companyService.ServiceName, companyService.Duration, companyService.Description, companyService.EmployeeID, companyService.EmployeeName, percentstring);
                    }

                }
                else
                {
                    dict.Add("invalidtime", false);
                }

                dict.Add("sevice", htmlSevice.ToString());
                // Get employee
                //var emp = _companyProfileService.GetListEmployeeById(employeeId, profileId);

                //dict.Add("employees", emp);

                StringBuilder htmlCalendar = new StringBuilder();

                List<Kuyam.Database.Calendar> listCal = _companyProfileService.GetCalendarByCustId(MySession.CustID);
                var calAppointment = _appointmentService.GetAppointmentByCalendarId(0, startTime);
                var calendarAvailability = listCal.Where(m => (calAppointment == null || calAppointment.Count() <= 0 || !calAppointment.Any(n => n.CalendarID == m.CalendarID)));
                foreach (var cal in calendarAvailability)
                {
                    string selected = string.Empty;
                    if (cal.CalendarID == calendarId)
                    {
                        selected = "selected";
                    }

                    htmlCalendar.AppendFormat("<option value=\"{0}\" {2} >{1}</option>", cal.CalendarID, Kuyam.Domain.UtilityHelper.TruncateText(cal.Name, 10), selected);

                }
                dict.Add("calendar", htmlCalendar.ToString());

                return Json(dict, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetGeneralServiceByCategoryId(int profileId, int? categoryId, [ModelBinderAttribute(typeof(CommaModelBinder))] string[] timeSlots, int? calendarId)
        {

            var profileCompany = _companyProfileService.GetProfileCompanyByID(profileId);

            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (profileCompany != null && profileCompany.IsShowCatagory.HasValue && profileCompany.IsShowCatagory.Value && !categoryId.HasValue)
            {
                var servicesCompany = _companyProfileService.GetServiceCompanyByProfileID(profileId);
                var categorys = _companyProfileService.GetCategoryByServiceAvailability(servicesCompany.Select(m => m.ServiceID).ToList(), profileId);
                var model = categorys.Select(m => new CategoryModel(m)).ToList();
                dict.Add("filltercatagory", true);
                dict.Add("category", model);
                return Json(dict, JsonRequestBehavior.AllowGet);

            }
            else
            {
                // Get service of employee
                List<CompanyService> lstService = _companyProfileService.GetGeneralServiceCompanybyEmployeeId(0, profileId, categoryId ?? 0);
                var result = lstService.Distinct(new CompanyServiceComparer());
                dict.Add("filltercatagory", false);
                // build options
                StringBuilder htmlSevice = new StringBuilder();
                htmlSevice.Append("<option value='0' selected='selected' >select a service</option>");
                if (result.Any())
                {
                    foreach (var companyService in result)
                    {


                        htmlSevice.AppendFormat("<option txt=\"{5}\" duration=\"{6}\" desc=\"{7}\" employeeid =\"{8}\"  employeename=\"{9}\" value=\"{0}\">{1}, {2} min, ${3}, {4} person</option>", companyService.ID, UtilityHelper.TruncateText(companyService.ServiceName, 36), companyService.Duration,
                           companyService.Price, companyService.AttendeesNumber, companyService.ServiceName, companyService.Duration, companyService.Description, string.Empty, string.Empty);
                    }

                }
                else
                {
                    dict.Add("invalidtime", false);
                }

                dict.Add("sevice", htmlSevice.ToString());
                // Get employee
                //var emp = _companyProfileService.GetListEmployeeById(0, profileId);

                //dict.Add("employees", emp);

                //Parse start time 

                List<DateTime> startTimeslots = new List<DateTime>();
                foreach (var item in timeSlots)
                {
                    string[] timeslot = item.Split('|');
                    startTimeslots.Add(DateTime.Parse(timeslot[0]));
                }


                StringBuilder htmlCalendar = new StringBuilder();

                List<Kuyam.Database.Calendar> listCal = _companyProfileService.GetCalendarByCustId(MySession.CustID);
                var calRequestAppointments = _appointmentService.GetRequestAppointmentsByCalendarId(0, startTimeslots);
                var calendarAvailability = listCal.Where(m => (calRequestAppointments == null || calRequestAppointments.Count() <= 0 || !calRequestAppointments.Any(n => n.CalendarId == m.CalendarID)));
                foreach (var cal in calendarAvailability)
                {
                    string selected = string.Empty;
                    if (cal.CalendarID == calendarId)
                    {
                        selected = "selected";
                    }

                    htmlCalendar.AppendFormat("<option value=\"{0}\" {2} >{1}</option>", cal.CalendarID, Kuyam.Domain.UtilityHelper.TruncateText(cal.Name, 10), selected);

                }
                dict.Add("calendar", htmlCalendar.ToString());

                return Json(dict, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult CheckTimeSlot(int serviceId, int employeeId, int calendarId, string startDate, int? packageId)
        {
            if (MySession.AppointmentbookingTempId > 0)
            {
                var appointmentTemp = _appointmentService.GetAppoinmentTempsById(MySession.AppointmentbookingTempId);
                _appointmentService.DeleteAppointmentTemp(appointmentTemp);
                MySession.AppointmentbookingTempId = 0;
            }

            var companyService = _companyProfileService.GetServiceCompanyByID(serviceId);
            var duration = companyService.Duration ?? 60;

            DateTime start = DateTime.ParseExact(startDate, "yy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);

            if (_appointmentService.GetAppoinmentTempsByEmployeeId(employeeId, calendarId, start, start.AddMinutes(duration)).Count > 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            if (_appointmentService.GetAppoinmentsByEmployeeId(employeeId, start, start.AddMinutes(duration)).Count > 0)
                return Json(false, JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClearTimeSlot()
        {
            if (MySession.AppointmentbookingTempId > 0)
            {
                var AppointmentbookingTmp = _appointmentService.GetAppoinmentTempsById(MySession.AppointmentbookingTempId);
                _appointmentService.DeleteAppointmentTemp(AppointmentbookingTmp);
                MySession.AppointmentbookingTempId = 0;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemovePackageFromList(int userPackageId)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            int profileId = _companyProfileService.DeleteUserPackage(userPackageId);
            dic.Add("profileId", profileId);
            if (profileId > 0)
                dic.Add("status", "true");
            else
                dic.Add("status", "false");
            return Json(dic, JsonRequestBehavior.DenyGet);
        }

        public ActionResult GetEmployeeInfo()
        {
            throw new NotImplementedException();
        }

        #region checkout

        [HttpPost]
        public ActionResult buyPackage(int packageId, int price, string discountCode)
        {
            CompanyPackage companyPackage = _companyProfileService.GetCompanyPackagebyPackageId(packageId);
            int numberMonth = (companyPackage != null && companyPackage.DurationInMonth.HasValue) ? companyPackage.DurationInMonth.Value : 0;
            decimal discountamount = 0;
            Discount discount = _orderService.GetDiscoutByCode(discountCode);
            if (discount != null)
            {
                if (discount.Amount > 0)
                {
                    discountamount = discount.Amount;
                }
                else
                {
                    discountamount = price * discount.Percent / 100;
                }
                MySession.DiscountCode = discount.Code;
            }
            UserPackagePurchase userPackagePurchase = new UserPackagePurchase
            {
                CompanyPackageId = packageId,
                CustID = MySession.CustID,
                MaxUses = companyPackage.NumberOfBooking,
                OrginalPrice = companyPackage.Price,
                PurchaseDate = DateTime.UtcNow,
                PurchasePrice = (price > discountamount) ? price - discountamount : 0,
                ExpiredDate = DateTime.UtcNow.AddMonths(companyPackage.DurationInMonth.Value),
                CompanyPackage = companyPackage
            };


            MySession.UserPackagePurchase = userPackagePurchase;
            MySession.PurchaseCompanyProfileId = companyPackage.ProfileCompanyId;
            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDiscountCode(string code, int serviceId, int profileId)
        {
            DiscountExt discount = _orderService.GetDiscoutByCode(code, serviceId, profileId, MySession.CustID);

            return Json(discount, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDiscountPackageCode(string code, int packageId)
        {
            DiscountExt discount = _orderService.GetDiscoutPackageByCode(code, packageId, MySession.CustID);
            return Json(discount, JsonRequestBehavior.AllowGet);

        }

        #endregion End checkout

        #region View Availability

        [Authorize]
        public ActionResult ProposedBook(int? Id)
        {
            if (Id.HasValue)
            {
                var apptp = _appointmentService.GetProposedAppointmentById(Id.Value);

                if (apptp != null && apptp.HotelStaff != null)
                {
                    int custId = apptp.HotelStaff.CustID;
                    if (custId != MySession.CustID && MySession.Cust.GetRole.Contains("Concierge"))
                    {
                        MySession.ImpersonateId = custId;
                    }
                }

                if (apptp == null || apptp.CustID != MySession.CustID)
                {
                    //return RedirectToRoute("home", new { message = true });
                    return new RedirectResult("~/Home/Index?message=true");
                }

                if (apptp.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.booked)
                {
                    //return RedirectToRoute("home", new { message = "booked" });
                    return new RedirectResult("~/Home/Index?message=booked");
                }
                return RedirectToAction("Availability", new { id = apptp.ProfileId, proposedId = Id });
            }
            return View();
        }

        //[Authorize]
        public ActionResult Availability(int? id, int? proposedId, int? categoryId, int? serviceId)
        {
            MySession.IsBookDirect = false;

            var model = new ProfileCompaniesModels();
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id.Value);
            if (proposedId.HasValue)
            {
                var apptp = _appointmentService.GetProposedAppointmentById(proposedId.Value);
                if (apptp == null || apptp.CustID != MySession.CustID)
                    return new RedirectResult("~/Home?message=true");
                if (apptp.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.booked)
                    return new RedirectResult("~/Home?message=booked");
            }

            ViewBag.EmployeeList = _companyProfileService.GetEmployeeListByProfileCompanyId(id.Value);
            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow).ToString("yyyy/MM/dd hh:mm:ss");
            ViewBag.ListCal = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            model.ProfileId = id.Value;
            model.ProfileCompany = profileCompany;
            var categorys = _companyProfileService.GetCategoryByProfileID(id.Value);
            model.ListServiceCompany = categorys;
            var category = categorys.FirstOrDefault();
            model.CategoryId = categoryId ?? 0;
            ViewBag.CategoryId = categoryId;
            model.Favorite = _companyProfileService.CheckFavoriteByProfileID(id.Value, MySession.CustID);
            model.CompanyName = profileCompany.Name;
            var Keywords = MyApp.Settings.TagSetting.Keywords;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);

            var serviceCompanys = _companyProfileService.GetServiceCompanyByProfileID(id.Value);
            StringBuilder htmlSevice = new StringBuilder();
            htmlSevice.Append("<option value='0' selected='selected' >all services (or select one)</option>");
            foreach (var item in serviceCompanys)
            {
                string selected = string.Empty;
                if (item.ServiceCompanyID == serviceId)
                {
                    selected = "selected";
                }

                htmlSevice.AppendFormat("<option  title=\"0\" value=\"{1}\" {2} >{3}</option>", item.Service.ServiceName, item.ServiceCompanyID, selected, string.Format("{0}, {1} min, ${2}, {3} person", UtilityHelper.TruncateAtWord(item.Service.ServiceName, 30), item.Duration, item.Price, item.AttendeesNumber));
            }
            model.ServiceString = htmlSevice.ToString();
            //business hours
            var hours = _companyProfileService.SplitCompanyHours(model.ProfileCompany.CompanyHours.ToList());
            hours = _companyProfileService.SortCompanyHours(hours);
            ViewBag.CompanyHoursSort = hours;
            //Ratings            
            if (id.HasValue)
            {
                ViewBag.RatingList = _appointmentService.GetRatingsByProfileId(id.Value);
            }
            else
            {
                ViewBag.RatingList = new List<Rating>();
            }
            //Package
            MySession.IsBookDirect = false;
            var packages = _companyProfileService.GetCompanyPackages(id.Value);
            ViewBag.CompanyPackages = packages;//GetCompanyPackages(packages);
            //Photo
            model.MediaCompanies = _companyProfileService.GetCompanyMediums(id ?? 0, Types.CompanyMediaType.IsBanner);
            //return View("_OldAvailability", model);
            return View(model);
        }

        public ActionResult Class(int id = 0, int serviceId = 0)
        {
            var model = new ProfileCompaniesModels();
            model.ProfileId = id;
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            model.ProfileCompany = profileCompany;
            var categorys = _companyProfileService.GetCategoryByProfileID(id);
            model.ListServiceCompany = categorys;
            var category = categorys.FirstOrDefault();
            model.CompanyName = profileCompany.Name;
            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            int dayOfWeek = (int)dtnow.DayOfWeek;

            //DateTime startTime = dtnow;
            //if (dayOfWeek == (int)System.DayOfWeek.Sunday)
            //{
            //    startTime = dtnow.AddDays(-6);
            //}
            //else
            //{
            //    startTime = dtnow.AddDays(-(dayOfWeek - 1));
            //}

            DateTime endTime = dtnow.AddDays(7);

            var calendars = _companyProfileService.GetSchedulerAvailabilityOfClass(id, serviceId, 0, dtnow.ToString("MM-dd-yyyy hh:mm"), endTime.ToString("MM-dd-yyyy hh:mm"));
            model.CalendarString = BuildCalendar(calendars, dtnow);

            return View(model);
        }

        private string BuildCalendar(List<SchedulerAvailability> schedulerAvailability, DateTime dtNow)
        {
            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < 7; i++)
            {
                string dayInWeek = dtNow.AddDays(i).ToString("ddd").ToLower();
                string classToday = string.Empty;
                if (i == 0)
                {
                    classToday = "today";
                    dayInWeek = "today";
                }

                strBuilder.AppendFormat("<div class=\"column-day {0}\">", classToday);
                strBuilder.AppendFormat("<div class=\"column-header\"><span class=\"day\">{0}</span><span class=\"datetime\">{1}</span></div>", dayInWeek, dtNow.AddDays(i).ToString("MMM d, yyyy"));


                var classIndays = schedulerAvailability.Where(q => q.DayOfWeek == (int)dtNow.AddDays(i).DayOfWeek).OrderBy(o => o.FromHour).ToList();

                foreach (var item in classIndays)
                {
                    DateTime start = dtNow.AddDays(i).Date.AddTicks(item.FromHour.Ticks);
                    string isFull = "full";

                    if (item.IsAvailability && start < dtNow)
                    {
                        isFull = "expired";
                    }

                    string btnReserve = string.Empty;
                    if (start >= dtNow.AddMinutes(10) && item.IsAvailability)
                    {
                        isFull = string.Empty;
                        btnReserve = string.Format("<a id=\"reserve\" classSchedulerId =\"{0}\" startTime=\"{1}\" emdTime=\"{2}\" className= \"{3}\" instructorName=\"{4}\" class=\"reserve\"  href=\"javascript:void(0);\">reserve</a>",
                            item.ClassSchedulerID, start.ToString("MM/dd/yyyy hh:mm tt"), start.AddMinutes(item.Duration).ToString("MM/dd/yyyy hh:mm tt"), item.ServiceName, item.EmployeeName);
                    }

                    strBuilder.Append("<div class=\"column-sessions\">");
                    strBuilder.AppendFormat(" <div class=\"session {0}\">", isFull);
                    strBuilder.AppendFormat("<span class=\"time\">{0} – {1}</span>", new DateTime(item.FromHour.Ticks).ToString("h:mm tt").ToLower(), new DateTime(item.FromHour.Ticks).AddMinutes(item.Duration).ToString("h:mm tt").ToLower());
                    strBuilder.AppendFormat("<span class=\"instructor\">{0}</span>", item.EmployeeName);
                    strBuilder.AppendFormat("<span class=\"type\">{0}</span>", item.ServiceName);
                    strBuilder.Append(btnReserve);
                    //strBuilder.AppendFormat("<a id=\"reserve\" classSchedulerId =\"{0}\" startTime=\"{1}\" emdTime=\"{2}\" className= \"{3}\" instructorName=\"{4}\" class=\"reserve\"  href=\"javascript:void(0);\">reserve</a>",
                    //item.ClassSchedulerID, start.ToString("MM/dd/yyyy hh:mm tt"), start.AddMinutes(item.Duration).ToString("MM/dd/yyyy hh:mm tt"), item.ServiceName, item.EmployeeName);

                    strBuilder.Append("</div></div>");

                }
                strBuilder.Append("</div>");
            }

            return strBuilder.ToString();
        }

        public ActionResult GetTimslotsClassById(int profileId = 0, int serviceId = 0, int employeeId = 0, int weekNumber = 0, bool mobiledevice = false)
        {
            int dayPerWeek = 7;
            if (mobiledevice)
            {
                weekNumber = 3 * weekNumber;
                dayPerWeek = 3;
            }
            else
            {
                weekNumber = 7 * weekNumber;
            }
            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow.AddDays(weekNumber));
            DateTime endTime = dtnow.AddDays(dayPerWeek);

            var calendars = _companyProfileService.GetSchedulerAvailabilityOfClass(profileId, serviceId, employeeId, dtnow.ToString("MM-dd-yyyy hh:mm"), endTime.ToString("MM-dd-yyyy hh:mm"));

            var calendarResult = GetObjetCalendar(calendars, dtnow, dayPerWeek);

            return Json(calendarResult, JsonRequestBehavior.AllowGet);
        }


        private List<WeekObject> GetObjetCalendar(List<SchedulerAvailability> schedulerAvailability, DateTime dtNow, int dayPerWeek)
        {
            List<WeekObject> weekObject = new List<WeekObject>();

            for (int i = 0; i < dayPerWeek; i++)
            {
                string dayInWeek = dtNow.AddDays(i).ToString("dddd");
                if (i == 0 && dtNow.Date == DateTime.Now.Date)
                {
                    dayInWeek = "Today";
                }

                WeekObject timslotPerDay = new WeekObject();
                int dayOfWeek = (int)dtNow.AddDays(i).DayOfWeek;
                timslotPerDay.DayOfWeek = dayOfWeek;
                timslotPerDay.Day = dayInWeek;
                timslotPerDay.DateTime = dtNow.AddDays(i).ToString("MMM d, yyyy");

                var classIndays = schedulerAvailability.Where(q => q.DayOfWeek == dayOfWeek).OrderBy(o => o.FromHour).ToList();

                foreach (var item in classIndays)
                {
                    DateTime start = dtNow.AddDays(i).Date.AddTicks(item.FromHour.Ticks);
                    string isFull = "full";

                    if (item.IsAvailability && start < dtNow)
                    {
                        isFull = "expired";
                    }

                    string btnReserve = string.Empty;
                    if (start >= dtNow.AddMinutes(10) && item.IsAvailability)
                    {
                        isFull = string.Empty;
                        btnReserve = string.Format("<a id=\"reserve\" classSchedulerId =\"{0}\" startTime=\"{1}\" emdTime=\"{2}\" className= \"{3}\" instructorName=\"{4}\" class=\"reserve\"  href=\"javascript:void(0);\">reserve</a>",
                            item.ClassSchedulerID, start.ToString("MM/dd/yyyy hh:mm tt"), start.AddMinutes(item.Duration).ToString("MM/dd/yyyy hh:mm tt"), item.ServiceName, item.EmployeeName);
                    }

                    CalendarObject calendarObject = new CalendarObject
                    {
                        id = item.ServiceCompanyID.ToString(),
                        className = item.ServiceName,
                        duration = item.Duration,
                        employeeName = item.EmployeeName,
                        title = new DateTime(item.FromHour.Ticks).ToString("h:mm tt").ToLower(),
                        start = new DateTime(item.FromHour.Ticks).ToString("h:mm tt").ToLower(),
                        end = new DateTime(item.FromHour.Ticks).AddMinutes(item.Duration).ToString("h:mm tt").ToLower()

                    };
                    timslotPerDay.TimeSlot.Add(calendarObject);
                }
                weekObject.Add(timslotPerDay);

            }

            return weekObject;
        }


        public ActionResult GetCalendarAvailability(int classSchedulerId, string startDate)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var classInfor = _appointmentService.GetDataCheckOutOfClass(classSchedulerId);
            dict.Add("classInfor", classInfor);
            //Parse start time
            DateTime startTime = DateTime.ParseExact(startDate, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);
            DateTime endTime = startTime.AddMinutes(classInfor.Duration);
            dict.Add("startTime", startTime.ToString("MM/dd/yyyy hh:mm tt"));
            dict.Add("endTime", endTime.ToString("MM/dd/yyyy hh:mm tt"));
            StringBuilder htmlCalendar = new StringBuilder();
            List<Kuyam.Database.Calendar> listCal = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            var calAppointment = _appointmentService.GetAppointmentByCalendarId(0, startTime);
            var calendarAvailability = listCal.Where(m => (calAppointment == null || calAppointment.Count() <= 0 || !calAppointment.Any(n => n.CalendarID == m.CalendarID)));
            foreach (var cal in calendarAvailability)
            {
                htmlCalendar.AppendFormat("<option value=\"{0}\">{1}</option>", cal.CalendarID, UtilityHelper.TruncateText(cal.Name, 10));
            }
            dict.Add("calendar", htmlCalendar.ToString());

            return Json(dict, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        public ActionResult Description(int? id)
        {
            var model = new ProfileCompaniesModels();
            if (id.HasValue)
            {
                model.ProfileId = id.Value;
                var profileCompany = _companyProfileService.GetProfileCompanyByID(id ?? 0);
                ViewBag.Company = profileCompany;
                model.ProfileCompany = profileCompany;
                model.CompanyName = profileCompany.Name;
                model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Photo(int? id)
        {
            var model = new ProfileCompaniesModels();
            if (id.HasValue && id.Value > 0)
            {
                model.ProfileId = id.Value;
                var profileCompany = _companyProfileService.GetProfileCompanyByID(id ?? 0);
                model.ProfileCompany = profileCompany;
                model.CompanyName = profileCompany.Name;
                model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
                model.MediaCompanies = _companyProfileService.GetCompanyMediums(id ?? 0, Types.CompanyMediaType.IsBanner);
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Review(int? id)
        {
            var model = new ProfileCompaniesModels();
            if (id.HasValue && id.Value > 0)
            {
                model.ProfileId = id.Value;
                var profileCompany = _companyProfileService.GetProfileCompanyByID(id ?? 0);
                model.ProfileCompany = profileCompany;
                model.CompanyName = profileCompany.Name;
                model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
                int totalRecord = 0;
                ViewBag.RatingList = _appointmentService.GetRatingListByProfileID(id ?? 0, 1, 10, out totalRecord);
                ViewBag.TotalRecords = totalRecord;
                ViewBag.Page = 1;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Review(int id, int page)
        {
            int profileID = id;
            int pageIndex = page;
            int totalRecord = 0;
            ViewBag.RatingList = _appointmentService.GetRatingListByProfileID(profileID, pageIndex, 10, out totalRecord);
            ViewBag.Page = pageIndex;
            ViewBag.TotalRecords = totalRecord;
            var model = new ProfileCompaniesModels();
            return PartialView("_RatingList", model);

        }


        [Authorize]
        public ActionResult Package(int? id)
        {
            MySession.IsBookDirect = false;
            var model = new ProfileCompaniesModels();
            ViewBag.CompanyPackage = string.Empty;
            if (id.HasValue && id.Value > 0)
            {
                model.ProfileId = id.Value;
                var packages = _companyProfileService.GetCompanyPackages(id.Value);
                ViewBag.CompanyPackage = GetCompanyPackages(packages);
                var profileCompany = _companyProfileService.GetProfileCompanyByID(id ?? 0);
                model.ProfileCompany = profileCompany;
                model.CompanyName = profileCompany.Name;
                model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            }
            return View(model);
        }

        public ActionResult GetServiceByPackageId(int packageId)
        {
            var services = _companyProfileService.GetServiceByPackageId(packageId);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            StringBuilder html = new StringBuilder();
            if (services != null && services.Count > 0)
            {
                var companypackage = services[0].CompanyPackage;
                dict.Add("packagename", string.Format("{0}&#8216;{1}&#8217; details", (companypackage.NumberOfBooking > 0 ? companypackage.NumberOfBooking.ToString() + " " : ""), companypackage.PackageName));
                dict.Add("expiresdate", CaculatorExpiresDate(companypackage.DurationInMonth.Value));
            }

            html.Append("<div id=\"pkgdetailscroll\">");
            html.Append("<div class=\"pkgpcontent\" id=\"pkgdetailcontent\">");
            bool odd = false;
            foreach (var item in services)
            {
                if (odd)
                {
                    html.Append("<div class=\"pkgpitem\">");
                }
                else
                {
                    html.Append("<div class=\"pkgpitem1\">");
                }
                html.AppendFormat("<div class=\"pkgdetailcontent\">{0}, {1}min, ${2}, {3} person</div>", Kuyam.Domain.UtilityHelper.TruncateText(item.ServiceCompany.Service.ServiceName, 30), item.ServiceCompany.Duration, item.ServiceCompany.Price, item.ServiceCompany.AttendeesNumber);
                html.Append("<div class=\"clear\"> </div>");
                html.Append("</div><div class=\"clear\"></div>");
                odd = !odd;
            }
            html.Append("</div></div>");
            dict.Add("content", html.ToString());

            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserPuchasedByPackageId(int packageId)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            StringBuilder html = new StringBuilder();
            int countPurchased = 0;

            var services = _companyProfileService.GetServiceByPackageId(packageId);
            var userPackagepurchased = _companyProfileService.GetUserPackagePurchaseByPackageId(packageId);

            // get package name
            string packageName = string.Empty;
            if (services != null && services.Count > 0)
            {
                packageName = services[0].CompanyPackage.PackageName;
            }
            dict.Add("packagename", string.Format("&#8216;{0}&#8217; details", packageName));


            if (userPackagepurchased != null && userPackagepurchased.Count > 0)
            {
                countPurchased = userPackagepurchased.Count;
            }


            html.Append("<div class=\"pkgpcontent\" id=\"pkgpcontent\">");
            bool odd = false;
            foreach (var item in userPackagepurchased)
            {
                if (odd)
                {
                    if (item.CompanyPackage.Status == (int)Types.CompanyPackageStatus.Active)
                    {
                        html.Append("<div class=\"pkgpitem\">");
                    }
                    else
                    {
                        html.Append("<div class=\"pkgpitem pkgpdepleted\">");
                    }
                }
                else
                {
                    if (item.CompanyPackage.Status == (int)Types.CompanyPackageStatus.Active)
                    {
                        html.Append("<div class=\"pkgpitem1\">");
                    }
                    else
                    {
                        html.Append("<div class=\"pkgpitem1 pkgpdepleted\">");
                    }
                }
                var cust = DAL.xGetCust(item.CustID);
                html.AppendFormat("<div class=\"pkgpname\">{0}</div>", string.Format("{0} {1}", cust.FirstName, cust.LastName));
                html.AppendFormat("<div class=\"pkgpqtyleft\">{0}</div>", (item.MaxUses > 0 ? item.MaxUses.ToString() : "unlimited"));
                html.AppendFormat("<div class=\"pkgpstartdate\">{0}</div>", DateTimeUltility.ConvertToUserTime(item.PurchaseDate, DateTimeKind.Utc).ToString("dd/MM/yy"));
                html.AppendFormat("<div class=\"pkgpexpirydate\">{0}</div>", DateTimeUltility.ConvertToUserTime(item.ExpiredDate.Value, DateTimeKind.Utc).ToString("dd/MM/yy"));
                html.Append("<div class=\"clear\"> </div>");

                html.Append("</div><div class=\"clear\"></div>");
                odd = !odd;
            }
            html.Append("</div>");
            dict.Add("content", html.ToString());
            dict.Add("totalPurchased", countPurchased.ToString());

            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCalendars(int profileId = 0, int companyType = 0, int serviceId = 0, int employeeId = 0, int calendarId = 0, int weekNumber = 0, bool mobiledevice = false)
        {

            int dayPerWeek = 7;
            if (mobiledevice)
            {
                weekNumber = 3 * weekNumber;
                dayPerWeek = 3;
            }
            else
            {
                weekNumber = 7 * weekNumber;
            }
            DateTime startDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow.AddDays(weekNumber));
            DateTime endDate = startDate.AddDays(dayPerWeek);

            if (MySession.AppointmentbookingTempId > 0)
            {
                ProfileCompany.DeleteAppointmentTemp(MySession.AppointmentbookingTempId);
                MySession.AppointmentbookingTempId = 0;
            }

            if (companyType == (int)Types.CompanyType.KuyamBookIt || companyType == (int)Types.CompanyType.KuyamInstantBook || companyType == (int)Types.CompanyType.HybridKuyamBookIt)
            {
                return Json(GetData(startDate, endDate, employeeId, serviceId, calendarId, profileId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetGeneralAvailability(startDate, endDate, employeeId, calendarId, profileId), JsonRequestBehavior.AllowGet);
            }
        }


        //Trong edit
        [HttpGet]
        public ActionResult GetPackageInfo(int packageId)
        {

            CompanyProfileService _companyProfileService = Kuyam.Repository.Infrastructure.EngineContext.Current.Resolve<CompanyProfileService>();
            UserPackagePurchase userPackage = _companyProfileService.GetUserPackagePurchasePackageID(packageId);

            CompanyPackageObject package = new CompanyPackageObject();

            if (userPackage != null && userPackage.CompanyPackage != null)
            {
                CompanyPackageService companyPackageService = _companyProfileService.GetCompanyPackageServiceByCompanyPackageId(userPackage.CompanyPackage.PackageId);
                package.PackageName = userPackage.CompanyPackage.PackageName;
                package.PackageDesscription = userPackage.CompanyPackage.Description;
                package.PackagePrice = userPackage.CompanyPackage.Price.ToString();
                package.PackageID = userPackage.CompanyPackageId;
                package.ProfileId = companyPackageService.ServiceCompany.ProfileID;
                if (companyPackageService != null && companyPackageService.CompanyPackage != null && companyPackageService.CompanyPackage.ProfileCompany != null)
                {
                    package.PackageCompanyName = companyPackageService.CompanyPackage.ProfileCompany.Name;
                }
                if (userPackage.CompanyPackage.NumberOfBooking == 0)
                {
                    package.PackageRemain = "-1";
                }
                else
                {
                    package.PackageRemain = string.Format("{0}", userPackage.MaxUses.ToString());
                }
                package.PackageExpiredDate = String.Format("{0:MM/dd/yy}", userPackage.ExpiredDate);
            }
            return Json(package, JsonRequestBehavior.AllowGet);
        }

        #endregion View Availability

        #region MyRegion

        [HttpPost]
        public ActionResult AddRequestAppointment(RequestAppointmentModel model, [ModelBinderAttribute(typeof(CommaModelBinder))] string[] timeSlots)
        {
            try
            {
                var listTimeslots = new List<RequestAppointment>();
                foreach (var item in timeSlots)
                {
                    string[] timeslot = item.Split('|');

                    var requestApp = new RequestAppointment
                    {
                        ServiceCompanyId = model.ServiceId,
                        ProfileId = model.ProfileId,
                        CustID = MySession.CustID,
                        CalendarId = model.CalendarId,
                        Start = DateTime.Parse(timeslot[0]),
                        End = DateTime.Parse(timeslot[1]),
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        Status = (int)Types.RequestAppoitmentStatus.Default

                    };

                    if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                        && MySession.Concierge != null && MySession.HotelId > 0)
                    {
                        requestApp.HotelID = MySession.HotelId;
                        requestApp.StaffID = MySession.Concierge.Id;
                    }

                    listTimeslots.Add(requestApp);
                }
                var result = _appointmentService.AddRequestAppointment(listTimeslots);

                if (!result)
                    return Json("", JsonRequestBehavior.AllowGet);
                model.CustID = MySession.CustID;
                AddRequestToZendeskTicket(model, listTimeslots, "request appointment");

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private void AddRequestToZendeskTicket(RequestAppointmentModel model, List<RequestAppointment> requestAppointment, string subject)
        {
            string url = ConfigurationManager.AppSettings["WebHost"];

            if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
            {
                subject = "[QA] " + subject;

            }
            else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
            {
                subject = "[DEV] " + subject;

            }

            var status = (TicketStatus.Pending);
            var type = (TicketType.Incident);
            var priority = (TicketPriority.High);

            StringBuilder description = new StringBuilder();
            var company = _companyProfileService.GetProfileCompanyByID(model.ProfileId);
            var cust = DAL.xGetCust(model.CustID);
            if (cust != null)
            {
                description.AppendFormat("first name and last name: {0} {1} \n", cust.FirstName, cust.LastName);
                description.AppendFormat("email: {0} \n", cust.Email);
                description.AppendFormat("company name: {0} \n", company.Name);
                description.AppendFormat("company address: {0} \n", company.Address);
                string timeSlot = string.Empty;
                foreach (var item in requestAppointment)
                {
                    timeSlot += string.Format("{0} from {1} to {2}, ", item.Start.ToString("dddd, MMM dd"), item.Start.ToString("hh:mmtt"), item.End.ToString("hh:mmtt"));
                }
                description.AppendFormat("Requested time slots: {0} \n", timeSlot);

                var service = _companyProfileService.GetCompanyServiceById(model.ServiceId ?? 0);
                if (service != null)
                {
                    description.AppendFormat("Requested service: {0}, ${1:#.00}, {2} minutes \n", service.Service.ServiceName, service.Price, service.Duration);

                }
                var calendar = _appointmentService.GetCalendarIdById(model.CalendarId ?? 0);
                if (calendar != null)
                {
                    description.AppendFormat("calendar name: {0} \n", calendar.Name);
                }
                foreach (var item in requestAppointment)
                {
                    description.AppendFormat("Link to the specific appointment request: {0}Admin/RequestAppointmentDetails?Id={1}&page=1 \n", url, item.Id);
                }

                if (description.Length > 0)
                    ZenAPI.CreateTicket(subject, status, type, priority, description.ToString());
            }

        }


        #endregion

    }

}
