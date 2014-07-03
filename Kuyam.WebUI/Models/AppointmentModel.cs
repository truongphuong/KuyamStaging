using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;
using System.Web.Mvc;

using M2.Util;
using M2.Util.MVC;
using Kuyam.Domain;

namespace Kuyam.WebUI.Models
{
    public class AppointmentReviewModel
    {
        public AppointmentReviewModel() { }

        public int Id { get; set; }
        public string ServiceName { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public string PriviteContent { get; set; }
        public string CompanyName { get; set; }
        public string EmployeeName { get; set; }
        public string ServiceDate { get; set; }
        public int ServiceCompanyID { get; set; }
    }

    public class RequestAppointmentModel
    {
        public int Id { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Notes { get; set; }
        public int CustID { get; set; }
        public int? CalendarId { get; set; }
        public int ProfileId { get; set; }

    }

    public class CategoryModel
    {
        public CategoryModel()
        {
            ServicesList = new List<SelectListItem>();
        }

        public CategoryModel(Service entity)
        {
            if (entity != null)
            {
                this.ServiceID = entity.ServiceID;
                this.ServiceName = entity.ServiceName;
                this.kalturaId = entity.kalturaId;
            }

        }

        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string kalturaId { get; set; }
        public IList<SelectListItem> ServicesList { get; set; }
    }

    public class SetrviceModel
    {
        public SetrviceModel()
        {
        }
        public int serviceid { get; set; }
        public string servicename { get; set; }
    }


