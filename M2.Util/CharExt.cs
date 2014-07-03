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
    public static class CharExt
    {
        public static bool IsNumeric(this char c)
        {
            byte by = 0;
            return (Byte.TryParse(c.ToString(), out by));
        }

        public static byte ToByte(this char c)
        {
            byte by = 0;
            if (Byte.TryParse(c.ToString(), out by))
                return by;
            else
                return 0;
        }

        public static char ToUpper(this char c)
        {
            return c.ToString().ToUpper()[0];
        }

        public static char ToLower(this char c)
        {
            return c.ToString().ToLower()[0];
        }

        public static string Repeat(this char c, int count)
        {
            StringBuilder sb = new StringBuilder(count);
            for (int ix = 0; ix < count; ix++)
                sb.Append(c);
            return sb.ToString();
        }

        public static bool IsWhitespace(this char c)
        {
            return Char.IsWhiteSpace(c);
        }

        public static bool IsUpper(this char c)
        {
            return Char.IsUpper(c);
        }

        public static bool IsLower(this char c)
        {
            return Char.IsLower(c);
        }

        public static bool IsUpperLetter(this char c)
        {
            return Char.IsUpper(c) && Char.IsLetter(c);
        }

        public static bool IsLowerLetter(this char c)
        {
            return Char.IsLower(c) && Char.IsLetter(c);
        }

        public static bool IsLetter(this char c)
        {
            return Char.IsLetter(c);
        }


        //public static bool IsWhiteSpace(this char c)
        //{
        //    if (IsLatin1(c))
        //    {
        //        return IsWhiteSpaceLatin1(c);
        //    }
        //    return CharUnicodeInfo.IsWhiteSpace(c);
        //}

        //private static bool IsLatin1(this char ch)
        //{
        //    return (ch <= '\x00ff');
        //}

        //private static bool IsWhiteSpaceLatin1(this char c)
        //{
        //    if (((c != ' ') && ((c < '\t') || (c > '\r'))) && ((c != '\x00a0') && (c != '\x0085')))
        //    {
        //        return false;
        //    }
        //    return true;
        //}    
    
    }
}
