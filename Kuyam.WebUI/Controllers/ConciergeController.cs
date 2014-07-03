using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iTextSharp.text.pdf;
using Kuyam.Domain;
using Kuyam.Database;
using Kuyam.Domain.HotelVisits;
using Kuyam.WebUI.Helpers;
using Kuyam.WebUI.Models;
using System.Web.Routing;
using Kuyam.WebUI.InfoConnServiceReference;
using System.IO;
using Kuyam.Utility;
using System.Threading;
using Kuyam.Repository.Infrastructure;

namespace Kuyam.WebUI.Controllers
{
    public class ConciergeController : KuyamBaseController
    {
        #region property/const
        private readonly AdminService _adminService;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly CustService _custService;
        private readonly IHotelVisitService _hotelVisitService;
        private readonly HotelService _hotelService;
        private readonly IAppointmentService _appointmentService;
        private const string ROLE_CONCIERGE = "Concierge";
        private const string DATE_TIME_FORMAT = "MM/dd/yyyy";
        #endregion
        #region contructor
        public ConciergeController(AdminService adminService,
           IFormsAuthenticationService formsService,
           IMembershipService membershipService,
           CustService custService,
           IHotelVisitService hotelVisitService,
           HotelService hotelService,
           IAppointmentService appointmentService)
        {
            this._adminService = adminService;
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._custService = custService;
            this._hotelVisitService = hotelVisitService;
            this._hotelService = hotelService;
            this._appointmentService = appointmentService;
        }
        #endregion

        #region authen/login
        /// <summary>
        /// check role is concierge, only role conciege can access
        /// </summary>
        /// <returns></returns>
        public bool AuthorizationConcierge()
        {
            bool isLogin = Request.IsAuthenticated;
            bool isAllow = User.IsInRole(ROLE_CONCIERGE);
            return (isLogin && isAllow && IsHasHotel);
        }
        /// <summary>
        /// login return login page
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            var isHotel = GetHotelId(MySession.CustID);
            ViewBag.NoHotel = !IsHasHotel && Request.IsAuthenticated
                ? "your login is not success because user don't have hotel. please contact support."
                : string.Empty;
            return View();
        }

