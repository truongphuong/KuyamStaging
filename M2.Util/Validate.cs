// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System.Text.RegularExpressions;

namespace M2.Util
{
    public static class Validate
    {
        public static bool IsSSN(string value)
        {
            Regex myRegex = new Regex("^((?!000)([0-6]\\d{2}|[0-7]{2}[0-2]))-((?!00)\\d{2})-((?!0000)\\d{4})$");
            return myRegex.IsMatch(value);
        }

        public static bool IsEIN(string value)
        {
            Regex myRegex = new Regex("^[1-9]\\d?-\\d{7}$");
            return myRegex.IsMatch(value);
        }
    }
}
