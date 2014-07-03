using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HtmlAgilityPack;
using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.Utility;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.Models.CompanyAppointment;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;
using System.Configuration;
using Kuyam.WebUI.InfoConnServiceReference;
using M2.Util;
using Kuyam.Domain;
using System.Threading;
using Kuyam.Repository.Infrastructure;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.WebUI.Helpers;
using Kuyam.Domain.MessageServcies;

namespace Kuyam.WebUI.Controllers
{
    //[Authorize(Roles = "personal, admin, support")]
    public class CompanyAppointmentController : KuyamBaseController
    {

        private readonly CompanyProfileService _companyProfileService;
        private readonly IAppointmentService _appointmentService;
        private readonly OrderService _orderService;
        private readonly PdfService _pdfService;
        private readonly NotificationService _notificationService;
        private readonly IMembershipService _membershipService;
        private readonly ISMSProvider _smsProvider;
        private const int _limit = 10;

        public CompanyAppointmentController(CompanyProfileService companyProfileService,
            IAppointmentService appointmentService,
            OrderService orderService,
            PdfService pdfService,
            NotificationService notificationService,
            IMembershipService membershipService,
            ISMSProvider smsProvider)
        {
            this._companyProfileService = companyProfileService;
            this._appointmentService = appointmentService;
            this._orderService = orderService;
            this._pdfService = pdfService;
            this._notificationService = notificationService;
            this._membershipService = membershipService;
            this._smsProvider = smsProvider;

        }

        //
        // GET: /CompanyAppointment/

        [Authorize]
        public ActionResult Index()
        {
            int profileId = this.ProfileId;
            ViewBag.ListEmployee = ProfileCompany.GetEmployeeByProfileId(profileId != 0 ? profileId : MySession.ProfileID);
            List<Appointment> lstApp = new List<Appointment>();
            ViewBag.BannerURL = string.Empty;
            ViewBag.Data = string.Empty;
            ViewBag.PageIndex = 0;
            ViewBag.PageLimit = _limit;
            ViewBag.ProfileId = profileId;


            //lstApp = ProfileCompany.GetAppointmentByProfileId(profileId != 0 ? profileId : MySession.ProfileID);
            var appointmentQuery = ProfileCompany.GetAppointmentByProfileIdV2(profileId != 0 ? profileId : MySession.ProfileID);
            lstApp = appointmentQuery.Take(_limit).ToList();
            var appointmentCount = appointmentQuery.Count();
            ViewBag.TotalPages = Math.Round(appointmentCount * 1.0 / _limit + 0.5);
            //Thêm dữ liệu giả
            //lstApp.AddRange(lstApp);
            //lstApp.AddRange(lstApp);
            //---------------------
            CompanyProfile profileCompany = ProfileCompany.GetCompanyProfile(profileId != 0 ? profileId : MySession.ProfileID);
            Profile newprofileCompany = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            string kalturaId = string.Empty;
            //Trong added
            //Check login with facebook;
            ViewBag.UserType = Types.CustType.Personal;
            if (!string.IsNullOrEmpty(newprofileCompany.Cust.FacebookUserID))
                ViewBag.UserType = Types.CustType.Facebook;
            //-------------
            if (newprofileCompany != null && newprofileCompany.ProfileCompany != null)
            {
                if (newprofileCompany.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                    return RedirectToAction("../company/verificationcode/");
                ViewBag.BannerURL = ProfileCompany.GetCompanyMediaURL(MySession.ProfileID, Types.CompanyMediaType.IsBanner, out kalturaId);

                ViewBag.Data = AppointmentHtmlData(lstApp);
            }
            else
            {
                return RedirectToAction("../company/verificationcode/");
            }


            return View();

        }

        [Authorize]
        [HttpPost]
        public ActionResult GetAppointmentsByActionStatus(string status, int profileId, int pageIndex, int limit)
        {
            List<int> lstStatus = new List<int>();
            var totalPages = 0;
            IQueryable<Appointment> appointments;
            switch (status)
            {
                //case "app":
                //    lstStatus.Add((int)Types.AppointmentStatus.NotAvailable);
                //    break;
                case "new":
                    lstStatus.Add((int)Types.AppointmentStatus.Pending);
                    break;
                case "mod":
                    lstStatus.Add((int)Types.AppointmentStatus.Modified);
                    lstStatus.Add((int)Types.AppointmentStatus.CompanyModified);
                    break;
                case "con":
                    lstStatus.Add((int)Types.AppointmentStatus.Confirmed);
                    break;
                case "can":
                    lstStatus.Add((int)Types.AppointmentStatus.Cancelled);
                    break;
            }
            List<Appointment> lstApp = new List<Appointment>();
            if (status == "app")
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(profileId != 0 ? profileId : MySession.ProfileID);
                appointments = ProfileCompany.GetAppointmentByProfileIdV2(profileId != 0 ? profileId : MySession.ProfileID);
            }
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId((profileId != 0 ? profileId : MySession.ProfileID), lstStatus);
                appointments = ProfileCompany.GetAppointmentByProfileIdV2((profileId != 0 ? profileId : MySession.ProfileID), lstStatus);
            }
            totalPages = (int)Math.Round(appointments.Count() * 1.0 / _limit + 0.5);
            appointments = appointments.Skip(limit * pageIndex).Take(limit);
            //string data = AppointmentHtmlData(lstApp);
            string data = AppointmentHtmlData(appointments.ToList());
            return Json(new { data = new { totalPages = totalPages, appointments = data }, success = true });
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetAppointmentByEmployee(int employeeId, int profileId)
        {
            int id = profileId != 0 ? profileId : MySession.ProfileID;
            List<Appointment> lstApp = new List<Appointment>();
            if (employeeId == 0)
                lstApp = ProfileCompany.GetAppointmentByProfileId(id);
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(id, employeeId);
            }
            string data = AppointmentHtmlData(lstApp);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetAppointmentByEmployeeV2(int employeeId, int profileId, int pageIndex, int limit)
        {
            int id = profileId != 0 ? profileId : MySession.ProfileID;
            IQueryable<Appointment> lstApp;
            if (employeeId == 0)
                lstApp = ProfileCompany.GetAppointmentByProfileIdV2(id);
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileIdV2(id, employeeId);
            }
            var totalPages = (int)Math.Round(lstApp.Count() * 1.0 / _limit + 0.5);
            lstApp = lstApp.Skip(pageIndex * limit).Take(limit);
            string data = AppointmentHtmlData(lstApp.ToList());
            return Json(new { data = new { totalPages = totalPages, appointments = data }, success = true });
        }

