using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace Kuyam.WebUI.Models.LandingPage
{
    public class LandingPageList
    {
        public IPagedList<Kuyam.Database.LandingPage> PagedList { get; set; }
        public string SearchKey { get; set; }
        public int Status { get; set; }
        public SelectList StatusList { get; set; }

        public string LinkReturn
        {
            get { return string.Format("/AdminLandingPage/Index?page={0}&key={1}&status={2}", PagedList.PageNumber, SearchKey, Status); }
        }
    }
}