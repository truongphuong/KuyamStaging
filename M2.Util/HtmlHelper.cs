// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
    public static class HtmlHelper
    {
        public static string GetTagContent(string src, string tagName)
        {
            string ret = null;

            int start = src.IndexOf("<" + tagName + ">", StringComparison.CurrentCultureIgnoreCase);
            if (start >= 0)
            {
                int end = src.IndexOf("</" + tagName + ">", start + 1, StringComparison.CurrentCultureIgnoreCase);
                if (end > start)
                {
                    ret = src.Substring(start + tagName.Length + 2, end - (start + tagName.Length + 2));
                }
            }

            return ret;
        }

        public static string ReplaceTagContent(string src, string tagName, string newContent)
        {
            string ret = src;

            int start = src.IndexOf("<" + tagName + ">", StringComparison.CurrentCultureIgnoreCase);
            if (start >= 0)
            {
                int end = src.IndexOf("</" + tagName + ">", start + 1, StringComparison.CurrentCultureIgnoreCase);
                if (end > start)
                {
                    ret = src.Substring(0, start+tagName.Length + 2) + newContent + src.Substring(end);
                }
            }

            return ret;
        }
    }
}
