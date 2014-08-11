using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain.GiftCardServices;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.Helpers;
using Kuyam.Domain;
using Kuyam.Database;
using PayPal.AdaptivePayments.Model;
using System.Configuration;
using PayPal.AdaptivePayments;
using Kuyam.Utility;
using Kuyam.WebUI.InfoConnServiceReference;
using System.Threading;
using System.Globalization;
using System.Text;
using Kuyam.Domain.Seo;
using Kuyam.Domain.CompanyProfileServices;
using System.Data.Entity;
using Kuyam.Domain.ClassModel;
using System.Data.SqlClient;
using Kuyam.Database.Extensions;
using Kuyam.WebUI.Models.BookKing;
using Kuyam.Domain.MessageServcies;

namespace Kuyam.WebUI.Controllers
{
    [Authorize]
    public class PayPalController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly OrderService _orderService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly NotificationService _notificationService;
        private readonly IGiftCardServices _giftCardServices;
        private readonly AdminService _adminService;
        private readonly ISMSProvider _smsProvider;
        public PayPalController(DbContext dbContext,
            IAppointmentService appointmentService,
            CompanyProfileService companyProfileService,
            OrderService orderService,
            NotificationService notificationService,
            AdminService adminService,
            IGiftCardServices giftCardServices,
            ISMSProvider smsProvider)
        {
            this._appointmentService = appointmentService;
            this._orderService = orderService;
            this._companyProfileService = companyProfileService;
            this._notificationService = notificationService;
            this._giftCardServices = giftCardServices;
            this._adminService = adminService;
            this._smsProvider = smsProvider;
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

        private void ZendeskAdd(Appointment appointment, ProfileCompany company, string title = "")
        {
            try
            {
                if (appointment != null && company != null)
                {
                    //Create Zendesk ticket here -- Khoi Tran
                    //Information for the ticket   
                    List<AppointmentLog> notes = appointment.AppointmentLogs.OrderBy(n => n.LogDT).ToList();
                    AppointmentParticipant appart =
                        EngineContext.Current.Resolve<IAppointmentService>().GetAppointmentParticipantByAppointmentId(appointment.AppointmentID);
                    string methodCommunication;
                    string subject = "";
                    int? preferedContact = null;
                    if (company.PreferredContact.HasValue)
                        preferedContact = company.PreferredContact.Value;
                    if (preferedContact != null && ((preferedContact.Value == 1)))
                    {
                        methodCommunication = "Email";
                    }
                    else if (preferedContact != null && ((preferedContact.Value == 2)))
                    {
                        methodCommunication = "SMS";
                    }

                    else if (preferedContact != null && ((preferedContact.Value == 3)))
                    {
                        methodCommunication = "SMS&Email";
                    }
                    else
                    {
                        methodCommunication = "not specified";
                    }

                    subject = "Appointment " + appointment.AppointmentID + title;//" has beeen confirmed";
                    if (ConfigurationManager.AppSettings["KuyamVersion"] != null &&
                        int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                        subject = "[QA] " + subject;
                    else if (ConfigurationManager.AppSettings["KuyamVersion"] != null &&
                             int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                        subject = "[DEV] " + subject;

                    var status = (TicketStatus.Pending);
                    var type = (TicketType.Incident);
                    var priority = (TicketPriority.High);
                    string description = "appointment information: \n"
                                         + "company name: " + company.Name.ToLower() + "\n"
                                         + "company contact info: " + "\n";
                    if (company.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                    {
                        if (methodCommunication == "Email")
                        {
                            description += "   email:	" + company.Email.ToLower() + "\n";
                        }
                        else if (methodCommunication == "SMS")
                        {
                            description += "   sms: " + Kuyam.Domain.UtilityHelper.FormatPhone(company.Phone) + "\n";
                        }
                        else if (methodCommunication == "SMS&Email")
                        {
                            description += "   email:	" + company.Email.ToLower() + "\n"
                                           + "   sms: " + UtilityHelper.FormatPhone(company.Phone) + "\n";
                        }
                    }
                    else if (company.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt ||
                             company.CompanyTypeID == (int)Types.CompanyType.NonKuyamBookIt)
                    {
                        description += "   phone: " + UtilityHelper.FormatPhone(company.Phone) + "\n";
                    }

                    //basic info
                    var employeeName = appointment.CompanyEmployee != null
                        ? appointment.CompanyEmployee.EmployeeName
                        : appointment.EmployeeName;
                    var serviceName = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Service.ServiceName
                        : appointment.ServiceName;
                    var duration = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Duration
                        : appointment.Duration;
                    var price = appointment.ServiceCompany != null
                        ? appointment.ServiceCompany.Price
                        : appointment.Price;

                    description += "primary contact: " +
                                   (company.ContactFirstName != null ? company.ContactFirstName.ToLower() : string.Empty) +
                                   " "
                                   + (company.ContactLastName != null ? company.ContactLastName.ToLower() : string.Empty) +
                                   "\n"
                                   + "employee name: " + employeeName.ToLower() + "\n"
                                   + "calendar: " + appart.Calendar.Name.ToLower() + "\n"
                                   + "service: " + serviceName.ToLower() + "\n"
                                   + "duration: " + duration + "\n"
                                   + "price: " + string.Format("${0:0.00}", price) + "\n"
                                   + "date: " + string.Format("{0:MMM dd yyyy}", appointment.Start).ToLower() + "\n"
                                   + "time: " + appointment.Start.ToString("h:mm tt").ToLower() + " - " +
                                   appointment.End.ToString("h:mm tt").ToLower() + "\n"
                                   + "appointment status: " + (Types.AppointmentStatus.Confirmed).ToString().ToLower() +
                                   "\n"
                                   + "appointment notes: " + "\n";
                    description += "\t" + appointment.Notes + "\n";

                    if (notes.Count > 0)
                    {
                        foreach (var note in notes)
                        {
                            description += "\t" + note.Message.Replace("<br/>", "\n").ToLower() + "\n";
                        }
                    }

                    int group_id = Int32.Parse(ConfigurationManager.AppSettings["groupid"]);

                    ZenAPI.CreateTicket(subject, status, type, priority, description);
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        private void SendReminders(Appointment appointment)
        {
            Cust userInfo = MySession.Cust;

            if (MySession.ImpersonateId > 0 && MySession.OriginalCustIfImpersonated != null)
            {
                //TODO:
            }
            else if (userInfo != null && userInfo.FirstAlert > 0 || userInfo.SecondAlert > 0)
            {
                string emailBcc = string.Empty;
                if (MyApp.Settings.Admin.EnableEmailBcc)
                    emailBcc = MyApp.Settings.Admin.EmailBcc;

                int firstalertTime = GetMinutesByType(userInfo.FirstAlert.Value);
                int secondalertTime = GetMinutesByType(userInfo.SecondAlert.Value);
                var company = appointment.GetProfileCompany();
                var employeeName = appointment.EmployeeName ?? (appointment.CompanyEmployee != null ? appointment.CompanyEmployee.EmployeeName : string.Empty);
                var serviceName = appointment.ServiceName ?? appointment.ServiceCompany.Service.ServiceName;
                var duration = appointment.Duration ?? appointment.ServiceCompany.Price;
                var price = appointment.Price ?? appointment.ServiceCompany.Price;
                int cancelpolicy = company.CancelPolicy.HasValue ? company.CancelPolicy.Value : 0;
                string stringPolicy = string.Empty;
                int hours = 0;
                if (cancelpolicy > 0)
                {
                    if (cancelpolicy == (int)Types.CancellationType.Standard)
                    {
                        stringPolicy = "standard";
                        hours = 24;
                    }
                    else if (cancelpolicy == (int)Types.CancellationType.Strict)
                    {
                        stringPolicy = "strict";
                        hours = 72;
                    }
                    else if (cancelpolicy == (int)Types.CancellationType.Custom)
                    {
                        stringPolicy = "custom";
                        hours = company.CancelHour.Value;
                    }
                }
                string message = string.Format("Kuyam scheduled reminder for your appointment for {0} {1}  on {2} at {3} with {4}, {5}",
                                MySession.Cust.FirstName, MySession.Cust.LastName, appointment.Start.ToString("ddd, MMM d"), appointment.Start.ToString("h:mm tt"),
                                employeeName, company.Name);
                string templateAppointmentReminder;

                if (firstalertTime > 0 && (userInfo.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Email) != 0)
                {
                    dynamic firstalertObject = new
                    {
                        UserName = MySession.Cust.FirstName.ToString(),
                        DateNow = String.Format("{0:dddd, MMMM d, yyyy}", appointment.Start.AddMinutes(-firstalertTime)),
                        CancelPolicy = stringPolicy,
                        CancleHours = hours,
                        Date = appointment.Start.ToString("ddd, MMM d"),
                        Time = appointment.Start.ToString("h:mm tt"),
                        Service = string.Format("{0}, {1}min, ${2}", serviceName,
                        duration, price),
                        EmployeeName = employeeName,
                        CompanyName = company.Name,
                        CompanyAddress = string.Format("{0}", company.Street1),
                        CompanyCity = string.Format("{0},{1} {2}", company.City,
                        company.State, company.Zip),
                        CompanyPhone = UtilityHelper.FormatPhone(company.Phone)
                    }.ToExpando();

                    templateAppointmentReminder = this.RenderPartialViewToString("AppointmentReminder", (object)firstalertObject);
                    IncomingRequest obj = new IncomingRequest
                    {
                        EntityId = "KuyamWeb",
                        EntityType = (int)Types.EntityAlertType.FirstAlert,
                        PreviousAlert = firstalertTime,
                        CustId = MySession.CustID,
                        AppointmentId = appointment.AppointmentID,
                        ApptStartDate = appointment.Start,
                        Data = Kuyam.Domain.UtilityHelper.ObjectToXml(new { Emailtemplate = templateAppointmentReminder, Emailto = MySession.Cust.Username, EmailBcc = emailBcc, Subject = "Appointment Reminders" })
                    };

                    Thread oThread = new Thread(() =>
                    {
                        try
                        {
                            InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                            serviceInfo.AddIncomingRequest(obj, IncommingRequestType.SEND_EMAIL);
                        }
                        catch (Exception ex)
                        {
                            //Todo: Handle Exception Occur
                            LogHelper.Error("BookAppointmentCash fail:", ex);
                        }

                    });
                    oThread.Start();
                }
                if (!string.IsNullOrEmpty(userInfo.MobilePhone) && firstalertTime > 0 && (userInfo.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Text) != 0)
                {
                    string strphoneNumber = UtilityHelper.CleanPhone(userInfo.MobilePhone);

                    if (MyApp.Settings.Admin != null && MyApp.Settings.Admin.EnablePhoneBcc)
                    {
                        strphoneNumber += ";";
                        strphoneNumber += MyApp.Settings.Admin.PhoneNumber;
                    }

                    var objPhone = new IncomingRequest
                    {
                        EntityId = "KuyamWeb",
                        EntityType = (int)Types.EntityAlertType.FirstAlert,
                        PreviousAlert = firstalertTime,
                        CustId = MySession.CustID,
                        AppointmentId = appointment.AppointmentID,
                        ApptStartDate = appointment.Start,
                        Data = UtilityHelper.ObjectToXml(new { Message = message, PhoneNumber = strphoneNumber }),

                    };


                    Thread phoneThread = new Thread(() =>
                    {
                        try
                        {
                            InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                            serviceInfo.AddIncomingRequest(objPhone, IncommingRequestType.SEND_SMS);
                            LogHelper.SMS(message + " was sent to sms number " + strphoneNumber);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.SMS("Send SMS fail:", ex);
                        }

                    });

                    phoneThread.Start();

                }


                if (secondalertTime > 0 && (userInfo.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Email) != 0)
                {
                    dynamic secondalertObject = new
                    {
                        UserName = MySession.Cust.FirstName,
                        DateNow = String.Format("{0:dddd, MMMM d, yyyy}", appointment.Start.AddMinutes(-secondalertTime)),
                        CancelPolicy = stringPolicy,
                        CancleHours = hours,
                        Date = appointment.Start.ToString("ddd, MMM d"),
                        Time = appointment.Start.ToString("h:mm tt"),
                        Service = string.Format("{0}, {1}min, ${2}", serviceName,
                        duration, price),
                        EmployeeName = employeeName,
                        CompanyName = company.Name,
                        CompanyAddress = string.Format("{0}", company.Street1),
                        CompanyCity = string.Format("{0},{1} {2}", company.City,
                        company.State, company.Zip),
                        CompanyPhone = UtilityHelper.FormatPhone(company.Phone)
                    }.ToExpando();

                    templateAppointmentReminder = this.RenderPartialViewToString("AppointmentReminder", (object)secondalertObject);
                    IncomingRequest obj = new IncomingRequest
                    {
                        EntityId = "KuyamWeb",
                        EntityType = (int)Types.EntityAlertType.SecondAlert,
                        PreviousAlert = secondalertTime,
                        CustId = MySession.CustID,
                        AppointmentId = appointment.AppointmentID,
                        ApptStartDate = appointment.Start,
                        Data = Kuyam.Domain.UtilityHelper.ObjectToXml(new { Emailtemplate = templateAppointmentReminder, Emailto = MySession.Cust.Username, EmailBcc = emailBcc, Subject = "Appointment Reminders" })
                    };

                    var oThread = new Thread(() =>
                    {
                        try
                        {
                            InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                            serviceInfo.AddIncomingRequest(obj, IncommingRequestType.SEND_EMAIL);
                        }
                        catch (Exception ex)
                        {
                            //Todo: Handle Exception Occur
                            LogHelper.Error("Send email fail:", ex);
                        }

                    });
                    oThread.Start();
                }

                if (!string.IsNullOrEmpty(userInfo.MobilePhone) && secondalertTime > 0 && (userInfo.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Text) != 0)
                {
                    string strphoneNumber = UtilityHelper.CleanPhone(userInfo.MobilePhone);

                    if (MyApp.Settings.Admin != null && MyApp.Settings.Admin.EnablePhoneBcc)
                    {
                        strphoneNumber += ";";
                        strphoneNumber += MyApp.Settings.Admin.PhoneNumber;
                    }

                    var objPhone = new IncomingRequest
                    {
                        EntityId = "KuyamWeb",
                        EntityType = (int)Types.EntityAlertType.SecondAlert,
                        PreviousAlert = secondalertTime,
                        CustId = MySession.CustID,
                        AppointmentId = appointment.AppointmentID,
                        ApptStartDate = appointment.Start,
                        Data = UtilityHelper.ObjectToXml(new { Message = message, PhoneNumber = strphoneNumber }),

                    };

                    Thread phoneThread = new Thread(() =>
                    {
                        try
                        {
                            InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                            serviceInfo.AddIncomingRequest(objPhone, IncommingRequestType.SEND_SMS);
                            LogHelper.SMS(message + " was sent to sms number " + strphoneNumber);
                        }
                        catch (Exception ex)
                        {
                            //Todo: Handle Exception Occur
                            LogHelper.SMS("Send SMS fail:", ex);
                        }

                    }
                    );
                    phoneThread.Start();

                }
            }
        }

        private string GetEmailTemplateAppointment(int appointmentID, string templateName)
        {
            string templateResult = string.Empty;          

            Appointment appointment = _appointmentService.GetAppointmentByID(appointmentID);

            string userName = string.Empty;
            string email = string.Empty;
            string employeeName = string.Empty;
            string companyName = string.Empty;
            string serviceName = string.Empty;
            string serviceDetail = string.Empty;
            string appointmentTime = string.Empty;
            string appointmentDate = string.Empty;
            string payAfter = string.Empty;
            string companyID = string.Empty;
          
            if (appointment.Cust != null)
            {
                userName = appointment.Cust.FirstName;
            }

            if (appointment != null)
            {
                appointmentTime = appointment.Start.ToString("h:mm tt");
                appointmentDate = appointment.Start.ToString("ddd, MMM dd");
            }

            serviceDetail = string.Format("{0}min, ${1}, {2} person", appointment.Duration, appointment.Price, appointment.AttendeesNumber);
            serviceName = appointment.ServiceName;

            companyName = appointment.ProfileCompany.Name;
            email = appointment.ProfileCompany.Email;
            companyID = appointment.ProfileId.ToString();

            if (appointment.ProfileCompany.CancelPolicy == (int)Types.CompanyPolicies.Strict)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to <strong>strict.</strong><br />you cancelled <strong>less than 72 hours</strong> before the appointment time so you will be charged <strong>50% of the Total Fees.</strong>");
            }
            else if (appointment.ProfileCompany.CancelPolicy == (int)Types.CompanyPolicies.Standard)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to <strong>standard</strong>.<br />since you cancelled <strong>more than 24 hours</strong> before the appointment time,you will not be charged.");
            }
            else if (appointment.ProfileCompany.CancelPolicy == (int)Types.CancellationType.None)
            {
                payAfter = string.Format("{0}", "this company’s cancellation policy is set to none so you'll not be charged");
            }
            else
            {
                payAfter = string.Format("this company’s cancellation policy is set to <strong>custom.</strong><br />you cancelled <strong>less than {0} hours</strong> before the appointment time so you will be charged <strong>{1}% of the Total Fees.</strong>", appointment.ProfileCompany.CancelHour, 100 - (int)appointment.ProfileCompany.CancelRefundPercent);
            }

            employeeName = appointment.EmployeeName;

            // create template data
            dynamic myObject = new
            {
                Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                Host = Kuyam.WebUI.Helpers.EmailHelper.GetStoreHost(),
                UserName = appointment.Cust.FirstName.ToString(),
                Email = email,
                EmployeeName = employeeName,
                CompanyName = companyName,
                ServiceName = serviceName,
                ServiceDetail = serviceDetail,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime.ToLower(),
                PayAfterText = payAfter,
                CompanyID = companyID               
            }.ToExpando();

            // generate the content using razor engine
            templateResult = this.RenderPartialViewToString(templateName, (object)myObject);
            return templateResult;
        }

        private void ConfirmNotification(Appointment appointment, string emailstring)
        {
            string emailTo = string.Empty;
            string phoneNumber = string.Empty;
            Cust aUser = DAL.GetCustByCustId(appointment.CustID);

            if (aUser != null)
            {
                emailTo = aUser.Email;
                phoneNumber = aUser.MobilePhone;
            }

            if (appointment.ContactType == (int)Types.ContactType.Email)
            {
                EmailHelper.SendEmailConfirmAppointment(string.Empty, emailTo, emailstring);
            }
            else if (appointment.ContactType == (int)Types.ContactType.SMS)
            {
                _smsProvider.SendSms(new string[] { phoneNumber }, "confirm appointment", "your appointment has been confirmed.", false);
            }
            else if (appointment.ContactType == (int)Types.ContactType.EmailSMS)
            {
                _smsProvider.SendSms(new string[] { phoneNumber }, "confirm appointment", "your appointment has been confirmed.", false);
                EmailHelper.SendEmailConfirmAppointment(string.Empty, emailTo, emailstring);
            }
        }

        public ActionResult GetDataCheckoutByServiceId(int serviceId, int employeeId, int calendarId, string startDate, int? packageId)
        {
            DateTime dt = DateTime.ParseExact(startDate, "yy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);
            var infoCheckout = _appointmentService.GetDataCheckOutForServiceIdAndEmployeeId(serviceId, employeeId);
            var calendar = _companyProfileService.GetCalendarByCalendarId(calendarId);
            var cseItem = _adminService.GetAvalableCompanyServiceEventFromCompanyServiceId(infoCheckout.ServiceCompanyID, dt);

            string encryptPackageId = string.Empty;
            bool isPackage = false;
            if (packageId.HasValue)
            {
                encryptPackageId = SecurityHelper.EncryptStringToBytesAes(packageId.Value.ToString());
                var userPackage = _companyProfileService.GetUserPackagePurchasePackageID(packageId.HasValue ? packageId.Value : 0);
                isPackage = (userPackage != null && (userPackage.MaxUses > 0 || userPackage.MaxUses == -1));
            }

            Appointment appointment = new Appointment()
            {
                AppointmentStatusID = (int)Types.AppointmentStatus.Pending,
                StatusChangeDate = DateTime.UtcNow,
                Title = infoCheckout.ServiceName,
                ContactPerson = string.Empty,
                AllDay = false,
                ProfileId = infoCheckout.ProfileID,
                ServiceCompanyID = infoCheckout.ServiceCompanyID,
                ServiceName = infoCheckout.ServiceName,
                EmployeeName = infoCheckout.EmployeeName,
                Price = cseItem != null ? cseItem.NewPrice.Value : infoCheckout.Price,
                Duration = infoCheckout.Duration,
                AttendeesNumber = infoCheckout.AttendeesNumber,
                Start = dt,
                End = dt.AddMinutes(infoCheckout.Duration),
                PersonCount = 1,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CustID = MySession.CustID,
                CalendarId = calendarId,
                EmployeeID = employeeId == 0 ? (int?)null : employeeId
            };

            if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                && MySession.Concierge != null && MySession.HotelId > 0)
            {
                appointment.HotelID = MySession.HotelId;
                appointment.StaffID = MySession.Concierge.Id;
            }

            MySession.Appointmentbooking = appointment;

            var appointmentTemp = appointment.ToAppointmentTemp();
            _appointmentService.InsertAppointmentTemp(appointmentTemp);
            MySession.AppointmentbookingTempId = appointmentTemp.AppointmentID;

            DateTime dtend = dt.AddMinutes(infoCheckout.Duration);

            var model = new CheckoutModel()
            {
                CheckOutSummary = "appointment summary",
                ProfileId = infoCheckout.ProfileID,
                CheckoutType = (int)Types.CheckoutType.Availability,
                PaymentMethod = infoCheckout.PaymentMethod ?? 0,
                IsPackage = isPackage,
                PackageId = encryptPackageId,
                CompanyName = infoCheckout.Name,
                Address = infoCheckout.Street1 + " " + infoCheckout.Street2,
                City = !string.IsNullOrWhiteSpace(infoCheckout.City) ? string.Format("{0}, {1} {2}", infoCheckout.City, infoCheckout.State, infoCheckout.Zip) : string.Format("{1} {2}", infoCheckout.City, infoCheckout.State, infoCheckout.Zip),
                EmployeeName = Kuyam.Domain.UtilityHelper.TruncateText(infoCheckout.EmployeeName, 12),
                CalendarName = Kuyam.Domain.UtilityHelper.TruncateText(calendar.Name, 12),
                ServiceName = UtilityHelper.TruncateText(infoCheckout.ServiceName, 12),
                Price = cseItem != null ? cseItem.NewPrice.Value : infoCheckout.Price,
                Totaldue = cseItem != null ? cseItem.NewPrice.Value : infoCheckout.Price,
                Duration = infoCheckout.Duration,
                Datetime = String.Format("{0:ddd, MMM d}", dt),
                Time = String.Format("{0:t}", dt).Replace(" ", string.Empty).ToLower() + " - " + String.Format("{0:t}", dtend).Replace(" ", string.Empty).ToLower()
            };

            if (model.IsPackage)
            {
                model.Totaldue = 0;
            }
            return Json(new { content = this.RenderPartialViewToString("_CheckoutPopup", model) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataCheckoutForClass(int classSchedulerId, int calendarId, string startDate, int? packageId)
        {
            var classInfor = _appointmentService.GetDataCheckOutOfClass(classSchedulerId);

            if (classInfor == null)
                return Json(new { content = string.Empty }, JsonRequestBehavior.AllowGet);

            DateTime dt = DateTime.ParseExact(startDate, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);
            var cseItem = _adminService.GetAvalableCompanyServiceEventFromCompanyServiceId(classInfor.ServiceCompanyID, dt);
            var calendar = _companyProfileService.GetCalendarByCalendarId(calendarId);
            string encryptPackageId = string.Empty;
            bool isPackage = false;
            if (packageId.HasValue)
            {
                encryptPackageId = SecurityHelper.EncryptStringToBytesAes(packageId.Value.ToString());
                var userPackage = _companyProfileService.GetUserPackagePurchasePackageID(packageId.HasValue ? packageId.Value : 0);
                isPackage = (userPackage != null && (userPackage.MaxUses > 0 || userPackage.MaxUses == -1));
            }

            Appointment appointment = new Appointment()
            {
                AppointmentStatusID = (int)Types.AppointmentStatus.Pending,
                BookingType = (int)Types.BookingType.ClassBooking,
                ClassSchedulerID = classSchedulerId,
                StatusChangeDate = DateTime.UtcNow,
                AllDay = false,
                Start = dt,
                End = dt.AddMinutes(classInfor.Duration),
                Notes = string.Empty,
                PersonCount = 1,
                ProfileId = classInfor.ProfileID,
                CustID = MySession.CustID,
                CalendarId = calendarId,
                ServiceCompanyID = classInfor.ServiceCompanyID,
                EmployeeID = classInfor.EmployeeID,
                ServiceName = classInfor.ServiceName,
                EmployeeName = classInfor.EmployeeName,
                Price = cseItem != null ? cseItem.NewPrice.Value : classInfor.Price,                
                Duration = classInfor.Duration,
                AttendeesNumber = classInfor.AttendeesNumber,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
            };

            if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                && MySession.Concierge != null && MySession.HotelId > 0)
            {
                appointment.HotelID = MySession.HotelId;
                appointment.StaffID = MySession.Concierge.Id;
            }

            MySession.Appointmentbooking = appointment;
            var appointmentTemp = appointment.ToAppointmentTemp();
            _appointmentService.InsertAppointmentTemp(appointmentTemp);
            MySession.AppointmentbookingTempId = appointmentTemp.AppointmentID;

            DateTime dtend = dt.AddMinutes(classInfor.Duration);

            var model = new CheckoutModel()
            {
                CheckOutSummary = "class summary",
                ProfileId = classInfor.ProfileID,
                CheckoutType = (int)Types.CheckoutType.ClassBooking,
                PaymentMethod = classInfor.PaymentMethod ?? 0,
                IsPackage = isPackage,
                PackageId = encryptPackageId,
                CompanyName = classInfor.Name,
                Address = classInfor.Street1 + " " + classInfor.Street2,
                City = !string.IsNullOrWhiteSpace(classInfor.City) ? string.Format("{0}, {1} {2}", classInfor.City, classInfor.State, classInfor.Zip) : string.Format("{1} {2}", classInfor.City, classInfor.State, classInfor.Zip),
                EmployeeName = UtilityHelper.TruncateText(classInfor.EmployeeName, 12),
                CalendarName = UtilityHelper.TruncateText(calendar.Name, 12),
                ServiceName = UtilityHelper.TruncateText(classInfor.ServiceName, 12),
                Price = cseItem != null ? cseItem.NewPrice.Value : classInfor.Price,
                Totaldue = cseItem != null ? cseItem.NewPrice.Value : classInfor.Price,
                Duration = classInfor.Duration,
                Datetime = String.Format("{0:ddd, MMM d}", dt),
                Time = String.Format("{0:t}", dt).Replace(" ", string.Empty).ToLower() + " - " + String.Format("{0:t}", dtend).Replace(" ", string.Empty).ToLower()
            };

            if (model.IsPackage)
            {
                model.Totaldue = 0;
            }
            return Json(new { content = this.RenderPartialViewToString("_CheckoutPopup", model) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataCheckoutByNonKuyamByAppId(int apptId)
        {
            var nonAppointment = _appointmentService.GetLastNonKuyamAppointmentById(apptId);
            var company = nonAppointment.ProfileCompany;
            DateTime dtend = nonAppointment.Start.AddMinutes(nonAppointment.Duration.Value);
            var model = new CheckoutModel()
            {
                CheckOutSummary = "appointment summary",
                ProfileId = company.ProfileID,
                NonApptTempId = apptId,
                CheckoutType = (int)Types.CheckoutType.NonAvailability,
                PaymentMethod = company.PaymentMethod ?? 0,
                IsPackage = false,
                PackageId = string.Empty,
                CompanyName = company.Name,
                Address = company.Street1 + " " + company.Street2,
                City = !string.IsNullOrWhiteSpace(company.City) ? string.Format("{0}, {1} {2}", company.City, company.State, company.Zip) : string.Format("{1} {2}", company.City, company.State, company.Zip),
                EmployeeName = Kuyam.Domain.UtilityHelper.TruncateText(nonAppointment.EmployeeName, 12),
                CalendarName = Kuyam.Domain.UtilityHelper.TruncateText(nonAppointment.Calendar.Name, 12),
                ServiceName = UtilityHelper.TruncateText(nonAppointment.ServiceName, 12),
                Price = nonAppointment.Price ?? 0,
                Totaldue = nonAppointment.Price ?? 0,
                Duration = nonAppointment.Duration ?? 0,
                Datetime = String.Format("{0:ddd, MMM d}", nonAppointment.Start),
                Time = String.Format("{0:t}", nonAppointment.Start).Replace(" ", string.Empty).ToLower() + " - " + String.Format("{0:t}", dtend).Replace(" ", string.Empty).ToLower()
            };

            MySession.Appointmentbooking = nonAppointment.ToAppointment();
            var appointmentTemp = MySession.Appointmentbooking.ToAppointmentTemp();
            _appointmentService.InsertAppointmentTemp(appointmentTemp);
            MySession.AppointmentbookingTempId = appointmentTemp.AppointmentID;

            return Json(new { content = this.RenderPartialViewToString("_CheckoutPopup", model) }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetProposedDataCheckoutByAppId(int apptId)
        {
            var appointment = _appointmentService.GetProposedAppointmentById(apptId);
            if (appointment == null || appointment.CustID != MySession.CustID)
                return Json(new { content = string.Empty }, JsonRequestBehavior.AllowGet);

            var company = appointment.ProfileCompany;
            DateTime dtend = appointment.Start.AddMinutes(appointment.Duration.Value);
            var model = new CheckoutModel()
            {
                CheckOutSummary = "appointment summary",
                ProfileId = company.ProfileID,
                NonApptTempId = apptId,
                CheckoutType = (int)Types.CheckoutType.GeneralAvailability,
                PaymentMethod = company.PaymentMethod ?? 0,
                IsPackage = false,
                PackageId = string.Empty,
                CompanyName = company.Name,
                Address = company.Street1 + " " + company.Street2,
                City = !string.IsNullOrWhiteSpace(company.City) ? string.Format("{0}, {1} {2}", company.City, company.State, company.Zip) : string.Format("{1} {2}", company.City, company.State, company.Zip),
                EmployeeName = Kuyam.Domain.UtilityHelper.TruncateText(appointment.EmployeeName, 12),
                CalendarName = Kuyam.Domain.UtilityHelper.TruncateText(appointment.Calendar.Name, 12),
                ServiceName = UtilityHelper.TruncateText(appointment.ServiceName, 12),
                Price = appointment.Price ?? 0,
                Totaldue = appointment.Price ?? 0,
                Duration = appointment.Duration ?? 0,
                Datetime = String.Format("{0:ddd, MMM d}", appointment.Start),
                Time = String.Format("{0:t}", appointment.Start).Replace(" ", string.Empty).ToLower() + " - " + String.Format("{0:t}", dtend).Replace(" ", string.Empty).ToLower()
            };

            MySession.Appointmentbooking = appointment.ToAppointment();
            var appointmentTemp = MySession.Appointmentbooking.ToAppointmentTemp();
            _appointmentService.InsertAppointmentTemp(appointmentTemp);
            MySession.AppointmentbookingTempId = appointmentTemp.AppointmentID;

            return Json(new { content = this.RenderPartialViewToString("_CheckoutPopup", model) }, JsonRequestBehavior.AllowGet);

        }


        #region book appointment by Paypal

        [HttpPost]
        public ActionResult BookAppointment(int checkoutType, int? apptId, string sms, string email, string mess, string price, int? duration, string promoCode, string giftCode)
        {
            decimal apptprice = 0;
            Decimal.TryParse(price, out apptprice);
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

            if (MySession.Appointmentbooking == null)
            {
                MySession.Appointmentbooking = new Appointment();
            }

            MySession.IsCashAppoiment = false;
            MySession.DiscountCode = promoCode ?? string.Empty;
            MySession.GiftCardCode = giftCode ?? string.Empty;

            MySession.Appointmentbooking.AppointmentID = apptId ?? 0;
            MySession.Appointmentbooking.Notes = mess;
            MySession.Appointmentbooking.ContactType = (int)contactType;
            MySession.Appointmentbooking.Price = apptprice;
            MySession.Appointmentbooking.Duration = duration;

            string redirectAction = "/PayPal/AvailabilityPreapprove";
            if (checkoutType == (int)Types.CheckoutType.Availability || checkoutType == (int)Types.CheckoutType.ClassBooking)
            {
                redirectAction = "/PayPal/AvailabilityPreapprove";
            }
            else if (checkoutType == (int)Types.CheckoutType.NonAvailability)
            {
                redirectAction = "/PayPal/NonkuyamPreapprove";
            }
            else if (checkoutType == (int)Types.CheckoutType.GeneralAvailability)
            {
                redirectAction = "/PayPal/ProposedPreapprove";
            }
            return Json(new { redirectAction = redirectAction }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmAppointment(string sms, string email, int? apptId, string mess, string price, int? duration, string promoCode, string giftCode)
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

            if (MySession.Appointmentbooking == null)
            {
                MySession.Appointmentbooking = new Appointment();
            }

            MySession.IsCashAppoiment = false;
            MySession.Appointmentbooking.AppointmentID = apptId ?? 0;
            MySession.Appointmentbooking.Notes = mess;
            MySession.Appointmentbooking.ContactType = (int)contactType;
            MySession.Appointmentbooking.Price = dbprice;
            MySession.Appointmentbooking.Duration = duration;
            MySession.DiscountCode = promoCode ?? string.Empty;
            MySession.GiftCardCode = giftCode ?? string.Empty;

            SavePaymentInfo(MySession.Appointmentbooking);

            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AvailabilityPreapprove()
        {
            string cancelUrl = string.Format("{0}CompanyProfile/availability", EmailHelper.GetStoreHost());
            string currencyCode = ConfigManager.currencyCode;
            string returnUrl = string.Format("{0}PayPal/PayPal", EmailHelper.GetStoreHost());

            string startingDate = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            decimal amoutMax = (MySession.Appointmentbooking != null && MySession.Appointmentbooking.Price.HasValue) ? (decimal)MySession.Appointmentbooking.Price.Value : 1;
            Discount discount = _orderService.GetDiscoutByCode(MySession.DiscountCode);
            decimal totalDiscount = 0;
            if (discount != null)
            {
                decimal discountamount = 0;
                if (discount.Amount > 0)
                {
                    discountamount = discount.Amount;
                }
                else
                {
                    discountamount = amoutMax * discount.Percent / 100;
                }

                totalDiscount += discountamount;
            }

            GiftCard giftCard = _giftCardServices.GetGiftCardByGiftCardCode(MySession.GiftCardCode);
            if (giftCard != null)
            {
                var used = giftCard.GiftCardHistories.Sum(m => m.UsedValue);
                var giftBlance = giftCard.Amount - used;
                totalDiscount += giftBlance;
            }

            var req = new PreapprovalRequest(new RequestEnvelope("en_US"), cancelUrl, currencyCode, returnUrl, startingDate);
            req.endingDate = DateTime.UtcNow.ToUniversalTime().AddDays(int.Parse(ConfigurationManager.AppSettings["Paypal_Payment_Pending_Days"])).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            amoutMax = amoutMax - totalDiscount;
            req.maxTotalAmountOfAllPayments = amoutMax;
            req.displayMaxTotalAmount = true;
            req.maxAmountPerPayment = amoutMax;
            req.feesPayer = ConfigurationManager.AppSettings["FeesPayer"];
            //req.maxNumberOfPayments = 2;
            if (Request.UrlReferrer != null)
            {
                req.cancelUrl = Request.UrlReferrer + "?paypalcancel=true";
            }
            req.memo = "fee for appointment booked at kuyam.com";
            var service = new AdaptivePaymentsService();
            PreapprovalResponse resp;
            try
            {
                resp = service.Preapproval(req);
            }
            catch (System.Exception e)
            {
                throw new Exception(e.Message);
            }
            string redirectUrl = null;
            if (resp.responseEnvelope.ack != AckCode.FAILURE && resp.responseEnvelope.ack != AckCode.FAILUREWITHWARNING)
            {
                if (MySession.Appointmentbooking != null)
                {
                    MySession.Appointmentbooking.PreapprovalKey = resp.preapprovalKey;
                }
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-preapproval&preapprovalkey=" + resp.preapprovalKey;
            }

            if (resp.error != null && resp.error.Count > 0)
            {
                string error = string.Empty;
                foreach (ErrorData ero in resp.error)
                {
                    error = string.Join(",", ero.message);
                }
                throw new Exception(error);
                // process Mes
            }
            if (redirectUrl != null)
            {
                //Response.Redirect(redirectUrl);
                return Redirect(redirectUrl);

            }

            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult NonkuyamPreapprove()
        {
            string cancelUrl = string.Format("{0}company/companysearch", EmailHelper.GetStoreHost());
            string currencyCode = ConfigManager.currencyCode;
            string returnUrl = string.Format("{0}PayPal/PayPal", EmailHelper.GetStoreHost());

            string startingDate = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            decimal amoutMax = (MySession.Appointmentbooking != null && MySession.Appointmentbooking.Price.HasValue) ? (decimal)MySession.Appointmentbooking.Price.Value : 1;
            Discount discount = _orderService.GetDiscoutByCode(MySession.DiscountCode);
            if (discount != null)
            {
                int discountId = discount.DiscountId;
                decimal discountamount = 0;
                if (discount.Amount > 0)
                {
                    discountamount = discount.Amount;
                }
                else
                {
                    discountamount = amoutMax * discount.Percent / 100;
                }

                amoutMax -= discountamount;
            }

            GiftCard giftCard = _giftCardServices.GetGiftCardByGiftCardCode(MySession.GiftCardCode);
            if (giftCard != null)
            {
                var used = giftCard.GiftCardHistories.Sum(m => m.UsedValue);
                var giftBlance = giftCard.Amount - used;
                amoutMax -= giftBlance;
            }

            var req = new PreapprovalRequest(new RequestEnvelope("en_US"), cancelUrl, currencyCode, returnUrl, startingDate);
            req.endingDate = DateTime.UtcNow.ToUniversalTime().AddDays(int.Parse(ConfigurationManager.AppSettings["Paypal_Payment_Pending_Days"])).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            req.maxTotalAmountOfAllPayments = amoutMax;
            req.displayMaxTotalAmount = true;
            req.maxAmountPerPayment = amoutMax;
            req.feesPayer = ConfigurationManager.AppSettings["FeesPayer"] as string;
            //req.maxNumberOfPayments = 2;
            if (Request.UrlReferrer != null)
            {
                if (Request.UrlReferrer.ToString().IndexOf('?') == -1)
                    req.cancelUrl = Request.UrlReferrer + "?paypalcancel=true";
                else
                    req.cancelUrl = Request.UrlReferrer + "&paypalcancel=true";
            }
            req.memo = "fee for appointment booked at kuyam.com";
            var service = new AdaptivePaymentsService();
            PreapprovalResponse resp = null;
            try
            {
                resp = service.Preapproval(req);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            string redirectUrl = null;
            if (resp.responseEnvelope.ack != AckCode.FAILURE && resp.responseEnvelope.ack != AckCode.FAILUREWITHWARNING)
            {
                if (MySession.Appointmentbooking != null)
                {
                    MySession.Appointmentbooking.PreapprovalKey = resp.preapprovalKey;
                }
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-preapproval&preapprovalkey=" + resp.preapprovalKey;
            }

            if (resp.error != null && resp.error.Count > 0)
            {

                string error = string.Empty;
                foreach (ErrorData ero in resp.error)
                {
                    error = string.Join(",", ero.message);
                }
                throw new Exception(error);
                // process Mes

            }
            if (redirectUrl != null)
            {

                return Redirect(redirectUrl);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProposedPreapprove()
        {
            string cancelUrl = string.Format("{0}CompanyProfile/availability/{1}?proposedId={2}", EmailHelper.GetStoreHost(), MySession.Appointmentbooking.ProfileId, MySession.Appointmentbooking.AppointmentID);
            string currencyCode = ConfigManager.currencyCode;
            string returnUrl = string.Format("{0}PayPal/PayPal", EmailHelper.GetStoreHost());

            string startingDate = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            decimal amoutMax = (MySession.Appointmentbooking != null && MySession.Appointmentbooking.Price.HasValue) ? (decimal)MySession.Appointmentbooking.Price.Value : 1;
            Discount discount = _orderService.GetDiscoutByCode(MySession.DiscountCode);
            if (discount != null)
            {
                int discountId = discount.DiscountId;
                decimal discountamount = 0;
                if (discount.Amount > 0)
                {
                    discountamount = discount.Amount;
                }
                else
                {
                    discountamount = amoutMax * discount.Percent / 100;
                }

                amoutMax -= discountamount;
            }

            GiftCard giftCard = _giftCardServices.GetGiftCardByGiftCardCode(MySession.GiftCardCode);
            if (giftCard != null)
            {
                amoutMax -= giftCard.Amount;
            }

            var req = new PreapprovalRequest(new RequestEnvelope("en_US"), cancelUrl, currencyCode, returnUrl, startingDate);
            req.endingDate = DateTime.UtcNow.ToUniversalTime().AddDays(int.Parse(ConfigurationManager.AppSettings["Paypal_Payment_Pending_Days"])).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            req.maxTotalAmountOfAllPayments = amoutMax;
            req.displayMaxTotalAmount = true;
            req.maxAmountPerPayment = amoutMax;
            req.feesPayer = ConfigurationManager.AppSettings["FeesPayer"] as string;
            //req.maxNumberOfPayments = 2;
            if (Request.UrlReferrer != null)
            {
                if (Request.UrlReferrer.ToString().IndexOf('?') == -1)
                    req.cancelUrl = Request.UrlReferrer + "?paypalcancel=true";
                else
                    req.cancelUrl = Request.UrlReferrer + "&paypalcancel=true";
            }
            req.memo = "fee for appointment booked at kuyam.com";
            var service = new AdaptivePaymentsService();
            PreapprovalResponse resp = null;
            try
            {
                resp = service.Preapproval(req);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            string redirectUrl = null;
            if (resp.responseEnvelope.ack != AckCode.FAILURE && resp.responseEnvelope.ack != AckCode.FAILUREWITHWARNING)
            {
                if (MySession.Appointmentbooking != null)
                {
                    MySession.Appointmentbooking.PreapprovalKey = resp.preapprovalKey;
                }
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-preapproval&preapprovalkey=" + resp.preapprovalKey;
            }

            if (resp.error != null && resp.error.Count > 0)
            {

                string error = string.Empty;
                foreach (ErrorData ero in resp.error)
                {
                    error = string.Join(",", ero.message);
                }
                throw new Exception(error);
                // process Mes

            }
            if (redirectUrl != null)
            {

                return Redirect(redirectUrl);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayPal()
        {
            if (MySession.Appointmentbooking != null)
            {
                var req = new PreapprovalDetailsRequest(new RequestEnvelope("en_US"), MySession.Appointmentbooking.PreapprovalKey);
                req.getBillingAddress = true;
                var service = new AdaptivePaymentsService();
                PreapprovalDetailsResponse resp = service.PreapprovalDetails(req);
                MySession.Appointmentbooking.SenderEmail = resp.senderEmail;
                SavePaymentInfo(MySession.Appointmentbooking);
            }
            return RedirectToAction("Index", "Appointment");
        }

        private void SavePaymentInfo(Appointment appointment)
        {
            if (appointment != null)
            {
                if (appointment.AppointmentID > 0)
                {
                    int apptId = appointment.AppointmentID;
                    var nonAppt = _appointmentService.GetLastNonKuyamAppointmentById(apptId);
                    if (nonAppt != null)
                    {
                        MySession.CalendarId = nonAppt.CalendarId ?? 0;
                        nonAppt.AppointmentStatusID = (int)Types.NonKuyamAppointment.booked;
                        _appointmentService.UpdateNonKuyamAppointment(nonAppt);
                    }

                    var proposedAppt = _appointmentService.GetProposedAppointmentById(apptId);
                    if (proposedAppt != null)
                    {
                        MySession.CalendarId = proposedAppt.CalendarId ?? 0;
                        proposedAppt.AppointmentStatusID = (int)Types.ProposedAppointmentStatus.booked;
                        _appointmentService.UpdateProposedKuyamAppointment(proposedAppt);
                    }
                }

                appointment.AppointmentStatusID = (int)Types.AppointmentStatus.Pending;
                if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                    && MySession.Concierge != null && MySession.HotelId > 0)
                {
                    appointment.HotelID = MySession.HotelId;
                    appointment.StaffID = MySession.Concierge.Id;
                }

                _appointmentService.AddAppointment(appointment);
                MySession.AppoimentID = appointment.AppointmentID;

                int appId = appointment.AppointmentID;

                if (appointment.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending)
                {
                    appointment.AppointmentStatusID = (int)Types.AppointmentStatus.Pending;
                    _appointmentService.UpdateAppointment(appointment);
                }

                if (!string.IsNullOrEmpty(MySession.Appointmentbooking.Notes))
                {
                    var item = new AppointmentLog
                    {
                        Viewed = false,
                        Message = MySession.Appointmentbooking.Notes.Replace("\n", "<br/>"),
                        AppointmentID = appId,
                        CustID = MySession.CustID,
                        LogDT = DateTime.UtcNow
                    };
                    _appointmentService.AddNote(item);

                }
                if (appointment.Price != null)
                {
                    decimal apptPrice = appointment.Price.Value;
                    decimal totaldue = 0;
                    decimal discountamount = 0;
                    string discountCodeNumber = string.Empty;

                    var company = appointment.GetProfileCompany();

                    int profileID = company.ProfileID;

                    Discount discount = _orderService.GetDiscoutByCode(MySession.DiscountCode);
                    int discountId = 0;
                    if (discount != null)
                    {
                        discountId = discount.DiscountId;
                        if (discount.Amount > 0)
                        {
                            discountamount = discount.Amount;
                        }
                        else
                        {
                            discountamount = apptPrice * discount.Percent / 100;
                        }
                        discountCodeNumber = discount.Code;

                    }

                    totaldue = apptPrice - discountamount;

                    GiftCard giftCard = _giftCardServices.GetGiftCardByGiftCardCode(MySession.GiftCardCode);
                    decimal useGiftAmount = 0;
                    if (giftCard != null)
                    {
                        useGiftAmount = (giftCard.Amount - giftCard.GiftCardHistories.Sum(m => m.UsedValue));
                        if (useGiftAmount > totaldue)
                            useGiftAmount = totaldue;
                        if (useGiftAmount < 0)
                            useGiftAmount = 0;
                        if (useGiftAmount > 0)
                        {
                            var giftUserHistory = new GiftCardHistory
                            {
                                GiftCardId = giftCard.Id,
                                AppointmentId = appId,
                                UsedValue = useGiftAmount,
                                Created = DateTime.UtcNow
                            };

                            _giftCardServices.GiftRedeem(giftUserHistory);

                            totaldue = totaldue - useGiftAmount;
                        }
                    }


                    if (totaldue < 0)
                        totaldue = 0;

                    var regularClient = _orderService.GetRegularClientByEmail(MySession.Username, profileID);
                    decimal kuyamFee = MyApp.Settings.PaySetting.PercentKuyamFee;
                    if (regularClient != null)
                    {
                        kuyamFee = 0;
                    }
                    decimal paymentFeeTotal = (totaldue * MyApp.Settings.PaySetting.PercentPaymentFee / 100) + MyApp.Settings.PaySetting.TransactionAdditionalFee;
                    decimal kuyamFeeTotal = (totaldue * kuyamFee / 100) + MyApp.Settings.PaySetting.AppointmentAdditionalFee;

                    decimal kuyamPaidTotal = 0;
                    decimal companyPaidTotal = 0;
                    decimal companyCharge = 0;

                    if (discount != null && discount.DiscountType == (int)Types.DiscountType.Admin)
                    {
                        kuyamPaidTotal = totaldue;
                        companyPaidTotal = 0;
                    }
                    else
                    {
                        kuyamPaidTotal = kuyamFeeTotal;
                        companyPaidTotal = totaldue - kuyamFeeTotal;
                    }

                    var order = new Order()
                    {
                        OrderGuID = Guid.NewGuid().ToString(),
                        CustID = MySession.CustID,
                        ProfileID = profileID,
                        PaymentMethodID = (int)Types.PaymentMethod.Paypal,
                        PaymentStatusID = (int)Types.PaymentStatus.Pending,
                        OrderStatusID = (int)Types.AppointmentStatus.Pending,
                        PercentKuyamFee = kuyamFee,
                        AppointmentAdditionalFee = MyApp.Settings.PaySetting.AppointmentAdditionalFee,
                        PercentPaymentFee = MyApp.Settings.PaySetting.PercentPaymentFee,
                        TransactionAdditionalFee = MyApp.Settings.PaySetting.TransactionAdditionalFee,
                        PaymentFeeTotal = paymentFeeTotal,
                        KuyamFeeTotal = kuyamFeeTotal,
                        DiscountCodeNumber = discountCodeNumber,
                        OrderDiscount = discountamount,
                        GiftCardCodeNumber = MySession.GiftCardCode,
                        GiftCardAmount = useGiftAmount,
                        PurchaseOrderNumber = UtilityHelper.GenerateRandomDigitCode(8),
                        KuyamPaidTotal = kuyamPaidTotal,
                        CompanyPaidTotal = companyPaidTotal,
                        OrderSubtotal = apptPrice,
                        OrderTotal = totaldue,
                        Deleted = false,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    if (company != null && company.CompanyTypeID == (int)Types.CompanyType.KuyamInstantBook)
                    {
                        if (!string.IsNullOrEmpty(appointment.PreapprovalKey))
                        {
                            string Key = appointment.PreapprovalKey;
                            ConfirmPreapprovalRequest req = new ConfirmPreapprovalRequest(new RequestEnvelope("en_US"), Key);
                            AdaptivePaymentsService service = new AdaptivePaymentsService();
                            ConfirmPreapprovalResponse resp = null;
                            try
                            {
                                resp = service.ConfirmPreapproval(req);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("PayPal Confirm fail:", ex);
                            }

                            int countMinutes = company.CancelHour.HasValue ? company.CancelHour.Value * 60 : 0;
                            int payDefault;
                            int.TryParse(ConfigManager.PayDate, out payDefault);
                            DateTime paidDate = countMinutes != 0 ? appointment.Start.AddMinutes(-countMinutes) : appointment.Start.AddMinutes(-payDefault * 60);
                            paidDate = DateTimeUltility.ConvertToUtcTime(paidDate);

                            decimal paidPrice = 0;
                            order.PaidDateUtc = paidDate;
                            order.OrderStatusID = (int)Types.AppointmentStatus.Confirmed;
                            if (order.OrderTotal != null)
                                paidPrice = order.OrderTotal.Value;

                            companyCharge = paidPrice - kuyamFeeTotal;

                            if (discount != null && discount.DiscountType == (int)Types.DiscountType.Admin)
                            {
                                companyCharge = 0;
                            }

                            var obj = new IncomingRequest
                            {
                                EntityId = "KuyamWeb",
                                DateAlert = paidDate,
                                Data = UtilityHelper.ObjectToXml(
                                    new
                                    {
                                        PreapprovalKey = MySession.Appointmentbooking.PreapprovalKey,
                                        Companytype = company.CompanyTypeID,
                                        EmailSender = appointment.SenderEmail,
                                        CompanyChargePrice = companyCharge,
                                        KuyamChargePrice = paidPrice,
                                        CompanyEmailReceive = company.PaymentOptions,
                                        KuyamEmailReceice = MyApp.Settings.PaySetting.PaypalAccount
                                    })
                            };
                            try
                            {
                                var serviceInfo = new InfoConnSoapClient();
                                serviceInfo.AddIncomingRequest(obj, IncommingRequestType.PAYMENT_PAYPAL);
                            }
                            catch (Exception ex)
                            {
                                order.OrderStatusID = (int)Types.AppointmentStatus.Pending;
                                LogHelper.Error("PayPal fail:", ex);

                            }
                        }
                        appointment.AppointmentStatusID = (int)Types.AppointmentStatus.Confirmed;
                        _appointmentService.UpdateAppointment(appointment);
                        if (appointment.ContactType == (int)Types.ContactType.Email
                        || appointment.ContactType == (int)Types.ContactType.SMS
                        || appointment.ContactType == (int)Types.ContactType.EmailSMS)
                        {                            
                            string emailstring = GetEmailTemplateAppointment(appointment.AppointmentID, "CompanyConfirm");
                            ConfirmNotification(appointment, emailstring);
                        }

                    }

                    _orderService.InsertOrder(order);

                    if (discount != null)
                    {
                        var userDiscount = new UserDiscount
                        {
                            CustId = MySession.CustID,
                            DiscountId = discountId,
                            NumberofUsage = 1,
                            DateUsage = DateTime.UtcNow,
                            OrderID = order.OrderID
                        };

                        _orderService.InsertUserDiscount(userDiscount);
                    }

                    var orderDetail = new OrderDetail
                    {
                        AppointmentID = appointment.AppointmentID,
                        OrderDetailGuid = Guid.NewGuid().ToString(),
                        OrderID = order.OrderID,
                        Price = apptPrice,
                        DiscountAmount = discountamount

                    };
                    _orderService.InsertOrderDetail(orderDetail);

                    var AppointmentbookingTmp = _appointmentService.GetAppoinmentTempsById(MySession.AppointmentbookingTempId);
                    _appointmentService.DeleteAppointmentTemp(AppointmentbookingTmp);

                    //reminders
                    SendReminders(appointment);

                    Thread zendeskThread = new Thread(() =>
                    {
                        try
                        {
                            ZendeskAdd(appointment, company, "has beeen created");
                        }
                        catch (Exception ex)
                        {
                            //Todo: Handle Exception Occur
                            LogHelper.Error("PayPal fail:", ex);
                        }

                    });
                    zendeskThread.Start();

                }

                MySession.Appointmentbooking = null;

            }
        }

        #endregion paypal

        #region Pay In Person

        [HttpPost]
        public ActionResult BookAppointmentCash(AppointmentBookingModel model)
        {
            var contactType = Types.ContactType.None;
            if (model.SMS == "checked" && model.Email == "checked")
            {
                contactType = Types.ContactType.EmailSMS;
            }
            else if (model.SMS == "checked")
            {
                contactType = Types.ContactType.SMS;
            }
            else if (model.Email == "checked")
            {
                contactType = Types.ContactType.Email;
            }

            MySession.IsCashAppoiment = true;
            var aptTemp = _appointmentService.GetAppoinmentTempsById(MySession.AppointmentbookingTempId);
            Appointment appointment = aptTemp.ToAppointment();
            appointment.Notes = model.Message;
            appointment.ContactType = (int)contactType;
            appointment.Price = model.Price;
            appointment.Duration = model.Duration;
            appointment.AppointmentStatusID = (int)Types.AppointmentStatus.Pending;
            SavePaymentInfo(appointment);

            return Json("true", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult BookNonAppointmentCash(string sms, string email, string mess, string price, int? duration, int apptId, string promoCode)
        {
            var nonAppt = _appointmentService.GetLastNonKuyamAppointmentById(apptId);
            if (nonAppt.CalendarId != null) MySession.CalendarId = nonAppt.CalendarId.Value;
            MySession.IsCashAppoiment = true;
            nonAppt.AppointmentStatusID = (int)Types.NonKuyamAppointment.booked;
            _appointmentService.UpdateNonKuyamAppointment(nonAppt);

            if (string.IsNullOrEmpty(price))
                return Json("false", JsonRequestBehavior.AllowGet); ;
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

            var entity = new Appointment()
            {
                CustID = nonAppt.CustID,
                ProfileId = nonAppt.ProfileId,
                Price = dbprice,
                ServiceName = nonAppt.Service.ServiceName,
                StatusChangeDate = DateTime.UtcNow,
                Start = nonAppt.Start,
                End = nonAppt.End,
                Duration = nonAppt.Duration,
                EmployeeName = nonAppt.EmployeeName,
                AppointmentStatusID = (int)Types.AppointmentStatus.Pending,
                AttendeesNumber = 1,
                ContactType = (int)contactType,
                Notes = mess,
                Desc = mess

            };

            if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                      && MySession.Concierge != null && MySession.HotelId > 0)
            {
                entity.HotelID = MySession.HotelId;
                entity.StaffID = MySession.Concierge.Id;
            }

            SavePaymentInfo(entity);

            return Json("true", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult BookProposedAppointmentCash(string sms, string email, string mess, string price, int? duration, int apptId, string promoCode)
        {
            var proposedAppt = _appointmentService.GetProposedAppointmentById(apptId);
            if (proposedAppt.CalendarId != null) MySession.CalendarId = proposedAppt.CalendarId.Value;
            MySession.IsCashAppoiment = true;
            proposedAppt.AppointmentStatusID = (int)Types.ProposedAppointmentStatus.booked;
            _appointmentService.UpdateProposedKuyamAppointment(proposedAppt);

            if (string.IsNullOrEmpty(price))
                return Json("false", JsonRequestBehavior.AllowGet); ;
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

            string serviceName = proposedAppt.ServiceCompany != null ? proposedAppt.ServiceCompany.Service.ServiceName :
                proposedAppt.Service.ServiceName;
            var entity = new Appointment()
            {
                CustID = proposedAppt.CustID,
                ProfileId = proposedAppt.ProfileId,
                Price = dbprice,
                ServiceCompanyID = proposedAppt.ServiceCompanyId,
                ServiceName = serviceName,
                StatusChangeDate = DateTime.UtcNow,
                Start = proposedAppt.Start,
                End = proposedAppt.End,
                Duration = proposedAppt.Duration,
                EmployeeName = proposedAppt.EmployeeName,
                AppointmentStatusID = (int)Types.AppointmentStatus.Pending,
                AttendeesNumber = 1,
                ContactType = (int)contactType,
                Notes = mess,
                Desc = mess

            };

            if (MySession.OriginalCustIfImpersonated != null && MySession.ImpersonateId > 0
                       && MySession.Concierge != null && MySession.HotelId > 0)
            {
                entity.HotelID = MySession.HotelId;
                entity.StaffID = MySession.Concierge.Id;
            }

            SavePaymentInfo(entity);

            return Json("true", JsonRequestBehavior.AllowGet);

        }


        #endregion pay in person

        #region buy package by paypal

        public ActionResult PackagePreapprove()
        {
            if (MySession.UserPackagePurchase != null && MySession.UserPackagePurchase.PurchasePrice <= 0)
            {
                PreapprovalDetailsRequest _req = new PreapprovalDetailsRequest(new RequestEnvelope("en_US"), MySession.PreapprovalKey);
                _req.getBillingAddress = true;

                AdaptivePaymentsService _service = new AdaptivePaymentsService();
                PreapprovalDetailsResponse _resp = null;
                _resp = _service.PreapprovalDetails(_req);
                string EmailSender = _resp.senderEmail;
                AddPayPackage(EmailSender);
                return RedirectToAction("Package", "CompanyProfile", new { id = MySession.PurchaseCompanyProfileId });
            }

            string cancelUrl = string.Format("{0}CompanyProfile/availability", EmailHelper.GetStoreHost());
            string currencyCode = ConfigManager.currencyCode;
            string returnUrl = string.Format("{0}Paypal/PayPackage", EmailHelper.GetStoreHost());

            string startingDate = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            decimal amoutMax = (MySession.UserPackagePurchase != null) ? (decimal)MySession.UserPackagePurchase.PurchasePrice : 1;

            PreapprovalRequest req = new PreapprovalRequest(new RequestEnvelope("en_US"), cancelUrl, currencyCode, returnUrl, startingDate);
            req.endingDate = DateTime.UtcNow.ToUniversalTime().AddDays(int.Parse(ConfigurationManager.AppSettings["Paypal_Payment_Pending_Days"])).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            req.maxTotalAmountOfAllPayments = amoutMax;
            req.displayMaxTotalAmount = true;
            req.maxAmountPerPayment = amoutMax;
            req.feesPayer = ConfigurationManager.AppSettings["FeesPayer"] as string;
            //req.maxNumberOfPayments = 1;
            if (Request.UrlReferrer != null)
            {
                req.cancelUrl = Request.UrlReferrer.ToString();
            }
            req.memo = "fee for purchased package at kuyam.com";
            AdaptivePaymentsService service = new AdaptivePaymentsService();
            PreapprovalResponse resp = null;
            try
            {
                resp = service.Preapproval(req);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            string redirectUrl = null;
            if (resp.responseEnvelope.ack != AckCode.FAILURE &&
                resp.responseEnvelope.ack != AckCode.FAILUREWITHWARNING)
            {
                MySession.PreapprovalKey = resp.preapprovalKey;
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-preapproval&preapprovalkey=" + resp.preapprovalKey;
            }

            if (resp.error != null && resp.error.Count > 0)
            {

                string error = string.Empty;
                foreach (ErrorData ero in resp.error)
                {
                    error = string.Join(",", ero.message);
                }
                throw new Exception(error);
                // process Mes

            }
            if (redirectUrl != null)
            {
                return Redirect(redirectUrl);

            }

            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayPackage()
        {
            //Get Key Preapprove below db            
            if (MySession.UserPackagePurchase != null)
            {
                PreapprovalDetailsRequest _req = new PreapprovalDetailsRequest(new RequestEnvelope("en_US"), MySession.PreapprovalKey);
                _req.getBillingAddress = true;

                AdaptivePaymentsService _service = new AdaptivePaymentsService();
                PreapprovalDetailsResponse _resp = null;
                _resp = _service.PreapprovalDetails(_req);
                string EmailSender = _resp.senderEmail;

                //Insert EmailSender --> db 
                AddPayPackage(EmailSender);
            }

            var urlName = new UserPackagePurchase().GetSeName(MySession.PurchaseCompanyProfileId);
            string urlredirect = Url.RouteUrl("availability", new { seName = urlName }) + "/package";
            if (MySession.IsBookDirect)
                urlredirect = Url.RouteUrl("Booking - Company Package", new { companyUrlName = urlName });
            return Redirect(urlredirect);
        }

        [NonAction]
        private void AddPayPackage(string emailSender)
        {
            int profileId = 0;
            //Get Key Preapprove below db            
            if (MySession.UserPackagePurchase != null)
            {
                profileId = MySession.PurchaseCompanyProfileId;
                ProfileCompany profile = _companyProfileService.GetProfileCompanyByID(profileId);
                decimal dbprice = MySession.UserPackagePurchase.CompanyPackage.Price;
                int numberOfBooking = MySession.UserPackagePurchase.CompanyPackage.NumberOfBooking;
                if (numberOfBooking == (int)Types.CompanyPackageType.ByUnlimited)
                    MySession.UserPackagePurchase.MaxUses = -1;
                MySession.UserPackagePurchase.CompanyPackage = null;
                _companyProfileService.InsertUserPackagePurchase(MySession.UserPackagePurchase);


                decimal totaldue = 0;
                decimal discountamount = 0;
                string discountCodeNumber = string.Empty;
                Discount discount = _orderService.GetDiscoutByCode(MySession.DiscountCode);
                int discountId = 0;
                if (discount != null)
                {
                    discountId = discount.DiscountId;
                    if (discount.Amount > 0)
                    {
                        discountamount = discount.Amount;
                    }
                    else
                    {
                        discountamount = dbprice * discount.Percent / 100;
                    }
                    discountCodeNumber = discount.Code;

                }
                totaldue = dbprice - discountamount;
                if (totaldue < 0)
                    totaldue = 0;

                var order = new Order()
                {
                    OrderGuID = Guid.NewGuid().ToString(),
                    CustID = MySession.CustID,
                    ProfileID = profileId,
                    PaymentMethodID = (int)Types.PaymentMethod.PayInPerson,
                    PaymentStatusID = (int)Types.PaymentStatus.Paid,
                    OrderStatusID = (int)Types.OrderStatus.Complete,
                    DiscountCodeNumber = discountCodeNumber,
                    OrderDiscount = discountamount,
                    OrderTotal = totaldue,
                    OrderSubtotal = dbprice,
                    Deleted = false,
                    CreatedOnUtc = DateTime.UtcNow
                };
                _orderService.InsertOrder(order);

                if (discount != null)
                {
                    UserDiscount userDiscount = new UserDiscount
                    {
                        CustId = MySession.CustID,
                        DiscountId = discountId,
                        NumberofUsage = 1,
                        DateUsage = DateTime.UtcNow,
                        OrderID = order.OrderID
                    };
                    _orderService.InsertUserDiscount(userDiscount);
                }

                var orderDetail = new OrderDetail
                {
                    UserPackagePurchaseId = MySession.UserPackagePurchase.UserPackagePurchaseId,
                    OrderDetailGuid = Guid.NewGuid().ToString(),
                    OrderID = order.OrderID,
                    Price = dbprice,
                    DiscountAmount = discountamount

                };
                _orderService.InsertOrderDetail(orderDetail);

                InfoConnSoapClient serviceInfo = new InfoConnSoapClient();
                IncomingRequest obj = new IncomingRequest
                {
                    EntityId = "KuyamWeb",
                    DateAlert = DateTime.UtcNow,
                    Data = Kuyam.Domain.UtilityHelper.ObjectToXml(new
                    {
                        PreapprovalKey = MySession.PreapprovalKey,
                        Companytype = profile.CompanyTypeID,
                        EmailSender = emailSender,
                        CompanyChargePrice = MySession.UserPackagePurchase.PurchasePrice,
                        KuyamChargePrice = 0,
                        CompanyEmailReceive = profile.PaymentOptions,
                        KuyamEmailReceice = MyApp.Settings.PaySetting.PaypalAccount
                    })
                };


                try
                {
                    serviceInfo.AddIncomingRequest(obj, IncommingRequestType.PAYMENT_PAYPAL);
                }
                catch (Exception ex)
                {
                    //Todo: Handle exception occur
                    LogHelper.Error("PayPackage fail", ex);
                }
                MySession.UserPackagePurchase = null;
            }

        }

        #endregion buy package by paypal

        #region pay appointment by package

        [HttpPost]
        public ActionResult BookAppointmentByPackage(string sms, string email, string mess, string price, int? duration, string packageId)
        {
            if (MySession.Appointmentbooking == null || string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(price))
                return Json("false", JsonRequestBehavior.AllowGet);
            string decryptpackageId = SecurityHelper.DecryptStringFromBytesAes(packageId.Replace(" ", "+"));
            int Id = 0;
            int.TryParse(decryptpackageId, out Id);
            UserPackagePurchase userPackagePurchase = _companyProfileService.GetUserPackagePurchasePackageID(Id);
            if (userPackagePurchase == null || userPackagePurchase.MaxUses == 0)
                return Json("false", JsonRequestBehavior.AllowGet);
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

            MySession.IsCashAppoiment = true;
            MySession.Appointmentbooking.Notes = mess;
            MySession.Appointmentbooking.ContactType = (int)contactType;
            MySession.Appointmentbooking.Price = dbprice;
            MySession.Appointmentbooking.Duration = duration;
            MySession.Appointmentbooking.AppointmentStatusID = (int)Types.AppointmentStatus.Confirmed;
            MySession.Appointmentbooking.CalendarId = MySession.CalendarId;

            SavePaymentInfo(MySession.Appointmentbooking);

            return Json("true", JsonRequestBehavior.AllowGet);

        }

        #endregion pay appointment by package

        #region giftCard

        public ActionResult BuyGiftCard(int id, string amount)
        {
            NameValueCollection parameters = new NameValueCollection();
            ReceiverList receiverList = new ReceiverList();
            receiverList.receiver = new List<Receiver>();
            Receiver rec = new Receiver(Decimal.Parse(amount));
            rec.email = ConfigurationManager.AppSettings["Email_Receive3"];
            receiverList.receiver.Add(rec);
            string cancelUrl = string.Format("{0}GiftCard/GetPaymentFail", EmailHelper.GetStoreHost());
            string currencyCode = ConfigManager.currencyCode;
            string returnUrl = string.Format("{0}GiftCard/GetPaymentSuccess/{1}", EmailHelper.GetStoreHost(), id);
            parameters["actionType"] = "PAY";
            parameters["returnUrl"] = returnUrl;
            parameters["currencyCode"] = currencyCode;
            parameters["cancelUrl"] = cancelUrl;

            PayRequest req = new PayRequest(new RequestEnvelope("en_US"), parameters["actionType"],
                                            parameters["cancelUrl"], parameters["currencyCode"],
                                            receiverList, parameters["returnUrl"]);
            req.feesPayer = ConfigurationManager.AppSettings["FeesPayer"];

            AdaptivePaymentsService service = new AdaptivePaymentsService();
            PayResponse resp = null;
            try
            {
                resp = service.Pay(req);
                LogHelper.Info(string.Format("---BUYGIFTCARD_REQUEST_{0}", service.getLastRequest()));
                LogHelper.Info(string.Format("---BUYGIFTCARD_RESPONSE_{0}", service.getLastResponse()));
            }
            catch (System.Exception ex)
            {
                var err = resp.error.Aggregate("Error:", (current, er) => current + (er.message.ToString() + ","));
                LogHelper.Error(string.Format("--BUYGIFTCARD_PAY_{0}", err));
            }

            // Display response values. 
            Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
            string redirectUrl = null;
            if (!(resp.responseEnvelope.ack == AckCode.FAILURE) &&
                !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
            {
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"]
                                     + "_ap-payment&paykey=" + resp.payKey;
                var giftCard = _giftCardServices.GetGiftCardById(id);
                giftCard.PayKey = resp.payKey;
                _giftCardServices.UpdateGiftCardInfo(giftCard);
                Session["isActive"] = null;
                return Json(new { url = redirectUrl, isSuccess = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { url = returnUrl, isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTransactionId(int idGiftCard)
        {
            var url = Url.Action("GiftCardPurchased", "GiftCard");
            var giftCard = _giftCardServices.GetGiftCardById(idGiftCard);
            giftCard.IsActivated = true;
            giftCard.TransactionID = GetPaymenetTransactionId(giftCard.PayKey);
            _giftCardServices.UpdateGiftCardInfo(giftCard);
            var sess = Session["isActive"];
            Session["isActive"] = sess == null;
            var idEncrypt = SecurityHelper.EncryptStringToBytesAes(giftCard.Id.ToString());
            return RedirectToAction("GiftCardPurchased", "GiftCard", new { reval = idEncrypt });

            //return Json(new { url = returnUrl, isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Handle PaymentDetails API call
        /// </summary>
        /// <param name="context"></param>
        private string GetPaymenetTransactionId(string paymentKey)
        {
            NameValueCollection parameters = new NameValueCollection();
            PaymentDetailsRequest req = new PaymentDetailsRequest(new RequestEnvelope("en_US"));
            parameters["payKey"] = paymentKey;
            if (parameters["payKey"] != "")
                req.payKey = parameters["payKey"];

            // All set. Fire the request            
            AdaptivePaymentsService service = new AdaptivePaymentsService();
            PaymentDetailsResponse resp = null;
            try
            {
                resp = service.PaymentDetails(req);

            }
            catch (System.Exception e)
            {
            }

            // Display response values. 
            Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
            string redirectUrl = null;
            if (!(resp.responseEnvelope.ack == AckCode.FAILURE) &&
                !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
            {
                var paymentInfo = resp.paymentInfoList.paymentInfo;
                return paymentInfo[0].senderTransactionId;
            }
            return string.Empty;
        }


        #endregion

    }
}
