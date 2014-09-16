using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kuyam.Database
{
    [XmlRoot("ServicesXml")]
    public class ServicesXml
    {
        [XmlArray("Services"), XmlArrayItem("Service", Type = typeof(ServiceXml))]
        public List<ServiceXml> Services { get; set; }
    }
    public class ServiceXml
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
    }
}
