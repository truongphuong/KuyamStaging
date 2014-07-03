using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.WebUI.Models;

namespace Kuyam.Domain
{
    public static class Extensions
    {

        public static string TimeAgo(this DateTime date)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(date);
            if (timeSince.TotalMilliseconds < 1) return "not yet";
            if (timeSince.TotalMinutes < 1) return "just now";
            if (timeSince.TotalMinutes < 2) return "1 minute ago";
            if (timeSince.TotalMinutes < 60) return string.Format("{0} minutes ago", timeSince.Minutes);
            if (timeSince.TotalMinutes < 120) return "1 hour ago";
            if (timeSince.TotalHours < 24) return string.Format("{0} hours ago", timeSince.Hours);
            if (timeSince.TotalDays < 2) return "yesterday";
            if (timeSince.TotalDays < 7) return string.Format("{0} days ago", timeSince.Days);
            if (timeSince.TotalDays < 14) return "last week";
            if (timeSince.TotalDays < 21) return "2 weeks ago";
            if (timeSince.TotalDays < 28) return "3 weeks ago";
            if (timeSince.TotalDays < 60) return "last month";
            if (timeSince.TotalDays < 365) return string.Format("{0} months ago", Math.Round(timeSince.TotalDays / 30));
            if (timeSince.TotalDays < 730) return "last year"; //last but not least...
            return string.Format("{0} years ago", Math.Round(timeSince.TotalDays / 365));
        }


        public static UserEditModel ToEditModel(this Cust cust)
        {
            if (cust == null)
                return null;
            var model = new UserEditModel()
            {
                UserName = cust.Username,
                CustID = cust.CustID,              
                AspUserID = cust.AspUserID,
                Email = cust.aspnet_Users.aspnet_Membership.Email,
                HomePhone = cust.HomePhone,
                MobilePhone = cust.MobilePhone,
                PreferredPhoneTypeID = cust.PreferredPhoneTypeID,
                Modified = cust.Modified,
                Zip = cust.Zip,
                EmailType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Email) != 0),
                TextType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Text) != 0),
                CallType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Call) != 0),
                FirstAlert = cust.FirstAlert,
                SecondAlert = cust.SecondAlert
            };
            return model;
        }

        public static UserModel ToUserModel(this Cust cust)
        {
            if (cust == null)
                return null;
            var model = new UserModel()
            {
                UserName = cust.Username,
                FirstName =cust.FirstName,
                LastName = cust.LastName,
                CustID = cust.CustID,               
                AspUserID = cust.AspUserID,
                FacebookToken = cust.FacebookToken,
                FacebookUserID = cust.FacebookUserID,
                Email = cust.Email,
                HomePhone = cust.HomePhone,
                MobilePhone = cust.MobilePhone,
                PreferredPhoneTypeID = cust.PreferredPhoneTypeID,
                Modified = cust.Modified,
                Zip = cust.Zip,
                EmailType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Email) != 0),
                TextType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Text) != 0),
                CallType = ((cust.PreferredPhoneTypeID.HasValue) && (cust.PreferredPhoneTypeID.Value & (int)Types.PreferredPhone.Call) != 0),
                FirstAlert = cust.FirstAlert,
                SecondAlert = cust.SecondAlert,
                IcalUrl = cust.IcalUrl
            };
            if (!string.IsNullOrEmpty(model.FacebookUserID))
            {
                model.PhotoUrl = string.Format("https://graph.facebook.com/{0}/picture?width=100&height=100", model.FacebookUserID);
            }
            else
            {
                model.PhotoUrl = "/Images/placeholder.png";
            }
            return model;
        }

        public static CompanySettingModel ToCompanySettingModel(this ProfileCompany company)
        {
            if (company == null)
                return null;
            var model = new CompanySettingModel()
            {
                Name = company.Name,
                Email = company.Email,
                Phone = company.Phone,
                PaymentOptions =company.PaymentOptions,
                EmailType = ((company.PreferredContact.HasValue) && (company.PreferredContact.Value & (int)Types.PreferredPhone.Email) != 0),
                TextType = ((company.PreferredContact.HasValue) && (company.PreferredContact.Value & (int)Types.PreferredPhone.Text) != 0),
                FirstAlert = company.FirstAlert,
                SecondAlert = company.SecondAlert,  
                Policy = company.PayAfter,
                PaymentMethod =company.PaymentMethod,
                CancelPolicy = company.CancelPolicy != null ? company.CancelPolicy.Value : 0,
                anytime = (company.CancelHour != null ? company.CancelHour.Value : 0).ToString(CultureInfo.InvariantCulture),
                norefund = (company.CancelRefundPercent != null ? company.CancelRefundPercent.Value : 0).ToString(CultureInfo.InvariantCulture),
                ContactFirstName = company.ContactFirstName,
                ContactLastName = company.ContactLastName
            };
            return model;
        }
        
    }
}
