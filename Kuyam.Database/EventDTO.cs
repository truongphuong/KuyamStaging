using System;

namespace Kuyam.Database
{
    public class EventDTO
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CompanyEventID { get; set; }

        public int CompanyServiceEventsNumber { get; set; }

        public int ClassEventsNumber { get; set; }
    }
}
