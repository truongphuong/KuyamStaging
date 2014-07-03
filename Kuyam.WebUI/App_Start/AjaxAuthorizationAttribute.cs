using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kuyam.WebUI.App_Start
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AjaxAuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest()
                && !filterContext.HttpContext.User.Identity.IsAuthenticated
                &&(filterContext.ActionDescriptor.GetCustomAttributes(typeof(AjaxAuthorizationAttribute), true).Count() > 0
                  || filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AjaxAuthorizationAttribute), true).Count() > 0))
            {

                filterContext.HttpContext.SkipAuthorization = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                filterContext.Result = new HttpUnauthorizedResult("Unauthorized");
                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
                filterContext.HttpContext.Response.End();                
            }
           
        }

    }


    public class LoggingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {  
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
        }


    }

}