        private string AppointmentHtmlData(List<Appointment> lstApp)
        {
            StringBuilder builder = new StringBuilder();
            //for (int j = 0; j <= 6; j++)
            var listDays = lstApp.GroupBy(p => p.Start.Date, p => p.Start,
                         (key, g) => key);
            List<Appointment> lstToday = new List<Appointment>();
            foreach (var dt in listDays)
            {
                lstToday = lstApp.Where(a => a.Start.Date == dt.Date).ToList();
                if (lstToday.Count > 0)
                {
                    string today = "today";
                    if (dt.Date != DateTime.Today.Date)
                        today = lstToday[0].Start.ToString("dddd").ToLower();
                    if (lstToday.Count == 1)
                    {
                        Appointment currentApp = lstToday[0];
                        string time = currentApp.Start.ToString("hh:mmt\\M").ToLower();
                        string cName = string.Empty;
                        string dName = string.Empty;
                        string confirmButtonStatus = "confirm";
                        string modifiedButton = "";
                        string cancelButton = "";
                        switch (currentApp.AppointmentStatusID)
                        {
                            case (int)Types.AppointmentStatus.Pending:
                                cName = "newrequest";
                                dName = "new request";
                                break;
                            case (int)Types.AppointmentStatus.Modified:
                                cName = "modified";
                                dName = "modified";
                                break;
                            case (int)Types.AppointmentStatus.CompanyModified:
                                cName = "modified";
                                dName = "modified";
                                confirmButtonStatus = "confirmed";
                                modifiedButton = "hidea";
                                break;
                            case (int)Types.AppointmentStatus.Confirmed:
                                cName = "confirmed";
                                dName = "confirmed";
                                confirmButtonStatus = "confirmed";
                                modifiedButton = "hidea";
                                break;
                            case (int)Types.AppointmentStatus.Cancelled:
                                cName = "cancelled";
                                dName = "cancelled";
                                confirmButtonStatus = "remove";
                                cancelButton = "hidea";
                                modifiedButton = "hidea";
                                break;
                        }
                        string minuteAgo = DateTimeUltility.RelativeDatetime(currentApp.Modified.Value);// (int)DateTimeUltility.ConvertToUtcMinus7(DateTime.Now).Subtract(currentApp.Modified.Value).TotalMinutes;
                        string username = currentApp.Cust.FullName;
                        username = username.Length > 14 ? username.Substring(0, 14) : username;
                        string employeeName = currentApp.CompanyEmployee != null ? currentApp.CompanyEmployee.EmployeeName : currentApp.EmployeeName;
                        employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                        string serviceName = currentApp.ServiceCompany != null ? currentApp.ServiceCompany.Service.ServiceName : currentApp.ServiceName;
                        string appDesc = string.Format("{0} {1}min, ${2}", serviceName,
                                                       currentApp.Duration, currentApp.Price);
                        string appNote = currentApp.Notes ?? string.Empty;
                        appNote = appNote.Length > 25 ? appNote.Substring(0, 25) : appNote;
                        string modifyApp =
                            SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "modify", currentApp.AppointmentID));
                        string cancelApp = SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "cancel", currentApp.AppointmentID));
                        string reason = "reason: " + currentApp.Desc ?? "";
                        reason = reason.Length > 25 ? reason.Substring(0, 25) : reason;
                        if (currentApp.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled)
                            reason = string.Empty;
                        string confirmText = "confirm";
                        if (confirmButtonStatus == "confirmed")
                            confirmText = "confirmed!";
                        else if (confirmButtonStatus == "remove")
                            confirmText = "Remove";

                        if (modifiedButton == "hidea")
                            modifyApp = "";
                        if (cancelButton == "hidea")
                            cancelApp = "";

                        //begin box
                        builder.Append("<div class=\"boxlist\">");
                        //title
                        builder.AppendFormat("<div class=\"title\">{2}, {0} {1}</div>",
                                             lstToday[0].Start.ToString("MMMM").ToLower(), lstToday[0].Start.Day, today);
                        //clear
                        builder.Append("<div class=\"clear\"></div>");
                        //item
                        builder.AppendFormat("" +
                                             "<div class=\"item last\">" +
                                             "<div class=\"colarrow\">" +
                                             "<div class=\"nonearrow\">" +
                                             "&nbsp;</div>" +
                                             "</div>" +
                                             "<div class=\"coltime\">" +
                                             "<div class=\"hours\">" +
                                             "{0}</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "<div class=\"{1}\">" +
                                             "{2}</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "<div class=\"ago\">" +
                                             "{3}</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "</div>" +
                                             "<div class=\"colconfirm\">" +
                                             "<div class=\"name\">" +
                                             "{4}</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "<div class=\"button\">" +
                                             "<input type=\"button\" title=\"\" value=\"{15}\" class=\"{8}\" onclick=\"javascript:actionApp('{11}','{15}','{14}')\"/>" +
                                             "</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "<div class=\"modify\">" +
                                             "<a href=\"#\" title=\"modify\" class=\"{9}\" onclick=\"javascript:modifyApp('{11}','{13}','{14}')\">modify</a> <a href=\"#\" title=\"cancel\" class=\"{10}\" onclick=\"javascript:cancelApp('{12}','{13}')\">cancel</a>" +
                                             "</div>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "</div>" +
                                             "<div class=\"coldescription\">" +
                                             "<h2>" +
                                             "with {5}</h2>" +
                                             "<a href=\"#\" class=\"calendarClass\" val=\"{14}\" onclick=\"javascript:gotoCalendar('{14}')\">view calendar</a>" +
                                             "<div class=\"clear\">" +
                                             "</div>" +
                                             "<div class=\"description\">" +
                                             "{6} {7}" +
                                             "</div>" +
                                             "<div class=\"clear\"></div>" +
                                             "<a href=\"javascript:viewNote('{14}');\" title=\"view notes\"  id=\"lnkviewnotes\">view notes</a>" +
                                             "<div id=\"viewnotespopup\" class=\"viewnotespopup\"></div>" +
                                             "<div class=\"reason\">{16}</div>" +
                                             "</div>" +
                                             "</div>" +
                                             "", time, cName, dName, minuteAgo, username,
                                             employeeName, appDesc, appNote, confirmButtonStatus, modifiedButton,
                                             cancelButton, modifyApp, cancelApp, "hdfEmp" + currentApp.AppointmentID,
                                             currentApp.AppointmentID, confirmText, reason);

