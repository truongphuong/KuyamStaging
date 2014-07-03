// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;

using System.Drawing;

namespace M2.Util
{
	public static class ColorExt
	{
		public static Color GetIdealTextColor(this System.Drawing.Color c)
		{
			int nThreshold = 115;
			int bgDelta = Convert.ToInt32((c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114));
			Color foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
			return foreColor;
		}
	}
}
