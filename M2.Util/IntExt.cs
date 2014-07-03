// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC



using System;
using System.Collections.Generic;

using System.Text;

namespace M2.Util
{
	public static class IntExt
	{
		public static int IsNull(this int? i, int value)
		{
			return (i == null ? value : (int)i);
		}

		public static bool ToBool(this int i)
		{
			return (i != 0);
		}

		public static string GetLengthString(this int len)
		{
			if (len < 1024)
				return String.Format("{0} bytes", len);
			else if (len < 1024 * 1024)
				return String.Format("{0:F1}KB", (float)len / 1024.0);
			else if (len < 1024 * 1024 * 1024)
				return String.Format("{0:F1}MB", (float)len / (1024.0 * 1024.0));
			else
				return String.Format("{0:F1}GB", (float)len / (1024.0 * 1024.0 * 1024.0));
		}
	}
}