    #region not use
    /*
    public class TimeSlot
    {
        public int Index { get; set; }
        public DateTime Start { get; set; }
        public int Duration { get; set; }   // minutes

        public string Caption
        {
            get
            {
                return String.Format("{0}-{1}",
                    Start.ToString("mmmm, MM/dd/yy hh:mm"),
                    Start.AddMinutes(Duration).ToString("hh:mm"));
            }
        }
}

    public class ScheduleAppointmentModel : IModel
    {
        public Appointment Appointment { get; set; }
        public Cust Cust { get; set; }
        public List<Cust> Custs { get; set; }
        public ProfileCompany Company { get; set; }
        public string ActionTitle { get; set; }

        public CompanySearchModel CompanySearchModel { get; set; }

        public int NewCalendarID { get; set; }
        public int OldCalendarID { get; set; }

        public TimeSlot TimeSlot { get; set; }
        //public List<TimeSlot> TimeSlots { get; set; }
        //public SelectList TimeSlotSelectList { get; set; }
        public bool Flexible { get; set; }
        public SelectList CalendarSelectList { get; set; }

        public DateTime ApptStartDate { get; set; }
        public DateTime ApptStartTime { get; set; }

        public SelectList ProfileSelectList { get; set; }
        public List<int> SelectedProfileIDs { get; set; }

        public AppointmentParticipant AppointmentParticipant { get; set; }

        public ScheduleAppointmentModel()
        {
            Init();
        }

        public void Init(Appointment appt = null)
        {
            if (appt == null)
                appt = new Appointment();

            Appointment = appt;
            Cust = new Cust();
            Custs = new List<Cust>();
            Company = new ProfileCompany();
            AppointmentParticipant = new AppointmentParticipant();
            CompanySearchModel = new CompanySearchModel();
            Flexible = true;
            SelectedProfileIDs = new List<int>();
        }

        public void LockAndLoad()
        {
            //if (Company == null && Appointment != null && Appointment.AppointmentParticipants.Count > 0)
            //    ProfileCompany = AppointmentHelper.GetCompany(Appointment);

            // Figure out timeslots
            //TimeSlots = null; // GetTimeSlots(Company, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(2));

            // Setup selectlistscale
            //TimeSlotSelectList = new SelectList(TimeSlots.AsEnumerable(), "Index", "Caption");

            Cust cust = MySession.Cust;
            List<Calendar> cals = cust.GetCalendars();
            var calNames = from c in cals orderby c.Name select new { text=(c.Name.SubstringSafe(0,33) + (MySession.DebugMode ? " (" + c.CalendarID.ToString() + ")" : null)), value=c.CalendarID };

            OldCalendarID = NewCalendarID = Appointment.GetCalendarID(Cust);
            CalendarSelectList = new SelectList(calNames, "value", "text", OldCalendarID);

            CompanySearchModel = new CompanySearchModel();
            CompanySearchModel.LockAndLoad();

            ApptStartDate = Appointment.Start;
            ApptStartTime = Appointment.Start;

            SelectedProfileIDs = Appointment.GetParticipantProfileIDs();
            ProfileSelectList = new SelectList(Cust.Profiles.Where(x => x.ProfileTypeID != (int)Types.CustType.Company && !x.IsDefault), "ProfileID", "Name", SelectedProfileIDs);

            if (Appointment.AppointmentID > 0)
                Company = Appointment.GetProfileCompany();

            AppointmentParticipant = Cust.GetAppointmentParticipant(Appointment.AppointmentID);
        }

        public void PostProcess(HttpRequestBase req)
        {
            Appointment.Start = new DateTime(ApptStartDate.Year, ApptStartDate.Month, ApptStartDate.Day, ApptStartTime.Hour, ApptStartTime.Minute, ApptStartTime.Second);
            if (NewCalendarID <= 0)
            {
                NewCalendarID = MySession.Cust.GetDefaultCalendar().CalendarID;
            }
            SelectedProfileIDs = InputHelper.GetCheckboxListSelections(req, "SelProf");
        }

        public MvcHtmlString Dump()
        {
            return Util.FormatDebugValues("apptmodel", "apptid", Appointment != null ? Appointment.AppointmentID : 0, "oldcalid", OldCalendarID, "newcalid", NewCalendarID, "profileCompany", Company != null ? Company.ProfileID : 0);
        }

        //public List<TimeSlot> GetTimeSlots(Company v, DateTime start, DateTime end)
        //{
        //    List<TimeSlot> timeSlots = new List<TimeSlot>();

        //    // TODO: Calc timeslots

        //    return timeSlots;
        //}
    }

    public class ReviewAppointmentModel : IModel
    {
        public Appointment Appointment { get; set; }
        public ProfileCompany Company { get; set; }
        public string ActionTitle { get; set; }

        public int NewCalendarID { get; set; }
        public int OldCalendarID { get; set; }
        public SelectList CalendarSelectList { get; set; }

        public ReviewAppointmentModel()
        {
            Init();
        }

        public void Init(Appointment appt = null)
        {
            if (appt == null)
                appt = new Appointment();

            Appointment = appt;
            Company = new ProfileCompany();
        }

        public void LockAndLoad()
        {
            Cust cust = MySession.Cust;
            List<Calendar> cals = cust.GetCalendars();
            var calNames = from c in cals orderby c.Name select new { text = (c.Name.SubstringSafe(0, 33) + (MySession.DebugMode ? " (" + c.CalendarID.ToString() + ")" : null)), value = c.CalendarID };

            //OldCalendarID = NewCalendarID = Appointment.GetCalendarID(Cust);
            CalendarSelectList = new SelectList(calNames, "value", "text", OldCalendarID);

            if (Appointment.AppointmentID > 0)
                Company = Appointment.GetProfileCompany();
        }

        public void PostProcess()
        {
        }

        public MvcHtmlString Dump()
        {
            return Util.FormatDebugValues("revapptmodel", "apptid", Appointment != null ? Appointment.AppointmentID : 0, "oldcalid", OldCalendarID, "newcalid", NewCalendarID, "profileCompany", Company != null ? Company.ProfileID : 0);
        }
    }


    public class ProcessAppointmentModel : IModel
    {
        public Appointment Appointment { get; set; }
        public List<Cust> Custs { get; set; }
        public int Action { get; set;}
        public string ActionMessage { get; set;}

        public SelectList Actions { get; set; }

        public string Referrer { get; set; }

        public void Init(Appointment appt = null)
        {
            Appointment = appt;
        }

        public void LockAndLoad()
        {
            Custs = Appointment.GetCusts();
            Actions = Types.GetTypeList(Types.TypeGroup.AppointmentCompanyAction).ToSelectList();
        }
    }
    */
    #endregion
}