using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Database.Extensions;
using Kuyam.WebUI.Models;
using System.Web.Security;
using Kuyam.Database;
using Kuyam.Domain;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using Kuyam.Utility;
using System.IO;
using Kuyam.WebUI.InfoConnServiceReference;
using System.Collections.Specialized;
using Kuyam.WebUI.Helpers;
using Kuyam.WebUI.Models.CompanyAppointment;
using RazorEngine;
using System.Dynamic;
using Kuyam.Domain.KuyamServices;
using Kuyam.WebUI.Extension;
using Kuyam.Domain.Seo;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.OfferServices;
using Kuyam.WebUI.Models.Offers;


namespace Kuyam.WebUI.Controllers
{

    public class CompanyController : KuyamBaseController
    {
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly EmailSender _emailSender;
        private readonly CustService _custService;
        private readonly IAppointmentService _appointmentService;
        private readonly IProfileCompanyService _profileCompanyService;
        private readonly ISeoFriendlyUrlService _seoFriendlyUrlService;
        private readonly ClassService _classService;
        private readonly CompanySearchService _companySearchService;
        private readonly IOfferService _offerService;
        public CompanyController(IFormsAuthenticationService formsService,
            IMembershipService membershipService,
            CompanyProfileService companyProfileService,
            EmailSender emailSender,
            CustService custService,
            IAppointmentService appointmentService,
            IProfileCompanyService profileCompanyService,
            ISeoFriendlyUrlService seoFriendlyUrlService,
            ClassService classService,
            CompanySearchService companySearchService,
            IOfferService offerService)
        {
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._companyProfileService = companyProfileService;
            this._emailSender = emailSender;
            this._custService = custService;
            this._appointmentService = appointmentService;
            this._profileCompanyService = profileCompanyService;
            this._seoFriendlyUrlService = seoFriendlyUrlService;
            this._classService = classService;
            this._companySearchService = companySearchService;
            this._offerService = offerService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("CompanySearch");
        }


        public ActionResult CompanySearch(string id)
        {
            //string id = Request.Params["id"];
            string compare = Request.Params["compare"];
            string price = Request.Params["price"];
            string time = Request.Params["time"];
            string isAvailable = Request.Params["isavailable"];
            string sortBy = Request.Params["sortBy"];
            string key = Request.Params["key"];
            string page = Request.Params["page"];
            if (id == null) { id = "0"; }
            if (compare == null) { compare = "0"; }
            if (price == null) { price = "0"; }
            if (time == null) { time = "0"; }
            if (isAvailable == null) { isAvailable = "false"; }
            if (sortBy == null) { sortBy = "0"; }
            if (key == null) { key = "find a company"; }

            if (page == null) { page = "1"; }

            int serviceId = 0;
            int.TryParse(id, out serviceId);

            int typeID = 0;
            if (!string.IsNullOrEmpty(id))
                int.TryParse(id, out typeID);
            double distance = double.Parse(compare);
            int totalRecord = 0;
            int pageIndex = int.Parse(page);
            decimal priceFrom = 0;
            decimal priceTo = 0;
            DateTime hourFrom = DateTime.Today;
            DateTime hourTo = DateTime.Today;
            if (int.Parse(price) == 1)
            {
                priceFrom = 0;
                priceTo = 50;
            }
            if (int.Parse(price) == 2)
            {
                priceFrom = 50;
                priceTo = 100;
            }
            if (int.Parse(price) == 3)
            {
                priceFrom = 100;
                priceTo = 250;
            }
            if (int.Parse(price) == 4)
            {
                priceFrom = 250;
                priceTo = decimal.MaxValue;
            }

            if (int.Parse(time) == 1)
            {
                hourFrom = hourFrom.AddHours(6);
                hourTo = hourTo.AddHours(12);

            }
            else if (int.Parse(time) == 2)
            {
                hourFrom = hourFrom.AddHours(12);
                hourTo = hourTo.AddHours(18);

            }
            else if (int.Parse(time) == 3)
            {
                hourFrom = hourFrom.AddHours(18);
                hourTo = hourTo.AddHours(24);
            }

            if (key == "find a company")
            {
                key = string.Empty;
            }

            var custID = Kuyam.WebUI.Models.MySession.CustID;
            var cust = _custService.GetCustomerCustID(custID);
            List<CompanyProfileSearch> resultList = _companySearchService.GetCompaniesFromTypeIDWithDistance(cust, typeID, distance,
                priceFrom, priceTo, hourFrom, hourTo, Boolean.Parse(isAvailable), pageIndex, int.Parse(sortBy),
                out totalRecord, key);

            ViewBag.ProfileCompanies = resultList;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            int categoryId = 0;
            int.TryParse(id, out categoryId);
            Service service = _companyProfileService.GetServiceByID(categoryId);
            ViewBag.Category = service != null ? service.ServiceName : "all";
            ViewBag.CategoryId = categoryId;

            if (cust != null)
            {
                ViewBag.UserLat = cust.Latitude;
                ViewBag.UserLon = cust.Longitude;
                ViewBag.SearchArea = string.Empty;
                if (!string.IsNullOrEmpty(cust.City) && !string.IsNullOrEmpty(cust.State))
                    ViewBag.SearchArea = string.Format("{0}, {1}", cust.City, cust.State);
                else if (!string.IsNullOrEmpty(cust.City))
                    ViewBag.SearchArea = cust.City;
                else if (!string.IsNullOrEmpty(cust.State))
                    ViewBag.SearchArea = cust.State;
            }

            if (!string.IsNullOrEmpty(key))
            {
                ViewBag.SearchBy = UtilityHelper.TruncateText(key, 133);
            }
            else
            {
                ViewBag.SearchBy = UtilityHelper.TruncateText(ViewBag.Category, 133);
            }

            var description = MyApp.Settings.TagSetting.SearchDescription;
            ViewBag.MetaTagExtension = new MetaTagExtension(description);
            return View("CompanyProfileSearch");
        }

        [HttpPost]
        public ActionResult CompanySearchWithDistance(string Id, string compare, string price, string time, bool isAvailableToday, string sortBy, string key)
        {
            int serviceId = 0;
            if (!string.IsNullOrEmpty(Id))
                serviceId = int.Parse(Id);
            double distance = double.Parse(compare);
            decimal priceFrom = 0;
            decimal priceTo = 0;
            DateTime hourFrom = DateTime.Today;
            DateTime hourTo = DateTime.Today;
            if (int.Parse(price) == 1)
            {
                priceFrom = 0;
                priceTo = 50;
            }
            if (int.Parse(price) == 2)
            {
                priceFrom = 50;
                priceTo = 100;
            }
            if (int.Parse(price) == 3)
            {
                priceFrom = 100;
                priceTo = 250;
            }
            if (int.Parse(price) == 4)
            {
                priceFrom = 250;
                priceTo = decimal.MaxValue;
            }

            if (int.Parse(time) == 1)
            {
                hourFrom = hourFrom.AddHours(6);
                hourTo = hourTo.AddHours(12);

            }
            else if (int.Parse(time) == 2)
            {
                hourFrom = hourFrom.AddHours(12);
                hourTo = hourTo.AddHours(18);

            }
            else if (int.Parse(time) == 3)
            {
                hourFrom = hourFrom.AddHours(18);
                hourTo = hourTo.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            if (key == "find a company")
            {
                key = string.Empty;
            }

            var custID = Kuyam.WebUI.Models.MySession.CustID;
            var cust = _custService.GetCustomerCustID(custID);
            int totalRecord = 0;
            List<CompanyProfileSearch> resultList = _companySearchService.GetCompaniesFromTypeIDWithDistance(cust, serviceId,
                distance, priceFrom, priceTo, hourFrom, hourTo, isAvailableToday, 1, int.Parse(sortBy), out totalRecord,
                key);
            ViewBag.ProfileCompanies = resultList;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = 1;
            ViewBag.Key = key;
            ViewBag.CategoryId = serviceId;

            return PartialView("_CompanyProfiles");
        }

        [HttpPost]
        public ActionResult CompanySearchWithDistanceAndPaging(string id, string compare, string price, string time, bool isAvailableToday, string page, string sortBy, string key)
        {
            int typeID = 0;
            if (!string.IsNullOrEmpty(id))
                typeID = int.Parse(id);
            double distance = double.Parse(compare);
            int totalRecord = 0;
            int pageIndex = int.Parse(page);
            decimal priceFrom = 0;
            decimal priceTo = 0;
            DateTime hourFrom = DateTime.Today;
            DateTime hourTo = DateTime.Today;
            if (int.Parse(price) == 1)
            {
                priceFrom = 1;
                priceTo = 50;
            }
            if (int.Parse(price) == 2)
            {
                priceFrom = 50;
                priceTo = 100;
            }
            if (int.Parse(price) == 3)
            {
                priceFrom = 100;
                priceTo = 250;
            }
            if (int.Parse(price) == 4)
            {
                priceFrom = 250;
                priceTo = decimal.MaxValue;
            }

            if (int.Parse(time) == 1)
            {
                hourFrom = hourFrom.AddHours(6);
                hourTo = hourTo.AddHours(12);

            }
            else if (int.Parse(time) == 2)
            {
                hourFrom = hourFrom.AddHours(12);
                hourTo = hourTo.AddHours(18);

            }
            else if (int.Parse(time) == 3)
            {
                hourFrom = hourFrom.AddHours(18);
                hourTo = hourTo.AddHours(24);
            }

            if (key == "find a company")
            {
                key = string.Empty;
            }

            var custID = Kuyam.WebUI.Models.MySession.CustID;
            var cust = _custService.GetCustomerCustID(custID);
            List<CompanyProfileSearch> resultList = _companySearchService.GetCompaniesFromTypeIDWithDistance(cust, typeID, distance,
                priceFrom, priceTo, hourFrom, hourTo, isAvailableToday, pageIndex, int.Parse(sortBy), out totalRecord, key);
            ViewBag.ProfileCompanies = resultList;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;


            return PartialView("_CompanyProfiles");
        }

        [HttpGet]
        public ActionResult LoadDetailPopup(int? profileId)
        {
            ProfileCompany company = _companyProfileService.GetProfileCompanyByID(profileId ?? 0);
            ViewBag.company = company;
            return PartialView("_SearchPopup");
        }
             

        public ActionResult LoadOfferPopup(int? companyEventId)
        {

            var companyEvent = _offerService.GetCompanyEventByCompanyEventId(companyEventId ?? 0);
            var listOffers = _offerService.GetListServicesEventByCompanyEventId(companyEventId ?? 0, 0);
            var ListClasses = listOffers.Where(m => m.ServiceTypeId == (int)Types.ServiceType.ClassType).OrderBy(o => o.NewPrice).Take(3).ToList();
            var ListServices = listOffers.Where(m => m.ServiceTypeId == (int)Types.ServiceType.ServiceType).OrderBy(o => o.NewPrice).Take(3).ToList();
            bool hasClass = (ListClasses != null && ListClasses.Count() > 0);

            List<CompanyServiceEventDTO> offers = new List<CompanyServiceEventDTO>();

            if (ListClasses != null && ListClasses.Count() > 0 && companyEvent.ProfileCompany.HasClassBooking)
            {
                offers = ListClasses.Select(item => new CompanyServiceEventDTO
                 {
                     ID = item.ID,
                     ServiceTypeId = item.ServiceTypeId,
                     Description = item.Description,
                     CompanyEventID = item.CompanyEventID,
                     ServiceCompanyID = item.ServiceCompanyID,
                     OldPrice = item.OldPrice,
                     NewPrice = item.NewPrice,
                     ServiceName = item.ServiceName,
                     CategoryName = item.CategoryName,
                     IsClass =true

                 }).ToList();
            }
            int numberOfferClass = offers != null ? offers.Count() : 0;
            var serviceOffers = ListServices.Select(item => new CompanyServiceEventDTO
                 {
                     ID = item.ID,
                     ServiceTypeId = item.ServiceTypeId,
                     Description = item.Description,
                     CompanyEventID = item.CompanyEventID,
                     ServiceCompanyID = item.ServiceCompanyID,
                     OldPrice = item.OldPrice,
                     NewPrice = item.NewPrice,
                     ServiceName = item.ServiceName,
                     CategoryName = item.CategoryName,
                     IsClass =false

                 }).Take(3 - numberOfferClass).ToList();

            offers.AddRange(serviceOffers);

            var model = new OfferModel
            {
                SlugName = Url.RouteUrl("Slug", new { sename = companyEvent.GetSeName(companyEvent.ProfileCompany.ProfileID) }),
                Event = companyEvent.Event,
                Offers = offers

            };
            return PartialView("_OfferPopup", model);
        }


        [HttpGet]
        public ActionResult LoadDefaultLeftTab()
        {
            return PartialView("_CompanySearchLeftTab");
        }

        [HttpPost]
        public ActionResult LoadScheduleTab(string id)
        {
            int companyID = int.Parse(id);
            ProfileCompany company = DAL.GetProfileCompany(companyID);
            ViewBag.Company = company;
            ViewBag.IsSchedule = true;

            return PartialView("_CompanySearchLeftTab");
        }

        [HttpPost]
        public ActionResult RequireSendEmailListCompanyHours(string companyName)
        {
            Cust user = MySession.Cust;
            string emailTo = ConfigurationManager.AppSettings["StaffEmail"];

            // send email
            EmailHelper.SendEmailCompanyListHourRequest(emailTo, user.FirstName, user.LastName, user.Email, companyName);

            string mess = "Thanks!, We will contact you soon";
            return Json(mess, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompanySetup()
        {
            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles != null)
            {
                return RedirectToAction("SetupBasicEdit");
            }
            CompanySetupModel model = new CompanySetupModel();
            List<Service> lstService = _companyProfileService.GetListService();
            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            ViewBag.PaypalAccountlink = ConfigManager.PaypalSigupAccount;
            return View(model);
        }


        [HttpPost]
        public ActionResult CompanySetup(CompanySetupModel model, string hdcategory, string hdhour)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(hdcategory))
                {
                    var Categories = _companyProfileService.GetCategories();
                    model.Listcategory = hdcategory.Split(',');
                    foreach (string item in model.Listcategory)
                    {
                        int id = 0;
                        int.TryParse(item, out id);
                        var categoryitem = Categories.Where(m => m.ServiceID == id).FirstOrDefault();
                        Category category = new Category();
                        category.CategoryID = item;
                        category.NamCategory = categoryitem != null ? categoryitem.ServiceName : string.Empty;
                        model.Categories.Add(category);
                    }
                }

                if (!string.IsNullOrEmpty(hdhour))
                {
                    model.Listhour = hdhour.Split('|');
                }

                List<CompanyHour> companyHours = new List<CompanyHour>();
                if (model.Listhour != null)
                {
                    foreach (string item in model.Listhour)
                    {
                        string strday = item.Split(',')[0].Trim();
                        int day = 0;
                        TimeSpan start;
                        TimeSpan end;
                        TimeSpan.TryParseExact(item.Split(',')[1].Trim(), @"hh\:mm", null, out start);
                        TimeSpan.TryParseExact(item.Split(',')[2].Trim(), @"hh\:mm", null, out end);
                        switch (strday)
                        {
                            case "mon": { day = (int)Types.Day.Monday; break; }
                            case "tue": { day = (int)Types.Day.Tuesday; break; }
                            case "wed": { day = (int)Types.Day.Wednesday; break; }
                            case "thu": { day = (int)Types.Day.Thursday; break; }
                            case "fri": { day = (int)Types.Day.Friday; break; }
                            case "sat": { day = (int)Types.Day.Saturday; break; }
                            case "sun": { day = (int)Types.Day.Sunday; break; }
                            case "isdaily": { day = (int)Types.Day.Isdaily; break; }
                        }
                        Companylist companyHour = new Companylist()
                        {
                        };
                        string tmp = GetDayString(day);
                        if (day == (int)Types.Day.Isdaily)
                        {
                            tmp = "isdaily";
                        }
                        string id = string.Format("{0},{1},{2}", tmp, start.ToString("hh':'mm"), end.ToString("hh':'mm"));
                        string dayofweek = string.Format("{0} {1}-{2}", GetDayString(day), start.ToString("hh':'mm"), end.ToString("hh':'mm"));
                        Companylist company = new Companylist()
                        {
                            CompanyID = id,
                            CompanyNam = dayofweek
                        };
                        model.Companylist.Add(company);

                        companyHours.Add(new CompanyHour()
                        {
                            DayOfWeek = day,
                            FromHour = start,
                            ToHour = end,
                            IsDaily = day == (int)Types.Day.Isdaily
                        });
                    }
                }

                string[] stateCity = model.City.Split(',');
                string strAddress = string.Empty;
                if (stateCity[0] != "")
                    strAddress = strAddress + stateCity[0];
                if (stateCity[1] != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + ", " + stateCity[1];
                    else
                        strAddress = model.State;
                if (model.Zip != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + " " + model.Zip;
                    else
                        strAddress = model.Zip;

                GeoClass.Coordinate coordinate;
                if (!string.IsNullOrEmpty(strAddress))
                    coordinate = GeoClass.GetCoordinates(strAddress);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);


                ProfileCompany profileCompany = new ProfileCompany
                {
                    CompanyTypeID = (int)Types.CompanyType.KuyamInstantBook,
                    CompanyStatusID = (int)Types.CompanyStatus.Pending,
                    Name = model.Name,
                    Street1 = model.Street1,
                    Street2 = model.Street2,
                    City = stateCity[0],
                    State = UtilityHelper.TruncateData(stateCity[1], 4),
                    Zip = model.Zip,
                    Email = model.Email,
                    Url = model.Url,
                    PaymentOptions = model.PaymentOptions,
                    YoutubeLink = model.Youtubelink,
                    Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone),
                    ContactName = model.ContactName,
                    ApptAutoConfirm = true,
                    ApptDefaultSlotDuration = 0,
                    ApptDefaultPeoplePerSlot = 0,
                    Latitude = (double)coordinate.Latitude,
                    Longitude = (double)coordinate.Longitude,
                    ExpiredDate = DateTime.UtcNow,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow

                };
                int PreferredContact = 0;
                if (model.EmailType)
                {
                    PreferredContact |= (int)Types.PreferredPhone.Email;
                }
                if (model.TextType)
                {
                    PreferredContact |= (int)Types.PreferredPhone.Text;
                }
                profileCompany.PreferredContact = PreferredContact;
                Profile profile = new Profile
                {
                    CustID = MySession.CustID,
                    PrivacyTypeID = (int)Types.PrivacyType.Private,

                    RelationshipTypeID = (int)Types.RelationshipType.Company,
                    Name = model.Name,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    ProfileCompany = profileCompany
                };

                Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
                if (profiles != null)
                {
                    profiles.CustID = profile.CustID;
                    profiles.PrivacyTypeID = (int)Types.PrivacyType.Private;
                    profiles.RelationshipTypeID = (int)Types.RelationshipType.Company;
                    profiles.ProfileTypeID = 115;
                    profiles.Created = DateTime.Now;
                    profiles.Modified = DateTime.Now;

                    profiles.ProfileCompany.ProfileID = profiles.ProfileID;
                    profiles.ProfileCompany = profileCompany;
                    profiles.ProfileCompany.CompanyTypeID = (int)Types.CompanyType.KuyamInstantBook;
                    profiles.ProfileCompany.CompanyStatusID = (int)Types.CompanyStatus.Pending;
                    profiles.ProfileCompany.Name = model.Name;
                    profiles.ProfileCompany.Street1 = model.Street1;
                    profiles.ProfileCompany.Street2 = model.Street2;
                    profiles.ProfileCompany.City = stateCity[0];
                    profiles.ProfileCompany.State = UtilityHelper.TruncateData(stateCity[1], 4);
                    profiles.ProfileCompany.Zip = model.Zip;
                    profiles.ProfileCompany.PaymentOptions = model.PaymentOptions;
                    profiles.ProfileCompany.Email = model.Email;
                    profiles.ProfileCompany.Url = model.Url;
                    profiles.ProfileCompany.YoutubeLink = model.Youtubelink;
                    profiles.ProfileCompany.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                    profiles.ProfileCompany.ContactName = model.ContactName;
                    profiles.ProfileCompany.ApptAutoConfirm = true;
                    profiles.ProfileCompany.ApptDefaultSlotDuration = 0;
                    profiles.ProfileCompany.ApptDefaultPeoplePerSlot = 0;
                    profiles.ProfileCompany.Latitude = (double)coordinate.Latitude;
                    profiles.ProfileCompany.Longitude = (double)coordinate.Longitude;
                    profiles.ProfileCompany.Created = DateTime.UtcNow;
                    profile.ProfileCompany.ExpiredDate = DateTime.UtcNow;
                    profiles.ProfileCompany.Modified = DateTime.UtcNow;
                    _companyProfileService.UpdateProfile(profiles);
                }
                else
                {
                    _companyProfileService.InsertProfile(profile);
                }

                if (model.Listhour != null && profile.ProfileID != 0)
                {
                    foreach (CompanyHour companyHour in companyHours)
                    {
                        companyHour.ProfileCompanyID = profile.ProfileID;
                    }


                    _companyProfileService.InsertCompanyHour(companyHours);
                }

                if (model.Categories != null && profile.ProfileID != 0)
                {
                    List<ServiceCompany> lstServiceCompany = new List<ServiceCompany>();
                    foreach (Category item in model.Categories)
                    {
                        int serviceId = 0;
                        int.TryParse(item.CategoryID, out serviceId);
                        ServiceCompany serviceCompany = new ServiceCompany
                        {
                            ProfileID = profile.ProfileID,
                            ServiceID = serviceId,
                            Status = 0,
                            Created = DateTime.Now,
                            Modified = DateTime.Now

                        };
                        lstServiceCompany.Add(serviceCompany);
                    }
                    _companyProfileService.InsertServiceCompany(lstServiceCompany);
                }

                List<CompanyMedia> lstcompanyMedia = new List<CompanyMedia>();
                if (model.LogoID != 0)
                {
                    CompanyMedia companyMedialogo = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMedialogo.MediaID = model.LogoID;
                    companyMedialogo.IsLogo = true;
                    lstcompanyMedia.Add(companyMedialogo);
                }
                if (model.PhotoID != 0)
                {
                    CompanyMedia companyMediaphoto = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMediaphoto.MediaID = model.PhotoID;
                    companyMediaphoto.IsBanner = true;
                    lstcompanyMedia.Add(companyMediaphoto);
                }
                if (model.VideoID != 0)
                {
                    CompanyMedia companyMediavideo = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMediavideo.MediaID = model.VideoID;
                    companyMediavideo.IsVideo = true;
                    lstcompanyMedia.Add(companyMediavideo);
                }

                string strflash = KalturaHelper.GetEmbedVideothumbnail(model.VideoData, model.VideoID, 216, 302);
                model.FlashData = MvcHtmlString.Create(strflash);
                _companyProfileService.InsertCompanyMedia(lstcompanyMedia);

                string invite = _companyProfileService.AddInviteCode(MySession.Cust.CustID, model.Email);

                //underreview
                try
                {
                    string template = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/underreview.cshtml")))
                    {
                        template = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = MySession.Cust.FirstName.ToString(),
                        Email = model.Email.ToString(),
                        CompanyName = model.Name,
                        Host = EmailHelper.GetStoreHost()
                    };
                    string templateResult = RazorEngine.Razor.Parse(template, myObject);
                    EmailHelper.SendEmailCompanyUnderReviewVerificationCode(model.Email, templateResult);
                }
                catch
                {
                    throw;
                }

                return RedirectToAction("postsetup");
            }

