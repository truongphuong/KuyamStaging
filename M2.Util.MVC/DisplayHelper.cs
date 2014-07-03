using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace M2.Util.MVC
{
    public static class DisplayHelper
    {
        public static MvcHtmlString DisplayShortDate(this System.Web.Mvc.HtmlHelper htmlHelper, DateTime? dt)
        {
            if (dt.HasValue)
                return MvcHtmlString.Create(dt.Value.ToShortDateString());
            else
                return null;
        }

        public static MvcHtmlString DisplayShortDate(this System.Web.Mvc.HtmlHelper htmlHelper, DateTime dt)
        {
            return MvcHtmlString.Create(dt.ToShortDateString());
        }

        public static MvcHtmlString RawBreaks(this System.Web.Mvc.HtmlHelper htmlHelper, string text)
        {
            return MvcHtmlString.Create(text.Replace("/n", "<br />").Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\\n", "<br />").Replace("\\r", ""));
        }
	
		public static MvcHtmlString Div(this System.Web.Mvc.HtmlHelper htmlHelper, string cssclass = null, string inner = null)
		{
			return MvcHtmlString.Create(String.Format("<div class=\"{0}\">{1}</div>", cssclass, inner));
		}
	}
}


