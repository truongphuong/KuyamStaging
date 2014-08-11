using Kuyam.Database;
using Kuyam.Domain;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.WebUI.Areas.API.Models;
using Kuyam.WebUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain.Seo;
using Kuyam.WebUI.Areas.API.Models.Offers;
using Kuyam.Domain.OfferServices;

namespace Kuyam.WebUI.Areas.API.Controllers
{
    public class BeautifyController : Controller
    {
        private readonly CompanySearchService _companySearchService;
        private readonly IOfferService _offerService;
        public BeautifyController(CompanySearchService companySearchService,
            IOfferService offerService)
        {
            _companySearchService = companySearchService;
            _offerService = offerService;
        }


        //
        // GET: /API/Beautify/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Lists(BeatifyInputParamModel inputParam)
        {
            var results = _companySearchService.GetProfileCompaniesByEventId(inputParam.EventId, inputParam.CateGoryId, inputParam.CityName);

            //var listCategorys = results.Select(m => new { m.CategoryID, m.CategoryName }).ToList().Distinct();

            var listCategorys = new List<string> { "nails", "blowout", "facial", "wax", "tanning", "massage", "yoga", "Pilates", "personal trainer" };

            var beatifyModel = new List<CategoryModel>();

            foreach (var item in listCategorys)
            {
                var groupCompany = results.Where(m => m.CategoryName.ToLower() == item.ToLower()).OrderBy(o => o.Name).ToList();
                if (groupCompany != null && groupCompany.Count() > 0)
                {
                    var categoryModel = new CategoryModel();
                    categoryModel.CategoryID = (groupCompany != null && groupCompany.Count() > 0) ? groupCompany.First().CategoryID : 0;
                    categoryModel.CategoryName = item;
                    var model = groupCompany.Select(m => new CompanyProfilesModel
                    {
                        ProfileID = m.ProfileID,
                        Name = m.Name,
                        SlugName = Url.RouteUrl("Slug", new { seName = m.GetSeName(m.ProfileID) }),
                        AvailableTimeSlots = m.CompanyAvailableTimeSlots != null ? new TimeSlotsDTO
                        {
                            DayAvaiable = m.CompanyAvailableTimeSlots.DayAvaiable,
                            IsShowMore = m.CompanyAvailableTimeSlots.IsShowMore,
                            IsRederect = m.CompanyAvailableTimeSlots.IsRederect,
                            IsClass = m.CompanyAvailableTimeSlots.IsAvailableToday,
                            IsAvailableToday = m.CompanyAvailableTimeSlots.IsAvailableToday,
                            CompanyProfileId = m.CompanyAvailableTimeSlots.CompanyProfileId,
                            CompanyTypeID = m.CompanyAvailableTimeSlots.CompanyTypeID,
                            CompanyTimeSlots = m.CompanyAvailableTimeSlots.CompanyTimeSlots,
                            CompanyHours = m.CompanyAvailableTimeSlots.CompanyHours != null ? m.CompanyAvailableTimeSlots.CompanyHours.Select(a => new CompanyHourDTO
                            {
                                CompanyHourID = a.CompanyHourID,
                                ProfileCompanyID = a.ProfileCompanyID,
                                FromHour = a.FromHour,
                                ToHour = a.ToHour,
                                DayOfWeek = a.DayOfWeek,
                                IsDaily = a.IsDaily
                            }).ToList() : null,

                        } : null,
                        LogoMediaId = m.LogoMediaId,
                        CompanyEvents = m.CompanyEvents,
                        CompanyGenreralTimes = m.CompanyAvailableTimeSlots.CompanyGenreralTimes,
                        CompanyHours = (m.CompanyHours != null && m.CompanyHours.Count() > 0) ? m.CompanyHours.Select(b => new CompanyHourDTO
                        {
                            CompanyHourID = b.CompanyHourID,
                            ProfileCompanyID = b.ProfileCompanyID,
                            FromHour = b.FromHour,
                            ToHour = b.ToHour,
                            DayOfWeek = b.DayOfWeek,
                            IsDaily = b.IsDaily
                        }).ToList() : null,
                        CompanyMedias = (m.CompanyMedias != null && m.CompanyMedias.Count() > 0) ? new List<CompanyMediaDTO> { new CompanyMediaDTO(m.CompanyMedias) } : null,
                        ClassSchedulerHours = (m.InstructorClassSchedulerHours != null && m.InstructorClassSchedulerHours.Count() > 0) ? m.InstructorClassSchedulerHours.Select(c => new EmployeeHourDTO
                        {
                            ID = c.ID,
                            FromHour = c.FromHour,
                            ToHour = c.ToHour,
                            CompanyEmployeeID = c.CompanyEmployeeID,
                            DayOfWeek = c.DayOfWeek,
                            IsPreview = c.IsPreview,
                            ServiceCompanyID = c.ServiceCompanyID,
                            AttendeesNumber = c.AttendeesNumber,
                            ServiceName = c.ServiceName,

                        }).ToList() : null,
                        EmployeeHours = (m.EmployeeHours != null && m.EmployeeHours.Count() > 0) ? m.EmployeeHours.Select(c => new EmployeeHourDTO
                        {
                            ID = c.ID,
                            FromHour = c.FromHour,
                            ToHour = c.ToHour,
                            CompanyEmployeeID = c.CompanyEmployeeID,
                            DayOfWeek = c.DayOfWeek,
                            IsPreview = c.IsPreview,
                            ServiceCompanyID = c.ServiceCompanyID,
                            AttendeesNumber = c.AttendeesNumber,
                            ServiceName = c.ServiceName,

                        }).ToList() : null,
                        ListServices = m.ListServices,
                        Rate = m.Rate,
                        TotalReviews = m.TotalReviews,
                        Street1 = m.Street1,
                        Street2 = m.Street2,
                        City = m.City,
                        State = m.State,
                        Zip = m.Zip,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        CompanyStatusID = m.CompanyStatusID,
                        CompanyTypeID = m.CompanyTypeID,
                        IsClass = m.IsClass,
                        IsEvent = m.IsEvent,
                        IsVideo = m.IsVideo,
                        IsFeature = m.IsFeature,
                        PaymentMethod = m.PaymentMethod,
                        PaymentOptions = m.PaymentOptions,
                        Url = m.Url,
                        YoutubeLink = m.YoutubeLink,
                        FirstAlert = m.FirstAlert,
                        SecondAlert = m.SecondAlert,
                        CancelHour = m.CancelHour,
                        CancelPolicy = m.CancelPolicy,
                        CancelRefundPercent = m.CancelRefundPercent,
                        ContactFirstName = m.ContactFirstName,
                        ContactLastName = m.ContactLastName,
                        ContactName = m.ContactName,
                        ContactTitle = m.ContactTitle,
                        Email = m.Email
                    });
                    categoryModel.CompanyProfiles.AddRange(model);
                    beatifyModel.Add(categoryModel);
                }
            }
            return new JsonNetResult(beatifyModel);
        }


