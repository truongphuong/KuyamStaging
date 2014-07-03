using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Utility;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.Domain;
using Kuyam.Repository.Infrastructure;
using System.Web.Routing;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    public class KuyamBaseController : Controller
    {
        protected int ProfileId { get; set; }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (Request.IsAuthenticated)
                this.ProfileId = this.GetProfileId();
        }

        private int GetProfileId()
        {
            ViewBag.IsAdmin = false;
            ViewBag.IsAgent = false;
            ViewBag.IsAdminOrAgent = false;
            ViewBag.companyId = 0;
            ViewBag.CompanyName = string.Empty;
            ViewBag.CompanyProfile = new Profile();
            int profileId = 0;
            try
            {
                string companyID = Request.Params["companyId"];
                int.TryParse(companyID, out profileId);
            }
            catch (Exception) // catch for error when post data contain Html string
            {
                return 0;
            }
            Profile profile = EngineContext.Current.Resolve<CompanyProfileService>().GetProfileByID(profileId != 0 ? profileId : MySession.ProfileID);
            ViewBag.CompanyName = (profile != null ? profile.Name : string.Empty);
            ViewBag.CompanyProfile = profile;
            profileId = profile != null ? profile.ProfileID : 0;
            if (profileId == 0)
                return 0;
            bool isAdmin = this.AuthorizationAdmin(profileId);
            bool isAgent = this.AuthorizationAgent(profileId);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsAgent = isAgent;
            ViewBag.IsAdminOrAgent = isAdmin || isAgent;
            ViewBag.companyId = profileId;
            if (MySession.CustID == profile.CustID)
                return profileId;
            else if (!isAdmin && !isAgent)
                return 0;
            return profileId;
        }

        private bool AuthorizationAdmin(int companyId)
        {
            bool isLogin = Request.IsAuthenticated;
            bool isAdmin = User.IsInRole("Admin");
            ProfileCompany profile = EngineContext.Current.Resolve<CompanyProfileService>().GetProfileCompanyByID(companyId, MySession.CustID);
            return (isLogin && isAdmin && (profile == null) && (companyId > 0));
        }

        private bool AuthorizationAgent(int companyId)
        {
            bool isLogin = Request.IsAuthenticated;
            bool isAgent = User.IsInRole("Agent");
            ProfileCompany profile = EngineContext.Current.Resolve<CompanyProfileService>().GetProfileCompanyByID(companyId, MySession.CustID);
            return (isLogin && isAgent && (profile == null) && (companyId > 0));
        }

        protected virtual ActionResult InvokeHttp404()
        {
            IController errorController = EngineContext.Current.Resolve<Kuyam.WebUI.Controllers.ErrorController>();
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "PageNotFound");
            errorController.Execute(new RequestContext(this.HttpContext, routeData));

            return new EmptyResult();
        }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage
        {
            get { return (string)TempData[Contants.SuccessMessageTempData]; }
            set { TempData[Contants.SuccessMessageTempData] = value; }
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage
        {
            get { return (string)TempData[Contants.ErrorMessageTempData]; }
            set { TempData[Contants.ErrorMessageTempData] = value; }
        }


    }
}
