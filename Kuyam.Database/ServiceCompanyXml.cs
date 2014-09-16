using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kuyam.Database
{

    [XmlRoot("ServiceCompaniesXml")]
    public class ServiceCompaniesXml
    {
        [XmlArray("ServiceCompanies"), XmlArrayItem("ServiceCompany", Type = typeof(ServiceCompanyXml))]
        public List<ServiceCompanyXml> ServiceCompanies { get; set; }
    }


    public class ServiceCompanyXml
    {
        public int ServiceCompanyID { get; set; }
        public int ProfileID { get; set; }
        public int ServiceID { get; set; }
        public System.DateTime? Created { get; set; }
        public System.DateTime? Modified { get; set; }
        public System.DateTime? FromDateTime { get; set; }
        public System.DateTime? ToDateTime { get; set; }
        public decimal? Price { get; set; }
        public int? Duration { get; set; }
        public int? AttendeesNumber { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string ServiceName { get; set; }
        public string EmployeeName { get; set; }
        public int ServiceTypeId { get; set; }
        public bool IsPerDay { get; set; }

    }
}
