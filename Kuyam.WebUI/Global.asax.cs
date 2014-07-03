using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using M2.Util;
using System.Configuration;
using Kuyam.WebUI.Controllers;
using Kuyam.Domain;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.App_Start;
using System.Data.SqlClient;
using Kuyam.Repository;
using System.Web.Optimization;
using Kuyam.Domain.Tasks;
using System.Text.RegularExpressions;
using Kuyam.WebUI.Routes;
using Kuyam.Repository.Infrastructure.DependencyManagement;
using Kuyam.Repository.Base;

namespace Kuyam.WebUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class MySSLFilter : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext.Request.IsSecureConnection)
            {
                return;
            }

            if (string.Equals(filterContext.HttpContext.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            HandleNonHttpsRequest(filterContext);
        }
    }
       
    public class KuyamAuthorizeAttribute : AuthorizeAttribute
    {

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool isLogin = filterContext.RequestContext.HttpContext.Request.IsAuthenticated;

            if (!isLogin)
            {
                if (this.Roles.Contains("Admin") || this.Roles.Contains("Agent"))
                {
                    if (filterContext == null)
                    {
                        throw new ArgumentNullException("filterContext");
                    }

                    if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                    {
                        filterContext.Result = new HttpUnauthorizedResult();
                    }
                    var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        filterContext.Result = new RedirectResult("~/Admin/Login?ReturnUrl=" + returnUrl);
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Admin/Login");
                    }

                }

            }
        }

    }

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            MyApp.Settings.UseSSL = (ConfigurationManager.AppSettings["UseSSL"].ToBool() == true);
            if (MyApp.Settings.UseSSL)
                filters.Add(new MySSLFilter());

            //filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
            routePublisher.RegisterRoutes(routes);

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Kuyam.WebUI.Controllers" }
            );

        }

        protected void Application_Start()
        {
            EngineContext.Initialize(false);
            //set dependency resolver
            var dependencyResolver = new KuyamDependencyResolver();
            DependencyResolver.SetResolver(dependencyResolver);
            SitemapLoader.RegisterLoader();
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //TaskManager.Instance.Initialize();
            //TaskManager.Instance.Start();

            SMTPClient.Host = MyApp.Settings.Email.Host;
            SMTPClient.Port = MyApp.Settings.Email.Port;
            SMTPClient.EnableSSL = MyApp.Settings.Email.SSL;
            SMTPClient.Username = MyApp.Settings.Email.Username;
            SMTPClient.Password = MyApp.Settings.Email.Password;
            SMTPClient.From = MyApp.Settings.Email.FromEmail;
            SMTPClient.GoAsync = false;
            //Logger.Init(Logger.DestinationType.DebugStream, "KuyamSite");
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            var httpException = exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                if (!webHelper.IsStaticResource(this.Request))
                {
                    Response.Clear();
                    Server.ClearError();
                    Response.TrySkipIisCustomErrors = true;

                    IController errorController = EngineContext.Current.Resolve<Kuyam.WebUI.Controllers.ErrorController>();
                    var routeData = new RouteData();
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "Error404");
                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));

                }
            }

        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //if (HttpContext.Current.Request.IsSecureConnection.Equals(false))
            //{
            //    Response.Redirect("http://" + Request.ServerVariables["HTTP_HOST"] + HttpContext.Current.Request.RawUrl);
            //}
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            var requestType = new HttpRequestWrapper(HttpContext.Current.Request).IsAjaxRequest();
            string rawUrl = HttpContext.Current.Request.RawUrl;
            if (!requestType && !rawUrl.ToLower().Contains("admin") && !rawUrl.ToLower().Contains("/concierge"))
            {
                string ReturnUrl = HttpContext.Current.Request.QueryString["ReturnUrl"];
                HttpContext context = HttpContext.Current;

                var proposedId = context.Request.QueryString["proposedId"];
                var start = context.Request.QueryString["start"];

                if (context.Request.IsAuthenticated && !string.IsNullOrEmpty(ReturnUrl))
                {
                    var javascript = "<script type=\"text/javascript\">$(document).ready(function(){window.location.href='" + ReturnUrl + "';}); </script>";
                    context.Response.Write(javascript);
                    CompleteRequest();

                }
                else if (!context.Request.IsAuthenticated && !string.IsNullOrEmpty(ReturnUrl))
                {
                    var javascript = "<script type=\"text/javascript\">$(document).ready(function(){ShowLoginPopup();}); </script>";
                    context.Response.Write(javascript);
                    CompleteRequest();
                }

                if (!string.IsNullOrEmpty(proposedId))
                {
                    var javascript = "<script type=\"text/javascript\">$(document).ready(function(){getProposedDataCheckout(" + proposedId + ")}); </script>";
                    context.Response.Write(javascript);
                    CompleteRequest();
                }
                else if (!string.IsNullOrEmpty(start) && rawUrl.ToLower().Contains("book"))
                {
                    if (context.Request.IsAuthenticated)
                    {
                        var javascript = "<script type=\"text/javascript\">$(document).ready(function(){getServicebyStartTime('" + start + "')}); </script>";
                        context.Response.Write(javascript);
                        CompleteRequest();
                    }
                    else
                    {
                        var javascript = "<script type=\"text/javascript\">$(document).ready(function(){ShowLoginPopup();}); </script>";
                        context.Response.Write(javascript);
                        CompleteRequest();
                    }
                }
            }
        }

    }
}