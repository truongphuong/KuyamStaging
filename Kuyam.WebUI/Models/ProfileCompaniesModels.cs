using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;
using M2.Util;
using M2.Util.MVC;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Kuyam.Domain;
using Kuyam.Database.Extensions;
using Kuyam.WebUI.Extension;

namespace Kuyam.WebUI.Models
{
    public class ProfileCompaniesModels
    {

        public ProfileCompaniesModels()
        {
            ProfileId = 0;
            this.ProfileCompany = new ProfileCompany();
            this.MediaCompanies = new List<Medium>();
            this.ProfileCompanyDetails = new ProfileCompanyDetails();
        }
        public int ProfileId { get; set; }
        public string CompanyName { get; set; }
        public string Stress { get; set; }
        public string CompanyUrl { get; set; }
        public string CompanyPhone { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ImageUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<CompanyHour> ListHours { get; set; }
        public List<Kuyam.Database.Service> ListServices { get; set; }
        public List<CompanyEmployee> ListEmployees { get; set; }
        public bool Favorite { get; set; }
        public int CategoryId { get; set; }
        public List<ServiceCompany> ListServiceCompany { get; set; }
        public List<Medium> MediaCompanies { get; set; }
        public MetaTagExtension MetaTagExtension { get; set; }
        public ProfileCompany ProfileCompany { get; set; }
        public ProfileCompanyDetails ProfileCompanyDetails { get; set; }
        public string CompanyJsionData { get; set; }
        public CompanyService GetServices(int serviceID)
        {
            CompanyService companyService = ProfileCompany.GetServiceByServiceID(serviceID);
            return companyService;
        }
        public List<CompanyEmployee> GetEmployeesList(int serviceID)
        {
            List<CompanyEmployee> employeesService = ProfileCompany.GetEmployeesList(serviceID);

            return employeesService;
        }

        public bool IsBookDirect
        {
            get
            {
                return HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToLower() == "book";
            }
        }

        public bool AllowScroll
        {
            get
            {
                return (HttpContext.Current.Request.QueryString["categoryId"] != null || HttpContext.Current.Request.QueryString["serviceId"] != null
                      || (HttpContext.Current.Request.RawUrl.Contains("/review") && HttpContext.Current.Request.RawUrl.Contains("/book")));
            }
        }

        public string ServiceString { get; set; }

        public string CalendarString { get; set; }
    }

    public class cFavorite : IModel
    {
        public void LockAndLoad()
        {
        }
        public cFavorite() { }
        public int ProfileID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }

}