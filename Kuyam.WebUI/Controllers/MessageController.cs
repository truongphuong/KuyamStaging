using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kuyam.WebUI.Controllers
{
    public class MessageController : KuyamBaseController
    {
        //
        // GET: /Message/

        public ActionResult Index()
        {
            return View();
        }

    }
}
