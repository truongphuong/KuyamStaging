using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using System.Web.Routing;
using Kuyam.WebUI.InfoConnServiceReference;
using System.IO;
using Kuyam.Utility;
using System.Threading;

namespace Kuyam.WebUI.Controllers
{
    [Authorize(Roles = "personal, admin, support, god")]
    public class SettingController : KuyamBaseController
    {
        private readonly CustService _custService;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly EmailSender _emailSender;

        public SettingController(CustService custService, IFormsAuthenticationService formsService,
            IMembershipService membershipService, EmailSender emailSender)
        {
            this._formsService = formsService;
            this._membershipService = membershipService;
            this._custService = custService;
            this._emailSender = emailSender;
        }

        public ActionResult Index()
        {
            return RedirectToAction("UserSetting");
        }
        public ActionResult UserSetting()
        {
            Cust customner = _custService.GetCustomerCustID(MySession.CustID);
            var model = customner.ToUserModel();
            model.FirstName = model.FirstName;
            model.FirstListItem = GetListAlertTime(model.FirstAlert);
            model.SecondListItem = GetListAlertTime(model.SecondAlert);
            return View(model);
        }

        [HttpPost]
        public ActionResult UserSetting(UserModel model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                Cust cust = _custService.GetCustomerCustID(MySession.CustID);
                int preferredPhoneTypeID = 0;

                if (model.EmailType == true)
                {
                    preferredPhoneTypeID |= (int)Types.PreferredPhone.Email;
                }
                if (model.TextType == true)
                {
                    preferredPhoneTypeID |= (int)Types.PreferredPhone.Text;
                }
                //if (model.CallType == true)
                //{
                //    preferredPhoneTypeID |= (int)Types.PreferredPhone.Call;
                //}            

                cust.PreferredPhoneTypeID = preferredPhoneTypeID;
                cust.FirstAlert = model.FirstAlert;
                cust.SecondAlert = model.SecondAlert;
                cust.Modified = DateTime.UtcNow;

                var firstAlertTime = GetMinutesByType(model.FirstAlert.Value);               
                var firstAlertRequest = new IncomingRequest
                {
                    EntityId = "KuyamWeb",
                    EntityType =(int)Types.EntityAlertType.FirstAlert,
                    PreviousAlert = firstAlertTime,
                    CustId = MySession.CustID
                };


                Thread firstAlertThread = new Thread(() =>
                {
                    try
                    {
                        InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                        serviceInfo.UpdateUserSetting(firstAlertRequest);                        
                    }
                    catch (Exception ex)
                    {
                        LogHelper.SMS("Save user setting fail:", ex);
                    }

                });

                firstAlertThread.Start();
               
                var SecondAlertTime = GetMinutesByType(model.SecondAlert.Value);
                var SecondAlertRequest = new IncomingRequest
                {
                    EntityId = "KuyamWeb",
                    EntityType = (int)Types.EntityAlertType.SecondAlert,
                    PreviousAlert = SecondAlertTime,
                    CustId = MySession.CustID
                };


                Thread SecondAlertThread = new Thread(() =>
                {
                    try
                    {
                        InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                        serviceInfo.UpdateUserSetting(SecondAlertRequest);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.SMS("save user setting fail:", ex);
                    }

                });

                SecondAlertThread.Start();

                _custService.UpdateCustomer(cust);

                if (file != null)
                {
                    try
                    {
                        InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

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
                        string fileContent = new StreamReader(file.InputStream).ReadToEnd();

                        InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
                        option.ICSString = fileContent;
                        client.SaveEvents(MySession.CustID, option, InfoConnServiceReference.ConnectorSourceType.iCalendar);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            model.FirstListItem = GetListAlertTime(model.FirstAlert);
            model.SecondListItem = GetListAlertTime(model.SecondAlert);
            string str = "saveChange();";
            ViewBag.PopupString = MvcHtmlString.Create(str);
            return View(model);
        }

        public ActionResult EditUser()
        {
            Cust customner = _custService.GetCustomerCustID(MySession.CustID);
            var model = customner.ToEditModel();
            model.FirstName = customner.FirstName;
            model.LastName = customner.LastName;
            model.FirstListItem = GetListAlertTime(model.FirstAlert);
            model.SecondListItem = GetListAlertTime(model.SecondAlert);
            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            model.SelectCarrier = customner.MobileCarrier;
            Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.MobilePhone));
            if (invite != null)
            {
                ViewBag.Defaultfuntion = "isverification();";
            }
            Session["userphoneNumber"] = UtilityHelper.CleanPhone(model.MobilePhone);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(UserEditModel model, HttpPostedFileBase file)
        {
            ModelState.Remove("Email");

            if (!string.IsNullOrEmpty(model.MobilePhone))
            {
                Invite invite = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(model.MobilePhone));
                string phone = Session["userphoneNumber"] != null ? Session["userphoneNumber"] as string : string.Empty;
                string newphone = UtilityHelper.CleanPhone(model.MobilePhone);
                if (invite == null || (phone != newphone && Session["phoneNumber"] == null))
                {
                    ModelState.AddModelError("VerifiPhoneNumber", "unverified phone number");
                    ViewBag.Defaultfuntion = "setDefaultvalue();";
                }
                else
                {
                    if (invite.PhoneNumber != UtilityHelper.CleanPhone(model.MobilePhone))
                    {
                        ModelState.AddModelError("VerifiPhoneNumber", "phone number do not match");
                        ViewBag.Defaultfuntion = "setDefaultvalue();";
                    }

                }
            }

            model.CarrierList = DAL.GetCarrier((int)Types.TypeGroup.Carrier);
            Cust cust = _custService.GetCustomerCustID(MySession.CustID);

            if (ModelState.IsValid)
            {

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.OldPassword))
                    {
                        if (cust != null && cust.aspnet_Users != null && cust.aspnet_Users.aspnet_Membership != null)
                            model.Email = cust.aspnet_Users.aspnet_Membership.Email;
                        ModelState.AddModelError("UserEditModel", "The current password is incorrect.");
                        return View(model);
                    }

