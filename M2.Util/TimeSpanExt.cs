// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

// Portions based on http://stackoverflow.com/questions/842057/how-do-i-convert-a-timespan-to-a-formatted-string

using System;
using System.Collections.Generic;

using System.Text;

namespace M2.Util
{
	public static class TimeSpanExt
	{
		public static void Clear(this TimeSpan ts)
		{
			ts = new TimeSpan();
		}

		public static string ToAgeString(this TimeSpan span)
		{
			return string.Format("{0:0}", span.Duration().Days / 365.25);
		}

		public static string ToHHMMString(this TimeSpan span, bool showZeroMin=false, bool hours24=false)
		{
			string ampm = null;
			int hours = span.Duration().Hours;
			string hoursFormat = "{0:00}";

			if (!hours24)
			{
				if (span.Duration().Hours > 12)
				{
					hours -= 12;
				}

				if (span.Duration().Hours >= 12)
				{
					ampm = "PM";
				}
				else
				{
					ampm = "AM";
				}

				hoursFormat = "{0:0}";
			}

			string formatted = string.Format("{0}{1}{2}",
				string.Format(hoursFormat, hours),
				(!showZeroMin && span.Duration().Minutes == 0 ? null : string.Format(":{0:00}", span.Duration().Minutes)),
				ampm);

			return formatted;
		}

		public static string ToShortTimeString(this TimeSpan span)
		{
			string formatted = string.Format("{0}{1}{2}{3}",
				string.Format("{0:00}.", span.Duration().Days),
				string.Format("{0:00}:", span.Duration().Hours),
				string.Format("{0:00}:", span.Duration().Minutes),
				string.Format("{0:00}", span.Duration().Seconds));

			return formatted;
		}

		public static string ToVerboseTimeString(this TimeSpan span)
		{
			string formatted = string.Format("{0}{1}{2}{3}",
				span.Duration().Days > 0 ? string.Format("{0:0} days, ", span.Duration().Days) : string.Empty,
				span.Duration().Hours > 0 ? string.Format("{0:0} hours, ", span.Duration().Hours) : string.Empty,
				span.Duration().Minutes > 0 ? string.Format("{0:0} minutes, ", span.Duration().Minutes) : string.Empty,
				span.Duration().Seconds > 0 ? string.Format("{0:0} seconds", span.Duration().Seconds) : string.Empty);

			if (formatted.EndsWith(", ")) 
				formatted = formatted.Substring(0, formatted.Length - 2);

			return formatted;
		}
	}
}
