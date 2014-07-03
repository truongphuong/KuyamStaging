using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using System.Xml.Serialization;

namespace Kuyam.Utility
{
    public class UtiHelper
    {
        public static bool IsLockedTime(string key)
        {
            if (System.Web.HttpContext.Current.Application[key] == null) { return false; }
            DateTime expiredDate = (DateTime)System.Web.HttpContext.Current.Application[key];
            return DateTime.Now < expiredDate;
        }

        public static bool UseSsl
        {
            get
            {
                bool useSsl = !String.IsNullOrEmpty(ConfigurationManager.AppSettings["UseSSL"]) &&
                Convert.ToBoolean(ConfigurationManager.AppSettings["UseSSL"]);
                return useSsl;
            }
        }

        public static string GetSeName(string name, bool allowUnicode = false)
        {
            if (String.IsNullOrEmpty(name))
                return name;
            string okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
            name = name.Trim().ToLowerInvariant();

            var sb = new StringBuilder();
            foreach (char c in name.ToCharArray())
            {
                string c2 = c.ToString();

                if (allowUnicode)
                {
                    if (char.IsLetterOrDigit(c) || okChars.Contains(c2))
                        sb.Append(c2);
                }
                else if (okChars.Contains(c2))
                {
                    sb.Append(c2);
                }
            }
            string name2 = sb.ToString();
            name2 = name2.Replace(" ", "-");
            while (name2.Contains("--"))
                name2 = name2.Replace("--", "-");
            while (name2.Contains("__"))
                name2 = name2.Replace("__", "_");
            return name2;
        }

        public static T Deserialize<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            try
            {
                var deSerializer = new XmlSerializer(typeof(T));
                T model;
                using (TextReader reader = new StringReader(xml))
                {
                    model = deSerializer.Deserialize(reader) as T;
                }

                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
