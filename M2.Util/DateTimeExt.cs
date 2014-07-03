// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;

using System.Text;

namespace M2.Util
{
	public static class DateTimeExt
	{
		public static int CalcAge(this DateTime dt1, DateTime dt2)
		{
			DateTime d1 = dt1;
			DateTime d2 = dt2;

			switch (dt1.CompareTo(dt2))
			{
				case -1:
					d1 = dt2;
					d2 = dt1;
					break;

				case 0:
					return 0;
			}

			// get the difference in years
			int years = d1.Year - d2.Year;

			// subtract another year if we're before the birth day in the current year
			if (d1.Month < d2.Month ||
				(d1.Month == d2.Month && d1.Day < d2.Day))
				years--;

			if (years < 0)
				years = 0;

			return years;
		}

        public static int CalcAge(this DateTime bday)
        {
            return CalcAge(bday, DateTime.Now, 0);
        }

        public static int CalcAge(this DateTime bday, int offsetDays)
        {
            return CalcAge(bday, DateTime.Now, offsetDays);
        }

        public static int CalcAge(this DateTime bday, DateTime targetDate, int offsetDays)
        {
            DateTime today = targetDate.AddDays(offsetDays);

            // get the difference in years
            int years = today.Year - bday.Year;

            // subtract another year if we're before the birth day in the current year
            if (today.Month < bday.Month ||
                (today.Month == bday.Month && today.Day < bday.Day))
                years--;

            if (years < 0)
                years = 0;

            return years;
        }

        // Format as YYYYMMDD
        public static string ToFileDate(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }

        // Format as YYYYMMDD
        public static string ToFileDateTime(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd-hhmmss");
        }

		// http://www.codeproject.com/KB/cs/timestamp.aspx
		public static double ToUnixDateTime(this DateTime d)
		{
			DateTime baseDT = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan span = d.ToUniversalTime() - baseDT;
			return span.TotalSeconds;
		}

		public static DateTime FromUnixDateTime(this double d)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(d).ToLocalTime();
		}

		public static string ToStringNoSeconds(this DateTime dt)
		{
			return dt.ToString("M/dd/yyyy hh:mm tt");
		}

		public static string ToCasual(this DateTime dt)
		{
			StringBuilder sb = new StringBuilder();
			bool pm = dt.Hour >= 12;

			int hour = dt.Hour;
			if (pm && hour > 12)
			{
				hour -= 12;
			}
			else if (hour == 0)
			{
				hour = 12;
			}
			sb.Append(hour);

			if (dt.Second > 0 || dt.Minute > 0)
			{
				sb.AppendFormat(":{0:00}", dt.Minute);
				if (dt.Second > 0)
					sb.AppendFormat(":{0:00}", dt.Second);
			}
			sb.Append(pm ? "pm" : "am");

			return dt.ToString("ddd MMM dd, yyyy") + " at " + sb.ToString();
		}
	
		public static string ToCasualRange(this DateTime dt, DateTime dtEnd)
		{
			String ret = dt.ToCasual() + " - " + dtEnd.ToCasual();
			return ret;
		}
	}
}