                    if (!_membershipService.ChangePassword(MySession.Cust.Username, model.OldPassword, model.NewPassword))
                    {
                        if (cust != null && cust.aspnet_Users != null && cust.aspnet_Users.aspnet_Membership != null)
                            model.Email = cust.aspnet_Users.aspnet_Membership.Email;
                        ModelState.AddModelError("UserEditModel", "The current password is incorrect.");
                        return View(model);
                    }

                    try
                    {

                        string template = string.Empty;
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/passchange.cshtml")))
                        {

                            template = reader.ReadToEnd();
                        }

                        dynamic myObject = new
                        {

                            Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                            DateChage = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                            Host = Kuyam.WebUI.Helpers.EmailHelper.GetStoreHost(),
                            Email = cust.Username,
                            UserName = cust.FirstName
                        };

                        string templateResult = RazorEngine.Razor.Parse(template, myObject);
                        Kuyam.WebUI.Helpers.EmailHelper.SendEmailChangePassword(string.Empty, cust.Username, templateResult);
                        //System.Threading.Thread oThread = new System.Threading.Thread(() => Kuyam.WebUI.Helpers.EmailHelper.SendEmailChangePassword(model.Email, "", templateResult));

                        //oThread.Start();
                    }
                    catch (Exception ex)
                    {

                        ModelState.AddModelError("Error", ex);
                    }

                }

                cust.MobilePhone = UtilityHelper.CleanPhone(model.MobilePhone);
                cust.Zip = model.Zip;
                int preferredPhoneTypeID = (int)Types.PreferredPhone.Default;

                if (model.EmailType == true)
                {
                    preferredPhoneTypeID |= (int)Types.PreferredPhone.Email;
                }
                if (model.TextType == true)
                {
                    preferredPhoneTypeID |= (int)Types.PreferredPhone.Text;
                }
                if (model.CallType == true)
                {
                    preferredPhoneTypeID |= (int)Types.PreferredPhone.Call;
                }
                cust.PreferredPhoneTypeID = preferredPhoneTypeID;
                cust.FirstAlert = model.FirstAlert;
                cust.SecondAlert = model.SecondAlert;
                cust.Modified = DateTime.Now;
                cust.MobileCarrier = model.SelectCarrier;

