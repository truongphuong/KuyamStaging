﻿using System;
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
using Kuyam.Domain.Seo;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.ClassModel;
using Kuyam.Domain.CategoryServices;
using Kuyam.WebUI.Models.Home;
using Kuyam.WebUI.Models.BookKing;
using PagedList;
using Kuyam.Domain.SearchServices;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Kuyam.Utility;

namespace Kuyam.WebUI.Controllers
{
    public class BookController : KuyamBaseController
    {
        public BookController(ISeoFriendlyUrlService seoFriendlyUrlService,
            CompanyProfileService companyProfileService,
            IAppointmentService appointmentService,
            OrderService orderService,
            ICategoryService categoryService,
            ISearchService searchService
            )
        {
            this.seoFriendlyUrlService = seoFriendlyUrlService;
            this._companyProfileService = companyProfileService;
            this._appointmentService = appointmentService;
            this._orderService = orderService;
            this._categoryService = categoryService;
            this._searchService = searchService;
        }

        private readonly ISeoFriendlyUrlService seoFriendlyUrlService;
        private readonly IAppointmentService _appointmentService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly OrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly ISearchService _searchService;
        public ActionResult Index(string key = "", int categoryId = 0, int page = 1, double lat = 0, double lon = 0)
        {
            var model = new BookingPageListModel();
            model.SearchKey = key;
            ViewBag.KeyWord = key;
            model.CategoryId = categoryId;
            if (MySession.CustID > 0)
                model.IsLogin = true;

            if (lat > 0 || lon > 0)
            {
                MySession.Latitude = lat;
                MySession.Longitude = lon;
            }

            model.Lat = MySession.Latitude;
            model.Lon = MySession.Longitude;

            var categories = _categoryService.GetActiveCategories();


            if (categories != null && categories.Count > 0)
            {
                if (page < 1)
                    page = 1;
                int totalRecord = 0;
                model.Page = page.ToString();
                if (MySession.DetectedLocationExpired)
                {
                    model.DetectLocation = "detectLocation()";
                }
                List<string> categoriesId = new List<string>();

                var companyList = _searchService.CompanySearchForWeb(out totalRecord, categoriesId, key, categoryId, MySession.Latitude, MySession.Longitude, ConfigManager.DefaultDistance, MySession.CustID, page, 10);
                model.PagedList = new StaticPagedList<CompanyProfileSearch>(companyList, page, 10, totalRecord);


                int totalPages = (int)Math.Ceiling((double)totalRecord / 10);

                model.TotalPages = totalPages;

                categoriesId = categoriesId.Distinct().OrderBy(o => o).ToList();

                StringBuilder htmlCategories = new StringBuilder();
                htmlCategories.Append("<option value='0' selected='selected' >select a category</option>");
                categories = categories.Where(m => categoriesId.Contains(m.ServiceID.ToString())).Distinct().ToList();
                model.Categories = categories;
                if (categoryId == 0 && categories.Count() > 0)
                {
                    model.CategoryId = categories[0].ServiceID;
                }

                foreach (var item in categories)
                {

                    string selected = string.Empty;
                    if (model.CategoryId == item.ServiceID)
                        selected = "selected";
                    htmlCategories.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", item.ServiceID, selected, UtilityHelper.TruncateText(item.ServiceName, 36));

                }
                model.HtmlCategories = htmlCategories.ToString();

                model.locations = companyList.Select(item => new CompanyGoogleMap
                {
                    IndexId = item.IndexId,
                    Name = item.Name,
                    ProfileID = item.ProfileID,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    IconMarker = string.Format("/content/images/num-{0}.png", item.IndexId),
                    Slug = Url.RouteUrl("Slug", new { sename = item.GetSeName(item.ProfileID) })
                }).ToList();

            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { content = this.RenderPartialViewToString("_companyBox", (object)model), locations = model.locations }, JsonRequestBehavior.AllowGet);
            }
            model.MarkerData = JsonConvert.SerializeObject(model.locations);
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadMore(string key = "", int categoryId = 0, int page = 1, double lat = 0, double lon = 0)
        {
            var model = new BookingPageListModel();
            model.SearchKey = key;
            model.CategoryId = categoryId;
            double distance = 80.467;

            if (MySession.CustID > 0)
                model.IsLogin = true;

            if (lat > 0 || lon > 0)
            {
                MySession.Latitude = lat;
                MySession.Longitude = lon;
            }

            model.Lat = MySession.Latitude;
            model.Lon = MySession.Longitude;

            model.CategoryId = categoryId;


            if (page < 1)
                page = 1;

            int totalRecord = 0;
            model.Page = page.ToString();
            List<string> categoriesId = new List<string>();
            var companyList = _searchService.CompanySearchForWeb(out totalRecord, categoriesId, key, model.CategoryId, MySession.Latitude, MySession.Longitude, ConfigManager.DefaultDistance, MySession.CustID, page, 10);
            model.PagedList = new StaticPagedList<CompanyProfileSearch>(companyList, page, 10, totalRecord);

            return Json(new { content = this.RenderPartialViewToString("_LoadMoreBox", (object)model) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id = 0, int proposedId = 0, int categoryId = 0, int serviceId = 0)
        {
            ViewBag.HeaderActive = 1;
            var model = new ProfileCompaniesModels();
            model.ProfileId = id;
            var profileCompany = _searchService.GetCompanyProfileDetials(id, categoryId);
            model.ProfileCompanyDetails = profileCompany;
            model.ProfileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.CategoryId = categoryId;
            profileCompany.CategoryId = categoryId;
            model.Favorite = _companyProfileService.CheckFavoriteByProfileID(id, MySession.CustID);
            var Keywords = MyApp.Settings.TagSetting.Keywords;
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            profileCompany.locations = new CompanyGoogleMap
                {
                    IndexId = 0,
                    Name = profileCompany.Name,
                    ProfileID = profileCompany.ProfileID,
                    Latitude = profileCompany.Latitude,
                    Longitude = profileCompany.Longitude,
                    IconMarker = "/content/images/num-0.png",
                    Slug = Url.RouteUrl("Slug", new { sename = profileCompany.GetSeName(profileCompany.ProfileID) })
                };
            model.CompanyJsionData = JsonConvert.SerializeObject(profileCompany);
           
            var hours = _companyProfileService.SplitCompanyHours(model.ProfileCompany.CompanyHours.ToList());
            hours = _companyProfileService.SortCompanyHours(hours);
            ViewBag.CompanyHoursSort = hours;
            //Ratings         
            ViewBag.RatingList = _appointmentService.GetRatingByCompanyProfile(id);//.GetRatingsByProfileId(id);           
            //Package           
            var packages = _companyProfileService.GetCompanyPackages(id);
            ViewBag.CompanyPackages = packages;//GetCompanyPackages(packages);
            //Photo
            model.MediaCompanies = _companyProfileService.GetCompanyMediums(id, Types.CompanyMediaType.IsBanner);

            return View(model);
        }


        public ActionResult GetServiceCompanyByCategoryId(int profileId, int categoryId)
        {
            var serviceCompanies = _searchService.GetServiceCompanyByCategoryId(profileId, categoryId);
            return Json(serviceCompanies.Select(o => new ServiceCompany
            {
                ServiceCompanyID = o.ServiceCompanyID,
                ServiceID = o.ServiceID,
                ServiceTypeId = o.ServiceTypeId,
                ServiceName = o.ServiceName,
                ToDateTime = o.ToDateTime,
                FromDateTime = o.FromDateTime,
                Price = o.Price,
                Duration = o.Duration,
                Description = o.Description,
                EmployeeName = o.EmployeeName,
                IsPerDay = o.IsPerDay,
                Modified = o.Modified,
                Created = o.Created,
                ProfileID = o.ProfileID,
                Status = o.Status
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmployeeByServiceCompanyId(int serviceComapanyId)
        {
            var emloyeeslist = _searchService.GetEmployeeByServiceCompanyId(serviceComapanyId);
            return Json(emloyeeslist.Select(o => new CompanyEmployee { EmployeeName = o.EmployeeName, EmployeeID = o.EmployeeID }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Availability(string companyUrlName, int? proposedId, int? categoryId, int? serviceId)
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
            model.CategoryId = categoryId ?? 0;
            ViewBag.CategoryId = categoryId;
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

        public ActionResult Class(string companyUrlName)
        {
            MySession.IsBookDirect = true;

            var id = GetCompanyIdViaUrl(companyUrlName);
            if (id == 0)
                return InvokeHttp404();
            var model = new ProfileCompaniesModels();
            model.ProfileId = id;
            var profileCompany = _companyProfileService.GetProfileCompanyByID(id);
            model.MetaTagExtension = new MetaTagExtension(profileCompany.Desc);
            model.ProfileCompany = profileCompany;
            var categorys = _companyProfileService.GetCategoryByProfileID(id);
            model.ListServiceCompany = categorys;
            var category = categorys.FirstOrDefault();
            model.CompanyName = profileCompany.Name;
            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            int dayOfWeek = (int)dtnow.DayOfWeek;

            //DateTime startTime = dtnow;
            //if (dayOfWeek == (int)System.DayOfWeek.Sunday)
            //{
            //    startTime = dtnow.AddDays(-6);
            //}
            //else
            //{
            //    startTime = dtnow.AddDays(-(dayOfWeek - 1));
            //}

            DateTime endTime = dtnow.AddDays(7);

            var calendars = _companyProfileService.GetSchedulerAvailabilityOfClass(id, 0, 0, dtnow.ToString("MM-dd-yyyy hh:mm"), endTime.ToString("MM-dd-yyyy hh:mm"));
            model.CalendarString = BuildCalendar(calendars, dtnow);

            return View(model);
        }

        private string BuildCalendar(List<SchedulerAvailability> schedulerAvailability, DateTime dtNow)
        {
            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < 7; i++)
            {
                string dayInWeek = dtNow.AddDays(i).ToString("ddd").ToLower();
                string classToday = string.Empty;
                if (i == 0)
                {
                    classToday = "today";
                    dayInWeek = "today";
                }

                strBuilder.AppendFormat("<div class=\"column-day {0}\">", classToday);
                strBuilder.AppendFormat("<div class=\"column-header\"><span class=\"day\">{0}</span><span class=\"datetime\">{1}</span></div>", dayInWeek, dtNow.AddDays(i).ToString("MMM d, yyyy"));


                var classIndays = schedulerAvailability.Where(q => q.DayOfWeek == (int)dtNow.AddDays(i).DayOfWeek).OrderBy(o => o.FromHour).ToList();

                foreach (var item in classIndays)
                {
                    DateTime start = dtNow.AddDays(i).Date.AddTicks(item.FromHour.Ticks);
                    string isFull = "full";

                    if (item.IsAvailability && start < dtNow)
                    {
                        isFull = "expired";
                    }

                    string btnReserve = string.Empty;
                    if (start >= dtNow.AddMinutes(10) && item.IsAvailability)
                    {
                        isFull = string.Empty;
                        btnReserve = string.Format("<a id=\"reserve\" classSchedulerId =\"{0}\" startTime=\"{1}\" emdTime=\"{2}\" className= \"{3}\" instructorName=\"{4}\" class=\"reserve\"  href=\"javascript:void(0);\">reserve</a>",
                            item.ClassSchedulerID, start.ToString("MM/dd/yyyy hh:mm tt"), start.AddMinutes(item.Duration).ToString("MM/dd/yyyy hh:mm tt"), item.ServiceName, item.EmployeeName);
                    }

                    strBuilder.Append("<div class=\"column-sessions\">");
                    strBuilder.AppendFormat(" <div class=\"session {0}\">", isFull);
                    strBuilder.AppendFormat("<span class=\"time\">{0} – {1}</span>", new DateTime(item.FromHour.Ticks).ToString("h:mm tt").ToLower(), new DateTime(item.FromHour.Ticks).AddMinutes(item.Duration).ToString("h:mm tt").ToLower());
                    strBuilder.AppendFormat("<span class=\"instructor\">{0}</span>", item.EmployeeName);
                    strBuilder.AppendFormat("<span class=\"type\">{0}</span>", item.ServiceName);
                    strBuilder.Append(btnReserve);
                    //strBuilder.AppendFormat("<a id=\"reserve\" classSchedulerId =\"{0}\" startTime=\"{1}\" emdTime=\"{2}\" className= \"{3}\" instructorName=\"{4}\" class=\"reserve\"  href=\"javascript:void(0);\">reserve</a>",
                    //item.ClassSchedulerID, start.ToString("MM/dd/yyyy hh:mm tt"), start.AddMinutes(item.Duration).ToString("MM/dd/yyyy hh:mm tt"), item.ServiceName, item.EmployeeName);

                    strBuilder.Append("</div></div>");

                }
                strBuilder.Append("</div>");
            }

            return strBuilder.ToString();
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
