using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class CompanyProfile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Phone { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Url { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<CompanyHour> BusinessHours { get; set; }
        public List<Service> Services { get; set; }
        public List<CompanyEmployee> Employees { get; set; }

        public string ImageUrl { get; set; }
    }
}
