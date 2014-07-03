using Kuyam.Repository;
using Kuyam.Repository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Kuyam.Domain.Seo
{
    public class GenericPathRoute : Route
    {
        #region Constructors

        public GenericPathRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Methods

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            string virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            string applicationPath = httpContext.Request.ApplicationPath;

            string tabName = null;
            if (virtualPath.ToLowerInvariant().ExitsTabNameStart(out tabName))
            {
                string rawUrl = httpContext.Request.RawUrl;
                var newVirtualPath = rawUrl.ToLowerInvariant().RemoveTabNameFromRawUrl(tabName);
                if (string.IsNullOrEmpty(newVirtualPath))
                    newVirtualPath = "/";
                newVirtualPath = "~" + newVirtualPath;
                httpContext.RewritePath(newVirtualPath, true);
            }

            RouteData data = base.GetRouteData(httpContext);

            if (data != null)
            {
                var friendlyUrlService = EngineContext.Current.Resolve<ISeoFriendlyUrlService>();
                var slug = data.Values["generic_se_name"] as string;
                var friendlyUrl = friendlyUrlService.GetBySlug(slug);
                if (friendlyUrl == null)
                {
                    //no URL record found                  
                    data.Values["controller"] = "Error";
                    data.Values["action"] = "PageNotFound";
                    return data;
                }
                if (!friendlyUrl.IsActive)
                {
                    
                    var activeSlug = friendlyUrlService.GetActiveSlug(friendlyUrl.EntityId, friendlyUrl.EntityName);
                    if (!string.IsNullOrWhiteSpace(activeSlug))
                    {
                        //the active one is found
                        var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                        var response = httpContext.Response;
                        response.Status = "301 Moved Permanently";
                        response.RedirectLocation = string.Format("{0}{1}", webHelper.GetStoreLocation(false), activeSlug);
                        response.End();
                        return null;
                    }
                    else
                    {
                        //no active slug found

                        data.Values["controller"] = "Error";
                        data.Values["action"] = "PageNotFound";
                        return data;
                    }
                }



                switch (friendlyUrl.EntityName.ToLowerInvariant())
                {
                    case "company":
                        {
                            data.Values["controller"] = "CompanyProfile";
                            data.Values["action"] = tabName ?? "availability";
                            data.Values["id"] = friendlyUrl.EntityId;
                            data.Values["SeName"] = friendlyUrl.Slug;
                        }
                        break;
                    case "category":
                        {
                            data.Values["controller"] = "company";
                            data.Values["action"] = tabName ?? "companysearch";
                            data.Values["id"] = friendlyUrl.EntityId;
                            data.Values["SeName"] = friendlyUrl.Slug;
                        }
                        break;
                    case "blog":
                        {
                            data.Values["controller"] = "Home";
                            data.Values["action"] = tabName ?? "Index";
                            data.Values["id"] = friendlyUrl.EntityId;
                            data.Values["SeName"] = friendlyUrl.Slug;
                        }
                        break;
                    case "post":
                        {
                            data.Values["controller"] = "Blog";
                            data.Values["action"] = tabName ?? "Post";
                            data.Values["id"] = friendlyUrl.EntityId;
                            data.Values["SeName"] = friendlyUrl.Slug;
                        }
                        break;
                    default:
                        {
                        }
                        break;
                }

            }

            return data;
        }

        #endregion
    }
}
