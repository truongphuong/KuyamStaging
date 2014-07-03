using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain.LandingePageServices;
using Kuyam.Domain.MediaServices;
using Kuyam.WebUI.Models.LandingPage;
using PagedList;

namespace Kuyam.WebUI.Controllers
{
    [KuyamAuthorizeAttribute(Roles = "Admin")]
    public class AdminLandingPageController : KuyamBaseController
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminLandingPageController"/> class.
        /// </summary>
        /// <param name="landingPageServices">The landing page services.</param>
        public AdminLandingPageController(ILandingPageServices landingPageServices, CustGettyImageService custGettyImageService)
        {
            _landingPageServices = landingPageServices;
            _custGettyImageService = custGettyImageService;
            var statusList = new List<SelectListItem>
            {
                new SelectListItem {Text = "all", Value = "0"},
                new SelectListItem
                {
                    Text = "draft",
                    Value = ((int) Types.LandingPageStatus.Draft).ToString(CultureInfo.InvariantCulture)
                },
                new SelectListItem
                {
                    Text = "published",
                    Value = ((int) Types.LandingPageStatus.Published).ToString(CultureInfo.InvariantCulture)
                },
                new SelectListItem
                {
                    Text = "unpublish",
                    Value = ((int) Types.LandingPageStatus.Unpublished).ToString(CultureInfo.InvariantCulture)
                }
            };
            _statusSelectList = new SelectList(statusList, "Value", "Text");
        }
        #endregion


        #region Private Properties

        private readonly ILandingPageServices _landingPageServices;
        private readonly CustGettyImageService _custGettyImageService;

        private readonly SelectList _statusSelectList;
        #endregion


        #region Actions
        public ActionResult Index(string key, int status = 0, int page = 1)
        {
            if (page < 1)
                page = 1;
            var landingPages = _landingPageServices.GetLandingPages(key, status).OrderByDescending(l => l.Id);
            IPagedList<LandingPage> items = landingPages.ToPagedList(page, 10);

            var model = new LandingPageList
            {
                StatusList = _statusSelectList,
                Status = status,
                PagedList = items,
                SearchKey = key
            };

            if (Request.IsAjaxRequest())
                return PartialView("_AdminLandingPageList", model);
            return View(model);
        }


        public ActionResult Edit(int id = 0, string linkReturn = null)
        {
            var landingPage = new LandingPage();
            if (id != 0)
                landingPage = _landingPageServices.GetLandingPage(id);

            LandingPageModel model = null;
            if (landingPage != null)
            {
                model = new LandingPageModel(landingPage);
                if (!string.IsNullOrEmpty(linkReturn))
                    model.LinkReturn = linkReturn;
            }
            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(LandingPageModel model, int[] companiesRelated)
        {
            ActionResult redirect = null;
            if (ModelState.IsValid)
            {
                try
                {
                    LandingPage landingPage = model.GetLandingPage(companiesRelated);
                    if (landingPage.Id == 0)
                    {
                        landingPage = _landingPageServices.CreateLandingPage(landingPage);
                        SuccessMessage = "create new landing page successfully";
                        model.Id = landingPage.Id;
                    }
                    else
                    {
                        landingPage = _landingPageServices.UpdateLandingPage(landingPage);
                        SuccessMessage = "update landing page successfully";
                    }

                    if (Request.IsAjaxRequest())
                        return Json(new {result = true, id = landingPage.Id, message = SuccessMessage},
                            JsonRequestBehavior.AllowGet);

                    if (landingPage.StatusEnum == Types.LandingPageStatus.Published ||
                        landingPage.StatusEnum == Types.LandingPageStatus.Unpublished)
                    {
                        if (!string.IsNullOrEmpty(model.LinkReturn))
                            return Redirect(model.LinkReturn);
                        return RedirectToAction("Index");
                    }

                }
                catch (Exception ex)
                {
                    ErrorMessage = model.Id == 0 ? "create new landing page fail" : "update landing page fail";
                }
            }
            else
            {
                ErrorMessage = "data is invalid, please check again";
            }
          
            if (Request.IsAjaxRequest())
                return Json(new { result = false, id = model.Id, message = ErrorMessage }, JsonRequestBehavior.AllowGet);


            // reload data for model
            var oldLandingPage = _landingPageServices.GetLandingPage(model.Id);
            if (oldLandingPage != null)
            {
                model.LastUpdated = oldLandingPage.LastUpdated;
                model.Status = oldLandingPage.Status;

                if (model.Banner != 0 && model.Banner == oldLandingPage.Banner)
                {
                    model.SetImageCrop(oldLandingPage.ImageCrop);
                }
            }
            else if (model.Banner > 0)
            {
                ImageCrop image = _custGettyImageService.GetImageCrop(model.Banner);
                if (image != null)
                    model.SetImageCrop(image);
            }

            if (companiesRelated != null)
            {
                model.SetCompaniesRelated(companiesRelated);
            }

            return View(model);
        }

        public JsonResult ValidateUrlName(string urlName, int id)
        {
           return Json(_landingPageServices.ValidateUrlName(urlName, id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Preview(int id)
        {
            var landingPage = new LandingPage();
            if (id != 0)
                landingPage = _landingPageServices.GetLandingPage(id);

            LandingPageModel model = null;
            if (landingPage != null)
            {
                model = new LandingPageModel(landingPage);
            }
            return View(model);
        }
        #endregion
    }
}
