using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace M2.Util.MVC
{
    public static class ImageHelper
    {
        public static MvcHtmlString GravatarImage(this System.Web.Mvc.HtmlHelper helper, string email, int size, object htmlAttributes = null)
        {

            TagBuilder tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttribute("src", Gravatar.GetUrl(email, size));
            tagBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return MvcHtmlString.Create(tagBuilder.ToString());
        }
    }
}
