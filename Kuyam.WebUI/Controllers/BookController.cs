using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.Seo;
using Kuyam.WebUI.Extension;
using Kuyam.WebUI.Models;
using MvcSiteMapProvider.Reflection;
using System.Text;

namespace Kuyam.WebUI.Controllers
{
    public class BookController : KuyamBaseController
    {
        public BookController(ISeoFriendlyUrlService seoFriendlyUrlService,
            CompanyProfileService companyProfileService,
            IAppointmentService appointmentService,
            OrderService orderService)
        {
            this.seoFriendlyUrlService = seoFriendlyUrlService;
            _companyProfileService = companyProfileService;
            _appointmentService = appointmentService;
            _orderService = orderService;
        }

        private ISeoFriendlyUrlService seoFriendlyUrlService;
        private readonly IAppointmentService _appointmentService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly OrderService _orderService;

        public ActionResult Index(string companyUrlName)
        {
            return RedirectToAction("Availability", new { companyUrlName });
        }

        public ActionResult Availability(string companyUrlName, int? proposedId, int? categotyId, int? serviceId)
        {
            MySession.IsBookDirect = true;
            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);

            //if (profileCompany.CompanyTypeID == (int)Types.CompanyType.GeneralAvailability
            //    || profileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt
            //    || profileCompany.CompanyTypeID == (int)Types.CompanyType.NonKuyamBookIt
            //    || profileCompany.CompanyTypeID == (int)Types.CompanyType.Unknown)
            //    return InvokeHttp404();


            var model = new ProfileCompaniesModels();

            if (proposedId.HasValue)
            {
                var apptp = _appointmentService.GetProposedAppointmentById(proposedId.Value);
                if (apptp == null || apptp.CustID != MySession.CustID)
                    return new RedirectResult("~/Home?message=true");
                if (apptp.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.booked)
                    return new RedirectResult("~/Home?message=booked");
            }
            ViewBag.EmployeeList = _companyProfileService.GetEmployeeListByProfileCompanyId(id);
            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow).ToString("yyyy/MM/dd hh:mm:ss");
            ViewBag.ListCal = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            model.ProfileId = id;
            model.ProfileCompany = profileCompany;
            var categorys = _companyProfileService.GetCategoryByProfileID(id);
            model.ListServiceCompany = categorys;
            var category = categorys.FirstOrDefault();
            model.CategoryId = categotyId ?? 0;
            ViewBag.CategoryId = categotyId;
            model.Favorite = _companyProfileService.CheckFavoriteByProfileID(id, MySession.CustID);
            model.CompanyName = profileCompany.Name;
            var Keywords = MyApp.Settings.TagSetting.Keywords;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            var serviceCompanys = _companyProfileService.GetServiceCompanyByProfileID(id);
            StringBuilder htmlSevice = new StringBuilder();
            htmlSevice.Append("<option value='0' selected='selected' >all services (or select one)</option>");
            foreach (var item in serviceCompanys)
            {
                string selected = string.Empty;
                if (item.ServiceCompanyID == serviceId)
                {
                    selected = "selected";
                }

                htmlSevice.AppendFormat("<option  title=\"0\" value=\"{1}\" {2} >{3}</option>", item.Service.ServiceName, item.ServiceCompanyID, selected, string.Format("{0}, {1} min, ${2}, {3} person", UtilityHelper.TruncateAtWord(item.Service.ServiceName, 30), item.Duration, item.Price, item.AttendeesNumber));
            }
            model.ServiceString = htmlSevice.ToString();
            return View(model);
        }

        public ActionResult Description(string companyUrlName)
        {
            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();

            var model = new ProfileCompaniesModels();
            model.ProfileId = id;
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            ViewBag.Company = profileCompany;
            model.ProfileCompany = profileCompany;
            model.CompanyName = profileCompany.Name;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);

            return View(model);
        }

        public ActionResult Photo(string companyUrlName)
        {
            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();

            var model = new ProfileCompaniesModels();

            model.ProfileId = id;
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.ProfileCompany = profileCompany;
            model.CompanyName = profileCompany.Name;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            model.MediaCompanies = _companyProfileService.GetCompanyMediums(id, Types.CompanyMediaType.IsBanner);

            return View(model);
        }

        public ActionResult Review(string companyUrlName)
        {
            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();

            var model = new ProfileCompaniesModels();

            model.ProfileId = id;
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.ProfileCompany = profileCompany;
            model.CompanyName = profileCompany.Name;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            int totalRecord = 0;
            ViewBag.RatingList = _appointmentService.GetRatingListByProfileID(id, 1, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = 1;

            return View(model);
        }

        public ActionResult Package(string companyUrlName)
        {
            MySession.IsBookDirect = true;
            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();

            var model = new ProfileCompaniesModels();
            ViewBag.CompanyPackage = string.Empty;
            model.ProfileId = id;
            var packages = _companyProfileService.GetCompanyPackages(id);
            ViewBag.CompanyPackage = CompanyProfileController.GetCompanyPackages(packages);
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.ProfileCompany = profileCompany;
            model.CompanyName = profileCompany.Name;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            return View(model);
        }

        private int GetCompanyIdViaUrl(string urlName)
        {
            var urlCompany = seoFriendlyUrlService.GetBySlug(urlName);
            if (urlCompany == null || urlCompany.EntityName != "company" || urlCompany.IsActive == false)
                return 0;
            return urlCompany.EntityId;
        }

    }
}
