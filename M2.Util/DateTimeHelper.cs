using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace M2.Util
{
	public static class DateTimeHelper
	{
		public static string FormatHoursEasy(DateTime start, DateTime end)
		{
			string ret = null;

			if (start.Date == end.Date)
			{
				ret = String.Format("{0} {1} - {2}",
				start.ToShortDateString(),
				start.ToShortTimeString(),
				end.ToShortTimeString());
			}
			else
			{
				ret =String.Format("{0} - {1}", start, end);
			}

			return ret;
		}

		public static DateTime? ParseJS(string js)
		{
			// "Fri Nov 18 2011 10:59:03 GMT-0800 (Pacific Standard Time)"
			if (js.IsNullOrEmpty())
				return null;

			return DateTime.ParseExact(js.Substring(0, js.IndexOf("GMT")-1),
                                  "ddd MMM d yyyy HH:mm:ss",
                                  CultureInfo.InvariantCulture);

		}
	}
}
