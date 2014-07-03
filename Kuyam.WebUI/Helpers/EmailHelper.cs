using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Utility;
using RestSharp;
using Kuyam.WebUI.Models;
using M2.Util;
using System.Web.Security;
using Kuyam.Domain;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Controllers;

namespace Kuyam.WebUI.Helpers
{
    public static class EmailHelper
    {
        public const string NewLine = "<br/>";
        public static string EmailSystem
        {
            get
            {
                return string.Empty;
            }
        }

        public static string NameSystem
        {
            get
            {
                return "Kuyam team";
            }
        }

        public static string GetStoreHost(bool useSsl = false)
        {
            useSsl = (ConfigurationManager.AppSettings["UseSSL"].ToBool() == true);
            bool isMobile = (ConfigurationManager.AppSettings["IsMobile"].ToBool() == true);
            string result = "http://" + ServerVariables("HTTP_HOST");
            if (!result.EndsWith("/"))
                result += "/";
            if (useSsl)
            {
                result = result.Replace("http:/", "https:/");
            }

            if (!result.EndsWith("/"))
                result += "/";
            if (isMobile)
                return ConfigurationManager.AppSettings["WebHost"].ToString();
            return result.ToLowerInvariant();
        }

        public static string EmailAdmin = ConfigurationManager.AppSettings["AdminEmail"];

