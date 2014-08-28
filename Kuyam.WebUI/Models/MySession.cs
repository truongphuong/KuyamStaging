using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using Kuyam.Database;
using System.Security.Principal;
using System.Collections.Generic;
using Kuyam.Domain;
using System.Diagnostics;
using Kuyam.Utility;
using System.Linq;

namespace Kuyam.WebUI.Models
{
    public static class MySession
    {

        #region Const

        private const string CookieName = "kuyam.customer";

        #endregion

        #region Fields

        public static InfoConnServiceReference.ConnectorSource FacebookConnectorSource { get; set; }
        public static InfoConnServiceReference.ConnectorSource GoogleConnectorSource { get; set; }
        public static int FlagPage { get; set; }
        public static int FlagCalendarType { get; set; }
        public static HttpPostedFileBase iCalUpload { get; set; }

        #endregion

        #region Utilities

        public static HttpCookie GetCustCookie()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return null;

            return HttpContext.Current.Request.Cookies[CookieName];
        }

        public static void SetCustCookie(Guid customerGuid)
        {
            if (HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                var cookie = new HttpCookie(CookieName);
                cookie.HttpOnly = true;
                cookie.Value = customerGuid.ToString();
                if (customerGuid == Guid.Empty)
                {
                    cookie.Expires = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    int cookieExpires = ConfigManager.CookieExpires;
                    cookie.Expires = DateTime.Now.AddHours(cookieExpires);
                }

                HttpContext.Current.Response.Cookies.Remove(CookieName);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        #endregion

        #region method

        public static Cust GetAuthenticatedUser()
        {
            if (HttpContext.Current == null ||
                HttpContext.Current.Request == null ||
                !HttpContext.Current.Request.IsAuthenticated ||
                !(HttpContext.Current.User.Identity is FormsIdentity))
            {
                return null;
            }

            var formsIdentity = (FormsIdentity)HttpContext.Current.User.Identity;
            var user = GetAuthenticatedUserFromTicket(formsIdentity.Ticket);



            if (user != null && user.Status == (int)Types.UserStatusType.Active)
            {
                //if (user.GetRole.Contains("Admin") || user.GetRole.Contains("Agent")
                //    || user.GetRole.Contains("HotelAdmin") || user.GetRole.Contains("HotelStaff") || user.GetRole.Contains("HotelConcierge"))
                //{
                //    var hotelStaff = DAL.GetConcierge(user.CustID);
                //    if (MySession.Concierge == null)
                //    {
                //        MySession.HotelId = hotelStaff != null ? hotelStaff.HotelID : 0;
                //        MySession.Concierge = hotelStaff;
                //    }
                //}

                if (ImpersonateId.HasValue && ImpersonateId > 0)
                {
                    var impersonatedCust = GetUserImpersonateById(ImpersonateId.Value);
                    if (impersonatedCust != null && impersonatedCust.Status == (int)Types.UserStatusType.Active)
                    {
                        if (MySession.HotelId > 0)
                        {
                            if (Concierge == null)
                                Concierge = Cust.GetConcierge(user.CustID);
                        }
                        OriginalCustIfImpersonated = user;
                        user = impersonatedCust;

                    }
                }
            }


            return user;
        }

        public static Cust GetAuthenticatedUserFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");
            var usernameOrEmail = ticket.Name;
            if (String.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            var user = Cust.Load(usernameOrEmail);
            return user;

        }

        public static Cust GetUserImpersonateById(int id)
        {
            if (HttpContext.Current.Items["HttpContext.UserImpersonate"] == null)
            {
                var user = Cust.Load(id);
                HttpContext.Current.Items["HttpContext.UserImpersonate"] = user;
                return user;
            }
            return (Cust)HttpContext.Current.Items["HttpContext.UserImpersonate"];

        }

        public static TimeZoneInfo GetCustomerTimeZone(Cust customer)
        {
            TimeZoneInfo timeZoneInfo = null;

            string timeZoneId = string.Empty;
            if (customer != null)
                timeZoneId = customer.TimeZoneId;

            try
            {
                if (!String.IsNullOrEmpty(timeZoneId))
                    timeZoneInfo = DateTimeUltility.FindTimeZoneById(timeZoneId);
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            if (timeZoneInfo == null)
                timeZoneInfo = DateTimeUltility.DefaultStoreTimeZone;

            return timeZoneInfo;
        }

        public static void EndImpersonation()
        {
            ImpersonateId = 0;
            //Init(OrgUsername);
        }

        public static MvcHtmlString Dump()
        {
            return Util.FormatDebugValues("Session", "Account", "Cust", CustID, "Profile", ProfileID, "Company", ProfileID);
        }

        #endregion method

        #region Properties

        /// <summary>
        /// Loads a cust on EVERY call
        /// </summary>
        public static Cust Cust
        {
            get
            {
                var cust = GetAuthenticatedUser();
                if (cust == null)
                {
                    var custCookie = GetCustCookie();
                    if (custCookie != null && !String.IsNullOrEmpty(custCookie.Value))
                    {
                        Guid custGuid;
                        if (Guid.TryParse(custCookie.Value, out custGuid))
                        {
                            var customerByCookie = DAL.GetCustByGuid(custGuid);
                            if (customerByCookie != null)
                                cust = customerByCookie;
                        }
                    }
                    if (cust != null)
                        SetCustCookie(cust.AspUserID);
                }

                return cust;
            }
        }

        public static HotelStaff Concierge
        {
            get
            {
                return Session["Concierge"] as HotelStaff;
            }
            set
            {
                Session["Concierge"] = value;
            }

        }

        public static bool DebugMode { get; set; }

        public static HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        public static string CompanyEmployeeBusyOnOff
        {
            get
            {
                string status = (string)Session["CompanyEmployeeBusyOnOff"];
                return status ?? "on";
            }
            set
            {
                Session["CompanyEmployeeBusyOnOff"] = value;
            }
        }

        public static double Latitude
        {
            get
            {

                return Convert.ToInt32(Session["Latitude"]);
            }
            set
            {
                Session["Latitude"] = value;
            }
        }

        public static double Longitude
        {
            get
            {

                return Convert.ToDouble(Session["Longitude"]);
            }
            set
            {
                Session["Longitude"] = value;
            }
        }


        public static Guid AspUserID
        {
            get
            {
                if (Cust == null)
                    return Guid.Empty;
                return Cust.AspUserID;
            }

        }

        public static int CustID
        {
            get
            {
                if (Cust == null)
                    return 0;
                return Cust.CustID;
            }

        }

        public static int ProfileID
        {
            get
            {
                if (Cust == null)
                    return 0;
                return Cust.GetCompanyProfileID();

            }
        }

        public static int HotelId
        {
            get
            {

                return Convert.ToInt32(Session["HotelId"]);
            }
            set
            {
                Session["HotelId"] = value;
            }
        }

        public static int? ImpersonateId
        {
            get
            {
                return Convert.ToInt32(Session["impersonateId"]);
            }

            set
            {
                Session["impersonateId"] = value;
            }
        }

        public static int? ImpersonatedFrom
        {
            get
            {
                return Convert.ToInt32(Session["ImpersonatedFrom"]);
            }

            set
            {
                Session["ImpersonatedFrom"] = value;
            }
        }

        public static Cust OriginalCustIfImpersonated
        {
            get
            {
                return Session["OriginalCustIfImpersonated"] as Cust;
            }
            set
            {
                Session["OriginalCustIfImpersonated"] = value;
            }
        }

        public static string OrgUsername
        {
            get
            {
                return Session["orgUsername"].ToString();
            }

            set
            {
                Session["orgUsername"] = value;
            }
        }

        public static string Username
        {
            get
            {
                if (Cust == null)
                    return string.Empty;
                return Cust.Username;
            }

        }

        public static string FirstName
        {
            get
            {
                if (Cust == null)
                    return string.Empty;
                return Cust.FirstName;
            }
        }

        public static List<string> Messages
        {
            get
            {
                List<string> ret = Session["messages"] as List<String>;
                if (ret == null)
                {
                    ret = new List<String>();
                    Messages = ret;
                }
                return ret;
            }

            set
            {
                Session["messages"] = value;
            }
        }

        public static string ViewName
        {
            get
            {
                return Session["viewName"] == null ? null : Session["viewName"].ToString();
            }
            set
            {
                Session["viewName"] = value;
            }
        }

        public static string ViewDate
        {
            get
            {
                return Session["viewDate"] == null ? null : Session["viewDate"].ToString();
            }
            set
            {
                Session["viewDate"] = value;
            }
        }

        public static string SchedApptDT
        {
            get
            {
                return Session["SchedApptDT"] == null ? null : Session["SchedApptDT"].ToString();
            }
            set
            {
                Session["SchedApptDT"] = value;
            }
        }

        public static TimeZoneInfo CurrentTimeZone
        {
            get
            {
                return GetCustomerTimeZone(MySession.Cust);
            }
            set
            {
                string timeZoneId = string.Empty;
                if (value != null)
                {
                    timeZoneId = value.Id;
                }

                var customer = MySession.Cust;
                if (customer != null)
                {
                    customer.TimeZoneId = timeZoneId;
                    DAL.UpdateRec(customer);
                }
            }
        }

        public static RegisterModel RegisterModel
        {
            get { return Session["RegisterModel"] as RegisterModel; }
            set { Session["RegisterModel"] = value; }
        }

        public static Appointment Appointmentbooking
        {
            get { return Session["Appointmentbooking"] as Appointment; }
            set { Session["Appointmentbooking"] = value; }
        }

        public static int AppointmentbookingTempId
        {
            get { return Session["AppointmentbookingTempId"] is int ? (int)Session["AppointmentbookingTempId"] : 0; }
            set { Session["AppointmentbookingTempId"] = value; }
        }

        //public static int DiscountId
        //{
        //    get
        //    {
        //        int DiscountId = 0;
        //        int.TryParse(Session["DiscountId"].ToString(), out DiscountId);
        //        return DiscountId;
        //    }
        //    set { Session["DiscountId"] = value; }
        //}

        public static string DiscountCode
        {
            get
            {
                if (Session["DiscountCode"] == null)
                    return string.Empty;
                return Session["DiscountCode"].ToString();
            }
            set { Session["DiscountCode"] = value; }
        }

        public static string GiftCardCode
        {
            get
            {
                if (Session["GiftCardCode"] == null)
                    return string.Empty;
                return Session["GiftCardCode"].ToString();
            }
            set { Session["GiftCardCode"] = value; }
        }

        public static int GiftCardId
        {
            get
            {
                int DiscountId = 0;
                int.TryParse(Session["GiftCardId"].ToString(), out DiscountId);
                return DiscountId;
            }
            set { Session["GiftCardId"] = value; }
        }

        public static int CalendarId
        {
            get
            {
                int CalendarId = 0;
                int.TryParse(Session["CalendarId"].ToString(), out CalendarId);
                return CalendarId;
            }
            set { Session["CalendarId"] = value; }
        }

        public static bool IsCashAppoiment
        {
            get { return Convert.ToBoolean(Session["IsCashAppoiment"]); }
            set { Session["IsCashAppoiment"] = value; }
        }

        public static UserPackagePurchase UserPackagePurchase
        {
            get { return Session["UserPackagePurchase"] as UserPackagePurchase; }
            set { Session["UserPackagePurchase"] = value; }
        }

        public static string PreapprovalKey
        {
            get { return Session["PreapprovalKey"] as string; }
            set { Session["PreapprovalKey"] = value; }
        }

        public static int PurchaseCompanyProfileId
        {
            get { return Session["PurchaseCompanyProfileID"] is int ? (int)Session["PurchaseCompanyProfileID"] : 0; }
            set { Session["PurchaseCompanyProfileID"] = value; }
        }

        public static string FacebookAccessToken
        {
            get { return Session["FacebookAccessToken"] as string; }
            set { Session["FacebookAccessToken"] = value; }
        }

        public static string FacebookCsrfToken
        {
            get { return Session["FacebookCsrfToken"] as string; }
            set { Session["FacebookCsrfToken"] = value; }
        }

        public static string PreviousPage
        {
            get { return Session["PreviousPage"] as string; }
            set { Session["PreviousPage"] = value; }
        }

        public static bool ShowLiveChat
        {
            get { return Convert.ToBoolean(Session["ShowLiveChat"]); }
            set { Session["ShowLiveChat"] = value; }
        }

        public static IList<Kuyam.GettyImagesClient.Domain.Image> GettyImages
        {
            get { return Session["GettyImages"] as IList<Kuyam.GettyImagesClient.Domain.Image>; }
            set { Session["GettyImages"] = value; }
        }

        public static int AppoimentID
        {
            get { return Convert.ToInt32(Session["AppoimentID"]); }
            set { Session["AppoimentID"] = value; }
        }

        public static AppointmentReviewModel AppointmentReview
        {
            get { return Session["AppointmentReviewModel"] as AppointmentReviewModel; }
            set { Session["AppointmentReviewModel"] = value; }
        }

        public static bool IsBookDirect
        {
            get
            {
                if (Session["_kuyamIsBookDirectSession"] == null)
                    return false;
                return (bool)Session["_kuyamIsBookDirectSession"];
            }
            set { Session["_kuyamIsBookDirectSession"] = value; }
        }
        #endregion property



    }
}