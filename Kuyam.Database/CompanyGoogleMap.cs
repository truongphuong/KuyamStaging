using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database
{
    public class CompanyGoogleMap
    {        
        public int IndexId { get; set; }
        public int ProfileID { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string IconMarker { get; set; }
        public string Slug { get; set; }
    }
}
