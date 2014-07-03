using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class UserPackagePurchaseIP
    {
        public int UserPackagePurchaseId { get; set; }
        public int CompanyPackageId { get; set; }
        public string PurchaseDate { get; set; }
        public decimal OrginalPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public int CustID { get; set; }
        public string ExpiredDate { get; set; }
        public int MaxUses { get; set; }
        public string CompanyName { get; set; }
        public Nullable<int> UserPackageStatus { get; set; }
        public PackageIP PackageDetail { get; set; }
    }

    public class CityState
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
    }
}
