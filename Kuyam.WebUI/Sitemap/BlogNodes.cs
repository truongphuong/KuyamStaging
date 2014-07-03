using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.LandingePageServices;
using MvcSiteMapProvider;
using Kuyam.Domain.Seo;

namespace Kuyam.WebUI.Sitemap
{
    public class BlogNodes : DynamicNodeProviderBase 
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            var _blogPostService = (IBlogPostService)DependencyResolver.Current.GetService(typeof(IBlogPostService));
            var posts = _blogPostService.GetAll();
            foreach (var post in posts)
            {
                DynamicNode dynamicNode = new DynamicNode();
                dynamicNode.Title = post.Title;
                dynamicNode.Controller = "Blog";
                dynamicNode.Action = "Post";
                dynamicNode.RouteValues.Add("seName", post.GetSeName(post.PostRowID,"post"));
                dynamicNode.LastModifiedDate = post.DateModified;
                yield return dynamicNode;
            }
        }
    }
}