using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Net;
using Kuyam.WebUI.Models;
using Kuyam.Utility;
using Kuyam.Database;

namespace Kuyam.WebUI.Helpers
{

    public class GoogleHelper
    {
        private string clientID;

        public GoogleHelper()
        {
            clientID = ConfigurationManager.AppSettings["InfoConn.Connector.Google.ClientIdentifier"];
        }
        /// <summary>
        /// Insert To Google Calendar by Token
        /// </summary>
        /// <param name="googleToken"></param>
        public void InsertToGoogleCalendar(string googleToken)
        {

            var jsonSerializer = new JavaScriptSerializer();

            #region Get google calendarID

            var request = WebRequest.Create("https://www.googleapis.com/calendar/v3/users/me/calendarList?pp=1&key=" + clientID) as HttpWebRequest;
            request.Headers.Add("Accept-Charset", "utf-8");
            request.KeepAlive = true;
            request.Method = "GET";
            request.Headers.Add("Authorization", "OAuth " + googleToken.ToString());
            WebResponse response = request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream());
            var calendar = jsonSerializer.Deserialize<GoogleCalendarList>(stream.ReadToEnd().Trim());

            var calendarID = calendar.Items.Where(x => x.AccessRole == "owner").Select(x => x.ID).FirstOrDefault();

            #endregion

            #region Insert a event

            request = WebRequest.Create("https://www.googleapis.com/calendar/v3/calendars/" + calendarID + "/events?pp=1&key=" + clientID) as HttpWebRequest;
            request.Headers.Add("Accept-Charset", "utf-8");
            request.KeepAlive = true;
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add("Authorization", "OAuth " + googleToken.ToString());

            //Get appointment

            Kuyam.Database.Appointment appointment = Kuyam.Database.ProfileCompany.GetAppointmentById(MySession.AppoimentID);
            var actEvent = new GoogleCalendarEvent();
            if (appointment != null && MySession.AppoimentID > 0)
            {

                string description = string.Empty;
                DateTime start = DateTimeUltility.ConvertToUserTime(appointment.Start.ToUniversalTime(), DateTimeKind.Utc);
                DateTime end = DateTimeUltility.ConvertToUserTime(appointment.End.ToUniversalTime(), DateTimeKind.Utc);
               
                var serviceName = appointment.ServiceCompany != null ? appointment.ServiceCompany.Service.ServiceName : appointment.ServiceName;
                int? duration = appointment.ServiceCompany != null ? appointment.ServiceCompany.Duration : appointment.Duration;
                decimal? price = appointment.ServiceCompany != null ? appointment.ServiceCompany.Price : appointment.Price;
                int? attendeesNumber = appointment.ServiceCompany != null ? appointment.ServiceCompany.AttendeesNumber : appointment.AttendeesNumber;

                description = string.Format("{0} {1} min, ${2}, {3} person", serviceName, duration, price, attendeesNumber);

                actEvent = new GoogleCalendarEvent
                {
                    summary = appointment.Title,
                    description = description,
                    start = new GoogleCalendarEventTime(start),
                    end = new GoogleCalendarEventTime(end)
                };
            }
            //Post event
            var data = jsonSerializer.Serialize(actEvent);
            var postData = Encoding.UTF8.GetBytes(data);
            Stream ws = request.GetRequestStream();
            ws.Write(postData, 0, postData.Length);
            ws.Close();
            response = request.GetResponse();
            stream = new StreamReader(response.GetResponseStream());
            var result = stream.ReadToEnd().Trim();
            MySession.AppoimentID = 0;
            #endregion
        }
    }
}

class GoogleCalendarEvent
{
    public string summary { get; set; }
    public string description { get; set; }
    public GoogleCalendarEventTime start { get; set; }
    public GoogleCalendarEventTime end { get; set; }
}
class GoogleCalendarList
{
    public string Kind { get; set; }
    public string Etag { get; set; }
    public List<GoogleCalendar> Items { get; set; }
}
class GoogleCalendar
{
    public string Kind { get; set; }
    public string Etag { get; set; }
    public string ID { get; set; }
    public string Summary { get; set; }
    public string TimeZone { get; set; }
    public string AccessRole { get; set; }
}

class GoogleCalendarEventTime
{
    private DateTime _datetime;

    public string date
    {
        get
        {
            return _datetime.ToString("yyyy-MM-dd");
        }
    }

    public GoogleCalendarEventTime(DateTime time)
    {
        _datetime = time;
    }
}