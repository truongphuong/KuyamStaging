using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database.Extensions;

namespace Kuyam.Database
{
    public class PackageIP
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public int ProfileCompanyId { get; set; }
        public int PackageType { get; set; }
        public int NumberOfBooking { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public Nullable<int> DurationInMonth { get; set; }
        public string KalturaImageId { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime CreateDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public List<CompanyService> Services { get; set; }
    }
}
