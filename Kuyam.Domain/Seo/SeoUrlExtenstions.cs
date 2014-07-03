using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Seo
{
    public static class SeoUrlExtenstions
    {
        public static bool ExitsTabNameEnd(this string url, out string tabName)
        {
            tabName = null;
            if (string.IsNullOrEmpty(url))
                return false;
            url = url.TrimEnd('/');
            if (url.EndsWith("availability"))
            {
                tabName = "availability";
                return true;
            }
            else if (url.EndsWith("description"))
            {
                tabName = "description";
                return true;
            }
            else if (url.EndsWith("photo"))
            {
                tabName = "photo";
                return true;
            }
            else if (url.EndsWith("review"))
            {
                tabName = "review";
                return true;
            }
            else if (url.EndsWith("package"))
            {
                tabName = "package";
                return true;
            }
            return false;
        }


        public static bool ExitsTabNameStart(this string url, out string tabName)
        {
            tabName = null;
            if (string.IsNullOrEmpty(url))
                return false;
            url = url.TrimStart('~').TrimStart();
            if (url.StartsWith("/availability"))
            {
                tabName = "availability";
                return true;
            }
            else if (url.StartsWith("/class"))
            {
                tabName = "class";
                return true;
            }
            else if (url.StartsWith("/description"))
            {
                tabName = "description";
                return true;
            }
            else if (url.StartsWith("/photo"))
            {
                tabName = "photo";
                return true;
            }
            else if (url.StartsWith("/review"))
            {
                tabName = "review";
                return true;
            }
            else if (url.StartsWith("/package"))
            {
                tabName = "package";
                return true;
            }
            return false;
        }

        public static string RemoveTabNameFromRawUrl(this string url, string tabName)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            return url.Replace("/" + tabName, "");

        }

    }
}
