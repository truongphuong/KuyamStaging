using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace M2.Util.MVC
{
	public static class Util
	{
		public static MvcHtmlString Bool(this System.Web.Mvc.HtmlHelper htmlHelper, bool value)
		{
			return MvcHtmlString.Create(value ? "true" : " false");
		}
	}
}
