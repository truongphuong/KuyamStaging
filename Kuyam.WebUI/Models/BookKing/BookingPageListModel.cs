using Kuyam.Database;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models.BookKing
{
    public class BookingPageListModel
    {
        public BookingPageListModel()
        {
            locations = new List<CompanyGoogleMap>();
            Categories = new List<Service>();
        }
        public bool IsLogin { get; set; }
        public int CategoryId { get; set; }
        public List<Service> Categories { get; set; }
        public string HtmlCategories { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string SearchKey { get; set; }
        public string Page { get; set; }
        public List<CompanyGoogleMap> locations { get; set; }
        public string DetectLocation { get; set; }
        public string MarkerData { get; set; }
        public int TotalPages { get; set; }
        public IPagedList<CompanyProfileSearch> PagedList { get; set; }
    }
}