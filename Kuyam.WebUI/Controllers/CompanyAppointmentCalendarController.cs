using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Utility;
using Kuyam.WebUI.Models;
using Kuyam.Domain;
using System.Configuration;
using System.Threading;
using Kuyam.Repository.Infrastructure;

namespace Kuyam.WebUI.Controllers
{
    public class CompanyAppointmentCalendarController : KuyamBaseController
    {
        private const string COMPANYCALENDAR_EMPID = "COMPANYCALENDAR_EMPID";
        private const string COMPANYCALENDAR_EMPNAME = "COMPANYCALENDAR_EMPNAME";
        private const string COMPANYCALENDAR_APPID = "COMPANYCALENDAR_APPID";
        private readonly NotificationService _notificationService;

        public CompanyAppointmentCalendarController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        //
        // GET: /CompanyAppointmentCalendar/

        public ActionResult Index(int? id)
        {
            Session[COMPANYCALENDAR_EMPID] = 0;
            Session[COMPANYCALENDAR_EMPNAME] = string.Empty;
            Session[COMPANYCALENDAR_APPID] = 0;
            ViewBag.ListEmployee = ProfileCompany.GetEmployeeByProfileId(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            string employeeName = "employee";
            var lstApp = ProfileCompany.GetAppointmentByProfileId(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            ViewBag.Data = string.Empty;
            ViewBag.AppointmentId = 0;
            if (lstApp.Count > 0)
            {
                if (id == null)
                {
                    var currentApp = lstApp[0];
                    ViewBag.AppointmentId = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                }
                else
                {

                    int index = lstApp.FindIndex(a => a.AppointmentID == id);
                    if (index == -1 || index > lstApp.Count - 1)
                        index = 0;
                    var currentApp = lstApp[0];
                    ViewBag.AppointmentId = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                }
            }
            //Trong added
            //Check login with facebook;
            ViewBag.UserType = Types.CustType.Personal;
            var profileCompany = ProfileCompany.GetProfileCompany(ProfileId != 0 ? ProfileId : MySession.ProfileID);
            if (!string.IsNullOrEmpty(profileCompany.Profile.Cust.FacebookUserID))
                ViewBag.UserType = Types.CustType.Facebook;
            //-------------

            ViewBag.EmpName = employeeName;
            return View();
        }

        [HttpGet]
        public ActionResult LoadNextAppointment(int? id)
        {
            ViewBag.Data = string.Empty;
            string employeeName = "employee";
            var lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
            if (id == null)
            {
                var currentApp = lstApp[0];
                if (lstApp.Count > 0)
                {
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            else
            {
                int index = lstApp.FindIndex(a => a.AppointmentID == id);
                if (index == -1 || index >= lstApp.Count - 1)
                    index = 0;
                else
                    index++;
                if (lstApp.Count > 0)
                {
                    var currentApp = lstApp[index];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            return Json(ViewBag.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadCurrentAppointmentId()
        {
            return Json(Session[COMPANYCALENDAR_APPID], JsonRequestBehavior.AllowGet);
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

        private string HtmlAppointment(Appointment currentApp)
        {
            StringBuilder builder = new StringBuilder();
            string today = "today";
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
            string minuteAgo = DateTimeUltility.RelativeDatetime(currentApp.Modified.Value); //(int)DateTimeUltility.ConvertToUtcMinus7(DateTime.Now).Subtract(currentApp.Modified.Value).TotalMinutes;
            string username = currentApp.Cust.FullName;
            username = username.Length > 14 ? username.Substring(0, 14) : username;
            string employeeName = currentApp.CompanyEmployee.EmployeeName;
            employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
            string appDesc = string.Format("{0} {1}min, ${2}", Kuyam.Domain.UtilityHelper.TruncateText(currentApp.ServiceCompany.Service.ServiceName, 20),
                                           currentApp.ServiceCompany.Duration, currentApp.ServiceCompany.Price);
            string appNote = currentApp.Notes ?? string.Empty;
            appNote = appNote.Length > 25 ? appNote.Substring(0, 25) : appNote;
            string modifyApp =
                SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "modify",
                                                                     currentApp.AppointmentID));
            string cancelApp = SecurityHelper.EncryptStringToBytesAes(string.Format("{0}@{1}", "cancel",
                                                                     currentApp.AppointmentID));
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

            builder.Append("<div class=\"title\">");
            builder.AppendFormat("<div class=\"txttitle\">{2}, {0} {1}</div>",
                                 currentApp.Start.ToString("MMMM").ToLower(), currentApp.Start.Day, today);
            //next appointment
            builder.AppendFormat(
                "<a href=\"#\" class=\"nextappointment\" id=\"refNextAppointment\" onclick=\"javascript:goNext({0},'{1}',{2})\">next appointment ></a>",
                currentApp.AppointmentID, employeeName, currentApp.EmployeeID);
            builder.Append("</div>");

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
                                 "<div class=\"clear\">" +
                                 "</div>" +
                                 "<div class=\"description\">" +
                                 "{6} {7}" +
                                 "</div>" +
                                  "<div class=\"clear\" ></div>" +
                                  "<div class=\"reason\">{16}</div>" +
                                 "</div>" +
                                 "</div>" +
                                 "", time, cName, dName, minuteAgo, username,
                                 employeeName, appDesc, appNote, confirmButtonStatus, modifiedButton,
                                 cancelButton, modifyApp, cancelApp, "hdfEmp" + currentApp.EmployeeID,
                                 currentApp.AppointmentID, confirmText, reason);

            if (cancelApp != string.Empty)
            {
                string datetime = currentApp.Start.ToString("ddd, MMM dd, hh:mmt\\M").ToLower();
                string custname = username;
                string compname = currentApp.ServiceCompany.ProfileCompany.Name;
                compname = compname.Length > 25 ? compname.Substring(0, 25) : compname;
                string empname = employeeName;
                string sername = currentApp.ServiceCompany.Service.ServiceName;
                sername = sername.Length > 20 ? sername.Substring(0, 20) : sername;
                string note = appNote;
                note = note.Length > 20 ? note.Substring(0, 20) : note;
                builder.AppendFormat(
                    "<input id=\"hdfEmp{0}\" type=\"hidden\" datetime=\"{1}\" custname=\"for {2}\" compname=\"{3}\" empname=\"{4}\" sername=\"with {5}\" note=\"{6}\" />",
                    currentApp.EmployeeID,
                    datetime, custname, compname, empname, sername, note)
                    ;
            }
            //clear
            builder.Append("<div class=\"clear\"></div>");
            //endbox
            builder.Append("</div>");
            return builder.ToString();
        }

        private List<CalendarObject> GetData(DateTime start, DateTime end)
        {

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
                var eventsAppointment = ProfileCompany.GetKuyamEventsByEmployeeId(empId, start, end);

                if (eventsAppointment != null)
                {
                    foreach (var evt in eventsAppointment)
                    {
                        CalendarObject item = new CalendarObject();

                        item.id = evt.AppointmentID.ToString();
                        item.title = evt.Title;
                        item.start = evt.Start.ToString();
                        item.end = evt.End.ToString();
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
        public ActionResult LoadNextEmployee(int? id)
        {
            string data = string.Format("{0}#_#{1}", Session[COMPANYCALENDAR_EMPID], Session[COMPANYCALENDAR_EMPNAME]);
            return Json(data, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult LoadCurrentAppointment(int? id)
        {
            ViewBag.Data = string.Empty;
            string employeeName = "employee";
            var lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
            if (id == null)
            {
                if (lstApp.Count > 0)
                {
                    var currentApp = lstApp[0];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            else
            {
                if (lstApp.Count > 0)
                {
                    int index = lstApp.FindIndex(a => a.AppointmentID == id);
                    if (index == -1)
                        index = 0;
                    var currentApp = lstApp[index];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            return Json(ViewBag.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadCurrentEmployee(int? id)
        {
            string data = string.Format("{0}#_#{1}", Session[COMPANYCALENDAR_EMPID], Session[COMPANYCALENDAR_EMPNAME]);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadCurrentAppointmentCode(string appCode)
        {
            ViewBag.Data = string.Empty;
            Cust user = MySession.Cust;
            try
            {
                string[] codes = SecurityHelper.DecryptStringFromBytesAes(appCode.Replace(" ", "+")).Split('@');
                var lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
                if (codes.Length == 2 && codes[0] == "cancel" && lstApp.Count > 0)
                {
                    int appId = Int32.Parse(codes[1]);
                    int index = lstApp.FindIndex(a => a.AppointmentID == appId);
                    if (index == -1)
                        index = 0;
                    var currentApp = lstApp[index];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    string employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("LoadCurrentAppointmentCode fail:", ex);
            }
            return Json(ViewBag.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ActionAppointment(int appId, string appCode)
        {
            string data = string.Empty;
            if (appCode.Equals("Remove", StringComparison.InvariantCultureIgnoreCase))
            {
                ProfileCompany.RemoveAppointment(appId);
                //_notificationService.SendAppoinmentChangeNotify(appId);
            }
            if (appCode.Equals("Confirm", StringComparison.InvariantCultureIgnoreCase))
            {
                ProfileCompany.Confirm(appId);
                _notificationService.SendAppointmentChangeNotify(appId);
                try
                {
                    Thread zendeskThread = new Thread(() =>
                    {
                        //Create Zendesk ticket here -- Khoi Tran
                        //Information for the ticket                           
                        Appointment appointment = ProfileCompany.GetAppointmentById(appId);
                        List<AppointmentLog> Notes = appointment.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                        var appart = EngineContext.Current.Resolve<IAppointmentService>().GetAppointmentParticipantByAppointmentId(appId);
                        string methodCommunication = string.Empty;
                        string subject = "";
                        string companyEmail = string.Empty;
                        string companySMS = string.Empty;
                        int? preferedContact = null;
                        if (appointment.ServiceCompany.ProfileCompany.PreferredContact.HasValue) preferedContact = appointment.ServiceCompany.ProfileCompany.PreferredContact.Value;
                        if (preferedContact != null && ((preferedContact.Value == 1)))
                        {
                            methodCommunication = "Email";
                            companyEmail = appointment.ServiceCompany.ProfileCompany.Email;
                        }
                        else if (preferedContact != null && ((preferedContact.Value == 2)))
                        {
                            methodCommunication = "SMS";
                            companySMS = appointment.ServiceCompany.ProfileCompany.Phone;
                        }

                        else if (preferedContact != null && ((preferedContact.Value == 3)))
                        {
                            methodCommunication = "SMS&Email";
                            companySMS = appointment.ServiceCompany.ProfileCompany.Phone;
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


                        TicketStatus status = (TicketStatus.Pending);
                        TicketType type = (TicketType.Incident);
                        TicketPriority priority = (TicketPriority.High);
                        string description = "appointment information: \n"
                            + "company name: " + appointment.ServiceCompany.ProfileCompany.Name.ToLower() + "\n"
                            + "company contact info: " + "\n";
                        if (appointment.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                        {
                            if (methodCommunication == "Email")
                            {
                                description += "   email:	" + appointment.ServiceCompany.ProfileCompany.Email.ToLower() + "\n";
                            }
                            else if (methodCommunication == "SMS")
                            {
                                description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                            }
                            else if (methodCommunication == "SMS&Email")
                            {
                                description += "   email:	" + appointment.ServiceCompany.ProfileCompany.Email.ToLower() + "\n"
                                            + "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                            }
                        }
                        else if (appointment.ServiceCompany.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt)
                        {
                            description += "   phone: " + Kuyam.Domain.UtilityHelper.FormatPhone(appointment.ServiceCompany.ProfileCompany.Phone) + "\n";
                        }

                        //basic info

                        description += "primary contact: " + appointment.ServiceCompany.ProfileCompany.ContactFirstName.ToLower() + " " + appointment.ServiceCompany.ProfileCompany.ContactLastName.ToLower() + "\n"
                                    + "employee name: " + appointment.CompanyEmployee.EmployeeName.ToLower() + "\n"
                                    + "calendar: " + appart.Calendar.Name.ToLower() + "\n"
                                    + "service: " + appointment.ServiceCompany.Service.ServiceName.ToLower() + "\n"
                                    + "duration: " + appointment.ServiceCompany.Duration + "\n"
                                    + "price: " + string.Format("${0:0.00}", appointment.ServiceCompany.Price) + "\n"
                                    + "date: " + string.Format("{0:MMM dd yyyy}", appointment.Start).ToLower() + "\n"
                                    + "time: " + appointment.Start.ToString("h:mm tt").ToLower() + " - " + appointment.End.ToString("h:mm tt").ToLower() + "\n"
                                    + "appointment status: " + (Types.AppointmentStatus.Confirmed).ToString().ToLower() + "\n"
                                     + "appointment notes: " + "\n";
                        description += "\t" + appointment.Notes + "\n";
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
                    //Todo: Handle Exception Occur
                    LogHelper.Error("ActionAppointment fail:", ex);
                }
            }
            var lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
            if (lstApp.Count > 0)
            {
                var currentApp = lstApp[0];
                data = HtmlAppointment(currentApp);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAppointmentByEmployee(int employeeId, int profileId = 0)
        {
            ViewBag.Data = string.Empty;
            string employeeName = "employee";
            var lstApp = new List<Appointment>();
            if (employeeId == 0)
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(profileId != 0 ? profileId : MySession.ProfileID);
                if (lstApp.Count > 0)
                {
                    var currentApp = lstApp[0];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId((profileId != 0 ? profileId : MySession.ProfileID), employeeId);
                if (lstApp.Count > 0)
                {
                    var currentApp = lstApp[0];
                    Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                    Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                    employeeName = currentApp.CompanyEmployee.EmployeeName;
                    employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                    Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                    ViewBag.Data = HtmlAppointment(currentApp);
                }
            }
            return Json(ViewBag.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ActionStatus(string status)
        {
            ViewBag.Data = string.Empty;
            string employeeName = "employee";
            List<int> lstStatus = new List<int>();
            switch (status)
            {
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
                lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID);
            }
            else
            {
                lstApp = ProfileCompany.GetAppointmentByProfileId(MySession.ProfileID, lstStatus);
            }
            if (lstApp.Count > 0)
            {
                var currentApp = lstApp[0];
                Session[COMPANYCALENDAR_EMPID] = currentApp.EmployeeID;
                Session[COMPANYCALENDAR_APPID] = currentApp.AppointmentID;
                employeeName = currentApp.CompanyEmployee.EmployeeName;
                employeeName = employeeName.Length > 6 ? employeeName.Substring(0, 6) : employeeName;
                Session[COMPANYCALENDAR_EMPNAME] = employeeName;
                ViewBag.Data = HtmlAppointment(currentApp);
            }
            return Json(ViewBag.Data, JsonRequestBehavior.AllowGet);
        }
    }

}
