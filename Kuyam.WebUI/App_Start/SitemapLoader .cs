using Kuyam.Repository.Infrastructure;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;

namespace Kuyam.WebUI.App_Start
{
    public class SitemapLoader
    {
        public static void RegisterLoader()
        {
            // Setup global sitemap loader (required)
            MvcSiteMapProvider.SiteMaps.Loader = EngineContext.Current.Resolve<ISiteMapLoader>();

            // Check all configured .sitemap files to ensure they follow the XSD for MvcSiteMapProvider (optional)
            var validator = EngineContext.Current.Resolve<ISiteMapXmlValidator>();
            validator.ValidateXml(HostingEnvironment.MapPath("~/Mvc.sitemap"));

            // Register the Sitemaps routes for search engines (optional)
            XmlSiteMapController.RegisterRoutes(RouteTable.Routes);
        }
    }
}