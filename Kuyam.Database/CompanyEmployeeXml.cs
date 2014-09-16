using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kuyam.Database
{

    [XmlRoot("CompanyEmployeesXml")]
    public class CompanyEmployeesXml
    {
        [XmlArray("CompanyEmployees"), XmlArrayItem("CompanyEmployee", Type = typeof(CompanyEmployeeXml))]
        public List<CompanyEmployeeXml> CompanyEmployees { get; set; }
    }

    public class CompanyEmployeeXml
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int ProfileCompanyID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsDefault { get; set; }
        public string PaymentAccount { get; set; }
        public int? EmployeeTypeId { get; set; }
    }
}
