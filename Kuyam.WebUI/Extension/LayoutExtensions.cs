using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Kuyam.WebUI.Extension
{
    public static class LayoutExtensions
    {
        private static string _title;
        private static string _description;
        private static string _url;
        private static string _image;

        public static MvcHtmlString MetaTag(this HtmlHelper html)
        {
            StringBuilder stbdBuilder = new StringBuilder();
            //fb
            stbdBuilder.AppendFormat(" <meta name=\"og:title\" content=\"{0}\" />", _title);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:url\" content=\"{0}\"/>", _url);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:description\" content=\"{0}\"/>", _description);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:image\" content=\"{0}\" />", _image);
            // twitter 
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:card\" content=\"summary\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:site\" content=\"@iacquire\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:creator\" content=\"@iacquire\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:title\" content=\"{0}\" />", _title);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:url\" content=\"{0}\" />", _url);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:description\" content=\"{0}\" />", _description);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:image\" content=\"{0}\"/>", _image);
            //Google+ 
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"name\" content=\"{0}\" />", _title);
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"description\" content=\"{0}\" />", _description);
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"image\" content=\"{0}\" />", _image);

            return MvcHtmlString.Create(stbdBuilder.ToString());
        }

        public static void SetTitle(this HtmlHelper html, string title = "")
        {
            _title = title;
        }

        public static void SetDescription(this HtmlHelper html, string description = "")
        {
            _description = description;
        }

        public static void SetUrl(this HtmlHelper html, string url = "")
        {
            _url = url;
        }

        public static void SetImage(this HtmlHelper html, string image = "")
        {
            _image = image;
        }
    }
}