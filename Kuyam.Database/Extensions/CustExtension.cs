using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database.Extensions
{
    public class CustExtension
    {
        public int CustID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public Guid AspUserID { get; set; }
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
        public int PreferredPhoneTypeID { get; set; }
        public int? FirstAlert { get; set; }
        public int? SecondAlert { get; set; }
        public int? GenderTypeID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }      
        public string TimeZoneId { get; set; }
        public int Status { get; set; }
        public string FacebookToken { get; set; }
        public string FacebookUserID { get; set; }
        public bool? LocationReminder { get; set; }
    }
}
