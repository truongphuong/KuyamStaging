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
        }
        public List<Service> Categories { get; set; }
    }
}