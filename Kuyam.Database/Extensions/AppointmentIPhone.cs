using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class AppointmentIPhone
    {
        public string CompanyName { get; set; }
        public int ProfileID { get; set; }
        public int AppointmentID { get; set; }
        public int AppointmentStatusID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int CalendarID { get; set; }
        public string CalendarName { get; set; }
        public string CalendarColor { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int Duration { get; set; }
        public decimal? Price { get; set; }
        public string ServiceName { get; set; }
        public int NumberAppointmentHasSameStatus { get; set; }
        public int? CancelPolicy { get; set; }
        public int? CancelHour { get; set; }
        public int? CancelRefundPercent { get; set; }
        public List<AppointmentLogIPhone> Notes { get; set; }
        public bool IsViewDetail { get; set; }
        public int Rating { get; set; }
        public string Created { get; set; }
        public int ServiceCompanyID { get; set; }
        public bool IsReviewed { get; set; }
        public int NumberNotesUnread { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public AppointmentIPhone() { }

        public AppointmentIPhone(Appointment appointment, int calendarId = 0, string calendarName = "")
        {
            ProfileCompany profileCompany = appointment.ServiceCompany != null
                                                ? appointment.ServiceCompany.ProfileCompany
                                                : appointment.ProfileCompany;
            ServiceCompany serviceCompany = appointment.ServiceCompany;
            string serviceName = serviceCompany != null ? serviceCompany.Service.ServiceName : appointment.ServiceName;
            AppointmentID = appointment.AppointmentID;
            AppointmentStatusID = appointment.AppointmentStatusID;
            CompanyName = profileCompany.Name;
            ProfileID = profileCompany.ProfileID;
            CalendarID = calendarId;
            CalendarName = calendarName;
            ServiceName = serviceName;
            EmployeeID = appointment.CompanyEmployee != null
                             ? appointment.CompanyEmployee.EmployeeID
                             : appointment.EmployeeID.HasValue
                                   ? appointment.EmployeeID.Value
                                   : 0;
            EmployeeName = appointment.CompanyEmployee != null
                               ? appointment.CompanyEmployee.EmployeeName
                               : appointment.EmployeeName;
            Start = appointment.Start.ToString("yyyy-MM-dd HH:mm:ss");
            End = appointment.End.ToString("yyyy-MM-dd HH:mm:ss");
            Duration = (int)(appointment.End - appointment.Start).TotalMinutes;
            Price = appointment.Price;
            Rating = appointment.Rating.HasValue ? appointment.Rating.Value : 0;
            CancelPolicy = profileCompany.CancelPolicy;
            CancelHour = profileCompany.CancelHour;
            CancelRefundPercent = profileCompany.CancelRefundPercent;
            IsViewDetail =
                appointment.IsViewDetail.HasValue && appointment.IsViewDetail.Value;
            ServiceCompanyID = serviceCompany != null ? serviceCompany.ServiceCompanyID : 0;
            NumberNotesUnread = appointment.GetUnreadNotes.Count;
            Latitude = profileCompany.Latitude;
            Longitude = profileCompany.Longitude;
            switch (AppointmentStatusID)
            {
                case (int)Types.AppointmentStatus.CompanyModified:
                    AppointmentStatusID = (int)Types.AppointmentStatus.Modified;
                    break;
            }
        }

        public AppointmentIPhone(ProposedAppointment appointment)
        {
            ProfileCompany profileCompany = appointment.ServiceCompany != null
                                                ? appointment.ServiceCompany.ProfileCompany
                                                : appointment.ProfileCompany;            
            string serviceName = appointment.ServiceCompany != null ? appointment.ServiceCompany.Service.ServiceName : appointment.Service.ServiceName;
            AppointmentID = appointment.AppointmentID;
            AppointmentStatusID = appointment.AppointmentStatusID;
            CompanyName = profileCompany.Name;
            ProfileID = profileCompany.ProfileID;
            CalendarID = appointment.CalendarId ?? 0;
            CalendarName = appointment.Calendar.Name;
            CalendarColor = appointment.Calendar.BackColor;
            ServiceCompanyID = appointment.ServiceCompany != null ? appointment.ServiceCompany.ServiceCompanyID : appointment.Service.ServiceID;
            ServiceName = serviceName;
            EmployeeID = 0;
            EmployeeName = appointment.EmployeeName;
            Start = appointment.Start.ToString("yyyy-MM-dd HH:mm:ss");
            End = appointment.End.ToString("yyyy-MM-dd HH:mm:ss");
            Duration = appointment.Duration ?? 0;
            Price = appointment.Price;
            Rating = 0;
            CancelPolicy = profileCompany.CancelPolicy;
            CancelHour = profileCompany.CancelHour;
            CancelRefundPercent = profileCompany.CancelRefundPercent;
            IsViewDetail = false;
            NumberNotesUnread = 0;
            Latitude = profileCompany.Latitude;
            Longitude = profileCompany.Longitude;

        }

        public AppointmentIPhone(Appointment appointment, AppointmentParticipant appointmentParticipant, DateTime? currentTime = null)
            : this(appointment)
        {
            if (appointmentParticipant != null)
            {
                CalendarID = appointmentParticipant.Calendar.CalendarID;
                CalendarName = appointmentParticipant.Calendar.Name;
                CalendarColor = appointmentParticipant.Calendar.BackColor;
            }

            if (currentTime.HasValue)
            {
                NumberAppointmentHasSameStatus = DAL.GetCountOfAppointmentHasSameStatus(AppointmentID, currentTime.Value);
            }
        }

        public AppointmentIPhone(Appointment appointment, AppointmentParticipant appointmentParticipant, DateTime currentTime, bool isGetNotes = false)
            : this(appointment, appointmentParticipant, (DateTime?)currentTime)
        {
            if (isGetNotes)
                Notes = DAL.GetAppointmentNoteIPhonesByAppointmentID(appointment.AppointmentID).Select(n =>
                    new AppointmentLogIPhone
                    {
                        AppointmentLogID = n.AppointmentID,
                        CustID = n.CustID,
                        Message = n.Message.Replace("<br/>", "\n"),
                        Viewed = n.Viewed,
                        DateString = n.LogDT.ToString("yyyy-MM-dd HH:mm:ss"),
                        LogDT = n.LogDT
                    }).OrderByDescending(m => m.LogDT).ToList();
        }
    }

    public class AppointmentLogIPhone
    {
        public int AppointmentLogID { get; set; }
        public int CustID { get; set; }
        public string Message { get; set; }
        public bool Viewed { get; set; }
        public string DateString { get; set; }
        public System.DateTime LogDT { get; set; }
    }


}
