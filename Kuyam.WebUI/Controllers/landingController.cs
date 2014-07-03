using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Database;
using Kuyam.Domain.BlogServices;
using Kuyam.Database.BlogModels;
using Kuyam.Domain;
using System.Configuration;
using Kuyam.Domain.GiftCardServices;
using Kuyam.Domain.KuyamServices;
using Kuyam.Domain.LandingePageServices;
using Kuyam.Utility;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.Models.LandingPage;
using Kuyam.WebUI.WebMapper;
using Kuyam.WebUI.Extension;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Helpers;
using M2.Util;

namespace Kuyam.WebUI.Controllers
{
    public class LandingController : KuyamBaseController
    {
        private readonly ILandingPageServices _landingPageServices;
        private readonly GettyImageService _gettyImageService;
        public LandingController(ILandingPageServices landingPageServices,
            GettyImageService gettyImageService)
        {
            _landingPageServices = landingPageServices;
            _gettyImageService = gettyImageService;
        }

        public ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error404", "Error");
            var model = _landingPageServices.GetLandingPage(id);
            if (model==null || model.StatusEnum!=Types.LandingPageStatus.Published)
                return RedirectToAction("Error404", "Error");
            var viewModel = new LandingPageModel(model);
            var companyAppointmentService = (CompanyAppointmentController)DependencyResolver.Current.GetService(typeof (CompanyAppointmentController));
            viewModel.MainContent = companyAppointmentService.RenderCompanyProfileTimeSlots(this, viewModel.MainContent);

            return View(viewModel);
        }
    }
}
