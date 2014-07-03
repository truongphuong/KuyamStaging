using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using Kuyam.Domain;
using M2.Util.MVC;
using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using Kuyam.WebUI.InfoConnServiceReference;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Kuyam.WebUI.Helpers;
using System.Web;
using Kuyam.Utility;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly EmailSender _emailSender;
        private readonly CustService _custService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly IAppointmentService _appointmentService;

        public AccountController(IFormsAuthenticationService formsService,
            IMembershipService membershipService,
            AdminService adminService,
            EmailSender emailSender, 
            CustService custService,
            CompanyProfileService companyProfileService,
            IAppointmentService appointmentService)
        {
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._emailSender = emailSender;
            this._custService = custService;
            this._companyProfileService = companyProfileService;
            this._appointmentService = appointmentService;
        }

        #region signup

        [HttpPost]
        public ActionResult SignUp(string email, string name, string lname)
        {
            LogHelper.Info("Sign up by email request - email: " + email);
            AccountHelper.AddInviteCode(email, name, lname);
            return new EmptyResult();
        }

        #endregion

        #region Login

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (_membershipService.ValidateUser(model.UserName, model.Password))
                {
                    Cust customer = DAL.xGetCust(model.UserName);
                    if (customer == null)
                        return View(model);

                    if (customer.Status != (int)Types.UserStatusType.Active)
                    {
                        string x = "your account is not active. please contact support.";
                        LogHelper.Info(string.Format("{0} can't login because: {1}", model.UserName, x));
                        ModelState.AddModelError("", x);
                        return View(model);
                    }

                    if (customer == null || customer.Status != (int)Types.UserStatusType.Active)
                    {
                        string x = "your account is not active. please contact support.";
                        ModelState.AddModelError("", x);
                        LogHelper.Info(string.Format("{0} can't login because: {1}", model.UserName, x));
                        return View(model);
                    }

                    LogHelper.Info(string.Format("{0} login success", model.UserName));

                    _formsService.SignIn(model.UserName, model.RememberMe/* createPersistentCookie */);

                }
                else
                {
                    LogHelper.Info("log in fail for email: " + model.UserName + " because the user name or password provided is incorrect.");
                    ModelState.AddModelError("", "the user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Logout()
        {
            LogHelper.Info(MySession.Username + " log out");
            if (MySession.ImpersonateId > 0)
            {
                MySession.ImpersonateId = 0;
                return RedirectToAction("logout", "home");
            }
            _formsService.SignOut();
            Session.Abandon();
            return RedirectToAction("logout", "home");
        }

        [HttpPost]
        public ActionResult LoginAjax(string email, string password, string url, string timeZoneId)
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                bool validateUser = _membershipService.ValidateUser(email, password);
                if (validateUser)
                {
                    Cust customer = DAL.xGetCust(email);
                    if (customer != null)
                    {
                        if (customer.Status != (int)Types.UserStatusType.Active)
                        {
                            string x = "your account is not active. please contact support.";
                            return Json(new { status = "false", message = x }, JsonRequestBehavior.AllowGet);
                        }


                        string windowsTimeZoneId = DateTimeUltility.TimeZoneToTimeZoneInfo(timeZoneId);
                        customer.TimeZoneId = windowsTimeZoneId;
                        DAL.UpdateRec(customer, customer.CustID);

                        MySession.ImpersonateId = 0;
                        MySession.OriginalCustIfImpersonated = null;
                        MySession.HotelId = 0;
                        MySession.Concierge = null;

                        _formsService.SignIn(email, false/* createPersistentCookie */); // change from true to false to use false by default

                        //Get a appointment review
                        GetAppointmentReview(customer.CustID);

                        LogHelper.Info(email + " login success");
                        result.Add("status", "true");
                        result.Add("linkurl", url);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { status = "false", message = "the user name or password is invalid please try again." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "false", message = ex.Message.ToLower() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LogoutAjax()
        {
            LogHelper.Info(MySession.Username + " log out");
            var model = new LogOutModel();
            if (MySession.ImpersonateId > 0)
            {
                MySession.ImpersonateId = 0;
                MySession.OriginalCustIfImpersonated = null;
                MySession.HotelId = 0;
                MySession.Concierge = null;
                model.IsOriginalPage = MySession.ImpersonatedFrom;
                if (MySession.ImpersonatedFrom == 1)
                {
                    model.RedirectUrl = Url.RouteUrl("userlist");
                }
                else if (MySession.ImpersonatedFrom == 2)
                {
                    model.RedirectUrl = Url.RouteUrl("conciergeGustlist");
                }
                else if (MySession.ImpersonatedFrom == 3)
                {
                    model.RedirectUrl = Url.RouteUrl("conciergeAppointment");
                }
                else if (MySession.ImpersonatedFrom == 4)
                {
                    model.RedirectUrl = Url.RouteUrl("conciergeProposals");
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            _formsService.SignOut();
            Session.Abandon();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        private void GetAppointmentReview(int custId)
        {
            ///Appointment apt = DAL.GetAppointmentByCustId(custId);
            var apt = _appointmentService.GetAppointmentReview(custId);
            if (apt != null)
            {
                AppointmentReviewModel aptReview = new AppointmentReviewModel();
                if (apt != null && apt.ServiceCompany != null
                    && apt.ServiceCompany.Service != null
                    && apt.ServiceCompany.ProfileCompany != null
                    && apt.CompanyEmployee != null)
                {
                    aptReview = new AppointmentReviewModel
                    {
                        Id = apt.AppointmentID,
                        CompanyName = apt.ServiceCompany.ProfileCompany.Name,
                        EmployeeName = apt.CompanyEmployee.EmployeeName,
                        ServiceName = apt.ServiceCompany.Service.ServiceName,
                        ServiceDate = String.Format("{0:ddd, MMMM d} at {0:HH:mm tt}", apt.Start),
                        ServiceCompanyID = apt.ServiceCompany.ServiceCompanyID
                    };
                    MySession.AppointmentReview = aptReview;
                }
            }
        }


        #region verify phone number

        [HttpPost]
        public ActionResult SendInviteCode(string phoneNumber, string firstName, string lastName, string email)
        {
            string phone = UtilityHelper.CleanPhone(phoneNumber);
            string inviteCode = AccountHelper.AddInviteCode(email, firstName, lastName, phone, (int)Types.InviteType.SMSVerify);
            if (inviteCode == Types.FlagInvite.Verified.ToString())
            {
                return Json((int)Types.FlagInvite.Verified, JsonRequestBehavior.AllowGet);
            }
            string message = "thanks for verifying your mobile number with kuyam. your unique verification code is:";
            if (!string.IsNullOrEmpty(inviteCode))
            {
                string strphoneNumber = UtilityHelper.CleanPhone(phoneNumber);
                if (MyApp.Settings.Admin != null && MyApp.Settings.Admin.EnablePhoneBcc)
                {
                    strphoneNumber += ";";
                    strphoneNumber += MyApp.Settings.Admin.PhoneNumber;
                }
                InfoConnSoapClient service = new InfoConnSoapClient();
                IncomingRequest obj = new IncomingRequest
                {
                    EntityId = "KuyamWeb",
                    Data = UtilityHelper.ObjectToXml(new { Message = string.Format("{0} {1}", message, inviteCode), PhoneNumber = strphoneNumber }),

                };
                service.AddIncomingRequest(obj, IncommingRequestType.SEND_SMS);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.DenyGet);
        }

        public ActionResult CheckPhoneNumber(string phoneNumber, string email)
        {
            string phone = Session["phoneNumber"] != null ? Session["phoneNumber"] as string : string.Empty;
            phoneNumber = UtilityHelper.CleanPhone(phoneNumber);
            if (phoneNumber.Equals(phone) && !string.IsNullOrEmpty(phone))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            Invite invitebyPhone = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(phoneNumber));
            Invite iviteByemail = DAL.GetInviteByEmail(email.Trim());
            if (invitebyPhone != null && iviteByemail != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        //edit

        public ActionResult CheckEmail(string email)
        {
            MembershipUserCollection isEmail = Membership.FindUsersByEmail(email);
            if (isEmail.Count > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SMSVerifyInviteCode(string inviteCode)
        {
            string phoneNumber = AccountHelper.SMSVerifyInviteCode(inviteCode, (int)Types.InviteType.SMSVerify);
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                Session["phoneNumber"] = UtilityHelper.CleanPhone(phoneNumber);
                LogHelper.Info(string.Format("Verify Invite code {0} success", inviteCode));
                return Json(true, JsonRequestBehavior.DenyGet);
            }
            LogHelper.Info(string.Format("Verify Invite code {0} fail", inviteCode));
            return Json(false, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region Register

        public ActionResult Register()
        {
            _formsService.SignOut();
            Session.Abandon();
            return View("RegisterInviteCode");
        }

        [HttpPost]
        public ActionResult RegisterInviteCode(RegisterInviteCode model)
        {
            if (ModelState.IsValid)
            {
                //add info to session variable
                if (MySession.RegisterModel == null)
                    MySession.RegisterModel = new RegisterModel();

                //MySession.RegisterModel.UserName = model.Email;
                MySession.RegisterModel.TestKey = model.TestKey;
                ValidateInviteCode(MySession.RegisterModel);

                Invite inv = DAL.GetInvite(model.TestKey);
                if (inv != null)
                {
                    MySession.RegisterModel.FirstName = inv.Name;
                    MySession.RegisterModel.LastName = inv.LName;
                    MySession.RegisterModel.ContactEmail = inv.Email;
                }

                //if invitecode is valid, continue to page 2, else back to page 1
                if (MySession.RegisterModel.TestKeyIsValid)
                {
                    LogHelper.Info(string.Format("Test key {0} to register success", model.TestKey));
                    return RedirectToAction("RegisterEmail");
                }
                else
                    ModelState.AddModelError(String.Empty, "invalid invite code or invite code was activated");
            }
            LogHelper.Info(string.Format("Test key {0} to register fail", model.TestKey));

            // If we got this far, something failed, redisplay form
            return View("RegisterInviteCode", model);
        }

        public ActionResult RegisterEmail()
        {
            if (MySession.RegisterModel == null || !MySession.RegisterModel.TestKeyIsValid)
                return RedirectToAction("Index", "Home");
            RegisterEmail model = new RegisterEmail();
            model.FirstName = MySession.RegisterModel.FirstName;
            model.LastName = MySession.RegisterModel.LastName;
            model.Email = MySession.RegisterModel.ContactEmail;
            model.Phone = Session["phoneNumber"] as string;
            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            model.IsFacebookRegister = MySession.RegisterModel.IsFacebookRegister;
            Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.Phone));
            if (invite != null)
            {
                ViewBag.Defaultfuntion = "isverification();";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult RegisterEmail(RegisterEmail model)
        {
            if (MySession.RegisterModel == null || !MySession.RegisterModel.TestKeyIsValid)
                return RedirectToAction("Index", "Home");

            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            model.Email = MySession.RegisterModel.ContactEmail;
            model.IsFacebookRegister = MySession.RegisterModel.IsFacebookRegister;

            var cust = Cust.Load(model.Email);
            model.IsGuest = false;
            if (cust != null)
            {
                model.IsGuest = cust.GetRole.Contains("Guest");
            }

            if (ModelState.IsValid)
            {
                //custom validations
                /*
                if (!model.AgreeToNonDisclosure)
                    ModelState.AddModelError(String.Empty, "you must agree to the non-disclosure to continue.");
                */
                if (!model.AgreeToTerms)
                    ModelState.AddModelError(String.Empty, "you must agree to the terms to continue.");

                if (!MySession.RegisterModel.IsFacebookRegister)
                {
                    if (!model.IsGuest)
                    {
                        MembershipUserCollection email = Membership.FindUsersByEmail(model.Email);
                        if (email.Count > 0)
                        {
                            ModelState.AddModelError(String.Empty, "the email is activated.");
                        }
                    }
                }

                bool zipok = DAL.VerifyZipCode(model.ZipCode);
                if (!zipok)
                {
                    //model.ZipCode = "90011";
                    ModelState.AddModelError(String.Empty, "Currently we only support Santa Monica zip codes.");
                }

                //Trong edit
                ZipCode zipCode = _companyProfileService.GetZipCodeByKey(model.ZipCode);

                if (zipCode == null)
                {
                    ModelState.AddModelError(String.Empty, string.Format("{0} is invalid", model.ZipCode));
                    ViewBag.CheckPhone = "checkPhoneNumber($('#txtPhoneBlur').val());";
                }

                if (zipCode != null && zipCode.Active == false)
                {
                    ModelState.AddModelError(String.Empty, string.Format("{0} is inactive", model.ZipCode));
                }

                if (!string.IsNullOrEmpty(model.Phone))
                {
                    Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.Phone));
                    string phone = Session["phoneNumber"] != null ? Session["phoneNumber"] as string : string.Empty;
                    if (invite == null || string.IsNullOrEmpty(phone))
                    {
                        ModelState.AddModelError(string.Empty, "unverified phone number");
                        ViewBag.Defaultfuntion = "setDefaultvalue();";
                    }
                    else
                    {

                        if (invite.PhoneNumber != UtilityHelper.CleanPhone(model.Phone))
                        {
                            ModelState.AddModelError(string.Empty, "phone number do not match");
                            ViewBag.Defaultfuntion = "setDefaultvalue();";
                        }

                    }
                }


                if (!ModelState.IsValid)
                    return View(model);
                //if no session, then restart at beginning
                if (MySession.RegisterModel == null)
                    return RedirectToAction("Index", "Home");

                //DateTime dt = new DateTime();
                //DateTime.TryParse(model.Birthday, out dt);

                MySession.RegisterModel.Birthday = model.Birthday.ToDateTime("MM/dd/yy");
                MySession.RegisterModel.Password = model.Password;
                if (MySession.RegisterModel.IsFacebookRegister)
                    MySession.RegisterModel.Password = Guid.NewGuid().ToString();

                MySession.RegisterModel.GenderTypeID = model.selectgender;
                MySession.RegisterModel.ZipCode = model.ZipCode;
                MySession.RegisterModel.ContactEmail = model.Email;
                MySession.RegisterModel.Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone);
                MySession.RegisterModel.Carrier = model.SelectCarrier.ToString();
                MySession.RegisterModel.FirstName = model.FirstName;
                MySession.RegisterModel.LastName = model.LastName;
                MySession.RegisterModel.IsGuest = model.IsGuest;
                if (!MySession.RegisterModel.IsFacebookRegister)
                    MySession.RegisterModel.UserName = model.Email;
                else
                    MySession.RegisterModel.UserName = MySession.RegisterModel.FacebookUserId;

                MySession.RegisterModel.DoSubmit = true;
                Session["phoneNumber"] = null;

                LogHelper.Info(string.Format("{0} register success", model.Email));
                return SaveCust(MySession.RegisterModel);
                //return RedirectToAction("RegisterName");
            }

            LogHelper.Info(string.Format("{0} register fail because {1}", model.Email, ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception)));
            return View(model);
        }

        public ActionResult RegisterName()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterName(RegisterName model)
        {

            if (ModelState.IsValid)
            {

                //custom validations
                //if (model.DoOtherCalendar == null)
                //    ModelState.AddModelError(String.Empty, "The full name is not empty!");
                if (!ModelState.IsValid)
                    return View(model);

                //if no session, then restart at beginning
                if (MySession.RegisterModel == null)
                    return RedirectToAction("Register");
                if (model.OtherCalendar == "_Other")
                {
                    model.OtherCalendar = null;
                }
                MySession.RegisterModel.UploadUrl = model.UploadUrl;
                //MySession.RegisterModel.PreferredPhone = GetPreferredPhone(model);
                MySession.RegisterModel.IsCompany = model.IsCompany;
                MySession.RegisterModel.DoOutlookCalendar = model.DoOutlookCalendar;
                MySession.RegisterModel.DoYahooCalendar = model.DoYahooCalendar;
                MySession.RegisterModel.OtherCalendar = model.OtherCalendar;
                MySession.RegisterModel.DoSubmit = true;
                LogHelper.Info(string.Format("register name success"));

                return SaveCust(MySession.RegisterModel);
            }

            LogHelper.Info(string.Format("register name fail because {0}", ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception)));

            return RedirectToAction("RegisterName", MySession.RegisterModel);

        }

        [NonAction]
        private ActionResult SaveCust(RegisterModel model)
        {

            //if no session, then restart at beginning
            if (MySession.RegisterModel == null)
                return RedirectToAction("Index", "Home");
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            //a little hack, we're only storing in session on the last page.
            //Maybe this was the wrong way to do this? Should break out into 
            //multiple actions?
            if (MySession.RegisterModel != null)
            {
                //bool doSubmit = model.DoSubmit;
                //model = MySession.RegisterModel;                
                //model.DoSubmit = doSubmit;
                MySession.RegisterModel = null;
            }

            model.GenderList = Types.GetTypeList(Types.TypeGroup.Gender).ToSelectList();
            if (model.DoSubmit)
            {
                //save the new user

                return CreateCustomer(model);
            }
            else if (!String.IsNullOrEmpty(model.FullName))
            {
                //Add the registration into the session
                //build a 'mock' user profile object
                //return the view that shows this mock profile
                MySession.RegisterModel = model;
                return View("RegisterConfirm", model);
            }
            else if (!String.IsNullOrEmpty(model.Password))
            {
                //validate data
                model.FullName = AccountHelper.InviteCodeName(model.TestKey, model.UserName);
                return RedirectToAction("Index", "Home");
            }
            else if (model.TestKeyIsValid)
            {
                //there's probably a more MVC way of doing this
                //but this works, so putting it in.
                ModelState.Clear();

                if (model.Birthday == null) ModelState.AddModelError(String.Empty, "please enter a birthday.");
                if (String.IsNullOrEmpty(model.UserName)) ModelState.AddModelError(String.Empty, "please enter an email.");
                if (String.IsNullOrEmpty(model.Password)) ModelState.AddModelError(String.Empty, "please enter a password.");
                if (String.IsNullOrEmpty(model.ConfirmPassword)) ModelState.AddModelError(String.Empty, "please confirm your password.");
                if (String.IsNullOrEmpty(model.ZipCode)) ModelState.AddModelError(String.Empty, "please enter a zipcode.");
                if (model.Password != model.ConfirmPassword) ModelState.AddModelError(String.Empty, "passwords do not match. Please re-enter the passwords.");

                return View("RegisterEmail", model);
            }
            else
            {
                ModelState.AddModelError(String.Empty, "the supplied invite code is not valid or invite coden is activated.");
                return View("RegisterInviteCode", new RegisterInviteCode() { TestKey = model.TestKey });
            }
        }

        private ActionResult CreateCustomer(RegisterModel model)
        {
            bool isAdmin = false;
            int custID = 0;
            var cust = Cust.Load(model.UserName);
            model.IsGuest = false;

            if (cust != null && cust.GetRole.Contains("Guest"))
            {
                model.IsGuest = cust.GetRole.Contains("Guest");
                cust.FirstName = model.FirstName;
                cust.LastName = model.LastName;
                cust.GenderTypeID = model.GenderTypeID;
                cust.CompanyName = model.CompanyName;
                cust.Birthday = model.Birthday;
                cust.LastVisit = DateTime.Now;
                cust.MobilePhone = model.Phone;
                cust.MobileCarrier = model.Carrier;
                cust.Zip = model.ZipCode;
                cust.Status = (int)Types.UserStatusType.Active;
                cust.Update();
                Roles.RemoveUserFromRole(model.UserName, "Guest");
                custID = cust.CustID;
            }
            else
            {
                MembershipCreateStatus createStatus = _membershipService.CreateUser(model.UserName, model.Password, model.ContactEmail);
                if (createStatus != MembershipCreateStatus.Success)
                {
                    DAL.DeleteAspUser(model.UserName);
                    throw new Exception(AccountValidation.ErrorCodeToString(createStatus));
                }

                bool zipok = DAL.VerifyZipCode(model.ZipCode);

                model.CompanyName = string.Format("{0}'s company", model.FirstName);
                if (model.Birthday == DateTime.MinValue)
                {
                    model.Birthday = DateTime.Now;
                }
                model.CustType = 115; //115 = Personal; 116 = Vendor

                Guid userid = DAL.GetAspUserID(model.UserName);
                try
                {
                    //double lat;
                    //double lon;
                    GeoClass.Coordinate coordinate;
                    if (!string.IsNullOrEmpty(model.ZipCode))
                        coordinate = GeoClass.GetCoordinates(model.ZipCode);
                    else
                        coordinate = new GeoClass.Coordinate(0, 0);

                    custID = Cust.Create(model.UserName, userid, model.FirstName, model.LastName,
                        model.CompanyName, model.Phone, model.Carrier, isAdmin, model.CustType, model.ZipCode, model.PreferredPhone,
                        model.Birthday, model.IsCompany, model.DoYahooCalendar, model.DoOutlookCalendar, model.OtherCalendar, (double)coordinate.Latitude, (double)coordinate.Longitude, 0, model.FacebookUserId);

                    _custService.AddDefaultCalendar(custID, model.FirstName);
                }
                catch (Exception ex)
                {
                    DAL.DeleteUser(model.UserName);
                    throw ex;
                }

                AccountHelper.UpdateInviteUsage(model.TestKey, custID);  // do this last, after eveything is ok           

                //if (model.CustType == (int)Types.CustType.Company)
                //{
                //    Roles.AddUserToRole(model.UserName, "Company");
                //}
                //else
                //{
                //    Roles.AddUserToRole(model.UserName, "Personal");
                //}

                Roles.AddUserToRole(model.UserName, "Personal");

            }


            _formsService.SignIn(model.UserName, false /* createPersistentCookie */);

            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

            if (model.UploadUrl != null)
            {
                var connectorSource = client.GetConnectorSource(MySession.CustID, InfoConnServiceReference.ConnectorSourceType.iCalendar);
                if (connectorSource == null)
                {
                    connectorSource = new InfoConnServiceReference.ConnectorSource
                    {
                        UserId = MySession.CustID,
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
                string fileContent = new StreamReader(model.UploadUrl.InputStream).ReadToEnd();

                //throw new Exception(fileContent);


                InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
                option.ICSString = fileContent;
                client.SaveEvents(MySession.CustID, option, InfoConnServiceReference.ConnectorSourceType.iCalendar);
            }

            try
            {
                //Post Events to server
                if (MySession.FacebookConnectorSource != null)
                {
                    MySession.FacebookConnectorSource.UserId = custID;
                    client.AddConnectorSource(MySession.FacebookConnectorSource);
                }
                if (MySession.GoogleConnectorSource != null)
                {
                    MySession.GoogleConnectorSource.UserId = custID;
                    client.AddConnectorSource(MySession.GoogleConnectorSource);
                }

            }
            catch (Exception ex)
            {
                //Todo: Handle Exception Occur
                LogHelper.Error("Create customer fail:", ex);
            }

            //Don't send the second email when sign up
            //EmailHelper.SendInfoEmail(model.FirstName, model.ContactEmail, model.UserName, model.Password);
            if (model.IsFacebookRegister)
            {

                try
                {
                    string templateInvitecode = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/thankssignupfacebook.cshtml")))
                    {
                        templateInvitecode = reader.ReadToEnd();
                    }

                    // create template data
                    dynamic myObject = new
                    {
                        Email = model.ContactEmail,
                        UserName = model.FirstName.ToString()
                    };

                    // generate the content using razor engine
                    string templateResultInvitecode = RazorEngine.Razor.Parse(templateInvitecode, myObject);
                    System.Threading.Thread oThreadInvitecode = new System.Threading.Thread(() => EmailHelper.SendEmailThanksSignUpFacebook(model.ContactEmail, templateResultInvitecode));
                    oThreadInvitecode.Start();

                    string message = string.Format("Send email  thanks sign up facebook: Email={0}", model.ContactEmail);
                    LogHelper.Info(message);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Send email thanks sign up facebook fail:", ex);
                }
            }

            MySession.RegisterModel = null;

            return RedirectToAction("companysearch", "company");

            //if (model.IsCompany)
            //{
            //    return RedirectToAction("SetupBasic", "Company");
            //}

            //return RedirectToAction("UserSetting", "Setting");
        }

        private void ValidateInviteCode(RegisterModel model)
        {
            model.TestKeyIsValid = AccountHelper.InviteCodeIsValid(model.TestKey);
        }

        /*
        public ActionResult Register1()
        {
            ViewBag.PasswordLength = _membershipService.MinPasswordLength;

            RegisterModel model = new RegisterModel();
            //model.LockAndLoad();

            return View("Register", model);
        }

        [HttpPost]
        public ActionResult Register1(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                bool isAdmin = false;

                // Check invite code
                int typeID = AccountHelper.VerifyInviteCode(model.TestKey);
                if (typeID < 1)
                {
                    ModelState.AddModelError("", "invalid invite code. please contact support for assistance.");
                    return View("Register", model);
                }

                // Attempt to register the user
                MembershipCreateStatus createStatus = _membershipService.CreateUser(model.UserName, model.Password, model.UserName);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    bool zipok = DAL.VerifyZipCode(model.ZipCode);

                    // TODO: Create and link our user and account
                    Guid userid = DAL.GetAspUserID(model.UserName);
                    int accountID = 0;
                    try
                    {
                        accountID = AccountHelper.CreateAccount(zipok, (int)(zipok ? Types.AccountStatus.Ok : Types.AccountStatus.UnservedZipCode));
                    }
                    catch (Exception ex)
                    {
                        DAL.DeleteAspUser(model.UserName);
                        throw ex.GetBaseException();
                    }

                    model.CustType = typeID;
                    model.GenderTypeID = 0; // default value
                    int custID = 0;
                    try
                    {
                        custID = Cust.Create(model.UserName, userid, accountID, model.FirstName, model.LastName,
                            model.CompanyName, null, string.Empty, isAdmin, model.CustType, model.ZipCode, model.PreferredPhone,
                            model.Birthday, model.IsCompany, model.DoYahooCalendar, model.DoOutlookCalendar, model.OtherCalendar, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        DAL.DeleteUser(model.UserName);
                        throw ex;
                    }

                    AccountHelper.UpdateInviteUsage(model.TestKey, custID);  // do this last, after eveything is ok                   

                    if (isAdmin)
                    {
                        Roles.AddUserToRole(model.UserName, "admin");
                    }

                    if (model.CustType == (int)Types.CustType.Company)
                    {
                        Roles.AddUserToRole(model.UserName, "Company");
                    }
                    else
                    {
                        Roles.AddUserToRole(model.UserName, "Personal");
                    }

                    //if (!zipok)
                    //{
                    //	  // TODO: make sure updating account status to 139
                    //    // TODO: where did the noservice form go?
                    //    return View("noservice");
                    //}

                    _formsService.SignIn(model.UserName, false  createPersistentCookie);

                    Notifier.NewCustomerSignedUp();

                    return RedirectToAction("index", "home");
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = _membershipService.MinPasswordLength;
            return View("Register", model);
        }
        
        */

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            return SaveCust(model);
        }

        #endregion

        #region Change password

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = _membershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (_membershipService.ChangePassword(MySession.Username, model.OldPassword, model.NewPassword))
                {
                    //AccountHelper.ForcePasswordChange(User.Identity.Name, false);
                    return RedirectToAction("changepasswordsuccess");
                }
                else
                {
                    ModelState.AddModelError("", "the current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = _membershipService.MinPasswordLength;
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            ResetPasswordModel model = new ResetPasswordModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            try
            {
                string newPass = ControllerUtil.ResetPassword(model.UserName);
                model.Message = "a new password has been sent to the email you entered.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
                return View("ResetPassword", model);
            }

            return View("ResetPasswordDone", model);
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {

            MembershipUserCollection userEmail = Membership.FindUsersByEmail(email);

            if (userEmail == null || userEmail.Count <= 0)
            {
                return Json(false);
            }

            Cust user = DAL.xGetCust(email);

            //string key = Kuyam.Utility.SecurityHelper.EncryptStringToBytesAes(email);
            string key = Kuyam.Utility.SecurityHelper.EncryptText(email, Kuyam.Utility.ConfigManager.CryptKey);

            try
            {
                string template = string.Empty;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/forgotpassword.cshtml")))
                {
                    template = reader.ReadToEnd();
                }
                dynamic myObject = new
                {

                    Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                    Host = EmailHelper.GetStoreHost(),
                    Email = email,
                    UserName = user.FirstName,
                    Key = key
                };

                string templateResult = RazorEngine.Razor.Parse(template, myObject);

                EmailHelper.SendEmailForgotPassword(string.Empty, email, templateResult);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
            }

            return Json(true);
        }

        public ActionResult ConfirmForgotPassword()
        {

            string key = HttpUtility.UrlDecode(Request.Params["key"]);
            string email = Kuyam.Utility.SecurityHelper.DecryptText(key, Kuyam.Utility.ConfigManager.CryptKey);
            Cust user = DAL.xGetCust(email);

            try
            {

                string template = string.Empty;
                string newPass = ControllerUtil.ResetPassword(email);
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/resetpassworduser.cshtml")))
                {
                    template = reader.ReadToEnd();
                }

                dynamic myObject = new
                {

                    Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                    Host = EmailHelper.GetStoreHost(),
                    Email = email,
                    UserName = user.FirstName,
                    NewPassword = newPass
                };

                string templateResult = RazorEngine.Razor.Parse(template, myObject);

                System.Threading.Thread oThread = new System.Threading.Thread(() => EmailHelper.SendEmailUserResetPassword(email, templateResult));

                oThread.Start();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
            }
            return RedirectToAction("index", "home");
        }

        public ActionResult ForgotPasswordNew()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPasswordNew(string password, string email)
        {
            _formsService.SignOut();
            Session.Abandon();
            bool result = ChangePassword(email, password);

            if (!result)
            {
                return Json(false);
            }

            return Json(true);
        }

        public bool ChangePassword(string username, string newPassword)
        {
            bool result = false;

            if (!string.IsNullOrWhiteSpace(username))
            {

                MembershipUserCollection users = Membership.FindUsersByName(username);

                if (users != null || users.Count > 0)
                {

                    MembershipUser user = users[username];
                    string oldPassword = user.ResetPassword();
                    if (user != null)
                    {
                        if (user.IsLockedOut)
                            user.UnlockUser();
                        user.ChangePassword(oldPassword, newPassword);
                        result = true;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Facebook

        /// <summary>
        /// Log to facebook.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public ActionResult LogToFacebook(string returnUrl, string redirectTo)
        {
            // Sign out current account
            _formsService.SignOut();
            Session.Clear();

            MySession.PreviousPage = Request.UrlReferrer.AbsolutePath;
            if (!string.IsNullOrEmpty(redirectTo) && !redirectTo.Equals("undefined"))
                MySession.PreviousPage = redirectTo;
            FacebookHelper facebookHelper = new FacebookHelper();
            return Redirect(facebookHelper.LogToFacebook(returnUrl));

        }

        /// <summary>
        /// Handle Facebook callback.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public ActionResult FacebookCallback(string code, string state)
        {
            FacebookHelper facebookHelper = new FacebookHelper();
            string nextPage = facebookHelper.HandleFacebookCallback(code, state);
            if (string.IsNullOrEmpty(nextPage))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(nextPage);
        }

        /// <summary>
        /// Register by facebook account
        /// </summary>
        /// <returns></returns>
        public ActionResult RegisterByFacebook()
        {
            FacebookHelper facebookHelper = new FacebookHelper();
            dynamic personalInfo = facebookHelper.GetPersonalInfo();
            if (personalInfo == null)
                return RedirectToAction("Index", "Home");

            var user = Membership.GetUser(personalInfo["id"]);
            if (user != null)
            {
                return RedirectToAction("LoginByFacebook");
            }


            try
            {
                RegisterModel registerModel = new RegisterModel();
                registerModel.IsFacebookRegister = true;
                registerModel.FacebookUserId = personalInfo["id"];
                registerModel.FirstName = personalInfo["first_name"];
                registerModel.LastName = personalInfo["last_name"];
                try
                {
                    registerModel.GenderTypeID = personalInfo["gender"] == "male" ? 24 : 25;
                }
                catch
                {
                    registerModel.GenderTypeID = 24;
                }
                registerModel.ContactEmail = personalInfo["email"];
                registerModel.ConfirmEmail = registerModel.ContactEmail;
                registerModel.UserName = registerModel.ContactEmail;
                registerModel.TestKeyIsValid = true;
                MySession.RegisterModel = registerModel;
                return RedirectToAction("RegisterEmail");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Logins the by facebook.
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginByFacebook()
        {
            FacebookHelper facebookHelper = new FacebookHelper();
            dynamic personalInfo = facebookHelper.GetPersonalInfo();
            if (personalInfo == null)
                return RedirectToAction("Index", "Home");

            if (Membership.GetUser(personalInfo["id"]) != null)
            {
                Cust accountCust = DAL.xGetCust(personalInfo["id"]);
                if (accountCust != null)
                {
                    if (accountCust.Status != (int)Types.UserStatusType.Active)
                    {
                        string error = "your account is not active. please contact support.";
                        TempData["Error"] = error;
                        return RedirectToAction("Fail", "Error");
                    }

                    if (accountCust.Status != (int)Types.UserStatusType.Active)
                    {
                        string error = "your account is not active. please contact support.";
                        LogHelper.Info(string.Format("{0} can't login because: {1}", personalInfo["id"], error));
                        TempData["Error"] = error;
                        return RedirectToAction("Fail", "Error");
                    }

                    _formsService.SignIn(personalInfo["id"], false /* createPersistentCookie */);
                    if (MySession.PreviousPage != null)
                        return Redirect(MySession.PreviousPage);
                    return RedirectToAction("CompanySearch", "Company");
                }
            }
            else
            {
                return RedirectToAction("RegisterByFacebook");
            }
            return RedirectToAction("Index", "Home");
        }
        //Trong
        /// <summary>
        /// Log to facebook.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public ActionResult ConnectToFacebook(string returnUrl, string redirectTo)
        {
            MySession.PreviousPage = Request.UrlReferrer.AbsolutePath;
            if (!string.IsNullOrEmpty(redirectTo) && !redirectTo.Equals("undefined"))
                MySession.PreviousPage = redirectTo;
            FacebookHelper facebookHelper = new FacebookHelper();
            return Redirect(facebookHelper.LogToFacebook(returnUrl));

        }

        public ActionResult PushOnFacebook()
        {
            FacebookHelper facebookHelper = new FacebookHelper();
            string evtId = facebookHelper.CreateEvent();
            if (!string.IsNullOrEmpty(evtId))
            {
                return RedirectToAction("Index", "Appointment");
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
