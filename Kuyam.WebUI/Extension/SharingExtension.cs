using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace Kuyam.WebUI.Extension
{
    public class SharingExtension
    {
        public string Title { get; set; }
        public string Description;
        public string Url;
        public string Image;

        public MvcHtmlString MetaTag()
        {
            StringBuilder stbdBuilder = new StringBuilder();
            //fb
            stbdBuilder.AppendFormat(" <meta name=\"og:title\" content=\"{0}\" />", Title);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:url\" content=\"{0}\"/>", Url);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:description\" content=\"{0}\"/>", Description);
            stbdBuilder.AppendFormat("\n  <meta name=\"og:image\" content=\"{0}\" />", Image);
            // twitter 
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:card\" content=\"summary\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:site\" content=\"@iacquire\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:creator\" content=\"@iacquire\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:title\" content=\"{0}\" />", Title);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:url\" content=\"{0}\" />", Url);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:description\" content=\"{0}\" />", Description);
            stbdBuilder.AppendFormat("\n  <meta name=\"Twitter:image\" content=\"{0}\"/>", Image);
            //Google+ 
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"name\" content=\"{0}\" />", Title);
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"description\" content=\"{0}\" />", Description);
            stbdBuilder.AppendFormat("\n  <meta itemprop=\"image\" content=\"{0}\" />", Image);

            return MvcHtmlString.Create(stbdBuilder.ToString());
        }
    }
}