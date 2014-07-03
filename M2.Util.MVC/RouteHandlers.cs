using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;

namespace M2.Util.MVC
{
	/*
	 * Allows for defining controller and action names with underscores, but URL's use hyphens
	 * http://stackoverflow.com/questions/2070890/asp-net-mvc-support-for-urls-with-hyphens
	 * 
	 * Add this to :
			routes.Add(
				new Route("{controller}/{action}/{id}",
						 new RouteValueDictionary(
						 new { controller = "Home", action = "Index", id = UrlParameter.Optional }),
					 new M2.Util.MVC.HyphenatedRouteHandler())
			 );

	 */
	/// <summary>
	/// Allows for defining controller and action names with underscores, but URL's use hyphens
	/// http://stackoverflow.com/questions/2070890/asp-net-mvc-support-for-urls-with-hyphens
	/// </summary>
	public class HyphenatedRouteHandler : MvcRouteHandler
	{
		protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			requestContext.RouteData.Values["controller"] = requestContext.RouteData.Values["controller"].ToString().Replace("-", "_");
			requestContext.RouteData.Values["action"] = requestContext.RouteData.Values["action"].ToString().Replace("-", "_");
			return base.GetHttpHandler(requestContext);
		}
	}
}