        public bool IsHasHotel
        {
            get
            {
                var num = GetHotelId(MySession.CustID);
                if (num == -1)
                    return false;
                return true;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ActionResult ConciergeUnauthorizedResult()
        {
            var redirectUrl = Request.RawUrl;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return new RedirectResult("~/Concierge/Login?ReturnUrl=" + redirectUrl);
            }
            else
            {
                return new RedirectResult("~/Concierge/Login");
            }
        }
        /// <summary>
        /// Function login with Admin role
        /// </summary>
        /// <param name="username">Input test</param>
        /// <param name="pass">Input password</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string username, string pass, bool? usesKey, string timeZoneId)
        {

            Cust customer = DAL.xGetCust(username);
            string[] roles = Roles.GetRolesForUser(username);
            ViewBag.NoHotel = string.Empty;
            if (!roles.Contains(ROLE_CONCIERGE))
            {
                ModelState.AddModelError("", "your login is not concierge. please contact support.");
            }
            else
            {
                bool validateUser = _membershipService.ValidateUser(username, pass);
                if (validateUser)
                {

                    if (customer != null)
                    {
                        var hotelId = GetHotelId(customer.CustID);
                        if (hotelId == -1)
                        {
                            ModelState.AddModelError("", "your login is not success because user don't have hotel. please contact support.");
                            return View();

                        }
                        if (customer.Status == (int)Types.UserStatusType.Active)
                        {
                            _formsService.SignIn(username, false /* createPersistentCookie */);
                            //change from true to false by default                                
                            string windowsTimeZoneId = DateTimeUltility.TimeZoneToTimeZoneInfo(timeZoneId);
                            customer.TimeZoneId = windowsTimeZoneId;
                            DAL.UpdateRec(customer, customer.CustID);

                            LogHelper.Info("Login concierge success with username: " + username);
                            string returnUrl = Request.Params["ReturnUrl"];
                            if (!string.IsNullOrEmpty(returnUrl))
                                return Redirect(returnUrl);
                            return RedirectToAction("Index");
                        }
                    }
                }

                ModelState.AddModelError("", "your login is not success. please contact support.");
            }
            LogHelper.Info("Login concierge fail with username: " + username);
            return View();
        }

        #endregion

        #region guest page
        public ActionResult Index(int? page, string key, int? searchType)
        {
            if (!AuthorizationConcierge())
            {
                return ConciergeUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int searchTypeKey = searchType ?? -1;
            var hotelId = GetHotelId(MySession.CustID);
            var guests = _hotelVisitService.GetGuestByHotelId(key, pageIndex, 10, out totalRecord,
                searchTypeKey, hotelId);
            //var hotels = _adminService.GetListHotelOfAdminByCustId(string.Empty, 1, int.MaxValue, out totalRecord, MySession.CustID);
            //ViewBag.Hotels = hotels;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.UsersList = guests;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchTypeKey;
            return View();
        }
        public ActionResult GuestList(int? page, string key, int? searchType)
        {
            if (!AuthorizationConcierge())
            {
                return ConciergeUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int searchTypeKey = searchType ?? 1;
            var _hotelId = GetHotelId(MySession.CustID);
            var guests = _hotelVisitService.GetGuestByHotelId(key, pageIndex, 10, out totalRecord,
                searchTypeKey, _hotelId);
            //var hotels = _adminService.GetListHotelOfAdminByCustId(string.Empty, 1, int.MaxValue, out totalRecord, MySession.CustID);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.UsersList = guests;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchTypeKey;
            //ViewBag.Hotels = hotels;
            ViewBag.Title = "concierge";
            return PartialView("_GuestListResults");
        }

        [HttpPost]
        public ActionResult CheckExistedCust(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { re = 1 }, JsonRequestBehavior.AllowGet);
            }
            var cus = _custService.GetUsersByUserID(email);
            if (cus != null)
            {
                return Json(new { re = 2 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { re = 3 }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddGuest(
            string firstName,
            string lastName,
            string checkin,
            string checkout,
            string roomNumber,
            string email,
            string cellphone)
        {
            //check email 

            if (string.IsNullOrEmpty(email))
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
            string format = DATE_TIME_FORMAT;
            var cin = checkin.ToDateTime(format);
            var cout = checkout.ToDateTime(format);
            var cus = _custService.GetUsersByUserID(email);
            var typeSearch = cout.Date < DateTime.Now.Date ? 1 : -1;
            var _hotelId = GetHotelId(MySession.CustID);
            if (cus != null) //Existed -> update
            {
                //update customer
                cus.LastName = lastName;
                cus.FirstName = firstName;
                cus.MobilePhone = cellphone;
                _custService.UpdateCustomer(cus);

                //update hotel visite
                var guest = _hotelVisitService.GetHotelVisitByCusId(cus.CustID);
                if (guest == null)
                {
                    guest = new HotelVisit();
                    guest.RoomNumber = roomNumber;
                    guest.CheckInDate = cin;
                    guest.CheckOutDate = cout;
                    guest.CustID = cus.CustID;
                    guest.HotelID = _hotelId;
                    _hotelVisitService.CreateGuest(guest);
                }
                else
                {
                    guest.RoomNumber = roomNumber;
                    guest.CheckInDate = DateTime.Parse(checkin);
                    guest.CheckOutDate = DateTime.Parse(checkout);
                    _hotelVisitService.UpdateHotelVisit(guest);
                }

                //insert or update hotel visit

                return RedirectToAction("GuestList", new { page = 1, key = string.Empty, searchType = typeSearch });
            }
            else //not existed -> insert
            {
                //insert customer
                var newCust = new AddUserModel();
                newCust.UserName = email;
                newCust.LastName = lastName;
                newCust.FirstName = firstName;
                newCust.PhoneNumber = cellphone;
                newCust.Password = "S5mSfs75qh";
                newCust.RoleName = "guest";
                var flag = _membershipService.CreateCustomer(newCust);
                //insert hotel visit
                if (flag) //success insert customer then insert to hotel visit
                {
                    var guest = new HotelVisit();
                    guest.RoomNumber = roomNumber;
                    guest.CheckInDate = cin;
                    guest.CheckOutDate = cout;
                    guest.CustID = newCust.CustID;
                    guest.HotelID = _hotelId;
                    _hotelVisitService.CreateGuest(guest);
                    return RedirectToAction("GuestList", new { page = 1, key = string.Empty, searchType = typeSearch });//Json(new {result = true, resultHTML =  }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = false }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult EditGuest(
            int guestId,
            string firstName,
            string lastName,
            string checkin,
            string checkout,
            string roomNumber,
            string email,
            string cellphone)
        {
            //check email 
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }

            var cin = checkin.ToDateTime(DATE_TIME_FORMAT);
            var cout = checkout.ToDateTime(DATE_TIME_FORMAT);
            var typeSearch = cout < DateTime.Now ? 1 : -1;
            var cus = _custService.GetUsersByUserID(email);
            if (cus != null) //Existed -> update
            {
                cus.LastName = lastName;
                cus.FirstName = firstName;
                cus.MobilePhone = cellphone;
                _custService.UpdateCustomer(cus);

                //update hotel visite
                var guest = _hotelVisitService.GetHotelVisit(guestId);
                guest.RoomNumber = roomNumber;
                guest.CheckInDate = cin;
                guest.CheckOutDate = cout;
                _hotelVisitService.UpdateHotelVisit(guest);
            }
            return RedirectToAction("GuestList", new { page = 1, key = string.Empty, searchType = typeSearch });
        }

        #endregion

        #region appoinment page
        public ActionResult Appointment(int? page, string key, int? type, int? hotelId, int? status)
        {
            if (!AuthorizationConcierge())
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                return ConciergeUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int companyType = type ?? 1;
            // int _hotelId = hotelId ?? 0;
            int appointmentStatus = status ?? (int)Types.AppointmentStatus.Unknown;
            var _hotelId = GetHotelId(MySession.CustID);
            int isAgent = (int)Types.Role.Unknown;

            ViewBag.Appointments = _adminService.GetAppointmentsConcierge(key, companyType, _hotelId, appointmentStatus,
                pageIndex, 10, out totalRecord, isAgent);
            ViewBag.Hotels = _adminService.GetAllHotel();
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.HotelId = _hotelId;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_AppointmentConciergeResults");
            }
            return View();
        }

        #endregion

        #region proposal page
        public ActionResult Proposals(int? pageIndex, string key)
        {
            if (!AuthorizationConcierge())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                else
                {
                    return ConciergeUnauthorizedResult();
                }
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;

            var _hotelId = GetHotelId(MySession.CustID);//hotelId ??2;
            var proAppointments = _appointmentService.GetProposedAppointmentsByHotelId(_hotelId, DateTime.Now, index, 10, key, out totalRecord);
            var apts = new List<ProposedAppointmentModel>();
            foreach (var proposedAppointment in proAppointments)
            {
                var proApp = new ProposedAppointmentModel();
                proApp.Cus = proposedAppointment.Cust;
                proApp.ProfileCompanyName = proposedAppointment.ProfileCompany.Name;
                proApp.Start = proposedAppointment.Start.ToString("MM/dd/yyyy");
                proApp.End = proposedAppointment.End.ToString("MM/dd/yyyy");
                proApp.Duration = proposedAppointment.Duration.HasValue ? proposedAppointment.Duration.Value : 0;
                proApp.Price = proposedAppointment.Price.HasValue ? proposedAppointment.Price.Value : 0;
                proApp.AppointmentID = proposedAppointment.AppointmentID;
                proApp.CustID = proposedAppointment.CustID;
                proApp.CusFullName = proposedAppointment.Cust.FullName;
                proApp.ApptTime = string.Format("{0} {1}", proposedAppointment.Start.ToString("M/d/yy"), proposedAppointment.Start.ToString("h:mm tt"));
                apts.Add(proApp);
            }

            //_appointmentService.GetProposedAppointment(comId, index, 10, out totalRecord);

            ViewBag.ProposedAppointments = apts;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.profileID = _hotelId;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProposalsConciergeResults");
            }
            return View();
        }
        #endregion

        #region method

        private int GetHotelId(int custId)
        {
            var hotelID = -1;
            var hotels = _hotelService.GetHotelStaffByCustId(custId);
            if (hotels == null || !hotels.Any())
            {
                return hotelID;
            }
            var hotelDefault = hotels.SingleOrDefault(a => a.IsDefault.HasValue && a.IsDefault.Value);

            if (hotelDefault != null)
            {
                hotelID = hotelDefault.HotelID;
            }
            else
            {
                var hdefault = hotels.OrderByDescending(a => a.Id).FirstOrDefault();
                if (hdefault != null)
                    hotelID = hdefault.HotelID;
            }
            return hotelID;
        }
        #endregion

        #region impersonal
        public ActionResult GuestImpersonal(int? id, int? formType)
        {
            if (!AuthorizationConcierge())
            {
                return ConciergeUnauthorizedResult();
            }
            try
            {
                MySession.HotelId = GetHotelId(MySession.CustID);
                MySession.ImpersonateId = id;
                MySession.ImpersonatedFrom = formType;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Login with Impersonate is fail: ", ex);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region print agenda
        public ActionResult Agenda(int type)
        {
            var model = new List<Appointment>();
            var _hotelId = GetHotelId(MySession.CustID);
            int appointmentStatus = (int)Types.AppointmentStatus.Unknown;
            int isAgent = (int)Types.Role.Unknown;
            int totalRecord = 0;
            int pageIndex = 1;

            model = _adminService.GetAppointmentsConcierge(string.Empty, type, _hotelId, appointmentStatus,
                pageIndex, 100000, out totalRecord, isAgent);
            return View("AgendaPrint", model);
        }
        #endregion



    }
}