        public ActionResult GetListCities()
        {
            var listCities = _offerService.GetCitiesByCompanyEventId();
            return new JsonNetResult(listCities);
        }
        public ActionResult GetOffer(int? companyEventId)
        {

            var companyEvent = _offerService.GetCompanyEventByCompanyEventId(companyEventId ?? 0);
            var listOffers = _offerService.GetListServicesEventByCompanyEventId(companyEventId ?? 0, 0);
            var ListClasses = listOffers.Where(m => m.ServiceTypeId == (int)Types.ServiceType.ClassType).OrderBy(o => o.NewPrice).Take(3).ToList();
            var ListServices = listOffers.Where(m => m.ServiceTypeId == (int)Types.ServiceType.ServiceType).OrderBy(o => o.NewPrice).Take(3).ToList();
            var model = new OfferModel
            {
                SlugName = Url.RouteUrl("Slug", new { sename = companyEvent.GetSeName(companyEvent.ProfileCompany.ProfileID) }),
                Event = new Kuyam.WebUI.Areas.API.Models.Offers.EventDTO(companyEvent.Event),
                ClassOffers = (ListClasses != null && ListClasses.Count() > 0 && companyEvent.ProfileCompany.IsClass.HasValue && companyEvent.ProfileCompany.IsClass.Value && companyEvent.ProfileCompany.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt) ?
                 ListClasses.Select(item => new Kuyam.WebUI.Areas.API.Models.Offers.CompanyServiceEventDTO
                 {
                     ID = item.ID,
                     ServiceTypeId = item.ServiceTypeId,
                     Description = item.Description,
                     CompanyEventID = item.CompanyEventID,
                     ServiceCompanyID = item.ServiceCompanyID,
                     OldPrice = item.OldPrice,
                     NewPrice = item.NewPrice,
                     ServiceName = item.ServiceName,
                     CategoryName = item.CategoryName

                 }).ToList() : null,
                ServiceOffers = (ListServices != null && ListServices.Count() > 0) ?
                ListServices.Select(item => new Kuyam.WebUI.Areas.API.Models.Offers.CompanyServiceEventDTO
                {
                    ID = item.ID,
                    ServiceTypeId = item.ServiceTypeId,
                    Description = item.Description,
                    CompanyEventID = item.CompanyEventID,
                    ServiceCompanyID = item.ServiceCompanyID,
                    OldPrice = item.OldPrice,
                    NewPrice = item.NewPrice,
                    ServiceName = item.ServiceName,
                    CategoryName = item.CategoryName

                }).ToList() : null
            };
            return new JsonNetResult(model);
        }

    }
}
