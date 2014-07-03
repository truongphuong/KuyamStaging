// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Windows.Forms;

namespace M2.Util
{
	public static class ClipboardExt
	{
		public static void SendToClipboard(string text)
		{
			Clipboard.SetDataObject(text);
		}

        public static string GetClipboardText()
        {
            return Clipboard.GetText();
        }
	}
}
