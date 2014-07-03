using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Utility;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Models;
using Kuyam.Domain;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;
using System.Configuration;

namespace Kuyam.WebUI.Controllers
{
    [Authorize(Roles = "personal, admin, support")]
    public class AppointmentPreviewController : KuyamBaseController
    {
        [HttpPost]
        public ContentResult SetLockedTimeSlot(string key, int minute)
        {
            Session["LockedTimeSlotId"] = key;            
            System.Web.HttpContext.Current.Application[key] = DateTime.Now.AddMinutes(minute);
            return Content("true");
        }
        [HttpPost]
        public ContentResult RemoveLockedTimeSlot(string key)
        {
            System.Web.HttpContext.Current.Application.Remove(key);
            return Content("true");

        }
        private DateTime _expectedStartDate = DateTime.Now;
        private DateTime _expectedEndDate = DateTime.Now;

        private Appointment _appointment = null;
        private const string APPOINTMENTOBJECT = "APPOINTMENTOBJECT";


        //
        // GET: /AppointmentPreview/Index?genre=Disco
        public ActionResult Index(string cipherText)
        {
            Session["APPOINTMENTOBJECT"] = new Appointment();
            string encryptText = Request.QueryString["genre"].Replace(" ", "+");
            if (!string.IsNullOrEmpty(encryptText))
            {
                try
                {
                    //=============
                    //string Cancel = Request.QueryString["Cancel"];
                    //============

                    string decryptText = Kuyam.Utility.SecurityHelper.DecryptStringFromBytesAes(encryptText);
                    string[] elements = decryptText.Split('@');                    
                    DateTime expectedDate = DateTime.Parse(elements[0]);
                    int serviceId = Int32.Parse(elements[1]);
                    int employeeId = Int32.Parse(elements[2]);
                    int custId = Int32.Parse(elements[3]);
                    ViewBag.UserName = MySession.Cust.FirstName;
                    var service = ProfileCompany.GetServiceByID(serviceId);
                    ViewBag.ProfileCompany = service.ProfileCompany;
                    ViewBag.CompanyId = service.ProfileCompany.ProfileID;
                    ViewBag.Company = service.ProfileCompany.Name;
                    ViewBag.Serviceid = serviceId.ToString(CultureInfo.InvariantCulture);
                    ViewBag.Servicename = service.Service.ServiceName;
                    ViewBag.Detail = string.Format("{0}min, ${1}, {2} person", service.Duration, service.Price, service.AttendeesNumber);
                    ViewBag.Price = service.Price;
                    ViewBag.ServicenamePopup = string.Format("{0} {1}min", service.Service.ServiceName, service.Duration);
                    ViewBag.DetailPopup = string.Format("${0}, {1} person",service.Price, service.AttendeesNumber);
                    _expectedStartDate = expectedDate;
                    _expectedEndDate = expectedDate.AddMinutes((dynamic)service.Duration ?? 0);

                    if (employeeId > 0)
                    {
                        var employee = ProfileCompany.GetCompanyEmployee(employeeId);
                        ViewBag.Employee = employee.EmployeeName;
                    }
                    else
                    {
                        ViewBag.Employee = "N/A";
                    }

                    ViewBag.StartTime = expectedDate;
                    ViewBag.StartHour = expectedDate;
                    ViewBag.EndHour = expectedDate.AddMinutes(service.Duration.Value);
                    ViewBag.IsGoogleConnected = true;
                    ViewBag.IsFBConnected = true;

                    _appointment = new Appointment()
                                       {
                                           ServiceCompanyID = service.ServiceCompanyID,
                                           AppointmentStatusID = (int)Types.AppointmentStatus.Pending,
                                           StatusChangeDate = DateTime.Now,
                                           Title = _expectedStartDate.ToString() + " with " + service.Service.ServiceName,
                                           ContactPerson = string.Empty,
                                           AllDay = false,
                                           Start = _expectedStartDate,
                                           End = _expectedEndDate,
                                           Url = string.Empty,
                                           PersonCount = 1,
                                           Notes = _expectedStartDate.ToString() + " with " + service.Service.ServiceName,
                                           Created = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now),
                                           Modified = DateTimeUltility.ConvertToUtcMinus7(DateTime.Now),
                                           CustID = custId,
                                           EmployeeID = employeeId == 0 ? (int?)null : employeeId
                                       };
                    Session["APPOINTMENTOBJECT"] = _appointment;
                    var serviceHours = new List<EventCustom>();
                    ViewBag.Services = GetServiceHourByListServices(serviceHours, "", 0, 0);
                    string key = employeeId + "_" + expectedDate.ToString("yyyyMMddhhsstt");
                    ViewBag.LockKey = key;
                    int cancellationPolicy = service.ProfileCompany.PayAfter != null ? service.ProfileCompany.PayAfter.Value : 0;
                    ViewBag.Policy = DateTime.Now > expectedDate.AddHours(-cancellationPolicy) ? cancellationPolicy : 0;
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "CompanyProfile", new { id = 5140 });
                    //InitAppointment();
                }
            }
            else
            {
                return RedirectToAction("Index", "CompanyProfile", new { id = 5140 });
                //InitAppointment();
            }
            return View();
        }
     
        private void InitAppointment()
        {
            DateTime dt = DateTime.Now;
            /*
            Session["companyid"] = "589";
            Session["serviceid"] = "1";
            Session["servicename"] = "shiatsu massage";
            Session["duration"] = "60";
            Session["price"] = "$40";
            Session["employee"] = "any employee";
            Session["startdate"] = new DateTime(dt.Year, dt.Month, dt.Day, 16, 00, 00);
            Session["starttime"] = "10:00am";
             * */
            Session["companyid"] = "N/A";
            Session["serviceid"] = "N/A";
            Session["servicename"] = "N/Ae";
            Session["duration"] = "N/A";
            Session["price"] = "N/A";
            Session["employee"] = "N/A";
            Session["startdate"] = new DateTime(dt.Year, dt.Month, dt.Day, 16, 00, 00);
            Session["starttime"] = "N/A";
        }

        private string GetServiceHourByListServices(List<EventCustom> serviceHour, string duration, int employeeId, int servicesId)
        {
            string html = string.Empty;
            Cust user = MySession.Cust;
            DateTime dtNow = DateTime.Now;
            DateTime beginDay = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

            var service = new InfoConnSoapClient();
            var searchOption = new SearchOption()
            {
                UId = user.CustID,
                StartDate = beginDay.AddDays(-1),
                EndDate = beginDay.AddDays(7),
                ConnectorSourceType = ConnectorSourceType.All
            };


            var events = service.GetBusyInfo(searchOption).ToList();

            var kuyamEvent = ProfileCompany.GetKuyamEvents(user.CustID);

            events.AddRange(kuyamEvent.Select(u => new BusyInfo()
            {
                SourceType = ConnectorSourceType.All,
                StartDate = u.StartDate,
                EndDate = u.EndDate
            }).ToList());

            for (var index = 0; index < 7; index++)
            {
                string str = string.Empty;
                DateTime currentDate = beginDay.Date.AddDays(index);               
                if (index == 4)
                {
                    html += "<div class=\"apcalendarcol sat\">";
                }
                else
                {
                    html += "<div class=\"apcalendarcol\">";
                }
                html += String.Format("<div class=\"apcalendaritem aligncenter f14\">{0:ddd dd}</div>", currentDate);

                for (int i = 7; i < 23; i++)
                {
                    str = string.Empty;                    
                    DateTime expectedDate = currentDate.AddHours(i);                   
                    DateTime beginDate = beginDay.AddDays(index).AddHours(i);
                    DateTime endDate = beginDay.AddDays(index).AddHours(i + 1);

                    BusyInfo busyInfo = events.FirstOrDefault(e => ((e.StartDate <= beginDate && e.EndDate > beginDate)
                                                                   || (e.StartDate < endDate && e.EndDate >= endDate)
                                                                   || (e.StartDate >= beginDate && e.EndDate <= endDate)));
                    if ((beginDate <= _expectedStartDate && endDate > _expectedStartDate)
                                                                 || (beginDate < _expectedEndDate && endDate >= _expectedEndDate)
                                                                 || (beginDate >= _expectedStartDate && endDate <= _expectedEndDate))
                    {
                        str = "previewcal";
                    }
                    if (busyInfo != null)
                    {
                        if (busyInfo.SourceType == ConnectorSourceType.Google)
                            str = "googlecal";
                        else if (busyInfo.SourceType == ConnectorSourceType.Facebook)
                        {
                            str = "fbcal";
                        }
                        else if (busyInfo.SourceType == ConnectorSourceType.iCalendar)
                        {
                            str = "ical";
                        }
                        else {
                            str = "kuyamcal";
                        }
                    }
                    if ((beginDate <= _expectedStartDate && endDate > _expectedStartDate)
                                                                   || (beginDate < _expectedEndDate && endDate >= _expectedEndDate)
                                                                   || (beginDate >= _expectedStartDate && endDate <= _expectedEndDate))
                    {
                        str = "previewcal";
                    }
                    html = string.Format("{0}{1}", html, CalendarCel(expectedDate, serviceHour, str, duration, index));
                }

                html += "</div>";
            }

            ViewData["Html"] = html;            
            return html;
        }

        public string CalendarCel(DateTime expectedDate, List<EventCustom> hours, string active, string duration, int index)
        {
            /*
            <div class="apcalendaritem bgedf3b7">
                <div class="googlecal"></div>
            </div>
             * */
            string html = string.Empty;
            if (index == 0)
            {
                if (active == string.Empty)
                    html = "<div class='apcalendaritem bgedf3b7'></div>";
                else
                {
                    if (active == "previewcal")
                    {
                        string val = _expectedStartDate.ToString("hht\\M").ToLower();                        
                        if (ViewBag.Company != null)
                        {
                            val = val + " " + ViewBag.Company;
                            if (val.Length > 15)
                            {
                                val = val.Substring(0, 14);
                            }
                        }
                        html = string.Format("<div class='apcalendaritem'><div class='{0}'>{1}</div></div>", active,
                                             val);
                    }
                    else
                    {
                        html = string.Format("<div class='apcalendaritem bgedf3b7'><div class='{0}'></div></div>",
                                             active);
                    }
                    
                }                
            }
            else
            {
                if (active == string.Empty)
                    html = "<div class='apcalendaritem'></div>";
                else
                {
                    if (active == "previewcal")
                    {
                        string val = _expectedStartDate.ToString("hht\\M").ToLower();
                        if (ViewBag.Company != null)
                        {
                            val = val + " " + ViewBag.Company;
                            if (val.Length > 15)
                            {
                                val = val.Substring(0, 14);
                            }
                        }
                        html = string.Format("<div class='apcalendaritem'><div class='{0}'>{1}</div></div>", active,
                                             val);
                    }
                    else
                        html = string.Format("<div class='apcalendaritem'><div class='{0}'></div></div>", active);
                }                

            }
            
            return html;

        }

        [HttpPost]
        public ActionResult BookAppointment(string sms, string email, string mess, string price)
        {
            //string sms = Request.Params["sms"];
            //string email = Request.Params["email"];
            //string mess = Request.Params["mess"]; 
            //string priceString =Request.Params["price"];
            decimal dbprice = 0;
            Decimal.TryParse(price, out dbprice);
            Types.ContactType contactType = Types.ContactType.None;
            if (sms == "checked" && email == "checked")
            {
                contactType = Types.ContactType.EmailSMS;
            }
            else if (sms == "checked")
            {
                contactType = Types.ContactType.SMS;
            }
            else if (email == "checked")
            {
                contactType = Types.ContactType.Email;
            }
            if (Session["APPOINTMENTOBJECT"] != null)
            {
                Appointment appointment = Session["APPOINTMENTOBJECT"] as Appointment;
                if (appointment != null)
                {
                    appointment.Notes = mess;
                    appointment.ContactType = (int)contactType;
                    appointment.Price = dbprice;                    
                }
            }
            return Json("result", JsonRequestBehavior.AllowGet);           

        }

        [HttpPost]
        public ActionResult BookAppointmentCash(string sms, string email, string mess, string price)
        {           
            decimal dbprice = 0;
            Decimal.TryParse(price, out dbprice);
            Types.ContactType contactType = Types.ContactType.None;
            if (sms == "checked" && email == "checked")
            {
                contactType = Types.ContactType.EmailSMS;
            }
            else if (sms == "checked")
            {
                contactType = Types.ContactType.SMS;
            }
            else if (email == "checked")
            {
                contactType = Types.ContactType.Email;
            }
            if (Session["APPOINTMENTOBJECT"] != null)
            {
                Appointment appointment = Session["APPOINTMENTOBJECT"] as Appointment;
                if (appointment != null)
                {
                    appointment.Notes = mess;
                    appointment.ContactType = (int)contactType;
                    appointment.Price = dbprice;
                    ProfileCompany.AddAppointment(appointment, MySession.CalendarId);
                    Session["APPOINTMENTOBJECT"] = null;
                }

            }
            return Json("true", JsonRequestBehavior.AllowGet);

        }     

    }
}
