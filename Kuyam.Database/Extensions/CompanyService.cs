using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class CompanyService
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public int ServiceID { get; set; }
        public string EmployeeName { get; set; }
        public string ServiceName { get; set; }
        public int AttendeesNumber { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public int ServiceTypeId { get; set; }
        public List<ServiceHour> ServiceHour { get; set; }
        public List<CompanyEmployee> Employee { get; set; }
    }
    public class CompanyPackageObject
    {
        public int ID { get; set; }
        public int PackageID { get; set; }
        public int ProfileId { get; set; }
        public string PackageName { get; set; }
        public string PackageDesscription { get; set; }
        public string PackagePrice { get; set; }
        public string PackageExpiredDate { get; set; }
        public string PackageCompanyName { get; set; }
        public string PackageRemain { get; set; }

    }
}
