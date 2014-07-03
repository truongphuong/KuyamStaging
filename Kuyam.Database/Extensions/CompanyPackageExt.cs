using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class CompanyPackageExt
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
        public List<int> Services { get; set; }
        public List<string> KalturaImages { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal? UnitPrice { get; set; }
        public int CountPurchase
        {
            get 
            {
                return DAL.GetCountPurchaseByPackageId(this.PackageId);
            } 
        }
    }
}
