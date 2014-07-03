using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web.Security;
using System.Web;


namespace Kuyam.Database
{
    public static class CustExt
    {
        public static List<Cust> Decrypt(this List<Cust> custs)
        {
            foreach (Cust c in custs)
                c.Decrypt();
            return custs;
        }
    }

    public partial class Cust
    {
        public Cust Encrypt()
        {
            FirstName = CryptoHelper.Encrypt(FirstName);
            LastName = CryptoHelper.Encrypt(LastName);
            Street1 = CryptoHelper.Encrypt(Street1);
            Street2 = CryptoHelper.Encrypt(Street2);
            HomePhone = CryptoHelper.Encrypt(HomePhone);
            MobilePhone = CryptoHelper.Encrypt(MobilePhone);
            WorkPhone = CryptoHelper.Encrypt(WorkPhone);

            return this;
        }

        private string[] _userRole;
        public Cust Decrypt()
        {
            FirstName = CryptoHelper.Decrypt(FirstName);
            LastName = CryptoHelper.Decrypt(LastName);
            Street1 = CryptoHelper.Decrypt(Street1);
            Street2 = CryptoHelper.Decrypt(Street2);
            HomePhone = CryptoHelper.Decrypt(HomePhone);
            MobilePhone = CryptoHelper.Decrypt(MobilePhone);
            WorkPhone = CryptoHelper.Decrypt(WorkPhone);

            return this;
        }

        public static Cust Load(int id)
        {
            Cust c = DAL.xGetCust(id);
            return c.Decrypt();
        }

        public static HotelStaff GetConcierge(int custId, int hotelId)
        {
            return DAL.GetConcierge(hotelId, custId);
        }

        public static HotelStaff GetConcierge(int custId)
        {
            return DAL.GetConcierge(custId);
        }

        public static Cust Load(string username)
        {
            if (HttpContext.Current.Items["HttpContext.Cust"] == null)
            {
                var user = DAL.xGetCust(username);
                if (user == null)
                    return null;
                var decrypt = user.Decrypt();
                HttpContext.Current.Items["HttpContext.Cust"] = decrypt;
                return decrypt;
            }
            return (Cust)HttpContext.Current.Items["HttpContext.Cust"];

        }

        public HotelVisit GetHotelVisit
        {
            get
            {
                return DAL.GetHotelVisit(this.CustID);
            }
        }
        public string[] GetRole
        {
            get
            {
                if (_userRole == null)
                    _userRole = Roles.GetRolesForUser(this.Username);
                return _userRole;
            }


        }
        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
            }
        }

        public IQueryable<Appointment> Appointments2()
        {
            return DAL.GetCustAppointments(CustID, DateTime.Today, DateTime.MaxValue);
        }

        public int Age
        {
            get
            {
                if (Birthday != null)
                    return (int)DateTime.Now.Subtract(Birthday.Value).TotalDays / 365;
                else
                    return -1;
            }
        }

        public List<Cust> GetCusts()
        {
            List<Cust> custs = DAL.xGetCusts();
            return custs.Decrypt();
        }

        public static int Create(string username, Guid aspUserid, string firstName,
            string lastName, string CompanyName, string mobilePhone, string mobileCarrier, bool isAdmin, int custType, string zipCode,
            int PreferredPhone, DateTime? birthday, bool isCompany, bool YahooCalendar, bool OutlookCalendar,
            string OtherCalendar, double lat, double lon, int GenderTypeID, string FacebookUserID = "")
        {
            //bool isPersonal = (custType != (int)Types.CustType.Company);

            // Create customer
            Cust c = new Cust();
            c.AspUserID = aspUserid;
            c.CustTypeID = custType;
            c.FirstName = firstName;
            c.LastName = lastName;
            c.MobilePhone = mobilePhone;
            c.MobileCarrier = mobileCarrier;
            c.Birthday = birthday;
            c.LastVisit = DateTime.UtcNow;
            c.PreferredPhoneTypeID = (int)Types.PreferredPhone.Call;
            c.CustStatusTypeID = (int)Types.CustStatus.Definition;
            c.Zip = zipCode;
            c.Encrypt();
            c.PreferredPhoneTypeID = PreferredPhone;
            c.OutlookCalendar = OutlookCalendar;
            c.YahooCalendar = YahooCalendar;
            c.OtherCalendar = OtherCalendar;
            c.Created = DateTime.UtcNow;
            c.GenderTypeID = GenderTypeID;
            c.Latitude = lat;
            c.Longitude = lon;
            c.Status = (int)Types.UserStatusType.Active;
            c.FacebookUserID = FacebookUserID;
            c.TimeZoneId = DateTimeUltility.CurrentTimeZone.Id;
            DAL.CreateCust(c);

            // Create ProfileCompany if needed
            //if (isCompany)
            //{
            //    // Create first profile and calendar
            //    Profile p = Profile.Create((Types.CustType)custType, c.CustID);
            //    Calendar cal = Calendar.Create((Types.CustType)custType, p.ProfileID, null, true);
            //    p.CreateCompany(Types.CompanyCategory.Unknown, CompanyName, phone, Types.CompanyStatus.Unverified, zipCode);

            //}

            return c.CustID;
        }

        public void Update()
        {
            Encrypt();
            DAL.UpdateRec(this, CustID);
            Decrypt();
        }

        public int GetUnviewedNotificationCount()
        {
            return DAL.GetUnviewedNotificationCount(CustID);
        }

        public static void EncryptAll()
        {
            var custs = DAL.xGetCusts();
            foreach (Cust c in custs)
            {
                c.Update();
            }
        }

        public List<Calendar> GetCalendars()
        {
            return DAL.GetCustCalendars(CustID).ToList();
        }

        public List<Calendar> GetCalendars(List<int> calIDs)
        {
            return DAL.GetCustCalendars(CustID, calIDs).ToList();
        }

        public List<Calendar> GetSelectedCalendars()
        {
            return DAL.GetSelectedCalendarsForCust(CustID);
        }

        public void ResetSelectedCalendars()
        {
            DAL.ResetSelectedCalendars(CustID);
        }

        public Calendar GetDefaultCalendar()
        {
            return DAL.GetDefaultCalendarForCust(CustID);
        }

        public AppointmentParticipant GetAppointmentParticipant(int apptID)
        {
            return DAL.GetCustAppointmentParticipant(this.CustID, apptID);
        }

        public int GetCompanyProfileID()
        {
            return DAL.GetCustCompanyProfileID(this.CustID);
        }

        public string Username
        {
            get
            {
                return DAL.GetUsername(AspUserID);
            }

        }


        public string UserAcount { get; set; }

        public string Password { get; set; }

        public string CompanyName { get; set; }

        public int CustType { get; set; }

        public string InviteCode { get; set; }

        public int PreferredPhone { get; set; }

        private string _custEmail = null;

        public bool HasDevice { get; set; }
        public string Email
        {
            get
            {
                if (_custEmail == null)
                {
                    var user = Membership.GetUser(Username);
                    if (user != null)
                        _custEmail = user.Email;
                }
                return _custEmail;
            }
        }

        public bool IsAgent { get; set; }

    }
}
