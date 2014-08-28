using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Kuyam.WebUI.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            Categories = new List<Service>();
            ProfileCompanies = new List<CompanyProfileSearch>();
        }
        public int CategoryId { get; set; }
        public List<Service> Categories { get; set; }
        public string HtmlCategories { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string SearchKey { get; set; }
        public List<CompanyProfileSearch> ProfileCompanies { get; set; }
    }
}