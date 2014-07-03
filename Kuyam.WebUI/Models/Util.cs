using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using Kuyam.Database;
using System.Text;
using M2.Util;
using System.Web.Mvc;
using Kuyam.Domain;

namespace Kuyam.WebUI.Models
{
	public static class Util
	{
		public static MvcHtmlString FormatDebugValues(string title, params object[] val)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("<span class=\"title\">{0}:</span> ", title);
			for (int ix = 0; ix < val.Count(); ix += 2 )
				sb.AppendFormat("<span class=\"caption\">{0}:</span><span class=\"value\">{1}</span> ", val[ix], val[ix + 1]);
			//sb.Remove(sb.Length - 2, 2);
			sb.Append("<br />");
			return MvcHtmlString.Create(sb.ToString());
		}

		public static string GetCustFullName(IPrincipal user)
		{
			object o = HttpContext.Current.Session["custfullname"];
			string ret = o == null ? null : o.ToString();
			if (ret == null)
			{
				ret = MySession.Cust.FullName;
				HttpContext.Current.Session["custfullname"] = ret;
			}

			return ret;
		}

		public static MvcHtmlString GetDisplayableRegularHours(Profile profile)
		{
			List<ProfileHour> hours = profile.ProfileHours.ToList();
			StringBuilder sb = new StringBuilder();
			int lastDay = -1;
			foreach (ProfileHour h in hours)
			{
				if (lastDay != h.Day)
				{
					sb.AppendFormat("<br>{0} {1}-{2}", Types.GetTypeName(Types.TypeGroup.Day, h.Day), h.Start.ToHHMMString(), h.End.ToHHMMString());
					lastDay = h.Day;
				}
				else
				{
					sb.AppendFormat(", {0}-{1}", h.Start.ToHHMMString(), h.End.ToHHMMString());
				}
			}

			if (sb.Length > 4)
				sb = sb.Remove(0, 4);

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString GetDisplayableExceptionHours(Profile profile)
		{
			List<ProfileHoursException> hours = profile.ProfileHoursExceptions.ToList();
			StringBuilder sb = new StringBuilder();

			foreach (ProfileHoursException h in hours)
			{
				sb.AppendFormat("{0} {1}<br />", DateTimeHelper.FormatHoursEasy(h.Start, h.End), h.IsOpen ? "open" : "closed");
			}

			return MvcHtmlString.Create(sb.ToString());
		}

		public static List<SelectListItem> GetSelectList(TimeLine.Intervals intervals)
		{
			List<SelectListItem> items = new List<SelectListItem>();
			{
				foreach (TimeLine.Interval i in intervals)
					items.Add(new SelectListItem
					{
						Text = (String.Format("{0}: {1} - {2}", i.Start.ToShortDateString(), i.Start.ToShortTimeString(), i.End.ToShortTimeString())),
						Value = (String.Format("{0},{1},{2}", i.Start.ToShortDateString(), i.Start.ToShortTimeString(), i.End.ToShortTimeString()))
					});
			}

			return items;
		}

		public static MvcHtmlString GetDropdownList(TimeLine.Intervals intervals, DateTime targetDate)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("<select custid=\"TimeSlot\" name=\"TimeSlot\">");

			foreach (TimeLine.Interval i in intervals)
				sb.AppendFormat("<option value='{3}' {4}>{0}, {1} - {2}</option>\r\n",
					i.Start.ToString("dddd, MM/dd"), i.Start.ToShortTimeString(), i.End.ToShortTimeString(),
					String.Format("{0} {1},{2}", i.Start.ToShortDateString(), i.Start.ToShortTimeString(), i.End.Subtract(i.Start).TotalMinutes),
					(i.Start.Date == targetDate.Date ? "style=\"background-hslcolor:#99ff33;\"" : "style=\"background-hslcolor:#E0E0E0 ;\"")
				);

			sb.AppendLine("</select>");

			return MvcHtmlString.Create(sb.ToString());
		}

		public static int GetUnviewedNotificationCount()
		{
			return MySession.Cust.GetUnviewedNotificationCount();
		}

	}
}