        public static string ServerVariables(string name)
        {
            string tmpS = string.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables[name] != null)
                {
                    tmpS = HttpContext.Current.Request.ServerVariables[name];
                }
            }
            catch
            {
                tmpS = string.Empty;
            }
            return tmpS;
        }

        public static bool SslEnabled()
        {
            bool useSsl = !String.IsNullOrEmpty(ConfigurationManager.AppSettings["UseSSL"]) &&
                Convert.ToBoolean(ConfigurationManager.AppSettings["UseSSL"]);
            return useSsl;
        }

        public static void SendMobileVeryficationCode(string inviteCode)
        {
            string subject = "SMS Verification code";
            string body = "";
            body = NewLine + "Thank you for signing up to join kuyam." + NewLine;
            body += NewLine + "Invite Code (for testing only): " + inviteCode;
            body += NewLine + NewLine + " Thanks!" + NewLine;
            body += "Kuyam team";

            SendMail(EmailSystem, EmailAdmin, subject, body);

        }

        public static void SendSignUpEmail(string emailTo, string body)
        {
            string subject = "Confirm your subscription";
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendThanksSignUpEmail(string emailTo, string body)
        {
            string subject = "Thanks for signing up !";

            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendInfoEmail(string name, string email, string user, string pass)
        {

            /*var mail = new MailMessage();
            string emailSystem = ConfigurationManager.AppSettings["StaffEmail"];
            mail.From = new MailAddress(emailSystem);
            mail.Subject = "Confirm your subscription";
            mail.Body = "<br/>" + "Thank you for joining kuyam. <br/>";
            mail.Body += "Name: " + name;
            mail.Body += "<br/> Email: " + email;
            mail.Body += "<br/> User name : " + user;
            mail.Body += "<br/> Password Code : " + pass;
            mail.Body += "<br/><br/> Thanks! <br/>";
            mail.Body += "Kuyam team";
            mail.IsBodyHtml = true;
            mail.To.Add(new MailAddress(EmailAdmin));
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = ConfigurationManager.AppSettings["Smtp.Host"];
            smtpClient.Port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
            smtpClient.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Smtp.UserName"],
                                                           ConfigurationManager.AppSettings["Smtp.Password"]);
            smtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["Smtp.UseSSL"]);
            smtpClient.Send(mail);*/

            string subject = "Confirm your subscription";
            string content = NewLine + "Thank you for joining kuyam." + NewLine;
            content += "Name: " + name;
            content += NewLine + " Email: " + email;
            content += NewLine + "User name : " + user;
            content += NewLine + "Password Code : " + pass;
            content += NewLine + "Thanks! " + NewLine;
            content += "Kuyam team";

            SendMail(EmailSystem, email, subject, content);

        }

        public static void SendSheduleEmail(string url, string email)
        {
            SendMail(EmailSystem, EmailAdmin, "SendSheduleEmail - Confirmation Email", "Confirmation Email");
        }

        public static void SendCompanySetupInviteCode(string invite, string email, string companyName, string url)
        {

            StringBuilder body = new StringBuilder();
            body.AppendFormat("{0}" + NewLine, "Confirm your VerificationCode");
            body.AppendFormat(NewLine + "{0}", "Thank you! we’ll be in touch.");
            body.AppendFormat(NewLine + "Company name:{0}", companyName);
            body.AppendFormat(NewLine + "Verification Code:{0}", invite);
            body.AppendFormat(NewLine + "Your company active go to {0}{1}", EmailHelper.GetStoreHost(), "Company/VerificationCode/");
            body.AppendFormat("{0}" + NewLine + NewLine, "Thanks!");
            body.AppendFormat("{0}" + NewLine, "Kuyam team");
            string subject = "Company setup verification code";
            SendMail(email, EmailAdmin, subject, body.ToString());
        }
        public static void SendCompanyEditNewEmail(string newEmail, string companyName)
        {

            StringBuilder body = new StringBuilder();
            body.AppendFormat("{0}" + NewLine, "Company edit: email changed");
            body.AppendFormat(NewLine + "{0}", "Thank you! we’ll be in touch.");
            body.AppendFormat(NewLine + "Company name:{0}", companyName);
            body.AppendFormat(NewLine + "Your new email:{0}", newEmail);
            body.AppendFormat("{0}" + NewLine + NewLine, "Thanks!");
            body.AppendFormat("{0}" + NewLine, "Kuyam team");
            string subject = "Company setup verification code";
            SendMail(EmailSystem, EmailAdmin, subject, body.ToString());
        }


        public static void SendContactEmail(string name, string email, string subject, string message)
        {
            SendMail(email, EmailAdmin, subject, message);
        }
        public static IRestResponse SendMail(string from, string to, string subject, string content)
        {
            string[] toEmails = to.Split(';');
            RestClient client = new RestClient();
            client.BaseUrl = "https://api.mailgun.net/v2";
            client.Authenticator = new HttpBasicAuthenticator("api", ConfigurationManager.AppSettings["MAILGUN_API_KEY"]);
            RestRequest request = new RestRequest();
            request.AddParameter("domain", ConfigurationManager.AppSettings["MAILGUN_DOMAIN"], ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", ConfigurationManager.AppSettings["MAILGUN_MAIL_FROM"]);

            string[] emailBccs = MyApp.Settings.Admin.EmailBcc.Split(';');
            bool hasEmailTo = false;
            foreach (string toEmail in toEmails)
            {
                if (!string.IsNullOrEmpty(toEmail.Trim()))
                {
                    var role = Roles.GetRolesForUser(toEmail.Trim());
                    if (role != null && !role.Contains("Guest"))
                    {
                        request.AddParameter("to", toEmail);
                        hasEmailTo = true;
                    }
                }

            }
            //if not exits email to use email default
            if (!hasEmailTo)
                request.AddParameter("to", emailBccs[0]);

            foreach (string email in emailBccs)
            {
                if (!string.IsNullOrEmpty(email.Trim()))
                    request.AddParameter("bcc", email);
            }

            request.AddParameter("subject", subject);
            
            request.AddParameter("html", "<html>" + content + "</html>");

            request.Method = Method.POST;

            var result = client.Execute(request);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                LogHelper.Error(result.ErrorMessage);
            }
            return result;
        }

        public static void SendEmailCompanySetupVerificationCode(string emailTo, string body)
        {
            string subject = string.Format("{0}", "Verification Code");
            SendMail(EmailSystem, emailTo, subject, body);
        }
        public static void SendEmailCompany30DayTrialBeginCode(string emailTo, string body)
        {
            string subject = string.Format("{0}", "30 Day Trial Begins");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailCompanyUnderReviewVerificationCode(string emailTo, string body)
        {
            string subject = string.Format("{0}", "Under Review");
            SendMail(EmailSystem, emailTo, subject, body);
        }
        public static void SendEmailUserInfomationChanged(string emailTo, string newEmail)
        {
            StringBuilder body = new StringBuilder();
            body.AppendFormat("{0}<br/>", "Change information");
            body.AppendFormat("<br/>{0}<br/>", "Thank you! your changes are saved");
            body.AppendFormat("<br/>New email :{0}<br/>", newEmail);
            body.AppendFormat("</br></br>{0}<br/><br/><br/>", "Thanks!");
            body.AppendFormat("{0}<br/>", "Kuyam team");

            SendMail(EmailSystem, emailTo, "Change information", body.ToString());
        }
        public static void SendEmailUserInfomationChanged(string emailTo, string newEmail, string newPhone)
        {
            StringBuilder body = new StringBuilder();
            body.AppendFormat("{0}<br/>", "Change information");
            body.AppendFormat("<br/>{0}<br/>", "Thank you! your changes are saved");
            body.AppendFormat("<br/>New email :{0}<br/>", newEmail);
            body.AppendFormat("<br/>New phone :{0}<br/>", newPhone);
            body.AppendFormat("</br></br>{0}<br/><br/><br/>", "Thanks!");
            body.AppendFormat("{0}<br/>", "Kuyam team");

            SendMail(EmailSystem, emailTo, "Change information", body.ToString());
        }
        /// <summary>
        /// This function will send list hour request to company from user by click on 'list your hours' in search page under shecdule button
        /// </summary>
        /// <param name="emailTo">company email</param>
        /// <param name="firstName">name of user</param>
        /// <param name="lastName">name of user</param>
        /// <param name="userName"></param>
        /// <param name="companyName"></param>
        public static void SendEmailCompanyListHourRequest(string emailTo, string firstName, string lastName, string userName, string companyName)
        {
            StringBuilder body = new StringBuilder();
            body.AppendFormat("Hi <br/>");
            body.AppendFormat("Full Name:{0} {1}<br/>", firstName, lastName);
            body.AppendFormat("Email: {0} <br/>", userName);
            body.AppendFormat("Company: {0}", companyName);
            body.AppendFormat("</br></br>{0}<br/><br/>", "Thanks!");
            body.AppendFormat("{0}<br/>", "Kuyam team");

            SendMail(EmailSystem, emailTo, "requestion send company list hours", body.ToString());
        }

        #region Appointent email
        public static void SendEmailAppointmentConfirmedNotification(string serviceProviderName)
        {
            StringBuilder body = new StringBuilder();
            body.AppendFormat("{0}<br/>", "Appointment was confired and payment was done.");
            body.AppendFormat("<br/>{0}<br/>", "Please check your paypal account");
            body.AppendFormat("{0}<br/>", "Kuyam team");

            SendMail(EmailSystem, EmailAdmin, "Appointment was confirmed", body.ToString());
        }
        #endregion

        public static void SendEmailUserAddCompany(string emailTo, string companyName, string contactName, string phoneNumber, string address, string cityState, string zipCode)
        {
            StringBuilder body = new StringBuilder();
            var cust = MySession.Cust;
            string email = string.Empty;
            if (cust != null)
            {
                if (string.IsNullOrEmpty(contactName))
                    contactName = string.Format("{0} {1}.", cust.FirstName, cust.LastName);
                email = cust.Email;
            }
            body.AppendFormat("Hi <br/>");
            body.AppendFormat("User: {0} <br/>", email);
            body.AppendFormat("Company Name:{0} <br/>", companyName);
            body.AppendFormat("Contact Name: {0} <br/>", contactName);
            body.AppendFormat("Phone Number: {0} <br/>", phoneNumber);
            body.AppendFormat("Address: {0} <br/>", address);
            body.AppendFormat("City, State: {0} <br/>", cityState);
            body.AppendFormat("Zip Code: {0} <br/>", zipCode);

            body.AppendFormat("<br/><br/>{0}<br/>", "Thanks!");
            body.AppendFormat("{0}<br/>", "Kuyam team");

            SendMail(EmailSystem, emailTo, "request to add a company", body.ToString());
        }


        public static void SendEmailImpersonate(string emailTo, string emailLogin, string htmlContent)
        {
            SendMail(EmailSystem, emailTo, "Impersonate", htmlContent);
        }

        public static void SendEmailAdminResetPassword(string emailTo, string body)
        {

            SendMail(EmailSystem, emailTo, "Reset Password", body);
        }

        public static void SendEmailConfirmAppointment(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Confirm Appointment");
            SendMail(emailFrom, emailTo, subject, body);
        }

        public static void SendEmailCancelAppointment(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Cancel Appointment");
            SendMail(emailFrom, emailTo, subject, body);
        }

        public static void SendEmailModificationAppointment(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Modification Appointment");
            SendMail(emailFrom, emailTo, subject, body);
        }

        public static void SendEmailForgotPassword(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Forgot Password");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailUserResetPassword(string emailTo, string body)
        {
            string subject = string.Format("{0}", "Reset Password");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailChangePassword(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Change Password");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailInviteCodeSignUp(string emailTo, string body)
        {
            string subject = string.Format("{0}", "Invite Code");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailNewsletter(string emailTo, string body)
        {
            string subject = string.Format("{0}", "Newsletter");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailRegularClient(string emailFrom, string emailTo, string body)
        {
            string subject = string.Format("{0}", "Invited Regular Client");
            //body = string.Format("This is email sent from the company");
            SendMail(EmailSystem, emailTo, subject, body);
        }

        public static void SendEmailDiscountCode(string emailFrom, string emailTo, string body)
        {

            string subject = string.Format("{0}", "Discount");
            SendMail(emailFrom, emailTo, subject, body);
        }

        public static void SendEmailChangedStatus(string emailFrom, string emailTo, string body, string subject)
        {
            //string subject = string.Format("{0}", "Discount");
            SendMail(emailFrom, emailTo, subject, body);
        }

        public static void SendEmailThanksSignUpFacebook(string emailTo, string body)
        {
            string subject = string.Format("{0}", "welcome to kuyam!");
            SendMail(EmailSystem, emailTo, subject, body);
        }


        /// <summary>
        /// Send the newsletter email to list company
        /// </summary>
        /// <param name="listCompanyProfiles">The list company profiles.</param>
        /// <param name="body">The body.</param>
        /// <param name="subject">The subject.</param>
        public static void SendNewsletterEmail(List<ProfileCompany> listCompanyProfiles, string body, string subject)
        {
            foreach (ProfileCompany profileCompany in listCompanyProfiles)
            {
                string content = body.Replace("[ProfileName]", profileCompany.ContactFirstName == null ? string.Empty : profileCompany.ContactFirstName);
                content = content.Replace("[YourEmail]", profileCompany.Email);
                if (!string.IsNullOrEmpty(profileCompany.Email))
                    SendMail(EmailSystem, profileCompany.Email, subject, content);
            }

        }

        /// <summary>
        /// Sends the newsletter email cust.
        /// </summary>
        /// <param name="listCustomers">The list customers.</param>
        /// <param name="body">The body.</param>
        /// <param name="subject">The subject.</param>
        public static void SendNewsletterEmailCust(List<Cust> listCustomers, string body, string subject)
        {
            foreach (Cust cust in listCustomers)
            {
                string content = body.Replace("[ProfileName]", cust.FirstName == null ? string.Empty : cust.FirstName);
                content = content.Replace("[YourEmail]", cust.Email);
                if (!string.IsNullOrEmpty(cust.Email))
                    SendMail(EmailSystem, cust.Email, subject, content);
            }

        }

        public static void SendAppointmentNotifyToCompanyOrAdmin(Appointment appoinment, Types.NotifyType type, Controller controller)
        {
            var visitName = appoinment.Cust.FullName;
            var visitEmail = appoinment.Cust.Email;
            var profileCompany = appoinment.GetProfileCompany();
            dynamic firstalertObject = new
            {
                DateNow = String.Format("{0:dddd, MMMM d, yyyy}", DateTimeUltility.ConvertToUserTime(DateTime.Now)),
                UserName = "Company",
                CompanyName = appoinment.ProfileCompany,
                CompanyAddress = string.Format("{0}, {1}, {2}, {3}, {4}", profileCompany.Name, profileCompany.Street1, profileCompany.City, profileCompany.State, profileCompany.Zip),
                UpdateType = type.ToString(),
                Guest = visitName + " " + visitEmail
            }.ToExpando();
            var templateAppointmentReminder = controller.RenderPartialViewToString("AppointmentConciergeEmail", (object)firstalertObject);
            string subject = string.Format("{0}", "Concierge Communications");
            SendMail(EmailSystem, profileCompany.Email, subject, templateAppointmentReminder);
        }


        public static void SendAppointmentNotifyToConcierge(Appointment appoinment, Types.NotifyType type, Controller controller)
        {
            var visitName = appoinment.Cust.FullName;
            var visitEmail = appoinment.Cust.Email;

            var hotelStaffCustomer = appoinment.HotelStaff.Cust;
            var usreName = hotelStaffCustomer.FirstName;
            var conciergetEmail = hotelStaffCustomer.Email;
            var profileCompany = appoinment.GetProfileCompany();
            dynamic firstalertObject = new
            {
                DateNow = String.Format("{0:dddd, MMMM d, yyyy}", DateTimeUltility.ConvertToUserTime(DateTime.Now)),
                UserName = usreName,
                CompanyName = appoinment.ProfileCompany,
                CompanyAddress = string.Format("{0}, {1}, {2}, {3}, {4}", profileCompany.Name, profileCompany.Street1, profileCompany.City, profileCompany.State, profileCompany.Zip),
                UpdateType = type.ToString(),
                Guest = visitName + " " + visitEmail
            }.ToExpando();
            var templateAppointmentReminder = controller.RenderPartialViewToString("AppointmentConciergeEmail", (object)firstalertObject);
            string subject = string.Format("{0}", "Concierge Communications");
            SendMail(EmailSystem, conciergetEmail, subject, templateAppointmentReminder);
        }

        public static string ProposeEmailTemplateForConcierge(ProposedAppointment proposedAppointment, Controller controller)
        {
            var visitName = proposedAppointment.Cust.FullName;
            var visitEmail = proposedAppointment.Cust.Email;
            var userName = proposedAppointment.HotelStaff != null &&
                proposedAppointment.HotelStaff.Cust != null ? proposedAppointment.HotelStaff.Cust.FirstName : string.Empty;
            var profileCompany = proposedAppointment.ProfileCompany;

            dynamic firstalertObject = new
            {
                DateNow = String.Format("{0:dddd, MMMM d, yyyy}", DateTimeUltility.ConvertToUserTime(DateTime.Now)),
                UserName = userName,
                CompanyName = profileCompany.Name,
                CompanyAddress = string.Format("{0}, {1}, {2}, {3}, {4}", profileCompany.Name, profileCompany.Street1, profileCompany.City, profileCompany.State, profileCompany.Zip),
                UpdateType = "new proposed appointment",
                Guest = visitName + " " + visitEmail
            }.ToExpando();
            var templateAppointmentReminder = controller.RenderPartialViewToString("AppointmentConciergeEmail", (object)firstalertObject);
            return templateAppointmentReminder;

        }

        //public static IRestResponse SendMail_BK(string from, string to, string subject, string content)
        //{
        //    MailMessage mail = new MailMessage();
        //    if (string.IsNullOrWhiteSpace(from))
        //    {
        //        mail.From = new MailAddress(ConfigurationManager.AppSettings["MAILGUN_MAIL_FROM"]);
        //    }
        //    else
        //    {
        //        mail.From = new MailAddress(from);
        //    }

        //    mail.Subject = subject;
        //    mail.Body = content;
        //    mail.IsBodyHtml = true;
        //    string[] toEmails = to.Split(';');
        //    foreach (string toemail in toEmails)
        //    {
        //        mail.To.Add(new MailAddress(toemail));
        //    }

        //    MyApp.Settings.Load();
        //    if (MyApp.Settings.Admin.EnableEmailBcc)
        //    {
        //        string[] emailBcc = MyApp.Settings.Admin.EmailBcc.Split(';');
        //        foreach (string email in emailBcc)
        //        {
        //            mail.Bcc.Add(new MailAddress(email));
        //        }

        //    }

        //    SmtpClient smtpClient = new SmtpClient();
        //    smtpClient.Host = ConfigurationManager.AppSettings["MAILGUN_SMTP_SERVER"];
        //    smtpClient.Port = int.Parse(ConfigurationManager.AppSettings["MAILGUN_SMTP_PORT"]);
        //    smtpClient.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MAILGUN_SMTP_LOGIN"],
        //                                                   ConfigurationManager.AppSettings["MAILGUN_SMTP_PASSWORD"]);
        //    //smtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["Smtp.UseSSL"]);
        //    mail.Headers.Add("List-Unsubscribe", "<mailto:support@kuyam.com?subject=unsubscribe>");
        //    smtpClient.Send(mail);
        //    return null;

        //    //RestClient client = new RestClient();
        //    //client.BaseUrl = "https://api.mailgun.net/v2";
        //    //client.Authenticator =
        //    //        new HttpBasicAuthenticator("api",
        //    //                                   ConfigurationManager.AppSettings["MAILGUN_API_KEY"]);
        //    //RestRequest request = new RestRequest();
        //    //request.AddParameter("domain",
        //    //                     ConfigurationManager.AppSettings["MAILGUN_DOMAIN"], ParameterType.UrlSegment);
        //    //request.Resource = "{domain}/messages";
        //    //request.AddParameter("from", ConfigurationManager.AppSettings["MAILGUN_SENDER"]);
        //    //string[] toEmails = to.Split(';');
        //    //foreach (string toemail in toEmails)
        //    //{
        //    //    request.AddParameter("to", toemail);
        //    //}

        //    //request.AddParameter("subject", subject);
        //    //request.AddParameter("text", content);
        //    //request.Method = Method.POST;
        //    //return client.Execute(request);
        //}
    }
}
