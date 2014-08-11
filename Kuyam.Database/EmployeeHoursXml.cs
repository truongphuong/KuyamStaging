using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Kuyam.Database
{
    public class EmployeeHoursXml
    {
        [XmlArray("EmployeeHours"), XmlArrayItem("EmployeeHour", Type = typeof(EmployeeHourXml))]
        public List<EmployeeHourXml> EmployeeHours { get; set; }
    }

    public class EmployeeHourXml
    {
        public int ID { get; set; }
        public int ServiceCompanyID { get; set; }
        public string ServiceName { get; set; }

        public int AttendeesNumber { get; set; }
        public int CompanyEmployeeID { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsPreview { get; set; }
        public System.DateTime LastedUpdate { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

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
