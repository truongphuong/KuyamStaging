using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
    public class CheckoutModel
    {
        public CheckoutModel()
        {
            this.PaymentMethod = 0;
            this.Duration = 0;
            this.Price = 0;
            this.Totaldue = 0;
            this.IsPackage = false;
            this.CheckoutType = 0;
            this.NonApptTempId = 0;
            this.ProfileId = 0;
        }
        public string CheckOutSummary { get; set; }
        public int NonApptTempId { get; set; }
        public int ProfileId { get; set; }
        public int CheckoutType { get; set; }
        public int PaymentMethod { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ServiceName { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public string EmployeeName { get; set; }
        public string CalendarName { get; set; }
        public string Datetime { get; set; }
        public string Time { get; set; }
        public decimal Totaldue { get; set; }
        public string PackageId { get; set; }
        public bool IsPackage { get; set; }
    }
}