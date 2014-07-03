using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;

namespace Kuyam.WebUI.Models
{
    public class ItemCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemCustom(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    public class Event {

        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public Event()
        {

        }
    }
    public class ItemServiceCustom
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public int AttendeesNumber { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }

        public ItemServiceCustom(int id, string serviceName, int attendeesNumber,
            decimal price, int duration, string description)
        {
            Id = id;
            ServiceName = serviceName;
            AttendeesNumber = attendeesNumber;
            Price = price;
            Duration = duration;
            Description = description;

        }
    }

    public class EventCustom
    {
        public int EmployeeID { get; set; }
        public int ProfileID { get; set; }
        public string EmployeeName { get; set; }
        public int DayOfWeek { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ClassCustom { get; set; }
        public int Hour { get; set; }
        public string title { get; set; }        
        public EventCustom()
        {

        }

    }
}