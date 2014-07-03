using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models.BookKing
{
    public class AppointmentBookingModel
    {
        public string SMS { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public decimal Price { get; set; }
        public int? Duration { get; set; }
        public string PromoCode { get; set; }
    }
}