                        if (cancelApp != string.Empty)
                        {
                            string datetime = currentApp.Start.ToString("ddd, MMM dd, hh:mmt\\M").ToLower();
                            string custname = username;
                            string compname = currentApp.ServiceCompany != null ? currentApp.ServiceCompany.ProfileCompany.Name : currentApp.ProfileCompany.Name;
                            compname = compname.Length > 25 ? compname.Substring(0, 25) : compname;
                            string empname = employeeName;
                            string sername = currentApp.ServiceCompany != null ? currentApp.ServiceCompany.Service.ServiceName : currentApp.ServiceName;
                            sername = sername.Length > 25 ? sername.Substring(0, 25) : sername;
                            string note = appNote;
                            note = note.Length > 20 ? note.Substring(0, 20) : note;
                            builder.AppendFormat("<input id=\"hdfEmp{0}\" type=\"hidden\" datetime=\"{1}\" custname=\"for {2}\" compname=\"{3}\" empname=\"{4}\" sername=\"with {5}\" note=\"{6}\" />",
                                currentApp.AppointmentID, datetime, custname, compname, empname, sername, note);
                        }
                        //clear
                        builder.Append("<div class=\"clear\"></div>");
                        //endbox
                        builder.Append("</div>");
                    }
                    else
                    {
                        //begin box
                        builder.Append("<div class=\"boxlist\">");
                        //title
                        builder.AppendFormat("<div class=\"title\">{2}, {0} {1}</div>", lstToday[0].Start.ToString("MMMM").ToLower(), lstToday[0].Start.Day, today);
                        //clear
                        builder.Append("<div class=\"clear\"></div>");

                        for (int i = 0; i < lstToday.Count; i++)
                        {
                            Appointment currentApp = lstToday[i];
                            string titleclass = "item";
                            if (i == lstToday.Count - 1)
                                titleclass = "item last";
                            string time = currentApp.Start.ToString("hh:mmt\\M").ToLower();
                            string cName = string.Empty;
                            string dName = string.Empty;
                            string confirmButtonStatus = "confirm";
                            string modifiedButton = "";
                            string cancelButton = "";
                            switch (currentApp.AppointmentStatusID)
                            {
                                case (int)Types.AppointmentStatus.Pending:
                                    cName = "newrequest";
                                    dName = "new request";
                                    break;
                                case (int)Types.AppointmentStatus.Modified:
                                    cName = "modified";
                                    dName = "modified";
                                    break;
                                case (int)Types.AppointmentStatus.CompanyModified:
                                    cName = "modified";
                                    dName = "modified";
                                    confirmButtonStatus = "confirmed";
                                    modifiedButton = "hidea";
                                    break;
                                case (int)Types.AppointmentStatus.Confirmed:
                                    cName = "confirmed";
                                    dName = "confirmed";
                                    confirmButtonStatus = "confirmed";
                                    modifiedButton = "hidea";
                                    break;
                                case (int)Types.AppointmentStatus.Cancelled:
                                    cName = "cancelled";
                                    dName = "cancelled";
                                    confirmButtonStatus = "remove";
                                    cancelButton = "hidea";
                                    modifiedButton = "hidea";
                                    break;
                            }
                            string minuteAgo = DateTimeUltility.RelativeDatetime(currentApp.Modified.Value);// (int)DateTimeUltility.ConvertToUtcMinus7(DateTime.Now).Subtract(currentApp.Modified.Value).TotalMinutes;
                            string username = currentApp.Cust.FullName;
                            username = username.Length > 14 ? username.Substring(0, 14) : username;
                            string employeeName = currentApp.CompanyEmployee != null ? currentApp.CompanyEmployee.EmployeeName : currentApp.EmployeeName;
                            employeeName = (!string.IsNullOrEmpty(employeeName) && employeeName.Length > 6) ? employeeName.Substring(0, 6) : employeeName;
                            string serviceName = currentApp.ServiceCompany != null
                                ? Kuyam.Domain.UtilityHelper.TruncateText(
                                    currentApp.ServiceCompany.Service.ServiceName, 20)
                                : Kuyam.Domain.UtilityHelper.TruncateText(
                                    currentApp.ServiceName, 20);
                            int? duration = currentApp.ServiceCompany != null
                                ? currentApp.ServiceCompany.Duration
                                : currentApp.Duration;
                            decimal? price = currentApp.ServiceCompany != null
                                ? currentApp.ServiceCompany.Price
                                : currentApp.Price;
                            string appDesc = string.Format("{0} {1}min, ${2}", UtilityHelper.TruncateAtWord(serviceName, 15), duration, price);
                            string appNote = currentApp.Notes ?? string.Empty;
                            appNote = (appNote.Length > 25) ? appNote.Substring(0, 25) : appNote;
                            string modifyApp = SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "modify", currentApp.AppointmentID));
                            string cancelApp = SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "cancel", currentApp.AppointmentID));
                            string reason = "reason: " + currentApp.Desc ?? "";
                            reason = reason.Length > 25 ? reason.Substring(0, 25) : reason;
                            if (currentApp.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled)
                                reason = string.Empty;

                            string confirmText = "confirm";
                            if (confirmButtonStatus == "confirmed")
                                confirmText = "confirmed!";
                            else if (confirmButtonStatus == "remove")
                                confirmText = "Remove";


                            if (modifiedButton == "hidea")
                                modifyApp = "";
                            if (cancelButton == "hidea")
                                cancelApp = "";

                            //item
                            builder.AppendFormat("" +
                                                 "<div class=\"{8}\">" +
                                                 "<div class=\"colarrow\">" +
                                                 "<div class=\"nonearrow\">" +
                                                 "&nbsp;</div>" +
                                                 "</div>" +
                                                 "<div class=\"coltime\">" +
                                                 "<div class=\"hours\">" +
                                                 "{0}</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "<div class=\"{1}\">" +
                                                 "{2}</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "<div class=\"ago\">" +
                                                 "{3}</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "</div>" +
                                                 "<div class=\"colconfirm\">" +
                                                 "<div class=\"name\">" +
                                                 "{4}</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "<div class=\"button\">" +
                                                 "<input type=\"button\" title=\"\" value=\"{16}\" class=\"{9}\" onclick=\"javascript:actionApp('{12}','{16}','{15}')\"/>" +
                                                 "</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "<div class=\"modify\">" +
                                                 "<a href=\"#\" title=\"modify\" class=\"{10}\" onclick=\"javascript:modifyApp('{12}','{14}','{15}')\">modify</a> <a href=\"#\" title=\"cancel\" class=\"{11}\" onclick=\"javascript:cancelApp('{13}','{14}')\">cancel</a>" +
                                                 "</div>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "</div>" +
                                                 "<div class=\"coldescription\">" +
                                                 "<h2>" +
                                                 "with {5}</h2>" +
                                                 "<a href=\"#\" class=\"calendarClass\" val=\"{15}\">view calendar</a>" +
                                                 "<div class=\"clear\">" +
                                                 "</div>" +
                                                 "<div class=\"description\">" +
                                                 "{6} {7} " +
                                                 "</div>" +
                                                 "<div class=\"clear\" ></div>" +
                                                 "<a href=\"javascript:viewNote('{15}');\" title=\"view notes\"  id=\"lnkviewnotes\">view notes</a>" +
                                                 "<div id=\"viewnotespopup\" class=\"viewnotespopup\"></div>" +
                                                 "<div class=\"reason\">{17}</div>" +
                                                 "</div>" +
                                                 "</div>" +
                                                 "", time, cName, dName, minuteAgo, username,
                                                 employeeName, appDesc, appNote, titleclass, confirmButtonStatus,
                                                 modifiedButton, cancelButton, modifyApp, cancelApp,
                                                 "hdfEmp" + currentApp.AppointmentID, currentApp.AppointmentID, confirmText,
                                                 reason);
                            //clear
                            builder.Append("<div class=\"clear\"></div>");
                            if (cancelApp != string.Empty)
                            {
                                string datetime = currentApp.Start.ToString("ddd, MMM dd, hh:mmt\\M").ToLower();
                                string custname = username;
                                string compname = currentApp.ServiceCompany != null ? currentApp.ServiceCompany.ProfileCompany.Name : currentApp.ProfileCompany.Name;
                                compname = compname.Length > 25 ? compname.Substring(0, 25) : compname;
                                string empname = employeeName;
                                string sername = currentApp.ServiceCompany != null ? currentApp.ServiceCompany.Service.ServiceName : currentApp.ServiceName;
                                sername = sername.Length > 25 ? sername.Substring(0, 25) : sername;
                                string note = appNote;
                                note = note.Length > 20 ? note.Substring(0, 20) : note;
                                builder.AppendFormat(
                                    "<input id=\"hdfEmp{0}\" type=\"hidden\" datetime=\"{1}\" custname=\"for {2}\" compname=\"{3}\" empname=\"{4}\" sername=\"with {5}\" note=\"{6}\" />",
                                    currentApp.AppointmentID, datetime, custname, compname, empname, sername, note);
                            }
                        }
                        //endbox
                        builder.Append("</div>");
                        builder.Append("<div class=\"clear14\"></div>");
                    }
                }
            }
            return builder.ToString();
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

        [HttpGet]
        public ActionResult VerifyPassword(string pass)
        {
            string data = string.Empty;
            if (string.IsNullOrEmpty(MySession.Cust.FacebookUserID))
            {
                //AccountMembershipService membershipService = new AccountMembershipService();
                Cust user = MySession.Cust;
                if (!_membershipService.ValidateUser(user.Username, pass))
                {
                    data = "invalid password";
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetServiceByEmployee(int employeeId, int? serviceId)
        {
            StringBuilder data = new StringBuilder();
            var lstService = ProfileCompany.GetServiceCompanybyEmployeeId(0, employeeId, serviceId ?? 0);
            foreach (var companyService in lstService)
            {
                data.AppendFormat("<option value='{0}'>{1}, {2}, {3}</option>", companyService.ID,
                                  companyService.ServiceName, companyService.Duration + "min",
                                  "$" + companyService.Price);
            }
            return Json(data.ToString(), JsonRequestBehavior.AllowGet);
        }

        private void ConfirmNotification(Appointment appointment, string emailstring)
        {
            string emailTo = string.Empty;
            string phoneNumber = string.Empty;
            Cust aUser = DAL.GetCustByCustId(appointment.CustID);

            if (aUser != null)
            {
                emailTo = aUser.Email;
                phoneNumber = aUser.MobilePhone;
            }

            if (appointment.ContactType == (int)Types.ContactType.Email)
            {
                EmailHelper.SendEmailConfirmAppointment(string.Empty, emailTo, emailstring);
            }
            else if (appointment.ContactType == (int)Types.ContactType.SMS)
            {
                _smsProvider.SendSms("your appointment has been confirmed.", new string[] { phoneNumber });
            }
            else if (appointment.ContactType == (int)Types.ContactType.EmailSMS)
            {
                _smsProvider.SendSms("your appointment has been confirmed.", new string[] { phoneNumber });
                EmailHelper.SendEmailConfirmAppointment(string.Empty, emailTo, emailstring);
            }
        }

        [Authorize]
        public ActionResult ActionAppointment(int appId, string appCode, int profileId)
        {
            if (appCode.Equals("Remove", StringComparison.InvariantCultureIgnoreCase))
            {
                ProfileCompany.RemoveAppointment(appId);
                //_notificationService.SendAppoinmentChangeNotify(appId);
            }
            if (appCode.Equals("Confirm", StringComparison.InvariantCultureIgnoreCase))
            {

                Appointment appointment = ProfileCompany.GetAppointmentById(appId);
                ProfileCompany company = ProfileCompany.GetProfileCompany(profileId > 0 ? profileId : MySession.ProfileID);

                if (company == null)
                    return Json("false", JsonRequestBehavior.AllowGet);

                if (appointment != null && !string.IsNullOrEmpty(appointment.PreapprovalKey))
                {
                    string Key = appointment.PreapprovalKey;
                    ConfirmPreapprovalRequest req = new ConfirmPreapprovalRequest(new RequestEnvelope("en_US"), Key);

                    // All set. Fire the request            
                    AdaptivePaymentsService service = new AdaptivePaymentsService();
                    ConfirmPreapprovalResponse resp = null;
                    try
                    {
                        resp = service.ConfirmPreapproval(req);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("PayPal Confirm fail:", ex);
                        return Json("false", JsonRequestBehavior.AllowGet);
                    }

                    string emailSender = appointment.SenderEmail;

                    int countMinutes = company.CancelHour.HasValue ? company.CancelHour.Value * 60 : 0;
                    int payDefault = 0;
                    int.TryParse(ConfigManager.PayDate, out payDefault);
                    DateTime paidDate = countMinutes != 0 ? appointment.Start.AddMinutes(-countMinutes) : appointment.Start.AddMinutes(-payDefault * 60);
                    paidDate = DateTimeUltility.ConvertToUtcTime(paidDate, DateTimeUltility.CurrentTimeZone);
                    if (company.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt ||
                        company.CompanyTypeID == (int)Types.CompanyType.NonKuyamBookIt)
                    {
                        paidDate = DateTime.UtcNow;
                    }
                    decimal paidPrice = 0;
                    decimal paymentFeeTotal = 0;
                    decimal kuyamFeeTotal = 0;
                    var order = _orderService.GetOrderByAppointmentId(appId);
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
                            EmailSender = emailSender,
                            Companytype = company.CompanyTypeID,
                            CompanyChargePrice = companyCharge,
                            KuyamChargePrice = kuyamFeeTotal,
                            CompanyEmailReceive = company.PaymentOptions,
                            KuyamEmailReceice = MyApp.Settings.PaySetting.PaypalAccount
                        })
                    };
                    try
                    {
                        serviceInfo.AddIncomingRequest(obj, IncommingRequestType.PAYMENT_PAYPAL);
                    }
                    catch
                    {
                        return Json("false", JsonRequestBehavior.AllowGet);

                    }
                }

                ProfileCompany.Confirm(appId);


                if (appointment.StaffID.HasValue && appointment.StaffID > 0)
                {
                    EmailHelper.SendAppointmentNotifyToConcierge(appointment, Types.NotifyType.Confirm, this);
                }
                else
                {
                    if (appointment.ContactType == (int)Types.ContactType.Email
                        || appointment.ContactType == (int)Types.ContactType.SMS
                        || appointment.ContactType == (int)Types.ContactType.EmailSMS)
                    {
                        string emailstring = GetEmailTemplateAppointment(appointment.AppointmentID, "CompanyConfirm");
                        ConfirmNotification(appointment, emailstring);
                    }
                    _notificationService.SendAppointmentChangeNotify(appId);
                }
              
                try
                {
                    List<AppointmentLog> notes = appointment.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                    var appart = _appointmentService.GetAppointmentParticipantByAppointmentId(appId);
                    string methodCommunication = string.Empty;
                    string subject = "";
                    string companyEmail = string.Empty;
                    string companySMS = string.Empty;
                    int? preferedContact = null;

                    if (company.PreferredContact.HasValue)
                        preferedContact = company.PreferredContact.Value;

                    if (preferedContact != null && ((preferedContact.Value == 1)))
                    {
                        methodCommunication = "Email";
                        companyEmail = company.Email;
                    }
                    else if (preferedContact != null && ((preferedContact.Value == 2)))
                    {
                        methodCommunication = "SMS";
                        companySMS = company.Phone;
                    }

                    else if (preferedContact != null && ((preferedContact.Value == 3)))
                    {
                        methodCommunication = "SMS&Email";
                        companySMS = company.Phone;
                    }
                    else
                    {
                        methodCommunication = "not specified";
                    }

                    subject = "Appointment " + appointment.AppointmentID + " has beeen confirmed";
                    if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                        subject = "[QA] " + subject;
                    else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                        subject = "[DEV] " + subject;

                    //basic info
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

                    string description = "appointment information: \n"
                         + "company name: " + company.Name.ToLower() + "\n"
                         + "company contact info: " + "\n";

                    if (company.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                    {
                        if (methodCommunication == "Email")
                        {
                            description += "   email:	" + company.Email.ToLower() + "\n";
                        }
                        else if (methodCommunication == "SMS")
                        {
                            description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                        }
                        else if (methodCommunication == "SMS&Email")
                        {
                            description += "   email:	" + company.Email.ToLower() + "\n"
                                        + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                        }
                    }
                    else if (company.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt || company.CompanyTypeID == (int)Types.CompanyType.NonKuyamBookIt)
                    {
                        description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                    }


                    if (notes.Count > 0)
                    {
                        foreach (var note in notes)
                        {
                            description += "\t" + note.Message.Replace("<br/>", "\n").ToLower() + "\n";
                        }
                    }

                    description += "primary contact: " + (company.ContactFirstName != null ? company.ContactFirstName.ToLower() : string.Empty) + " "
                                + (company.ContactLastName != null ? company.ContactLastName.ToLower() : string.Empty) + "\n"
                                + "employee name: " + employeeName.ToLower() + "\n"
                                + "calendar: " + appart.Calendar.Name.ToLower() + "\n"
                                + "service: " + serviceName.ToLower() + "\n"
                                + "duration: " + duration + "\n"
                                + "price: " + string.Format("${0:0.00}", price) + "\n"
                                + "date: " + string.Format("{0:MMM dd yyyy}", appointment.Start).ToLower() + "\n"
                                + "time: " + appointment.Start.ToString("h:mm tt").ToLower() + " - " + appointment.End.ToString("h:mm tt").ToLower() + "\n"
                                + "appointment status: " + (Types.AppointmentStatus.Confirmed).ToString().ToLower() + "\n"
                                 + "appointment notes: " + "\n";
                    description += "\t" + appointment.Notes + "\n";

                    Thread zendeskThread = new Thread(() =>
                    {
                        //Create Zendesk ticket here -- Khoi Tran
                        //Information for the ticket                      

                        TicketStatus status = (TicketStatus.Pending);
                        TicketType type = (TicketType.Incident);
                        TicketPriority priority = (TicketPriority.High);

                        int group_id = Int32.Parse(ConfigurationManager.AppSettings["groupid"]);

                        ZenAPI.CreateTicket(subject, status, type, priority, description);
                        //End of creating ticket
                    });
                    zendeskThread.Start();
                }
                catch (Exception ex)
                {
                    //Todo: Handle Exception Occur
                    LogHelper.Error("ActionAppointment fail:", ex);
                }

            }

            List<Appointment> lstApp = new List<Appointment>();
            lstApp = ProfileCompany.GetAppointmentByProfileIdV2(profileId != 0 ? profileId : MySession.ProfileID).ToList();
            string data = AppointmentHtmlData(lstApp);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult CancelAppointment(int employeeId, string appCode, string reason, string pass)
        {
            //Cust user = MySession.Cust;
            try
            {
                string[] codes = SecurityHelper.DecryptStringFromBytesAes(appCode.Replace(" ", "+")).Split('@');
                Appointment appointment = ProfileCompany.GetAppointmentById(Int32.Parse(codes[1]));
                int profileId = appointment.ServiceCompany != null
                    ? appointment.ServiceCompany.ProfileID
                    : (appointment.ProfileId.HasValue ? appointment.ProfileId.Value : 0);
                ProfileCompany company = ProfileCompany.GetProfileCompany(profileId);

                if (codes.Length == 2 && codes[0] == "cancel")
                {
                    int appId = Int32.Parse(codes[1]);
                    ProfileCompany.CancelAppointment(appId, reason);
                    _notificationService.SendAppointmentChangeNotify(appId);

                    try
                    {
                        Thread zendeskThread = new Thread(() =>
                        {
                            //Create Zendesk ticket here -- Khoi Tran
                            //Information for the ticket   
                            List<AppointmentLog> notes = appointment.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                            AppointmentParticipant appart = EngineContext.Current.Resolve<IAppointmentService>().GetAppointmentParticipantByAppointmentId(appId);
                            string methodCommunication = string.Empty;
                            string subject = "";
                            string companyEmail = string.Empty;
                            string companySMS = string.Empty;
                            int? preferedContact = null;
                            if (company.PreferredContact.HasValue)
                                preferedContact = company.PreferredContact.Value;
                            if (preferedContact != null && ((preferedContact.Value == 1)))
                            {
                                methodCommunication = "Email";
                                companyEmail = company.Email;
                            }
                            else if (preferedContact != null && ((preferedContact.Value == 2)))
                            {
                                methodCommunication = "SMS";
                                companySMS = company.Phone;
                            }

                            else if (preferedContact != null && ((preferedContact.Value == 3)))
                            {
                                methodCommunication = "SMS&Email";
                                companySMS = company.Phone;
                            }
                            else
                            {
                                methodCommunication = "not specified";
                            }

                            subject = "Appointment " + appointment.AppointmentID + " has beeen cancelled";
                            if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                                subject = "[QA] " + subject;
                            else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                                subject = "[DEV] " + subject;

                            TicketStatus status = (TicketStatus.Pending);
                            TicketType type = (TicketType.Incident);
                            TicketPriority priority = (TicketPriority.High);
                            string description = "appointment information: \n"
                                + "company name: " + company.Name.ToLower() + "\n"
                                + "company contact info: " + "\n";
                            if (company.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                            {
                                if (methodCommunication == "Email")
                                {
                                    description += "   email:	" + company.Email.ToLower() + "\n";
                                }
                                else if (methodCommunication == "SMS")
                                {
                                    description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                                }
                                else if (methodCommunication == "SMS&Email")
                                {
                                    description += "   email:	" + company.Email.ToLower() + "\n"
                                                + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                                }
                            }
                            else if (company.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt || company.CompanyTypeID == (int)Types.CompanyType.NonKuyamBookIt)
                            {
                                description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                            }

                            //basic info
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

                            description += "primary contact: " + (company.ContactFirstName != null ? company.ContactFirstName.ToLower() : string.Empty) + " "
                                        + (company.ContactLastName != null ? company.ContactLastName.ToLower() : string.Empty) + "\n"
                                        + "employee name: " + employeeName.ToLower() + "\n"
                                        + "calendar: " + appart.Calendar.Name.ToLower() + "\n"
                                        + "service: " + serviceName.ToLower() + "\n"
                                        + "duration: " + duration + "\n"
                                        + "price: " + string.Format("${0:0.00}", price) + "\n"
                                        + "date: " + string.Format("{0:MMM dd yyyy}", appointment.Start).ToLower() + "\n"
                                        + "time: " + appointment.Start.ToString("h:mm tt").ToLower() + " - " + appointment.End.ToString("h:mm tt").ToLower() + "\n"
                                        + "appointment status: " + (Types.AppointmentStatus.Confirmed).ToString().ToLower() + "\n"
                                         + "appointment notes: " + "\n";
                            description += "\t" + appointment.Notes + "\n";

                            if (notes.Count > 0)
                            {
                                foreach (var note in notes)
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
                        //Todo: Handle Exception Occur
                        LogHelper.Error("CancelAppointment fail:", ex);
                    }

                    //Trong edit 19/11/12
                    if (appointment.StaffID == null)
                    {
                        string emailTo = string.Empty;
                        Cust aUser = DAL.GetCustByCustId(appointment.CustID);
                        emailTo = aUser != null ? aUser.Email : String.Empty;
                        string emailFrom = company.Email;
                        string templateString = GetEmailTemplateAppointment(Int32.Parse(codes[1]), "companycancel");
                        EmailHelper.SendEmailCancelAppointment(string.Empty, emailTo, templateString);
                       
                    }
                    else
                    {
                        EmailHelper.SendAppointmentNotifyToConcierge(appointment, Types.NotifyType.Cancel, this);
                    }
                }
            }
            catch (Exception ex)
            {
                //Todo: Handle Exception Occur
                LogHelper.Error("CancelAppointment fail:", ex);
            }
            List<Appointment> lstApp = new List<Appointment>();
            if (employeeId == 0)
                lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID, employeeId);
            }
            string data = AppointmentHtmlData(lstApp);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        [HttpGet]
        public ActionResult ActionStatus(string status, int profileId)
        {
            List<int> lstStatus = new List<int>();
            switch (status)
            {
                //case "app":
                //    lstStatus.Add((int)Types.AppointmentStatus.NotAvailable);
                //    break;
                case "new":
                    lstStatus.Add((int)Types.AppointmentStatus.Pending);
                    break;
                case "mod":
                    lstStatus.Add((int)Types.AppointmentStatus.Modified);
                    lstStatus.Add((int)Types.AppointmentStatus.CompanyModified);
                    break;
                case "con":
                    lstStatus.Add((int)Types.AppointmentStatus.Confirmed);
                    break;
                case "can":
                    lstStatus.Add((int)Types.AppointmentStatus.Cancelled);
                    break;
            }
            List<Appointment> lstApp = new List<Appointment>();
            if (status == "app")
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(profileId != 0 ? profileId : MySession.ProfileID);
            }
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId((profileId != 0 ? profileId : MySession.ProfileID), lstStatus);
            }

            string data = AppointmentHtmlData(lstApp);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadMasterAgenda(int profileId)
        {
            int id = profileId != 0 ? profileId : MySession.ProfileID;

            List<Appointment> lstApp = ProfileCompany.GetAppointmentByProfileIdV2(id).ToList();
            Session[Contants.COMPANY_APPOINTMENT_LISTAPPOINTMENT] = lstApp;
            int numofNewApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Pending);
            int numofModApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Modified || a.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified);
            int numofConApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed);
            int numofCanApp = lstApp.Count(a => a.AppointmentStatusID == (int)Types.AppointmentStatus.Cancelled);
            return Json(string.Format("{0}#{1}#{2}#{3}", numofNewApp, numofModApp, numofConApp, numofCanApp),
                        JsonRequestBehavior.AllowGet);
        }

        #region Invoices

        [Authorize]
        public ActionResult CompanyInvoices()
        {
            //Get the profile info for the company
            Profile profiles = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("CompanySetup", "Company");

            int profileId = profiles.ProfileID;

            List<Service> categories = _appointmentService.GetListService();
            List<PaymentMethod> paymentMethods = _appointmentService.GetPaymentMethodByProfileId(profileId);
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.Categories = categories;
            ViewBag.Profileid = profileId;
            int totalRecord = 0;
            List<CompanyInvoices> companyInvoicesList = _orderService.GetCompanyInvoicesInfo(DateTime.Now, 0, string.Empty, -1, profileId, 1, 10, out totalRecord);
            //_companyProfileService.GetCompanyInvoicesInfo(DateTime.Now, 0, "", 0, profileId, 1, 10, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.InvoicesList = companyInvoicesList;
            ViewBag.Page = 1;

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult CompanyInvoices(DateTime? serviceStartDate, int? serviceId, string empName, int? paymentMethod, string page, int companyId)
        {
            //Get the profile info for the company
            Profile profiles = _companyProfileService.GetProfileByID(companyId != 0 ? companyId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("CompanySetup", "Company");

            int profileId = profiles.ProfileID;

            List<Service> categories = _appointmentService.GetListService();
            List<PaymentMethod> paymentMethods = _appointmentService.GetPaymentMethodByProfileId(profileId);
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.Categories = categories;

            if (empName == "search by name") empName = "";
            int pageSize = 10;
            int pageIndex = 0;
            int.TryParse(page, out pageIndex);
            int totalRecord = 0;
            List<CompanyInvoices> companyInvoicesList = _orderService.GetCompanyInvoicesInfo(serviceStartDate, serviceId, empName, paymentMethod, profileId, pageIndex, pageSize, out totalRecord);
            // _companyProfileService.GetCompanyInvoicesInfo(serviceStartDate, serviceId, empName, paymentMethod, profileId, pageIndex, pageSize, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.InvoicesList = companyInvoicesList;
            ViewBag.Page = pageIndex;

            return PartialView("CompanyInvoicesList");
        }

        public ActionResult DownloadInvoicesAsPdf(string serviceStartDate, string serviceId, string empName, string paymentMethod, int profileId = 0)
        {
            try
            {
                if (empName == "search by name") empName = "";
                DateTime temp = new DateTime();
                int id = 0;
                int payMethod = -1;
                DateTime? startDate = null;
                if (DateTime.TryParse(serviceStartDate, out temp))
                    startDate = temp;

                int.TryParse(serviceId, out id);
                int.TryParse(paymentMethod, out payMethod);

                //Get the profile info for the company
                Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
                if (profiles == null)
                    return RedirectToAction("CompanySetup", "Company");

                profileId = profiles.ProfileID;

                int totalRecord = 0;
                //_orderService.GetCompanyInvoicesInfo(DateTime.Now, 0, string.Empty, -1, profileId, 1, 10, out totalRecord);
                //var companyInvoicesList = _orderService.GetCompanyInvoicesInfo(DateTime.Now, 0, string.Empty, -1, profileId, 1, 10, out totalRecord);
                var companyInvoicesList = _orderService.GetCompanyInvoicesInfo(temp, id, empName, payMethod, profileId, 1, 10, out totalRecord);
                string fileName = string.Format("pdfinvoice_{0}_{1}.pdf", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), UtilityHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}UploadMedia\\{1}", this.Request.PhysicalApplicationPath, fileName);
                _pdfService.PrintInvoicesToPdf(companyInvoicesList, filePath);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pdf", fileName);

            }
            catch (Exception exc)
            {
                return RedirectToAction("CompanyInvoices", new { companyId = this.ProfileId });
            }
        }

        #endregion


        public ActionResult CompanyAvailableTimeSlotsOnly(int companyId, ProfileCompany profileCompany = null)
        {
            return PartialView("_CompanyAvailableTimeSlots", GetCompanyAvailableTimeSlots(companyId, profileCompany));
        }

        public ActionResult CompanyAvailableTimeSlotsOnlyImprove(int companyId, ProfileCompany profileCompany = null)
        {
            return PartialView("_CompanyAvailableTimeSlots", GetCompanyAvailableTimeSlotsImprove(companyId, profileCompany));
        }


        public ActionResult CompanyProfileTimeSlots(int companyId)
        {
            var model = GetCompanyProfileTimeSlots(companyId);
            return PartialView("_CompanyProfileTimeSlots", model);
        }

        public string RenderCompanyProfileTimeSlots(Controller controllerCall, string html, string tagName = "")
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            string classToFind = "profileCompanyHtmlEdit";
            var profileDivs = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", classToFind));
            if (profileDivs == null)
                return html;

            foreach (HtmlNode htmlNode in profileDivs)
            {
                try
                {
                    var profileIdString = Regex.Match(htmlNode.GetAttributeValue("id", "0"), @"\d+").Value;
                    var style = htmlNode.GetAttributeValue("style", "0");
                    var profileId = 0;
                    if (int.TryParse(profileIdString, out profileId))
                    {
                        var model = GetCompanyProfileTimeSlots(profileId);

                        string htmlString = string.Empty;
                        if (!string.IsNullOrEmpty(tagName) && tagName == "thelatest")
                        {
                            htmlString = controllerCall.RenderPartialViewToString("_CompanyInfor", model);
                        }
                        else
                        {
                            htmlString = controllerCall.RenderPartialViewToString("_CompanyProfileTimeSlots", model);
                        }
                        var stringNode = htmlString;
                        if (!string.IsNullOrEmpty(style))
                            stringNode = string.Format("<div style=\"{0}\"> {1}</div>", style, htmlString.Trim());
                        var node = HtmlNode.CreateNode(stringNode);
                        htmlNode.ParentNode.ReplaceChild(node, htmlNode);
                    }
                }
                catch
                {

                }
            }

            return doc.DocumentNode.OuterHtml;

        }

        public CompanyProfileTimeSlots GetCompanyProfileTimeSlots(int companyId)
        {
            var profileCompany = DAL.GetProfileCompany(companyId);
            if (profileCompany == null || profileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return null;

            var model = new CompanyProfileTimeSlots();
            model.Company = profileCompany;
            model.TimeSlots = GetCompanyAvailableTimeSlots(companyId, profileCompany);
            return model;
        }

        private CompanyAvailableTimeSlots GetCompanyAvailableTimeSlots(int companyId, ProfileCompany profileCompany = null)
        {
            if (profileCompany == null)
                profileCompany = DAL.GetProfileCompany(companyId);

            var startTime = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            var model = new Models.CompanyAppointment.CompanyAvailableTimeSlots() { CompanyProfileId = companyId };
            model.ProfileCompany = profileCompany;

            var companyhours =
                        _companyProfileService.GetCompanyHourProfileID(companyId)
                            .ToList();
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
                        model.SetCompanyHours(hoursOfDate, date, startTime);
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
                var companyTimeslots = _appointmentService.GetCompanyTimeSlotsAvailable(companyId, startTime, endTime,
                    Models.CompanyAppointment.CompanyAvailableTimeSlots.NumberTimeSlots + 1);

                model.SetTimeSlots(companyTimeslots, startTime);

            }

            //else if (profileCompany.CompanyTypeID == (int) Types.CompanyType.GeneralAvailability)
            //{
            //    var generalTimes = _companyProfileService.GetCompanyGeneralServiceTimes(companyId, startTime);
            //    for (int i = 0; i < 7; i++)
            //    {
            //        var date = startTime.Date.AddDays(i);
            //        var hoursOfDate = generalTimes.Where(c => c.DateOfWeek == (int) date.DayOfWeek).Take(2)
            //            .ToList();

            //        if (hoursOfDate.Any())
            //        {
            //            model.SetCompanyGenreralTimes(hoursOfDate, date, startTime);
            //            break;
            //        }
            //    }
            //}
            return model;
        }

        private CompanyAvailableTimeSlots GetCompanyAvailableTimeSlotsImprove(int companyId, ProfileCompany profileCompany = null)
        {
            if (profileCompany == null)
                profileCompany = DAL.GetProfileCompany(companyId);

            var startTime = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            var model = new Models.CompanyAppointment.CompanyAvailableTimeSlots() { CompanyProfileId = companyId };
            model.ProfileCompany = profileCompany;

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
                        model.SetCompanyHours(hoursOfDate, date, startTime);
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
                var companyTimeslots = _appointmentService.GetCompanyTimeSlotsAvailable(companyId, startTime, endTime,
                    Models.CompanyAppointment.CompanyAvailableTimeSlots.NumberTimeSlots + 1);

                model.SetTimeSlots(companyTimeslots, startTime);

            }

            return model;
        }
    }
}
