using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using Kuyam.Utility;
using Kuyam.Domain;
using Kuyam.Domain.CompanyProfileServices;


namespace Kuyam.WebUI.Controllers
{
    [Authorize(Roles = "personal, admin, support")]
    public class CalendarViewController : KuyamBaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly CompanyProfileService _companyProfileService;

        public CalendarViewController(IAppointmentService appointmentService, CompanyProfileService companyProfileService)
        {
            _appointmentService = appointmentService;
            _companyProfileService = companyProfileService;
        }

        public ActionResult Index(long? tick)
        {

            //Reload Events
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            Cust user = MySession.Cust;
            //client.RefreshUserCalendars(user.CustID, CacheType.Short);

            ViewBag.CurrentMonth = DateTime.Now.Month - 1;
            ViewBag.CurrentYear = DateTime.Now.Year;
            ViewBag.CurrentDay = DateTime.Now.Day;
            if (tick != null)
            {
                Session["CurrentTick"] = tick.Value;
                long tickVal = 0;
                if (long.TryParse(Session["CurrentTick"].ToString(), out tickVal))
                {
                    DateTime currentTime = DateTimeUltility.ConvertFromUnixTimestampMS(tickVal);
                    ViewBag.CurrentDay = currentTime.Day;
                    ViewBag.CurrentMonth = currentTime.Month - 1;
                    ViewBag.CurrentYear = currentTime.Year;
                }
            }
            else
                Session["CurrentTick"] = null;
            InfoConnSoapClient service = new InfoConnSoapClient();
            ViewBag.IsFBConnected = false;
            ViewBag.IsGoogleConnected = false;


            var connectorSource = client.GetConnectorSource(user.CustID, InfoConnServiceReference.ConnectorSourceType.Facebook);

            if (connectorSource == null)
                ViewBag.IsFBConnected = true;

            connectorSource = client.GetConnectorSource(user.CustID, InfoConnServiceReference.ConnectorSourceType.Google);
            if (connectorSource == null)
                ViewBag.IsGoogleConnected = true;

            //var test = service.GetBusyInfo(603);

            return View();
        }

        public ActionResult Setting()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetCadlendars()
        {
            var start = Request.Params["start"];
            var end = Request.Params["end"];
            var calendarId = 0;
            DateTime _start = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(start));
            DateTime _end = DateTimeUltility.ConvertFromUnixTimestamp(double.Parse(end));

            if (Session["CurrentTick"] != null)
            {
                long tick = 0;
                if (long.TryParse(Session["CurrentTick"].ToString(), out tick))
                {
                    DateTime currentTime = DateTimeUltility.ConvertFromUnixTimestampMS(tick);
                    _start = new DateTime(currentTime.Year, currentTime.Month, 1);
                    _end = new DateTime(currentTime.Year, currentTime.Month, 1).AddMonths(1).AddDays(-1);
                    ViewBag.CurrentDay = currentTime.Day;
                }
            }
            ViewBag.CurrentMonth = _start.Month - 1;
            ViewBag.CurrentYear = _start.Year;

            return Json(GetData(_start, _end, calendarId), JsonRequestBehavior.AllowGet);
        }

        private List<CalendarObject> GetData(DateTime start, DateTime end, int calendarId)
        {

            var calendarObject = new List<CalendarObject>();

            try
            {
                var syncCalendar = _appointmentService.GetCalendarIdById(calendarId);
                var service = new InfoConnSoapClient();

                var searchOption = new SearchOption
                {
                    CalendarId = syncCalendar.SyncCalendarId,
                    StartDate = DateTimeUltility.ConvertToUtcTime(start, DateTimeUltility.CurrentTimeZone),
                    EndDate = DateTimeUltility.ConvertToUtcTime(end, DateTimeUltility.CurrentTimeZone),
                    ConnectorSourceType = ConnectorSourceType.Google
                };

                var user = MySession.Cust;

                var eventsGoogle = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.Google);
                //var eventsAppointment = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.iCalendar);

                var eventsAppointment = ProfileCompany.GetKuyamEvents(user.CustID);

                searchOption.ConnectorSourceType = ConnectorSourceType.Facebook;
                var eventsFacebook = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.Facebook);

                //For iCal
                searchOption.ConnectorSourceType = ConnectorSourceType.iCalendar;
                var eventsiCal = service.GetEvents(user.CustID, searchOption, ConnectorSourceType.iCalendar);
                if (eventsiCal != null)
                {
                    foreach (var evt in eventsiCal)
                    {
                        var flag = true;
                        var current = evt.StartDate;
                        while (flag)
                        {
                            var item = new CalendarObject
                                {
                                    id = evt.Id.ToString(CultureInfo.InvariantCulture),
                                    title = evt.Summary,
                                    start = current.ToString(CultureInfo.InvariantCulture),
                                    end = current.ToString(CultureInfo.InvariantCulture),
                                    className = "icalapp"
                                };
                            calendarObject.Add(item);

                            if ((current.Year == evt.EndDate.Year && current.Month == evt.EndDate.Month && current.Day == evt.EndDate.Day) || current > evt.EndDate)
                            {
                                flag = false;
                            }
                            current = current.AddDays(1);
                        }
                    }
                }


                //For Google
                if (eventsGoogle != null)
                {
                    foreach (var evt in eventsGoogle)
                    {
                        var flag = true;
                        var current = evt.StartDate;
                        while (flag){
                            var item = new CalendarObject{
                                    id = evt.Id.ToString(CultureInfo.InvariantCulture),
                                    title = evt.Summary,
                                    start = current.ToString(CultureInfo.InvariantCulture),
                                    end = current.ToString(CultureInfo.InvariantCulture),
                                    className = "ggapp"
                                };
                            calendarObject.Add(item);

                            if ((current.Year == evt.EndDate.Year && current.Month == evt.EndDate.Month && current.Day == evt.EndDate.Day) || current > evt.EndDate)
                            {
                                flag = false;
                            }
                            current = current.AddDays(1);
                        }
                    }
                }

                //For Facebook
                if (eventsFacebook != null)
                {
                    foreach (var evt in eventsFacebook)
                    {
                        var flag = true;
                        var current = evt.StartDate;
                        while (flag)
                        {
                            var item = new CalendarObject
                                {
                                    id = evt.Id.ToString(CultureInfo.InvariantCulture),
                                    title = evt.Summary,
                                    start = current.ToString(CultureInfo.InvariantCulture),
                                    end = current.ToString(CultureInfo.InvariantCulture),
                                    className = "fbapp"
                                };
                            calendarObject.Add(item);

                            if ((current.Year == evt.EndDate.Year && current.Month == evt.EndDate.Month && current.Day == evt.EndDate.Day) || current > evt.EndDate)
                            {
                                flag = false;
                            }
                            current = current.AddDays(1);
                        }
                    }
                }
                if (eventsAppointment != null)
                {
                    calendarObject.AddRange(eventsAppointment.Select(evt => new CalendarObject
                        {
                            id = evt.AppointmentID.ToString(CultureInfo.InvariantCulture), 
                            title = evt.Appointment.Title, 
                            start = evt.StartDate.ToString(CultureInfo.InvariantCulture), 
                            end = evt.EndDate.ToString(CultureInfo.InvariantCulture), 
                            className = "kuyamapp"
                        }));
                }



            }
            catch (Exception ex)
            {
                LogHelper.Error("Get calendar is error: ",ex);
            }

            return calendarObject;
        }
    }
}
