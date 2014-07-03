using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Appointment
    {
        public int CalendarId { get; set; }
        public static Appointment Load(int id)
        {
            return DAL.GetAppointment(id);
        }
        /// <summary>
        /// To the appointment temp.
        /// </summary>
        /// <returns></returns>
        public AppointmentTemp ToAppointmentTemp()
        {
            return new AppointmentTemp()
            {
                ProfileId = this.ProfileId,
                Title = this.Title,
                Url = this.Url,
                Notes = this.Notes,
                Desc = this.Desc,
                Rating = this.Rating,
                AllDay = this.AllDay,
                AppointmentID = this.AppointmentID,
                AppointmentStatusID = (int)Types.AppointmentStatus.TemporaryPending,               
                ContactPerson = this.ContactPerson,
                ContactType = this.ContactType,
                CustID = this.CustID,
                CalendarId = this.CalendarId,
                EmployeeID = this.EmployeeID,
                StaffID = this.StaffID,
                HotelID = this.HotelID,
                ServiceCompanyID = this.ServiceCompanyID,
                ClassSchedulerID = this.ClassSchedulerID,
                Start = this.Start,
                End = this.End,
                PersonCount = this.PersonCount,
                EmployeeName = this.EmployeeName,
                ServiceName = this.ServiceName,
                Price = this.Price,
                Duration = this.Duration,
                AttendeesNumber = this.AttendeesNumber,
                PreapprovalKey = this.PreapprovalKey,
                SenderEmail = SenderEmail,
                Created = this.Created,
                Modified = this.Modified,

            };
        }

        public ProfileCompany GetProfileCompany()
        {
            if (this.ServiceCompanyID.HasValue)
            {
                var serviceCompany = DAL.GetServiceCompany(this.ServiceCompanyID ?? 0);
                if (serviceCompany != null)
                {
                    return serviceCompany.ProfileCompany;
                }
            }

            if (this.ProfileId.HasValue)
            {
                return DAL.GetProfileCompany(this.ProfileId ?? 0);
            }
            return null;
        }

        public List<Profile> GetProfiles(Cust cust = null)
        {
            return DAL.GetAppointmentProfiles(this, cust);
        }

        public List<Cust> GetCusts()
        {
            List<Cust> custs = DAL.GetAppointmentCusts(this);
            return custs.Decrypt();
        }

        public Dictionary<Cust, string> GetParticipants()
        {
            Dictionary<Cust, string> ret = DAL.GetAppointmentParticipantNames(this);
            foreach (Cust c in ret.Keys)
                c.Decrypt();
            return ret;
        }

        public List<Calendar> GetCalendars(int custID)
        {
            return DAL.GetAppointmentCalendarsForCust(AppointmentID, custID).ToList();
        }

        public List<int> GetParticipantProfileIDs()
        {
            List<int> ret = DAL.GetAppointmentProfiles(this, null, Types.CustType.Personal).Select(x => x.ProfileID).ToList();
            return ret;
        }

        public void LogChange(string msg)
        {
            List<Cust> custs = GetCusts();
            foreach (Cust c in custs)
                DAL.LogAppointmentChange(this, msg);
        }

        public void GetObjects(ref Cust c, ref ProfileCompany co)
        {
            throw new NotImplementedException();
        }

        public Calendar Owner
        {
            get
            {
                return DAL.GetAppointmentCalendars(this, Types.AppointmentParticipantType.Owner).FirstOrDefault();
            }
            set
            {
                DAL.SetAppointmentOwner(this, value);
            }
        }

        public void SetNotificationsViewed(int custid)
        {
            DAL.SetNotificationsViewedForAppointment(this.AppointmentID, custid);
        }

        public int GetCalendarID(Cust c)
        {
            return DAL.GetCalendarIDForCustID(this.AppointmentID, c.CustID);
        }

        public void DeleteAppointmentParticipant(int apID)
        {
            DAL.DeleteAppointmentParticipant(apID);
        }

        public List<AppointmentNotify> GetUnreadNotes
        {
            get { return DAL.GetUnreadNotes(this.AppointmentID); }
        }

    }
}
