using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain;
using System.Web.Security;
using Kuyam.WebUI.Models;
using Kuyam.Database.Extensions;
using System.IO;
using Kaltura;
using Kuyam.Utility;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    [Authorize(Roles = "HotelAdmin, HotelStaff, Admin, Agent")]
    public class HotelController : KuyamBaseController
    {
        private readonly AdminService _adminService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly ICategoryFeaturedHotelService _categoryFeatureHotelService;
        private readonly IBlogCategoryService _categoryService;

        public HotelController(AdminService adminService,
            CompanyProfileService companyProfileService, ICategoryFeaturedHotelService categoryFeatureHotelService,
            IBlogCategoryService categoryService)
        {
            this._adminService = adminService;
            this._companyProfileService = companyProfileService;
            _categoryFeatureHotelService = categoryFeatureHotelService;
            _categoryService = categoryService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("HotelList");
        }

        public ActionResult StaffList(int? page, string key, int? hotelId)
        {
            var roles = _adminService.GetAllRoles();
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int id = hotelId ?? 0;
            List<Hotel> hotels = null;


            //if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            //{
            //}
            //else
            //{
            //    hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord, MySession.CustID);
            //}

            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfAdminByCustId(string.Empty, 1, int.MaxValue, out totalRecord, MySession.CustID);
            }

            List<UserStaff> users = _adminService.AdminGetListUserByRoleAndHotelId(MySession.CustID, id, key, pageIndex, 10, out totalRecord);
            ViewBag.Roles = roles;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.HotelId = hotelId;
            ViewBag.Hotels = hotels;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_HotelStaffList", users);
            }
            return View(users);

        }

        public ActionResult StaffAdd(int? hotelId)
        {
            int id = hotelId ?? 0;
            ViewBag.Users = _adminService.AdminGetListUserByRoleAndHotelId(id);
            ViewBag.HotelId = id;
            return View();
        }

        [HttpPost]
        public ActionResult StaffAdd(int hotelId, int custId)
        {

            var staff = new HotelStaff
            {
                CustID = custId,
                HotelID = hotelId,
                IsDefault = false,
                CreateDate = DateTime.UtcNow
            };

            _adminService.InsertHotelStaff(staff);
            var user = _adminService.GetUserById(custId);
            if (!Roles.IsUserInRole(user.Username, "HotelStaff"))
                Roles.AddUserToRole(user.Username, "HotelStaff");
            return RedirectToAction("StaffList", new { hotelId = hotelId });
        }

        public ActionResult StaffDelete(int id, int hotelId, int? page)
        {
            var staff = _adminService.GetHotelStaffById(id);
            _adminService.DeleteHotelStaff(staff);
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            List<UserStaff> users = _adminService.AdminGetListUserByRoleAndHotelId(MySession.CustID, hotelId, null, pageIndex, 10, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.HotelId = hotelId;
            return PartialView("_HotelStaffList", users);
        }

        public ActionResult HotelList(string keyName, int? pageIndex)
        {
            int totalRecord = 0;
            int index = pageIndex ?? 1;
            List<Hotel> hotels = null;
            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(keyName, index, 10, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfAdminByCustId(keyName, index, 10, out totalRecord, MySession.CustID);
            }

            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.Key = keyName;
            var models = hotels.Select(m => new HotelListModel
            {
                HotelID = m.HotelID,
                LogoId = m.LogoId,
                Name = m.Name,
                UserName = m.Cust.Username
            }).ToList();
            if (Request.IsAjaxRequest())
                return PartialView("_HotelList", models);
            return View(models);
        }

        public ActionResult HotelEdit(int id)
        {
            int pageIndex = 1;
            string keyName = Request.Params["keyname"];
            if (!int.TryParse(Request.Params["page"], out pageIndex))
            {
                pageIndex = 1;
            }
            var hotel = _adminService.GetHotelById(id);
            var users = DAL.xGetCusts();
            var model = new HotelModel
            {
                CustID = hotel.CustID,
                Name = hotel.Name,
                CustList = users,
                Cust = users.Where(m => m.CustID == hotel.CustID).FirstOrDefault()
            };
            ViewBag.Page = pageIndex;
            ViewBag.KeyName = keyName;
            model.HotelID = id;
            return View(model);
        }



        [HttpPost]
        public ActionResult HotelEdit(HotelModel model)
        {
            var file = model.FileUpload;
            KalturaMediaEntry kalturaMediaEntry = null;
            var hotel = _adminService.GetHotelById(model.HotelID);
            if (file != null)
            {
                var fullPath = ConfigManager.StorageRoot + Path.GetFileName(file.FileName);
                string fileName = file.FileName;
                file.SaveAs(fullPath);
                FileStream _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(_fileStream, Kaltura.KalturaMediaType.IMAGE, fileName);
                _fileStream.Flush();
                _fileStream.Close();
                ConfigManager.DeleteFile(fullPath);

                var media = new Medium
                {
                    CustID = MySession.CustID,
                    CustTypeID = (int)Types.CustType.Company,
                    MediaLocationTypeID = (int)Types.MediaLocation.Kaltura,
                    LocationData = kalturaMediaEntry.Id,
                    LocationPath = kalturaMediaEntry.DataUrl,
                    MediaTypeID = (int)Types.MediaType.Image,
                    Desc = kalturaMediaEntry.Description
                };
                var hotelMedia = new HotelMedia
                {
                    HotelID = hotel.HotelID,
                    IsDefault = true,
                    HotelMediaID = (int)Types.HotelMediaType.IsLogo,

                };
                media.HotelMedias.Add(hotelMedia);
                _companyProfileService.InsertMedia(media);
            }

            if (hotel != null)
            {
                hotel.Name = model.Name;
                hotel.CustID = model.CustID;
                if (file != null)
                    hotel.LogoId = kalturaMediaEntry.Id;
                _adminService.UpdateHotel(hotel);
            }

            return RedirectToAction("HotelList");

        }

        #region generate code

        public ActionResult HotelCodeList(int? id, string keyCode, string KeyName, int? pageIndex)
        {

            int index = pageIndex ?? 1;

            int totalRecord = 0;
            List<Hotel> hotels = null;
            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfAdminByCustId(string.Empty, 1, int.MaxValue, out totalRecord, MySession.CustID);
            }
            int hotelId = id ?? ((hotels != null && hotels.Count > 0) ? hotels[0].HotelID : -1);
            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord, MySession.CustID);

            ViewBag.Page = index;
            ViewBag.KeyCode = keyCode;
            ViewBag.KeyName = KeyName;
            ViewBag.HotelId = hotelId;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Hotels = hotels;

            if (models == null)
                models = new List<HotelCode>();
            if (Request.IsAjaxRequest())
                return PartialView("_HotelCodeList", models);
            return View(models);

        }

        [HttpPost]
        public ActionResult HotelCodeAdd(int? id, string keyCode, string KeyName, int? pageIndex)
        {
            int hotelId = id ?? 1;
            int index = pageIndex ?? 1;
            int totalRecord = 0;

            DateTime dt = DateTime.UtcNow;
            dt = dt.AddDays(7);
            var hotelCode = new HotelCode
            {
                HotelID = hotelId,
                CodeNumber = UtilityHelper.GenerateRandomDigitCode(5),
                ExpiredDate = dt,
                Status = (int)Types.HotelCodeStatus.Active,
                Hotel = _adminService.GetHotelById(hotelId)
            };
            _adminService.InsertHotelCode(hotelCode);
            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord, MySession.CustID);
            ViewBag.Page = index;
            ViewBag.KeyCode = keyCode;
            ViewBag.KeyName = KeyName;
            ViewBag.TotalRecords = totalRecord;
            return PartialView("_HotelCodeList", models);
        }

        [HttpPost]
        public ActionResult HotelCodeDelete(int? id, string keyCode, string KeyName, int? pageIndex)
        {
            int hotelId = -1;
            int codeId = id ?? 0;
            int index = pageIndex ?? 1;
            int totalRecord = 0;

            DateTime dt = DateTime.UtcNow;
            dt = dt.AddDays(7);
            var hotelCode = _adminService.GetHotelCodeById(codeId);
            if (hotelCode != null)
                hotelId = hotelCode.HotelID;
            _adminService.DeleteHotelCode(hotelCode);

            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord, MySession.CustID);
            ViewBag.Page = index;
            ViewBag.KeyCode = keyCode;
            ViewBag.KeyName = KeyName;
            ViewBag.TotalRecords = totalRecord;
            return PartialView("_HotelCodeList", models);
        }

        #endregion generate code

        public ActionResult FeaturedHotel(int? hotelId)
        {
            List<Hotel> hotels = null;
            int totalRecord = 0;
            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfStaffByCustId(MySession.CustID);
            }
            int _hotelId = hotelId ?? ((hotels != null && hotels.Count > 0) ? hotels[0].HotelID : 0);
            var model = new FeatureCompanyHotelModel
            {
                FeaturedHotel = _adminService.GetFeaturedHotelByHotelId(_hotelId),
                ProfileCompany = _adminService.AdminGetListCompany(),
                HotelId = _hotelId,
                Hotel = hotels
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult FeaturedHotel(int? hotelId, int? profileId, int? priority, int? featureId)
        {
            bool result = false;
            int _profileId = profileId ?? 0;
            int index = priority ?? 0;
            List<Hotel> hotels = null;
            int totalRecord = 0;
            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfStaffByCustId(MySession.CustID);
            }

            int _hotelId = hotelId ?? ((hotels != null && hotels.Count > 0) ? hotels[0].HotelID : 0);
            int _featureId = featureId ?? 0;

            var featurelist = _adminService.GetFeaturedHotelByProfileIdAndHotelId(_hotelId);

            var feature = featurelist.Where(m => m.Priority == index).FirstOrDefault();

            var profile = featurelist.Where(m => m.ProfileID == _profileId).FirstOrDefault();

            if (_profileId == 0)
            {
                result = _adminService.DeleteFeatureHotel(_featureId);
            }
            else if (feature != null && profile == null)
            {
                feature.Priority = index;
                feature.ProfileID = _profileId;
                result = _adminService.UpdateFeatureHotel(feature);
            }
            else if (feature == null && profile == null)
            {
                var featuredHotel = new FeaturedHotel
                {
                    HotelID = _hotelId,
                    Priority = index,
                    ProfileID = _profileId,
                    Created = DateTime.UtcNow
                };
                result = _adminService.AddFeatureHotel(featuredHotel);
            }

            if (Request.IsAjaxRequest())
            {
                return Json(result);
            }

            var model = new FeatureCompanyHotelModel
            {
                FeaturedHotel = _adminService.GetFeaturedHotelByHotelId(_hotelId),
                ProfileCompany = _adminService.AdminGetListCompany(),
                HotelId = _hotelId,
                Hotel = hotels
            };
            return View(model);
        }

        #region Category Featured Hotel
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Feature Hotel Id</param>
        /// <returns></returns>
        public ActionResult EditCategoriesForFeaturedHotel(int id, int hotelId)
        {
            var categoryFeatureHotels = _categoryFeatureHotelService.GetCategories(id);
            var categories = _categoryService.GetAll();
            ViewBag.featuredHotelId = id;
            ViewBag.categories = categories;
            ViewBag.categoryFeatureHotels = categoryFeatureHotels.ToList();
            ViewBag.hotelId = hotelId;
            return View();
        }

        [HttpPost]
        public ActionResult EditCategoriesForFeaturedHotel(int featuredId, int hotelId, List<int> categories)
        {
            if (categories == null)
            {
                categories = new List<int>();
            }
            var categoryFeatured = new List<CategoryFeaturedHotel>();
            foreach (var item in categories)
            {
                var category = _categoryFeatureHotelService.GetById(item, featuredId);
                if (category != null)
                    categoryFeatured.Add(category);
                else
                {
                    category = new CategoryFeaturedHotel
                    {
                        FeaturedId = featuredId,
                        BeCategoryId = item,
                        Created = DateTime.Now
                    };
                    categoryFeatured.Add(category);
                }
            }
            _categoryFeatureHotelService.Insert(featuredId, categoryFeatured);
            if (hotelId == 0)
                return RedirectToAction("FeaturedHotel");
            return RedirectToAction("FeaturedHotel", new { hotelId = hotelId });
        }
        #endregion

        #region Appointments

        public ActionResult Appointment(int? page, string key, int? status, int? hotelId = -1)
        {
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            //int companyType = type ?? (int)Types.CompanyType.Unknown;
            int _hotelId = hotelId ?? 0;
            int appointmentStatus = status ?? (int)Types.AppointmentStatus.Unknown;

            ViewBag.Hotels = null;

            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                ViewBag.Hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                ViewBag.Hotels = _adminService.GetListHotelOfStaffByCustId(MySession.CustID);
            }

            ViewBag.Appointments = _adminService.GetAppointments(key, (int)Types.CompanyType.Unknown, _hotelId, appointmentStatus, pageIndex, 10, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.HotelId = _hotelId;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_AppointmentAdminListResults");
            }
            return View();
        }

        #endregion

    }
}
