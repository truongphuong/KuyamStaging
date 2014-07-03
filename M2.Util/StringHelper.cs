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
    public static class StringHelper
    {
        public static string MakeRuler(int len, int padding)
        {
            int iy;
            string s;
            len = len + 2;
            StringBuilder sb = new StringBuilder((len + padding) * 3);
            sb.Fill(' ', ((len + padding) * 3) - 2);
            for (int ix = 0; ix < len - 2; ix++)
            {
                s = ix.ToString();
                iy = s.Length;
                switch (iy)
                {
                    case 3:
                        sb[padding + ix] = s[0];
                        sb[padding + padding + len + ix] = s[1];
                        sb[padding + padding + padding + (len * 2) + ix] = s[2];
                        break;

                    case 2:
                        sb[padding + padding + len + ix] = s[0];
                        sb[padding + padding + padding + (len * 2) + ix] = s[1];
                        break;

                    case 1:
                        sb[padding + padding + padding + (len * 2) + ix] = s[0];
                        break;
                }
            }
            sb[padding + len - 2] = sb[padding + padding + (len * 2) - 2] = '\r';
            sb[padding + len - 1] = sb[padding + padding + (len * 2) - 1] = '\n';

            sb.Append("\r\n    " + "_".Repeat(len-2));

            return sb.ToString();
        }

        public static string MakeRuler(string text, int len, int padding)
        {
            string ruler = MakeRuler(len, padding);
            return MakeRuler(text, ruler);
        }
            
        public static string MakeRuler(string text, string ruler)
        {
            if (text.IsNullOrEmpty())
                return null;

            string[] tok = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            int len = tok.LongestLength();
            int count = tok.Count();

            // header
            StringBuilder sb = new StringBuilder((len*count)+ruler.Length);
            sb.Append(ruler);
            sb.Append("\r\n");
            for (int ix=0; ix < count; ix++)
                sb.AppendFormat("{0:000}|{1}\r\n", ix, tok[ix]);
            
            return sb.ToString();
        }
    }
}
