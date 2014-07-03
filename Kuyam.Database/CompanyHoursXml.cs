using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Kuyam.Database
{
    [XmlRoot("CompanyHoursXml")]
    public class CompanyHoursXml
    {
        [XmlArray("CompanyHours"), XmlArrayItem("CompanyHour", Type = typeof(CompanyHourXml))]
        public List<CompanyHourXml> CompanyHours { get; set; }
    }

    public class CompanyHourXml
    {
        public int ID { get; set; }

        
        public int CompanyEmployeeID { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsDaily { get; set; }
        public System.DateTime LastedUpdate { get; set; }


        [XmlIgnore]
        public TimeSpan FromHour { get; set; }

        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "FromHourString")]
        public string FromHourString
        {
            get
            {
                return XmlConvert.ToString(FromHour);
            }
            set
            {
                FromHour = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : TimeSpan.ParseExact(value, "hh':'mm':'ss", null);
            }
        }

        [XmlIgnore]
        public System.TimeSpan ToHour { get; set; }

        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "ToHourString")]
        public string ToHourString
        {
            get
            {
                return XmlConvert.ToString(ToHour);
            }
            set
            {
                ToHour = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : TimeSpan.ParseExact(value, "hh':'mm':'ss", null);
            }
        }
    }
}