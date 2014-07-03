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
    public static class StringBuilderExt
    {
        public static void Clear(this StringBuilder sb)
        {
            sb.Remove(0, sb.Length);
        }

        public static void Fill(this StringBuilder sb, char c, int len)
        {
            for (int ix = 0; ix < len; ix++)
                sb.Append(c);
        }
    }
}