            List<Service> lstService = _companyProfileService.GetListService();
            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            return View(model);
        }

        [Authorize]
        public ActionResult CompanyEdit()
        {
            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            ViewBag.PartnerId = ConfigManager.KULTURA_PARTNER_ID;
            int profileID = profiles.ProfileID;
            CompanySetupModel model = new CompanySetupModel();
            List<Service> lstService = _companyProfileService.GetListService();
            List<ServiceCompany> serviceCmpanies = _companyProfileService.GetCategoryByProfileID(profileID);
            model.ProfileID = profiles.ProfileID;
            model.Name = profiles.Name;
            model.Street1 = profiles.ProfileCompany.Street1;
            model.Street2 = profiles.ProfileCompany.Street2;
            model.City = string.Format("{0},{1}", profiles.ProfileCompany.City, profiles.ProfileCompany.State);
            model.State = UtilityHelper.TruncateData(profiles.ProfileCompany.State, 4);
            model.Zip = profiles.ProfileCompany.Zip;
            model.Email = profiles.ProfileCompany.Email;
            model.PaymentOptions = profiles.ProfileCompany.PaymentOptions;
            model.Url = profiles.ProfileCompany.Url;
            model.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(profiles.ProfileCompany.Phone);
            model.ContactName = profiles.ProfileCompany.ContactName;
            model.Youtubelink = profiles.ProfileCompany.YoutubeLink;
            foreach (ServiceCompany item in serviceCmpanies)
            {
                Category category = new Category();
                category.CategoryID = item.ServiceID.ToString();
                category.NamCategory = item.Service.ServiceName;
                model.Categories.Add(category);
            }

            foreach (CompanyHour item in profiles.ProfileCompany.CompanyHours)
            {
                string dayOfWeek = GetDayString(item.DayOfWeek);
                if (item.IsDaily.HasValue && item.IsDaily.Value)
                {
                    dayOfWeek = "isdaily";
                }
                string id = string.Format("{0},{1},{2}", dayOfWeek, item.FromHour.ToString("hh':'mm"), item.ToHour.ToString("hh':'mm"));
                string day = string.Format("{0} {1}-{2}", GetDayString(item.DayOfWeek), item.FromHour.ToString("hh':'mm"), item.ToHour.ToString("hh':'mm"));
                Companylist company = new Companylist()
                {
                    CompanyID = id,
                    CompanyNam = day
                };
                model.Companylist.Add(company);
            }

            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            List<CompanyMedia> companyMedia = _companyProfileService.GetCompanyMediaByProfileID(profileID);
            foreach (CompanyMedia item in companyMedia)
            {
                if (item.IsLogo)
                {
                    model.LogoID = item.MediaID;
                    model.LogoData = item.Medium.LocationData;
                }
                else if (item.IsBanner)
                {
                    model.PhotoID = item.MediaID;
                    model.PhotoData = item.Medium.LocationData;
                }
                else if (item.IsVideo)
                {
                    model.VideoID = item.MediaID;
                    model.VideoData = item.Medium.LocationData;
                }
            }
            string strflash = KalturaHelper.GetEmbedVideothumbnail(model.VideoData, model.VideoID, 216, 302);
            model.FlashData = MvcHtmlString.Create(strflash);
            ViewBag.EntryId = model.VideoData;
            model.CompanyMedias = companyMedia;
            ViewBag.PaypalAccountlink = ConfigManager.PaypalSigupAccount;
            return View(model);
        }

