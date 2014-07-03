using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using M2.Util;

namespace Kuyam.Database
{
    public class AppSettings
    {
        public class EmailData
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public bool SSL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FromName { get; set; }
            public string FromEmail { get; set; }
            public string KuyamFeedbackEmail { get; set; }
            public string KuyamApptNotificationEmail { get; set; }
            public string KuyamErrorEmail { get; set; }

        }

        public class AdminData
        {
            public bool EnableEmailBcc { get; set; }
            public bool EnablePhoneBcc { get; set; }
            public string PhoneNumber { get; set; }
            public string EmailBcc { get; set; }
        }

        public class PaySettingData
        {
            public decimal PercentKuyamFee { get; set; }
            public decimal AppointmentAdditionalFee { get; set; }
            public decimal PercentPaymentFee { get; set; }
            public decimal TransactionAdditionalFee { get; set; }
            public string PaypalAccount { get; set; }
            public bool SkipRegularFee { get; set; }
        }

        public class MetaTagSetting
        {
            public string HomeDescription { get; set; }
            public string SearchDescription { get; set; }
            public string Keywords { get; set; }
        }
        //----------------------------------------------

        public EmailData Email { get; set; }
        public AdminData Admin { get; set; }
        public PaySettingData PaySetting { get; set; }
        public MetaTagSetting TagSetting { get; set; }
        public int MaxAppointmentsPerDayInMonthCalendar { get; set; }
        public int MaxMonthCalendarTitleLength { get; set; }
        public bool UseSSL { get; set; }  // from web.config

        //------------------------------------------------

        public AppSettings()
        {
            Email = new EmailData();
            Admin = new AdminData();
            PaySetting = new PaySettingData();
            TagSetting = new MetaTagSetting();
            Load();
        }


        public void Load()
        {
            Dictionary<string, string> settings = DAL.GetSettings();
            Email.Host = settings["Email.Host"];
            Email.Port = settings["Email.Port"].ToInt32();
            Email.SSL = settings["Email.EnableSSL"].ToBool();
            Email.Username = settings["Email.Username"];
            Email.Password = settings["Email.Password"];
            Email.FromEmail = settings["Email.FromEmail"];
            Email.FromName = settings["Email.FromName"];
            Email.KuyamFeedbackEmail = settings["Email.KuyamFeedbackEmail"];
            Email.KuyamApptNotificationEmail = settings["Email.KuyamNotificationEmail"];
            Email.KuyamErrorEmail = settings["Email.KuyamErrorEmail"];
            MaxAppointmentsPerDayInMonthCalendar = settings["MaxAppointmentsPerDayInMonthCalendar"].ToInt32();
            MaxMonthCalendarTitleLength = settings["MaxMonthCalendarTitleLength"].ToInt32();
            Admin.EnableEmailBcc = settings["Admin.EnableEmailBcc"].ToBool();
            Admin.EnablePhoneBcc = settings["Admin.EnablePhoneBcc"].ToBool();
            Admin.EmailBcc = settings["Admin.EmailBcc"];
            Admin.PhoneNumber = settings["Admin.PhoneNumber"];

            PaySetting.PercentKuyamFee = decimal.Parse(settings["Pay.PercentKuyamFee"]);
            PaySetting.AppointmentAdditionalFee = decimal.Parse(settings["Pay.AppointmentAdditionalFee"]);
            PaySetting.PercentPaymentFee = decimal.Parse(settings["Pay.PercentPaymentFee"]);
            PaySetting.TransactionAdditionalFee = decimal.Parse(settings["Pay.TransactionAdditionalFee"]);
            PaySetting.SkipRegularFee = settings["Pay.SkipRegularFee"].ToBool();
            PaySetting.PaypalAccount = settings["Pay.PaypalAccount"];

            TagSetting.HomeDescription = settings["SEO.HomeDescription"];
            TagSetting.SearchDescription = settings["SEO.SearchDescription"];
            TagSetting.Keywords = settings["SEO.Keywords"];
        }

        public void SaveAdminSetting(AdminData data)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["Admin.EnableEmailBcc"] = data.EnableEmailBcc.ToString();
            settings["Admin.EnablePhoneBcc"] = data.EnablePhoneBcc.ToString();
            settings["Admin.EmailBcc"] = data.EmailBcc.ToString();
            settings["Admin.PhoneNumber"] = data.PhoneNumber.ToString();
            DAL.SaveSettings(settings);
        }

        public void SavePaySetting(PaySettingData data)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["Pay.PercentKuyamFee"] = data.PercentKuyamFee.ToString();
            settings["Pay.AppointmentAdditionalFee"] = data.TransactionAdditionalFee.ToString();
            settings["Pay.PercentPaymentFee"] = data.PercentPaymentFee.ToString();
            settings["Pay.TransactionAdditionalFee"] = data.TransactionAdditionalFee.ToString();
            settings["Pay.SkipRegularFee"] = data.SkipRegularFee.ToString();
            settings["Pay.PaypalAccount"] = data.PaypalAccount;
            DAL.SaveSettings(settings);
        }

        public void Save()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["Email.Host"] = Email.Host;
            settings["Email.Port"] = Email.Port.ToString();
            settings["Email.EnableSSL"] = Email.SSL.ToString();
            settings["Email.Username"] = Email.Username;
            settings["Email.Password"] = Email.Password;
            settings["Email.FromEmail"] = Email.FromEmail;
            settings["Email.FromName"] = Email.FromName;
            settings["Email.KuyamFeedbackEmail"] = Email.KuyamFeedbackEmail;
            settings["Email.KuyamNotificationEmail"] = Email.KuyamApptNotificationEmail;
            settings["Email.KuyamErrorEmail"] = Email.KuyamErrorEmail;
            settings["MaxAppointmentsPerDayInMonthCalendar"] = MaxAppointmentsPerDayInMonthCalendar.ToString();
            settings["MaxMonthCalendarTitleLength"] = MaxMonthCalendarTitleLength.ToString();
            DAL.SaveSettings(settings);
        }
    }
}
