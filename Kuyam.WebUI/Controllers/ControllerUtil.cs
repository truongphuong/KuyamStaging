using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using M2.Util;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text;
using Kuyam.WebUI.Models;
using System.Web.Mvc;
using System.Drawing;
using System.Web.Security;
using Kuyam.Domain;
using Kuyam.WebUI.Helpers;

namespace Kuyam.WebUI.Controllers
{
	public static class ControllerUtil
	{
		public static void SetFCColors(M2.Util.FullCalendarHelper.Event e, int apptStatus)
		{
			e.textColor = "black";
			switch (apptStatus)
			{
				case 113:
				case 112:
					e.borderColor = "green";
					//e.color = "#A3FFA3";
					break;

				case 111:
					e.borderColor = "#ffcc00";
					//e.color = "#fffbb2";
					break;

				case 114:
					e.borderColor = "red";
					//e.color = "#ffb5b5";
					break;
			}
		}

		public static void TraceEntityValidationException(DbEntityValidationException ex)
		{
			Trace.TraceInformation(GetEntityValidations(ex));
		}

		public static string GetEntityValidations(DbEntityValidationException ex)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var validationErrors in ex.EntityValidationErrors)
			{
				foreach (var validationError in validationErrors.ValidationErrors)
				{
					sb.AppendFormat("Property {0}: {1}\r\n", validationError.PropertyName, validationError.ErrorMessage);
				}
			}

			return sb.ToString();
		}
        
		public static List<SelectListItem> GetCalendarColors(string defaultHex)
		{
			List<SelectListItem> ret = new List<SelectListItem>();
			HSLColor hslcolor = new HSLColor();
			string hex = null;
			foreach (int hue in Enum.GetValues(typeof(HSLColor.StandardColorHues)))
			{
				hslcolor.SetPastel(hue);
				hex = hslcolor.ToHexString();
				ret.Add(new SelectListItem { Value = hex, Text = hex });
			}
			ret.Add(new SelectListItem() { Value = "ffffff", Text = "ffffff" });
			SelectListItem item = ret.Where(x => x.Value == defaultHex).FirstOrDefault();
			if (item != null)
				item.Selected = true;

			return ret;
		}

		public static string ResetPassword(string username, bool notify=true)
		{
			MembershipUser user = Membership.GetUser(username);
			user.UnlockUser();
			string newPass = user.ResetPassword();
			//AccountHelper.ForcePasswordChange(username, true);
            //if (notify)
            //    EmailHelper.SendEmailAdminResetPassword("", newPass);
				//Notifier.SendEmail(username, "Password Reset", "Your password has been reset to:\r\n\r\n" + newPass + "\r\n\r\nPlease login to www.Kuyam.com with this password and then immediately change it.");
			return newPass;
		}

		public static void ChangePassword(string username, string newPassword)
		{
			string tmpPass = ResetPassword(username, false);
			MembershipUser user = Membership.GetUser(username);
			user.ChangePassword(tmpPass, newPassword);
		}

	}
}