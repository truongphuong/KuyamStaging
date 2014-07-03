using System.Collections.Generic;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain.LandingePageServices;
using Kuyam.Utility;
using MvcSiteMapProvider;

namespace Kuyam.WebUI.Sitemap
{
    public class LandingPageNodes : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            var _landingPageServices = (ILandingPageServices)DependencyResolver.Current.GetService(typeof(ILandingPageServices));
            var landingPages = _landingPageServices.GetLandingPages(string.Empty, (int)Types.LandingPageStatus.Published);
            foreach (LandingPage landingPage in landingPages)
            {
                DynamicNode dynamicNode = new DynamicNode();
                dynamicNode.Title = landingPage.Name;
                dynamicNode.Route = "landing page";
                dynamicNode.RouteValues.Add("id", landingPage.UrlName);
                dynamicNode.LastModifiedDate = landingPage.PublishDate;
                yield return dynamicNode;
            }
        }
    }
}