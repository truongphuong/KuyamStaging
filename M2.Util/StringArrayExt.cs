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
    public static class StringArrayExt
    {
        public static int FindFirstInt(this string[] tok)
        {
            return FindFirstInt(tok, 0);
        }

        public static int FindFirstInt(this string[] tok, int startIndex)
        {
            Int32 iy = 0;
            for (int ix = startIndex; ix < tok.Length; ix++)
            {
                if (Int32.TryParse(tok[ix], out iy))
                    return ix;
            }

            return -1;
        }

        public static int LongestLength(this string[] tok)
        {
            int len = tok.Max(s => s.Length);
            return len;
        }

        public static string[] Unquote(this string[] toks)
        {
            string[] ret = new string[toks.Length];
            for (int ix = 0; ix < toks.Length; ix++)
            {
                ret[ix] = toks[ix].Unquote();
            }

            return ret;
        }

        public static string[] Trim(this string[] ary, int start, int end)
        {
            return ary.TakeWhile((str, index) => index >= start && index <= end).ToArray();
        }

        public static string Join(this string[] ary, string delim)
        {
            return String.Join(delim, ary);
        }

        // Returns the query variable, not query results!
        public static List<string> Sort(this string[] lines, int col, char delim)
        {
            // Split the string and sort on field[num]
            var qry = from line in lines
                      let fields = line.Split(delim)
                      orderby fields[col] descending
                      select line;

            return qry.ToList();
        }
    }
}
