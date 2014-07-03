using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class CustIPhone
    {
        public int CustID { get; set; }
        public int AccountID { get; set; }
        public string Email { get; set; }
        public System.Guid AspUserID { get; set; }
        public int CustTypeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string MobileCarrier { get; set; }
        public string WorkPhone { get; set; }
        public Nullable<int> PreferredPhoneTypeID { get; set; }
        public Nullable<int> FirstAlert { get; set; }
        public Nullable<int> SecondAlert { get; set; }
        public string IcalUrl { get; set; }
        public bool EmailHtml { get; set; }
        public Nullable<int> GenderTypeID { get; set; }
        public int CustStatusTypeID { get; set; }
        public Nullable<int> CustStatusReasonTypeID { get; set; }
        public string CustStatusNote { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
        public string Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public System.DateTime LastVisit { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OtherCalendar { get; set; }
        public bool OutlookCalendar { get; set; }
        public bool YahooCalendar { get; set; }
        public string TimeZoneId { get; set; }
        public int Status { get; set; }
        public string FacebookToken { get; set; }
        public string FacebookUserID { get; set; }
        public bool? LocationReminder { get; set; }

        public bool IsPhoneVerified
        {
            get
            {
                if (string.IsNullOrEmpty(MobilePhone))
                    return false;
                System.Text.RegularExpressions.Regex digitsOnly = new System.Text.RegularExpressions.Regex(@"[^\d;]");
                string phone = digitsOnly.Replace(MobilePhone, "");

                return DAL.GetInviteByPhoneNumber(phone) != null;
            }
        }

        public CustIPhone()
        {

        }

        public CustIPhone(Cust cust)
        {
            this.CustID = cust.CustID;
            //this.AccountID = cust.AccountID;
            this.AspUserID = cust.AspUserID;
            this.Email = cust.Email;
            this.CustTypeID = cust.CustTypeID;
            this.FirstName = cust.FirstName;
            this.LastName = cust.LastName;
            this.Street1 = cust.Street1;
            this.Street2 = cust.Street2;
            this.City = cust.City;
            this.State = cust.State;
            this.Zip = cust.Zip;
            this.HomePhone = cust.HomePhone;
            this.MobilePhone = cust.MobilePhone;
            this.MobileCarrier = cust.MobileCarrier;
            this.WorkPhone = cust.WorkPhone;
            this.PreferredPhoneTypeID = cust.PreferredPhoneTypeID;
            this.FirstAlert = cust.FirstAlert;
            this.SecondAlert = cust.SecondAlert;
            this.IcalUrl = cust.IcalUrl;
            this.EmailHtml = cust.EmailHtml;
            this.GenderTypeID = cust.GenderTypeID;
            this.CustStatusTypeID = cust.CustStatusTypeID;
            this.CustStatusReasonTypeID = cust.CustStatusReasonTypeID;
            this.CustStatusNote = cust.CustStatusNote;
            this.Notes = cust.Notes;
            this.Birthday = cust.Birthday;
            this.LastLogin = cust.LastLogin;
            this.Created = cust.Created.ToString("yyyy-MM-dd HH:mm:ss");
            this.Modified = cust.Modified;
            this.LastVisit = cust.LastVisit;
            this.Latitude = cust.Latitude;
            this.Longitude = cust.Longitude;
            this.OtherCalendar = cust.OtherCalendar;
            this.OutlookCalendar = cust.OutlookCalendar;
            this.YahooCalendar = cust.YahooCalendar;
            this.TimeZoneId = cust.TimeZoneId;
            this.Status = cust.Status;
            this.FacebookToken = cust.FacebookToken;
            this.FacebookUserID = cust.FacebookUserID;
            this.LocationReminder = cust.LocationReminder;

        }
    }
}
