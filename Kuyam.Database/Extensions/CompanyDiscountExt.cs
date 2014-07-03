using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class CompanyDiscountExt
    {
        public int DiscountId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public decimal Percent { get; set; }
        public int Quantity { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool ApplyToAllServices { get; set; }
        public int ProfileCompanyId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string Description { get; set; }
        public int NumberofSent { get; set; }
        public int Status { get; set; }
        public int NumberOfUsage { get; set; }
        public int ServiceId { get; set; }
        public bool IsSent { get; set; }
    }
}
