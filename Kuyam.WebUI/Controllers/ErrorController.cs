using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain;
using Kuyam.Utility;

namespace Kuyam.WebUI.Controllers
{
    public class ErrorController : KuyamBaseController
    {
        private NotificationService _notificationService;

        public ErrorController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public ActionResult GenericUrl()        {
            
            return InvokeHttp404();
        }
        public ActionResult Fail()
        {
            return View("Index");
        }

        public ActionResult Error404() {
            return View();
        }

        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;

            return View();
        }

        public ActionResult TestIos(string id)
        {
            _notificationService.TestIOSNotification(id);
            return RedirectToAction("Index");
        }

        //[Log]
        public ActionResult TestFilter()
        {
            //throw new Exception("aaaaaa");
            return Content("aa");
        }
    }
}
