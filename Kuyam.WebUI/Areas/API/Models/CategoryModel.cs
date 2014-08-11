using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class CategoryModel
    {
        public CategoryModel()
        {
            CompanyProfiles = new List<CompanyProfilesModel>();
        }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<CompanyProfilesModel> CompanyProfiles { get; set; }
    }
}