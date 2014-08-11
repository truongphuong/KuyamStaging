using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Text;
using System.Web.Routing;
using DDay.iCal;
using Kuyam.Domain.Admin;
using Kuyam.Domain.GiftCardServices;
using Kuyam.Domain.MessageServcies;
using Kuyam.Domain.PromoCodeServices;
using Kuyam.Domain.RequestTimeSlotServices;
using Kuyam.Domain.SmsServices;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.WebUI.Models.Company;
using M2.Util;
using System.Web.Security;
using Kuyam.Domain;
using Kuyam.Utility;
using Kuyam.WebUI.Helpers;
using System.IO;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.Database.Extensions;
using Newtonsoft.Json.Converters;
using PayPal.Util;
using RazorEngine;
using Kaltura;
using System.Globalization;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.KuyamServices;
using System.Configuration;
using Kuyam.Domain.HipmobServices;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.Seo;

namespace Kuyam.WebUI.Controllers
{
    public class AdminController : KuyamBaseController
    {
        private readonly AdminService _adminService;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly IBlogCategoryService _categoryService;
        private readonly ICategoryFeaturedService _categoryFeaturedService;
        private readonly IProfileCompanyService _profileCompanyService;
        private readonly IGiftCardServices _giftCardServices;
        private readonly IPromoCodeServices _promoCodeServices;
        private readonly ISmsServices _smsServices;
        private readonly ISMSProvider _smsProvider;
        private readonly IRequestTimeSlotServices _requestTimeSlotServices;
        private readonly IAppointmentService _appointmentService;
        private readonly NotificationService _notificationService;
        private readonly IHipmobService _hipmobService;
        private readonly ImportService _importService;
        private readonly ISeoFriendlyUrlService _seoFriendlyUrlService;
        private readonly ClassService _classService;
        public AdminController(AdminService adminService,
            IFormsAuthenticationService formsService, IMembershipService membershipService,
            CompanyProfileService companyProfileService, IBlogCategoryService categoryService,
            ICategoryFeaturedService categoryFeaturedService,
            IProfileCompanyService profileCompanyService,
            IGiftCardServices giftCardServices,
            IPromoCodeServices promoCodeServices,
            ISmsServices smsServices,
            ISMSProvider smsProvider,
            IRequestTimeSlotServices requestTimeSlotServices,
            IAppointmentService appointmentService,
            NotificationService notificationService,
            IHipmobService hipmobService,
            CompanyProfileService companyProfile,
            ImportService importService,
            ISeoFriendlyUrlService seoFriendlyUrlService,
            ClassService classService)
        {
            this._adminService = adminService;
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._companyProfileService = companyProfileService;
            this._categoryService = categoryService;
            this._categoryFeaturedService = categoryFeaturedService;
            this._profileCompanyService = profileCompanyService;
            this._giftCardServices = giftCardServices;
            this._promoCodeServices = promoCodeServices;
            this._smsServices = smsServices;
            this._smsProvider = smsProvider;
            this._requestTimeSlotServices = requestTimeSlotServices;
            this._appointmentService = appointmentService;
            this._notificationService = notificationService;
            this._hipmobService = hipmobService;
            this._importService = importService;
            this._seoFriendlyUrlService = seoFriendlyUrlService;
            this._classService = classService;
        }

        public bool AuthorizationAdmin()
        {
            bool isLogin = Request.IsAuthenticated;
            bool isAdmin = User.IsInRole("Admin");
            return (isLogin && isAdmin);
        }

        public bool AuthorizationAdminOrAgent()
        {
            bool isLogin = Request.IsAuthenticated;
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Agent");
            return (isLogin && isAdmin);
        }

        private RedirectResult AdminUnauthorizedResult()
        {
            var redirectUrl = Request.RawUrl;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return new RedirectResult("~/Admin/Login?ReturnUrl=" + redirectUrl);
            }
            else
            {
                return new RedirectResult("~/Admin/Login");
            }
        }

        [KuyamAuthorizeAttribute(Roles = "Admin, Agent")]
        public ActionResult Index()
        {
            if (!AuthorizationAdminOrAgent())
            {
                //_formsService.SignOut();
                //Session.Abandon();
                return AdminUnauthorizedResult();
            }
            MyApp.GetPlatformInfo();
            MyApp.GetServerInfo(Request.ServerVariables["SERVER_NAME"]);
            return View();
        }



        public ActionResult ChangePassword(string username, string newPassword)
        {
            MembershipUserCollection users = Membership.FindUsersByName(username);
            if (users != null)
            {
                MembershipUser user = users[username];
                if (user != null)
                {
                    try
                    {
                        if (user.IsLockedOut)
                            user.UnlockUser();
                        user.ChangePassword(user.GetPassword(), newPassword);
                        LogHelper.Info(string.Format("Password change: UserName= {0}", user.UserName));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("Change password fail:", ex);
                    }

                }
            }


            return AdminUnauthorizedResult();
        }


        public ActionResult Login()
        {
            if (AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            return View();
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

            //if (Request.IsAuthenticated)
            //{
            //    return RedirectToAction("Index");
            //}
            ViewBag.IsUsesAccessKey = usesKey;
            Cust customer = DAL.xGetCust(username);
            if (!usesKey.HasValue)
            {
                string[] roles = Roles.GetRolesForUser(username);

                if (!roles.Contains("Admin") && !roles.Contains("Agent"))
                {
                    ModelState.AddModelError("", "your login is not admin. please contact support.");
                }
                else
                {
                    bool validateUser = _membershipService.ValidateUser(username, pass);
                    if (validateUser)
                    {
                        if (customer != null)
                        {
                            if (customer.Status == (int)Types.UserStatusType.Active)
                            {
                                _formsService.SignIn(username, false /* createPersistentCookie */);
                                //change from true to false by default                                
                                string windowsTimeZoneId = DateTimeUltility.TimeZoneToTimeZoneInfo(timeZoneId);
                                customer.TimeZoneId = windowsTimeZoneId;
                                DAL.UpdateRec(customer, customer.CustID);

                                LogHelper.Info("Login admin success with username: " + username);
                                string returnUrl = Request.Params["ReturnUrl"];
                                if (!string.IsNullOrEmpty(returnUrl))
                                    return Redirect(returnUrl);
                                return RedirectToAction("Index", "Admin");
                            }
                        }
                    }
                    ModelState.AddModelError("", "your login is not success. please contact support.");
                }
            }
            else
            {
                string newpass = string.Empty;
                AccessKeyManagement AccessKey = _adminService.GetAccessKeyManagementByUserAndkey(username.Trim(), pass.Trim());

                if (AccessKey == null || customer == null)
                {
                    ModelState.AddModelError("", "your login is not admin. please contact support.");
                }
                else
                {
                    _formsService.SignIn(username, false);
                    string returnurl = EmailHelper.GetStoreHost();
                    string windowsTimeZoneId = DateTimeUltility.TimeZoneToTimeZoneInfo(timeZoneId);
                    customer.TimeZoneId = windowsTimeZoneId;
                    DAL.UpdateRec(customer, customer.CustID);

                    LogHelper.Info("Login admin success with username: " + username);
                    return Redirect(returnurl);
                }
            }
            LogHelper.Info("Login admin fail with username: " + username);
            return View();
        }

        public ActionResult Communication()
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            if (MyApp.Settings.Admin != null)
            {
                ViewBag.EnableEmailBcc = MyApp.Settings.Admin.EnableEmailBcc;
                ViewBag.EnablePhoneBcc = MyApp.Settings.Admin.EnablePhoneBcc;
                ViewBag.EmailBcc = MyApp.Settings.Admin.EmailBcc;
                ViewBag.PhoneNumber = MyApp.Settings.Admin.PhoneNumber;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Communication(string emailaddress, string txtphoneNumber, bool? enableEmailBcc,
            bool? enablePhoneBcc)
        {
            AppSettings.AdminData AdminData = new AppSettings.AdminData
            {
                EnableEmailBcc =
                    enableEmailBcc.HasValue ? enableEmailBcc.Value : false,
                EnablePhoneBcc =
                    enablePhoneBcc.HasValue ? enablePhoneBcc.Value : false,
                EmailBcc = emailaddress,
                PhoneNumber = UtilityHelper.CleanPhone(txtphoneNumber)

            };

            MyApp.Settings.SaveAdminSetting(AdminData);
            if (MyApp.Settings.Admin != null)
            {
                ViewBag.EnableEmailBcc = MyApp.Settings.Admin.EnableEmailBcc;
                ViewBag.EnablePhoneBcc = MyApp.Settings.Admin.EnablePhoneBcc;
                ViewBag.EmailBcc = MyApp.Settings.Admin.EmailBcc;
                ViewBag.PhoneNumber = MyApp.Settings.Admin.PhoneNumber;
            }
            return View();
        }

        public ActionResult AccessKeyManagement()
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            string[] lst = Roles.GetUsersInRole("Admin");
            ViewBag.ListAdmin = lst;
            ViewBag.ListUser = Membership.GetAllUsers();
            string emailAdmin = Request.Params["email"];
            if (lst.Count() > 0 && string.IsNullOrEmpty(emailAdmin))
                emailAdmin = lst[0];
            ViewBag.EmailAdmin = emailAdmin;
            List<AccessKeyManagement> listAccessKey = _adminService.GetAccessKeyManagementByAdminAndkey(emailAdmin);
            ViewBag.ListAccessKey = listAccessKey;
            return View();
        }

        [HttpPost]
        public ActionResult AccessKeyManagement(string emailUser, string emailAdmin)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            ViewBag.ListAdmin = Roles.GetUsersInRole("Admin");
            ViewBag.ListUser = Membership.GetAllUsers();
            ViewBag.EmailUser = emailUser;
            ViewBag.EmailAdmin = emailAdmin;
            string keyhashed = SecurityHelper.EncryptStringToBytesAes(Guid.NewGuid().ToString());
            AccessKeyManagement accessKeyManagement = new AccessKeyManagement
            {
                Key = keyhashed,
                EmailAdmin = emailAdmin,
                EmailUser = emailUser,
                Active = true
            };
            _adminService.CreateAccessKeyManagement(accessKeyManagement);

            LogHelper.Info(string.Format("Create new access key success for emailUser: {0} & userAdmin: {1}", emailUser,
                emailAdmin));
            List<AccessKeyManagement> listAccessKey = _adminService.GetAccessKeyManagementByAdminAndkey(emailAdmin);
            ViewBag.ListAccessKey = listAccessKey;
            return View();
        }

