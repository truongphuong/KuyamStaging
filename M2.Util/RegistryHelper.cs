// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using win32 = Microsoft.Win32;

namespace M2.Util
{
    public static class RegistryHelper
    {
        public static long GetKey(string rootKey, string keyName, string valueName, long defaultValue) // where T : struct
        {
            object o = win32.Registry.GetValue(String.Format("{0}\\{1}", rootKey, keyName), valueName, defaultValue);
            return o == null ? defaultValue : Convert.ToInt64(o);
        }

        public static int GetKey(string rootKey, string keyName, string valueName, int defaultValue) // where T : struct
        {
            object o = win32.Registry.GetValue(String.Format("{0}\\{1}", rootKey, keyName), valueName, defaultValue);
            return o == null ? defaultValue : Convert.ToInt32(o);
        }

        public static string GetKey(string rootKey, string keyName, string valueName, string defaultValue)
        {
            object o = win32.Registry.GetValue(String.Format("{0}\\{1}", rootKey, keyName), valueName, defaultValue);
            return o == null ? defaultValue : o.ToString();
        }

    }
}