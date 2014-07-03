using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class DiscountExt
    {
        public int discountid { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public decimal amount { get; set; }
        public decimal percent { get; set; }
        public int quantity { get; set; }
        public bool applytoallservices { get; set; }
        public int profilecompanyid { get; set; }
        public int status { get; set; }
        public string ServerTimezone { get; set; }
        public string ServerDate { get; set; }
    }
}
