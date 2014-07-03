using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using Kuyam.WebUI.InfoConnServiceReference;
using Facebook;
using System.IO;
using Kuyam.Utility;


namespace Kuyam.WebUI.Controllers
{
    public class CalendarSettingController : KuyamBaseController
    {
        #region Fields

        private readonly CalendarService _calendarService;
        private readonly IMembershipService _membershipService;

        AuthenticationFacebook oAuthFacbook = new AuthenticationFacebook(new SettingService());
        AuthenticationGoogle oAuthGoogle = new AuthenticationGoogle(new SettingService());

        InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

        #endregion

        #region Ctor

        public CalendarSettingController(CalendarService calendarService, IMembershipService membershipService){
            this._calendarService = calendarService;
            this._membershipService = membershipService;
        }

        #endregion

        #region Functions

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Calendars = _calendarService.GetActiveCalendarsbyCustId(MySession.CustID);
            ViewBag.IsFacebookAccount = (MySession.Cust != null && !string.IsNullOrEmpty(MySession.Cust.FacebookUserID));
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddCalendar(string name, string backColor)
        {

            string color = string.Empty;
            foreach (var c in Kuyam.Database.Constants.colors){
                if (c.Key.Trim().ToLower()==backColor.Trim().ToLower()){
                    color = c.Value;
                }
            }
            bool result = false;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(backColor))
            {
                var calendars = _calendarService.GetActiveCalendarsbyCustId(MySession.CustID);
                if (calendars.Any(c => c.Name.ToLower() == name.ToLower()))
                    return Json(new {result = result, message = "this calendar is already exits"},JsonRequestBehavior.AllowGet);
                result = _calendarService.AddCalendar(MySession.CustID, name, color, (int)Types.CalendarType.Default,string.Empty);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateCalendar(int id, string name, string backColor)
        {

            string color = string.Empty;
            foreach (var c in Kuyam.Database.Constants.colors){
                if (c.Key.Trim().ToLower() == backColor.Trim().ToLower()){
                    color = c.Value;
                }
            }

            bool result = false;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(backColor))
            {
                var calendars = _calendarService.GetActiveCalendarsbyCustId(MySession.CustID);
                if (calendars.Any(c => c.Name.ToLower() == name.ToLower() && c.CalendarID != id ))
                    return Json(new { result = result, message = "this calendar is already exits" }, JsonRequestBehavior.AllowGet);

                result = _calendarService.UpdateCalendar(id, name, color);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteCalendar(int id, string pass)
        {
            // not facebook account
            if (MySession.Cust != null && string.IsNullOrEmpty(MySession.Cust.FacebookUserID))
            {
                if (!_membershipService.ValidateUser(MySession.Cust.Username, pass))
                {
                    return Json(2, JsonRequestBehavior.AllowGet);
                }
            }

            bool result = _calendarService.DeleteCalendar(MySession.CustID, id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCalendar(int id)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Kuyam.Database.Calendar calendar = _calendarService.GetCalendarByCalendarId(id);
            string color = string.Empty;
            if (calendar != null){
                foreach (var c in Kuyam.Database.Constants.colors){
                    if (c.Value.Trim().ToLower() == calendar.BackColor.Trim().ToLower()){
                        color = c.Key.ToLower();
                    }
                }
                result.Add("calName", calendar.Name);
                result.Add("calBackColor", calendar.BackColor);
                result.Add("calBackColorName", color);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public void AddCalendars(List<InfoConnServiceReference.Calendar> lstCalendar, string color, int calendarType, string calendarName ="")
        {
            bool result = false;
            //Delete list calendar
            _calendarService.DeleteCalendars(MySession.CustID, calendarType);
            //Add list calendar
            if (lstCalendar != null && lstCalendar.Count > 0)
            {
                foreach (InfoConnServiceReference.Calendar calendar in lstCalendar)
                {
                    if (!string.IsNullOrEmpty(calendar.Summary) && !string.IsNullOrEmpty(color))
                    {    
                        if (!string.IsNullOrEmpty(calendarName))
                             calendar.Summary= calendarName;
                        result = _calendarService.AddCalendar(MySession.CustID, calendar.Summary, color, calendarType,calendar.CalendarId);
                    }
                }
            }
        }

        #endregion

        #region InfoConn Service

        public ActionResult FacebookInfoConnService()
        {
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            var connectorSourceExist = client.GetConnectorSource(user.CustID, InfoConnServiceReference.ConnectorSourceType.Facebook);

            if (connectorSourceExist!=null){
                LogOutFacebook(connectorSourceExist.AccessToken);
            }

            if (user != null)
            {
                if (Request["code"] == null)
                {
                    return new RedirectResult(oAuthFacbook.AuthorizationLinkGet(), true);
                }
                else
                {
                    var conectorAuth = oAuthFacbook.AccessTokenGet(Request["code"]);
                    var connectorSource = new InfoConnServiceReference.ConnectorSource
                    {
                        UserId = user.CustID,
                        AccessToken = conectorAuth.AccessToken,
                        RefressToken = conectorAuth.RefressToken,
                        ExpiresDate = conectorAuth.ExpiresDate,
                        ConnectorSourceType = (int)ConnectorSourceType.Facebook,
                        LastModified = DateTime.UtcNow,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.UtcNow,
                        CacheLastUpdate_Longer = DateTime.UtcNow,
                        CacheLastUpdate_Medium = DateTime.UtcNow,
                        DoCacheUpdate_Longer = true,
                        DoCacheUpdate_Medium = true,
                        DoCacheUpdate_Short = true,
                        Username = conectorAuth.Username
                    };
                    List<InfoConnServiceReference.Calendar> calendars = new List<InfoConnServiceReference.Calendar>();

                    try{
                        calendars = client.AddConnectorSource(connectorSource).ToList();
                    }
                    catch (Exception ex){
                        LogHelper.Error("Insert facebook calendar to service fail:", ex);
                    }

                    AddCalendars(calendars, Kuyam.Database.Constants.colors["Slate Blue"], (int)Types.CalendarType.Facebook);
                    return Redirect(LogOutFacebook(connectorSource.AccessToken));
                }
            }
            return RedirectToAction("Index","Home");

        }

        public ActionResult GoogleInfoConnService()
        {
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

            if (user != null){
                if (Request["code"] == null){

                    return new RedirectResult(oAuthGoogle.AuthorizationLinkGet(), true);
                }
                else{

                    var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);

                    if (MySession.AppoimentID > 0 && conectorAuth != null){

                        Kuyam.WebUI.Helpers.GoogleHelper gg = new Helpers.GoogleHelper();
                        gg.InsertToGoogleCalendar(conectorAuth.AccessToken);
                        return RedirectToAction("Index", "Appointment");
                    }else{

                        var connectorSource = new InfoConnServiceReference.ConnectorSource{

                            UserId = user.CustID,
                            AccessToken = conectorAuth.AccessToken,
                            RefressToken = conectorAuth.RefressToken,
                            ExpiresDate = conectorAuth.ExpiresDate,
                            ConnectorSourceType = (int)ConnectorSourceType.Google,
                            LastModified = DateTime.UtcNow,
                            IsUpdateRunning = false,
                            CacheLastUpdate_Short = DateTime.UtcNow,
                            CacheLastUpdate_Longer = DateTime.UtcNow,
                            CacheLastUpdate_Medium = DateTime.UtcNow,
                            DoCacheUpdate_Longer = true,
                            DoCacheUpdate_Medium = true,
                            DoCacheUpdate_Short = true,
                            Username = conectorAuth.Username
                        };
                        List<InfoConnServiceReference.Calendar> calendars = new List<InfoConnServiceReference.Calendar>();

                        try{
                            calendars = client.AddConnectorSource(connectorSource).ToList();
                        }
                        catch (Exception ex){
                            LogHelper.Error("Insert google calendar to service fail:", ex);
                        }

                        //List<InfoConnServiceReference.Calendar> calendars = client.AddConnectorSource(connectorSource).ToList();
                        AddCalendars(calendars, Kuyam.Database.Constants.colors["Deep Sky Blue"], (int)Types.CalendarType.Google);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ICalInfoConnService()
        {

            HttpPostedFileBase iCalUpload = Request.Files[0] as HttpPostedFileBase;

            var connectorSource = new InfoConnServiceReference.ConnectorSource
            {
                UserId = MySession.CustID,
                ConnectorSourceType = (int)ConnectorSourceType.iCalendar,
                ExpiresDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsUpdateRunning = false,
                CacheLastUpdate_Short = DateTime.UtcNow,
                CacheLastUpdate_Longer = DateTime.UtcNow,
                CacheLastUpdate_Medium = DateTime.UtcNow,
                DoCacheUpdate_Longer = true,
                DoCacheUpdate_Medium = true,
                DoCacheUpdate_Short = true
            };
            string calendarName = System.IO.Path.GetFileNameWithoutExtension(iCalUpload.FileName);
            List<InfoConnServiceReference.Calendar> calendars = client.AddConnectorSource(connectorSource).ToList();
            AddCalendars(calendars, Kuyam.Database.Constants.colors["Lime Green"], (int)Types.CalendarType.iCal, calendarName);

            string fileContent = new StreamReader(iCalUpload.InputStream).ReadToEnd();

            InfoConnServiceReference.SearchOption option = new InfoConnServiceReference.SearchOption();
            option.ICSString = fileContent;
            option.Name = System.IO.Path.GetFileNameWithoutExtension(iCalUpload.FileName);

            try{
                client.SaveEvents(MySession.CustID, option, InfoConnServiceReference.ConnectorSourceType.iCalendar);
            }
            catch (Exception ex){
                LogHelper.Error("Insert iCal calendar to service fail:", ex);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Oauth2Callback()
        {

            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

            var error = Request["error"];
            if (user != null && error != Kuyam.Database.Constants.GoogleErrorText){

                var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);

                if (MySession.AppoimentID > 0 && conectorAuth != null){
                    Kuyam.WebUI.Helpers.GoogleHelper gg = new Helpers.GoogleHelper();
                    gg.InsertToGoogleCalendar(conectorAuth.AccessToken);
                    return RedirectToAction("Index", "Appointment");
                }
                else{

                    var connectorSource = new InfoConnServiceReference.ConnectorSource{
                        UserId = user.CustID,
                        AccessToken = conectorAuth.AccessToken,
                        RefressToken = conectorAuth.RefressToken,
                        ExpiresDate = conectorAuth.ExpiresDate,
                        ConnectorSourceType = (int)ConnectorSourceType.Google,
                        LastModified = DateTime.UtcNow,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.UtcNow,
                        CacheLastUpdate_Longer = DateTime.UtcNow,
                        CacheLastUpdate_Medium = DateTime.UtcNow,
                        DoCacheUpdate_Longer = true,
                        DoCacheUpdate_Medium = true,
                        DoCacheUpdate_Short = true,
                        Username = conectorAuth.Username
                    };
                    List<InfoConnServiceReference.Calendar> calendars = new List<InfoConnServiceReference.Calendar>();
                    try{
                        calendars = client.AddConnectorSource(connectorSource).ToList();
                    }
                    catch (Exception ex){
                        LogHelper.Error("Insert google calendar to service fail:", ex);
                    }
                    AddCalendars(calendars, Kuyam.Database.Constants.colors["Deep Sky Blue"], (int)Types.CalendarType.Google);
                }
            }
            return RedirectToAction("Index");
        }

        public string LogOutFacebook(string accessToken){
            var oauth = new FacebookClient();
            string urlRedirect = string.Format("{0}CalendarSetting", Kuyam.WebUI.Helpers.EmailHelper.GetStoreHost());
            var logoutParameters = new Dictionary<string, object>
                  {
                     {"access_token", accessToken},
                      { "next", urlRedirect }
                  };
            var logoutUrl = oauth.GetLogoutUrl(logoutParameters);
            return logoutUrl.ToString();
        }

        #endregion

    }
}
