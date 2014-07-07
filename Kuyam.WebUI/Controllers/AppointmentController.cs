using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using M2.Util;
using Kuyam.Domain;
using Kuyam.Repository.Infrastructure;
using Kuyam.Utility;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;
using System.Configuration;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Helpers;
using Kuyam.Database.Extensions;
using System.Text;
using System.Data;
using System.Threading;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    [Authorize]
    public class AppointmentController : KuyamBaseController
    {
        private const string COMPANYCALENDAR_EMPID = "COMPANYCALENDAR_EMPID";

        private readonly IAppointmentService _appointmentService;
        private readonly EmailSender _emailSender;
        private readonly OrderService _orderService;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly PdfService _pdfService;
        private const string APPOINTMENTOBJECT = "APPOINTMENTOBJECT";
        private readonly NotificationService _notificationService;

        public AppointmentController(IAppointmentService appointmentService, EmailSender emailSender,
            IFormsAuthenticationService formsService, IMembershipService membershipService,
            CompanyProfileService companyProfileService,
            OrderService orderService,
             PdfService pdfService,
            NotificationService notificationService)
        {
            this._appointmentService = appointmentService;
            this._emailSender = emailSender;
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._orderService = orderService;
            this._companyProfileService = companyProfileService;
            this._pdfService = pdfService;
            this._notificationService = notificationService;
        }

        #region utility

        private DateTime GetFirstDayOfMonth(DateTime dtDate)
        {
            DateTime dtFrom = dtDate;
            dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));
            return dtFrom;
        }

        private DateTime GetLastDayOfMonth(DateTime dtDate)
        {
            DateTime dtTo = dtDate;
            dtTo = dtTo.AddMonths(1);
            dtTo = dtTo.AddDays(-(dtTo.Day));
            return dtTo;

        }

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

        #endregion

        public ActionResult Index(int id = 0)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, 0, id);
            ViewBag.Id = id;
            ViewBag.CalendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            ViewBag.Category = _appointmentService.GetListService();
            ViewBag.HtmlData = Generation(result);
            return View();

        }

        public ActionResult Proposed(int? calendarId)
        {
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            int totalRecord = 0;
            int pageSize = 10;
            int pageIndex = 1;
            var listResult = _appointmentService.GetProposedAppointmentsByCustId(MySession.CustID, calendarId, today, pageIndex, pageSize, out totalRecord);
            ViewBag.CalendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            ViewBag.Category = _appointmentService.GetListService();
            ViewBag.HtmlData = ProposedGeneration(listResult);
            if (Request.IsAjaxRequest())
            {
                return Json(ProposedGeneration(listResult), JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        private string ProposedGeneration(List<ProposedAppointment> ProposedApt)
        {
            StringBuilder html = new StringBuilder();
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            if (ProposedApt == null || ProposedApt.Count() <= 0)
                return string.Empty;
            DateTime start = DateTime.MinValue;

            // for (int i = 0; i < 7; i++)

            foreach (var item in ProposedApt)
            {
                if (item.Start.Date == start.Date)
                    continue;

                bool flag = false;
                string date = string.Empty;

                List<ProposedAppointment> newList = ProposedApt.Where(a => a.Start.Date == item.Start.Date).ToList();

                for (int j = 0; j < newList.Count; j++)
                {

                    int appointmentId = newList[j].AppointmentID;
                    int calendarId = newList[j].CalendarId ?? 0;
                    string turnClass = "nonturn";
                    string btnClass = "btnconfirmed";
                    string btnValue = "book now";
                    string lblClass = "newrequest";
                    string lblValue = "proposed";
                    string modifyClass = string.Empty;
                    string eventstring = string.Format("getProposedDataCheckout({0})", appointmentId);

                    //if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    //{
                    //    btnClass = "btnconfirmed hidea";
                    //    btnValue = "confirm";
                    //    lblClass = "newrequest";
                    //    lblValue = "pending";
                    //    eventstring = string.Format("void('{0}')", 0);
                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    //{
                    //    btnClass = "pending hidea";
                    //    btnValue = "pending";
                    //    lblClass = "modified";
                    //    lblValue = "modified";
                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    //{
                    //    btnClass = "confirm";
                    //    btnValue = "confirm";
                    //    lblClass = "modified";
                    //    lblValue = "modified";
                    //    eventstring = string.Format("confirm('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Confirmed);

                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                    //{
                    //    btnClass = "remove";
                    //    btnValue = "remove";
                    //    lblClass = "cancelled";
                    //    lblValue = "cancelled";
                    //    eventstring = string.Format("remove1('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Delete);
                    //    eventCancel = string.Empty;
                    //    cancelBtnClass = "hidea";
                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)
                    //{
                    //    btnClass = "btnconfirmed hidea";
                    //    btnValue = "confirmed!";
                    //    lblClass = "confirmed";
                    //    lblValue = "confirmed";
                    //    modifyClass = "hidea";
                    //    eventstring = string.Empty;
                    //    eventmodify = string.Empty;
                    //    hrfModify = "javascript:void(0)";
                    //}

                    //if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    //{
                    //    turnClass = "yourturn";
                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    //{
                    //    turnClass = "theirturn";
                    //}
                    //else if (newList[j].AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    //{
                    //    turnClass = "yourturn";
                    //}

                    var companyname = newList[j].ServiceCompany != null ? newList[j].ServiceCompany.ProfileCompany.Name : newList[j].ProfileCompany.Name;
                    //var employeename = newList[j].CompanyEmployee != null ? newList[j].CompanyEmployee.EmployeeName : newList[j].Appointment.EmployeeName;
                    var servicenName = newList[j].ServiceCompany != null ? newList[j].ServiceCompany.Service.ServiceName : newList[j].Service.ServiceName;
                    var duration = newList[j].ServiceCompany != null ? newList[j].ServiceCompany.Duration : newList[j].Duration;
                    var attendeesNumber = newList[j].ServiceCompany != null ? newList[j].ServiceCompany.AttendeesNumber : newList[j].AttendeesNumber;
                    string lastClass = string.Empty;
                    if (!flag)
                    {
                        if (today.Date == item.Start.Date)
                        {
                            date = string.Format("today, {0}", String.Format("{0: MMM d}", today));
                        }
                        else
                        {
                            date = String.Format("{0:dddd, MMM d}", item.Start.Date);
                        }
                        html.Append("<div class=\"boxlist\">");
                        html.AppendFormat("<div class=\"title\">{0}</div>", date.ToLower());
                        html.Append("<div class=\"clear\"> </div>");


                    }
                    if (j == newList.Count() - 1)
                        lastClass = "last";
                    int profileId = newList[j].ProfileId;
                    Medium CompanyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);

                    html.AppendFormat("<div class=\"item {0}\">", lastClass);
                    html.Append("<div class=\"colinfo\">");

                    html.AppendFormat("<div class=\"colinfotitle\"><div class=\"hours\">{0}</div><div class=\"divname\"> <div title={2} class=\"name\">for {1}</div> <div class=\"imgtime\"></div></div></div>", String.Format("{0:t}", newList[j].Start).ToLower().Replace(" ", ""), Kuyam.Domain.UtilityHelper.TruncateAtWord(newList[j].Calendar.Name, 5), newList[j].Calendar.Name);

                    html.Append("<div class=\"clear\"> </div>");

                    html.AppendFormat("<div class=\"turn\"><div class=\"{0}\">&nbsp;</div><div class=\"{1}\">{4}</div> <div class=\"button\"> <input type=\"button\" onclick=\"{5};\" title=\"\" value=\"{3}\" class=\"{2}\" /> </div></div>", turnClass, lblClass, btnClass, btnValue, lblValue, eventstring);

                    html.Append("<div class=\"clear\"></div>");
                    html.AppendFormat("<div class=\"coltime\"><div class=\"ago\">{0}</div></div>", DateTimeUltility.RelativeDatetime(newList[j].Modified ?? DateTime.UtcNow));

                    html.Append("<div class=\"clear\"></div>");
                    html.Append("</div>");

                    html.Append("<div class=\"coldescription\">");
                    if (CompanyMedia == null)
                    {
                        html.Append("<div class=\"divimg\"><span class=\"boxcontentimg\"><span class=\"boximage\"><img src=\"/Images/placeholder.png\" title=\"no logo\" alt=\"no logo\" width=\"86px;\" height=\"83px;\" /></span></span></div>");
                    }
                    else
                    {
                        html.AppendFormat("<div class=\"divimg\"><img alt=\"{1}\" title=\"{1}\" src=\"{2}/p/811441/thumbnail/entry_id/{0}/width/109/height/107\" /></div>", CompanyMedia != null ? CompanyMedia.LocationData : string.Empty, newList[j].ServiceCompany != null ? newList[j].ServiceCompany.ProfileCompany.Name : newList[j].ProfileCompany.Name, Types.KaturaDoman);
                    }

                    html.Append("<div class=\"colcontent\">");
                    html.AppendFormat("<h2> {0} with {1}</h2><div class=\"clear\"></div>", companyname, Kuyam.Domain.UtilityHelper.TruncateAtWord(string.Empty, 17));
                    html.AppendFormat("<div class=\"description\">{0}<br /> {1}min, ${2}, {3} person</div>", Kuyam.Domain.UtilityHelper.TruncateAtWord(servicenName, 17), duration, newList[j].Price, attendeesNumber);
                    //html.AppendFormat("<div class=\"viewnote\"><div class=\"lnkviewnote\"><a href=\"javascript:void(0);\"  onclick=\"viewnote('{0}')\" id=\"btnviewnote\">view notes</a> </div><div class=\"nonviewnote\"></div></div>", appointmentId);
                    html.Append("</div></div>");
                    //end item
                    html.Append("</div>");
                    html.Append(" <div class=\"clear\"></div>");

                    flag = true;


                }

                if (flag)
                {
                    html.Append("</div><div class=\"clear14\"></div>");
                }

                start = item.Start.Date;

            }

            return html.ToString();
        }

        public ActionResult fillterByCalendar(int calendarId, bool flag)
        {

            if (flag)
            {
                List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, calendarId);
                return Json(GenerationAppointment(result), JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, calendarId);
                return Json(Generation(result), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult fillterByStatus(int status, bool flag)
        {
            var result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, 0, status);
            if (flag)
            {
                return Json(GenerationAppointment(result), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(Generation(result), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult fillterByAll(int calendarId, int status, DateTime? startDate, int serviceid)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, startDate, false, calendarId, status, serviceid);
            return Json(Generation(result), JsonRequestBehavior.AllowGet);
        }

        private string Generation(List<AppointmentParticipant> listapt)
        {
            StringBuilder html = new StringBuilder();
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            if (listapt == null || listapt.Count() <= 0)
                return string.Empty;

            DateTime start = listapt[0].Appointment.Start.Date;
            for (int i = 0; i < 7; i++)
            {
                bool flag = false;
                string date = string.Empty;

                List<AppointmentParticipant> newList = listapt.Where(a => a.Appointment.Start.Date == start.Date).ToList();

                for (int j = 0; j < newList.Count; j++)
                {

                    int appointmentId = newList[j].Appointment.AppointmentID;
                    var isClass = newList[j].Appointment.ClassSchedulerID.HasValue && newList[j].Appointment.ClassSchedulerID.Value > 0;
                    int calendarId = newList[j].CalendarID;
                    string turnClass = "nonturn";
                    string btnClass = "confirm";
                    string btnValue = "confirm";
                    string lblClass = "newrequest";
                    string lblValue = "confirmed";
                    string modifyClass = string.Empty;
                    string eventstring = string.Empty;
                    string eventmodify = string.Format("onclick=\"modify('{0}');", appointmentId);
                    string hrfModify = string.Format("/appointment/modify?calendarId={0}&appointmentId={1}", calendarId, appointmentId);
                    string eventCancel = string.Format("onclick=\"cancel('{0}');\"", appointmentId);
                    string cancelBtnClass = string.Empty;

                    if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    {
                        btnClass = "btnconfirmed hidea";
                        btnValue = "confirm";
                        lblClass = "newrequest";
                        lblValue = "pending";
                        eventstring = string.Format("void('{0}')", 0);
                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    {
                        btnClass = "pending hidea";
                        btnValue = "pending";
                        lblClass = "modified";
                        lblValue = "modified";
                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    {
                        btnClass = "confirm";
                        btnValue = "confirm";
                        lblClass = "modified";
                        lblValue = "modified";
                        eventstring = string.Format("confirm('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Confirmed);

                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                    {
                        btnClass = "remove";
                        btnValue = "remove";
                        lblClass = "cancelled";
                        lblValue = "cancelled";
                        eventstring = string.Format("remove1('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Delete);
                        eventCancel = string.Empty;
                        cancelBtnClass = "hidea";
                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)
                    {
                        btnClass = "btnconfirmed hidea";
                        btnValue = "confirmed!";
                        lblClass = "confirmed";
                        lblValue = "confirmed";
                        modifyClass = "hidea";
                        eventstring = string.Empty;
                        eventmodify = string.Empty;
                        hrfModify = "javascript:void(0)";
                    }

                    if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    {
                        turnClass = "yourturn";
                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    {
                        turnClass = "theirturn";
                    }
                    else if (newList[j].Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    {
                        turnClass = "yourturn";
                    }

                    var companyname = newList[j].Appointment.ServiceCompany != null ? newList[j].Appointment.ServiceCompany.ProfileCompany.Name : newList[j].Appointment.ProfileCompany.Name;
                    var employeename = newList[j].Appointment.CompanyEmployee != null ? newList[j].Appointment.CompanyEmployee.EmployeeName : newList[j].Appointment.EmployeeName;
                    var servicenName = newList[j].Appointment.ServiceCompany != null ? newList[j].Appointment.ServiceCompany.Service.ServiceName : newList[j].Appointment.ServiceName;
                    var duration = newList[j].Appointment.ServiceCompany != null ? newList[j].Appointment.ServiceCompany.Duration : newList[j].Appointment.Duration;
                    var attendeesNumber = newList[j].Appointment.ServiceCompany != null ? newList[j].Appointment.ServiceCompany.AttendeesNumber : newList[j].Appointment.AttendeesNumber;
                    string lastClass = string.Empty;
                    if (!flag)
                    {
                        if (today.Date == start.Date)
                        {
                            date = string.Format("today, {0}", String.Format("{0: MMM d}", today));
                        }
                        else
                        {
                            date = String.Format("{0:dddd, MMM d}", start);
                        }
                        html.Append("<div class=\"boxlist\">");
                        html.AppendFormat("<div class=\"title\">{0}</div>", date.ToLower());
                        html.Append("<div class=\"clear\"> </div>");


                    }
                    if (j == newList.Count() - 1)
                        lastClass = "last";
                    int profileId = newList[j].Appointment.ProfileId ?? newList[j].Appointment.ServiceCompany.ProfileCompany.ProfileID;
                    Medium CompanyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);

                    html.AppendFormat("<div class=\"item {0}\">", lastClass);
                    html.Append("<div class=\"colinfo\">");

                    html.AppendFormat("<div class=\"colinfotitle\"><div class=\"hours\">{0}</div><div class=\"divname\"> <div title={2} class=\"name\">for {1}</div> <div class=\"imgtime\"></div></div></div>", String.Format("{0:t}", newList[j].Appointment.Start).ToLower().Replace(" ", ""), Kuyam.Domain.UtilityHelper.TruncateAtWord(newList[j].Calendar.Name, 8), newList[j].Calendar.Name);

                    html.Append("<div class=\"clear\"> </div>");

                    html.AppendFormat("<div class=\"turn\"><div class=\"{0}\">&nbsp;</div><div class=\"{1}\">{4}</div> <div class=\"button\"> <input type=\"button\" onclick=\"{5};\" title=\"\" value=\"{3}\" class=\"{2}\" /> </div></div>", turnClass, lblClass, btnClass, btnValue, lblValue, eventstring);

                    html.Append("<div class=\"clear\"></div>");
                    if(!isClass)
                    {
                        html.AppendFormat("<div class=\"coltime\"><div class=\"ago\">{0}</div><div class=\"modify\"><a href=\"{1}\" title=\"modify\" class=\"{2}\" id=\"btnmodify\">modify</a> <a href=\"javascript:void(0);\" {3} title=\"cancel\" id=\"btncancel\" class=\"{4}\">cancel</a></div></div>", DateTimeUltility.RelativeDatetime(newList[j].Appointment.Modified ?? DateTime.UtcNow), hrfModify, modifyClass, eventCancel, cancelBtnClass);

                        html.Append("<div class=\"clear\"></div>");
                    }
                    else
                    {
                        html.AppendFormat("<div class=\"coltime\"><div class=\"ago\">{0}</div><div class=\"modify\"><a href=\"{1}\" title=\"modify\" class=\"{2}\" style=\"display:none;\" id=\"btnmodify\">modify</a> <a href=\"javascript:void(0);\" {3} title=\"cancel\" id=\"btncancel\" class=\"{4}\">cancel</a></div></div>", DateTimeUltility.RelativeDatetime(newList[j].Appointment.Modified ?? DateTime.UtcNow), hrfModify, modifyClass, eventCancel, cancelBtnClass);

                        html.Append("<div class=\"clear\"></div>");
                    }
                    
                    html.Append("</div>");

                    html.Append("<div class=\"coldescription\">");
                    if (CompanyMedia == null)
                    {
                        html.Append("<div class=\"divimg\"><span class=\"boxcontentimg\"><span class=\"boximage\"><img src=\"/Images/placeholder.png\" title=\"no logo\" alt=\"no logo\" width=\"86px;\" height=\"83px;\" /></span></span></div>");
                    }
                    else
                    {
                        html.AppendFormat("<div class=\"divimg\"><img alt=\"{1}\" title=\"{1}\" src=\"{2}/p/811441/thumbnail/entry_id/{0}/width/109/height/107\" /></div>", CompanyMedia != null ? CompanyMedia.LocationData : string.Empty, newList[j].Appointment.ServiceCompany != null ? newList[j].Appointment.ServiceCompany.ProfileCompany.Name : newList[j].Appointment.ProfileCompany.Name, Types.KaturaDoman);
                    }

                    html.Append("<div class=\"colcontent\">");
                    html.AppendFormat("<h2> {0} with {1}</h2><div class=\"clear\"></div>", companyname, Kuyam.Domain.UtilityHelper.TruncateAtWord(employeename, 17));
                    html.AppendFormat("<div class=\"description\">{0}<br /> {1}min, ${2}, {3} person</div>", Kuyam.Domain.UtilityHelper.TruncateAtWord(servicenName, 30), duration, newList[j].Appointment.Price, attendeesNumber);
                    html.AppendFormat("<div class=\"viewnote\"><div class=\"lnkviewnote\"><a href=\"javascript:void(0);\"  onclick=\"viewnote('{0}')\" id=\"btnviewnote\">view notes</a> </div><div class=\"nonviewnote\"></div></div>", appointmentId);
                    html.Append("</div></div>");
                    //end item
                    html.Append("</div>");
                    html.Append(" <div class=\"clear\"></div>");

                    flag = true;


                }

                if (flag)
                {
                    html.Append("</div><div class=\"clear14\"></div>");
                }

                start = start.AddDays(1);

            }

            return html.ToString();
        }

        public ActionResult Modify()
        {
            var calendar = Request.Params["calendarId"];
            var appointment = Request.Params["appointmentId"];
            int appointmentId = 0;
            int calendarId = 0;
            int.TryParse(appointment, out appointmentId);
            int.TryParse(calendar, out calendarId);
            var appoiment = _appointmentService.GetAppointmentParticipantByAppointmentId(appointmentId);
            var calendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            ViewBag.Appoiment = appoiment;
            ViewBag.calendarList = calendarList;
            ViewBag.CalendarId = calendarId;
            ViewBag.AppointmentId = appointmentId;
            int employeeId = 0;

            if (appoiment.Appointment.EmployeeID != null)
                employeeId = appoiment.Appointment.EmployeeID.Value;

            int profileId = 0;
            var company = appoiment.Appointment.ProfileCompany ?? appoiment.Appointment.ServiceCompany.ProfileCompany;
            profileId = company.ProfileID;
            Medium CompanyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);
            ViewBag.EmployeeId = employeeId;
            ViewBag.SeviceEmployee = ProfileCompany.GetServiceCompanybyEmployeeId(profileId, employeeId, 0);
            ViewBag.EmployeeName = appoiment.Appointment.EmployeeName;
            if (CompanyMedia != null)
                ViewBag.LogoId = CompanyMedia.LocationData;
            return View();
        }

        [HttpGet]
        public ActionResult GetCalendars()
        {
            var start = Request.Params["start"];
            var end = Request.Params["end"];
            var calendar = Request.Params["calendarId"];
            var appointment = Request.Params["appointmentId"];
            var employee = Request.Params["employeeId"];
            DateTime startDate = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(start));
            DateTime endDate = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(end));

            int appointmentId = 0;
            int calendarId = 0;
            int employeeId = 0;
            int.TryParse(appointment, out appointmentId);
            int.TryParse(calendar, out calendarId);
            int.TryParse(employee, out employeeId);
            return Json(GetData(startDate, endDate, calendarId, appointmentId, employeeId), JsonRequestBehavior.AllowGet);
        }


        private List<CalendarObject> GetData(DateTime start, DateTime end, int calendarId, int appointmentId, int employeeId)
        {
            List<CalendarObject> calendarObject = new List<CalendarObject>();
            string resutl = string.Empty;
            try
            {
                DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
                DateTime beginDay = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

                var appoiment = _appointmentService.GetAppointmentByID(appointmentId);

                var calAppointment = _appointmentService.GetAppointmentByCalendarId(calendarId, dtNow, end);
                var lstAppointments = _appointmentService.GetAppoinmentsByEmployeeId(employeeId, dtNow, end);
                if (employeeId == 0)
                    employeeId = appoiment != null ? appoiment.EmployeeID.Value : 0;
                var serviceHour = GetEventList(employeeId);
                int duration = 1;

                if (calAppointment != null)
                {
                    foreach (var apt in calAppointment)
                    {
                        CalendarObject item = new CalendarObject
                        {
                            id = apt.CustID.ToString(),
                            title = apt.ServiceCompany.ProfileCompany.Name,
                            start = apt.Start.ToString(),
                            end = apt.End.ToString(),
                            className = "fc-red",
                            currentAppointment = apt.AppointmentID.ToString()
                        };
                        if (apt.AppointmentID == appointmentId)
                        {
                            item.currentAppointment = appointmentId.ToString();
                            item.className = "fc-red  fc-event-skin-active";
                            //TimeSpan diffResult = appoiment.End.Subtract(appoiment.Start);
                        }
                        calendarObject.Add(item);
                    }
                }

                if (serviceHour != null && lstAppointments != null)
                {
                    for (var index = 0; index < 7; index++)
                    {
                        if (!serviceHour.Any(m => m.DayOfWeek == index))
                            continue;
                        int dayOfWeek = (int)dtNow.DayOfWeek;
                        int detDay = 7 - dayOfWeek;
                        int day = 0;
                        if (index >= dayOfWeek)
                        {
                            day = index - dayOfWeek;
                        }
                        else
                        {
                            day = index + detDay;
                        }
                        DateTime currentDate = beginDay.Date.AddDays(day);
                        for (int i = 0; i < 24; i++)
                        {
                            string cssClass = string.Empty;
                            DateTime expectedDate = currentDate.AddHours(i);
                            EventCustom eventCustom = serviceHour.FirstOrDefault(x => x.DayOfWeek == index && x.Start <= expectedDate && x.End >= expectedDate);

                            DateTime beginDate = currentDate.AddHours(i);
                            DateTime endDate = currentDate.AddHours(i + 1);

                            if (eventCustom != null && (beginDate > dtNow))
                            {
                                if (eventCustom.Start <= beginDate && endDate <= eventCustom.End || (endDate > eventCustom.End && beginDate < eventCustom.End))
                                {
                                    DateTime enddt = (eventCustom.End <= endDate) ? eventCustom.End : endDate;
                                    CalendarObject item = new CalendarObject
                                    {
                                        id = eventCustom.EmployeeID.ToString(),
                                        title = eventCustom.title,
                                        start = beginDate.ToString(),
                                        end = enddt.ToString(),
                                        duration = duration,
                                        className = "fc-green"
                                    };

                                    var apt = lstAppointments.FirstOrDefault(m => m.Start <= beginDate && m.End >= endDate);
                                    if (apt == null)
                                    {
                                        var aptend = lstAppointments.FirstOrDefault(m => m.End >= beginDate && m.Start <= beginDate);
                                        if (aptend != null)
                                        {
                                            item.start = aptend.End.ToString();
                                            beginDate = aptend.End;
                                        }
                                        calendarObject.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                resutl = string.Empty;
            }

            return calendarObject;
        }

        private List<EventCustom> GetEventList(int employeeId)
        {
            var eventCustom = new List<EventCustom>();
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);

            var lstEmployeeHour = _appointmentService.GetListEmployeeHour(employeeId);

            if (lstEmployeeHour != null && lstEmployeeHour.Count > 0)
            {


                DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
                int dayOfWeek = (int)dtnow.DayOfWeek;
                int detDay = 7 - dayOfWeek;
                int day = 0;

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
                    dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                    int fromMinute = item.FromHour.Hours * 60 + item.FromHour.Minutes;
                    int toMinute = item.ToHour.Hours * 60 + item.ToHour.Minutes;
                    eventCustom.Add(new EventCustom
                    {
                        EmployeeID = item.CompanyEmployee.EmployeeID,
                        title = item.CompanyEmployee.ProfileCompany.Name,
                        EmployeeName = item.CompanyEmployee.EmployeeName,
                        DayOfWeek = (int)dt.DayOfWeek,
                        Start = dt.AddMinutes(fromMinute),
                        End = dt.AddMinutes(toMinute),
                        ClassCustom = string.Empty
                    });
                }
            }


            return eventCustom;
        }

        public ActionResult GetAppointmentInfo(int appointmentId)
        {
            var appoiment = _appointmentService.GetAppointmentParticipantByAppointmentId(appointmentId);
            if (appoiment == null)
                return Json("", JsonRequestBehavior.AllowGet);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            StringBuilder optionCalendar = new StringBuilder();
            StringBuilder optionEmployee = new StringBuilder();
            StringBuilder optionService = new StringBuilder();
            var calendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            int profileId = 0;
            if (appoiment != null && appoiment.Appointment != null
                && appoiment.Appointment.ServiceCompany != null
                && appoiment.Appointment.ServiceCompany.ProfileCompany != null)
                profileId = appoiment.Appointment.ServiceCompany.ProfileCompany.ProfileID;

            string selected = string.Empty;
            foreach (Kuyam.Database.Calendar item in calendarList)
            {
                if (item.CalendarID == appoiment.CalendarID)
                    selected = "selected=selected";
                else
                    selected = string.Empty;
                optionCalendar.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", item.CalendarID, selected, item.Name);
            }

            List<CompanyEmployee> listEmployee = null;
            listEmployee = appoiment.Appointment.ServiceCompany.ProfileCompany.CompanyEmployees.ToList();
            selected = string.Empty;
            int employeeId = appoiment.Appointment.EmployeeID.Value;
            foreach (CompanyEmployee item in listEmployee)
            {
                if (item.EmployeeID == employeeId)
                    selected = "selected=selected";
                else
                    selected = string.Empty;
                optionEmployee.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", item.EmployeeID, selected, item.EmployeeName);
            }
            List<CompanyService> companyServicelist = ProfileCompany.GetServiceCompanybyEmployeeId(profileId, employeeId, 0);
            selected = string.Empty;
            foreach (CompanyService item in companyServicelist)
            {
                if (item.ID == appoiment.Appointment.ServiceCompanyID)
                    selected = "selected=selected";
                else
                    selected = string.Empty;
                optionService.AppendFormat("<option value=\"{0}\" {1}>{2}, {3}min, ${4}...</option>", item.ID, selected, item.ServiceName, item.Duration, item.Price);

            }
            dict.Add("calendar", optionCalendar.ToString());
            dict.Add("employee", optionEmployee.ToString());
            dict.Add("service", optionService.ToString());
            Medium CompanyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);
            AppointmentExt appt = new AppointmentExt
            {
                appointmentID = appoiment.AppointmentID,
                appointmentStatusID = appoiment.Appointment.AppointmentStatusID,
                calendarname = appoiment.Calendar.Name,
                employeename = appoiment.Appointment.CompanyEmployee.EmployeeName,
                companyname = appoiment.Appointment.CompanyEmployee.ProfileCompany.Name,
                servicename = appoiment.Appointment.ServiceCompany.Service.ServiceName,
                url = string.Format("{1}/p/811441/thumbnail/entry_id/{0}/width/109/height/107", CompanyMedia != null ? CompanyMedia.LocationData : string.Empty, Types.KaturaDoman),
                startdate = string.Format("{0:MMM dd}", appoiment.Appointment.Start).ToLower(),
                hour = string.Format("{0}", appoiment.Appointment.Start.ToString("h:mm tt").ToLower()),
                sevicedescripton = string.Format("{0}min, ${1} {2} person", appoiment.Appointment.ServiceCompany.Duration, appoiment.Appointment.ServiceCompany.Price, appoiment.Appointment.ServiceCompany.AttendeesNumber)

            };
            dict.Add("appointment", appt);
            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int appointmentID, int status)
        {
            Appointment apt = _appointmentService.ChangeStatus(appointmentID, status);
            DateTime dt = DateTime.Today.AddDays(7);
            List<AppointmentParticipant> result = null;

            if (status != 0)
            {
                string subject = string.Empty;
                if ((Types.AppointmentStatus)status == Types.AppointmentStatus.CompanyModified)
                {
                    subject = "Confirmed";
                    if (apt.ServiceCompany != null && apt.ServiceCompany.ProfileCompany != null)
                    {
                        string Key = apt.PreapprovalKey;
                        string EmailSender = apt.SenderEmail;
                        int countMinutes = apt.ServiceCompany.ProfileCompany.CancelHour.HasValue ? apt.ServiceCompany.ProfileCompany.CancelHour.Value * 60 : 0;
                        int payDefault = 0;
                        int.TryParse(ConfigManager.PayDate, out payDefault);

                        DateTime paidDate = countMinutes != 0 ? apt.Start.AddMinutes(-countMinutes) : apt.Start.AddMinutes(-payDefault * 60);
                        decimal paidPrice = 0;
                        decimal paymentFeeTotal = 0;
                        decimal kuyamFeeTotal = 0;
                        var order = _orderService.GetOrderByAppointmentId(appointmentID);
                        if (order != null)
                        {
                            order.PaidDateUtc = paidDate;
                            order.OrderStatusID = (int)Types.AppointmentStatus.Confirmed;
                            paidPrice = order.OrderTotal.Value;
                            paymentFeeTotal = order.PaymentFeeTotal.Value;
                            kuyamFeeTotal = order.KuyamFeeTotal.Value;
                            _orderService.UpdateOrder(order);
                        }
                        decimal companyCharge = paidPrice - kuyamFeeTotal;

                        InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                        IncomingRequest obj = new IncomingRequest
                        {
                            EntityId = "KuyamWeb",
                            DateAlert = paidDate,
                            Data = Kuyam.Domain.UtilityHelper.ObjectToXml(
                            new
                            {
                                PreapprovalKey = Key,
                                EmailSender = EmailSender,
                                CompanyChargePrice = companyCharge,
                                KuyamChargePrice = kuyamFeeTotal,
                                CompanyEmailReceive = apt.ServiceCompany.ProfileCompany.PaymentOptions,
                                KuyamEmailReceice = MyApp.Settings.PaySetting.PaypalAccount
                            })
                        };
                        serviceInfo.AddIncomingRequest(obj, IncommingRequestType.PAYMENT_PAYPAL);
                    }

                }

                if (MySession.ImpersonateId > 0 && MySession.OriginalCustIfImpersonated != null)
                {
                    EmailHelper.SendAppointmentNotifyToCompanyOrAdmin(apt, Types.NotifyType.Confirm, this);
                }
                else
                {
                    try
                    {
                        //_emailSender.SendEmail(subject, subject, EmailHelper.EmailSystem, EmailHelper.NameSystem, toEmail, MySession.Cust.FullName);
                    }
                    catch
                    {
                        result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
                        ViewBag.appoitnmentlist = result;
                        return Json(result);
                    }
                }
            }

            result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
            var flag = Request.Params["flag"];

            if (flag != null && flag.Equals("true"))
            {
                return Json(GenerationAppointment(result));
            }
            else
            {
                return Json(Generation(result));
            }
        }
        private string GetEmailTemplateAppointment(int appointmentID, string TemplateName, object obj = null)
        {
            string templateResult = string.Empty;

            Appointment appointment = _appointmentService.GetAppointmentByID(appointmentID);

            string userName = string.Empty;
            string email = string.Empty;
            string employeeName = string.Empty;
            string companyName = string.Empty;
            string serviceName = string.Empty;
            string serviceDetail = string.Empty;
            string appointmentTime = string.Empty;
            string appointmentDate = string.Empty;
            string payAfter = string.Empty;
            string companyID = string.Empty;

            string serviceNameOld = string.Empty;
            string serviceDetailOld = string.Empty;
            string appointmentTimeOld = string.Empty;
            string appointmentDateOld = string.Empty;
            string employeeNameOld = string.Empty;

            if (obj != null)
            {

                employeeNameOld = obj.GetPropValue("EmployeeNameOld").ToString();
                serviceNameOld = obj.GetPropValue("ServiceNameOld").ToString();
                serviceDetailOld = obj.GetPropValue("ServiceDetailOld").ToString();
                appointmentDateOld = obj.GetPropValue("AppointmentDateOld").ToString();
                appointmentTimeOld = obj.GetPropValue("AppointmentTimeOld").ToString();
            }

            if (appointment.Cust != null)
            {
                userName = appointment.Cust.FirstName;
            }

            if (appointment != null)
            {
                appointmentTime = appointment.Start.ToString("h:mm tt");
                appointmentDate = appointment.Start.ToString("ddd, MMM dd");
            }

            serviceDetail = string.Format("{0}min, ${1}, {2} person", appointment.Duration, appointment.Price, appointment.AttendeesNumber);
            serviceName = appointment.ServiceName;

            companyName = appointment.ProfileCompany.Name;
            email = appointment.ProfileCompany.Email;
            companyID = appointment.ProfileId.ToString();

            if (appointment.ProfileCompany.CancelPolicy == (int)Types.CompanyPolicies.Strict)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to <strong>strict.</strong><br />you cancelled <strong>less than 72 hours</strong> before the appointment time so you will be charged <strong>50% of the Total Fees.</strong>");
            }
            else if (appointment.ProfileCompany.CancelPolicy == (int)Types.CompanyPolicies.Standard)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to <strong>standard</strong>.<br />since you cancelled <strong>more than 24 hours</strong> before the appointment time,you will not be charged.");
            }
            else if (appointment.ProfileCompany.CancelPolicy == (int)Types.CancellationType.None)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to none so you'll not be charged");
            }
            else
            {
                payAfter = string.Format("this company’s cancellation policy is set to <strong>custom.</strong><br />you cancelled <strong>less than {0} hours</strong> before the appointment time so you will be charged <strong>{1}% of the Total Fees.</strong>", appointment.ProfileCompany.CancelHour, 100 - (int)appointment.ProfileCompany.CancelRefundPercent);
            }

            employeeName = appointment.EmployeeName;

            // create template data
            dynamic myObject = new
            {
                Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                Host = Kuyam.WebUI.Helpers.EmailHelper.GetStoreHost(),
                UserName = appointment.Cust.FirstName.ToString(),
                Email = email,
                EmployeeName = employeeName,
                CompanyName = companyName,
                ServiceName = serviceName,
                ServiceDetail = serviceDetail,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime.ToLower(),
                PayAfterText = payAfter,
                CompanyID = companyID,

                EmployeeNameOld = employeeNameOld,
                ServiceNameOld = serviceNameOld,
                ServiceDetailOld = serviceDetailOld,
                AppointmentDateOld = appointmentDateOld,
                AppointmentTimeOld = appointmentTimeOld
            }.ToExpando();

            // generate the content using razor engine                   
            templateResult = this.RenderPartialViewToString(TemplateName, (object)myObject);
            return templateResult;
        }

        [HttpPost]
        public ActionResult DeleteAppointment(int appointmentID, string reason, string pass)
        {
            if (string.IsNullOrEmpty(MySession.Cust.FacebookUserID))
            {
                if (string.IsNullOrEmpty(pass))
                {
                    return Json(1, JsonRequestBehavior.AllowGet);
                }
                if (!_membershipService.ValidateUser(MySession.Cust.Username, pass))
                {
                    return Json(2, JsonRequestBehavior.AllowGet);
                }
            }

            Appointment appointment = _appointmentService.ChangeStatus(appointmentID, (int)Types.AppointmentStatus.Cancelled, reason);
            DateTime dt = DateTime.Today.AddDays(7);
            List<AppointmentParticipant> result = null;
            Appointment apt = _appointmentService.GetAppointmentByID(appointmentID);

            var profileCompany = apt.ServiceCompany != null ? apt.ServiceCompany.ProfileCompany : apt.ProfileCompany;

            try
            {
                //Create Zendesk ticket here -- Khoi Tran
                //Information for the ticket   
                List<AppointmentLog> Notes = apt.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                string methodCommunication = string.Empty;
                string subject = "";
                string companyEmail = string.Empty;
                string companySMS = string.Empty;
                int? preferedContact = null;

                if (profileCompany.PreferredContact.HasValue)
                    preferedContact = profileCompany.PreferredContact.Value;
                if (preferedContact != null && ((preferedContact.Value == 1)))
                {
                    methodCommunication = "Email";
                    companyEmail = profileCompany.Email;
                }
                else if (preferedContact != null && ((preferedContact.Value == 2)))
                {
                    methodCommunication = "SMS";
                    companySMS = profileCompany.Phone;
                }

                else if (preferedContact != null && ((preferedContact.Value == 3)))
                {
                    methodCommunication = "SMS&Email";
                    companySMS = profileCompany.Phone;
                }
                else
                {
                    methodCommunication = "not specified";
                }

                AppointmentParticipant appart = EngineContext.Current.Resolve<IAppointmentService>().GetAppointmentParticipantByAppointmentId(appointmentID);
                //var appoimentPart = _appointment.GetAppointmentParticipantByAppointmentId(apt.AppointmentID);
                subject = "Appointment " + apt.AppointmentID + " has beeen cancelled";

                if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                    subject = "[QA] " + subject;
                else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                    subject = "[DEV] " + subject;

                TicketStatus status = (TicketStatus.Pending);
                TicketType type = (TicketType.Incident);
                TicketPriority priority = (TicketPriority.High);
                string description = "appointment information: \n"
                    + "company name: " + profileCompany.Name.ToLower() + "\n"
                    + "company contact info: " + "\n";
                if (profileCompany.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                {
                    if (methodCommunication == "Email")
                    {
                        description += "   email:	" + profileCompany.Email.ToLower() + "\n";
                    }
                    else if (methodCommunication == "SMS")
                    {
                        description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(profileCompany.Phone) + "\n";
                    }
                    else if (methodCommunication == "SMS&Email")
                    {
                        description += "   email:	" + profileCompany.Email + "\n"
                                    + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(profileCompany.Phone) + "\n";
                    }
                }
                else if (profileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt)
                {
                    description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(profileCompany.Phone) + "\n";
                }

                //basic info
                var employeeName = apt.CompanyEmployee != null ? apt.CompanyEmployee.EmployeeName : apt.EmployeeName;
                var serviceName = apt.ServiceCompany != null ? apt.ServiceCompany.Service.ServiceName : apt.ServiceName;
                var duration = apt.ServiceCompany != null ? apt.ServiceCompany.Duration : apt.Duration;
                var price = apt.ServiceCompany != null ? apt.ServiceCompany.Price : apt.Price;
                description += "primary contact: " + profileCompany.ContactFirstName + " " + profileCompany.ContactLastName + "\n"
                            + "employee name: " + employeeName + "\n"
                            + "calendar: " + appart.Calendar.Name + "\n"
                            + "service: " + serviceName + "\n"
                            + "duration: " + duration + "\n"
                            + "price: " + string.Format("${0:0.00}", price) + "\n"
                            + "date: " + string.Format("{0:MMM dd yyyy}", apt.Start) + "\n"
                            + "time: " + apt.Start.ToString("h:mm tt") + " - " + apt.End.ToString("h:mm tt") + "\n"
                            + "appointment status: " + (Types.AppointmentStatus.Cancelled).ToString() + "\n"
                            + "appointment notes: " + "\n";
                description += "\t" + appointment.Notes + "\n";
                if (Notes != null && Notes.Count > 0)
                {
                    foreach (var note in Notes)
                    {
                        description += "\t" + note.Message.Replace("<br/>", "\n") + "\n";
                    }
                }
                int group_id = Int32.Parse(ConfigurationManager.AppSettings["groupid"]);

                Thread zendeskThread = new Thread(() =>
                {
                    ZenAPI.CreateTicket(subject, status, type, priority, description.ToLower());
                    //End of creating ticket
                });

                zendeskThread.Start();
            }
            catch (Exception ex)
            {
                //Todo: Handle Exception Occur
                LogHelper.Error("Delete appointment fail:", ex);
            }

            string emailTo = string.Empty;
            string emailFrom = string.Empty;

            if (apt.ServiceCompany != null && apt.ServiceCompany.ProfileCompany != null)
            {

                emailTo = apt.ServiceCompany.ProfileCompany.Email;
            }

            if (MySession.ImpersonateId > 0 && MySession.OriginalCustIfImpersonated != null)
            {
                emailFrom = MySession.OriginalCustIfImpersonated.Email;
                EmailHelper.SendAppointmentNotifyToCompanyOrAdmin(apt, Types.NotifyType.Cancel, this);
            }
            else
            {
                emailFrom = MySession.Cust.Email;
                try
                {
                    if ((profileCompany.PreferredContact.Value & (int)Types.PreferredPhone.Email) != 0)
                    {                        
                        EmailHelper.SendEmailCancelAppointment(emailFrom, emailTo, GetEmailTemplateAppointment(appointmentID, "usercancel"));

                    }
                }
                catch
                {
                    result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
                    ViewBag.appoitnmentlist = result;
                    return PartialView("_AppointmentList");
                }
            }


            result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
            ViewBag.appoitnmentlist = result;

            var flag = Request.Params["flag"];
            if (flag.Equals("true"))
            {
                return Json(GenerationAppointment(result), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(Generation(result), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadAppointmentLeftTab()
        {
            List<Appointment> lstAppointment = _appointmentService.GetListAppointmentByCustID(MySession.CustID);
            ViewBag.lstAppoitment = lstAppointment;
            return PartialView("_LoadAppointmentLeftTab");
        }


        public ActionResult LoadAppointment(string appointmentID, string profileID)
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            int id = 0;
            int companyid = 0;
            int.TryParse(appointmentID, out id);
            int.TryParse(profileID, out companyid);
            Appointment appointment = _appointmentService.GetAppointmentByID(id);
            int employeeID = appointment.EmployeeID.HasValue ? appointment.EmployeeID.Value : 0;
            List<CompanyEmployee> listEmployee = _appointmentService.GetCompanyEmployeeByCompanyID(companyid);
            List<ServiceCompany> listService = _appointmentService.GetListEmployeeServiceByEmployeeID(employeeID);
            ViewBag.appointment = appointment;
            ViewBag.listEmployee = listEmployee;
            ViewBag.listService = listService;
            return PartialView("_ModifyAppointmentPopup");
        }

        public ActionResult LoadCompanyAppointment(string appointmentID)
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            int id = 0;
            int.TryParse(appointmentID, out id);
            Appointment appointment = _appointmentService.GetAppointmentByID(id);
            var company = appointment.ProfileCompany ?? appointment.ServiceCompany.ProfileCompany;
            int companyid = company.ProfileID;
            int employeeID = appointment.EmployeeID.HasValue ? appointment.EmployeeID.Value : 0;
            List<CompanyEmployee> listEmployee = _appointmentService.GetCompanyEmployeeByCompanyID(companyid);
            List<ServiceCompany> listService = _appointmentService.GetListEmployeeServiceByEmployeeID(employeeID);
            ViewBag.appointment = appointment;
            ViewBag.listEmployee = listEmployee;
            ViewBag.listService = listService;
            return PartialView("_ModifyAppointmentPopup");
        }

        public ActionResult LoadAppointmentCancelPopup(string appointmentID)
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            int id = 0;
            int.TryParse(appointmentID, out id);
            AppointmentParticipant appointment = _appointmentService.GetAppointmentParticipantByAppointmentId(id);
            ViewBag.appointment = appointment.Appointment;
            ViewBag.CalendarName = Kuyam.Domain.UtilityHelper.TruncateAtWord(appointment.Calendar.Name, 15);
            ViewBag.UserType = Types.CustType.Personal;
            if (!string.IsNullOrEmpty(appointment.Appointment.Cust.FacebookUserID))
                ViewBag.UserType = Types.CustType.Facebook;
            return PartialView("_CancelAppointmentPopup");
        }

        [HttpGet]
        public ActionResult LoadAppointmentNotePopup(string appointmentID)
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int id = 0;
            int.TryParse(appointmentID, out id);
            List<AppointmentLog> appointmentlog = _appointmentService.GetAppointmentNoteByID(id);
            Appointment appointment = _appointmentService.GetAppointmentByID(id);
            ViewBag.AppointmentNotes = appointmentlog;
            ViewBag.Appointment = appointment;
            ViewBag.AptCustId = appointment.CustID;
            ViewBag.AppointmentID = appointmentID;
            return PartialView("_ViewAppointmentNotesPopup");
        }

        [HttpPost]
        public ActionResult AddNote(string appointmetnId, string message)
        {
            int id = 0;
            int.TryParse(appointmetnId, out id);

            Appointment appointment = _appointmentService.GetAppointmentByID(id);

            AppointmentLog item = new AppointmentLog();
            item.Viewed = false;
            item.Message = message.Replace("\n", "<br/>");
            item.AppointmentID = id;
            item.CustID = MySession.CustID;
            item.LogDT = DateTime.UtcNow;
            _appointmentService.AddNote(item);

            _notificationService.SendAppointmentNote(item, appointment.CustID);

            List<AppointmentLog> appointmentnotes = _appointmentService.GetAppointmentNoteByID(id);
            ViewBag.appointmentNotes = appointmentnotes;

            ViewBag.Appointment = appointment;
            return PartialView("_LoadNoteData");
            //return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadAppointmentReviewPopup(string appointmentID)
        {
            int id = 0;
            int.TryParse(appointmentID, out id);
            Appointment appointment = _appointmentService.GetAppointmentByID(id);
            if (appointment != null)
            {
                //ViewBag.Companyname = (appointment.ServiceCompany != null && appointment.ServiceCompany.ProfileCompany != null) ? appointment.ServiceCompany.ProfileCompany.Name : "";
                ViewBag.AppointmentID = id;
                ViewBag.ServiceCompanyID = (appointment.ServiceCompany != null) ? appointment.ServiceCompany.ServiceCompanyID : 0;
                ViewBag.Appointment = appointment;
            }
            return PartialView("_AddReviewPopup");
        }

        public ActionResult LoadNonAppointmentByFoUser(int custId, int profileId)
        {
            var appointment = _appointmentService.GetLastNonKuyamAppointment(custId, profileId);
            if (appointment == null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            StringBuilder html = new StringBuilder();
            html.AppendFormat("<div id=\"apptdetail\" class=\"colleft\" apptid=\"{8}\"><p><strong>{0}</strong></p><p>{1}</p><p>{2}min {3}</p></div><div class=\"colright\"><p> for {4}</p><p>{5} - {6}</p><p>Total: ${7}</p></div>",
                appointment.EmployeeName, appointment.Start.ToString("ddd, MMM dd").ToLower(), appointment.Duration, appointment.Service.ServiceName, appointment.Calendar.Name,
                appointment.Start.ToString("hh:mm tt").ToLower(), appointment.End.ToString("hh:mm tt").ToLower(), appointment.Price, appointment.AppointmentID);

            return Json(html.ToString(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataCheckoutByNonKuyamByAppId(int apptId)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var appointment = _appointmentService.GetLastNonKuyamAppointmentById(apptId);
            StringBuilder html = new StringBuilder();
            if (appointment != null)
            {
                html.Append("<div class=\"contentPopup\">");
                html.Append("<a class=\"btnClose\" href=\"JavaScript:void(0);\" onclick=\"cancelbooking();\" title=\"Close\"></a>");
                html.Append("<div class=\"newcheckout\">");
                html.Append("<div class=\"newcheckoutcol1\">");
                html.Append("<h3> appointment summary:</h3>");
                html.Append("<div class=\"clear8\"></div>");
                html.Append("<div class=\"blueinfo\">");
                html.Append(" <div class=\"brow\">");
                html.AppendFormat("<div class=\"bcol1 companyName\" style=\"text-transform: capitalize;\" title=\"{0}\">{0}</div>", appointment.ProfileCompany.Name);
                html.Append("<div class=\"bcol2\"> &nbsp;</div>");
                html.Append(" <div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("<div class=\"brow\">");
                html.AppendFormat("<div class=\"bcol1 employeeName\" style=\"text-transform: capitalize;\" title=\"{0}\">{0}</div>", appointment.EmployeeName);
                html.AppendFormat("<div class=\"bcol2\" title=\"{1}\">for {0}</div>", Kuyam.Domain.UtilityHelper.TruncateAtWord(appointment.Calendar.Name, 12), appointment.Calendar.Name);
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("<div class=\"brow\">");
                html.AppendFormat("<div class=\"bcol1\">{0}</div>", String.Format("{0:ddd MMM d}", appointment.Start));
                html.AppendFormat("<div class=\"bcol2\">{0} - {1}</div>", String.Format("{0:t}", appointment.Start).Replace(" ", string.Empty).ToLower(), String.Format("{0:t}", appointment.End).Replace(" ", string.Empty).ToLower());
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("<div class=\"brow\">");
                html.AppendFormat("<div class=\"bcol1\" title=\"{0}\">{0}</div>", string.Format("{0} {1}min", UtilityHelper.TruncateAtWord(appointment.Service.ServiceName, 12), appointment.Duration));
                html.AppendFormat("<div class=\"bcol2\">{0}</div>", string.Format("${0}, {1} person", appointment.Price, appointment.AttendeesNumber));
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("<div class=\"confirmarea\">");
                html.Append("<h3>how would you like confirmation?</h3>");
                html.Append("<div class=\"clear16\"></div>");
                html.Append("<div class=\"chkboxemail\">");
                html.Append("<input type=\"checkbox\" id=\"cbemail\" name=\"verification\" checked=\"checked\" />");
                html.Append("<label class=\"cbcheck\" for=\"cbemail\">email?</label>");
                html.Append("</div>");
                html.Append("<div class=\"chkboxsms\">");
                html.Append("<input type=\"checkbox\" id=\"cbsms\" name=\"verification\" />");
                html.Append("<label class=\"smscheck\" for=\"cbsms\"> SMS?</label>");
                html.Append("</div></center>");
                html.Append(" <div class=\"clear10\"></div>");
                html.Append("<h3>appointment notes?</h3>");
                html.Append("<div class=\"clear h7\"></div>");
                html.Append("<textarea cols=\"\" id=\"txtCustomerScheduleLog\" rows=\"\" onfocus=\"if (this.value=='e.g. i have lower back pain') {this.value = '';}\" onblur=\"if (this.value==''){this.value='e.g. i have lower back pain';}\" value=\"e.g. i have lower back pain\" >e.g. i have lower back pain</textarea>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"clear12\"></div>");
                //html.Append("<div class=\"timecontentpopup\">");
                //html.Append("<div class=\"lefttime countdownclock\">9:45</div>");
                //html.Append("<div class=\"ptime\">please complete your request in 10:00 minutes. otherwise this time will be released to others.</div>");
                //html.Append("</div>");
                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");
                html.Append("<div class=\"newcheckoutcol2\">");

                html.Append("<div class=\"checkoutpopup2\">");
                html.Append("<div class=\"paymentmethod\">");
                html.Append("<div class=\"divtotaldue\">");

                html.AppendFormat("<div class=\"totaldue\"><span>total due:</span><br />{0:C}</div>", appointment.Price);
                html.AppendFormat("<input type=\"hidden\" id=\"totaldue\" value=\"{0}\"/><input type=\"hidden\" id=\"price\" value=\"{0}\"/><input type=\"hidden\" id=\"discoutamountapply\" value=\"\"/><input id=\"discoutidapply\" type=\"hidden\" value=\"\"></div>", appointment.Price);
                html.Append("<div class=\"clear11\"></div>");
                html.Append("<div class=\"discountcodebox\"><div class=\"textdiscount\">promo code?</div><div class=\"clear\"></div>");
                html.Append("<div><input type=\"button\" id=\"btnapply\" class=\"btnapply\" value=\"apply\" title=\"apply\" /><input type=\"text\" id=\"txtdiscount\" class=\"txtdiscount\" /></div></div>");
                html.Append("<div class=\"clear\"></div><div id=\"applycodeError\" style=\"color:Red;\">&nbsp;</div>");
                html.Append("</div><div class=\"clear5\"></div>");

                html.Append("<div class=\"newpaymentmethod\"><div> please select your <br> method of payment:</div>");
                html.Append("</div><div class=\"clear16\"></div>");

                html.Append("<div class=\"buttonbox\">");
                html.Append("<div class=\"divpaypal\">");
                html.Append("<a href=\"javascript:void(0);\" onclick=\"nonKuyambook();\" title=\"paypal\"><img alt=\"\" src=\"/images/btnpaypal.png\" /></a>");
                html.Append("</div><div class=\"clear14\"></div>");

                html.Append("<div class=\"divamazon\">");
                //if (appointment.ProfileCompany.PaymentMethod == (int)Types.PaymentMethod.PayInPerson)
                //{
                html.Append("<a href=\"javascript:void(0);\" onclick=\"cashPayment();\" title=\"pay in person\"> <img alt=\"\" src=\"/images/btnpayperson.png\" /></a>");
                //}

                html.Append("</div>");
                html.Append("<div class=\"clear\"></div></div>");

                html.Append("<div class=\"clear\"></div></div>");
                //end div paymentmethod

                html.Append("<div class=\"clear\"></div>");
                html.Append("</div></div></div></div>");
            }
            dict.Add("content", html.ToString());

            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadServiceEmployee(string companyEmployeeID)
        {
            int id = 0;
            int.TryParse(companyEmployeeID, out id);
            List<ServiceCompany> listService = _appointmentService.GetListEmployeeServiceByEmployeeID(id);
            List<SetrviceModel> service = new List<SetrviceModel>();
            if (listService != null)
            {
                foreach (ServiceCompany item in listService)
                {
                    SetrviceModel obj = new SetrviceModel();
                    obj.serviceid = item.ServiceCompanyID;
                    obj.servicename = string.Format("{0},{1}min, ${2}...", item.Service.ServiceName, item.Duration, item.Price);
                    service.Add(obj);
                }
            }
            return Json(service, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RequestModification(int appointmentId, int serviceId, int? employeeid, string employeeName, int calendarId, string date)
        {
            //Get old appointment
            object appointmentOld = GetAppointmentOld(appointmentId);

            DateTime dt = DateTime.UtcNow;
            dt = DateTime.ParseExact(date, "MMM dd h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            AppointmentParticipant apt = _appointmentService.GetAppointmentParticipantByAppointmentId(appointmentId);
            apt.CalendarID = calendarId;
            apt.Appointment.ServiceCompanyID = serviceId;
            apt.Appointment.EmployeeID = employeeid;
            apt.Appointment.EmployeeName = employeeName;
            long diff = dt.Ticks - apt.Appointment.Start.Ticks;
            apt.Appointment.Start = apt.Appointment.Start.AddTicks(diff);
            apt.Appointment.End = apt.Appointment.End.AddTicks(diff);
            //Check business for appointment            
            apt.Appointment.AppointmentStatusID = (int)Types.AppointmentStatus.Modified;
            apt.Appointment.Modified = DateTime.UtcNow;
            //Update appointment
            _appointmentService.RequestModification(apt.Appointment);

            if (MySession.ImpersonateId > 0 && MySession.OriginalCustIfImpersonated != null)
            {
                EmailHelper.SendAppointmentNotifyToCompanyOrAdmin(apt.Appointment, Types.NotifyType.Modify, this);
            }
            else
            {
                string emailFrom = string.Empty;
                string emailTo = string.Empty;
                if (apt.Appointment.ServiceCompany != null && apt.Appointment.ServiceCompany.ProfileCompany != null)
                {
                    emailTo = apt.Appointment.ServiceCompany.ProfileCompany.Email;
                }

                emailFrom = MySession.Cust.Email;

                if ((apt.Appointment.ServiceCompany.ProfileCompany.PreferredContact.Value & (int)Types.PreferredPhone.Email) != 0)
                {                    
                    EmailHelper.SendEmailModificationAppointment(emailFrom, emailTo, GetEmailTemplateAppointment(appointmentId, "modifysent", appointmentOld));
                }
            }

            try
            {
                Thread zendeskThread = new Thread(() =>
                {
                    //Create Zendesk ticket here -- Khoi Tran
                    //Information for the ticket   
                    List<AppointmentLog> Notes = apt.Appointment.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                    string methodCommunication = string.Empty;
                    string subject = "";
                    string companyEmail = string.Empty;
                    string companySMS = string.Empty;
                    int? preferedContact = null;
                    if (apt.Appointment.ServiceCompany.ProfileCompany.PreferredContact.HasValue) preferedContact = apt.Appointment.ServiceCompany.ProfileCompany.PreferredContact.Value;
                    if (preferedContact != null && ((preferedContact.Value == 1)))
                    {
                        methodCommunication = "Email";
                        companyEmail = apt.Appointment.ServiceCompany.ProfileCompany.Email;
                    }
                    else if (preferedContact != null && ((preferedContact.Value == 2)))
                    {
                        methodCommunication = "SMS";
                        companySMS = apt.Appointment.ServiceCompany.ProfileCompany.Phone;
                    }

                    else if (preferedContact != null && ((preferedContact.Value == 3)))
                    {
                        methodCommunication = "SMS&Email";
                        companySMS = apt.Appointment.ServiceCompany.ProfileCompany.Phone;
                    }
                    else
                    {
                        methodCommunication = "not specified";
                    }

                    var appoimentPart = _appointmentService.GetAppointmentParticipantByAppointmentId(apt.Appointment.AppointmentID);
                    subject = "Appointment " + apt.Appointment.AppointmentID + " has beeen modified";

                    if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                        subject = "[QA] " + subject;
                    else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                        subject = "[DEV] " + subject;

                    TicketStatus status = (TicketStatus.Pending);
                    TicketType type = (TicketType.Incident);
                    TicketPriority priority = (TicketPriority.High);
                    string description = "appointment information: \n"
                        + "company name: " + apt.Appointment.ServiceCompany.ProfileCompany.Name.ToLower() + "\n"
                        + "company contact info: " + "\n";
                    if (apt.Appointment.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                    {
                        if (methodCommunication == "Email")
                        {
                            description += "   email:	" + apt.Appointment.ServiceCompany.ProfileCompany.Email.ToLower() + "\n";
                        }
                        else if (methodCommunication == "SMS")
                        {
                            description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.Appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                        }
                        else if (methodCommunication == "SMS&Email")
                        {
                            description += "   email:	" + apt.Appointment.ServiceCompany.ProfileCompany.Email.ToLower() + "\n"
                                        + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.Appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                        }
                    }
                    else if (apt.Appointment.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt)
                    {
                        description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.Appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                    }

                    //basic info

                    description += "primary contact: " + apt.Appointment.ServiceCompany.ProfileCompany.ContactFirstName.ToLower() + " " + apt.Appointment.ServiceCompany.ProfileCompany.ContactLastName.ToLower() + "\n"
                                + "employee name: " + apt.Appointment.CompanyEmployee.EmployeeName.ToLower() + "\n"
                                + "calendar: " + appoimentPart.Calendar.Name.ToLower() + "\n"
                                + "service: " + apt.Appointment.ServiceCompany.Service.ServiceName.ToLower() + "\n"
                                + "duration: " + apt.Appointment.ServiceCompany.Duration + "\n"
                                + "price: " + string.Format("${0:0.00}", apt.Appointment.ServiceCompany.Price) + "\n"
                                + "date: " + string.Format("{0:MMM dd yyyy}", apt.Appointment.Start).ToLower() + "\n"
                                + "time: " + apt.Appointment.Start.ToString("h:mm tt").ToLower() + " - " + apt.Appointment.End.ToString("h:mm tt").ToLower() + "\n"
                                + "appointment status: " + (Types.AppointmentStatus.Modified).ToString().ToLower() + "\n"
                                 + "appointment notes: " + "\n";
                    description += "\t" + apt.Appointment.Notes + "\n";
                    if (Notes.Count > 0)
                    {
                        foreach (var note in Notes)
                        {
                            description += "\t" + note.Message.Replace("<br/>", "\n").ToLower() + "\n";
                        }
                    }

                    int group_id = Int32.Parse(ConfigurationManager.AppSettings["groupid"]);

                    ZenAPI.CreateTicket(subject, status, type, priority, description);
                    //End of creating ticket
                });
                zendeskThread.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in calling zendesk api ", ex);

                throw;
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get appointment Old
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        private object GetAppointmentOld(int appointmentId)
        {

            Appointment appointment = _appointmentService.GetAppointmentByID(appointmentId);

            string employeeName = string.Empty;
            string serviceName = string.Empty;
            string serviceDetail = string.Empty;
            string appointmentTime = string.Empty;
            string appointmentDate = string.Empty;

            if (appointment != null)
            {

                appointmentTime = appointment.Start.ToString("h:mm tt");
                appointmentDate = appointment.Start.ToString("ddd, MMM dd");
            }
            if (appointment.ServiceCompany != null && appointment.ServiceCompany.Service != null)
            {

                serviceDetail = string.Format("{0}min, ${1}, {2} person", appointment.ServiceCompany.Duration,
                    appointment.ServiceCompany.Price, appointment.ServiceCompany.AttendeesNumber);

                serviceName = appointment.ServiceCompany.Service.ServiceName;
            }

            if (appointment.CompanyEmployee != null)
            {

                employeeName = appointment.CompanyEmployee.EmployeeName;
            }

            object appointmentOld = new
            {

                EmployeeNameOld = employeeName,
                ServiceNameOld = serviceName,
                ServiceDetailOld = serviceDetail,
                AppointmentDateOld = appointmentDate,
                AppointmentTimeOld = appointmentTime.ToLower()
            };

            return appointmentOld;
        }

        [HttpPost]
        public ActionResult RequestAdminModification(int appointmentId, int serviceId, int employeeid, string date)
        {
            DateTime dt = DateTime.Now;
            //Get old appointment
            object appointmentOld = GetAppointmentOld(appointmentId);

            dt = DateTime.ParseExact(date, "MMM dd h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            Appointment apt = _appointmentService.GetAppointmentByID(appointmentId);
            apt.ServiceCompanyID = serviceId;
            apt.EmployeeID = employeeid;
            long diff = dt.Ticks - apt.Start.Ticks;
            apt.Start = apt.Start.AddTicks(diff);
            apt.End = apt.End.AddTicks(diff);
            //Check business for appointment            
            apt.AppointmentStatusID = (int)Types.AppointmentStatus.CompanyModified;
            apt.Modified = DateTime.UtcNow;
            _appointmentService.RequestModification(apt);
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, true);

            string emailFrom = string.Empty;
            string emailTo = string.Empty;

            if (apt.ServiceCompany != null && apt.Cust != null)
            {

                emailTo = apt.Cust.Username;
            }

            emailFrom = emailFrom = apt.ServiceCompany.ProfileCompany.Email;

            if (apt.StaffID != null)
            {                
                EmailHelper.SendEmailModificationAppointment(emailFrom, emailTo, GetEmailTemplateAppointment(appointmentId, "companymodifysent", appointmentOld));
                _notificationService.SendAppointmentChangeNotify(appointmentId);
            }
            else
            {
                EmailHelper.SendAppointmentNotifyToConcierge(apt, Types.NotifyType.Modify, this);
            }

            try
            {
                Thread zendeskThread = new Thread(() =>
                {
                    //Create Zendesk ticket here -- Khoi Tran
                    //Information for the ticket   
                    List<AppointmentLog> Notes = apt.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                    string methodCommunication = string.Empty;
                    string subject = "";
                    string companyEmail = string.Empty;
                    string companySMS = string.Empty;
                    int? preferedContact = null;
                    if (apt.ServiceCompany.ProfileCompany.PreferredContact.HasValue) preferedContact = apt.ServiceCompany.ProfileCompany.PreferredContact.Value;
                    if (preferedContact != null && ((preferedContact.Value == 1)))
                    {
                        methodCommunication = "Email";
                        companyEmail = apt.ServiceCompany.ProfileCompany.Email;
                    }
                    else if (preferedContact != null && ((preferedContact.Value == 2)))
                    {
                        methodCommunication = "SMS";
                        companySMS = apt.ServiceCompany.ProfileCompany.Phone;
                    }

                    else if (preferedContact != null && ((preferedContact.Value == 3)))
                    {
                        methodCommunication = "SMS&Email";
                        companySMS = apt.ServiceCompany.ProfileCompany.Phone;
                    }
                    else
                    {
                        methodCommunication = "not specified";
                    }

                    var appoimentPart = _appointmentService.GetAppointmentParticipantByAppointmentId(apt.AppointmentID);
                    subject = "Appointment " + apt.AppointmentID + " has beeen modified";

                    if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                        subject = "[QA] " + subject;
                    else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                        subject = "[DEV] " + subject;

                    TicketStatus status = (TicketStatus.Pending);
                    TicketType type = (TicketType.Incident);
                    TicketPriority priority = (TicketPriority.High);
                    string description = "appointment information: \n"
                        + "company name: " + apt.ServiceCompany.ProfileCompany.Name.ToLower() + "\n"
                        + "company contact info: " + "\n";
                    if (apt.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                    {
                        if (methodCommunication == "Email")
                        {
                            description += "   email:	" + apt.ServiceCompany.ProfileCompany.Email.ToLower() + "\n";
                        }
                        else if (methodCommunication == "SMS")
                        {
                            description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.ServiceCompany.ProfileCompany.Phone) + "\n";
                        }
                        else if (methodCommunication == "SMS&Email")
                        {
                            description += "   email:	" + apt.ServiceCompany.ProfileCompany.Email.ToLower() + "\n"
                                        + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.ServiceCompany.ProfileCompany.Phone) + "\n";
                        }
                    }
                    else if (apt.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt)
                    {
                        description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(apt.ServiceCompany.ProfileCompany.Phone) + "\n";
                    }

                    //basic info

                    description += "primary contact: " + apt.ServiceCompany.ProfileCompany.ContactFirstName.ToLower() + " " + apt.ServiceCompany.ProfileCompany.ContactLastName.ToLower() + "\n"
                                + "employee name: " + apt.CompanyEmployee.EmployeeName.ToLower() + "\n"
                                + "calendar: " + appoimentPart.Calendar.Name.ToLower() + "\n"
                                + "service: " + apt.ServiceCompany.Service.ServiceName.ToLower() + "\n"
                                + "duration: " + apt.ServiceCompany.Duration + "\n"
                                + "price: " + string.Format("${0:0.00}", apt.ServiceCompany.Price) + "\n"
                                + "date: " + string.Format("{0:MMM dd yyyy}", apt.Start).ToLower() + "\n"
                                + "time: " + apt.Start.ToString("h:mm tt").ToLower() + " - " + apt.End.ToString("h:mm tt").ToLower() + "\n"
                                + "appointment status: " + (Types.AppointmentStatus.Modified).ToString().ToLower() + "\n"
                                + "appointment notes: " + "\n";
                    description += "\t" + apt.Notes + "\n";
                    if (Notes.Count > 0)
                    {
                        foreach (var note in Notes)
                        {
                            description += "\t" + note.Message.Replace("<br/>", "\n").ToLower() + "\n";
                        }
                    }
                    int group_id = Int32.Parse(ConfigurationManager.AppSettings["groupid"]);

                    ZenAPI.CreateTicket(subject, status, type, priority, description);
                    //End of creating ticket
                });
                zendeskThread.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in calling zendesk api ", ex);
                throw;
            }

            ViewBag.appoitnmentlist = result;
            return PartialView("_AppointmentList");
        }

        [HttpPost]
        public ActionResult ViewedNote(string appointmentId)
        {
            int id = 0;
            int.TryParse(appointmentId, out id);
            List<AppointmentLog> appointmentnotes = _appointmentService.GetAppointmentNoteByID(id);
            foreach (AppointmentLog item in appointmentnotes)
            {
                if (item.CustID != MySession.CustID)
                {
                    item.Viewed = true;
                }
            }
            _appointmentService.ViewedNote(appointmentnotes);
            return Json("", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AddRating(int appointmentId, int serviceCompanyId, string content, int ratingValue)
        {
            Rating rating = new Rating()
            {
                ServiceCompanyID = serviceCompanyId,
                Content = content,
                AppointmentID = appointmentId,
                RatingValue = ratingValue,
                CreateDate = DateTime.UtcNow,
                CustID = MySession.CustID

            };
            _appointmentService.AddRating(rating);
            _appointmentService.UpdateAppointment(appointmentId, ratingValue);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddRatingForReviewOnHome(string content, string privateContent, int ratingValue)
        {

            AppointmentReviewModel apt = MySession.AppointmentReview;
            try
            {
                if (apt != null)
                {

                    Rating rating = new Rating()
                    {

                        ServiceCompanyID = apt.ServiceCompanyID,
                        Content = content,
                        PrivateContent = privateContent,
                        AppointmentID = apt.Id,
                        RatingValue = ratingValue,
                        CreateDate = DateTime.UtcNow,
                        CustID = MySession.CustID

                    };
                    _appointmentService.AddRating(rating);
                    _appointmentService.UpdateAppointment(apt.Id, ratingValue);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                //log here
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        #region invoice

        public ActionResult Receipt()
        {
            List<Service> categories = _appointmentService.GetListService();
            ViewBag.Categories = categories;
            int totalRecord = 0;
            List<CompanyInvoices> usernvoicesList = _orderService.GetUserInvoicesInfo(DateTime.Now, 0, string.Empty, -1, MySession.CustID, 1, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.InvoicesList = usernvoicesList;
            ViewBag.Page = 1;
            return View();
        }

        [HttpPost]
        public ActionResult Receipt(DateTime? serviceStartDate, int? serviceId, string empName, int? paymentMethod, string page)
        {
            List<Service> categories = _appointmentService.GetListService();
            ViewBag.Categories = categories;

            if (empName == "search by name") empName = "";
            int pageSize = 10;
            int pageIndex = 0;
            int.TryParse(page, out pageIndex);
            int totalRecord = 0;
            List<CompanyInvoices> usernvoicesList = _orderService.GetUserInvoicesInfo(serviceStartDate, serviceId, empName, paymentMethod, MySession.CustID, pageIndex, pageSize, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.InvoicesList = usernvoicesList;
            ViewBag.Page = pageIndex;

            return PartialView("_UserInvoicesList");
        }



        public ActionResult DownloadInvoicesList(string serviceStartDate, string serviceId, string empName, string paymentMethod)
        {
            try
            {
                DateTime temp = new DateTime();
                int id = 0;
                int payMethod1 = -1;
                int paymethod = -1;
                DateTime? startDate = null;
                if (DateTime.TryParse(serviceStartDate, out temp))
                    startDate = temp;

                int.TryParse(serviceId, out id);

                if (int.TryParse(paymentMethod, out paymethod))
                {
                    payMethod1 = paymethod;
                }

                List<Service> categories = _appointmentService.GetListService();
                ViewBag.Categories = categories;

                int totalRecord = 0;
                List<CompanyInvoices> userInvoicesList = _orderService.GetUserInvoicesInfo(startDate, id, "", -1, MySession.CustID, 1, int.MaxValue, out totalRecord);

                string fileName = string.Format("pdfinvoice_{0}_{1}.pdf", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), UtilityHelper.GenerateRandomDigitCode(4));

                if (ConfigurationManager.AppSettings["IsExcelFle"] != null)
                {
                    fileName = string.Format("pdfinvoice_{0}_{1}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), UtilityHelper.GenerateRandomDigitCode(4));
                    if (bool.Parse(ConfigurationManager.AppSettings["IsExcelFle"]))
                    {
                        BusinessService.GenerateInvoicesListasExcel(userInvoicesList, fileName);
                    }
                }

                string filePath = string.Format("{0}UploadMedia\\{1}", this.Request.PhysicalApplicationPath, fileName);
                _pdfService.PrintUserInvoicesToPdf(userInvoicesList, filePath);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception exc)
            {
                return RedirectToAction("invoices");
            }
        }

        public ActionResult DownloadInvoicesAsPdf()
        {

            try
            {
                int totalRecord;
                var userInvoicesList = _orderService.GetUserInvoicesInfo(null, null, string.Empty, -1, MySession.CustID, 1, int.MaxValue, out totalRecord);
                string fileName = string.Format("pdfinvoice_{0}_{1}.pdf", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), UtilityHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}UploadMedia\\{1}", Request.PhysicalApplicationPath, fileName);
                _pdfService.PrintUserInvoicesToPdf(userInvoicesList, filePath);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception exc)
            {
                return RedirectToAction("invoices");
            }
        }

        #endregion

        #region Appoinment History

        public ActionResult History()
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID2(MySession.CustID, null, false);
            ViewBag.CalendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            ViewBag.Category = _appointmentService.GetServicesByCustomerId(MySession.CustID);
            ViewBag.HtmlData = GenerationHistoryAppointment(result);
            ViewBag.ShowLoadMore = result.Count == 10;
            return View();
        }

        //
        private string GenerationHistoryAppointment(List<AppointmentParticipant> lstApt)
        {
            StringBuilder html = new StringBuilder();
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            DateTime soonestday = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            if (!lstApt.Any())
                return string.Empty;
            if (lstApt.Count > 0)
                soonestday = lstApt[0].Appointment.Start;
            for (int i = 0; i < 7; i++)
            {
                string date = string.Empty;
                bool flag = false;
                List<AppointmentParticipant> newList =
                    lstApt.Where(a => a.Appointment.Start.Date == soonestday.Date).ToList();
                for (int j = 0; j < newList.Count; j++)
                {
                    int appointmentId = newList[j].Appointment.AppointmentID;
                    int appointmentStatus = newList[j].Appointment.AppointmentStatusID;
                    int calendarId = newList[j].CalendarID;
                    var appointment = newList[j].Appointment;
                    string turnClass = "nonturn";
                    string btnClass = "confirm";
                    string btnValue = "confirm";
                    string lblClass = "newrequest";
                    string lblValue = "confirmed";
                    string modifyClass = string.Empty;
                    string eventstring = string.Empty;
                    string eventmodify = string.Format("onclick=\"modify('{0}');", appointmentId);
                    if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    {
                        btnClass = "btnconfirmed hidea";
                        btnValue = "confirm";
                        lblClass = "newrequest";
                        lblValue = "pending";
                        eventstring = string.Format("void('{0}')", 0);
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    {
                        btnClass = "pending hidea";
                        btnValue = "pending";
                        lblClass = "modified";
                        lblValue = "modified";
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    {
                        btnClass = "confirm";
                        btnValue = "confirm";
                        lblClass = "modified";
                        lblValue = "modified";
                        eventstring = string.Format("confirm('{0}','{1}')", appointmentId,
                                                    (int)Types.AppointmentStatus.Confirmed);
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                    {
                        btnClass = "remove";
                        btnValue = "remove";
                        lblClass = "cancelled";
                        lblValue = "cancelled";
                        eventstring = string.Format("remove1('{0}','{1}')", appointmentId,
                                                    (int)Types.AppointmentStatus.Delete);
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)
                    {
                        btnClass = "btnconfirmed hidea";
                        btnValue = "confirmed!";
                        lblClass = "confirmed";
                        lblValue = "confirmed";
                        modifyClass = "hidea";
                        eventstring = string.Empty;
                        eventmodify = string.Empty;
                    }

                    if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                    {
                        turnClass = "yourturn";
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                    {
                        turnClass = "theirturn";
                    }
                    else if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                    {
                        turnClass = "yourturn";
                    }
                    if (j == 0)
                    {
                        date = appointment.Start.Date == today.Date ? string.Format("today, {0}", String.Format("{0: MMM d}", soonestday)) : String.Format("{0:dddd, MMM d}", soonestday);
                        html.Append("<div class=\"boxlist\">");
                        html.AppendFormat("<div class=\"title\">{0}</div>", date.ToLower());
                        html.Append("<div class=\"clear\"> </div>");
                    }
                    var company = appointment.ProfileCompany ?? appointment.ServiceCompany.ProfileCompany;
                    int profileId = company.ProfileID;
                    Medium companyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);
                    string lastClass = string.Empty;
                    if (j == newList.Count() - 1)
                        lastClass = "last";
                    html.AppendFormat("<div class=\"item  {0}\">", lastClass);
                    html.Append("<div class=\"colinfo\">");

                    html.AppendFormat(
                        "<div class=\"colinfotitle\"><div class=\"hours\">{0}</div><div class=\"divname\"> <div class=\"name\">for {1}</div> <div class=\"imgtime\"></div></div></div>",
                        String.Format("{0:t}", appointment.Start).ToLower().Replace(" ", ""), CutString(newList[j].Calendar.Name, 6));

                    html.Append("<div class=\"clear\"> </div>");
                    /*
                    //html.AppendFormat(
                    //    "<div class=\"turn\"><div class=\"{0}\">&nbsp;</div><div class=\"{1}\">{4}</div> <div class=\"button\"> <input type=\"button\" onclick=\"{5};\" title=\"\" value=\"{3}\" class=\"{2}\" /> </div></div>",
                    //    turnClass, lblClass, btnClass, btnValue, lblValue, eventstring);

                    //html.Append("<div class=\"clear\"></div>");

                    //html.AppendFormat(
                    //    "<div class=\"coltime\"><div class=\"ago\">{0} minutes ago</div><div class=\"modify\"><a href=\"/appointment/modify?calendarId={1}&appointmentId={3}\" title=\"modify\" class=\"{2}\" id=\"btnmodify\">modify</a> <a href=\"javascript:void(0);\" onclick=\"cancel('{3}')\" title=\"cancel\" id=\"btncancel\">cancel</a></div></div>",
                    //    1, calendarId, modifyClass, appointmentId);

                    //html.Append("<div class=\"clear\"></div>");
                     * */

                    //status
                    string statusClass = lblClass.Contains("cancelled") ? "cancelled" : "confirmed";
                    //review 
                    if ((appointment.Rating == null || appointment.Rating.Value <= 0) && appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)
                    {
                        html.AppendFormat(
                        "<div class=\"divlink\"> <a href=\"#\" class=\"lnk{1}\" title=\"{0}\">{0}</a><br /> </div>",
                        lblValue, statusClass);
                        html.Append("<div class=\"clear\"></div>");
                        if (appointmentStatus != (int)Types.AppointmentStatus.Cancelled && MySession.OriginalCustIfImpersonated == null)
                        {
                            html.AppendFormat("<div class=\"divsubmitreview\"> <a href=\"#\" class=\"lnksubmitreview\" id=\"btnsubmitreview\" title=\"submit a review\" appid=\"{0}\"></a> </div>", appointmentId);
                        }
                    }
                    else
                    {
                        var reviewStart = string.Empty;
                        int countRating = appointment.Rating ?? 0;
                        for (int start = 0; start < countRating; start++)
                        {
                            reviewStart += "<span class=\"apstar\"></span>";
                        }
                        html.AppendFormat("<div class=\"divlink\"> <div> <a href=\"#\" class=\"lnk{1}\" title=\"{0}\">{0}</a></div>", lblValue, statusClass);
                        html.Append("<div class=\"clear\"></div>");
                        if (countRating > 0)
                        {
                            html.AppendFormat("<div class=\"divreviewed\"> <a href=\"#\" class=\"lnkreviewed\" title=\"reviewed\" appid=\"{1}\">reviewed</a>{0}</div></div>", reviewStart, appointmentId);
                        }
                        else
                        {
                            html.AppendFormat("<div class=\"divreviewed\"> <a href=\"#\" class=\"lnkreviewed\" title=\"reviewed\" appid=\"{1}\">not review</a>{0}</div></div>", reviewStart, appointmentId);
                        }
                    }
                    //s
                    html.Append("<div class=\"clear\"></div>");

                    html.Append("</div>");

                    html.Append("<div class=\"coldescription\">");
                    if (companyMedia != null && companyMedia.LocationData != null)
                    {
                        html.AppendFormat("<div class=\"divimg\"><img alt=\"{1}\" title=\"{1}\" src=\"{2}/p/811441/thumbnail/entry_id/{0}/width/109/height/107\" /></div>",
                        companyMedia.LocationData, appointment.ServiceCompany.ProfileCompany.Name, Types.KaturaDoman);
                    }
                    else
                    {
                        html.Append("<div class=\"divimg\"><span class=\"boxcontentimg\"><span class=\"boximage\"><img src=\"/Images/placeholder.png\" title=\"no logo\" alt=\"no logo\" width=\"86px;\" height=\"83px;\" /></span></span></div>");
                    }

                    var employeeName = appointment.CompanyEmployee != null
                       ? appointment.CompanyEmployee.EmployeeName
                       : appointment.EmployeeName;
                    var serviceName = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Service.ServiceName
                        : appointment.ServiceName;
                    var duration = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Duration
                        : appointment.Duration;
                    var price = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Price
                        : appointment.Price;
                    var attendeesNumber = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.AttendeesNumber
                        : appointment.AttendeesNumber;

                    html.Append("<div class=\"colcontent\">");
                    html.AppendFormat("<h2> {0} with {1}</h2><div class=\"clear\"></div>",
                                      company.Name,
                                      UtilityHelper.TruncateAtWord(employeeName, 17));
                    html.AppendFormat("<div class=\"description\">{0}<br /> {1}min, ${2}, {3} person</div>",
                                      UtilityHelper.TruncateAtWord(serviceName, 17),
                                      duration, price,
                                      attendeesNumber);
                    html.AppendFormat(
                        "<div class=\"viewnote\"><div class=\"lnkviewnote\"><a href=\"javascript:void(0);\"  onclick=\"viewnote('{0}')\" id=\"btnviewnote\">view notes</a> </div><div class=\"nonviewnote\"></div></div>",
                        appointmentId);
                    html.Append("</div></div>");
                    //end item
                    html.Append("</div>");
                    html.Append(" <div class=\"clear\"></div>");

                    flag = true;
                }

                if (flag)
                {
                    html.Append("</div><div class=\"clear14\"></div>");
                }

                soonestday = soonestday.AddDays(1);

            }

            return html.ToString();
        }

        public ActionResult filterHistoryByAll(int calendarId, int status, DateTime? startDate, int serviceid)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID2(MySession.CustID, startDate, false, calendarId, status, serviceid);
            return Json(GenerationHistoryAppointment(result), JsonRequestBehavior.AllowGet);
        }

        public ActionResult fillterHistoryByCalendar(int calendarId, DateTime? startDate, int serviceid)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID2(MySession.CustID, startDate, false, calendarId, serviceid);
            return Json(GenerationHistoryAppointment(result), JsonRequestBehavior.AllowGet);
        }

        public ActionResult fillterHistoryByStatus(int status)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID2(MySession.CustID, null, false, 0, status);
            return Json(GenerationHistoryAppointment(result), JsonRequestBehavior.AllowGet);
        }

        public ActionResult filterAllHistory(int calendarId, int status, DateTime? startDate, int serviceid)
        {
            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID2(MySession.CustID, startDate, true, calendarId, status, serviceid);
            return Json(GenerationHistoryAppointment(result), JsonRequestBehavior.AllowGet);
        }

        private string CutString(string original, int length)
        {
            try
            {
                if (original.Length > length)
                    return original.Substring(0, length);
                return original;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion Appointment History

        public ActionResult IndexCalendar()
        {
            Session[COMPANYCALENDAR_EMPID] = 0;

            List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
            ViewBag.CalendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            ViewBag.Category = _appointmentService.GetListService();
            ViewBag.HtmlData = GenerationAppointment(result);

            return View();
        }

        [HttpGet]
        public ActionResult GetCalendar()
        {
            var start = Request.Params["start"];
            var end = Request.Params["end"];

            DateTime _start = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(start));
            DateTime _end = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(end));

            return Json(GetData(_start, _end), JsonRequestBehavior.AllowGet);
        }

        private List<CalendarObject> GetData(DateTime start, DateTime end)
        {
            int calendarId = 0;
            int.TryParse(Request.Params["calendarId"], out calendarId);

            int empId = Int32.Parse(Session[COMPANYCALENDAR_EMPID].ToString());

            var objEmpId = Request.Params["empId"];
            if (objEmpId != null && objEmpId != "0")
                empId = Int32.Parse(objEmpId);
            List<CalendarObject> calendarObject = new List<CalendarObject>();
            int appId = 0;
            var appointmentId = Request.Params["appointmentId"];
            if (!string.IsNullOrEmpty(appointmentId))
            {
                appId = Int32.Parse(appointmentId);
            }

            string resutl = string.Empty;
            try
            {
                //var eventsAppointment = ProfileCompany.GetKuyamEventsByEmployeeId(empId, start, end);
                List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, calendarId);
                if (result != null)
                {
                    foreach (var evt in result)
                    {
                        CalendarObject item = new CalendarObject();

                        item.id = evt.AppointmentID.ToString();
                        item.title = evt.Appointment.Title;
                        item.start = evt.Appointment.Start.ToString();
                        item.end = evt.Appointment.End.ToString();
                        item.className = evt.AppointmentID == appId
                                             ? "fc-event-skin-active"
                                             : "redclass";
                        item.allDay = false;
                        calendarObject.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                resutl = string.Empty;
            }

            return calendarObject;
        }

        [HttpGet]
        public ActionResult GetAppointmentByCalendar(int calendarId)
        {
            string result = string.Empty;
            List<AppointmentParticipant> apt = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, calendarId);
            if (apt.Count > 0)
            {
                result = Generation(apt);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadCurrentAppointment(int? id)
        {
            string result = string.Empty;
            List<AppointmentParticipant> apt = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false, 0);

            int index = apt.FindIndex(a => a.AppointmentID == id);
            if (index == -1)
            {
                index = 0;
            }
            List<AppointmentParticipant> currentApp = new List<AppointmentParticipant>();
            currentApp.Add(apt[index]);

            if (apt.Count > 0)
            {
                result = GenerationAppointment(currentApp);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string GenerationAppointment(List<AppointmentParticipant> listapt)
        {
            StringBuilder html = new StringBuilder();
            DateTime today = new DateTime();
            if (listapt.Count > 0)
            {
                bool flag = false;
                string date = string.Empty;

                AppointmentParticipant apointment = listapt[0];
                today = DateTimeUltility.ConvertToUserTime(apointment.Appointment.Start);
                int appointmentId = apointment.Appointment.AppointmentID;
                int calendarId = apointment.CalendarID;

                string turnClass = "nonturn";
                string btnClass = "confirm";
                string btnValue = "confirm";
                string lblClass = "newrequest";
                string lblValue = "confirmed";
                string modifyClass = string.Empty;
                string eventstring = string.Empty;
                string eventmodify = string.Format("onclick=\"modify('{0}');", appointmentId);
                string hrfModify = string.Format("/appointment/modify?calendarId={0}&appointmentId={1}", calendarId, appointmentId);
                if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                {
                    btnClass = "btnconfirmed hidea";
                    btnValue = "confirm";
                    lblClass = "newrequest";
                    lblValue = "pending";
                    eventstring = string.Format("void('{0}')", 0);
                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                {
                    btnClass = "pending hidea";
                    btnValue = "pending";
                    lblClass = "modified";
                    lblValue = "modified";
                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                {
                    btnClass = "confirm";
                    btnValue = "confirm";
                    lblClass = "modified";
                    lblValue = "modified";
                    eventstring = string.Format("confirm('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Confirmed);

                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled)
                {
                    btnClass = "remove";
                    btnValue = "remove";
                    lblClass = "cancelled";
                    lblValue = "cancelled";
                    eventstring = string.Format("remove1('{0}','{1}')", appointmentId, (int)Types.AppointmentStatus.Delete);
                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)
                {
                    btnClass = "btnconfirmed hidea";
                    btnValue = "confirmed!";
                    lblClass = "confirmed";
                    lblValue = "confirmed";
                    modifyClass = "hidea";
                    eventstring = string.Empty;
                    eventmodify = string.Empty;
                    hrfModify = "javascript:void(0)";
                }

                if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
                {
                    turnClass = "yourturn";
                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.Modified)
                {
                    turnClass = "theirturn";
                }
                else if (apointment.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)
                {
                    turnClass = "yourturn";
                }
                string lastClass = string.Empty;
                if (!flag)
                {
                    date = String.Format("{0:dddd, MMM d}", today);
                    html.Append("<div class=\"boxlist\">");
                    html.AppendFormat("<div class=\"title\">{0}<a href=\"#\" style=\"color: #0071BC;float: right;font-size: 14px;font-weight: bold;margin-right: 8px;\" id=\"refNextAppointment\" onclick=\"javascript:goNext({1})\">next appointment ></a></div>", date.ToLower(), apointment.AppointmentID);
                    html.Append("<div class=\"clear\"> </div>");

                }

                int profileId = apointment.Appointment.ServiceCompany.ProfileCompany.ProfileID;
                Medium CompanyMedia = _companyProfileService.GetCompanyLogoByProfileID(profileId);

                html.AppendFormat("<div class=\"item {0}\">", lastClass);
                html.Append("<div class=\"colinfo\">");

                html.AppendFormat("<div class=\"colinfotitle\"><div class=\"hours\">{0}</div><div class=\"divname\"> <div title={2} class=\"name\">for {1}</div> <div class=\"imgtime\"></div></div></div>", String.Format("{0:t}", apointment.Appointment.Start).ToLower().Replace(" ", ""), Kuyam.Domain.UtilityHelper.TruncateAtWord(apointment.Calendar.Name, 8), apointment.Calendar.Name);

                html.Append("<div class=\"clear\"> </div>");

                html.AppendFormat("<div class=\"turn\"><div class=\"{0}\">&nbsp;</div><div class=\"{1}\">{4}</div> <div class=\"button\"> <input type=\"button\" onclick=\"{5};\" title=\"\" value=\"{3}\" class=\"{2}\" /> </div></div>", turnClass, lblClass, btnClass, btnValue, lblValue, eventstring);

                html.Append("<div class=\"clear\"></div>");

                html.AppendFormat("<div class=\"coltime\"><div class=\"ago\">{0}</div><div class=\"modify\"><a href=\"{1}\" title=\"modify\" class=\"{2}\" id=\"btnmodify\">modify</a> <a href=\"javascript:void(0);\" onclick=\"cancel('{3}')\" title=\"cancel\" id=\"btncancel\">cancel</a></div></div>", DateTimeUltility.RelativeDatetime(apointment.Appointment.Modified.Value), hrfModify, modifyClass, appointmentId);

                html.Append("<div class=\"clear\"></div>");
                html.Append("</div>");

                html.Append("<div class=\"coldescription\">");
                if (CompanyMedia != null && CompanyMedia.LocationData != null)
                {
                    html.AppendFormat("<div class=\"divimg\"><img alt=\"{1}\" title=\"{1}\" src=\"{2}/p/811441/thumbnail/entry_id/{0}/width/109/height/107\" /></div>", CompanyMedia.LocationData, (apointment != null && apointment.Appointment != null && apointment.Appointment.ServiceCompany != null) ? apointment.Appointment.ServiceCompany.ProfileCompany.Name : string.Empty, Types.KaturaDoman);
                }
                else
                {
                    html.Append("<div class=\"divimg\"><span class=\"boxcontentimg\"><span class=\"boximage\"><img src=\"/Images/placeholder.png\" title=\"no logo\" alt=\"no logo\" width=\"86px;\" height=\"83px;\" /></span></span></div>");
                }
                html.Append("<div class=\"colcontent\">");
                var eplName = apointment.Appointment.CompanyEmployee != null
                    ? Kuyam.Domain.UtilityHelper.TruncateAtWord(apointment.Appointment.CompanyEmployee.EmployeeName, 17)
                    : string.Empty;
                html.AppendFormat("<h2> {0} with {1}</h2><div class=\"clear\"></div>", apointment.Appointment.ServiceCompany.ProfileCompany.Name, eplName);
                html.AppendFormat("<div class=\"description\">{0}<br /> {1}min, ${2}, {3} person</div>", Kuyam.Domain.UtilityHelper.TruncateAtWord(apointment.Appointment.ServiceCompany.Service.ServiceName, 20), apointment.Appointment.ServiceCompany.Duration, apointment.Appointment.ServiceCompany.Price, apointment.Appointment.ServiceCompany.AttendeesNumber);
                html.AppendFormat("<div class=\"viewnote\"><div class=\"lnkviewnote\"><a href=\"javascript:void(0);\"  onclick=\"viewnote('{0}')\" id=\"btnviewnote\"></a> </div><div class=\"nonviewnote\"></div></div>", appointmentId);
                html.Append("</div></div>");
                //end item
                html.Append("</div>");
                html.Append(" <div class=\"clear\"></div>");

                flag = true;



                if (flag)
                {
                    html.Append("</div><div class=\"clear14\"></div>");
                }

                today = today.AddDays(1);

            }

            return html.ToString();
        }

        [HttpGet]
        public ActionResult LoadNextAppointment(int? id)
        {
            List<AppointmentParticipant> lstApp = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, false);
            if (id != null)
            {
                int index = lstApp.FindIndex(a => a.AppointmentID == id);
                if (index == -1 || index >= lstApp.Count - 1)
                    index = 0;
                else
                    index++;
                if (lstApp.Count > 0)
                {
                    var currentApp = lstApp[index];
                    List<AppointmentParticipant> appList = new List<AppointmentParticipant>();
                    appList.Add(currentApp);
                    ViewBag.HtmlData = GenerationAppointment(appList);
                }
            }
            return Json(ViewBag.HtmlData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadMasterAgenda()
        {
            List<Appointment> lstApp = _appointmentService.GetleftAppointmentByCustID(Kuyam.WebUI.Models.MySession.CustID);
            int numofNewApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Pending);
            int numofModApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Modified || a.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified);
            int numofConApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed);
            int numofCanApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled);
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            //var proposed = _appointmentService.GetProposedAppointmentsByCustId(MySession.CustID, null, today);
            //int numofProposed = proposed.Count();
            return Json(string.Format("{0}#{1}#{2}#{3}", numofNewApp, numofModApp, numofConApp, numofCanApp), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TurnOffPopupPushToCalendar()
        {
            MySession.AppoimentID = 0;
            return Json(null);
        }

        public ActionResult PushOniCal()
        {

            string fileName = string.Format("iCalAppointment_{0}_{1}.ics", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), UtilityHelper.GenerateRandomDigitCode(4));

            string filePath = string.Format("{0}UploadMedia\\{1}", Request.PhysicalApplicationPath, fileName);

            var iCal = new iCalHelper();
            bool result = iCal.ExportCalendar(filePath);
            if (result)
            {
                MySession.AppoimentID = 0;
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/ics", fileName);
            }
            return RedirectToAction("Index", "Appointment");
        }
    }
}