        [HttpPost]
        public ActionResult DeleteAccessKeyManagement(string emailUser, string emailAdmin, string acessKeyid)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            int id = 0;
            int.TryParse(acessKeyid, out id);
            _adminService.DeleteAccessKeyManagement(id);
            LogHelper.Info("Delete access key success: ID: " + id);
            return RedirectToAction("AccessKeyManagement", new { email = emailAdmin });
        }

        [HttpPost]
        public ActionResult InviteAccessKeyManagement(string emailLogin, string emailAdmin, string acessKeyid)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            string htmlContent = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/accesskey.html")))
            {
                htmlContent = reader.ReadToEnd();
            }
            Cust user = DAL.xGetCust(emailAdmin);
            string body = string.Format(htmlContent, EmailHelper.GetStoreHost(), emailLogin, acessKeyid,
                String.Format("{0:dddd, MMMM d, yyyy}", DateTime.Now), UtilityHelper.TruncateText(user.FirstName, 15),
                emailAdmin);
            EmailHelper.SendEmailImpersonate(emailAdmin, emailLogin, body);
            LogHelper.Info(
                string.Format("Send invite access key success with emailLogin: {0}, emailAdmin: {1} & accessKeyId {2}",
                    emailLogin, emailAdmin, acessKeyid));
            return RedirectToAction("AccessKeyManagement", new { email = emailAdmin });
        }



        #region Admin User

        [KuyamAuthorizeAttribute(Roles = "Admin, Agent")]
        public ActionResult UserList(int? page, string key, int? searchType)
        {
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int searchTypeKey = searchType ?? 1;

            bool isAdmin = User.IsInRole("Admin");
            string stringRole = "Personal";
            if (isAdmin)
                stringRole = "Admin";

            List<Cust> users = _adminService.AdminGetListUserByKeyName(key, pageIndex, 10, out totalRecord,
                searchTypeKey, stringRole);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.UsersList = users;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchTypeKey;
            return View();
        }

        [HttpPost]
        public ActionResult UserList(string page, string key, int searchType)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int pageIndex = 0;
            int totalRecord = 0;
            int.TryParse(page, out pageIndex);

            List<Cust> users = new List<Cust>();

            bool isAdmin = User.IsInRole("Admin");
            string stringRole = "Personal";
            if (isAdmin)
                stringRole = "Admin";

            users = _adminService.AdminGetListUserByKeyName(key, pageIndex, 10, out totalRecord, searchType, stringRole);

            ViewBag.Page = pageIndex;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.UsersList = users;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            return PartialView("_UserListResults");
        }

        [HttpPost]
        public ActionResult AdminSearchUsers(string key, int searchType)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            bool isAdmin = User.IsInRole("Admin");
            string stringRole = "Personal";
            if (isAdmin)
                stringRole = "Admin";
            int totalRecord = 0;
            List<Cust> users = _adminService.AdminGetListUserByKeyName(key, 1, 10, out totalRecord, searchType,
                stringRole);

            ViewBag.Page = 1;
            ViewBag.Key = key;
            ViewBag.UsersList = users;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.SearchType = searchType;
            return PartialView("_UserListResults");
        }

        [HttpPost]
        public ActionResult ChangeCompanyType(int companyId, int companyType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            bool result = _adminService.ChangeCompanyTypeByCompanyId(companyId, companyType);
            return Json(result);

        }

        //Trong Added
        public ActionResult ImpersonateUser(string email)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            try
            {
                var customer = DAL.xGetCust(email);
                MySession.ImpersonateId = customer.CustID;
                MySession.ImpersonatedFrom = 1;
                MySession.HotelId = 2;

            }
            catch (Exception ex)
            {
                LogHelper.Error("Login with Impersonate is fail: ", ex);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion

        public ActionResult ResetPasswordConfirm(string email, int page, string key)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            try
            {
                string newPass = ControllerUtil.ResetPassword(email);
                string htmlContent = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/resetpassword.html")))
                {
                    htmlContent = reader.ReadToEnd();
                }
                Cust user = DAL.xGetCust(email);
                string body = string.Format(htmlContent, EmailHelper.GetStoreHost(), newPass,
                    String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                    UtilityHelper.TruncateText(user.FirstName, 15), email);
                EmailHelper.SendEmailAdminResetPassword(email, body);
                LogHelper.Info("Reset password success with username: " + email);
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Empty, ex);
                ModelState.AddModelError("", ex);
            }
            return RedirectToAction("UserList", "Admin", new { page = page, key = key });
        }

        public ActionResult SendEmailVerifiedCodeCompany(string profileId)
        {

            bool resutl = false;
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            try
            {

                int id = -1;
                int.TryParse(profileId, out id);
                ProfileCompany profiles = _companyProfileService.GetProfileCompanyByID(id);

                profiles.CompanyStatusID = (int)Types.CompanyStatus.Active;
                _companyProfileService.UpdateProfileCompany(profiles);
                //Send email

                string template30DayTrialBeginResult = string.Empty;
                string template30daysoverResult = string.Empty;
                //30daysover
                try
                {
                    string template = string.Empty;
                    using (
                        System.IO.StreamReader reader =
                            new System.IO.StreamReader(Server.MapPath("~/Templates/30daysover.cshtml")))
                    {
                        template = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = profiles.Name, //  MySession.Cust.FirstName.ToString(),
                        Date =
                            String.Format("{0:dddd, MMMM d, yyyy}",
                                DateTimeUltility.ConvertToUserTime(DateTime.UtcNow)),
                        Email = profiles.Email // MySession.Username
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
                    using (
                        System.IO.StreamReader reader =
                            new System.IO.StreamReader(Server.MapPath("~/Templates/30daytrialbegin.cshtml")))
                    {
                        template30DayTrialBegin = reader.ReadToEnd();
                    }
                    dynamic myObject = new
                    {
                        UserName = profiles.Profile.Cust.FirstName,
                        // MySession.Cust.FirstName.ToString(),
                        Date =
                            String.Format("{0:dddd, MMMM d, yyyy}",
                                DateTimeUltility.ConvertToPstTime(DateTime.UtcNow)),
                        Email = profiles.Profile.Cust.aspnet_Users.UserName // MySession.Username
                    };
                    template30DayTrialBeginResult = RazorEngine.Razor.Parse(template30DayTrialBegin, myObject);
                }
                catch
                {
                    throw;
                }
                EmailHelper.SendEmailCompany30DayTrialBeginCode(profiles.Profile.Cust.aspnet_Users.UserName,
                    template30DayTrialBeginResult);

                string emailBcc = string.Empty;
                if (MyApp.Settings.Admin.EnableEmailBcc)
                    emailBcc = MyApp.Settings.Admin.EmailBcc;
                InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                IncomingRequest obj = new IncomingRequest
                {
                    EntityId = "KuyamWeb",
                    DateAlert = DateTime.UtcNow.AddDays(29),
                    Data =
                        Kuyam.Domain.UtilityHelper.ObjectToXml(
                            new
                            {
                                Emailtemplate = template30daysoverResult,
                                Emailto = profiles.Email,
                                EmailBcc = emailBcc,
                                Subject = "30 Days Over"
                            })
                };
                serviceInfo.AddIncomingRequest(obj, IncommingRequestType.SEND_EMAIL);

                //CompanyProfileService _companyProfileService = Kuyam.Repository.Infrastructure.EngineContext.Current.Resolve<CompanyProfileService>();

                //ProfileCompany profileCompany = _companyProfileService.GetProfileCompanyByID(int.Parse(profileId));

                //string email = profileCompany.Email;
                //string inviteCode = _companyProfileService.AddInviteCode(int.Parse(profileId), email);

                //string template = string.Empty;
                //using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/verifiedcompany.cshtml"))){

                //    template = reader.ReadToEnd();
                //}
                //dynamic myObject = new{
                //    UserName = profileCompany.Name,
                //    Verified = inviteCode,
                //    Email = email
                //};
                //string templateResult = RazorEngine.Razor.Parse(template, myObject);
                //EmailHelper.SendEmailCompanySetupVerificationCode(email, templateResult);

                StringBuilder body = new StringBuilder();
                string subject = string.Format("{0}", "Your company is live!");
                string url = string.Format("{0}",
                    Kuyam.WebUI.Helpers.EmailHelper.GetStoreHost() + "/companyprofile/availability/" +
                    profiles.ProfileID);
                if (profiles.Profile != null && profiles.Profile.Cust != null)
                {
                    body.AppendFormat("hi {0} !<br/>", profiles.Profile.Cust.FirstName);
                }
                body.AppendFormat(
                    "we have verified your company information for {0}! you can now list your services and hours<br/>",
                    profiles.Name);
                body.AppendFormat("{0}", "on the kuyam website.<br/>");
                body.AppendFormat("click the following link now to start:<a href='{0}'>{0}</a><br/>", url);
                EmailHelper.SendEmailChangedStatus(string.Empty, profiles.Email, body.ToString(), subject);

                resutl = true;
            }
            catch (Exception ex)
            {
                resutl = false;
                ModelState.AddModelError("", ex);
            }
            return Json(resutl);
        }

        [HttpPost]
        public ActionResult ChangeCompanyStatus(string profileId, int status)
        {
            bool resutl = false;
            int id = -1;
            int.TryParse(profileId, out id);
            try
            {

                ProfileCompany profiles = _companyProfileService.GetProfileCompanyByID(id);

                profiles.CompanyStatusID = status;
                _companyProfileService.UpdateProfileCompany(profiles);

                resutl = true;
            }
            catch
            {
                resutl = false;
            }
            return Json(resutl);
        }

        public ActionResult AjaxSearchCompanies(string key)
        {
            List<ProfileCompany> lstCompany = _adminService.AdminGetListCompany(key, 100);
            var results = lstCompany.Select(c => new CompanyAjaxSearchModel(c));
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AdminCompanyList(int? page, string key, int? searchType, int? companyType)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int seachType1 = searchType ?? -1;
            int page1 = page ?? 1;

            List<ProfileCompany> lstCompany = new List<ProfileCompany>();

            if (companyType.HasValue && companyType > -1)
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, seachType1, companyType.Value, page1, 10,
                out totalRecord);
            }
            else
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, seachType1, page1, 10,
                out totalRecord);
            }


            ViewBag.Page = page1;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.IsAdmin = AuthorizationAdmin();
            return View();
        }

        [HttpPost]
        public ActionResult AdminCompanyList(string page, string key, int searchType, int companyType)
        {
            //Check Authorization
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 0;
            int.TryParse(page, out pageIndex);
            int totalRecord = 0;
            List<ProfileCompany> lstCompany = new List<ProfileCompany>();

            if (companyType > -1)
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType, companyType, pageIndex, 10, out totalRecord);
            }
            else
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType, pageIndex, 10, out totalRecord);

            }


            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.Key = key;
            ViewBag.Page = pageIndex;
            ViewBag.SearchType = searchType;

            return PartialView("_CompanyListResults");
        }

        [HttpPost]
        public ActionResult AdminCompayAddExtend(string profileid, string day)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            bool resutl = _adminService.CompayAddDayExtend(int.Parse(profileid), int.Parse(day));

            if (resutl == false)
            {
                return Json(resutl);
            }
            return Json(resutl);
        }

        [HttpPost]
        public ActionResult AdminCompayAddReduce(string profileid, string day)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }

            bool resutl = _adminService.CompayAddDayExtend(int.Parse(profileid), -int.Parse(day));

            if (resutl == false)
            {
                return Json(resutl);
            }
            return RedirectToAction("AdminCompanyList", "Admin");
        }

        [HttpPost]
        public ActionResult ChangeCompanyName(int profileId, string name)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            var company = _companyProfileService.GetProfileCompanyByID(profileId);
            if (company == null)
                return Json(new { status = false, name = string.Empty });
            company.Name = name;
            _companyProfileService.UpdateProfileCompany(company);
            string slug = string.Format("{0} {1} {2} {3}", company.Name, company.City, company.State, company.Street1);
            _seoFriendlyUrlService.SaveSlug(company.ProfileID, slug, "company");
            return Json(new { status = true, name = company.Name });
        }

        [HttpPost]
        public ActionResult AdminSearchCompanies(string key, int searchType, int companyType)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            int totalRecord = 0;
            ViewBag.Page = 1;
            ViewBag.Key = key;

            List<ProfileCompany> lstCompany = new List<ProfileCompany>();
            if (companyType > -1)
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType, companyType, 1, 10,
                                out totalRecord);
            }
            else
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType, 1, 10,
                                out totalRecord);
            }


            ViewBag.CompanyList = lstCompany;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.SearchType = searchType;
            return PartialView("_CompanyListResults");
        }

        public ActionResult AdminCompanyDetail()
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            string companyID = Request.Params["companyID"].ToString();
            int userListPageIndex = int.Parse(Request.Params["page"].ToString());
            string key = Request.Params["key"].ToString();
            int searchType = int.Parse(Request.Params["searchType"].ToString());
            int id = 0;
            int.TryParse(companyID, out id);
            var profileComapany = ProfileCompany.GetProfileCompany(id);
            ViewBag.CompanyDetail = profileComapany;
            ViewBag.ProfileDetail = profileComapany.Profile;
            ViewBag.UserListPageIndex = userListPageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.IsShowCategory = profileComapany.IsShowCatagory;
            ViewBag.IsEvent = profileComapany.IsEvent;
            ViewBag.IsClass = profileComapany.IsClass;
            return View();
        }

        public ActionResult AdminEditFeaturedCompaniesAtHomePage()
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            ViewBag.AdminEditFeatureCompanies = _adminService.AdminGetListFeaturedCompanies();
            ViewBag.AdminAllCompanies = _adminService.AdminGetListCompany();
            return View();
        }

        public ActionResult AdminUpdateFeaturedCompaniesAtHomePage(string listSelected, string priority, int featureType)
        {
            bool result = false;

            int profileID = 0;
            int index = 0;

            int.TryParse(listSelected, out profileID);
            int.TryParse(priority, out index);

            bool isFeatured = _adminService.CheckFeatured(index);
            bool isProfile = _adminService.CheckFeaturedProfileId(profileID);

            if (profileID == 0)
            {
                result = _adminService.AdminDelateCompanyFeatured(index);
            }
            else if (isFeatured == true)
            {
                result = _adminService.AdminUpdateCompanyFeatured(profileID, index, featureType);

            }
            else if (isFeatured == false && isProfile == false)
            {
                result = _adminService.AdminAddCompanyFeatured(profileID, index, featureType);
            }

            ViewBag.AdminEditFeatureCompanies = _adminService.AdminGetListFeaturedCompanies();
            ViewBag.AdminAllCompanies = _adminService.AdminGetListCompany();

            return Json(result);
        }

        #region Pending List User

        public ActionResult PendingUsers(int? page, string key, int? searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int searchTypeKey = searchType ?? (int)Types.UserInviteCodeStatusType.Pending;
            List<Invite> pendings = _adminService.GetAllPendingUsers(key, searchTypeKey, pageIndex, 10, out totalRecord);
            if (pendings.Count <= 0 && pageIndex > 1)
            {
                pageIndex = pageIndex - 1;
                pendings = _adminService.GetAllPendingUsers(key, searchTypeKey, pageIndex, 10, out totalRecord);
            }

            ViewBag.PendingList = pendings;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchTypeKey;

            return View();
        }

        [HttpPost]
        public ActionResult PendingUsers(string page, string key, int searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int pageIndex = 0;
            int totalRecord = 0;
            int.TryParse(page, out pageIndex);

            ViewBag.Page = pageIndex;
            ViewBag.PendingList = _adminService.GetAllPendingUsers(key, searchType, pageIndex, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;

            return PartialView("_PendingUserResults");
        }

        public ActionResult SearchPendingUsers(string key, int searchType)
        {

            if (!AuthorizationAdmin())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            int totalRecord = 0;

            ViewBag.Page = 1;
            ViewBag.Key = key;
            ViewBag.PendingList = _adminService.GetAllPendingUsers(key, searchType, 1, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.SearchType = searchType;
            return PartialView("_PendingUserResults");
        }

        public ActionResult DenyInviteCode(int id, int page, string key, int searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            //Kuyam.Database.Invite inviteCode = _adminService.GetInviteCodeByEmail(id);
            //Update invite code
            _adminService.UpdateInviteStatus(id, (int)Types.UserInviteCodeStatusType.Denied);

            return RedirectToAction("PendingUsers", "Admin", new { page = page, key = key, searchType = searchType });
        }

        public ActionResult ApproveInviteCode(int id, int page, string key, int searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            Kuyam.Database.Invite inviteCode = _adminService.GetInviteCodeById(id);

            if (inviteCode != null)
            {

                //Update invite code
                _adminService.UpdateInviteStatus(id, (int)Types.UserInviteCodeStatusType.Approved);
                try
                {

                    string templateInvitecode = string.Empty;
                    using (
                        System.IO.StreamReader reader =
                            new System.IO.StreamReader(Server.MapPath("~/Templates/invitecode.cshtml")))
                    {
                        templateInvitecode = reader.ReadToEnd();
                    }

                    // create template data
                    dynamic myObject = new
                    {

                        Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                        Host = EmailHelper.GetStoreHost(),
                        Email = inviteCode.Email,
                        InviteCode = inviteCode.Key,
                        UserName = inviteCode.Name.ToString()
                    };

                    // generate the content using razor engine
                    string templateResultInvitecode = RazorEngine.Razor.Parse(templateInvitecode, myObject);

                    System.Threading.Thread oThreadInvitecode =
                        new System.Threading.Thread(
                            () => EmailHelper.SendEmailInviteCodeSignUp(inviteCode.Email, templateResultInvitecode));
                    oThreadInvitecode.Start();

                    string message = string.Format("Send email invite code: Email={0},UserName={1}, InviteCode={2}",
                        inviteCode.Email, inviteCode.Name, inviteCode.Key);
                    LogHelper.Info(message);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Send email invite code fail:", ex);
                    ModelState.AddModelError("", ex);
                }
            }

            return RedirectToAction("PendingUsers", "Admin", new { page = page, key = key, searchType = searchType });
        }

        [KuyamAuthorizeAttribute(Roles = "Admin")]
        public ActionResult AddUser()
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            var roles = _adminService.GetAllRoles();
            var hotels = _adminService.GetListHotelByAll();
            var model = new AddUserModel
            {
                AllRoles = roles,
                HotelList = hotels

            };
            return View(model);
        }

        [KuyamAuthorizeAttribute(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddUser(AddUserModel model)
        {
            model.AllRoles = _adminService.GetAllRoles();
            model.HotelList = _adminService.GetListHotelByAll();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.HotelId.HasValue && model.HotelId > 0)
            {
                model.RoleName = "Concierge";
            }
            bool issuccess = _membershipService.CreateCustomer(model);
            if (!issuccess)
            {
                ModelState.AddModelError("existUser", "this user exist in system");
                return View(model);
            }
            if (model.HotelId.HasValue && model.HotelId > 0)
            {
                var staff = new HotelStaff
                {
                    CustID = model.CustID,
                    HotelID = model.HotelId.Value,
                    IsDefault = true,
                    CreateDate = DateTime.UtcNow
                };
                _adminService.InsertHotelStaff(staff);
            }

            ViewBag.ShowMessage = MvcHtmlString.Create("ShowMessage();");
            return View(model);
        }

        public ActionResult AdminUserDetail(int id, int userListPageIndex, string key, string searchType, string backAction = "UserList")
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            var cust = _adminService.GetUserById(id);
            cust.HasDevice = _notificationService.GetAllUserDevice().Any(m => m.CustId == id);
            ViewBag.UserDetail = _adminService.GetUserById(id);
            ViewBag.UserListPageIndex = userListPageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.Action = backAction;
            var listSms = _smsServices.GetAllMessage(id);
            ViewBag.Messages = listSms;
            ViewBag.TotalMessage = listSms.Count();
            ViewBag.CustId = id;
            return View();
        }



        public ActionResult ChangeUserStatus(int id)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        public ActionResult ChangeUserStatus(int id, int status)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            bool result = _adminService.ChangeUserStatus(id, status);
            return Json(result);
        }

        public ActionResult LockedUsers(int? page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;

            List<LockedUser> lockedUsers = _adminService.GetAllLockedUsers(key, pageIndex, 10, out totalRecord);
            if (lockedUsers.Count <= 0 && pageIndex > 1)
            {
                pageIndex = pageIndex - 1;
                lockedUsers = _adminService.GetAllLockedUsers(key, pageIndex, 10, out totalRecord);
            }

            ViewBag.LockedUsersList = lockedUsers;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            return View();
        }

        public ActionResult UnlockUser(Guid id, int page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            aspnet_Membership membershipUser = _adminService.UnLockUserById(id);

            return RedirectToAction("LockedUsers", "Admin", new { page = page, key = key });
        }

        [HttpPost]
        public ActionResult SearchLockedUsers(string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;

            ViewBag.Page = 1;
            ViewBag.Key = key;
            ViewBag.LockedUsersList = _adminService.GetAllLockedUsers(key, 1, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            return PartialView("_LockedUserResults");
        }

        public ActionResult ZipCodes(int? page, string key, int? searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            if (key == null || key == "\'\'") key = "";
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            searchType = searchType == null ? -1 : searchType.Value;

            List<ZipCode> zipCodes = _adminService.SearchZipCodes(key, searchType, pageIndex, 10, out totalRecord);
            if (zipCodes.Count <= 0 && pageIndex > 1)
            {
                pageIndex = pageIndex - 1;
                zipCodes = _adminService.SearchZipCodes(key, searchType, pageIndex, 10, out totalRecord);
            }

            ViewBag.ZipCodesList = zipCodes;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            return View();
        }

        [HttpPost]
        public ActionResult ZipCodesPaging(int? page, string key, int? searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            if (key == null) key = "";
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            searchType = searchType ?? -1;

            List<ZipCode> zipCodes = _adminService.SearchZipCodes(key, searchType, pageIndex, 10, out totalRecord);
            if (zipCodes.Count <= 0 && pageIndex > 1)
            {
                pageIndex = pageIndex - 1;
                zipCodes = _adminService.SearchZipCodes(key, searchType, pageIndex, 10, out totalRecord);
            }

            ViewBag.ZipCodesList = zipCodes;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType.Value;
            return PartialView("_ZipCodesResult");
        }

        public ActionResult ZipCodeDetail(int id, int? page, string key, int? searchType)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            if (key == null) key = "";
            int pageIndex = page ?? 1;
            searchType = searchType ?? -1;

            ZipCode zipCode = _adminService.GetZipCodeById(id);
            if (zipCode == null) zipCode = new ZipCode();

            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType.Value;
            ViewBag.States = _adminService.GetAllState();
            return View("ZipCodeDetail", zipCode);
        }

        [HttpPost]
        public ActionResult SaveZipCode(ZipCode model, int? page, string key, int? searchType, bool? isAddMode,
            int oldZipCodeId)
        {
            ViewBag.pageIndex = page.Value;
            ViewBag.Key = key;
            searchType = searchType ?? -1;
            ViewBag.SearchType = searchType;
            ViewBag.States = _adminService.GetAllState();
            //Validate model state

            if (model.Code == "" || model.Code == null)
            {
                ModelState.AddModelError("Code", "zipcode is required");
            }
            if (model.State == "" || model.State == null)
            {
                ModelState.AddModelError("State", "state is required");
            }
            if (model.City == "" || model.City == null)
            {
                ModelState.AddModelError("City", "city is required");
            }

            if (!String.IsNullOrEmpty(model.Code) && !model.Code.All(c => char.IsDigit(c)))
            {
                ModelState.AddModelError("Code", "zipcode must be digit only");
            }

            if (!String.IsNullOrEmpty(model.Code) && model.Code.All(c => char.IsDigit(c)))
            {
                if (model.Code.Length != 5)
                {
                    ModelState.AddModelError("Code", "the zipcode length must be 5 digits");
                }
            }

            bool isZipCodeExist = _adminService.IsZipCodeExisted(model);

            if (isZipCodeExist) // The zipCode already exist in database
            {
                ModelState.AddModelError("Code", "the zipcode already exists. please choose another one");
            }
            if (ModelState.IsValid)
            {
                _adminService.SaveZipCode(model, isAddMode.Value, oldZipCodeId);

                return RedirectToAction("ZipCodeDetail", new { id = model.ZipCodeId, page = page.Value, key = key });
            }
            else
            {
                ZipCode zipcode = null;
                if (isAddMode.Value)
                {
                    zipcode = new ZipCode();
                    return View("ZipCodeDetail", zipcode);
                }
                else
                {
                    return View("ZipCodeDetail", model);
                }
            }
        }

        public ActionResult DeleteZipCodeById(int id, int? page, string key)
        {
            bool hasError = false;
            hasError = _adminService.DeleteZipCodeById(id);

            return RedirectToAction("ZipCodes", new { id = id, page = page.Value, key = key });
        }

        [HttpPost]
        public ActionResult ChangeZipcodeStatus(int id, bool status, int page, string key, int searchType)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            bool result = _adminService.ChangeZipcodeStatus(id, status);
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            return Json(result);
        }

        [HttpPost]
        public ActionResult SearchZipCodes(string key, int searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;

            List<ZipCode> zipCodes = _adminService.SearchZipCodes(key, searchType, 1, 10, out totalRecord);

            ViewBag.Page = 1;
            ViewBag.Key = key;
            ViewBag.ZipCodesList = zipCodes;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.SearchType = searchType;
            return PartialView("_ZipCodesResult");
        }

        #endregion

        #region category and service

        #region category

        public ActionResult Category(int? page, string key, int? type)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int cType = type ?? 0;
            if (cType < 0)
                cType = 0;
            if (pageIndex < 1)
                pageIndex = 1;

            List<Service> data = _adminService.AdminGetListServiceByName(key, pageIndex, 10, out totalRecord, cType);

            int tmp;
            ViewBag.Categories = _adminService.AdminGetListServiceByName("", 1, 10000, out tmp, 1);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return View();
        }

        [HttpPost, ActionName("Category")]
        public ActionResult CategoryPost(int? page, string key, int? type)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int pageIndex = page ?? 1;
            int totalRecord = 0;
            int cType = type ?? 0;
            if (cType < 0)
                cType = 0;
            if (pageIndex < 1)
                pageIndex = 1;

            List<Service> data; // = new List<Service>();

            data = _adminService.AdminGetListServiceByName(key, pageIndex, 10, out totalRecord, cType);

            ViewBag.Page = pageIndex;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Key = key;
            return PartialView("_ServiceResult");
        }

        [HttpPost]
        public ActionResult CategoryCreate(Service model)
        {
            if (model.ServiceName.Trim() == "")
                return Json(new
                {
                    status = false,
                    message = "please input service name."
                });

            try
            {
                _adminService.CreateService(model);
                string slug = model.ServiceName;
                _seoFriendlyUrlService.SaveSlug(model.ServiceID, slug, "category");
                LogHelper.Info(string.Format("Created service: ServiceId= {0}", model.ServiceID));
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }
            catch (Exception er)
            {
                LogHelper.Error("Create service fail: ", er);
                return Json(new
                {
                    status = false,
                    message =
                        "Failed to create " + (model.ParentServiceID.HasValue ? "service." : "category."),
                    error = er.ToString()
                });
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult CategoryModify(Service model)
        {
            if (model.ServiceName.Trim() == "")
                return Json(new
                {
                    status = false,
                    message = "please input name."
                });

            if (model.ParentServiceID.HasValue && model.ParentServiceID.Value == model.ServiceID)
                return Json(new
                {
                    status = false,
                    message = "the parent could not be the same as the current."
                });

            try
            {
                _adminService.ModifyService(model);
                string slug = model.ServiceName;
                _seoFriendlyUrlService.SaveSlug(model.ServiceID, slug, "category");
                LogHelper.Info(string.Format("Updated category: ServiceID= {0}", model.ServiceID));
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }
            catch (Exception er)
            {
                LogHelper.Error("Update category fail: ", er);
                return Json(new
                {
                    status = false,
                    message =
                        "Failed to modify " + (model.ParentServiceID.HasValue ? "service." : "category."),
                    error = er.ToString()
                });
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult CategoryDelete(int serviceId)
        {
            try
            {
                _adminService.DeleteService(serviceId);
                LogHelper.Info(string.Format("Deleted service: ServiceId= {0}", serviceId));
            }
            catch (InvalidDataException ide)
            {

                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }
            catch (Exception er)
            {
                return Json(new
                {
                    status = false,
                    message = "Failed to delete this item.",
                    error = er.ToString()
                });
            }

            return Json(new { status = true });
        }

        public ActionResult ListServicesByCategory(int categoryId)
        {
            try
            {
                var lstServices = _adminService.ListServicesByCategory(categoryId);
                return Json(new
                {
                    status = true,
                    data = lstServices
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception er)
            {
                LogHelper.Error("Get list services by category fail: ", er);
                return Json(new
                {
                    status = false,
                    error = er.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion

        #region invite

        public ActionResult Invite(int? page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;

            if (pageIndex < 1)
                pageIndex = 1;

            List<Invite> data = _adminService.AdminGetListInvites(key, pageIndex, 10, out totalRecord);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            return View();
        }

        [HttpPost, ActionName("Invite")]
        public ActionResult InvitePost(int? page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int pageIndex = page ?? 1;
            int totalRecord = 0;
            if (pageIndex < 1)
                pageIndex = 1;

            List<Invite> data; // = new List<Service>();

            data = _adminService.AdminGetListInvites(key, pageIndex, 10, out totalRecord);

            ViewBag.Page = pageIndex;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Key = key;
            return PartialView("_InviteResult");
        }

        #endregion

        #region Appointments

        public ActionResult Appointment(int? page, string key, int? type, int? hotelId, int? status)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            int companyType = type ?? (int)Types.CompanyType.Unknown;
            int _hotelId = hotelId ?? 0;
            int appointmentStatus = status ?? (int)Types.AppointmentStatus.Pending;

            int isAgent = (int)Types.Role.Unknown;
            if (User.IsInRole("Admin"))
            {
                isAgent = (int)Types.Role.Admin;
            }


            ViewBag.Appointments = _adminService.GetAppointments(key, companyType, _hotelId, appointmentStatus,
                pageIndex, 10, out totalRecord, isAgent);
            ViewBag.Hotels = _adminService.GetAllHotel();
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = pageIndex;
            ViewBag.HotelId = _hotelId;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_AppointmentAdminListResults");
            }
            return View();
        }

        public ActionResult NonKuyamBookList(int? companyId, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;
            int _companyId = companyId ?? 0;

            ViewBag.Companys = _companyProfileService.GetAllProfileCompany((int)Types.CompanyType.NonKuyamBookIt);
            var appointmentlist = _adminService.GetlistNoAppointment(_companyId, index, 10, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.profileID = _companyId;

            var models = appointmentlist.Select(m => new NokuyamAppointmentListModel
            {
                AppointmentID = m.AppointmentID,
                ProfileId = m.ProfileId,
                ProfileName = m.ProfileCompany.Name,
                CalendarName = m.Calendar.Name,
                ServiceName = m.Service.ServiceName,
                Start = m.Start,
                End = m.End,
                Date = m.Start,
                Price = m.Price ?? 0,
                EmployeeName = m.EmployeeName,
                Duration = m.Duration ?? 0,
                Description = m.Description
            }).ToList();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_NokuyamBookList", models);
            }
            return View(models);
        }

        public ActionResult NonKuyamBookAdd()
        {
            var model = new NokuyamAppointmentModel();

            List<CustExtension> cust = _adminService.GetAllUser();
            model.ProfileCompanys = _companyProfileService.GetAllProfileCompany((int)Types.CompanyType.NonKuyamBookIt);
            model.CustList = cust;
            int custId = cust.FirstOrDefault().CustID;
            model.CalendarList = _companyProfileService.GetCalendarByCustId(custId);
            var hotels = _adminService.GetListHotelByAll();
            model.HotelList = hotels;
            model.ServiceList = _adminService.AdminGetListServiceAll();
            return View(model);
        }

        [HttpPost]
        public ActionResult NonKuyamBookAdd(NokuyamAppointmentModel model)
        {
            string startDate = string.Format("{0} {1}", model.Date, model.Start);
            DateTime start = DateTime.ParseExact(startDate, "MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);
            DateTime end = start.AddMinutes(model.Duration);
            var custs = _adminService.GetAllUser();
            model.ProfileCompanys = _companyProfileService.GetAllProfileCompany((int)Types.CompanyType.NonKuyamBookIt);
            model.CustList = custs;
            int custId = custs.FirstOrDefault().CustID;
            model.CalendarList = _companyProfileService.GetCalendarByCustId(custId);
            var services = _adminService.AdminGetListServiceAll();
            var service = services.Where(m => m.ServiceID == model.ServiceId).FirstOrDefault();
            model.ServiceList = services;
            var entity = new NonKuyamAppointment
            {
                CustID = model.CustID,
                CalendarId = model.CalendarId,
                ProfileId = model.ProfileId,
                ServiceId = model.ServiceId,
                ServiceName = service != null ? service.ServiceName : string.Empty,
                Start = start,
                End = end,
                Description = model.Description,
                Duration = model.Duration,
                EmployeeName = model.EmployeeName,
                Price = model.Price,
                AttendeesNumber = 1,
                Modified = DateTime.UtcNow,
                StatusChangeDate = DateTime.UtcNow,
                Created = DateTime.UtcNow
            };
            if (model.HotelId > 0 && model.ConciergeId > 0)
            {
                entity.HotelID = model.HotelId;
                entity.StaffID = model.ConciergeId;
            }
            _adminService.InsertAppointment(entity);
            return RedirectToAction("NonKuyamBookList");
        }

        public ActionResult NonKuyamBookEdit(int Id)
        {
            var appt = _adminService.GetNoAppointment(Id);
            var model = new NokuyamAppointmentModel();
            if (appt != null)
            {
                List<CustExtension> cust = _adminService.GetAllUser();
                model.ProfileCompanys = _companyProfileService.GetAllProfileCompany((int)Types.CompanyType.NonKuyamBookIt);
                model.AppointmentID = appt.AppointmentID;
                model.ProfileId = appt.ProfileId;
                model.ServiceId = appt.ServiceId;
                model.CalendarId = appt.CalendarId;
                model.CustID = appt.CustID;
                model.CustList = _adminService.GetAllUser();
                model.CalendarList = _companyProfileService.GetCalendarByCustId(appt.CustID);
                model.ServiceList = _adminService.AdminGetListServiceAll();
                model.EmployeeName = appt.EmployeeName;
                model.Duration = appt.Duration ?? 0;
                model.Description = appt.Description;
                model.Date = appt.Start.ToString("MM/dd/yy");
                model.Start = appt.Start.ToString("hh:mm tt");
                model.End = appt.End.ToString("hh:mm tt");
                model.Price = appt.Price ?? 0;
                var hotels = _adminService.GetListHotelByAll();
                model.HotelId = appt.HotelID;
                model.HotelList = hotels;
                if (appt.HotelID.HasValue)
                {
                    model.ConciergeId = appt.StaffID;
                    var concierges = _adminService.GetConciergeByHotelId(appt.HotelID ?? 0);
                    model.ConciergeList = concierges.Select(m =>
                        new StaffModel
                        {
                            HotelID = m.HotelID,
                            StaffID = m.Id,
                            ConciergeName = m.Cust.FirstName

                        }).ToList();
                }

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult NonKuyamBookEdit(NokuyamAppointmentModel model)
        {
            string startDate = string.Format("{0} {1}", model.Date, model.Start);
            DateTime start = DateTime.ParseExact(startDate, "MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);
            DateTime end = start.AddMinutes(model.Duration);
            var appt = _adminService.GetNoAppointment(model.AppointmentID);
            var services = _adminService.AdminGetListServiceAll();
            var service = services.Where(m => m.ServiceID == model.ServiceId).FirstOrDefault();

            appt.ProfileId = model.ProfileId;
            appt.CustID = model.CustID;
            appt.ServiceId = model.ServiceId;
            appt.ServiceName = service.ServiceName;
            appt.CalendarId = model.CalendarId;
            appt.Description = model.Description;
            appt.Duration = model.Duration;
            appt.Start = start;
            appt.End = end;
            appt.EmployeeName = model.EmployeeName;
            appt.Price = model.Price;
            if (model.HotelId > 0 && model.ConciergeId > 0)
            {
                appt.HotelID = model.HotelId;
                appt.StaffID = model.ConciergeId;
            }
            _adminService.UpdateAppointment(appt);
            return RedirectToAction("NonKuyamBookList");
        }


        public ActionResult GetCalendarByCustId(int custId)
        {
            StringBuilder html = new StringBuilder();
            var calendar = _companyProfileService.GetCalendarByCustId(custId);
            foreach (var item in calendar)
            {
                html.AppendFormat("<option value=\"{0}\">{1}</option>", item.CalendarID, item.Name);
            }
            return Json(html.ToString(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetConciergeByHotelId(int hotelId)
        {
            StringBuilder html = new StringBuilder();
            var concierge = _adminService.GetConciergeByHotelId(hotelId);
            foreach (var item in concierge)
            {
                html.AppendFormat("<option value=\"{0}\">{1}</option>", item.Id, item.Cust.FirstName);
            }
            return Json(html.ToString(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region News Letter

        public ActionResult NewsLetter(int? page, string key, int? searchType)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            searchType = searchType ?? -1;
            page = page ?? 1;

            List<ProfileCompany> lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType.Value,
                page.Value, 10, out totalRecord);

            ViewBag.Page = page.Value;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            var templates = new List<SelectListItem>();
            templates.Add(new SelectListItem { Text = "select a template", Value = "0" });
            templates.Add(new SelectListItem { Text = "default", Value = "1" });
            ViewBag.EmailTemplates = templates;
            return View();
        }



        [HttpPost]
        public ActionResult NewsLetterCompanySearch(string page, string key, int searchType)
        {
            //Check Authorization
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 0;
            int.TryParse(page, out pageIndex);
            int totalRecord = 0;
            List<ProfileCompany> lstCompany = new List<ProfileCompany>();

            lstCompany = _adminService.AdminGetListCompanyByKeyName(key, searchType, pageIndex, 10, out totalRecord);

            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.Key = key;
            ViewBag.Page = pageIndex;
            ViewBag.SearchType = searchType;

            return PartialView("_CompanyListResultWithCheckbox");
        }


        public ActionResult NewsLetterCust(int? page, string key, int? searchType)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            searchType = searchType ?? 1;
            page = page ?? 1;

            List<Cust> users = _adminService.AdminGetListUserByKeyName(key, page.Value, 10, out totalRecord,
                searchType.Value, "Admin");

            ViewBag.Page = page.Value;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CustList = users;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            var templates = new List<SelectListItem>();
            templates.Add(new SelectListItem { Text = "select a template", Value = "0" });
            templates.Add(new SelectListItem { Text = "default", Value = "1" });
            ViewBag.EmailTemplates = templates;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CustomerListResultWithCheckbox");
            }
            return View();

        }


        /// <summary>
        /// Sends the news letter.
        /// </summary>
        /// <param name="listProfileId">The list profile id.</param>
        /// <param name="emailTemplate">The email template.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendNewsLetter(int[] listProfileId, int emailTemplate, string emailSubject)
        {
            //Check Authorization
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            try
            {
                if (string.IsNullOrEmpty(emailSubject))
                    emailSubject = "newsletter";

                var companies = _adminService.GetListCompanyProfiles(listProfileId.ToList()).ToList();
                if (!companies.Any())
                    return Json(false, JsonRequestBehavior.AllowGet);

                string content = GetNewsLetterEmailContent();

                Thread oThread = new Thread(() => EmailHelper.SendNewsletterEmail(companies, content, emailSubject));

                //Start the thread
                oThread.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Send newsletter fail", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sends the news letter.
        /// </summary>
        /// <param name="listCustIds">The list profile id.</param>
        /// <param name="emailTemplate">The email template.</param>
        /// <param name="emailSubject">The email subject.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendNewsLetterCust(int[] listCustIds, int emailTemplate, string emailSubject)
        {
            //Check Authorization
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            try
            {
                if (string.IsNullOrEmpty(emailSubject))
                    emailSubject = "newsletter";

                var customers = _adminService.GetListCustomerProfiles(listCustIds.ToList()).ToList();
                if (!customers.Any())
                    return Json(false, JsonRequestBehavior.AllowGet);

                string content = GetNewsLetterEmailContent();

                Thread oThread = new Thread(() => EmailHelper.SendNewsletterEmailCust(customers, content, emailSubject));

                //Start the thread
                oThread.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Send newsletter fail", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the newsletter content 
        /// </summary>
        /// <returns></returns>
        private string GetNewsLetterEmailContent()
        {
            string template = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/newsletter.cshtml")))
            {
                template = reader.ReadToEnd();
            }

            var hostUrl = EmailHelper.GetStoreHost().Trim(); //"http://kuyamqa1.apphb.com";
            if (hostUrl.EndsWith("/"))
                hostUrl = hostUrl.Substring(0, hostUrl.Length - 1);

            List<NewsletterInfoDetail> data = _adminService.GetNewsletterData();

            // Update data
            for (int i = 0; i < data.Count; i++)
            {
                if (string.IsNullOrEmpty(data[i].CompanyImage))
                {
                    data[i].CompanyImage = hostUrl + "/images/photo_company_image.png";
                }
                if (data[i].CompanyName.Length > 25)
                {
                    data[i].CompanyName = data[i].CompanyName.Substring(0, 22) + "...";
                }
            }

            List<NewsletterInfoItemGroup> infoGroups = new List<NewsletterInfoItemGroup>();

            // group companies by feature & order manual
            IEnumerable<IGrouping<Types.FeatureCompanyType, NewsletterInfoDetail>> groups =
                data.GroupBy(d => d.FeatureCompanyType);
            List<IGrouping<Types.FeatureCompanyType, NewsletterInfoDetail>> orderedGroups =
                new List<IGrouping<Types.FeatureCompanyType, NewsletterInfoDetail>>();
            if (groups.Any(g => g.Key == Types.FeatureCompanyType.Relax))
                orderedGroups.Add(groups.First(g => g.Key == Types.FeatureCompanyType.Relax));
            if (groups.Any(g => g.Key == Types.FeatureCompanyType.Enegize))
                orderedGroups.Add(groups.First(g => g.Key == Types.FeatureCompanyType.Enegize));
            if (groups.Any(g => g.Key == Types.FeatureCompanyType.Grow))
                orderedGroups.Add(groups.First(g => g.Key == Types.FeatureCompanyType.Grow));
            if (groups.Any(g => g.Key == Types.FeatureCompanyType.Unknown))
                orderedGroups.Add(groups.First(g => g.Key == Types.FeatureCompanyType.Unknown));


            foreach (IGrouping<Types.FeatureCompanyType, NewsletterInfoDetail> group in orderedGroups)
            {
                int groupIndex = 0;
                int itemIndex = 0;

                // Add 2 companies into 1 group
                while (itemIndex < group.Count())
                {
                    NewsletterInfoItemGroup groupInfo = new NewsletterInfoItemGroup();
                    if (groupIndex == 0)
                    {
                        groupInfo.Name = group.Key.ToString().ToLower();
                        if (group.Key == Types.FeatureCompanyType.Unknown)
                            groupInfo.Name = "other";
                    }
                    groupInfo.InfoDetails.Add(group.Skip(itemIndex).First());

                    if (itemIndex + 1 == group.Count())
                    {
                        infoGroups.Add(groupInfo);
                        break;
                    }

                    groupInfo.InfoDetails.Add(group.Skip(itemIndex + 1).First());
                    infoGroups.Add(groupInfo);

                    groupIndex += 1;
                    itemIndex += 2;
                }

            }

            //create template data
            dynamic myObject = new
            {
                Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                Host = hostUrl,
                UserName = "[ProfileName]",
                InfoGroups = infoGroups
            }.ToExpando();

            //string templateResult = Razor.Parse(template, myObject);
            string templateResult = this.RenderPartialViewToString("newsletterEmail", (object)myObject);
            return templateResult;
        }

        #endregion

        #region "Agent/Role"

        public ActionResult AgentList(int? page, string key, string searchType)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            var roles = _adminService.GetAllRoles();
            //string roleName = "All";
            //if (roles != null && roles.Count > 0)
            //{
            //   roleName= roles.Where(m => m.RoleId.ToString() == searchType).FirstOrDefault().RoleName;
            //}
            int totalRecord = 0;
            int pageIndex = page ?? 1;
            //List<string> agentUsers = Roles.GetUsersInRole("Agent").ToList();
            //if (searchType == "Agent" && agentUsers.Count == 0)
            //    agentUsers.Add("0");

            List<Cust> users = _adminService.AdminGetListUserByRole(searchType, key, pageIndex, 10, out totalRecord);
            //foreach (Cust cust in users)
            //{
            //    if (agentUsers.Contains(cust.aspnet_Users.UserName))
            //        cust.IsAgent = true;
            //}
            ViewBag.Roles = roles;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.UsersList = users;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_UserAgentListResults");
            }
            return View();
        }

        public ActionResult ManageUserRole(int id, string key, string searchType, int pageIndex)
        {
            ViewBag.User = _adminService.GetUserById(id);
            ViewBag.Roles = _adminService.GetAllRoles();
            ViewBag.PageIndex = pageIndex;
            ViewBag.key = key;
            ViewBag.SearchType = searchType;
            ViewBag.HotelList = _adminService.GetListHotelByAll();
            ViewBag.Hotel = _adminService.GetDefaultHotelOfStaffByCustId(id);
            return View();
        }

        [HttpPost]
        public ActionResult ChangeUserRole(int userId, bool isChecked, string role, int hotelId)
        {
            Cust cust = DAL.GetCustByCustId(userId);
            if (cust != null)
            {
                if (isChecked)
                {
                    Roles.AddUserToRole(cust.aspnet_Users.UserName, role);
                    if (role == Types.Role.Concierge.ToString())
                    {
                        var staff = new HotelStaff
                        {
                            CustID = userId,
                            HotelID = hotelId,
                            IsDefault = true,
                            CreateDate = DateTime.UtcNow
                        };
                        _adminService.InsertHotelStaff(staff);

                    }
                }
                else
                {
                    Roles.RemoveUserFromRole(cust.aspnet_Users.UserName, role);
                    _adminService.RemoveDefaultHotelStaffByCustId(userId);
                }

            }
            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GrantAgentRole(int custId)
        {
            if (!AuthorizationAdmin())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }


            Cust cust = DAL.GetCustByCustId(custId);
            if (cust != null)
            {
                Roles.AddUserToRole(cust.aspnet_Users.UserName, "Agent");
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RevertAgentRole(int custId)
        {
            if (!AuthorizationAdmin())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }


            Cust cust = DAL.GetCustByCustId(custId);
            if (cust != null)
            {
                Roles.RemoveUserFromRole(cust.aspnet_Users.UserName, "Agent");
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Hotel

        public ActionResult HotelList(string keyName, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;
            List<Hotel> hotels = _adminService.AdminGetListHotelByKey(keyName, index, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.Key = keyName;
            var models =
                hotels.Select(
                    m =>
                        new HotelListModel
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


        public ActionResult HotelAdd()
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            var model = new HotelModel();
            var users = _adminService.AdminGetListUserByRole();
            model.CustList = users;
            return View(model);
        }

        [HttpPost]
        public ActionResult HotelAdd(HotelModel model)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            var hotel = _adminService.GetHotelByName(model.Name);
            var users = _adminService.AdminGetListUserByRole();
            model.CustList = users;

            if (ModelState.IsValid)
            {
                var file = model.FileUpload;
                KalturaMediaEntry kalturaMediaEntry = null;

                if (file != null)
                {
                    var fullPath = ConfigManager.StorageRoot + Path.GetFileName(file.FileName);
                    string fileName = file.FileName;
                    file.SaveAs(fullPath);
                    FileStream _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(_fileStream,
                        Kaltura.KalturaMediaType.IMAGE, fileName);
                    _fileStream.Flush();
                    _fileStream.Close();
                    ConfigManager.DeleteFile(fullPath);

                }

                if (hotel == null)
                {
                    hotel = new Hotel
                    {
                        CustID = model.CustID,
                        Name = model.Name,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow

                    };

                    if (file != null)
                        hotel.LogoId = kalturaMediaEntry.Id;
                    _adminService.InsertHotel(hotel);
                    var staff = new HotelStaff
                    {
                        CustID = model.CustID,
                        HotelID = hotel.HotelID
                    };
                    _adminService.InsertHotelStaff(staff);
                    if (file != null)
                    {
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

                }
                return RedirectToAction("HotelList", new { });
            }
            return View(model);
        }

        public ActionResult HotelEdit(int id)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 1;
            string keyName = Request.Params["keyname"];
            if (!int.TryParse(Request.Params["page"], out pageIndex))
            {
                pageIndex = 1;
            }
            var hotel = _adminService.GetHotelById(id);
            var users = _adminService.AdminGetListUserByRole();
            var model = new HotelModel
            {
                CustID = hotel.CustID,
                Name = hotel.Name,
                CustList = users
            };
            ViewBag.Page = pageIndex;
            ViewBag.KeyName = keyName;
            model.HotelID = id;
            return View(model);
        }

        [HttpPost]
        public ActionResult HotelEdit(HotelModel model)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            var file = model.FileUpload;
            KalturaMediaEntry kalturaMediaEntry = null;
            var hotel = _adminService.GetHotelById(model.HotelID);
            model.CustList = _adminService.AdminGetListUserByRole();

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var fullPath = ConfigManager.StorageRoot + Path.GetFileName(file.FileName);
                    string fileName = file.FileName;
                    file.SaveAs(fullPath);
                    FileStream _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(_fileStream,
                        Kaltura.KalturaMediaType.IMAGE, fileName);
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
            return View(model);
        }

        public ActionResult HotelDelete()
        {
            return View();
        }

        public ActionResult HotelCodeList(int? id, string keyCode, string KeyName, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int index = pageIndex ?? 1;

            int totalRecord = 0;
            List<Hotel> hotels = null;
            if (User.IsInRole("Admin") || User.IsInRole("Agent"))
            {
                hotels = _adminService.AdminGetListHotelByKey(string.Empty, 1, int.MaxValue, out totalRecord);

            }
            else
            {
                hotels = _adminService.GetListHotelOfAdminByCustId(string.Empty, 1, int.MaxValue, out totalRecord,
                    MySession.CustID);
            }
            int hotelId = id ?? ((hotels != null && hotels.Count > 0) ? hotels[0].HotelID : 0);
            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord,
                MySession.CustID);

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
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }

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

            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord);
            ViewBag.Page = index;
            ViewBag.KeyCode = keyCode;
            ViewBag.KeyName = KeyName;
            ViewBag.TotalRecords = totalRecord;
            return PartialView("_HotelCodeList", models);

        }

        [HttpPost]
        public ActionResult HotelCodeDelete(int? id, string keyCode, string KeyName, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
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

            var models = _adminService.GetHotelCodeByHotelId(hotelId, keyCode, index, 10, out totalRecord);
            ViewBag.Page = index;
            ViewBag.KeyCode = keyCode;
            ViewBag.KeyName = KeyName;
            ViewBag.TotalRecords = totalRecord;
            return PartialView("_HotelCodeList", models);
        }

        #endregion

        #region Feedback

        public ActionResult AppointmentFeedback(string keyName, int? companyId, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;
            int _companyId = companyId ?? 0;
            var feedback = _adminService.GetAppointmentFeedback(keyName, _companyId, index, 10, out totalRecord);
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.Key = keyName;
            ViewBag.profileID = _companyId;
            ViewBag.Companys = _companyProfileService.GetAllProfileCompany();
            var models = feedback.Select(m => new AppointmentFeedbackModel
            {
                AppointmentID = m.AppointmentID,
                Appointment = _adminService.GetAppointmentById(m.AppointmentID ?? 0),
                ServiceCompanyID = m.ServiceCompanyID,
                EmployeeID = m.EmployeeID,
                CompanyName = m.ServiceCompany.ProfileCompany.Name,
                Email = m.Cust.Username,
                FirstName = m.Cust.FirstName,
                LastName = m.Cust.LastName,
                Content = m.Content,
                PrivateContent = m.PrivateContent,
                RatingValue = m.RatingValue,
                CreateDate = m.CreateDate
            }).ToList();
            if (Request.IsAjaxRequest())
                return PartialView("_AppointmentFeedback", models);
            return View(models);
        }

        #endregion

        #region BlogEngine

        [KuyamAuthorizeAttribute(Roles = "Admin")]
        public ActionResult GetCategories()
        {
            var categories = _categoryService.GetAll().ToList();
            return View(categories);
        }

        public ActionResult AddFeaturedCompaniesToCategory(int id)
        {
            var profilesSummary =
                _profileCompanyService.GetAll()
                    .Select(t => new CompanyProfileSummary { ProfileId = t.ProfileID, Name = t.Name })
                    .ToList();
            var categoriesFeatured = _categoryFeaturedService.GetFeaturedCompaniesFromCategory(id);
            var category = _categoryService.GetAll().Where(t => t.CategoryRowID == id).FirstOrDefault();
            ViewBag.AdminEditFeatureCompanies = _adminService.AdminGetListFeaturedCompanies();
            ViewBag.categoriesFeatured = categoriesFeatured;
            ViewBag.profilesSummary = profilesSummary;
            ViewBag.Category = category;
            return View();
        }

        [HttpPost]
        public JsonResult UpdateFeaturedCategory(CategoryFeatured categoryFeatured)
        {
            var tg = _categoryFeaturedService.GetById(categoryFeatured.BeCategoryId, categoryFeatured.ProfileId);
            if (tg != null && tg.Order != categoryFeatured.Order)
                return
                    Json(
                        new
                        {
                            success = false,
                            errorMessage = "this company is already selected. please select another one."
                        });
            var oldCategoryFeatured = _categoryFeaturedService.GetByPosition(categoryFeatured.BeCategoryId,
                categoryFeatured.Order);
            if (oldCategoryFeatured == null)
            {
                categoryFeatured.Created = DateTime.UtcNow;
                _categoryFeaturedService.Insert(categoryFeatured);
            }
            else
            {
                if (categoryFeatured.ProfileId == 0)
                {
                    _categoryFeaturedService.Delelete(oldCategoryFeatured);
                }
                else
                {
                    oldCategoryFeatured.ProfileId = categoryFeatured.ProfileId;
                    _categoryFeaturedService.Update(oldCategoryFeatured);
                }
            }
            return Json(new { success = true });
        }

        #endregion

        #region Gift card

        public ActionResult GiftCardList(int? page, string key, int? searchType, int? type, int? status)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var sType = searchType.HasValue ? searchType.Value : 0;
            var typePurchased = 1;
            var statusPurchased = 1;
            var shippingMethod = string.Empty;
            if (sType != 0)
            {
                typePurchased = type.Value;
                statusPurchased = status.Value;
                shippingMethod = string.Empty;
            }
            List<GiftCard> data = _giftCardServices.AdminGetListGiftCardByMostRecent(key, pageIndex, 10, out totalRecord,
                sType, typePurchased, statusPurchased, shippingMethod);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return View();
        }


        [HttpPost, ActionName("GiftCardList")]
        public ActionResult GiftCardListPost(int? page, string key, int? searchType, int? type, int? status)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var sType = searchType.Value;
            var typePurchased = type.Value;
            var statusPurchased = status.Value;
            var shippingMethod = string.Empty;

            List<GiftCard> data = _giftCardServices.AdminGetListGiftCardByMostRecent(key, pageIndex, 10, out totalRecord,
                sType, typePurchased, statusPurchased, shippingMethod);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return PartialView("_GiftCardResult");
        }

        [HttpPost]
        public ActionResult GiftCardMarkSend(int? page, string key, int? searchType, int? type, int? status, int iditem)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            var giftCard = _giftCardServices.GetGiftCardById(iditem);
            giftCard.GiftStatus = (int)Types.GiftCardStatus.Send;
            _giftCardServices.UpdateGiftCardInfo(giftCard);


            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var sType = searchType.Value;
            var typePurchased = type.Value;
            var statusPurchased = status.Value;
            var shippingMethod = string.Empty;

            List<GiftCard> data = _giftCardServices.AdminGetListGiftCardByMostRecent(key, pageIndex, 10, out totalRecord,
                sType, typePurchased, statusPurchased, shippingMethod);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return PartialView("_GiftCardResult");

            //return RedirectToAction("GiftCardList", new { page = page, key = key ,searchType=searchType,type=type,status=status});
            //int totalRecord = 0;
            //int pageIndex = page ?? 1;
            //if (pageIndex < 1)
            //    pageIndex = 1;
            //var sType = 0;
            //if (searchType.HasValue)
            //{
            //    sType = searchType.Value;
            //}
            //var typeBuy = type.Value;
            //var statusShipping = status.Value;
            //var shippingMethod = string.Empty;
            //if (typeBuy != 1 && statusShipping != -1)
            //    if ((statusShipping == 2 || statusShipping == 3))
            //    {
            //        statusShipping = 0;
            //        shippingMethod = "1";
            //    }
            //    else
            //    {
            //        statusShipping = 1;
            //        shippingMethod = "2";
            //    }

            //List<GiftCard> data = _giftCardServices.AdminGetListGiftCardByMostRecent(key, pageIndex, 10, out totalRecord, sType, typeBuy, statusShipping, shippingMethod);

            //int tmp;
            //ViewBag.TotalRecords = totalRecord;
            //ViewBag.Data = data;
            //ViewBag.Page = pageIndex;
            //ViewBag.Key = key;
            //ViewBag.SearchType = type ?? -1;
            //return PartialView("_GiftCardResult");
        }

        //[HttpPost]
        public ActionResult GiftCardPrint(int id)
        {
            var gift = _giftCardServices.GetGiftCardById(id);
            string templateEmail;
            var nameUsers = gift.RecipientName.Split(new char[] { ' ' });
            string typeShipping = string.Empty;
            if (gift.ShippingMethod == 1)
            {
                typeShipping = "FREE standard shipping";
            }
            if (gift.ShippingMethod == 2)
            {
                typeShipping = "premium shipping";
            }
            var totalCost = gift.Amount + gift.ShippingCost;
            dynamic firstalertObject = new
            {
                RecipentNameFirsName = UppercaseWords(nameUsers[0]),
                DateSendEmail = DateTime.Now.ToString("D"),
                FullNameRecipentName = UppercaseWords(gift.RecipientName),
                FullNameSender = UppercaseWords(gift.SenderName),
                Memo = gift.Message,
                GiftCardNumber = gift.GiftCardCode,
                TransactionId = gift.TransactionID,
                ShoppingCost = gift.ShippingCost.Value.ToString("c"),
                AmountGift = gift.Amount.ToString("c"),
                DateOfPurchase = gift.Created.ToString("MMMM dd, yyyy"),
                TotalCost = totalCost.Value.ToString("c"),
                TypeShipping = typeShipping,
                GiftEstimateDate = gift.EstimateDate,
                AmoutNumber = (int)gift.Amount
            }.ToExpando();
            //templateEmail = this.RenderPartialViewToString("GiftCardPrint", (object)firstalertObject);

            return PartialView("GiftCardPrint", firstalertObject);
            //Json(new {printData = templateEmail}, JsonRequestBehavior.AllowGet);
        }

        public string UppercaseWords(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        #endregion

        #region promo code

        public ActionResult PromoCodeList(
            int? page,
            string key,
            int? type,
            string startDate,
            string endDate)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var status = (type != null && type.Value != -1) ? type.Value : -1;
            DateTime? sDate = null;
            if (!string.IsNullOrEmpty(startDate))
                sDate = DateTime.Parse(startDate);
            DateTime? eDate = null;
            if (!string.IsNullOrEmpty(endDate))
                eDate = DateTime.Parse(endDate);
            List<Discount> data = _promoCodeServices.AdminGetDiscounts(key, pageIndex, 10, out totalRecord, status,
                sDate, eDate, (int)Types.DiscountType.Admin);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return View();
        }


        [HttpPost, ActionName("PromoCodeList")]
        public ActionResult PromoCodeListPost(
            int? page,
            string key,
            int? type,
            string startDate,
            string endDate)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var status = (type != null && type.Value != -1) ? type.Value : -1;
            DateTime? sDate = null;
            if (!string.IsNullOrEmpty(startDate) && startDate != "start date")
                sDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(startDate), DateTimeKind.Utc);
            DateTime? eDate = null;
            if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
                eDate = DateTimeUltility.ConvertToPstTime(DateTime.Parse(endDate));
            List<Discount> data = _promoCodeServices.AdminGetDiscounts(key, pageIndex, 10, out totalRecord, status,
                sDate, eDate, (int)Types.DiscountType.Admin);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return PartialView("_PromoCodeResult");
        }

        // [HttpPost]
        //public ActionResult PromoCodeInvitePromocode(
        //   int id,
        //    string promoCode)
        //{

        //    if (!AuthorizationAdminOrAgent())
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }

        //    //int totalRecord = 0;
        //    //int pageIndex = page ?? 1;
        //    //if (pageIndex < 1)
        //    //    pageIndex = 1;

        //    //var status = (type != null && type.Value != -1) ? type.Value : -1;
        //    //DateTime? sDate = null;
        //    //if (!string.IsNullOrEmpty(startDate) && startDate != "start date")
        //    //    sDate = DateTimeUltility.ConvertToPstTime(DateTime.Parse(startDate));
        //    //DateTime? eDate = null;
        //    //if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
        //    //    eDate = DateTimeUltility.ConvertToPstTime(DateTime.Parse(endDate));
        //    //List<Discount> data = _promoCodeServices.AdminGetDiscounts(key, pageIndex, 10, out totalRecord, status, sDate, eDate, (int)Types.DiscountType.Admin);

        //    //int tmp;
        //    //ViewBag.TotalRecords = totalRecord;
        //    //ViewBag.Data = data;
        //    //ViewBag.Page = pageIndex;
        //    //ViewBag.Key = key;
        //    //ViewBag.SearchType = type ?? -1;
        //    return PartialView("_PromoCodeInvite");
        //}


        [HttpPost]
        public ActionResult CreateOrUpdatePromoCodes(
            string key,
            int statusSearch,
            string startDateSearch,
            string title,
            string code,
            decimal amount,
            int? maxUser,
            string startDate,
            string startTime,
            string endDate,
            string endTime,
            int status,
            bool actionType,
            int? discountId)
        {
            //Check existed promo code
            if (!AuthorizationAdminOrAgent())
            {
                return Json(new { result = false, isLogin = false, isCreatedOrUpdate = -1 }, JsonRequestBehavior.AllowGet);
            }
            if (actionType && _promoCodeServices.CheckExistedPromoCode(code, (int)Types.DiscountType.Admin))
                return Json(new { result = false, isLogin = true, isCreatedOrUpdate = 0 }, JsonRequestBehavior.AllowGet);
            var sDate = startDate;
            if (!string.IsNullOrEmpty(startTime))
                sDate = sDate + " " + startTime;
            var eDate = DateTime.MaxValue.ToString();
            if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
            {
                eDate = endDate;
                if (!string.IsNullOrEmpty(endTime))
                    eDate = eDate + " " + endTime;
            }
            var model = new Discount
            {
                Name = title,
                Code = code,
                Amount = amount,
                Percent = 0,
                Quantity = maxUser.HasValue ? maxUser.Value : -1,
                ApplyToAllServices = false,
                StartDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(sDate), DateTimeKind.Utc),
                EndDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(eDate), DateTimeKind.Utc),
                CreatedDate = DateTimeUltility.ConvertToUtcTime(DateTime.Now, DateTimeKind.Utc),
                ModifiedDate = DateTimeUltility.ConvertToUtcTime(DateTime.Now, DateTimeKind.Utc),
                Status = status,
                DiscountType = (int)Types.DiscountType.Admin
            };
            try
            {
                if (!actionType)
                {
                    model.DiscountId = discountId.Value;
                    var returnVal = _promoCodeServices.UpdateDiscount(model);
                    if (returnVal == null)
                        return Json(new { result = true, isLogin = true, isCreatedOrUpdate = 1, code = model.Code },
                            JsonRequestBehavior.AllowGet);
                }
                else
                    _promoCodeServices.CreateDiscount(model);
                LogHelper.Info(string.Format("promo code {0}-{1}", model.Code, model.DiscountId));
                return Json(new { result = true, isLogin = true, isCreatedOrUpdate = 1, code = model.Code },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception er)
            {
                LogHelper.Error("Create service fail: ", er);
                return Json(new { result = false, isLogin = true, isCreatedOrUpdate = 2, error = er.ToString() },
                    JsonRequestBehavior.AllowGet);
                //return Json(new
                //{
                //    status = false,
                //   // message = "Failed to create " + (model.ParentServiceID.HasValue ? "service." : "category."),
                //    error = er.ToString()

                //});
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult GetDiscountById(int discountId)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return Json(new { result = false, isLogin = false }, JsonRequestBehavior.AllowGet);
            }
            var discount = _promoCodeServices.GetDiscountById(discountId);
            if (discount == null)
                return Json(new { result = false, isLogin = true }, JsonRequestBehavior.AllowGet);
            var startDate = new DateTime(discount.StartDate.Year, discount.StartDate.Month, discount.StartDate.Day);
            var startTime = GetTimeFromDate(discount.StartDate);
            var endTime = string.Empty;
            if (discount.EndDate.Date != DateTime.MaxValue.Date)
            {
                endTime = GetTimeFromDate(discount.EndDate);
            }
            var formatDate = "MM/dd/yyyy";
            return Json(new
            {
                result = true,
                isLogin = true,
                title = discount.Name,
                code = discount.Code,
                amount = discount.Amount,
                maxUser = discount.Quantity == -1 ? string.Empty : discount.Quantity.ToString(),
                startDate = discount.StartDate.ToString(formatDate),
                startTime = startTime,
                endDate =
                    discount.EndDate.Date == DateTime.MaxValue.Date
                        ? string.Empty
                        : discount.EndDate.ToString(formatDate),
                endTime = endTime,
                status = discount.Status
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult PromoCodeActiveOrInActive(int id, bool actionType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                return Json(new { result = false, isLogin = false }, JsonRequestBehavior.AllowGet);
            }
            var discount = _promoCodeServices.GetDiscountById(id);
            if (discount == null)
                return Json(new { result = false, isLogin = true }, JsonRequestBehavior.AllowGet);
            discount.Status = actionType ? 1 : 0;
            _promoCodeServices.UpdateDiscount(discount);

            return Json(new { result = true, isLogin = true }, JsonRequestBehavior.AllowGet);
        }
        private string GetTimeFromDate(DateTime dateTime)
        {
            var returnTime = "00:00 AM";
            //Case not select time or select time = 0
            if (dateTime.Hour == dateTime.Minute && dateTime.Minute == dateTime.Second && dateTime.Second == 0)
            {
                return string.Empty;
            }
            else
            {
                return dateTime.ToString("HH:m:s tt");
            }
            return returnTime;
            //    var hour = "00";
            //    var ampm = " am";
            //    if (dateTime.Hour < 10)
            //    {
            //        hour = "0" + dateTime.Hour;
            //    }
            //    else
            //    {
            //        if (dateTime.Hour > 12)
            //        {
            //            var temp = dateTime.Hour - 12;
            //            if (temp < 10)
            //                hour = "0" + temp.ToString();
            //            else
            //                hour = temp.ToString();
            //            ampm = " pm";
            //        }
            //    }
            //    var minute = "00";
            //    if (dateTime.Minute < 10)
            //    { minute = "0" + dateTime.Minute; }
            //    else
            //    {
            //        minute = dateTime.Minute.ToString();
            //    }

            //    returnTime = hour + ":" + minute + ampm;
            //}
            //return returnTime;
        }

        #endregion

        #region promo code report

        public ActionResult PromoCodeListReport(
            int? page,
            string key,
            int? type,
            string startDate,
            string endDate)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var status = (type != null && type.Value != -1) ? type.Value : -1;
            DateTime? sDate = null;
            if (!string.IsNullOrEmpty(startDate))
                sDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(startDate), DateTimeKind.Utc);
            DateTime? eDate = null;
            if (!string.IsNullOrEmpty(endDate))
                eDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(endDate), DateTimeKind.Utc);
            List<Discount> data = _promoCodeServices.AdminGetDiscounts(key, pageIndex, 10, out totalRecord, status,
                sDate, eDate, (int)Types.DiscountType.Admin);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return View();
        }


        [HttpPost, ActionName("PromoCodeListReport")]
        public ActionResult PromoCodeListReportPost(
            int? page,
            string key,
            int? type,
            string startDate,
            string endDate)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var status = (type != null && type.Value != -1) ? type.Value : -1;
            DateTime? sDate = null;
            if (!string.IsNullOrEmpty(startDate) && startDate != "start date")
                sDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(startDate), DateTimeKind.Utc);
            DateTime? eDate = null;
            if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
                eDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(endDate), DateTimeKind.Utc);
            List<Discount> data = _promoCodeServices.AdminGetDiscounts(key, pageIndex, 10, out totalRecord, status,
                sDate, eDate, (int)Types.DiscountType.Admin);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            return PartialView("_PromoCodeReportResult");
        }

        public ActionResult AdminPromoCodeDetails(int id, int userListPageIndex, string key, string searchType,
            string backAction = "PromoCodeListReport")
        {
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var promoCode = _promoCodeServices.GetDiscountById(id);
            List<UserDiscount> listUserDiscounts = new List<UserDiscount>();
            if (promoCode.UserDiscounts.Any())
            {
                var currentDate = DateTime.UtcNow;
                listUserDiscounts =
                    promoCode.UserDiscounts.Where(
                        a => a.DateUsage.Month == currentDate.Month && a.DateUsage.Year == currentDate.Year).ToList();
            }
            var data = listUserDiscounts.OrderByDescending(u => u.DateUsage).Skip((pageIndex - 1) * 10).Take(10).ToList();
            ViewBag.Data = data;
            ViewBag.UserListPageIndex = userListPageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.Action = backAction;
            ViewBag.TotalRecords = listUserDiscounts.Count;
            ViewBag.Page = pageIndex;
            ViewBag.DiscountId = id;
            return View();
        }

        [HttpPost, ActionName("AdminPromoCodeDetails")]
        public ActionResult AdminPromoCodeDetailsPost(
            int? id,
            int? page,
            string key,
            int? type,
            string startDate,
            string endDate,
            bool isCheck)
        {

            if (!AuthorizationAdminOrAgent())
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var status = (type != null && type.Value != -1) ? type.Value : -1;
            DateTime? sDate = null;
            if (!string.IsNullOrEmpty(startDate) && startDate != "start date")
                sDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(startDate), DateTimeKind.Utc);
            DateTime? eDate = null;
            if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
                eDate = DateTimeUltility.ConvertToUtcTime(DateTime.Parse(endDate), DateTimeKind.Utc);
            var discount = _promoCodeServices.GetDiscountById(id.Value);
            var userDiscount = discount.UserDiscounts;
            if (sDate.HasValue && isCheck == false)
            {
                userDiscount =
                    userDiscount.Where(
                        a => a.DateUsage.Month == sDate.Value.Month && a.DateUsage.Year == sDate.Value.Year).ToList();
            }
            ViewBag.TotalRecords = userDiscount.Count();
            var data = userDiscount.OrderByDescending(u => u.DateUsage).Skip((pageIndex - 1) * 10).Take(10).ToList();
            ViewBag.Data = data;

            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            ViewBag.SearchType = type ?? -1;
            ViewBag.Action = "PromoCodeListReport";
            ViewBag.UserListPageIndex = pageIndex;
            return PartialView("_PromoCodeReportDetailResult");
        }

        #endregion

        #region "SMS admin"

        [HttpPost]
        public ActionResult SendMessage(string userId, string message, string destinationAdress)
        {
            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }

            var clearPhonefrom = ConfigurationManager.AppSettings["PhoneInapp"];
            var clearPhoneTo = UtilityHelper.CleanPhone(destinationAdress);
            var smsModel = new SmsModels
            {
                CustId = int.Parse(userId),
                Message = message,
                SenderAddress = clearPhonefrom,
                DestinationAddress = clearPhoneTo,
                DateTime = DateTime.UtcNow,
                DeliveryStatus = (int)Types.DeliveryStatus.Send,
                ReceivedStatus = (int)Types.ReceiveStatus.Sender,
            };
            var sms = ConvertToSmsMessage(smsModel);

            var phoneNumbers = new string[] { clearPhoneTo };

            var result = _smsProvider.SendSms(phoneNumbers, "hello", message, false);
            var data = result.Data as SendSMSResponse;
            sms.MessageId = data != null ? data.outBoundSMSResponse.messageId : UtilityHelper.GenerateRandomDigitCode(8);
            sms.DeliveryStatus = result.Status == (int)EResultStatus.Fail ?
                (int)Types.DeliveryStatus.CanNotSend :
                (int)Types.DeliveryStatus.Temp;
            sms.DecriptionStatus = result.Message;

            var flag = _smsServices.AddMessage(sms);

            //if (result.Status == (int)EResultStatus.Success)
            //{
            //    ClientAdapter.Instance.SendMessage(sms);
            //}

            return Json(sms.DeliveryStatus == (int)Types.DeliveryStatus.CanNotSend ? new { result = false, message = sms.Message, iAuthen = true } : new { result = true, message = string.Empty, iAuthen = true }, JsonRequestBehavior.AllowGet);


            //if (result.Status == (int) EResultStatus.Fail)
            //{
            //    var data = result.Data as SendSMSResponse;
            //    sms.MessageId = data != null ? data.outBoundSMSResponse.messageId : UtilityHelper.GenerateRandomDigitCode(8);
            //    sms.DeliveryStatus = (int) Types.DeliveryStatus.CanNotSend;
            //    sms.DecriptionStatus = result.Message;
            //    var flag = _smsServices.AddMessage(sms);

            //    return Json(new {result = false, message = result.Message, iAuthen = true}, JsonRequestBehavior.AllowGet);
            //}
            //return Json(new { result = false, message = string.Empty, iAuthen = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult TrySendMessage(string messageId, int userId)
        {
            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var message = _smsServices.GetMessageByMessageIdAndCustId(messageId, userId);
            message.DateTime = DateTime.UtcNow;
            message.DeliveryStatus = (int)Types.DeliveryStatus.Send;
            var clearPhoneTo = UtilityHelper.CleanPhone(message.DestinationAddress);

            var phoneNumbers = new string[] { clearPhoneTo };
            var result = _smsProvider.SendSms(phoneNumbers, "hello", message.Message, false);
            var data = result.Data as SendSMSResponse;
            message.MessageId = data != null ? data.outBoundSMSResponse.messageId : UtilityHelper.GenerateRandomDigitCode(8);
            message.DeliveryStatus = result.Status == (int)EResultStatus.Fail ?
               (int)Types.DeliveryStatus.CanNotSend :
               (int)Types.DeliveryStatus.Temp;
            message.DecriptionStatus = result.Message;
            _smsServices.UpdateMessage(message);
            return Json(message.DeliveryStatus == (int)Types.DeliveryStatus.CanNotSend ? new { result = false, message = result.Message, iAuthen = true } : new { result = true, message = string.Empty, iAuthen = true }, JsonRequestBehavior.AllowGet);

            //if (result.Status == (int)EResultStatus.Fail)
            //{
            //    var data = result.Data as SendSMSResponse;

            //    if (data != null)
            //    {
            //        message.MessageId = data.outBoundSMSResponse.messageId;
            //    }
            //    message.MessageId = UtilityHelper.GenerateRandomDigitCode(8);
            //    message.DeliveryStatus = (int)Types.DeliveryStatus.CanNotSend;
            //    message.DecriptionStatus = result.Message;
            //    _smsServices.UpdateMessage(message);

            //    return Json(new { result = false, message = result.Message, iAuthen = true }, JsonRequestBehavior.AllowGet);
            //}
            //else//delete message record with messageId
            //{
            //    message.DeliveryStatus = (int)Types.DeliveryStatus.Delete;
            //}

            //var flag = _smsServices.UpdateMessage(message);

            //return Json(new { result = true, message = string.Empty, iAuthen = true }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult UserAddNumber(int userId, string userAddNumber)
        {
            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var clearPhone = UtilityHelper.CleanPhone(userAddNumber);
            var isExisted = _adminService.CheckPhoneNumberExisted(clearPhone);
            if (!isExisted)
                return Json(new { result = false, existed = true }, JsonRequestBehavior.AllowGet);
            var flag = _adminService.AddPhoneNumberforUser(userId, clearPhone);
            return Json(new { result = flag, existed = false, iAuthen = true }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult LoadMessage(int userId, int? totalRecord)
        //{
        //    if (!AuthorizationAdmin())
        //    {
        //        var loginUrl = Url.Action("Login", "Admin");
        //        return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
        //    }
        //    var mess = _smsServices.GetAllMessage(userId);
        //    ViewBag.Messages = mess;
        //    ViewBag.TotalMessage = totalRecord;
        //    int total = mess.Count();
        //    var flag = (total > totalRecord);
        //    if (flag)
        //        ViewBag.TotalMessage = total;
        //    string html = this.RenderPartialViewToString("AdminSMSItems");
        //    return Json(new { result = true, iAuthen = true, totalRecord = total, pView = html }, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult LoadMessage(int userId, int? totalRecord)
        {

            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            //string message = ClientAdapter.Instance.GetMessage(userId.ToString());
            var mess = _smsServices.GetAllMessage(userId);
            ViewBag.Messages = mess;
            ViewBag.TotalMessage = totalRecord;
            int total = mess.Count();
            var flag = (total > totalRecord);
            if (flag)
                ViewBag.TotalMessage = total;
            string html = this.RenderPartialViewToString("AdminSMSItems");
            return Json(new { result = true, iAuthen = true, totalRecord = total, pView = html }, JsonRequestBehavior.AllowGet);


        }

        public SMSMessage ConvertToSmsMessage(SmsModels smsMessage)
        {
            return new SMSMessage
            {
                CustId = smsMessage.CustId,
                DateTime = smsMessage.DateTime,
                DecriptionStatus = smsMessage.DecriptionStatus,
                DeliveryStatus = smsMessage.DeliveryStatus,
                DestinationAddress = smsMessage.DestinationAddress,
                Message = smsMessage.Message,
                MessageId = smsMessage.MessageId,
                ReceivedStatus = smsMessage.ReceivedStatus,
                SenderAddress = smsMessage.SenderAddress,
                ErrorCode = smsMessage.ErrorCode,
                Id = smsMessage.Id
            };
        }
        #endregion


        #region "request timeslot"
        public ActionResult RequestTimeSlotList(int? companyId, int? page, string key, int searchType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            int totalRecord = 0;
            int index = page ?? 1;

            ViewBag.Company = _companyProfileService.GetProfileCompanyByID(companyId.Value);

            var timeSlots = _requestTimeSlotServices.GetAllTimeSlot(companyId, index, 10, out totalRecord);
            ViewBag.TimeSlots = new List<RequestTimeSlot>();
            if (timeSlots.Any())
            {
                var list = from slot in timeSlots
                           select new RequestTimeSlot
                                  {
                                      Id = slot.Id,
                                      CompanyName = slot.ProfileCompany.Name,
                                      ProfileId = slot.ProfileId.Value,
                                      FromHour = FormatTimeSpan(slot.FromHour),
                                      ToHour = FormatTimeSpan(slot.ToHour),
                                      DateOfWeek = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek)slot.DayOfWeek)
                                  };
                ViewBag.TimeSlots = list;
            }

            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.profileID = companyId ?? 0;
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RequestTimeSlotList");
            }
            return View();
        }
        public ActionResult RequestTimeSlotAdd(int Id, int? page, string key, int searchType, bool isDetail)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            var model = new RequestTimeSlot();
            var timeSlot = _requestTimeSlotServices.GetAllTimeSlot(Id);
            ViewBag.isDetail = isDetail;
            if (timeSlot.Any())
                return RedirectToAction("RequestTimeSlotEdit", new { Id = Id, page = page.Value, key = key, searchType = searchType, isDetail = isDetail });
            //var pro = _companyProfileService.GetProfileCompanyByProfileId(Id.Value);
            var pro = _companyProfileService.GetProfileCompanyByID(Id);
            model.ProfileCompany = pro;
            model.ProfileId = Id;
            model.FromHour = null;
            model.ToHour = null;
            ViewBag.CurrentDate = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc).ToString();
            ViewBag.previewHour = null;
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;

            model.htmlTimeSlots = string.Empty;
            return View(model);
        }
        [HttpPost]
        public ActionResult RequestTimeSlotAdd(
            string fromHour,
            string toHour,
            string stringListDays,
            int companyId)
        {
            if (!AuthorizationAdminOrAgent())
            {
                //return RedirectToAction("Login");
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var prExisted = _requestTimeSlotServices.CheckProfileExistedTimeSlot(companyId);
            if (!prExisted)
                return Json(new { result = true, iAuthen = true, isExisted = true });
            var listTimeSlot = GeneralTimeSlots(fromHour, toHour, stringListDays, companyId);
            _requestTimeSlotServices.SaveTimeSlot(listTimeSlot);
            return Json(new { result = true, iAuthen = true, isExisted = false });

        }
        public ActionResult RequestTimeSlotEdit(int Id, int page, string key, int searchType, bool isDetail)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
                //var loginUrl = Url.Action("Login", "Admin");
                //return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var request = _requestTimeSlotServices.GetAllTimeSlot(Id);
            var model = new RequestTimeSlot();
            var profile = _companyProfileService.GetProfileCompanyByID(Id);
            //GetAllProfileCompany((int)Types.CompanyType.NonKuyamBookIt);
            model.ProfileCompany = profile;
            model.ProfileId = Id;
            model.FromHour = null;//FormatTimeSpan(request[0].FromHour);
            model.ToHour = null;//FormatTimeSpan(request[0].ToHour);
            List<EmployeeHour> previewHour = new List<EmployeeHour>();
            var id = string.Empty;
            var flag = true;
            var ul = string.Empty;
            //if (request.Count() == 7)
            //{
            //    id = "isdaily," + model.FromHour.Replace(" ", "") + "," + model.ToHour.Replace(" ", "");
            //    span = "<span id='" + id + "'>mon - sun " + model.FromHour.Replace(" ", "") + "-" +
            //               model.ToHour.Replace(" ", "") + "</span><br>";
            //    flag = false;
            //}
            var results = from p in request
                          group p by p.DayOfWeek into g
                          select new { date = g.Key, groupDate = g.ToList() };
            var lsitRes = results.OrderBy(a => a.date);
            var stId = string.Empty;
            var dic = new Dictionary<int, string>();
            foreach (var result in lsitRes)
            {
                var date = result.date;
                var span = string.Empty;
                var dow = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek)date);

                foreach (var result1 in result.groupDate)
                {
                    var fromTime = FormatTimeSpan(result1.FromHour);
                    var toTime = FormatTimeSpan(result1.ToHour);
                    id = dow.Substring(0, 3) + "," + fromTime.Replace(" ", "") + "," + toTime.Replace(" ", "");
                    span = span + "<span id='" + id + "'>" + fromTime.Replace(" ", "") + "-" +
                        toTime.Replace(" ", "") + "</span><br>";
                    stId = stId + id + "*";
                }
                var ulItem = "<ul id='" + date + "'><li id='" + dow.Substring(0, 3) + "' class='first'>" + dow.Substring(0, 3) + ":</li><li id='" + dow.Substring(0, 3) + "content'>" +
                         span + "</li></ul>";
                dic.Add(result.date, ulItem);
                var em = new EmployeeHour();
                em.DayOfWeek = date;
                previewHour.Add(em);
            }
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            var listcontent = lsitRes.Select(a => a.date);
            var listRest = list.Where(a => !listcontent.Contains(a)).ToList();
            for (var i = 0; i < listRest.Count(); i++)
            {
                var dt = listRest[i];
                var dowName = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek)dt);
                var ulItem = "<ul id='" + dt + "'><li id='" + dowName.Substring(0, 3) + "' class='first'>" + dowName.Substring(0, 3) + ":</li><li id='" + dowName.Substring(0, 3) + "content'>" + "</li></ul>";
                dic.Add(dt, ulItem);
            }
            model.StringListDays = stId;
            //foreach (var hour in request)
            //{
            //    //if (flag)
            //    //{

            //    //var dow = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek) hour.DayOfWeek);
            //    //var fromTime = FormatTimeSpan(hour.FromHour);
            //    //var toTime = FormatTimeSpan(hour.ToHour);
            //    //id = dow.Substring(0, 3) + "," + fromTime.Replace(" ", "") + "," + toTime.Replace(" ", "");
            //    //span = "<span id='" + id + "'>" + fromTime.Replace(" ", "") + "-" +
            //    //    toTime.Replace(" ", "") + "</span><br>";
            //    // ul =ul + "<ul id='" + hour.DayOfWeek + "'><li id='" + dow.Substring(0, 3) + "' class='first'>" + dow.Substring(0, 3) + ":</li><li id='" + dow.Substring(0, 3) + "content'>" +
            //    //          span + "</li></ul>";
            //   // }
            //    var em = new EmployeeHour();
            //    em.DayOfWeek = hour.DayOfWeek;
            //    previewHour.Add(em);
            //}
            var dicSort = dic.OrderBy(a => a.Key);
            foreach (var keyValuePair in dicSort)
            {
                ul = ul + keyValuePair.Value;
            }
            model.htmlTimeSlots = ul;
            ViewBag.previewHour = previewHour;
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.isDetail = isDetail;
            return View(model);
        }

        [HttpPost]
        public ActionResult RequestTimeSlotEdit(string fromHour,
            string toHour,
            string stringListDays,
            int companyId)
        {
            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var listTimeSlot = GeneralTimeSlots(fromHour, toHour, stringListDays, companyId);
            _requestTimeSlotServices.EditTimeSlot(companyId, listTimeSlot);
            return Json(new { result = true, iAuthen = true });
        }
        public ActionResult RequestTimeSlotItemEdit(int Id, int companyId, int page, string key, int searchType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
                //var loginUrl = Url.Action("Login", "Admin");
                //return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var request = _requestTimeSlotServices.GetTimeSlotByCompanyAndId(companyId, Id);
            var model = new RequestTimeSlot();
            var profile = _companyProfileService.GetProfileCompanyByID(companyId);
            model.ProfileCompany = profile;
            model.ProfileId = companyId;
            model.Id = Id;
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            ViewBag.FromHour = FormatTimeSpan(request.FromHour);
            ViewBag.ToHour = FormatTimeSpan(request.ToHour);
            return View(model);
        }
        [HttpPost]
        public ActionResult RequestTimeSlotItemEdit(int companyId,
            int Id, string fromHour, string toHour,
            int page, string key, int searchType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                var loginUrl = Url.Action("Login", "Admin");
                return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
            }
            var req = _requestTimeSlotServices.GetAllTimeSlot(companyId);
            DateTime dt = DateTime.Parse(fromHour);
            TimeSpan fTime = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            dt = DateTime.Parse(toHour);
            TimeSpan tTime = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            var cuTs = req.SingleOrDefault(a => a.Id == Id);
            var listReq = req.Where(a => a.Id != Id && a.DayOfWeek == cuTs.DayOfWeek);

            var flag = CheckOverlap(listReq.ToList(), fTime, tTime);
            if (flag)
            {
                return Json(new { result = true, iAuthen = true, isOverlap = true });
            }
            _requestTimeSlotServices.UpdateTimeslot(Id, fTime, tTime);
            var model = new RequestTimeSlot();
            var profile = _companyProfileService.GetProfileCompanyByID(Id);
            model.ProfileCompany = profile;
            model.ProfileId = companyId;
            model.Id = Id;
            ViewBag.Page = page;
            ViewBag.Key = key;
            ViewBag.SearchType = searchType;
            return Json(new { result = true, iAuthen = true, isOverlap = false });
        }
        public List<GeneralTimeSlot> GeneralTimeSlots(string fromHour, string toHour, string stringListDays, int companyId)
        {
            var arrIds = stringListDays.Split(new char[] { '*' });
            var listTimeSlot = new List<GeneralTimeSlot>();
            foreach (var arrId in arrIds)
            {
                var arrDate = arrId.Split(new char[] { ',' });

                if (arrDate.Count() == 1 && arrDate[0] == "isdaily")
                {
                    for (var i = 0; i < 7; i++)
                    {
                        var generalTimeSlot = new GeneralTimeSlot();
                        generalTimeSlot.DayOfWeek = i;
                        generalTimeSlot.ProfileId = companyId;
                        fromHour = arrDate[1];
                        toHour = arrDate[2];
                        DateTime dt = DateTime.Parse(fromHour);
                        TimeSpan fHour = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
                        generalTimeSlot.FromHour = fHour;
                        dt = DateTime.Parse(toHour);
                        TimeSpan tHour = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
                        generalTimeSlot.ToHour = tHour;
                        listTimeSlot.Add(generalTimeSlot);
                    }
                }
                else
                {
                    //for (var i = 0; i < arrDate.Count() && arrDate.Count()>=3; i++)
                    //{
                    if (arrDate.Count() >= 3)
                    {
                        var numberDateOfWeek = GetNumberOfWeek(arrDate[0]);
                        var generalTimeSlot = new GeneralTimeSlot();
                        generalTimeSlot.DayOfWeek = numberDateOfWeek;
                        generalTimeSlot.ProfileId = companyId;
                        fromHour = arrDate[1];
                        toHour = arrDate[2];
                        DateTime dt = DateTime.Parse(fromHour);
                        TimeSpan fHour = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
                        generalTimeSlot.FromHour = fHour;

                        dt = DateTime.Parse(toHour);
                        TimeSpan tHour = new TimeSpan(dt.Hour, dt.Minute, dt.Second);

                        generalTimeSlot.ToHour = tHour;
                        listTimeSlot.Add(generalTimeSlot);
                    }
                    //}
                }
            }

            return listTimeSlot;
        }

        public ActionResult RequestTimeSlotDelete(int? id, int? companyId, int? page, string key, int searchType)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            _requestTimeSlotServices.DeleteTimeSlot(id.Value);
            return RedirectToAction("RequestTimeSlotList", new { companyId = companyId, page = page, key = key, searchType = searchType });
        }
        public string FormatTimeSpan(TimeSpan timeSpan)
        {
            var now = DateTime.Now;
            var time = new DateTime(now.Year, now.Month, now.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Milliseconds);
            return time.ToString("hh:mm tt").ToLower();
        }
        public int GetNumberOfWeek(string dateOfWeek)
        {
            //DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dateOfWeek);

            switch (dateOfWeek)
            {
                case "Mon":
                    return 1;
                    break;
                case "Tue":
                    return 2;
                    break;
                case "Wed":
                    return 3;
                    break;
                case "Thu":
                    return 4;
                    break;
                case "Fri":
                    return 5;
                    break;
                case "Sat":
                    return 6;
                    break;
                case "Sun":
                    return 0;
                    break;
            }
            return 0;
        }

        public bool CheckOverlap(IList<GeneralTimeSlot> list, TimeSpan from, TimeSpan to)
        {
            foreach (var generalTimeSlot in list)
            {
                if (generalTimeSlot.FromHour >= to || generalTimeSlot.ToHour <= from)
                {

                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        #endregion


        #region "request appoinment"
        /// <summary>
        /// Init and search
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult RequestAppoinmentList(int? companyId, int? pageIndex,
              string startDate,
            string endDate)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                else
                {
                    return AdminUnauthorizedResult();
                }
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;

            ViewBag.Companys = _companyProfileService.GetAllProfileCompany((int)Types.CompanyType.GeneralAvailability);

            var sd = DateTime.MinValue;
            string format = "MM/dd/yyyy";
            if (!string.IsNullOrEmpty(startDate) && startDate != "start date")
            {
                sd = DateTime.ParseExact(startDate, format, CultureInfo.InvariantCulture);
            }
            var ed = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endDate) && endDate != "end date")
            {
                ed = DateTime.ParseExact(endDate, format, CultureInfo.InvariantCulture);
                //ed = ed.AddDays(1);
            }
            var comId = companyId ?? 0;
            var apts = _appointmentService.GetRequestAppointments(comId, sd, ed, index, 10, out totalRecord);

            ViewBag.RequestAppointments = apts;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.profileID = comId;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RequestAppoinmentList");
            }
            return View();
        }

        public ActionResult RequestAppointmentDetails(int Id, int page)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                else
                {
                    return AdminUnauthorizedResult();
                }
            }
            var req = _appointmentService.GetRequestAppointmentsById(Id);
            return View(req);
        }

        [HttpPost]
        public ActionResult RequestAppoimentBook(int Id, string appointmentStart, string employeeName)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                else
                {
                    return AdminUnauthorizedResult();
                }
            }
            var req = _appointmentService.GetRequestAppointmentsById(Id);
            var requestStart = req.Start;
            var requestEnd = req.End;
            var duration = req.ServiceCompany.Duration;
            string format = "MM/dd/yy hh:mm tt";
            DateTime appointmentStartDate = appointmentStart.ToDateTime(format);

            //if (appointmentStartDate < requestStart)
            //    return Json(new { result = false, isCheckDate = false }, JsonRequestBehavior.AllowGet);
            //var appointmentEndDate = appointmentStartDate.AddMinutes(duration.Value);
            //if (requestEnd < appointmentEndDate)
            //    return Json(new { result = false, isCheckDate = false }, JsonRequestBehavior.AllowGet);

            var appointmentEndDate = appointmentStartDate.AddMinutes(duration.Value);
            var proposed = new ProposedAppointment();
            proposed.ServiceCompanyId = req.ServiceCompanyId;
            proposed.AppointmentStatusID = (int)Types.ProposedAppointmentStatus.Default;
            proposed.Description = req.Description;
            proposed.Start = appointmentStartDate;
            proposed.End = appointmentEndDate;
            proposed.Created = DateTime.UtcNow;
            proposed.CustID = req.CustID;
            proposed.CalendarId = req.CalendarId;
            proposed.HotelID = req.HotelID;
            proposed.StaffID = req.StaffID;
            proposed.Duration = req.ServiceCompany.Duration;
            proposed.Price = req.ServiceCompany.Price;
            proposed.ProfileId = req.ProfileId;
            proposed.AttendeesNumber = req.ServiceCompany.AttendeesNumber;

            proposed.EmployeeName = employeeName;
            proposed.ProfileId = req.ProfileId;

            var ret = _appointmentService.InsertProposedAppointment(proposed);

            bool flag = false;
            if (ret != null)
            {
                flag = true;
                req.Status = (int)Types.RequestAppoitmentStatus.Booked;
                _appointmentService.UpdateRequestAppointment(req);
                if (req.StaffID == null)
                {
                    _notificationService.SendProposedAppointment(ret);
                }

            }
            return Json(new { result = flag, isCheckDate = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProposedAppointmentList(int? companyId, int? pageIndex)
        {
            if (!AuthorizationAdminOrAgent())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                else
                {
                    return AdminUnauthorizedResult();
                }
            }
            int totalRecord = 0;
            int index = pageIndex ?? 1;

            ViewBag.Companys = _companyProfileService.GetAllProfileCompany();

            var comId = companyId ?? 0;
            var proAppointments = _appointmentService.GetProposedAppointment(comId, index, 10, out totalRecord);
            var apts = new List<ProposedAppointmentModel>();
            var custDevices = _notificationService.GetAllUserDevice();
            foreach (var proposedAppointment in proAppointments)
            {
                var proApp = new ProposedAppointmentModel();
                proApp.ProfileCompanyName = proposedAppointment.ProfileCompany.Name;
                proApp.Start = proposedAppointment.Start.ToString("MM/dd/yyyy hh:mm tt");
                proApp.End = proposedAppointment.End.ToString("MM/dd/yyyy hh:mm tt");
                proApp.Duration = proposedAppointment.Duration.HasValue ? proposedAppointment.Duration.Value : 0;
                proApp.Price = proposedAppointment.Price.HasValue ? proposedAppointment.Price.Value : 0;
                proApp.AppointmentID = proposedAppointment.AppointmentID;
                proApp.CustID = proposedAppointment.CustID;
                proApp.CusFullName = proposedAppointment.Cust.FullName;
                proApp.CalendarName = proposedAppointment.Calendar != null
                    ? proposedAppointment.Calendar.Name
                    : string.Empty;
                proApp.HotelId = proposedAppointment.HotelID;
                proApp.ConciergeId = proposedAppointment.StaffID;

                if (proposedAppointment.ServiceCompany != null && proposedAppointment.ServiceCompany.Service != null &&
                    !string.IsNullOrEmpty(proposedAppointment.ServiceCompany.Service.ServiceName))
                {
                    proApp.ServiceName = proposedAppointment.ServiceCompany.Service.ServiceName;
                }
                else
                {
                    if (proposedAppointment.ServiceId.HasValue)
                    {
                        var services = _adminService.AdminGetListServiceAll();
                        var ser = (from se in services
                                   where se.ServiceID == proposedAppointment.ServiceId
                                   select se).SingleOrDefault();
                        proApp.ServiceName = ser.ServiceName;
                    }

                }
                proApp.IsHasDevice = custDevices.Any(m => m.CustId == proposedAppointment.CustID);
                apts.Add(proApp);
            }

            //_appointmentService.GetProposedAppointment(comId, index, 10, out totalRecord);

            ViewBag.ProposedAppointments = apts;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Page = index;
            ViewBag.profileID = companyId ?? 0;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProposedAppointmentList");
            }
            return View();
        }

        [KuyamAuthorizeAttribute(Roles = "Admin, Agent")]
        public ActionResult ProposedBook()
        {
            var model = new ProposedAppointmentModel();

            List<CustExtension> cust = _adminService.GetAllUser();
            var listCompany = _companyProfileService.GetAllProfileCompany();
            model.ProfileCompanys = listCompany;
            model.CustList = cust;
            int custId = cust.FirstOrDefault().CustID;
            model.CalendarList = _companyProfileService.GetCalendarByCustId(custId);
            var hotels = _adminService.GetListHotelByAll();
            model.HotelList = hotels;
            model.ServiceList = _adminService.AdminGetListServiceAll();//_companyProfileService.GetProposedCompanyServiceById(profileId);
            return View(model);
        }

        [KuyamAuthorizeAttribute(Roles = "Admin, Agent")]
        [HttpPost]
        public ActionResult ProposedBook(ProposedAppointmentModel model)
        {
            string startDate = string.Format("{0} {1}", model.Date, model.Start);
            DateTime start = DateTime.ParseExact(startDate, "MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);
            DateTime end = start.AddMinutes(model.Duration);
            List<CustExtension> cust = _adminService.GetAllUser();
            model.ProfileCompanys = _companyProfileService.GetAllProfileCompany();
            model.CustList = _adminService.GetAllUser();
            var services = _adminService.AdminGetListServiceAll();
            var service = services.Where(m => m.ServiceID == model.ServiceId).FirstOrDefault();
            var entity = new ProposedAppointment
            {
                CustID = model.CustID,
                AppointmentStatusID = (int)Types.ProposedAppointmentStatus.Default,
                CalendarId = model.CalendarId,
                ProfileId = model.ProfileId,
                ServiceCompanyId = null,
                Start = start,
                End = end,
                Description = model.Description,
                Duration = model.Duration,
                EmployeeName = model.EmployeeName,
                Price = model.Price,
                AttendeesNumber = 1,
                Modified = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                ServiceId = model.ServiceId,
                ServiceName = service.ServiceName
            };

            if (model.HotelId.HasValue && model.HotelId > 0)
            {
                entity.HotelID = model.HotelId;
                entity.StaffID = model.ConciergeId;
            }
            _appointmentService.InsertProposedAppointment(entity);
            if (entity.StaffID == null)
            {
                _notificationService.SendProposedAppointment(entity);
            }

            return RedirectToAction("ProposedAppointmentList");
        }
        public ActionResult GetServiceCompanyById(int companyId)
        {
            var serviceCompanys = _companyProfileService.GetProposedCompanyServiceById(companyId);
            StringBuilder html = new StringBuilder();
            foreach (var item in serviceCompanys)
            {
                html.AppendFormat("<option value=\"{0}\">{1}</option>", item.ID, item.ServiceName);
            }
            return Json(html.ToString(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProposeSendEmail(int apptId)
        {
            string result = string.Empty;
            string email = string.Empty;
            var appt = _appointmentService.GetProposedAppointmentById(apptId);
            if (appt.StaffID != null && appt.StaffID > 0 && appt.HotelID > 0)
            {
                var cust = DAL.xGetCust(appt.HotelStaff.CustID);
                email = cust.Email;
                result = EmailHelper.ProposeEmailTemplateForConcierge(appt, this);
            }
            else
            {
                result = ProposeEmailTemplate(appt);
                email = appt.Cust.Email;
            }

            EmailHelper.SendMail(string.Empty, email, "your proposed appointment from kuyam", result);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        private string ProposeEmailTemplate(ProposedAppointment proposedAppointment)
        {
            string domainName = EmailHelper.GetStoreHost();
            string url = domainName.Split(':')[1];
            var serviceName = proposedAppointment.ServiceCompany != null ?
                proposedAppointment.ServiceCompany.Service.ServiceName :
                proposedAppointment.Service.ServiceName;

            string templateAppointmentReminder;
            var company = proposedAppointment.ProfileCompany;
            var duration = proposedAppointment.Duration;
            var price = proposedAppointment.Price;
            dynamic firstalertObject = new
            {
                DateNow = String.Format("{0:dddd, MMMM d, yyyy}", DateTimeUltility.ConvertToUserTime(DateTime.Now)),
                UserName = proposedAppointment.Cust.FirstName.ToString(),
                Date = proposedAppointment.Start.ToString("ddd, MMM d"),
                Time = proposedAppointment.Start.ToString("h:mm tt"),
                Service = string.Format("{0}, {1}min, ${2}", serviceName, duration, price),
                EmployeeName = proposedAppointment.EmployeeName,
                CompanyName = company.Name,
                CompanyAddress = string.Format("{0}", company.Street1),
                CompanyCity = string.Format("{0},{1} {2}", company.City, company.State, company.Zip),
                CompanyPhone = UtilityHelper.FormatPhone(company.Phone),
                linkBook = string.Format("companyprofile/proposedbook/{0}", proposedAppointment.AppointmentID)
            }.ToExpando();

            templateAppointmentReminder = this.RenderPartialViewToString("AppointmentProposed", (object)firstalertObject);
            return templateAppointmentReminder;
        }

        [HttpPost]
        public ActionResult ProposeSendSms(int apptId)
        {
            var appt = _appointmentService.GetProposedAppointmentById(apptId);
            if (appt == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            var result = ProposeSMSTemplate(appt);
            var clearPhoneTo = UtilityHelper.CleanPhone(appt.Cust.MobilePhone);
            var phoneNumbers = new string[] { clearPhoneTo };
            var smsData = _smsProvider.SendSms(phoneNumbers, "hello", result, false);

            var smsModel = new SmsModels
            {
                CustId = appt.CustID,
                Message = result,
                SenderAddress = clearPhoneTo,
                DestinationAddress = clearPhoneTo,
                DateTime = DateTime.UtcNow,
                DeliveryStatus = (int)Types.DeliveryStatus.Send,
                ReceivedStatus = (int)Types.ReceiveStatus.Sender,
            };
            var sms = ConvertToSmsMessage(smsModel);

            var data = smsData.Data as SendSMSResponse;
            sms.MessageId = data != null ? data.outBoundSMSResponse.messageId : UtilityHelper.GenerateRandomDigitCode(8);
            sms.DeliveryStatus = smsData.Status == (int)EResultStatus.Fail ?
                (int)Types.DeliveryStatus.CanNotSend :
                (int)Types.DeliveryStatus.Temp;
            sms.DecriptionStatus = smsData.Message;
            var flag = _smsServices.AddMessage(sms);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PushNotificationMessage(int proposedId, string deviceId, string message)
        {
            var appt = _appointmentService.GetProposedAppointmentById(proposedId);
            if (appt == null)
                return Json(false, JsonRequestBehavior.AllowGet);

            string txtmessage = ProposeSMSTemplate(appt);
            _notificationService.ProposedPushMessage(appt, txtmessage);
            //var result = _hipmobService.SendTextMessage(deviceId, message);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PushNotificationGeneralMessage(string deviceId, string message)
        {
            int custId = 0;
            int.TryParse(deviceId, out custId);
            var result = _hipmobService.SendTextMessage(deviceId, message);
            if (result)
                _notificationService.PushGeneralMessage(custId, message, 1, Types.NotificationType.GeneralPushMessage);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private string ProposeSMSTemplate(ProposedAppointment proposedAppointment)
        {
            string domainName = EmailHelper.GetStoreHost();
            string url = domainName.Split(':')[1];
            StringBuilder temp = new StringBuilder();
            var companyName = proposedAppointment.ProfileCompany.Name;
            temp.AppendFormat("kuyam message: you have a proposed appointment ready to view with {0} at {1} on {2}. open the app or web site to book.", companyName, proposedAppointment.Start.ToString("h:mmtt").ToLower(), proposedAppointment.Start.ToString("yyyy/MM/dd"));
            //temp.AppendFormat("kuyam:{0}{1} </br>", url, proposedAppointment.AppointmentID);
            //temp.AppendFormat("concierge:{0}{1} </br>", url, proposedAppointment.AppointmentID);
            //temp.AppendFormat("{0}{1}", domainName, proposedAppointment.AppointmentID);
            return temp.ToString();
        }

        #endregion


        #region "import data"

        public ActionResult ImportDataPage()
        {
            if (!AuthorizationAdmin())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            return View("AdminCompanyImportData");
        }
        [HttpPost]
        public ActionResult SubmitImportFile(HttpPostedFileBase fileUpload)
        {
            //if (isDirectoryContainFiles(Server.MapPath("~/Content/files/ExportImport")))
            //    return View("AdminCompanyImportDataShowInfo");
            //return Directory.EnumerateFiles(Server.MapPath("~/Content/files/ExportImport", "*", SearchOption.AllDirectories).Any();
            //if (_adminService.IsHaveTempDate())
            //{
            //    return View("AdminCompanyImportDataShowInfo");
            //}

            if (!AuthorizationAdmin())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                try
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileUpload.FileName);
                    var ext = Path.GetExtension(fileUpload.FileName);
                    var fileName = fileNameWithoutExt.Trim() + "_" + DateTime.Now.ToString("MMddyyyyHHmmss.ffff") + ext;
                    var path = Path.Combine(Server.MapPath("~/Content/files/ExportImport"), fileName);
                    fileUpload.SaveAs(path);
                    var result = _importService.ImportCompanyTempToDatabase(path);//ImportCompanyToDatabase(path);
                    TempData["importResult"] = result;

                    //EmptyFolder(Server.MapPath("~/Content/files/ExportImport"));
                    //_adminService.DeleteProfileCompanyTemp();
                    return RedirectToAction("GoToReviewPage", new { page = 1, searchType = -1 });
                }
                catch (Exception ex)
                {
                    LogHelper.ImportExportCompanyError("Import error:", ex);
                    return RedirectToAction("AdminCompanyList");
                }

            }
            return null;
        }

        public ActionResult ClearData()
        {
            if (!AuthorizationAdmin())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { result = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            _adminService.DeleteProfileCompanyTemp();
            return RedirectToAction("AdminCompanyList");
        }
        [HttpPost]
        public ActionResult CheckExistedTempDate()
        {
            if (_adminService.IsHaveTempDate())
            {
                return Json(new { isExisted = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isExisted = false }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GoToReviewPage(int? page, int? searchType)
        {

            if (!AuthorizationAdmin())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { isSuccess = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int seachType1 = searchType ?? -1;
            int page1 = page ?? 1;

            List<ProfileCompanyTemp> lstCompany = _adminService.GetListProfileCompany(page1, 10, out totalRecord, seachType1);
            ViewBag.Page = page1;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.SearchType = searchType;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CompanyListReviewResults");
            }
            return View("AdminReviewPageList");
        }
        public bool isDirectoryContainFiles(string path)
        {
            if (!Directory.Exists(path)) return false;
            return Directory.EnumerateFiles(path, "*", System.IO.SearchOption.AllDirectories).Any();
        }
        public void EmptyFolder(string path)
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
        public ActionResult AdminCompanyImportDataResults()
        {
            if (TempData["importResult"] != null)
                ViewBag.ImportResult = TempData["importResult"];
            else
                ViewBag.ImportResult = string.Empty;
            return View();
        }
        #endregion

        [HttpPost]
        public ActionResult SetCompanyshowCataloryServices(int companyId, bool typeCheck)
        {
            var profileCompany = _companyProfileService.GetProfileCompanyByID(companyId);
            profileCompany.IsShowCatagory = typeCheck;
            _companyProfileService.UpdateProfileCompany(profileCompany);
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetCompanyIsEventServices(int companyId, bool typeCheck)
        {
            var profileCompany = _companyProfileService.GetProfileCompanyByID(companyId);
            profileCompany.IsEvent = typeCheck;
            _companyProfileService.UpdateProfileCompany(profileCompany);
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetCompanyIsClassServices(int companyId, bool typeCheck)
        {
            var profileCompany = _companyProfileService.GetProfileCompanyByID(companyId);
            profileCompany.IsClass = typeCheck;
            _companyProfileService.UpdateProfileCompany(profileCompany);
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportDataToDB()
        {
            if (!AuthorizationAdmin())
            {
                if (Request.IsAjaxRequest())
                {
                    var loginUrl = Url.Action("Login", "Admin");
                    return Json(new { isSuccess = false, iAuthen = false, returnUrl = loginUrl });
                }
                return AdminUnauthorizedResult();
            }
            _adminService.ImportData();
            _adminService.DeleteProfileCompanyTemp();
            return Json(new { isSuccess = true, iAuthen = true }, JsonRequestBehavior.AllowGet);
        }


        #region Concierge



        #endregion concierge




        #region Event


        public ActionResult Events(int? page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            int totalRecord = 0;
            int pageIndex = page ?? 1;

            if (pageIndex < 1)
                pageIndex = 1;

            List<Kuyam.Database.Event> data = _adminService.AdminGetListEventByKeyName(key, pageIndex, 10, out totalRecord);

            int tmp;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.Data = data;
            ViewBag.Page = pageIndex;
            ViewBag.Key = key;
            return View();
        }

        [HttpPost]
        public ActionResult EventCreate(Kuyam.Database.Event model)
        {
            if (model.Name.Trim() == "")
                return Json(new
                {
                    status = false,
                    message = "please input event name."
                });

            try
            {
                _adminService.CreateEvent(model);
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult EventModify(Kuyam.Database.Event model)
        {
            if (model.Name.Trim() == "")
                return Json(new
                {
                    status = false,
                    message = "please input name."
                });

            try
            {
                _adminService.ModifyEvent(model);
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }

            return Json(new { status = true });
        }

        public ActionResult CompaniesForEachEvent(int? id, int? page, string key)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            if (!id.HasValue)
            {
                return RedirectToAction("Events", "Admin");
            }

            int totalRecord = 0;
            int page1 = page ?? 1;

            List<ProfileCompany> lstCompany = _adminService.AdminGetListCompanyJoinEvent(id.Value, out totalRecord, page1, 10);

            ViewBag.Page = page1;
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.CompanyEventIds = _adminService.GetListCompanyIdsByEventId(id.Value);
            ViewBag.Key = key;
            ViewBag.Event = _adminService.GetEventById(id.Value);
            ViewBag.IsAdmin = AuthorizationAdmin();
            return View();
        }

        [HttpPost]
        public ActionResult CompaniesForEachEvent(int id, string page, string key, int type)
        {
            //Check Authorization
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 1;

            if (!int.TryParse(page, out pageIndex))
                pageIndex = 1;
            int totalRecord = 0;
            List<ProfileCompany> lstCompany = new List<ProfileCompany>();


            if (type == 0)
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(key, (int)Types.CompanyStatus.Active, pageIndex, 10, out totalRecord);
            }
            else if (type == 1)
            {
                lstCompany = _adminService.AdminGetListCompanyJoinEvent(id, out totalRecord, pageIndex, 10);
            }
            else
            {
                lstCompany = _adminService.AdminGetListCompanyNotJoinEvent(id, out totalRecord, pageIndex, 10);
            }

            
            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.CompanyEventIds = _adminService.GetListCompanyIdsByEventId(id);
            ViewBag.Key = key;
            ViewBag.Page = pageIndex;
            ViewBag.Event = _adminService.GetEventById(id);

            return PartialView("_AdminCompaniesEvent");
        }


        [HttpPost]
        public ActionResult ShowCompaniesForEvent(int id, int type)
        {
            //Check Authorization
            if (!AuthorizationAdminOrAgent())
            {
                return AdminUnauthorizedResult();
            }
            int pageIndex = 1;

            int totalRecord = 0;
            List<ProfileCompany> lstCompany = new List<ProfileCompany>();

            if (type == 0)
            {
                lstCompany = _adminService.AdminGetListCompanyByKeyName(string.Empty, (int)Types.CompanyStatus.Active, pageIndex, 10, out totalRecord);
            }
            else if (type == 1)
            {
                lstCompany = _adminService.AdminGetListCompanyJoinEvent(id, out totalRecord, pageIndex, 10);
            }
            else
            {
                lstCompany = _adminService.AdminGetListCompanyNotJoinEvent(id, out totalRecord, pageIndex, 10);
            }


            ViewBag.TotalRecords = totalRecord;
            ViewBag.CompanyList = lstCompany;
            ViewBag.CompanyEventIds = _adminService.GetListCompanyIdsByEventId(id);
            ViewBag.Key = string.Empty;
            ViewBag.Page = pageIndex;
            ViewBag.Event = _adminService.GetEventById(id);

            return PartialView("_AdminCompaniesEvent");
        }


        public ActionResult CompanyServicesToEvent(int? profileId, int? eventId, int? typeId)
        {

            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            if (!profileId.HasValue || !eventId.HasValue)
            {
                return RedirectToAction("Events");
            }

            if(typeId.HasValue && typeId.Value == (int)Types.ServiceType.ClassType)
            {

                List<CompanyService> scList = _classService.GetClassesbyProfileId(profileId.Value);
                ViewBag.CompanyServices = scList;
                var company = DAL.GetProfileCompany(profileId.Value);
                ViewBag.CompanyName = company != null ? company.Name : string.Empty;
                ViewBag.EventName = _adminService.GetEventNameById(eventId.Value);
                ViewBag.EventId = eventId;
                var companyEventId = _adminService.GetCompanyEventIdByProfileIdEventId(profileId.Value, eventId.Value);

                if (companyEventId > 0)
                {
                    ViewBag.CompanyServiecsToEvent = _adminService.GetListClassesToEventByCompanyEventId(companyEventId);
                    ViewBag.CompanyEventID = companyEventId;
                }
                else
                {
                    CompanyEvent ce = new CompanyEvent();
                    ce.ProfileCompanyID = profileId.Value;
                    ce.EventID = eventId.Value;
                    ViewBag.CompanyEventID = _adminService.CreateCompanyEvent(ce);
                }

            }
            else
            {
                List<CompanyService> scList = _classService.GetServiceCompanybyProfileId(profileId.Value);
                ViewBag.CompanyServices = scList;
                var company = DAL.GetProfileCompany(profileId.Value);
                ViewBag.CompanyName = company != null ? company.Name : string.Empty;
                ViewBag.EventName = _adminService.GetEventNameById(eventId.Value);
                ViewBag.EventId = eventId;
                var companyEventId = _adminService.GetCompanyEventIdByProfileIdEventId(profileId.Value, eventId.Value);

                if (companyEventId > 0)
                {
                    ViewBag.CompanyServiecsToEvent = _adminService.GetListCompanyServicesToEventByCompanyEventId(companyEventId);
                    ViewBag.CompanyEventID = companyEventId;
                }
                else
                {
                    CompanyEvent ce = new CompanyEvent();
                    ce.ProfileCompanyID = profileId.Value;
                    ce.EventID = eventId.Value;
                    ViewBag.CompanyEventID = _adminService.CreateCompanyEvent(ce);
                }

            }
            
            return View();
        }

        [HttpPost]
        public ActionResult AddCompanyServiceToEvent(Kuyam.Database.CompanyServiceEvent model)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            try
            {
                _adminService.AddCompanyServiceToEvent(model);
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult RemoveCompanyServiceToEvent(int id)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            try
            {
                _adminService.DeleteCompanyServiceToEvent(id);
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }

            return Json(new { status = true });
        }

        [HttpPost]
        public ActionResult RemoveCompanyEvent(int profileId, int eventId)
        {
            if (!AuthorizationAdmin())
            {
                return AdminUnauthorizedResult();
            }

            try
            {
                _adminService.RemoveCompanyEvent(profileId, eventId);
            }
            catch (InvalidDataException ide)
            {
                LogHelper.Error("A data is in an invalid format", ide);
                return Json(new
                {
                    status = false,
                    message = ide.Message
                });
            }

            return Json(new { status = true });
        }


        #endregion
    }

}
