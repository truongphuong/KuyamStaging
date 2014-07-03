using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;

namespace Kuyam.WebUI.Controllers
{
	[Authorize]
    public class VersionHistoryController : KuyamBaseController
    {
        //private kuyamEntities _db = new kuyamEntities();

        //public ViewResult Index()
        //{
        //    var list = _db.VersionHistories.ToList();
        //    list.Reverse();
        //    return View(list);
        //}


        //protected override void Dispose(bool disposing)
        //{
        //    _db.Dispose();
        //    base.Dispose(disposing);
        //}
    }
}