                //Trong edit-update lat+lon by zipcode.
                //double lat;
                //double lon;
                //Kuyam.Domain.BusinessService.GetLatAndLonByAreaCode(int.Parse(model.Zip), out lat, out lon);
                GeoClass.Coordinate coordinate;
                if (!string.IsNullOrEmpty(model.Zip))
                    coordinate = GeoClass.GetCoordinates(model.Zip);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);

                cust.Longitude = (double)coordinate.Longitude;
                cust.Latitude = (double)coordinate.Latitude;
                //------------------------------------
                _custService.UpdateCustomer(cust);
                if (file != null)
                {
                    try
                    {
                        InfoConnSoapClient service = new InfoConnSoapClient();
                        string fileContent = new StreamReader(file.InputStream).ReadToEnd();
                        InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
                        option.ICSString = fileContent;
                        service.SaveEvents(MySession.CustID, option, ConnectorSourceType.iCalendar);
                    }
                    catch
                    {
                        throw;
                    }
                }
                Session["userphoneNumber"] = null;
                Session["phoneNumber"] = null;
                string str = "saveChange();";
                ViewBag.PopupString = MvcHtmlString.Create(str);

            }
            model.FirstListItem = GetListAlertTime(model.FirstAlert);
            model.SecondListItem = GetListAlertTime(model.SecondAlert);
            model.Email = cust.Username;
            model.SelectCarrier = cust.MobileCarrier;

            return View(model);
        }

        public ActionResult CheckPhoneNumber(string phoneNumber, string email)
        {
            //string phone = Session["phoneNumber"] != null ? Session["phoneNumber"] as string : string.Empty;
            //phoneNumber = UtilityHelper.CleanPhone(phoneNumber);
            //if (phoneNumber.Equals(phone) && !string.IsNullOrEmpty(phone))
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //Invite invitebyPhone = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(phoneNumber));
            //Invite iviteByemail = DAL.GetInviteByEmail(email.Trim());
            //if (invitebyPhone != null && iviteByemail != null)
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //return Json(false, JsonRequestBehavior.AllowGet);

            string phone = Session["userphoneNumber"] != null ? Session["userphoneNumber"] as string : string.Empty;
            phoneNumber = UtilityHelper.CleanPhone(phoneNumber);

            if (!string.IsNullOrEmpty(phone) && phoneNumber.Equals(phone))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }


            Invite i = DAL.GetInviteForSMSVerify(phoneNumber, email);
            if (i != null && i.Active == true)
            {
                return Json((int)Kuyam.Database.Types.FlagInvite.Verified, JsonRequestBehavior.AllowGet);
            }


            //string oldPhone = UtilityHelper.CleanPhone(MySession.Cust.MobilePhone);
            //Invite iviteByemail = DAL.GetInviteByEmail(email.Trim());
            //Invite invitebyPhone = DAL.GetInviteByPhoneNumber(UtilityHelper.CleanPhone(phoneNumber));
            //if (invitebyPhone != null && iviteByemail != null && oldPhone == phoneNumber)
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private List<SelectListItem> GetListAlertTime(int? selected)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (Types.AlertTime item in Enum.GetValues(typeof(Types.AlertTime)))
            {
                list.Add(new SelectListItem()
                {
                    Text = GetNameAlert(item),//UtilityHelper.ConvertEnum(item.ToString()),
                    Value = ((int)item).ToString(),
                    Selected = (selected.HasValue && item == (Types.AlertTime)selected)
                });
            }
            return list;
        }

        /// <summary>
        ///  count day
        /// </summary>
        /// <param name="type"></param>
        /// <returns> minutes of day </returns>
        private int GetMinutesByType(int type)
        {
            //const int SECOND = 1;
            const int MINUTE = 1;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            switch (type)
            {
                case (int)Types.AlertTime.none:
                    return -1;
                case (int)Types.AlertTime.AtTimeOfEvent:
                    return 1;
                case (int)Types.AlertTime.FifteenMinBefore:
                    return 15;
                case (int)Types.AlertTime.ThirtyMinBefore:
                    return 30;
                case (int)Types.AlertTime.OneHourBefore:
                    return HOUR;
                case (int)Types.AlertTime.TwoHoursBefore:
                    return 2 * HOUR;
                case (int)Types.AlertTime.OneDayBefore:
                    return DAY;
                case (int)Types.AlertTime.TwoDaysBefore:
                    return 2 * DAY;

            }

            return -1;
        }

        private string GetNameAlert(Types.AlertTime type)
        {
            switch (type)
            {
                case Types.AlertTime.none:
                    return "none";
                case Types.AlertTime.AtTimeOfEvent:
                    return "at time of event ";
                case Types.AlertTime.FifteenMinBefore:
                    return "15 min before";
                case Types.AlertTime.ThirtyMinBefore:
                    return "30 min before";
                case Types.AlertTime.OneHourBefore:
                    return "1 hour before";
                case Types.AlertTime.TwoHoursBefore:
                    return "2 hours before";
                case Types.AlertTime.OneDayBefore:
                    return "1 day before";
                case Types.AlertTime.TwoDaysBefore:
                    return "2 days before";
            }
            return string.Empty;
        }
        public JsonResult CheckOldPassword(string OldPassword)
        {
            Cust customner = _custService.GetCustomerCustID(MySession.CustID);
            string haspassWord = customner.aspnet_Users.aspnet_Membership.Password;
            //string passWord = _formsService.CreatePasswordHash(OldPassword,"SHA1");
            //if (haspassWord.Equals(passWord))
            //   return Json(true);            
            return Json("The old password do not match.", JsonRequestBehavior.AllowGet);
        }

        #region Calendar Management
        public ActionResult CalendarManagement()
        {
            Cust customner = _custService.GetCustomerCustID(MySession.CustID);
            ViewBag.CalendarList = _custService.GetActiveCalendarsbyCustId(customner.CustID);
            return View();
        }

        [HttpPost]
        public ActionResult AddCalendar(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _custService.AddDefaultCalendar(MySession.CustID, name);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteCalendar(string id)
        {
            try
            {
                int calendarId = 0;
                if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out calendarId))
                {
                    _custService.DeleteCalendar(MySession.CustID, calendarId);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult UpdateCalendar(string id, string name)
        {
            try
            {
                int calendarId = 0;
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name) && Int32.TryParse(id, out calendarId))
                {
                    _custService.UpdateCalendar(calendarId, name);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion Calendar Management
    }
}
