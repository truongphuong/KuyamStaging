using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Kuyam.WebUI
{
    public static class StringExtentions
    {
        /// <summary>
        /// Text to HTML by convert \n to <br/>
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string TextToHtml(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            str = str.Replace("\n", "<br/>");
            return str;
        }


        public static DateTime ToDateTime(this string str, string dateFormat)
        {
            try
            {
                DateTime date = DateTime.MinValue;
                DateTime.TryParseExact(str, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                return date;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }

        }

    }
}