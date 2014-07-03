using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Kuyam.WebUI.Models;
using System.IO;
using Kuyam.Utility;
using Kuyam.Database;

namespace Kuyam.WebUI.Helpers
{
    public class FacebookHelper
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookHelper"/> class.
        /// </summary>
        public FacebookHelper()
        {
            _appId = ConfigurationManager.AppSettings["InfoConn.Connector.Facebook.ClientIdentifier"];
            _appSecret = ConfigurationManager.AppSettings["InfoConn.Connector.Facebook.ClientSecret"];
            _appScope = ConfigurationManager.AppSettings["InfoConn.Connector.Facebook.Scope"];
            _returnUri = ConfigurationManager.AppSettings["InfoConn.Connector.Facebook.CallBackAuthentication"];
            _facebookClient = new FacebookClient();
        }
        #endregion

        #region Private Properties

        private string _appId;
        private string _appSecret;
        private string _appScope;
        private string _returnUri;
        private FacebookClient _facebookClient;

        #endregion

        #region Public methods

        /// <summary>
        /// Handles the facebook callback.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="state">The state.</param>
        /// <returns>Link to navigate </returns>
        public string HandleFacebookCallback(string code, string state)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                return null;

            // first validate the csrf token
            dynamic decodedState;
            try
            {
                decodedState = _facebookClient.DeserializeJson(Encoding.UTF8.GetString(Convert.FromBase64String(state)), null);
                var exepectedCsrfToken = MySession.FacebookCsrfToken;
                // make the fb_csrf_token invalid
                MySession.FacebookCsrfToken = null;

                if (!(decodedState is IDictionary<string, object>) || !decodedState.ContainsKey("csrf") || string.IsNullOrWhiteSpace(exepectedCsrfToken) || exepectedCsrfToken != decodedState["csrf"])
                {
                    return null;
                }
            }
            catch
            {
                // log exception
                return null;
            }

            try
            {
                dynamic result = _facebookClient.Post("oauth/access_token",
                                          new
                                          {
                                              client_id = _appId,
                                              client_secret = _appSecret,
                                              redirect_uri = _returnUri,
                                              code = code
                                          });

                MySession.FacebookAccessToken = result["access_token"];

                //if (result.ContainsKey("expires"))
                //    Session["fb_expires_in"] = DateTime.Now.AddSeconds(result.expires);

                if (decodedState.ContainsKey("returnUrl"))
                {
                    return decodedState["returnUrl"];
                }

                return null;
            }
            catch
            {
                // log exception
                return null;
            }
        }

        /// <summary>
        /// Logs to facebook.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public string LogToFacebook(string returnUrl)
        {
            var csrfToken = Guid.NewGuid().ToString();
            MySession.FacebookCsrfToken = csrfToken;

            var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(_facebookClient.SerializeJson(new { returnUrl = returnUrl, csrf = csrfToken })));

            var fbLoginUrl = _facebookClient.GetLoginUrl(
                new
                {
                    client_id = _appId,
                    client_secret = _appSecret,
                    redirect_uri = _returnUri,
                    response_type = "code",
                    scope = _appScope,
                    state = state
                });

            return fbLoginUrl.AbsoluteUri;
        }


        /// <summary>
        /// Gets the personal info.
        /// </summary>
        /// <returns></returns>
        public dynamic GetPersonalInfo()
        {
            try
            {
                _facebookClient.AccessToken = MySession.FacebookAccessToken;

                //return _facebookClient.Get("me/accounts");
                return _facebookClient.Get("me");
            }
            catch (FacebookOAuthException)
            {
                // log exception
                return new HttpUnauthorizedResult();
            }
        }

        //trong add
        /// <summary>
        /// Push a event to facebook by grap API
        /// </summary>
        /// <returns></returns>
        public string CreateEvent(){
            _facebookClient.AccessToken = MySession.FacebookAccessToken;
            
            Kuyam.Database.Appointment appointment = Kuyam.Database.ProfileCompany.GetAppointmentById(MySession.AppoimentID);

            try{
                if (appointment != null && MySession.AppoimentID > 0){

                    Dictionary<string, object> createEventParameters = new Dictionary<string, object>();
                    string format = "{0:yyyy-MM-ddTHH:mm:ss}";

                    string name = string.IsNullOrEmpty(appointment.Title) ? string.Format("kuyam event") : appointment.Title;
                    string description = string.Format("kuyam event");
                    string start = appointment.Start == null ? string.Format(format, DateTime.UtcNow) : string.Format(format, DateTimeUltility.ConvertToUserTime(appointment.Start.ToUniversalTime(), DateTimeKind.Utc));
                    string end = appointment.End==null?string.Format(format, DateTime.UtcNow.AddHours(2)):string.Format(format, DateTimeUltility.ConvertToUserTime(appointment.End.ToUniversalTime(), DateTimeKind.Utc));

                    if (appointment.ServiceCompany != null && appointment.ServiceCompany.ProfileCompany != null){
                        description = string.Format("{0}, {1} min, ${2}, {3} person",
                            appointment.ServiceCompany.Service.ServiceName,
                            appointment.ServiceCompany.Duration, appointment.ServiceCompany.Price,
                            appointment.ServiceCompany.AttendeesNumber);
                    }

                    createEventParameters.Add("name", name);
                    createEventParameters.Add("start_time", start);
                    createEventParameters.Add("end_time", end);
                    createEventParameters.Add("privacy", "SECRET");
                    createEventParameters.Add("description", description);

                    JsonObject result = _facebookClient.Post("/me/events", createEventParameters) as JsonObject;
                    MySession.AppoimentID = 0;
                    LogHelper.Info(string.Format("Pushed to Facebook eventID: {0}, name: {1}, start: {2}, end: {3}, des: {4}",result["id"].ToString(),appointment.Title,appointment.Start,appointment.End,description));
                    return result["id"].ToString();
                }
            }
            catch (Exception ex){
                if (appointment != null && MySession.AppoimentID > 0){
                    string tile="event";
                    if (appointment.ServiceCompany != null && appointment.ServiceCompany.ProfileCompany != null)
                    {
                        tile = string.Format("{0}, {1} min, ${2}, {3} person",
                            appointment.ServiceCompany.Service.ServiceName,
                            appointment.ServiceCompany.Duration, appointment.ServiceCompany.Price,
                            appointment.ServiceCompany.AttendeesNumber);
                    }
                    LogHelper.Info(string.Format("For Test: Pushed to Facebook name: {0}, start: {1}, end: {2}, des: {3}", appointment.Title, appointment.Start, appointment.End, tile));
                }
                LogHelper.Error("Push facebook event is fail:", ex);
                return null;
            }
            
            return null;
        }

        #endregion
    }
}