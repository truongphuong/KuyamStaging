using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database.Extensions
{
    public class PostExt
    {
        public int PostRowID { get; set; }
        public Guid PostID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Slug { get; set; }
        public string CoverPhoto { get; set; }
        public string Caption { get; set; }
        public bool? AlowChatToBook { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool? IsFeatured { get; set; }
        public bool IsLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }       
        public double Radius { get; set; }
        public DbGeography GeoLocation { get; set; }

        public bool? IsEvent { get; set; }    

    }
}