        private string GetDayString(int day)
        {
            string str = "mon - sun";
            switch (day)
            {
                case (int)Types.ShortDay.Mon: { str = Types.ShortDay.Mon.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Tue: { str = Types.ShortDay.Tue.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Wed: { str = Types.ShortDay.Wed.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Thu: { str = Types.ShortDay.Thu.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Fri: { str = Types.ShortDay.Fri.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Sat: { str = Types.ShortDay.Sat.ToString().ToLower(); break; }
                case (int)Types.ShortDay.Sun: { str = Types.ShortDay.Sun.ToString().ToLower(); break; }

            }
            return str;
        }

        [Authorize]
        [HttpPost]
        public ActionResult CompanyEdit(CompanySetupModel model, string hdcategory, string hdhour)
        {
            List<Service> lstService = _companyProfileService.GetListService();
            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            ModelState.Remove("Name");
            if (ModelState.IsValid)
            {
                string newEmail = string.Empty;
                if (!string.IsNullOrEmpty(hdcategory))
                {
                    model.Listcategory = hdcategory.Split(',');
                    foreach (string item in model.Listcategory)
                    {
                        int id = 0;
                        int.TryParse(item, out id);
                        Category category = new Category();
                        category.CategoryID = item;
                        category.NamCategory = _companyProfileService.GetServiceByID(id).ServiceName;
                        model.Categories.Add(category);
                    }
                }

                string[] stateCity = model.City.Split(',');
                string strAddress = string.Empty;
                if (stateCity[0] != "")
                    strAddress = strAddress + stateCity[0];
                if (stateCity[1] != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + ", " + stateCity[1];
                    else
                        strAddress = model.State;
                if (model.Zip != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + " " + model.Zip;
                    else
                        strAddress = model.Zip;
                GeoClass.Coordinate coordinate;
                if (!string.IsNullOrEmpty(strAddress))
                    coordinate = GeoClass.GetCoordinates(strAddress);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);

                if (!string.IsNullOrEmpty(hdhour))
                {
                    model.Listhour = hdhour.Split('|');
                }

                ProfileCompany profileCompany = _companyProfileService.GetProfileCompanyByID(model.ProfileID);
                if (!model.Email.Equals(profileCompany.Email))
                {
                    newEmail = model.Email;
                }

                profileCompany.CompanyTypeID = (int)Types.CompanyType.KuyamInstantBook;
                profileCompany.Name = model.Name;
                profileCompany.Street1 = model.Street1;
                profileCompany.Street2 = model.Street2;
                profileCompany.City = stateCity[0];
                profileCompany.State = UtilityHelper.TruncateData(stateCity[1], 4);
                profileCompany.Zip = model.Zip;
                profileCompany.Email = model.Email;
                profileCompany.PaymentOptions = model.PaymentOptions;
                profileCompany.Url = model.Url;
                profileCompany.YoutubeLink = model.Youtubelink;
                //profileCompany.Phone = model.Phone.Replace("-", "");
                profileCompany.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                profileCompany.ContactName = model.ContactName;
                profileCompany.ApptAutoConfirm = true;
                profileCompany.ApptDefaultSlotDuration = 0;
                profileCompany.ApptDefaultPeoplePerSlot = 0;
                profileCompany.Latitude = (double)coordinate.Latitude;
                profileCompany.Longitude = (double)coordinate.Longitude;
                profileCompany.Created = DateTime.Now;
                profileCompany.Modified = DateTime.Now;

                Profile profile = profileCompany.Profile;
                profile.CustID = MySession.CustID;
                profile.PrivacyTypeID = (int)Types.PrivacyType.Private;
                profile.ProfileTypeID = (int)Types.CustType.Company;
                profile.RelationshipTypeID = (int)Types.RelationshipType.Company;
                profile.Name = model.Name;
                profile.Created = DateTime.Now;
                profile.Modified = DateTime.Now;
                profile.ProfileCompany = profileCompany;

                _companyProfileService.UpdateProfile(profile);

                List<CompanyHour> lstProfileHourOld = _companyProfileService.GetCompanyHourProfileID(profile.ProfileID);
                if (model.Listhour != null && profile.ProfileID != 0)
                {
                    List<CompanyHour> lstProfileHourNew = new List<CompanyHour>();
                    foreach (string item in model.Listhour)
                    {
                        string strday = item.Split(',')[0].Trim();
                        int day = 0;
                        TimeSpan start;
                        TimeSpan end;
                        TimeSpan.TryParseExact(item.Split(',')[1].Trim(), @"hh\:mm", null, out start);
                        TimeSpan.TryParseExact(item.Split(',')[2].Trim(), @"hh\:mm", null, out end);
                        switch (strday)
                        {
                            case "mon": { day = (int)Types.Day.Monday; break; }
                            case "tue": { day = (int)Types.Day.Tuesday; break; }
                            case "wed": { day = (int)Types.Day.Wednesday; break; }
                            case "thu": { day = (int)Types.Day.Thursday; break; }
                            case "fri": { day = (int)Types.Day.Friday; break; }
                            case "sat": { day = (int)Types.Day.Saturday; break; }
                            case "sun": { day = (int)Types.Day.Sunday; break; }
                            case "isdaily": { day = (int)Types.Day.Isdaily; break; }
                        }
                        CompanyHour profileHour = new CompanyHour()
                        {
                            ProfileCompanyID = profile.ProfileID,
                            DayOfWeek = day,
                            FromHour = start,
                            ToHour = end

                        };
                        if (day == (int)Types.Day.Isdaily)
                        {
                            profileHour.IsDaily = true;
                        }
                        lstProfileHourNew.Add(profileHour);
                    }

                    _companyProfileService.UpdateCompanyHour(lstProfileHourNew, lstProfileHourOld);
                }
                else
                {
                    _companyProfileService.UpdateCompanyHour(null, lstProfileHourOld);
                }

                List<ServiceCompany> lstServicenOld = _companyProfileService.GetCategoryByProfileID(profile.ProfileID);
                if (model.Categories != null && profile.ProfileID != 0)
                {
                    List<ServiceCompany> lstServicenNew = new List<ServiceCompany>();
                    foreach (Category item in model.Categories)
                    {
                        int serviceId = 0;
                        int.TryParse(item.CategoryID, out serviceId);
                        ServiceCompany serviceCompany = new ServiceCompany
                        {
                            ProfileID = profile.ProfileID,
                            ServiceID = serviceId,
                            Status = 0,
                            Created = DateTime.Now,
                            Modified = DateTime.Now

                        };
                        lstServicenNew.Add(serviceCompany);
                    }

                    _companyProfileService.UpdateServiceCompany(lstServicenNew, lstServicenOld);
                }

                List<CompanyMedia> companyMediaOld = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID);
                List<CompanyMedia> lstcompanyMediaNew = new List<CompanyMedia>();
                if (model.LogoID != 0)
                {
                    CompanyMedia companyMedialogo = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMedialogo.MediaID = model.LogoID;
                    companyMedialogo.IsLogo = true;
                    lstcompanyMediaNew.Add(companyMedialogo);
                }
                if (model.PhotoID != 0)
                {
                    CompanyMedia companyMediaphoto = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMediaphoto.MediaID = model.PhotoID;
                    companyMediaphoto.IsBanner = true;
                    lstcompanyMediaNew.Add(companyMediaphoto);
                }
                if (model.VideoID != 0)
                {
                    CompanyMedia companyMediavideo = new CompanyMedia()
                    {
                        ProfileID = profile.ProfileID
                    };
                    companyMediavideo.MediaID = model.VideoID;
                    companyMediavideo.IsVideo = true;
                    lstcompanyMediaNew.Add(companyMediavideo);
                }
                _companyProfileService.UpdateCompanyMedia(lstcompanyMediaNew, companyMediaOld);

                if (!string.IsNullOrEmpty(newEmail))
                {

                    try
                    {
                        //_emailSender.SendEmail("Change infomation", body.ToString(), EmailHelper.EmailSystem, EmailHelper.NameSystem, newEmail, profileCompany.Name);
                        if ((profileCompany.PreferredContact.Value & (int)Types.PreferredPhone.Email) != 0)
                        {
                            EmailHelper.SendEmailUserInfomationChanged(newEmail, newEmail);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                string str = "saveChange();";
                ViewBag.PopupString = MvcHtmlString.Create(str);
                return View(model);


                //return RedirectToAction("CompanySetting");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteCompany(string pass, string reason, int profileId = 0)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
            {
                result.Add("status", "0");
                result.Add("message", "company is not exits!");
                return Json(result);
            }

            if (!_membershipService.ValidateUser(MySession.Username, pass))
            {
                result.Add("status", "0");
                result.Add("message", "password is invalid or user is locked!");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            _companyProfileService.DeleteCompany(profile.ProfileID, reason);
            result.Add("status", "1");
            result.Add("message", "Copany deleted");
            return Json(result);
        }

        [Authorize]
        public ActionResult CompanySetting()
        {
            int profileID = this.ProfileId;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }

            CompanySettingModel model = new CompanySettingModel();
            model = profile.ProfileCompany.ToCompanySettingModel();
            model.ProfileID = profile.ProfileID;
            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            model.FirstListItem = _companyProfileService.GetListAlertTime(model.FirstAlert);
            model.SecondListItem = _companyProfileService.GetListAlertTime(model.SecondAlert);
            //Trong edit
            if (profile != null && profile.ProfileCompany != null)
            {
                model.CompanyType = profile.ProfileCompany.CompanyTypeID;
                if (profile.Cust != null)
                {
                    model.FirstName = profile.Cust.FirstName;
                    model.LastName = profile.Cust.LastName;
                    model.CustEmail = profile.Cust.Email;
                }

            }
            //
            Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.Phone));
            model.SelectCarrier = profile.ProfileCompany.MobileCarrier;
            if (invite != null)
            {
                ViewBag.Defaultfuntion = "isverification();";
            }
            Session["companyphoneNumber"] = UtilityHelper.CleanPhone(model.Phone);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult CompanySetting(CompanySettingModel model)
        {
            int profileID = this.ProfileId;

            if (!model.EmailType)
                ModelState.Remove("Email");
            if (!model.TextType)
                ModelState.Remove("Phone");
            Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.Phone));
            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            string phone = Session["companyphoneNumber"] != null ? Session["companyphoneNumber"] as string : string.Empty;
            string newphone = UtilityHelper.CleanPhone(model.Phone);
            if (invite == null || (phone != newphone && Session["phoneNumber"] == null))
            {
                if (model.TextType)
                {
                    ModelState.AddModelError("VerifiPhoneNumber", "unverified phone number");
                    ViewBag.Defaultfuntion = "setDefaultvalue();";
                }
            }
            else
            {
                if (invite.PhoneNumber != UtilityHelper.CleanPhone(model.Phone))
                {
                    ModelState.AddModelError("VerifiPhoneNumber", "phone number do not match");
                    ViewBag.Defaultfuntion = "setDefaultvalue();";
                }

            }
            if (ModelState.IsValid)
            {
                ProfileCompany profileCompany = _companyProfileService.GetProfileCompanyByID(model.ProfileID);
                if (profileCompany == null)
                    return View(model);
                if (model.EmailType)
                    profileCompany.Email = model.Email;
                if (model.TextType)
                    profileCompany.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                profileCompany.FirstAlert = model.FirstAlert;
                profileCompany.SecondAlert = model.SecondAlert;
                int PreferredContact = 0;
                if (model.EmailType == true)
                {
                    PreferredContact |= (int)Types.PreferredPhone.Email;
                }
                if (model.TextType == true)
                {
                    PreferredContact |= (int)Types.PreferredPhone.Text;
                }

                profileCompany.PreferredContact = PreferredContact;
                profileCompany.PayAfter = model.Policy;
                profileCompany.PaymentMethod = model.PaymentMethod;
                profileCompany.MobileCarrier = model.SelectCarrier;
                profileCompany.PaymentOptions = model.PaymentOptions;
                //Trong edit
                if (model.CompanyType == (int)Types.CompanyType.KuyamBookIt || model.CompanyType == (int)Types.CompanyType.KuyamInstantBook)
                {
                    profileCompany.CompanyTypeID = model.CompanyType;
                }

                //
                int hour = 0;
                int percent = 0;
                profileCompany.CancelPolicy = model.CancelPolicy;
                profileCompany.ContactFirstName = model.ContactFirstName;
                profileCompany.ContactLastName = model.ContactLastName;
                if (model.CancelPolicy == (int)Types.CancellationType.None)
                {
                    model.anytime = "0";
                    model.norefund = "0";
                }
                else if (model.CancelPolicy == (int)Types.CancellationType.Standard)
                {
                    model.anytime = "24";
                    model.norefund = "50";
                }
                else if (model.CancelPolicy == (int)Types.CancellationType.Strict)
                {
                    model.anytime = "72";
                    model.norefund = "50";
                }
                profileCompany.CancelHour = Int32.TryParse(model.anytime, out hour) ? hour : 0;
                profileCompany.CancelRefundPercent = Int32.TryParse(model.norefund, out percent) ? percent : 0;
                //
                _companyProfileService.UpdateProfileCompany(profileCompany);

                string newPhone = string.Empty;
                string newEmail = string.Empty;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    newEmail = model.Email;
                }
                else
                {
                    newEmail = profileCompany.Email;
                }
                if (!string.IsNullOrEmpty(model.Phone))
                {
                    newPhone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                }
                else
                {
                    newPhone = Kuyam.Domain.UtilityHelper.CleanPhone(profileCompany.Phone);
                }

                try
                {
                    if ((profileCompany.PreferredContact.Value & (int)Types.PreferredPhone.Email) != 0)
                    {
                        EmailHelper.SendEmailUserInfomationChanged(newEmail, newEmail, newPhone);
                    }
                }
                catch
                {
                    throw;
                }

                string str = "saveChange();";
                ViewBag.PopupString = MvcHtmlString.Create(str);
            }
            model.FirstListItem = _companyProfileService.GetListAlertTime(model.FirstAlert);
            model.SecondListItem = _companyProfileService.GetListAlertTime(model.SecondAlert);

            return View(model);
        }

        public ActionResult PostSetup()
        {
            return View();
        }


        public ActionResult VerificationCode()
        {
            Profile profile = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profile != null)
                ViewBag.CompanyStatus = Enum.ToObject(typeof(Types.CompanyStatus), profile.ProfileCompany.CompanyStatusID).ToString();
            else
                ViewBag.CompanyStatus = string.Empty;
            return View();
        }

        [HttpPost]
        public ActionResult VerificationCode(string code)
        {
            if (ModelState.IsValid)
            {
                string template30DayTrialBeginResult = string.Empty;
                string template30daysoverResult = string.Empty;
                //30daysover
                try
                {
                    string template = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/30daysover.cshtml")))
                    {
                        template = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = MySession.Cust.FirstName.ToString(),
                        Email = MySession.Username
                    };
                    template30daysoverResult = RazorEngine.Razor.Parse(template, myObject);
                }
                catch
                {
                    throw;
                }
                //30daytrialbegin
                try
                {
                    string template30DayTrialBegin = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/30daytrialbegin.cshtml")))
                    {
                        template30DayTrialBegin = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = MySession.Cust.FirstName.ToString(),
                        Email = MySession.Username
                    };
                    template30DayTrialBeginResult = RazorEngine.Razor.Parse(template30DayTrialBegin, myObject);
                }
                catch
                {
                    throw;
                }


                if (code == null || code == string.Empty)
                {
                    ModelState.AddModelError("activated", "invite code is required");
                    return View();
                }

                Cust cust = DAL.xGetCust(MySession.CustID);


                Invite invite = _companyProfileService.GetInvite(code, cust.CustID);
                if (invite != null)
                {
                    if (invite.Active.HasValue && invite.Active.Value)
                    {
                        ModelState.AddModelError("activated", "invite code was activated, please try with another.");
                        return View();
                    }
                    try
                    {
                        ProfileCompany profiles = _companyProfileService.GetProfileCompanyByCustID(MySession.CustID);
                        _companyProfileService.Acvite(invite);
                        profiles.CompanyStatusID = (int)Types.CompanyStatus.Active;
                        profiles.Modified = DateTime.UtcNow;
                        _companyProfileService.UpdateProfileCompany(profiles);

                        EmailHelper.SendEmailCompany30DayTrialBeginCode(profiles.Email, template30DayTrialBeginResult);

                        string emailBcc = string.Empty;
                        if (MyApp.Settings.Admin.EnableEmailBcc)
                            emailBcc = MyApp.Settings.Admin.EmailBcc;
                        InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                        IncomingRequest obj = new IncomingRequest
                        {
                            EntityId = "KuyamWeb",
                            DateAlert = DateTime.UtcNow.AddDays(29),
                            Data = Kuyam.Domain.UtilityHelper.ObjectToXml(new { Emailtemplate = template30daysoverResult, Emailto = profiles.Email, EmailBcc = emailBcc, Subject = "30 Days Over" })
                        };
                        serviceInfo.AddIncomingRequest(obj, IncommingRequestType.SEND_EMAIL);

                        return RedirectToAction("ServiceType");
                    }
                    catch
                    {
                        ModelState.AddModelError("activated", "invite code was activated, please try with another.");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("activated", "invite code is invalid");
                    return View();
                }
            }
            return View();
        }


        public ActionResult ServiceType()
        {
            Profile profiles = _companyProfileService.IsVerified(MySession.CustID);
            if (profiles == null)
                return RedirectToAction("VerificationCode");
            return View();
        }

        //public ActionResult Confirmation(Guid id)
        //{
        //    BusinessService.ConfirmCustomerSchedules(id);

        //    return View();
        //}

        public ActionResult IscityAndState(string city)
        {
            if (string.IsNullOrEmpty(city) && string.IsNullOrWhiteSpace(city))
                return Json(string.Format("{0} is invalid", city), JsonRequestBehavior.AllowGet);
            string[] result = city.Split(',');
            if (result.Count() < 2)
                return Json(string.Format("{0} is invalid", "fomat"), JsonRequestBehavior.AllowGet);
            else if (string.IsNullOrEmpty(result[1]) || string.IsNullOrWhiteSpace(result[1]))//Trong: Changed 'result[1] == ""' to 'string.IsNullOrEmpty(result[1])||string.IsNullOrWhiteSpace(result[1])'
            {
                return Json(string.Format("{0} is invalid", "fomat"), JsonRequestBehavior.AllowGet);
            }
            else if (result[1].Length > 4)
            {
                return Json(string.Format("{0} no more 4 character ", "state"), JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);

        }

        public ActionResult IsZipCode(string zip)
        {
            ZipCode zipCode = _companyProfileService.GetZipCodeByKey(zip);

            //if (zipCode != null){
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //Trong edit
            if (zipCode == null)
            {
                return Json(string.Format("{0} is invalid", zip), JsonRequestBehavior.AllowGet);
            }
            else if (zipCode.Active == false)
            {
                return Json(string.Format("{0} is inactive", zip), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult AddToFavorite(string profileID)
        {
            bool result = false;
            Cust cust = MySession.Cust;
            if (!DAL.isFavorite(cust.CustID, int.Parse(profileID)))
            {
                Favorite fav = new Favorite();
                fav.ProfileID = int.Parse(profileID);

                fav.CustID = cust.CustID;
                fav.CreatedTime = DateTime.Now;
                result = ProfileCompany.AddToFavorite(fav);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RemoveFavorite(string profileID)
        {
            bool result = false;
            Cust cust = MySession.Cust;
            Favorite fav = DAL.GetFavoriteByCustIDProfileID(cust.CustID, int.Parse(profileID));
            if (fav != null)
                result = DAL.DeleteFavorite(fav);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFavorite()
        {

            List<cFavorite> result = new List<cFavorite>();
            List<ProfileCompany> favoriteResult = ProfileCompany.GetFavoriteListByCustID(MySession.CustID);

            foreach (var fav in favoriteResult)
            {
                cFavorite f = new cFavorite();
                f.ProfileID = fav.ProfileID;
                f.Name = fav.Name;
                result.Add(f);

            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SignUpNow()
        {
            string fname = Request.Params["fname"];
            string lname = Request.Params["lname"];
            string email = Request.Params["email"];
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (fname.Equals("Name") || email.Equals("E-mail address"))
            {
                result.Add("status", "0");
                result.Add("message", "please input data");
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            var cust = Cust.Load(email);

            if (cust == null || !cust.GetRole.Contains("Guest"))
            {
                if (DAL.IsExistEmailAddress(email))
                {
                    result.Add("status", "0");
                    result.Add("message", "this email has been used");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (AccountHelper.InviteCodeCheckStatus(email))
                {

                    result.Add("status", "0");
                    result.Add("message", "this email has been used");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }

            try
            {
                string inviteKey = AccountHelper.AddInviteCode(email, fname, lname);
                Kuyam.Domain.AccountHelper.UpdateInviteStatus(email);

                bool isValidKey = AccountHelper.InviteCodeIsValid(inviteKey);
                Invite invite = DAL.GetInvite(inviteKey);

                if (invite != null && isValidKey)
                {

                    if (MySession.RegisterModel == null)
                        MySession.RegisterModel = new RegisterModel();

                    MySession.RegisterModel.FirstName = invite.Name;
                    MySession.RegisterModel.LastName = invite.LName;
                    MySession.RegisterModel.ContactEmail = invite.Email;
                    MySession.RegisterModel.TestKeyIsValid = isValidKey;

                    //return RedirectToAction("RegisterEmail", "Account");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
            }

            result.Add("status", "1");
            result.Add("message", "Thank you for signing up to join kuyam");

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CompanyServices()
        {
            int profileId = this.ProfileId;
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            int profileID = profiles.ProfileID;
            List<ServiceCompany> scList = new List<ServiceCompany>();
            scList = DAL.GetCompanySevicesByProfileID(profileID);
            ViewBag.CompanyServices = scList;

            return View();
        }

        [Authorize]
        public ActionResult CompanyServiceEdit(int? id)
        {
            Profile profiles = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (!id.HasValue)
                return RedirectToAction("CompanyService");
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            int profileID = profiles.ProfileID;
            List<ServiceCompany> scList = new List<ServiceCompany>();
            scList = DAL.GetCompanySevicesByProfileID(profileID);
            ViewBag.CompanyServices = scList;

            ServiceCompany sc = DAL.GetServiceCompany(id.Value);
            if (sc == null)
                return RedirectToAction("CompanyService");
            Service service = DAL.GetService(sc.ServiceID);

            ViewBag.CategoryId = service.ParentServiceID;
            ViewBag.ServiceCompany = sc;
            return View();

        }

        [HttpPost]
        public ActionResult GetServices(int categoryID)
        {
            ViewBag.CategoryID = categoryID;
            List<Service> services = new List<Service>();
            services = DAL.GetServicesByCategoryID(categoryID);
            string serviceHTML = string.Empty;
            if (services != null && services.Count > 0)
            {
                serviceHTML = "<select name=\"category\" id=\"service\" class=\"selectservice selectserviceactive\">";
                serviceHTML = serviceHTML + "<option value=\"select a service\" selected=\"selected\">select a service</option>";
                foreach (Service service in services)
                {
                    serviceHTML = serviceHTML + "<option value=\"" + service.ServiceID + "\" serviceID=\"" + service.ServiceID + "\">" + Kuyam.Domain.UtilityHelper.TruncateText(service.ServiceName, 27) + "</option>";
                }
                serviceHTML = serviceHTML + "</select>";
            }

            return Json(serviceHTML, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddService(int serviceID, int time, decimal price, int maxPeople, string description, int profileId = 0)
        {
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);

            if (profiles == null)
                return Json("false", JsonRequestBehavior.AllowGet);

            int profileID = profiles.ProfileID;
            ServiceCompany sc = new ServiceCompany();

            sc.ProfileID = profileID;
            sc.ServiceID = serviceID;
            sc.Duration = time;
            sc.AttendeesNumber = maxPeople;
            sc.Price = price;
            sc.Description = description;
            sc.Created = DateTime.Now;
            sc.Status = (int)Types.ServiceCompanyStatus.Active;
            sc.ServiceName = DAL.GetServiceNameFromServiceID(serviceID);
            bool result = DAL.AddServiceCompany(sc);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateServiceCompany(int serviceCompanyID, int serviceID, int time, decimal price, int maxPeople, string description)
        {
            bool result = false;
            ServiceCompany sc = DAL.GetServiceCompany(serviceCompanyID);
            if (sc != null)
            {
                sc.Duration = time;
                sc.AttendeesNumber = maxPeople;
                sc.Price = price;
                sc.Description = description;
                sc.ServiceID = serviceID;
                sc.Modified = DateTime.UtcNow;

                if (sc.Appointments.Count > 0)
                {
                    ServiceCompany scItem = DAL.GetServiceCompany(serviceCompanyID);
                    scItem.Description = description;
                    result = DAL.UpdateServiceCompany(scItem);
                }
                else
                {
                    result = DAL.UpdateServiceCompany(sc);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteServiceCompany(int serviceCompanyID)
        {
            bool result = DAL.DeleteServiceCompany(serviceCompanyID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult CompanyEmployee(int? id)
        {
            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");

            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");


            int profileID = profiles.ProfileID;
            List<ServiceCompany> scList = new List<ServiceCompany>();
            scList = DAL.GetCompanySevicesByProfileID(profileID);
            ViewBag.CompanyServices = scList;
            ViewBag.CompanyEmployees = DAL.GetCompanyEmployeesByProfileID(profileID);

            if (id.HasValue && id.Value > 0)
            {
                CompanyEmployee employee = new CompanyEmployee();
                employee = ProfileCompany.GetCompanyEmployee(id.Value);
                if (employee != null)
                {
                    ViewBag.Employee = employee;
                    List<EmployeeService> esList = DAL.GetEmployeeServicesFromEmployeeID(id.Value);
                    string scListIDs = string.Empty;
                    if (esList != null && esList.Count > 0)
                    {
                        foreach (EmployeeService service in esList)
                        {
                            scListIDs = scListIDs + service.ServiceCompanyID.ToString() + ",";
                        }
                    }
                    ViewBag.StringSCListIDs = scListIDs;
                }

            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult UploadFiles()
        {

            MySession.iCalUpload = Request.Files[0] as HttpPostedFileBase;

            var name = Path.GetFileName(Request.Files[0].FileName);
            if (name.Length > 60)
            {
                name = Kuyam.Domain.UtilityHelper.TruncateText(name, 60);
            }
            return new JsonResult { ContentType = "text/html", Data = name };
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddEmployeeInFo(string employeeName, string email, string phone, string stringServiceCompanyIDs)
        {
            // HttpFileCollectionBase files = ControllerContext.HttpContext.Request.Files;
            //string fileContent = new StreamReader(file.InputStream).ReadToEnd();
            var buttonClicked = Request.Form["fileUpload"];

            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            int profileID = profiles.ProfileID;
            CompanyEmployee employee = new CompanyEmployee();
            employee.EmployeeName = employeeName;
            employee.Email = email;
            employee.ProfileCompanyID = profileID;
            employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
            int employeeID = DAL.AddEmployee(employee);
            bool result = false;

            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        Kuyam.Database.EmployeeService service = new Kuyam.Database.EmployeeService();
                        service.CompanyEmployeeID = employeeID;
                        service.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = DAL.AddEmployeeService(service);
                    }
                }
            }
            if (MySession.iCalUpload != null)
            {
                InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

                var connectorSource = client.GetConnectorSource(employeeID, InfoConnServiceReference.ConnectorSourceType.iCalendar);
                if (connectorSource == null)
                {
                    connectorSource = new InfoConnServiceReference.ConnectorSource
                    {
                        UserId = employeeID,
                        ConnectorSourceType = (int)ConnectorSourceType.iCalendar,
                        ExpiresDate = DateTime.Now,
                        LastModified = DateTime.Now,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.Now,
                        CacheLastUpdate_Longer = DateTime.Now,
                        CacheLastUpdate_Medium = DateTime.Now,
                        DoCacheUpdate_Longer = false,
                        DoCacheUpdate_Medium = false,
                        DoCacheUpdate_Short = false
                    };
                    client.AddConnectorSource(connectorSource);
                }
                string fileContent = new StreamReader(MySession.iCalUpload.InputStream).ReadToEnd();

                InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
                option.ICSString = fileContent;
                client.SaveEvents(employeeID, option, InfoConnServiceReference.ConnectorSourceType.iCalendar);
                MySession.iCalUpload = null;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditEmployeeInFo(string employeeID, string employeeName, string email, string phone, string stringServiceCompanyIDs)
        {
            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            int profileID = profiles.ProfileID;
            int empID = 0;
            if (!int.TryParse(employeeID, out empID))
            {
                return RedirectToAction("CompanyEmployee");
            }

            CompanyEmployee employee = ProfileCompany.GetCompanyEmployee(empID);
            if (employee != null)
            {
                employee.EmployeeName = employeeName;
                employee.Email = email;
                employee.ProfileCompanyID = profileID;
                employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
                DAL.UpdateCompanyEmployeeInfo(employee);
                DAL.DeleteEmployeeServicesByEmployeeID(employee.EmployeeID);
            }

            bool result = false;

            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        Kuyam.Database.EmployeeService service = new Kuyam.Database.EmployeeService();
                        service.CompanyEmployeeID = empID;
                        service.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = DAL.AddEmployeeService(service);
                    }
                }
            }

            int empl = int.Parse(employeeID);

            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            var connectorSource = client.GetConnectorSource(empl, InfoConnServiceReference.ConnectorSourceType.iCalendar);

            if (connectorSource == null)
            {
                connectorSource = new InfoConnServiceReference.ConnectorSource
                {
                    UserId = empl,
                    AccessToken = "",
                    RefressToken = string.Empty,
                    ExpiresDate = DateTime.Now,
                    ConnectorSourceType = (int)ConnectorSourceType.iCalendar,
                    LastModified = DateTime.Now,
                    IsUpdateRunning = false,
                    CacheLastUpdate_Short = DateTime.Now,
                    CacheLastUpdate_Longer = DateTime.Now,
                    CacheLastUpdate_Medium = DateTime.Now,
                    DoCacheUpdate_Longer = false,
                    DoCacheUpdate_Medium = false,
                    DoCacheUpdate_Short = true
                };

                client.AddConnectorSource(connectorSource);

            }

            if (MySession.iCalUpload != null)
            {
                try
                {
                    InfoConnSoapClient service = new InfoConnSoapClient();
                    string fileContent = new StreamReader(MySession.iCalUpload.InputStream).ReadToEnd();
                    InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
                    option.ICSString = fileContent;
                    service.SaveEvents(empID, option, ConnectorSourceType.iCalendar);
                    MySession.iCalUpload = null;
                }
                catch
                {
                    throw;
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteEmployeeInFo(string employeeID)
        {
            Profile profiles = _companyProfileService.GetProfileByID(MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");
            int profileID = profiles.ProfileID;
            int empID = 0;
            if (!int.TryParse(employeeID, out empID))
            {
                return RedirectToAction("Employee");
            }

            CompanyEmployee employee = ProfileCompany.GetCompanyEmployee(empID);

            bool result = DAL.DeleteEmployeeServicesByEmployeeID(employee.EmployeeID);
            if (result)
            {
                result = DAL.DeleteEmployeeByEmployeeID(employee.EmployeeID);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private List<BusyInfo> GetServiceHourByListServices(bool flag)
        {
            string html = string.Empty;
            Cust user = MySession.Cust;
            DateTime dtNow = DateTime.Now;
            DateTime beginDay = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

            var service = new InfoConnSoapClient();
            var searchOption = new Kuyam.WebUI.InfoConnServiceReference.SearchOption()
            {
                UId = user.CustID,
                StartDate = beginDay.AddDays(-1),
                EndDate = beginDay.AddDays(7),
                ConnectorSourceType = ConnectorSourceType.All
            };

            List<BusyInfo> events = new List<BusyInfo>();

            List<KuyamEvent> kuyamEvent = new List<KuyamEvent>();

            if (flag)
            {
                events = service.GetBusyInfo(searchOption).ToList();
            }
            return events;
        }

        [Authorize]
        public ActionResult CompanyEmployeeListHour(int? id, int companyId = 0)
        {
            string fromHour = Request.QueryString["fromHour"];
            string toHour = Request.QueryString["toHour"];
            ViewBag.FromHour = fromHour;
            ViewBag.ToHour = toHour;
            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc).ToString();
            companyId = this.ProfileId != 0 ? this.ProfileId : companyId;
            Profile profiles = _companyProfileService.GetProfileByID(companyId != 0 ? companyId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");

            int profileID = profiles.ProfileID;
            ViewBag.CompanyEmployees = DAL.GetCompanyEmployeesByProfileID(profileID);


            if (id.HasValue && id.Value > 0)
            {
                CompanyEmployee employee = new CompanyEmployee();
                employee = ProfileCompany.GetCompanyEmployee(id.Value);
                List<ServiceCompany> scList = DAL.GetCompanyServiceByEmployeeID(id.Value);
                ViewBag.CompanyServices = scList;
                if (employee != null)
                {
                    ViewBag.Employee = employee;
                }

                string companyHours = BusinessService.GetCompanyHoursJson(profileID);
                ViewBag.CompanyHours = companyHours;

                List<EmployeeHour> ehList = DAL.GetEmployeeHoursFromEmployeeID(id.Value);
                List<EmployeeHour> ihList = _classService.GetInstructorHoursFromInstructorId(id.Value);
                if (ehList != null)
                {
                    ViewBag.StringEvent = BusinessService.GetEventCalendar(ehList);
                }

                if (ihList != null)
                {
                    ViewBag.StringClassHour = BusinessService.GetListClassHourCalendar(ihList);
                }

                List<EmployeeHour> previewHour = DAL.GetEmployeeHourPreview(id.Value);
                ViewBag.previewHour = previewHour;


            }
            return View();


        }

        public ActionResult TurnOnOffEmployyCalendar(string status)
        {
            MySession.CompanyEmployeeBusyOnOff = status.ToLower();
            return Json(null);
        }


        [HttpGet]
        public ActionResult GetCompanyEmpoyeeBusyInfo(string employeeid)
        {

            List<CalendarObject> listBusyInfoEmployee = new List<CalendarObject>();

            //if (MySession.CompanyEmployeeBusyOnOff == "on")
            //{
            var service = new InfoConnSoapClient();

            Kuyam.WebUI.InfoConnServiceReference.SearchOption searchOption = new Kuyam.WebUI.InfoConnServiceReference.SearchOption();
            searchOption.ConnectorSourceType = ConnectorSourceType.All;
            searchOption.UId = int.Parse(employeeid);
            searchOption.StartDate = DateTime.Now.Date;
            searchOption.EndDate = searchOption.StartDate.Value.AddDays(7);
            List<BusyInfo> employeeBusyInfos = service.GetBusyInfo(searchOption).ToList();


            foreach (BusyInfo employeeBusyInfo in employeeBusyInfos)
            {
                CalendarObject calObject = new CalendarObject();
                calObject.id = employeeBusyInfo.EndDate.Ticks.ToString();
                calObject.start = employeeBusyInfo.StartDate.ToString("yyyy-MM-dd hh:mm");
                calObject.end = employeeBusyInfo.EndDate.ToString("yyyy-MM-dd hh:mm");
                calObject.title = "";
                calObject.className = "fc-event-skin-red";
                listBusyInfoEmployee.Add(calObject);
            }

            // }
            //listBusyInfoEmployee.Add(new CalendarObject() { id = "1", title = "1", start = "2012-09-29", end = "2012-09-30" });

            return Json(listBusyInfoEmployee, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public ActionResult AddEmployeeHour(int employeeID, string fromHour, string toHour, string stringListDays, int profileId = 0)
        {
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return Json("false", JsonRequestBehavior.AllowGet);

            int profileID = profiles.ProfileID;
            DateTime lasted = DateTime.Now;
            EmployeeHour eh = new EmployeeHour();
            eh.CompanyEmployeeID = employeeID;
            //TimeSpan start = TimeSpan.Parse("07:00:00");
            //TimeSpan end = TimeSpan.Parse("23:00:00");            
            eh.FromHour = TimeSpan.Parse(DateTime.Parse(fromHour).TimeOfDay.ToString());
            eh.ToHour = TimeSpan.Parse(DateTime.Parse(toHour).TimeOfDay.ToString());
            eh.LastedUpdate = lasted;
            eh.IsPreview = false;
            var result = false;
            if (eh.FromHour < eh.ToHour)// && eh.FromHour >= start && eh.ToHour <= end
            {
                //if (stringListDays != string.Empty)
                //{
                //    eh.DayOfWeek = int.Parse(stringListDays);
                //}
                if (stringListDays != string.Empty)
                {
                    string[] listDay = stringListDays.Split(',');
                    result = DAL.AddEmployeeHour(eh, listDay);
                }
                //if (DAL.UpdateLastedTimeForEmployeeHour(employeeID, lasted))
                //{
                //    result = DAL.AddEmployeeHour(eh, listDay);
                //}
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public ActionResult AddPreviewEmployeeHour(int employeeID, string fromHour, string toHour, string stringListDays, int profileId = 0)
        {
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            //DateTime lasted = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now);

            EmployeeHour eh = new EmployeeHour();
            eh.CompanyEmployeeID = employeeID;
            //TimeSpan start = TimeSpan.Parse("07:00:00");
            //TimeSpan end = TimeSpan.Parse("23:00:00");
            eh.FromHour = TimeSpan.Parse(DateTime.Parse(fromHour).TimeOfDay.ToString());
            eh.ToHour = TimeSpan.Parse(DateTime.Parse(toHour).TimeOfDay.ToString());
            var result = false;
            if (eh.FromHour < eh.ToHour)
            {
                eh.LastedUpdate = DateTime.Now;
                eh.IsPreview = true; // Preview Mode
                if (stringListDays != string.Empty)
                {
                    string[] listDay = stringListDays.Split(',');
                    result = DAL.AddEmployeeHour(eh, listDay);
                }
                else
                {
                    result = DAL.AddEmployeeHour(eh, new string[0]);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteEmployeeHour(int employeeID, int employeeHourID, string employeeHourEditDate)
        {
            //DateTime dateDelete = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now).AddDays(-1).Date;
            //DateTime.TryParse(employeeHourEditDate, out dateDelete);
            //DateTime lasted = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now);
            var result = DAL.DeleteEmployeeHour(employeeHourID, employeeID);
            //DAL.UpdateLastedTimeForEmployeeHour(employeeID, lasted);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditEmployeeHour(int employeeID, int employeeHourID, string employeeHourEditDate, string fromHour, string toHour, int profileId = 0)
        {
            DateTime dateDelete = DateTime.Now.AddDays(-1).Date;//DateTimeUltility.ConvertToUtcMinus7(DateTime.Now).AddDays(-1).Date;
            DateTime.TryParse(employeeHourEditDate, out dateDelete);
            var result = false;
            DateTime lasted = DateTime.Now;//DateTimeUltility.ConvertToUtcMinus7(DateTime.Now);

            TimeSpan newStartTime = TimeSpan.Parse(DateTime.Parse(fromHour).TimeOfDay.ToString());
            TimeSpan newEndTime = TimeSpan.Parse(DateTime.Parse(toHour).TimeOfDay.ToString());

            EmployeeHour eh = new EmployeeHour();
            eh = DAL.GetEmployeeHourByemployeeHourID(employeeHourID);
            eh.CompanyEmployeeID = employeeID;

            if (!DAL.IsEmployeeHourEditable(eh, newStartTime, newEndTime))
            {
                return Json("these is some appointments already registered within old employee hour", JsonRequestBehavior.AllowGet);
            }

            eh.FromHour = newStartTime;
            eh.ToHour = newEndTime;
            //TimeSpan start = TimeSpan.Parse("07:00:00");
            //TimeSpan end = TimeSpan.Parse("23:00:00");
            if (eh.FromHour < eh.ToHour)// && eh.FromHour >= start && eh.ToHour <= end
            {

                string[] listDay = eh.DayOfWeek.ToString().Split(',');
                result = DAL.AddEmployeeHour(eh, listDay);
            }
            else
            {
                result = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Request of user add new company
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="contactName"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="address"></param>
        /// <param name="city"></param>
        /// <param name="zipCode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RequestAddNewCompany(string companyName, string contactName, string phoneNumber, string address, string city, string zipCode)
        {
            EmailHelper.SendEmailUserAddCompany(string.Empty, companyName, contactName, phoneNumber, address, city, zipCode);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CallbackCreateAccountPaypal()
        {
            return View();
        }

        public ActionResult ReviewDetail(int profileId)
        {
            ViewBag.ListServiceCompany = _companyProfileService.GetServiceCompanyByProfileID(profileId);
            return PartialView("_ReviewDetail"); ;
        }

        #region Company Setup New

        [Authorize]
        public ActionResult SetupBasicEdit()
        {
            int profileID = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileID);

            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            CompanySetupModel model = new CompanySetupModel();
            List<Service> lstService = _companyProfileService.GetListService();
            List<ServiceCompany> serviceCompanies = _companyProfileService.GetCategoryByProfileID(profileID);
            model.ProfileID = profile.ProfileID;
            model.Name = profile.ProfileCompany.Name;
            model.Street1 = profile.ProfileCompany.Street1;
            model.Street2 = profile.ProfileCompany.Street2;
            model.City = string.Format("{0},{1}", profile.ProfileCompany.City, profile.ProfileCompany.State);
            model.State = UtilityHelper.TruncateData(profile.ProfileCompany.State, 4);
            model.Zip = profile.ProfileCompany.Zip;
            model.Email = profile.ProfileCompany.Email;
            if (profile.ProfileCompany != null)
            {
                model.Desc = profile.ProfileCompany.Desc;
            }

            model.Url = profile.ProfileCompany.Url;
            model.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(profile.ProfileCompany.Phone);
            model.ContactName = profile.ProfileCompany.ContactName;
            model.CompanyStatusID = profile.ProfileCompany.CompanyStatusID;

            foreach (ServiceCompany item in serviceCompanies)
            {
                Category category = new Category();
                category.CategoryID = item.ServiceID.ToString();
                category.NamCategory = item.Service.ServiceName;
                model.Categories.Add(category);
            }

            foreach (CompanyHour item in profile.ProfileCompany.CompanyHours)
            {
                string dayOfWeek = GetDayString(item.DayOfWeek);
                if (item.IsDaily.HasValue && item.IsDaily.Value)
                {
                    dayOfWeek = "isdaily";
                }

                string fromHour = String.Format("{0:hh:mmtt}", new DateTime(item.FromHour.Ticks)).ToLower();
                string toHour = String.Format("{0:hh:mmtt}", new DateTime(item.ToHour.Ticks)).ToLower();
                if (fromHour.StartsWith("0"))
                {
                    fromHour = fromHour.Substring(1);
                }
                if (toHour.StartsWith("0"))
                {
                    toHour = toHour.Substring(1);
                }
                string id = string.Format("{0},{1},{2}", dayOfWeek, fromHour, toHour);
                string day = string.Format("{0} {1}-{2}", GetDayString(item.DayOfWeek), fromHour, toHour);
                int companyIndex = item.DayOfWeek;

                if (item.DayOfWeek == 0)
                {
                    companyIndex = 7;
                }

                Companylist company = new Companylist()
                {
                    CompanyIndex = companyIndex,
                    CompanyID = id,
                    CompanyNam = day
                };

                model.Companylist.Add(company);
            }
            model.Companylist.OrderBy(c => c.CompanyIndex);

            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SetupBasicEdit(CompanySetupModel model, string hdcategory, string hdhour)
        {

            List<Service> lstService = _companyProfileService.GetListService();


            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            ModelState.Remove("Name");
            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {
                string newEmail = string.Empty;
                if (!string.IsNullOrEmpty(hdcategory))
                {
                    model.Listcategory = hdcategory.Split(',');
                    foreach (string item in model.Listcategory)
                    {
                        int id = 0;
                        int.TryParse(item, out id);
                        Category category = new Category();
                        var service = _companyProfileService.GetServiceByID(id);
                        if (service != null)
                        {
                            category.CategoryID = item;
                            category.NamCategory = service.ServiceName;
                            model.Categories.Add(category);
                        }
                    }
                }

                string[] stateCity = model.City.Split(',');
                string strAddress = string.Empty;

                if (!string.IsNullOrEmpty(model.Street1))
                    strAddress += model.Street1;
                if (!string.IsNullOrEmpty(model.Street2))
                {
                    if (!string.IsNullOrEmpty(model.Street1))
                    {
                        strAddress += "," + model.Street2;
                    }
                    else
                    {
                        strAddress += model.Street2;
                    }
                }
                if (stateCity[0] != "")
                    strAddress = strAddress + "," + stateCity[0];
                if (stateCity[1] != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + ", " + stateCity[1];
                    else
                        strAddress = model.State;
                if (model.Zip != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + " " + model.Zip;
                    else
                        strAddress = model.Zip;
                GeoClass.Coordinate coordinate;
                if (!string.IsNullOrEmpty(strAddress))
                    coordinate = GeoClass.GetCoordinates(strAddress);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);

                if (!string.IsNullOrEmpty(hdhour))
                {
                    model.Listhour = hdhour.Split('|');
                }

                ProfileCompany profileCompany = _companyProfileService.GetProfileCompanyByID(model.ProfileID);

                if (_companyProfileService.CheckCompanyName(model.Name, model.Street1)
                    && UtilityHelper.DamerauLevenshteinDistance(model.Street1, profileCompany.Street1) > 0)
                {
                    ModelState.AddModelError("ExistCompanyName", "company name have been used.");

                    List<Service> lstService1 = _companyProfileService.GetListService();
                    foreach (Service item in lstService1)
                    {
                        model.CategoryCompany.Add(new SelectListItem()
                        {
                            Text = item.ServiceName,
                            Value = item.ServiceID.ToString()
                        });
                    }

                    return View(model);
                }

                profileCompany.Name = model.Name;
                profileCompany.Street1 = model.Street1;
                profileCompany.Street2 = model.Street2;
                profileCompany.City = stateCity[0];
                profileCompany.State = UtilityHelper.TruncateData(stateCity[1], 4);
                profileCompany.Zip = model.Zip;
                profileCompany.Url = model.Url;
                profileCompany.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                profileCompany.ContactName = model.ContactName;
                profileCompany.Latitude = (double)coordinate.Latitude;
                profileCompany.Longitude = (double)coordinate.Longitude;
                profileCompany.Modified = DateTime.Now;
                profileCompany.Desc = model.Desc;

                Profile profile = profileCompany.Profile;
                profile.CustID = profile.CustID;
                profile.Name = model.Name;
                profile.Modified = DateTime.Now;
                profile.ProfileCompany = profileCompany;

                _companyProfileService.UpdateProfile(profile);
                string slug = string.Empty;
                slug = string.Format("{0} {1} {2} {3}", profileCompany.Name, profileCompany.City, profileCompany.State, profileCompany.Street1);
                _seoFriendlyUrlService.SaveSlug(model.ProfileID, slug, "company");
                List<CompanyHour> lstProfileHourOld = _companyProfileService.GetCompanyHourProfileID(profile.ProfileID);
                if (model.Listhour != null && profile.ProfileID != 0)
                {
                    List<CompanyHour> lstProfileHourNew = new List<CompanyHour>();
                    foreach (string item in model.Listhour)
                    {
                        string strday = item.Split(',')[0].Trim();
                        int day = 0;
                        TimeSpan start;
                        TimeSpan end;
                        start = DateTime.Parse(item.Split(',')[1].Trim()).TimeOfDay;
                        end = DateTime.Parse(item.Split(',')[2].Trim()).TimeOfDay;
                        //TimeSpan.TryParseExact(item.Split(',')[1].Trim(), @"hh\:mm", null, out start);
                        //TimeSpan.TryParseExact(item.Split(',')[2].Trim(), @"hh\:mm", null, out end);
                        switch (strday)
                        {
                            case "mon": { day = (int)Types.Day.Monday; break; }
                            case "tue": { day = (int)Types.Day.Tuesday; break; }
                            case "wed": { day = (int)Types.Day.Wednesday; break; }
                            case "thu": { day = (int)Types.Day.Thursday; break; }
                            case "fri": { day = (int)Types.Day.Friday; break; }
                            case "sat": { day = (int)Types.Day.Saturday; break; }
                            case "sun": { day = (int)Types.Day.Sunday; break; }
                            case "isdaily": { day = (int)Types.Day.Isdaily; break; }
                        }
                        CompanyHour profileHour = new CompanyHour()
                        {
                            ProfileCompanyID = profile.ProfileID,
                            DayOfWeek = day,
                            FromHour = start,
                            ToHour = end

                        };
                        if (day == (int)Types.Day.Isdaily)
                        {
                            profileHour.IsDaily = true;
                        }
                        lstProfileHourNew.Add(profileHour);
                    }

                    _companyProfileService.UpdateCompanyHour(lstProfileHourNew, lstProfileHourOld);
                }
                else
                {
                    _companyProfileService.UpdateCompanyHour(null, lstProfileHourOld);
                }

                List<ServiceCompany> lstServicenOld = _companyProfileService.GetCategoryByProfileID(profile.ProfileID);
                if (model.Categories != null && profile.ProfileID != 0)
                {
                    List<ServiceCompany> lstServicenNew = new List<ServiceCompany>();
                    foreach (Category item in model.Categories)
                    {
                        int serviceId = 0;
                        int.TryParse(item.CategoryID, out serviceId);
                        ServiceCompany serviceCompany = new ServiceCompany
                        {
                            ProfileID = profile.ProfileID,
                            ServiceID = serviceId,
                            Status = 0,
                            Created = DateTime.Now,
                            Modified = DateTime.Now

                        };
                        lstServicenNew.Add(serviceCompany);
                    }

                    _companyProfileService.UpdateServiceCompany(lstServicenNew, lstServicenOld);
                }

                return RedirectToAction("Image", "CompanySetup", new { companyId = profile.ProfileID });
                //return View(model);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult SetupBasic()
        {
            //int profileID = this.ProfileId;
            int profileID = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            if (profile != null)
            {
                return RedirectToAction("SetupBasicEdit", new { companyId = profile.ProfileID });
            }
            CompanySetupModel model = new CompanySetupModel();
            model.ProfileID = MySession.ProfileID;
            List<Service> lstService = _companyProfileService.GetListService();
            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SetupBasic(CompanySetupModel model, string hdcategory, string hdhour)
        {

            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {

                if (!string.IsNullOrEmpty(hdcategory))
                {

                    model.Listcategory = hdcategory.Split(',');
                    foreach (string item in model.Listcategory)
                    {

                        int id = 0;
                        int.TryParse(item, out id);
                        Category category = new Category();
                        category.CategoryID = item;
                        category.NamCategory = _companyProfileService.GetServiceByID(id).ServiceName;
                        model.Categories.Add(category);
                    }
                }

                if (!string.IsNullOrEmpty(hdhour))
                {

                    model.Listhour = hdhour.Split('|');
                }

                List<CompanyHour> companyHours = new List<CompanyHour>();
                if (model.Listhour != null)
                {

                    foreach (string item in model.Listhour)
                    {
                        string strday = item.Split(',')[0].Trim();
                        int day = 0;
                        TimeSpan start;
                        TimeSpan end;
                        start = DateTime.Parse(item.Split(',')[1].Trim()).TimeOfDay;
                        end = DateTime.Parse(item.Split(',')[2].Trim()).TimeOfDay;
                        //TimeSpan.TryParseExact(item.Split(',')[1].Trim(), @"hh\:mm", null, out start);
                        //TimeSpan.TryParseExact(item.Split(',')[2].Trim(), @"hh\:mm", null, out end);
                        switch (strday)
                        {
                            case "mon": { day = (int)Types.Day.Monday; break; }
                            case "tue": { day = (int)Types.Day.Tuesday; break; }
                            case "wed": { day = (int)Types.Day.Wednesday; break; }
                            case "thu": { day = (int)Types.Day.Thursday; break; }
                            case "fri": { day = (int)Types.Day.Friday; break; }
                            case "sat": { day = (int)Types.Day.Saturday; break; }
                            case "sun": { day = (int)Types.Day.Sunday; break; }
                            case "isdaily": { day = (int)Types.Day.Isdaily; break; }
                        }
                        Companylist companyHour = new Companylist()
                        {
                        };
                        string tmp = GetDayString(day);
                        if (day == (int)Types.Day.Isdaily)
                        {
                            tmp = "isdaily";
                        }
                        string id = string.Format("{0},{1},{2}", tmp, start.ToString("hh':'mm"), end.ToString("hh':'mm"));
                        string dayofweek = string.Format("{0} {1}-{2}", GetDayString(day), start.ToString("hh':'mm"), end.ToString("hh':'mm"));
                        Companylist company = new Companylist()
                        {
                            CompanyID = id,
                            CompanyNam = dayofweek
                        };
                        model.Companylist.Add(company);

                        companyHours.Add(new CompanyHour()
                            {
                                DayOfWeek = day,
                                FromHour = start,
                                ToHour = end,
                                IsDaily = day == (int)Types.Day.Isdaily
                            });
                    }
                }

                string[] stateCity = model.City.Split(',');
                string strAddress = string.Empty;
                if (stateCity[0] != "")
                    strAddress = strAddress + stateCity[0];
                if (stateCity[1] != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + ", " + stateCity[1];
                    else
                        strAddress = model.State;
                if (model.Zip != "")
                    if (!string.IsNullOrEmpty(strAddress))
                        strAddress = strAddress + " " + model.Zip;
                    else
                        strAddress = model.Zip;

                GeoClass.Coordinate coordinate;
                if (!string.IsNullOrEmpty(strAddress))
                    coordinate = GeoClass.GetCoordinates(strAddress);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);
                if (_companyProfileService.CheckCompanyName(model.Name, model.Street1))
                {
                    ModelState.AddModelError("ExistCompanyName", "company name have been used.");

                    List<Service> lstService1 = _companyProfileService.GetListService();
                    foreach (Service item in lstService1)
                    {
                        model.CategoryCompany.Add(new SelectListItem()
                        {
                            Text = item.ServiceName,
                            Value = item.ServiceID.ToString()
                        });
                    }

                    return View(model);
                }

                ProfileCompany profileCompany = new ProfileCompany
                {

                    CompanyTypeID = (int)Types.CompanyType.KuyamInstantBook,
                    CompanyStatusID = (int)Types.CompanyStatus.Pending,
                    Name = model.Name,
                    Street1 = model.Street1,
                    Street2 = model.Street2,
                    City = stateCity[0],
                    State = UtilityHelper.TruncateData(stateCity[1], 4),
                    Zip = model.Zip,
                    Url = model.Url,
                    Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone),
                    Desc = model.Desc,
                    ApptAutoConfirm = true,
                    ApptDefaultSlotDuration = 0,
                    ApptDefaultPeoplePerSlot = 0,
                    Latitude = (double)coordinate.Latitude,
                    Longitude = (double)coordinate.Longitude,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow

                };

                Profile profile = new Profile
                {
                    CustID = MySession.CustID,
                    PrivacyTypeID = (int)Types.PrivacyType.Private,

                    RelationshipTypeID = (int)Types.RelationshipType.Company,
                    Name = model.Name,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    ProfileCompany = profileCompany
                };
                _companyProfileService.InsertProfile(profile);
                string slug = string.Format("{0} {1} {2} {3}", profileCompany.Name, profileCompany.City, profileCompany.State, profileCompany.Street1);
                _seoFriendlyUrlService.SaveSlug(profile.ProfileID, slug, "company");
                if (model.Listhour != null && profile.ProfileID != 0)
                {
                    foreach (CompanyHour companyHour in companyHours)
                    {
                        companyHour.ProfileCompanyID = profile.ProfileID;
                    }

                    _companyProfileService.InsertCompanyHour(companyHours);
                }

                if (model.Categories != null && profile.ProfileID != 0)
                {
                    List<ServiceCompany> lstServiceCompany = new List<ServiceCompany>();
                    foreach (Category item in model.Categories)
                    {
                        int serviceId = 0;
                        int.TryParse(item.CategoryID, out serviceId);
                        ServiceCompany serviceCompany = new ServiceCompany
                        {
                            ProfileID = profile.ProfileID,
                            ServiceID = serviceId,
                            Status = 0,
                            Created = DateTime.Now,
                            Modified = DateTime.Now

                        };
                        lstServiceCompany.Add(serviceCompany);
                    }
                    _companyProfileService.InsertServiceCompany(lstServiceCompany);
                }
                string invite = _companyProfileService.AddInviteCode(MySession.Cust.CustID, MySession.Username);

                //Send email underreview
                try
                {
                    string template = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/underreview.cshtml")))
                    {
                        template = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = MySession.Cust.FirstName.ToString(),
                        Email = MySession.Username.ToString(),
                        CompanyName = model.Name,
                        Host = EmailHelper.GetStoreHost()
                    };
                    string templateResult = RazorEngine.Razor.Parse(template, myObject);
                    EmailHelper.SendEmailCompanyUnderReviewVerificationCode(MySession.Username, templateResult);
                }
                catch
                {
                    throw;
                }
                //MySession.ProfileID = profile.ProfileID;
                return RedirectToAction("Image", "CompanySetup", new { companyId = profile.ProfileID });
                //return RedirectToAction("SetupImage", new { companyId = profile.ProfileID });
            }

            List<Service> lstService = _companyProfileService.GetListService();
            foreach (Service item in lstService)
            {
                model.CategoryCompany.Add(new SelectListItem()
                {
                    Text = item.ServiceName,
                    Value = item.ServiceID.ToString()
                });
            }
            return View(model);
        }

        [Authorize]
        public ActionResult SetupVideo()
        {
            int profileID = this.ProfileId;
            CompanySetupModel model = new CompanySetupModel();
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            var profileCompany = _profileCompanyService.GetByProfileId(profileID != 0 ? profileID : MySession.ProfileID);
            if (profileCompany == null)
            {
                profileCompany = new ProfileCompany();
            }
            model.Youtubelink = profileCompany.YoutubeLink;
            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }
            model.ProfileID = profile.ProfileID;
            model.CompanyStatusID = profile.ProfileCompany.CompanyStatusID;
            List<CompanyMedia> companyMediaOld = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID).Where(m => m.IsVideo == true).ToList();
            if (companyMediaOld.Count != 0)
            {
                return RedirectToAction("SetupVideoEdit", new { companyId = profile.ProfileID });
            }
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult SetupVideo(CompanySetupModel model)
        {
            int profileID = model.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }
            List<CompanyMedia> profileMedium = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID);

            if (profileMedium.Any(m => m.IsVideo))
            {
                return RedirectToAction("SetupVideoEdit", new { companyId = profile.ProfileID });
            }
            List<CompanyMedia> lstcompanyMedia = new List<CompanyMedia>();
            if (model.VideoID != 0)
            {
                CompanyMedia companyMediavideo = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediavideo.MediaID = model.VideoID;
                companyMediavideo.IsVideo = true;
                lstcompanyMedia.Add(companyMediavideo);
            }

            string strflash = KalturaHelper.GetEmbedVideothumbnail(model.VideoData, model.VideoID, 216, 302);
            model.FlashData = MvcHtmlString.Create(strflash);
            //Check video

            //List<CompanyMedia> profileMedium = _companyProfileService.GetCompanyMediaByProfileID(profiles.ProfileID);

            //if (!profileMedium.Any(m => m.IsVideo)){
            //    _companyProfileService.InsertCompanyMedia(lstcompanyMedia);                
            //}
            _companyProfileService.InsertCompanyMedia(lstcompanyMedia);
            if (profile.ProfileCompany != null)
            {
                profile.ProfileCompany.YoutubeLink = model.Youtubelink;
            }
            _companyProfileService.UpdateProfile(profile);
            if (profile.ProfileCompany.CompanyStatusID == (int)Types.CompanyStatus.Pending)
            {
                return RedirectToAction("postsetup", new { companyId = profile.ProfileID });
            }
            //return View(model);
            return RedirectToAction("SetupVideo", new { companyId = profile.ProfileID });
        }

        [Authorize]
        public ActionResult SetupVideoEdit()
        {
            int profileId = this.ProfileId;
            CompanySetupModel model = new CompanySetupModel();
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);

            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }
            model.ProfileID = profile.ProfileID;
            model.CompanyStatusID = profile.ProfileCompany.CompanyStatusID;
            List<CompanyMedia> companyMediaOld = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID).Where(m => m.IsVideo == true).ToList();

            if (companyMediaOld == null || companyMediaOld.Count == 0)
            {
                return RedirectToAction("SetupVideo", new { companyId = profile.ProfileID });
            }

            if (profile.ProfileCompany != null)
            {
                model.Youtubelink = profile.ProfileCompany.YoutubeLink;
            }

            List<CompanyMedia> companyMedia = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID);
            //List<CompanyMedia> lstcompanyMediaNew = new List<CompanyMedia>();
            //if (model.VideoID != 0)
            //{
            //    CompanyMedia companyMediavideo = new CompanyMedia()
            //    {
            //        ProfileID = profile.ProfileID
            //    };
            //    companyMediavideo.MediaID = model.VideoID;
            //    companyMediavideo.IsVideo = true;

            //    _companyProfileService.UpdateCompanyVideo(companyMediavideo, companyMediaOld.Where(x => x.IsVideo).FirstOrDefault());
            //}


            foreach (CompanyMedia item in companyMedia)
            {
                if (item.IsVideo)
                {
                    model.VideoID = item.MediaID;
                    model.VideoData = item.Medium.LocationData;
                }
            }
            string strflash = KalturaHelper.GetEmbedVideothumbnail(model.VideoData, model.VideoID, 216, 302, UtiHelper.UseSsl);
            model.FlashData = MvcHtmlString.Create(strflash);

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult SetupVideoEdit(CompanySetupModel model)
        {
            int profileId = model.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            List<CompanyMedia> companyMediaOld = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID);
            List<CompanyMedia> lstcompanyMediaNew = new List<CompanyMedia>();
            if (model.VideoID != 0)
            {
                CompanyMedia companyMediavideo = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediavideo.MediaID = model.VideoID;
                companyMediavideo.IsVideo = true;

                _companyProfileService.UpdateCompanyVideo(companyMediavideo, companyMediaOld.Where(x => x.IsVideo).FirstOrDefault());
            }

            string strflash = KalturaHelper.GetEmbedVideothumbnail(model.VideoData, model.VideoID, 216, 302);
            model.FlashData = MvcHtmlString.Create(strflash);
            _companyProfileService.InsertCompanyMedia(lstcompanyMediaNew);
            if (profile.ProfileCompany != null)
            {
                profile.ProfileCompany.YoutubeLink = model.Youtubelink;
            }
            _companyProfileService.UpdateProfile(profile);
            return View(model);
        }

        [Authorize]
        public ActionResult SetupImage()
        {
            int profileID = this.ProfileId;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }

            List<CompanyMedia> companyMedia = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID).Where(x => x.IsVideo != true).ToList();
            if (companyMedia.Count > 0)
            {
                return RedirectToAction("SetupImageedit", new { companyId = profile.ProfileID });
            }
            CompanySetupModel model = new CompanySetupModel();
            model.ProfileID = profile.ProfileID;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SetupImage(CompanySetupModel model)
        {
            int profileID = model.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);
            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }

            List<CompanyMedia> lstcompanyMedia = new List<CompanyMedia>();
            if (model.LogoID != 0)
            {
                CompanyMedia companyMedialogo = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMedialogo.MediaID = model.LogoID;
                companyMedialogo.IsLogo = true;
                lstcompanyMedia.Add(companyMedialogo);
            }

            if (model.PhotoID != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID;
                companyMediaphoto.IsDefault = true;
                lstcompanyMedia.Add(companyMediaphoto);
            }
            if (model.PhotoID2 != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID2;
                companyMediaphoto.IsBanner = true;
                lstcompanyMedia.Add(companyMediaphoto);
            }
            if (model.PhotoID3 != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID3;
                companyMediaphoto.IsBanner = true;
                lstcompanyMedia.Add(companyMediaphoto);
            }
            _companyProfileService.InsertCompanyMedia(lstcompanyMedia);

            _companyProfileService.UpdateProfile(profile);
            //return View(model);
            return RedirectToAction("SetupVideo", new { companyId = profile.ProfileID });
        }

        [Authorize]
        public ActionResult SetupImageEdit()
        {
            int profileID = this.ProfileId;
            CompanySetupModel model = new CompanySetupModel();
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);

            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }

            model.ProfileID = profile.ProfileID;

            List<CompanyMedia> companyMedia = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID).Where(x => x.IsVideo != true).ToList();

            if (companyMedia == null || companyMedia.Count <= 0)
            {
                return RedirectToAction("SetupImage", new { companyId = profile.ProfileID });
            }
            int flagIndexPhoto = 1;
            foreach (CompanyMedia item in companyMedia)
            {
                if (item.IsLogo)
                {
                    model.LogoID = item.MediaID;
                    model.LogoData = item.Medium.LocationData;
                }
                else if (item.IsDefault)
                {
                    model.PhotoID = item.MediaID;
                    model.PhotoData = item.Medium.LocationData;
                }
                else if (item.IsBanner && flagIndexPhoto == 1)
                {
                    model.PhotoID2 = item.MediaID;
                    model.PhotoData2 = item.Medium.LocationData;

                    flagIndexPhoto = 2;
                }
                else if (item.IsBanner && flagIndexPhoto != 1)
                {
                    model.PhotoID3 = item.MediaID;
                    model.PhotoData3 = item.Medium.LocationData;
                }

            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SetupImageEdit(CompanySetupModel model)
        {
            int profileID = model.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileID != 0 ? profileID : MySession.ProfileID);

            if (profile == null)
            {
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            }
            List<CompanyMedia> companyMediaOld = _companyProfileService.GetCompanyMediaByProfileID(profile.ProfileID).Where(x => x.IsVideo != true).ToList();

            if (companyMediaOld.Count < 0 || companyMediaOld == null)
            {

                return RedirectToAction("SetupImage", new { companyId = profile.ProfileID });
            }
            List<CompanyMedia> lstcompanyMediaNew = new List<CompanyMedia>();
            if (model.LogoID != 0)
            {
                CompanyMedia companyMedialogo = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMedialogo.MediaID = model.LogoID;
                companyMedialogo.IsLogo = true;
                lstcompanyMediaNew.Add(companyMedialogo);
            }

            if (model.PhotoID != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID;
                companyMediaphoto.IsDefault = true;
                lstcompanyMediaNew.Add(companyMediaphoto);
            }
            if (model.PhotoID2 != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID2;
                companyMediaphoto.IsBanner = true;
                lstcompanyMediaNew.Add(companyMediaphoto);
            }
            if (model.PhotoID3 != 0)
            {
                CompanyMedia companyMediaphoto = new CompanyMedia()
                {
                    ProfileID = profile.ProfileID
                };
                companyMediaphoto.MediaID = model.PhotoID3;
                companyMediaphoto.IsBanner = true;
                lstcompanyMediaNew.Add(companyMediaphoto);
            }
            _companyProfileService.UpdateCompanyMedia(lstcompanyMediaNew, companyMediaOld);

            return RedirectToAction("SetupVideoEdit", new { companyId = profile.ProfileID });
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteMediaById(string id)
        {

            bool result = false;
            int mediaId = 0;
            int.TryParse(id, out mediaId);

            result = _companyProfileService.DeleteMediaById(mediaId);

            return Json(result);
        }

        #endregion

        #region Employees New

        [Authorize]
        public ActionResult Employee(int? id)
        {
            int profileId = this.ProfileId;
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");

            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");

            int profileID = profiles.ProfileID;
            List<ServiceCompany> scList = new List<ServiceCompany>();
            scList = DAL.GetCompanySevicesByProfileID(profileID);
            ViewBag.CompanyServices = scList;
            ViewBag.CompanyEmployees = DAL.GetCompanyEmployeesByProfileID(profileID);
            ViewBag.ProfileCompany = profiles;
            int verified = (int)Kuyam.Database.Types.FlagInvite.Unverified;
            if (id.HasValue && id.Value > 0)
            {
                CompanyEmployee employee = new CompanyEmployee();
                employee = ProfileCompany.GetCompanyEmployee(id.Value);
                if (employee != null)
                {
                    ViewBag.Employee = employee;
                    List<EmployeeService> esList = DAL.GetEmployeeServicesFromEmployeeID(id.Value);
                    string scListIDs = string.Empty;
                    if (esList != null && esList.Count > 0)
                    {
                        foreach (EmployeeService service in esList)
                        {
                            scListIDs = scListIDs + service.ServiceCompanyID.ToString() + ",";
                        }
                    }
                    ViewBag.StringSCListIDs = scListIDs;
                    if (profiles.Cust != null)
                    {
                        Invite i = DAL.GetInviteForSMSVerify(employee.Phone, profiles.Cust.Email);
                        if (i != null && i.Active == true)
                        {
                            verified = (int)Kuyam.Database.Types.FlagInvite.Verified;
                        }
                    }
                }

            }
            ViewBag.companyId = profileID;
            ViewBag.Verified = verified;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditEmployeeInfoNew(string employeeID, string employeeName, string email, string phone, string stringServiceCompanyIDs, string employeeDefault, string paypal, int profileId = 0)
        {
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            int profileID = profile.ProfileID;
            int empID = 0;
            if (!int.TryParse(employeeID, out empID))
            {
                return RedirectToAction("CompanyEmployee");
            }
            bool isDefault = false;
            bool.TryParse(employeeDefault, out isDefault);

            if (isDefault)
            {
                _companyProfileService.UpdateDefaultEmploee(profile.ProfileID);
            }
            CompanyEmployee employee = ProfileCompany.GetCompanyEmployee(empID);
            if (employee != null)
            {
                employee.EmployeeName = employeeName;
                employee.Email = email;
                employee.ProfileCompanyID = profileID;
                employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
                employee.PaymentAccount = paypal;
                employee.IsDefault = isDefault;
                DAL.UpdateCompanyEmployeeInfo(employee);
                _classService.DeleteEmployeeServicesNonClassByEmployeeID(employee.EmployeeID);
            }

            bool result = false;

            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        Kuyam.Database.EmployeeService service = new Kuyam.Database.EmployeeService();
                        service.CompanyEmployeeID = empID;
                        service.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = _classService.AddEmployeeServicesNonClass(service);
                    }
                }
            }

            int empl = int.Parse(employeeID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddEmployeeInfoNew(string employeeName, string email, string phone, string stringServiceCompanyIDs, string employeeDefault, string paypal, int profileId = 0)
        {

            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            int profileID = profile.ProfileID;

            bool isDefault = false;
            bool.TryParse(employeeDefault, out isDefault);

            if (isDefault)
            {
                _companyProfileService.UpdateDefaultEmploee(profile.ProfileID);
            }

            CompanyEmployee employee = new CompanyEmployee();
            employee.EmployeeName = employeeName;
            employee.Email = email;
            employee.ProfileCompanyID = profileID;
            employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
            employee.PaymentAccount = paypal;
            employee.IsDefault = isDefault;

            int employeeID = DAL.AddEmployee(employee);
            bool result = false;

            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        Kuyam.Database.EmployeeService service = new Kuyam.Database.EmployeeService();
                        service.CompanyEmployeeID = employeeID;
                        service.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = DAL.AddEmployeeService(service);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Packages

        [Authorize]
        public ActionResult CompanyPackages()
        {
            Profile profile = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });

            ViewBag.CompanyPackages = _companyProfileService.GetNonUsingPackageByProfileID(profile.ProfileID, Types.CompanyPackageStatus.Active);
            var listInactive = _companyProfileService.GetNonUsingPackageByProfileID(profile.ProfileID, Types.CompanyPackageStatus.Inactive);
            ViewBag.CountInactive = listInactive != null ? listInactive.Count : 0;
            ViewBag.ImageList = string.Join(";", _companyProfileService.GetCompanyImagesbyProfileID(profile.ProfileID));
            ViewBag.ServiceList = _companyProfileService.GetServiceCompanyByProfileID(profile.ProfileID,
                                                                                      (int)
                                                                                      Types.ServiceCompanyStatus.Active);
            return View();
        }

        [Authorize]
        public ActionResult CompanyPackagesInactive()
        {
            Profile profile = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });

            ViewBag.CompanyPackages = _companyProfileService.GetNonUsingPackageByProfileID(profile.ProfileID, Types.CompanyPackageStatus.Inactive);
            var listActive = _companyProfileService.GetNonUsingPackageByProfileID(profile.ProfileID, Types.CompanyPackageStatus.Active);
            ViewBag.CountActive = listActive != null ? listActive.Count : 0;
            ViewBag.ImageList = string.Join(";", _companyProfileService.GetCompanyImagesbyProfileID(profile.ProfileID));
            ViewBag.ServiceList = _companyProfileService.GetServiceCompanyByProfileID(profile.ProfileID,
                                                                                      (int)
                                                                                      Types.ServiceCompanyStatus.Active);
            return View();
        }

        //var parameters = { name: name, price: price, quantity: quantity, duration: duration, services: services, imageUrl: imageUrl };
        [Authorize]
        [HttpPost]
        public ActionResult AdminCompanyCreatePackage(string id, string name, string price, string quantity, string duration, string services, string imageUrl, int profileId = 0)
        {
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            decimal packPrice = decimal.Parse(price);
            int packQuantity = Int32.Parse(quantity);
            int packDuration = Int32.Parse(duration);
            List<int> packSerivces = new List<int>();
            foreach (var str in services.Split(';'))
            {
                if (!packSerivces.Contains(Int32.Parse(str)))
                    packSerivces.Add(Int32.Parse(str));
            }
            if (!string.IsNullOrEmpty(id))
            {
                if (id.Equals("0"))
                    _companyProfileService.CreateCompanyAdminPackage(profile.ProfileID, name, packPrice, packQuantity,
                                                                     packDuration, packSerivces, imageUrl);
                else
                {
                    int packageId;
                    if (Int32.TryParse(id, out packageId))
                    {
                        _companyProfileService.UpdateCompanyAdminPackage(profile.ProfileID, packageId, name, packPrice,
                                                                         packQuantity,
                                                                         packDuration, packSerivces, imageUrl);
                    }
                }

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AdminCompanyDeletePackage(string id, int profileId = 0)
        {
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (!string.IsNullOrEmpty(id))
            {
                int packageId;
                if (Int32.TryParse(id, out packageId) && packageId > 0)
                {
                    _companyProfileService.DeleteCompanyAdminPackage(packageId);
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AdminCompanyMakeInactive(int packageId)
        {
            _companyProfileService.AdminCompanyMakeInactive(packageId);

            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion Packages

        #region RegularcClients

        [Authorize]
        public ActionResult CompanyRegularClients()
        {

            ViewBag.RegularClientList = _custService.GetRegularClientListByCompanyProfileId(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult CompanyRegularClients(string key, string searchType, int profileId = 0)
        {
            int keySearchType = 0;
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });

            int.TryParse(searchType, out keySearchType);
            List<RegularClient> RegularClients = _custService.GetRegularClientListByCompanyProfileId(profile.ProfileID, key, keySearchType);

            if (searchType == "4")
            {

                List<RegularClient> RegularClientResult = new List<RegularClient>();
                foreach (RegularClient RegularClient in RegularClients)
                {
                    if (!_custService.CheckUserIsActiveByEmail(RegularClient.Email) && !_custService.CheckCustInAppoinmentsByCustId(RegularClient.Email))
                    {
                        RegularClientResult.Add(RegularClient);
                    }
                }
                ViewBag.RegularClientList = RegularClientResult;
            }
            else
            {
                ViewBag.RegularClientList = RegularClients;
            }

            return PartialView("_RegularClientsResults");

        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteRegularClient(int regularClientId)
        {
            bool result = _custService.DeleteRegularClient(regularClientId);
            return Json(result);
        }

        [Authorize]
        [HttpPost]
        public ActionResult InsertRegularClient(string regularClients, int profileId = 0)
        {
            bool result = false;
            ProfileCompany company = _companyProfileService.GetProfileCompanyByID(profileId != 0 ? profileId : MySession.ProfileID);
            List<RegularClient> oRegularClients = new List<RegularClient>();
            try
            {
                foreach (var o in regularClients.Split(';'))
                {
                    if (string.IsNullOrEmpty(o))
                        continue;
                    string[] arr = o.Split(',');
                    RegularClient oRegularClient = new RegularClient
                    {
                        FirstName = arr[0],
                        LastName = arr[1],
                        Email = arr[2],
                        CreatedDate = DateTime.UtcNow,
                        CompanyProfileId = company.ProfileID,
                        Status = true

                    };
                    if (!_custService.RegularClientCheckExistEmail(oRegularClient.Email, (profileId != 0 ? profileId : MySession.ProfileID)))
                    {
                        //Send email regularclient
                        try
                        {
                            string template = string.Empty;
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/surpriseregclients.cshtml")))
                            {
                                template = reader.ReadToEnd();
                            }
                            dynamic myObject = new
                            {
                                UserName = oRegularClient.FirstName,
                                Email = oRegularClient.Email,
                                CompanyName = company.Name,
                                CompanyLink = String.Format("{0}companyprofile/availability/{1}", EmailHelper.GetStoreHost(), (profileId != 0 ? profileId : MySession.ProfileID)),
                                Host = EmailHelper.GetStoreHost(),
                                Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow)
                            };
                            string templateResult = RazorEngine.Razor.Parse(template, myObject);
                            //EmailHelper.SendEmailCompanyUnderReviewVerificationCode(MySession.Username, templateResult);
                            EmailHelper.SendEmailRegularClient(MySession.Username, oRegularClient.Email, templateResult);
                        }
                        catch (Exception ex)
                        {
                            //Todo: Handle Exception Occur
                            LogHelper.Error("InsertRegularClient fail:", ex);
                        }
                        oRegularClients.Add(oRegularClient);
                    }
                }
                if (oRegularClients != null && oRegularClients.Count > 0)
                {
                    result = _custService.InsertRegularClient(oRegularClients);
                }
            }
            catch
            {
                result = false;
            }

            return Json(result);
        }

        [Authorize]
        [HttpGet]
        public ActionResult RegularClientInfo(string regularClientId)
        {
            int id = 0;
            int.TryParse(regularClientId, out id);

            RegularClient regularClient = _custService.GetRegularClientByID(id);

            cRegularClient regular = new cRegularClient
            {
                RegularClientId = regularClient.RegularClientId,
                Email = regularClient.Email,
                FirstName = regularClient.FirstName,
                LastName = regularClient.LastName
            };
            return Json(regular, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RegularClientDelete(string regularClientId)
        {

            bool result = false;
            if (!string.IsNullOrEmpty(regularClientId))
            {
                int id = 0;
                int.TryParse(regularClientId, out id);
                result = _custService.DeleteRegularClient(id);
            }
            return Json(result);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RegularClientUpdate(string regularClientId, string firstName, string lastName, string email)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(regularClientId))
            {
                int id = 0;
                int.TryParse(regularClientId, out id);

                RegularClient regularClient = _custService.GetRegularClientByID(id);
                if (regularClient != null)
                {
                    regularClient.FirstName = firstName;
                    regularClient.LastName = lastName;
                    regularClient.Email = email;

                    result = _custService.UpdateRegularClient(regularClient);
                }

            }
            return Json(result);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RegularClientCheckExistEmail(string email, int profileId = 0)
        {
            return Json(_custService.RegularClientCheckExistEmail(email, (profileId != 0 ? profileId : MySession.ProfileID)));
        }

        #endregion

        #region promo Code

        [Authorize]
        public ActionResult CompanyActiveDiscountCode()
        {
            Profile profile = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            ViewBag.ServiceList = _companyProfileService.GetServiceCompanyByProfileID(profile.ProfileID, (int)Types.ServiceCompanyStatus.Active);
            ViewBag.RegularClients = _companyProfileService.GetRegularClientsbyProfileId(profile.ProfileID);
            ViewBag.ProfileCompany = _companyProfileService.GetProfileCompanyByID(profile.ProfileID);
            List<CompanyDiscountExt> lstDiscount = _companyProfileService.GetDiscountCompanyByProfileID(profile.ProfileID, (int)
                                                                                                           Types.
                                                                                                               DiscountStatus
                                                                                                               .Active);
            ViewBag.ActiveDiscountList =
                lstDiscount.Where(
                    d =>
                    d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow &&
                    (d.NumberOfUsage < d.Quantity || d.Quantity == -1)).ToList();

            ViewBag.ActiveNumber = ViewBag.ActiveDiscountList.Count;
            ViewBag.UpcomingNumber = lstDiscount.Count(d => d.StartDate > DateTime.UtcNow);
            ViewBag.RemainNumber = lstDiscount.Count - ViewBag.ActiveNumber - ViewBag.UpcomingNumber;
            return View();
        }

        [Authorize]
        public ActionResult CompanyUpcomingDiscountCode()
        {
            Profile profile = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = profile.ProfileID });
            ViewBag.ServiceList = _companyProfileService.GetServiceCompanyByProfileID(profile.ProfileID,
                                                                                      (int)
                                                                                      Types.ServiceCompanyStatus.Active);
            ViewBag.RegularClients = _companyProfileService.GetRegularClientsbyProfileId(profile.ProfileID);
            ViewBag.ProfileCompany = _companyProfileService.GetProfileCompanyByID(profile.ProfileID);
            List<CompanyDiscountExt> lstDiscount = _companyProfileService.GetDiscountCompanyByProfileID(profile.ProfileID, (int)
                                                                                                           Types.
                                                                                                               DiscountStatus
                                                                                                               .Active);
            ViewBag.UpcomingDiscountList =
                lstDiscount.Where(d => d.StartDate > DateTime.UtcNow).ToList();

            ViewBag.UpcomingNumber = ViewBag.UpcomingDiscountList.Count;
            ViewBag.ActiveNumber =
                lstDiscount.Count(
                    d =>
                    d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow &&
                    (d.NumberOfUsage < d.Quantity || d.Quantity == -1));
            ViewBag.RemainNumber = lstDiscount.Count - ViewBag.ActiveNumber - ViewBag.UpcomingNumber;
            return View();
        }

        [Authorize]
        public ActionResult CompanyExpiredDiscountCode()
        {
            Profile profile = _companyProfileService.GetProfileByID(this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            ViewBag.ServiceList = _companyProfileService.GetServiceCompanyByProfileID(profile.ProfileID,
                                                                                      (int)
                                                                                      Types.ServiceCompanyStatus.Active);
            ViewBag.RegularClients = _companyProfileService.GetRegularClientsbyProfileId(profile.ProfileID);
            ViewBag.ProfileCompany = _companyProfileService.GetProfileCompanyByID(profile.ProfileID);
            List<CompanyDiscountExt> lstDiscount = _companyProfileService.GetDiscountCompanyByProfileID(profile.ProfileID, (int)
                                                                                                           Types.
                                                                                                               DiscountStatus
                                                                                                               .Active);

            ViewBag.ActiveNumber =
                lstDiscount.Count(
                    d =>
                    d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow &&
                    (d.NumberOfUsage < d.Quantity || d.Quantity == -1));
            ViewBag.UpcomingNumber = lstDiscount.Count(d => d.StartDate > DateTime.UtcNow);
            ViewBag.RemainNumber = lstDiscount.Count - ViewBag.ActiveNumber - ViewBag.UpcomingNumber;


            ViewBag.ExpiredDiscountList =
                lstDiscount.Where(d => !(d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow && (d.NumberOfUsage < d.Quantity || d.Quantity == -1))
                && !(d.StartDate > DateTime.UtcNow)).ToList();

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteDiscountCompany(string id)
        {
            int discountId = 0;
            if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out discountId) && discountId > 0)
            {
                _companyProfileService.DeleteDiscountCompany(discountId);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //AdminCompanyCreateDiscountCode
        //var parameters = {id: id,name: name,code: code,isamount: isamount,amount: amount,ispercent: ispercent,percent: percent,quantity: quantity,serviceid: serviceid};
        [Authorize]
        [HttpPost]
        public ActionResult AdminCompanyCreateDiscountCode(string id, string name, string code, string isamount, string amount, string ispercent, string percent,
            string quantity, string serviceid, string startdate, string starttime, string enddate, string endtime, int profileId = 0)
        {
            int discountId = 0;
            bool bamount = false;
            decimal damount = 0;
            bool bpercent = false;
            decimal dpercent = 0;
            int iquantity = 0;
            int iserviceId = 0;
            DateTime dStartDate = DateTime.UtcNow;
            DateTime dEndDate = DateTime.MaxValue;
            TimeSpan tStartTime;
            TimeSpan tEndTime = new TimeSpan(0, 0, 0, 0);
            DateTime t = DateTime.ParseExact(starttime, "hh:mm tt", CultureInfo.InvariantCulture);
            tStartTime = t.TimeOfDay;
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);


            if (string.IsNullOrEmpty(id) || !Int32.TryParse(id, out discountId) || discountId <= 0)
            {
                if (string.IsNullOrEmpty(code) || _companyProfileService.IsExistDiscountCode(profile.ProfileID, code))
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(code) || _companyProfileService.IsExistDiscountCode(profile.ProfileID, discountId, code))
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                if (_companyProfileService.IsUsedDiscount(discountId))
                {
                    return Json("isused", JsonRequestBehavior.AllowGet);
                }
            }

            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime.TryParse(enddate, out dEndDate);
                if (!string.IsNullOrEmpty(endtime))
                {
                    t = DateTime.ParseExact(endtime, "hh:mm tt", CultureInfo.InvariantCulture);
                    tEndTime = t.TimeOfDay;
                }
            }

            if (!string.IsNullOrEmpty(id))
            {
                if (Int32.TryParse(id, out discountId) && discountId > 0)
                {
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && bool.TryParse(isamount, out bamount) && decimal.TryParse(amount, out damount)
                        && bool.TryParse(ispercent, out bpercent) && decimal.TryParse(percent, out dpercent) && int.TryParse(quantity, out iquantity) && int.TryParse(serviceid, out iserviceId)
                        && (bpercent != bamount) && DateTime.TryParse(startdate, out dStartDate))

                        _companyProfileService.UpdateCompanyDiscount(profile.ProfileID, discountId, name, code, bamount,
                                                                     damount,
                                                                     bpercent, dpercent,
                                                                     iquantity, iserviceId, dStartDate, tStartTime,
                                                                     dEndDate, tEndTime);
                }
                else
                {
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && bool.TryParse(isamount, out bamount) && decimal.TryParse(amount, out damount)
                        && bool.TryParse(ispercent, out bpercent) && decimal.TryParse(percent, out dpercent) && int.TryParse(quantity, out iquantity) && int.TryParse(serviceid, out iserviceId)
                        && (bpercent != bamount) && DateTime.TryParse(startdate, out dStartDate))

                        _companyProfileService.CreateCompanyDiscount(profile.ProfileID, name, code, bamount, damount,
                                                                     bpercent, dpercent,
                                                                     iquantity, iserviceId, dStartDate, tStartTime,
                                                                     dEndDate, tEndTime);
                }
            }
            return Json("true", JsonRequestBehavior.AllowGet);
        }

        //GetCompanyRegularClient
        [Authorize]
        [HttpPost]
        public ActionResult GetCompanyRegularClient(string id)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DiscountSendEmail(string id, string message, string clients, string durationandservicename, int profileId = 0)
        {
            bool result = false;
            int discountId = 0;
            List<int> clientIds = new List<int>();
            foreach (string str in clients.Split(';'))
            {
                int outId = 0;
                if (Int32.TryParse(str, out outId))
                {
                    clientIds.Add(outId);
                    Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
                    if (profile == null)
                        return RedirectToAction("SetupBasic");
                    RegularClient regularClient = _custService.GetRegularClientByID(outId);
                    if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out discountId) && discountId > 0)
                    {
                        result = SendEmailTemplateDiscount(discountId, profile.ProfileCompany.Email, message, outId, durationandservicename);
                        if (regularClient != null)
                        {
                            Cust user = _custService.GetUsersByUserID(regularClient.Email);

                            if (user != null)
                            {
                                DiscountInvite userDiscont = new DiscountInvite
                                {
                                    DiscountId = discountId,
                                    NumberofUsage = 0,
                                    CustId = user.CustID,
                                    DateUsage = DateTime.UtcNow
                                };

                                _companyProfileService.InsertUserDiscountInvite(userDiscont);
                            }
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private bool SendEmailTemplateDiscount(int id, string emailFrom, string message, int clientId, string durationandservicename)
        {
            bool result = false;

            string companyId = string.Empty;
            string companyName = string.Empty;
            string email = string.Empty;
            string code = string.Empty;
            string start = string.Empty;
            string until = string.Empty;
            string discountTest = string.Empty;
            string userName = string.Empty;
            string serviceName = string.Empty;
            string promoName = string.Empty;

            Discount discount = _companyProfileService.GetDiscountCodeByID(id);
            RegularClient regularClient = _custService.GetRegularClientByID(clientId);

            if (regularClient != null)
            {
                userName = regularClient.FirstName;
                email = regularClient.Email;
            }
            if (discount != null)
            {
                promoName = discount.Name;
                if (discount.ApplyToAllServices)
                {
                    serviceName = string.Format("all services");
                }
                else
                {
                    serviceName = durationandservicename;
                }
                code = discount.Code;
                if (discount.Percent > 0)
                {
                    discountTest = string.Format("{0}%", Math.Round(discount.Percent, 2).ToString("0,0.00"));
                }
                else
                {
                    discountTest = string.Format("{0}$", Math.Round(discount.Amount, 2).ToString("0,0.00"));
                }
                start = string.Format("starting {0} at {1}", String.Format("{0:M/d/yyyy}", discount.StartDate), String.Format("{0:t}", discount.StartDate));
                until = discount.EndDate.Date.Year == DateTime.MaxValue.Date.Year ? "no expiry date" : string.Format("until {0} at {1}", String.Format("{0:M/d/yyyy}", discount.EndDate), String.Format("{0:t}", discount.EndDate));
                if (discount.ProfileCompany != null)
                {
                    companyId = discount.ProfileCompany.ProfileID.ToString(CultureInfo.InvariantCulture);
                    companyName = discount.ProfileCompany.Name;
                }
            }
            //Email template
            try
            {

                string template = string.Empty;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/discountcode.cshtml")))
                {
                    template = reader.ReadToEnd();
                }
                dynamic myObject = new
                {
                    Email = email,
                    CompanyName = companyName,
                    UserName = userName,
                    Start = start,
                    Until = until,
                    Code = code,
                    DiscountTest = discountTest,
                    CompanyId = companyId,
                    EmailFrom = emailFrom,
                    Message = message,
                    ServiceName = serviceName,
                    PromoName = promoName
                };

                string templateResult = RazorEngine.Razor.Parse(template, myObject);

                EmailHelper.SendEmailDiscountCode(emailFrom, email, templateResult);

                DiscountRegularClient discountRegularClient = new DiscountRegularClient
                {
                    DiscountId = id,
                    RegularClientId = clientId,
                    Status = 1
                };
                //Save into DiscountRegularClient table
                bool resultRegularClient = _custService.InsertDiscountRegularClient(discountRegularClient);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }


            return result;
        }

        [HttpPost]
        public ActionResult LoadRegularClient(string id, int profileId = 0)
        {
            int discountId = 0;
            var result = string.Empty;
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return RedirectToAction("SetupBasic");
            if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out discountId) && discountId > 0)
            {
                result = _companyProfileService.LoadRegularClient(discountId);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion Promo Code

        #region "generate company code"

        public ActionResult GenerateCodeCompany(int? companyId)
        {
            if (!companyId.HasValue)
                return RedirectToAction("Error404", "Error");
            int profileID = companyId.Value;
            Profile profile = _companyProfileService.GetProfileByID(profileID);

            if (profile == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            CompanySetupModel model = new CompanySetupModel();
            model.ProfileID = profileID;
            var companyAppointmentService = (CompanyAppointmentController)DependencyResolver.Current.GetService(typeof(CompanyAppointmentController));
            var vm = companyAppointmentService.GetCompanyProfileTimeSlots(profileID);
            //vm.Company = profile.ProfileCompany;
            vm.TimeSlots.IsRederect = true;
            var htmlString = this.RenderPartialViewToString("_CompanyProfileTimeSlotsTemplate1", vm);
            ViewBag.htmlItem = htmlString;
            //ViewBag.fram1 = "<ifram></ifram>";
            var htmlString1 = this.RenderPartialViewToString("_CompanyProfileTimeSlotsTemplate2", vm);
            ViewBag.htmlItem1 = htmlString1;
            ViewBag.Title = "generate script";
            ViewBag.EnCompanyId = SecurityHelper.EncryptText(profileID.ToString(), Kuyam.Utility.ConfigManager.CryptKey);
            model.ProfileCompany = profile.ProfileCompany;
            return View(model);
            // var htmlTimSlot =
            //List<Service> lstService = _companyProfileService.GetListService();
            //List<ServiceCompany> serviceCompanies = _companyProfileService.GetCategoryByProfileID(profileID);
            //model.ProfileID = profile.ProfileID;
            //model.Name = profile.ProfileCompany.Name;
            //model.Street1 = profile.ProfileCompany.Street1;
            //model.Street2 = profile.ProfileCompany.Street2;   
            //model.City = string.Format("{0},{1}", profile.ProfileCompany.City, profile.ProfileCompany.State);
            //model.State = UtilityHelper.TruncateData(profile.ProfileCompany.State, 4);
            //model.Zip = profile.ProfileCompany.Zip;
            //model.Email = profile.ProfileCompany.Email;
            //if (profile.ProfileCompany != null)
            //{
            //    model.Desc = profile.ProfileCompany.Desc;
            //}

            //model.Url = profile.ProfileCompany.Url;
            //model.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(profile.ProfileCompany.Phone);
            //model.ContactName = profile.ProfileCompany.ContactName;

            //foreach (ServiceCompany item in serviceCompanies)
            //{
            //    Category category = new Category();
            //    category.CategoryID = item.ServiceID.ToString();
            //    category.NamCategory = item.Service.ServiceName;
            //    model.Categories.Add(category);
            //}

            //foreach (CompanyHour item in profile.ProfileCompany.CompanyHours)
            //{
            //    string dayOfWeek = GetDayString(item.DayOfWeek);
            //    if (item.IsDaily.HasValue && item.IsDaily.Value)
            //    {
            //        dayOfWeek = "isdaily";
            //    }

            //    string fromHour = String.Format("{0:hh:mmtt}", new DateTime(item.FromHour.Ticks)).ToLower();
            //    string toHour = String.Format("{0:hh:mmtt}", new DateTime(item.ToHour.Ticks)).ToLower();
            //    if (fromHour.StartsWith("0"))
            //    {
            //        fromHour = fromHour.Substring(1);
            //    }
            //    if (toHour.StartsWith("0"))
            //    {
            //        toHour = toHour.Substring(1);
            //    }
            //    string id = string.Format("{0},{1},{2}", dayOfWeek, fromHour, toHour);
            //    string day = string.Format("{0} {1}-{2}", GetDayString(item.DayOfWeek), fromHour, toHour);
            //    int companyIndex = item.DayOfWeek;

            //    if (item.DayOfWeek == 0)
            //    {
            //        companyIndex = 7;
            //    }

            //    Companylist company = new Companylist()
            //    {
            //        CompanyIndex = companyIndex,
            //        CompanyID = id,
            //        CompanyNam = day
            //    };

            //    model.Companylist.Add(company);
            //}
            //model.Companylist.OrderBy(c => c.CompanyIndex);

            //foreach (Service item in lstService)
            //{
            //    model.CategoryCompany.Add(new SelectListItem()
            //    {
            //        Text = item.ServiceName,
            //        Value = item.ServiceID.ToString()
            //    });
            //}

        }

        public ActionResult GoTemplate1(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error404", "Error");
            var comId = SecurityHelper.DecryptText(id.Trim(), Kuyam.Utility.ConfigManager.CryptKey);
            int companyId;
            if (!int.TryParse(comId, out companyId))
                return RedirectToAction("Error404", "Error");
            ViewBag.IsScript = true;
            var companyAppointmentService = (CompanyAppointmentController)DependencyResolver.Current.GetService(typeof(CompanyAppointmentController));
            var vm = companyAppointmentService.GetCompanyProfileTimeSlots(companyId);
            return View("_CompanyProfileTimeSlotsTemplate1", vm);
        }
        public ActionResult GoTemplate2(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error404", "Error");
            var comId = SecurityHelper.DecryptText(id.Trim(), Kuyam.Utility.ConfigManager.CryptKey);
            int companyId;
            if (!int.TryParse(comId, out companyId))
                return RedirectToAction("Error404", "Error");
            var companyAppointmentService = (CompanyAppointmentController)DependencyResolver.Current.GetService(typeof(CompanyAppointmentController));
            var vm = companyAppointmentService.GetCompanyProfileTimeSlots(companyId);
            ViewBag.IsScript1 = true;
            return View("_CompanyProfileTimeSlotsTemplate2", vm);
        }
        [Authorize]
        public ActionResult AddToFavorite1(string id)
        {
            bool result = false;
            Cust cust = MySession.Cust;
            if (!DAL.isFavorite(cust.CustID, int.Parse(id)))
            {
                Favorite fav = new Favorite();
                fav.ProfileID = int.Parse(id);

                fav.CustID = cust.CustID;
                fav.CreatedTime = DateTime.Now;
                result = ProfileCompany.AddToFavorite(fav);
            }

            return RedirectToAction("Availability", "CompanyProfile", new { id = int.Parse(id) });
        }

        [Authorize]
        public ActionResult RemoveFavorite1(string id)
        {
            bool result = false;
            Cust cust = MySession.Cust;
            Favorite fav = DAL.GetFavoriteByCustIDProfileID(cust.CustID, int.Parse(id));
            if (fav != null)
                result = DAL.DeleteFavorite(fav);

            return RedirectToAction("Availability", "CompanyProfile", new { id = int.Parse(id) });
        }

        #endregion

        #region Instructor

        [Authorize]
        public ActionResult Instructor(int? id)
        {
            int profileId = this.ProfileId;
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic");

            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");

            int profileID = profiles.ProfileID;
            List<ServiceCompany> classList = new List<ServiceCompany>(); // will change to list class 
            classList = _classService.GetClassSevicesByProfileID(profileID);
            ViewBag.CompanyClassServices = classList;
            ViewBag.CompanyEmployees = _classService.GetInstructorByProfileID(profileID);
            ViewBag.ProfileCompany = profiles;
            int verified = (int)Kuyam.Database.Types.FlagInvite.Unverified;
            if (id.HasValue && id.Value > 0)
            {
                CompanyEmployee employee = new CompanyEmployee();
                employee = ProfileCompany.GetCompanyEmployee(id.Value);
                if (employee != null)
                {
                    ViewBag.Employee = employee;
                    List<EmployeeService> isList = _classService.GetListInstructorClassFromInstructorId(id.Value);
                    string scListIDs = string.Empty;
                    if (isList != null && isList.Count > 0)
                    {
                        foreach (var service in isList)
                        {
                            scListIDs = scListIDs + service.ServiceCompanyID.ToString() + ",";
                        }
                    }
                    ViewBag.StringSCListIDs = scListIDs;
                    if (profiles.Cust != null)
                    {
                        Invite i = DAL.GetInviteForSMSVerify(employee.Phone, profiles.Cust.Email);
                        if (i != null && i.Active == true)
                        {
                            verified = (int)Kuyam.Database.Types.FlagInvite.Verified;
                        }
                    }
                }

            }
            ViewBag.companyId = profileID;
            ViewBag.Verified = verified;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditInstructorInfo(string employeeID, string employeeName, string email, string phone, string stringServiceCompanyIDs, string unselectedService, string employeeDefault, string paypal, int profileId = 0)
        {
            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            int profileID = profile.ProfileID;
            int empID = 0;
            if (!int.TryParse(employeeID, out empID))
            {
                return RedirectToAction("instructor");
            }
            bool isDefault = false;
            bool.TryParse(employeeDefault, out isDefault);

            //update employeeInfo
            CompanyEmployee employee = ProfileCompany.GetCompanyEmployee(empID);
            if (employee != null)
            {
                employee.EmployeeName = employeeName;
                employee.Email = email;
                employee.ProfileCompanyID = profileID;
                employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
                employee.PaymentAccount = paypal;
                employee.IsDefault = isDefault;
                DAL.UpdateCompanyEmployeeInfo(employee);
                // _classService.DeleteInstructorClassByInstructorID(employee.EmployeeID);

            }

            bool result = false;
            string error = string.Empty;
            // Update class list for instructor
            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        EmployeeService es = new EmployeeService();
                        es.CompanyEmployeeID = int.Parse(employeeID);
                        es.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = _classService.AddInstructorClass(es);
                        if (!result)
                        {
                            error = "add class is not success";
                            return Json(new { Success = result, Message = error }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            string[] unselectServiceIds = unselectedService.Split(',');
            if (unselectServiceIds.Length > 0)
            {
                for (int i = 0; i < unselectServiceIds.Length; i++)
                {
                    if (unselectServiceIds[i] != string.Empty)
                    {
                        result = _classService.DeleteInstructorClass(int.Parse(unselectServiceIds[i]), int.Parse(employeeID));
                    }
                    if (!result)
                    {
                        error = "the class schedulers should be removed before remove this class from instructor";
                        return Json(new { Success = result, Message = error }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            int empl = int.Parse(employeeID);
            return Json(new { Success = result, Message = error }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddInstructorInfo(string employeeName, string email, string phone, string stringServiceCompanyIDs, string employeeDefault, string paypal, int profileId = 0)
        {

            Profile profile = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profile == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            int profileID = profile.ProfileID;

            bool isDefault = false;
            bool.TryParse(employeeDefault, out isDefault);

            if (isDefault)
            {
                _companyProfileService.UpdateDefaultEmploee(profile.ProfileID);
            }

            CompanyEmployee employee = new CompanyEmployee();
            employee.EmployeeName = employeeName;
            employee.Email = email;
            employee.ProfileCompanyID = profileID;
            employee.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(phone);
            employee.PaymentAccount = paypal;
            employee.IsDefault = isDefault;
            employee.EmployeeTypeId = (int)Types.EmployeeType.instructor;

            int employeeID = DAL.AddEmployee(employee);
            bool result = false;

            string[] serviceCompanyIds = stringServiceCompanyIDs.Split(',');
            if (serviceCompanyIds.Length > 0)
            {
                for (int i = 0; i < serviceCompanyIds.Length; i++)
                {
                    if (serviceCompanyIds[i] != string.Empty)
                    {
                        EmployeeService es = new EmployeeService();
                        es.CompanyEmployeeID = employeeID;
                        es.ServiceCompanyID = int.Parse(serviceCompanyIds[i]);
                        result = _classService.AddInstructorClass(es);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Class(int? id)
        {
            int profileId = this.ProfileId;
            ViewBag.Categories = DAL.GetParentService();
            if (profileId != 0)
            {
                ViewBag.CompanyServices = _classService.GetListClassByProfileId(profileId);
            }
            ViewBag.companyId = profileId;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddClass(int serviceID, int time, decimal price, int maxPeople, string description, DateTime start, DateTime end, int profileId = 0)
        {
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return Json("false", JsonRequestBehavior.AllowGet);

            int profileID = profiles.ProfileID;
            ServiceCompany sc = new ServiceCompany();

            sc.ProfileID = profileID;
            sc.ServiceID = serviceID;
            sc.Duration = time;
            sc.AttendeesNumber = maxPeople;
            sc.Price = price;
            sc.Description = description;
            sc.Created = DateTime.Now;
            sc.Status = (int)Types.ServiceCompanyStatus.Active;
            sc.ServiceTypeId = (int)Types.ServiceType.ClassType;
            sc.IsPerDay = true;
            sc.FromDateTime = start;
            sc.ToDateTime = end;
            sc.ServiceName = DAL.GetServiceNameFromServiceID(serviceID);
            bool result = DAL.AddServiceCompany(sc);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public ActionResult UpdateClass(int serviceCompanyID, int time, decimal price, int maxPeople, string description, DateTime start, DateTime end)
        {
            bool result = false;
            ServiceCompany sc = DAL.GetServiceCompany(serviceCompanyID);
            var isUpdateDuration = sc.Duration != time;
            if (sc != null)
            {
                sc.Duration = time;
                sc.AttendeesNumber = maxPeople;
                sc.Price = price;
                sc.Description = description;
                sc.FromDateTime = start;
                sc.ToDateTime = end;
                sc.Modified = DateTime.UtcNow;

                if (sc.Appointments.Count > 0)
                {
                    result = false;
                }
                else
                {
                    result = DAL.UpdateServiceCompany(sc);
                    if (isUpdateDuration)
                    {
                        result = _classService.UpdateTimeForInstructorClassScheduler(sc.ServiceCompanyID, sc.Duration.Value);
                    }

                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ClassScheduler(int? instructorId, int? classId, int companyId = 0)
        {
            string fromHour = Request.QueryString["fromHour"];
            string duration = Request.QueryString["duration"];

            var dtime = 0;
            int.TryParse(duration, out dtime);
            var iId = 0;
            var cId = 0;
            if (instructorId.HasValue)
            {
                iId = instructorId.Value;
            }
            if (classId.HasValue)
            {
                cId = classId.Value;
            }


            ViewBag.FromHour = fromHour;

            if (!string.IsNullOrEmpty(fromHour))
            {
                var toHour = DateTime.Parse(fromHour).AddMinutes(dtime);
                ViewBag.ToHour = toHour.ToString("h:mm tt");
            }


            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc).ToString();
            companyId = this.ProfileId != 0 ? this.ProfileId : companyId;
            Profile profiles = _companyProfileService.GetProfileByID(companyId != 0 ? companyId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");

            int profileID = profiles.ProfileID;
            ViewBag.EmployeeList = _classService.GetInstructorByProfileID(profileID);
            if (iId > 0 || cId > 0)
            {
                string companyHours = BusinessService.GetCompanyHoursJson(profileID);
                ViewBag.CompanyHours = companyHours;
                var stringEvent = string.Empty;
                var stringClassHour = string.Empty;
                if (iId > 0)
                {
                    List<EmployeeHour> ehList = DAL.GetEmployeeHoursFromEmployeeID(iId);
                    //List<EmployeeHour> ihList = _classService.GetInstructorHoursFromInstructorId(iId);
                    //if (ihList != null && ihList.Count > 0)
                    //{
                    //    if(ehList == null)
                    //    {
                    //        ehList = ihList;
                    //    }
                    //    else
                    //    {
                    //        ehList.AddRange(ihList);
                    //    }

                    //}
                    if (ehList != null)
                    {
                        stringEvent = BusinessService.GetEventCalendar(ehList);
                    }
                    //List<ClassScheduler> previewHour = _classService.GetInstructorHourPreview(iId);
                    //ViewBag.previewHour = previewHour;
                    ViewBag.InstructorId = iId;
                }

                if (cId > 0)
                {
                    //List<EmployeeHour> chList = _classService.GetInstructorHoursFromClassId(cId);
                    //if (chList != null && chList.Count > 0)
                    //{
                    //    stringClassHour = BusinessService.GetListClassHourCalendar(chList);
                    //}
                    ViewBag.Class = _classService.GetClassById(cId);

                }
                ViewBag.StringEvent = stringEvent + stringClassHour;
            }

            ViewBag.ServiceCompanys = _classService.GetListClassByProfileId(profileID);
            return View();
        }

        [Authorize]
        public ActionResult SchedulerForClasses(int? instructorId, int? classId, int companyId = 0)
        {

            var iId = 0;
            var cId = 0;
            if (instructorId.HasValue)
            {
                iId = instructorId.Value;
            }
            if (classId.HasValue)
            {
                cId = classId.Value;
            }

            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc).ToString();
            companyId = this.ProfileId != 0 ? this.ProfileId : companyId;
            Profile profiles = _companyProfileService.GetProfileByID(companyId != 0 ? companyId : MySession.ProfileID);
            if (profiles == null)
                return RedirectToAction("SetupBasic", new { companyId = MySession.ProfileID });
            if (profiles.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Active)
                return RedirectToAction("verificationcode", "Company");

            int profileID = profiles.ProfileID;


            if (iId > 0 && cId > 0)
            {
                ViewBag.ClassSchedulers = _classService.GetInstructorHourByInstructorIdClassId(cId, iId);
                ViewBag.InstructorClassId = _classService.GetInstructorClassId(cId, iId);
            }

            if (iId > 0 || cId > 0)
            {
                string companyHours = BusinessService.GetCompanyHoursJson(profileID);
                ViewBag.CompanyHours = companyHours;
                var stringEvent = string.Empty;
                var stringClassHour = string.Empty;
                if (iId > 0)
                {
                    ViewBag.InstructorId = iId;
                }
                if (cId > 0)
                {
                    ViewBag.Class = _classService.GetClassById(cId);
                    ViewBag.EmployeeList = _classService.GetInstructorByClassId(cId);

                }

            }

            ViewBag.ServiceCompanys = _classService.GetListClassByProfileId(profileID);
            return View();


        }

        [Authorize]
        [HttpPost]
        public ActionResult AddSchedulerForClass(int instructorClassId, int employeeID, string fromHour, int duration, string stringListDays, int profileId = 0)
        {
            Profile profiles = _companyProfileService.GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            if (profiles == null)
                return Json("false", JsonRequestBehavior.AllowGet);
            //DateTime lasted = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now);

            InstructorClassScheduler cs = new InstructorClassScheduler();
            cs.InstructorClassID = instructorClassId;
            var timeFrom = TimeSpan.Parse(DateTime.Parse(fromHour).TimeOfDay.ToString());
            cs.FromHour = timeFrom;
            cs.ToHour = TimeSpan.Parse(DateTime.Parse(fromHour).AddMinutes(duration).TimeOfDay.ToString());

            var result = false;
            var error = string.Empty;
            if (cs.FromHour < cs.ToHour)
            {
                if (stringListDays != string.Empty)
                {
                    string[] listDay = stringListDays.Split(',');
                    result = _classService.AddClassScheduler(cs, listDay, employeeID, out error);
                }
                else
                {
                    result = _classService.AddClassScheduler(cs, new string[0], employeeID, out error);
                }
            }

            return Json(new { Success = result, Message = error }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddClassSheduler(string previewHours)
        {
            var result = false;
            try
            {
                if (!string.IsNullOrEmpty(previewHours))
                {
                    string[] ids = previewHours.Split(';');

                    //foreach (var id in ids)
                    //{
                    //    if(!string.IsNullOrEmpty(id))
                    //    {
                    //        var classSchedulerId = int.Parse(id.Trim());
                    //        result = _classService.AddClassScheduler(classSchedulerId);
                    //        if (!result)
                    //            break;
                    //    }

                    //}
                }

            }
            catch
            {
                result = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteClassScheduler(int classSchedulerId)
        {
            var result = _classService.DeleteClassSchedulerById(classSchedulerId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion


        [Authorize]
        [HttpPost]
        public ActionResult DeleteClass(int serviceCompanyID)
        {

            bool result = _classService.DeleteClass(serviceCompanyID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
