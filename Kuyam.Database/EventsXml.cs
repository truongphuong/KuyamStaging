using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kuyam.Database
{
    [XmlRoot("EventsXml")]
    public class EventsXml
    {
        [XmlArray("Events"), XmlArrayItem("Event", Type = typeof(EventXml))]
        public List<EventXml> Events { get; set; }
    }

    public class EventXml
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }

        public int CompanyEventID { get; set; }
        public int CompanyServiceEventsNumber { get; set; }

        public int ClassEventsNumber { get; set; }
    }
}