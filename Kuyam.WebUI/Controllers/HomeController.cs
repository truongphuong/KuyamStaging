using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.WebUI.Models;
using System.Web.Security;
using Kuyam.Database;
using Kuyam.Domain.Authentication;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.Domain.Services;
using System.Reflection;
using Kuyam.WebUI.Helpers;
using Kuyam.Domain;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.KuyamServices;
using Kuyam.WebUI.Extension;
using Kuyam.Domain.HomeServices;
using Kuyam.WebUI.App_Start;
using Kuyam.Database.Extensions;


namespace Kuyam.WebUI.Controllers
{
    public class HomeController : KuyamBaseController
    {      
        private readonly IBlogPostService _postService;
        private readonly IFeaturedCompanyService _featuredCompanyService;
        private readonly IProfileCompanyService _profileCompanyService;
        private readonly IHomeService _homeService;

        public HomeController(IBlogPostService postService,
            IFeaturedCompanyService featuredCompanyService,
            IProfileCompanyService profileCompanyService,
            IHomeService homeService)
        {
            _postService = postService;
            _featuredCompanyService = featuredCompanyService;
            _profileCompanyService = profileCompanyService;
            _homeService = homeService;
        }


        public ActionResult Index()
        {
            return View();
        }

        /*
        public ActionResult Index(int? id)
        {
            var description = MyApp.Settings.TagSetting.HomeDescription;
            ViewBag.MetaTagExtension = new MetaTagExtension(description);

            if (!Request.IsAuthenticated)
            {
                ViewBag.ParentCategories = _profileCompanyService.GetParentService();
                var featuredPost = _postService.GetFeaturedPost();

                var featuredCompany = _featuredCompanyService.Get().OrderBy(f => f.priority).FirstOrDefault();
                var profileCompany = _profileCompanyService.GetByProfileId(featuredCompany != null ? featuredCompany.ProfileID : 0);
                List<PostExt> posts = null;
                if (profileCompany != null)
                {
                    posts = _postService.GetRecentPosts(6);
                }
                else
                {
                    posts = _postService.GetRecentPosts(8);
                }

                ViewBag.Posts = posts;
                ViewBag.FeaturedCompany = featuredCompany;
                ViewBag.FeaturedPost = featuredPost;
                ViewBag.ProfileCompany = profileCompany;

                if (id.HasValue)
                {
                    var category = _homeService.GeCategoryById(id.Value);

                    if (category != null && category.CategoryName.ToLower() == "top la spots")
                    {
                        //var model = _postService.GetPostByCategoryId(category.CategoryID);

                        var webBlogModel = new WebBlogModels();
                        var companysBlogsModels = new List<CompanysBlogsModels>();
                        var index = 0;
                        var featuredCompanies = new List<ProfileCompany>();

                        if (id.HasValue)
                        {
                            featuredPost = _homeService.GetFeaturedPostAtHomePage(category.FeaturedPostID, null, null);
                            featuredCompanies = _homeService.GetFeaturedCompaniesAtHomePage(id.Value);
                            webBlogModel.FeaturedPost = featuredPost;
                        }
                        else
                        {
                            featuredCompanies = _homeService.GetFeaturedCompaniesAtHomePage();
                            webBlogModel.FeaturedPost = _postService.GetFeaturedPost();
                        }


                        if (featuredCompanies != null && featuredCompanies.Count() > 0)
                        {
                            foreach (var featured in featuredCompanies)
                            {
                                var postsTemplate = posts.Skip(index).Take(6);
                                var companysBlogsModel = new CompanysBlogsModels
                                {
                                    FeaturedCompany = featured,
                                    Posts = postsTemplate.ToList()
                                };

                                companysBlogsModels.Add(companysBlogsModel);
                                index += 6;
                            }
                        }

                        webBlogModel.CompanysBlogsModels = companysBlogsModels;

                        //Add to view bag
                        ViewBag.CompaniesPostsFeatured = webBlogModel;
                        ViewBag.Id = id ?? -1;
                        ViewBag.Companies = featuredCompanies;
                        ViewBag.Posts = _postService.Get20LastestPostByCategoryId(category.CategoryID, featuredPost == null ? Guid.Empty : featuredPost.PostID);
                        return View("TopLaSpots");
                    }
                }
                //Trong added
            }
            else
            {
                var flag = Request.QueryString["message"];
                if (flag == "true")
                {
                    ViewBag.Message = MvcHtmlString.Create("showAlertMessage(\"you haven't permission to access this appointment.\")");
                }
                else if (flag == "booked")
                {
                    ViewBag.Message = MvcHtmlString.Create("showAlertMessage(\"you booked this appointment.\")");
                }


                //posts =_postService.GetRecentPosts();
                var localLat = MySession.Cust.Latitude;
                var localLng = MySession.Cust.Longitude;

                var webBlogModel = new WebBlogModels();
                var companysBlogsModels = new List<CompanysBlogsModels>();
                var index = 0;
                var featuredCompanies = new List<ProfileCompany>();
                List<PostExt> posts = null;

                if (id.HasValue)
                {
                    var category = _homeService.GeCategoryById(id.Value);
                    var featuredPost = _homeService.GetFeaturedPostAtHomePage(category.FeaturedPostID, localLat, localLng);
                    if (category != null && category.CategoryName.ToLower() == "top la spots")
                    {
                        posts = _postService.Get20LastestPostByCategoryId(category.CategoryID, featuredPost == null ? Guid.Empty : featuredPost.PostID);
                    }
                    else
                    {
                        posts = _homeService.GetPostsAtHomePage(category.CategoryID, category.FeaturedPostID, localLat, localLng);
                    }

                    featuredCompanies = _homeService.GetFeaturedCompaniesAtHomePage(id.Value);
                    webBlogModel.FeaturedPost = featuredPost;
                }
                else
                {
                    featuredCompanies = _homeService.GetFeaturedCompaniesAtHomePage();
                    posts = _postService.GetRecentPosts(localLat, localLng);
                    webBlogModel.FeaturedPost = _postService.GetFeaturedPost();
                }


                if (featuredCompanies != null && featuredCompanies.Count() > 0)
                {
                    foreach (var featured in featuredCompanies)
                    {
                        var postsTemplate = posts.Skip(index).Take(6);
                        var companysBlogsModel = new CompanysBlogsModels
                        {
                            FeaturedCompany = featured,
                            Posts = postsTemplate.ToList()
                        };

                        companysBlogsModels.Add(companysBlogsModel);
                        index += 6;
                    }
                }

                webBlogModel.CompanysBlogsModels = companysBlogsModels;

                //Add to view bag
                ViewBag.CompaniesPostsFeatured = webBlogModel;
                ViewBag.Id = id ?? -1;
                ViewBag.Companies = featuredCompanies;
                ViewBag.Posts = posts;
                return View("IndexLogin");
            }
            //------------


            return View();
        }
         */

        public ActionResult CropImage()
        {
            return View();
        }

        public ActionResult Setting()
        {
            return View();
        }

        public ActionResult fbCalendar()
        {
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            AuthenticationFacebook oAuthFacbook = new AuthenticationFacebook(new SettingService());
            if (user == null)
            {
                if (Request["code"] == null)
                {

                    return new RedirectResult(oAuthFacbook.AuthorizationLinkGet(), true);
                }
                else
                {

                    var conectorAuth = oAuthFacbook.AccessTokenGet(Request["code"]);
                    var connectorSource = new InfoConnServiceReference.ConnectorSource
                    {

                        UserId = -1,
                        AccessToken = conectorAuth.AccessToken,
                        RefressToken = conectorAuth.RefressToken,
                        ExpiresDate = conectorAuth.ExpiresDate,
                        ConnectorSourceType = (int)ConnectorSourceType.Facebook,
                        LastModified = DateTime.Now,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.Now,
                        CacheLastUpdate_Longer = DateTime.Now,
                        CacheLastUpdate_Medium = DateTime.Now,
                        DoCacheUpdate_Longer = false,
                        DoCacheUpdate_Medium = false,
                        DoCacheUpdate_Short = true
                    };
                    // phuong
                    //MySession.FacebookConnectorSource = connectorSource;
                    //end

                    //// reflect to readonly property
                    //PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

                    //// make collection editable
                    //isreadonly.SetValue(this.Request.QueryString, false, null);

                    //this.Request.QueryString.Remove("code");


                }
                return RedirectToAction("RegisterName", "Account");
            }
            else
            {
                var connectorSource = client.GetConnectorSource(user.CustID, InfoConnServiceReference.ConnectorSourceType.Facebook);
                if (connectorSource == null)
                {
                    if (Request["code"] == null)
                    {

                        return new RedirectResult(oAuthFacbook.AuthorizationLinkGet(), true);
                    }
                    else
                    {

                        var conectorAuth = oAuthFacbook.AccessTokenGet(Request["code"]);
                        connectorSource = new InfoConnServiceReference.ConnectorSource
                        {
                            UserId = user.CustID,
                            AccessToken = conectorAuth.AccessToken,
                            RefressToken = conectorAuth.RefressToken,
                            ExpiresDate = conectorAuth.ExpiresDate,
                            ConnectorSourceType = (int)ConnectorSourceType.Facebook,
                            LastModified = DateTime.Now,
                            IsUpdateRunning = false,
                            CacheLastUpdate_Short = DateTime.Now,
                            CacheLastUpdate_Longer = DateTime.Now,
                            CacheLastUpdate_Medium = DateTime.Now,
                            DoCacheUpdate_Longer = false,
                            DoCacheUpdate_Medium = false,
                            DoCacheUpdate_Short = true
                        };

                        client.AddConnectorSource(connectorSource);
                        //this.Request.QueryString.Remove("code");
                    }
                }
                /*
                if (MySession.FlagPage == (int)Types.FlagPageType.CalendarSetting){
                    return RedirectToAction("AddConnectorSource", "CalendarSetting");
                }
                else{
                    return View();
                }
                 */
                return View();
            }
        }

        public ActionResult ggCalendar()
        {
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            AuthenticationGoogle oAuthGoogle = new AuthenticationGoogle(new SettingService());

            if (Request["code"] == null)
            {
                return new RedirectResult(oAuthGoogle.AuthorizationLinkGet(), true);
            }

            var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);

            if (conectorAuth != null)
            {
                var connectorSource = new InfoConnServiceReference.ConnectorSource
                  {
                      UserId = (user != null ? user.CustID : -1),
                      AccessToken = conectorAuth.AccessToken,
                      RefressToken = conectorAuth.RefressToken,
                      ExpiresDate = conectorAuth.ExpiresDate,
                      ConnectorSourceType = (int)ConnectorSourceType.Google,
                      LastModified = DateTime.Now,
                      IsUpdateRunning = false,
                      CacheLastUpdate_Short = DateTime.Now,
                      CacheLastUpdate_Longer = DateTime.Now,
                      CacheLastUpdate_Medium = DateTime.Now,
                      DoCacheUpdate_Longer = false,
                      DoCacheUpdate_Medium = false,
                      DoCacheUpdate_Short = true,
                      Username = conectorAuth.Username
                  };

                var lstcalendar = client.AddConnectorSource(connectorSource);
            }

            return RedirectToAction("Index", "CalendarSetting");

            /*
            if (user == null)
            {
                if (Request["code"] == null)
                {

                    return new RedirectResult(oAuthGoogle.AuthorizationLinkGet(), true);
                }
                else
                {

                    var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);
                    var connectorSource = new InfoConnServiceReference.ConnectorSource
                    {

                        UserId = -1,
                        AccessToken = conectorAuth.AccessToken,
                        RefressToken = conectorAuth.RefressToken,
                        ExpiresDate = conectorAuth.ExpiresDate,
                        ConnectorSourceType = (int)ConnectorSourceType.Google,
                        LastModified = DateTime.Now,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.Now,
                        CacheLastUpdate_Longer = DateTime.Now,
                        CacheLastUpdate_Medium = DateTime.Now,
                        DoCacheUpdate_Longer = false,
                        DoCacheUpdate_Medium = false,
                        DoCacheUpdate_Short = true,
                        Username = conectorAuth.Username
                    };
                    MySession.GoogleConnectorSource = connectorSource;
                }
                return RedirectToAction("RegisterEmail", "Account");
            }
            else
            {
                var connectorSource = client.GetConnectorSource(MySession.CustID, InfoConnServiceReference.ConnectorSourceType.Google);
                if (connectorSource == null)
                {
                    if (Request["code"] == null)
                    {

                        return new RedirectResult(oAuthGoogle.AuthorizationLinkGet(), true);
                    }
                    else
                    {
                        var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);
                        connectorSource = new InfoConnServiceReference.ConnectorSource
                        {
                            UserId = user.CustID,
                            AccessToken = conectorAuth.AccessToken,
                            RefressToken = conectorAuth.RefressToken,
                            ExpiresDate = conectorAuth.ExpiresDate,
                            ConnectorSourceType = (int)ConnectorSourceType.Google,
                            LastModified = DateTime.Now,
                            IsUpdateRunning = false,
                            CacheLastUpdate_Short = DateTime.Now,
                            CacheLastUpdate_Longer = DateTime.Now,
                            CacheLastUpdate_Medium = DateTime.Now,
                            DoCacheUpdate_Longer = false,
                            DoCacheUpdate_Medium = false,
                            DoCacheUpdate_Short = true,
                            Username = conectorAuth.Username
                        };

                        client.AddConnectorSource(connectorSource);
                    }
                }
            */
            /*
            if (MySession.FlagPage == (int)Types.FlagPageType.CalendarSetting){
                return RedirectToAction("AddConnectorSource", "CalendarSetting");
            }
            else{
                return View();
            }
             */
            // return View();

        }

        public ActionResult oauth2callback()
        {
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();
            AuthenticationGoogle oAuthGoogle = new AuthenticationGoogle(new SettingService());

            var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);

            if (conectorAuth != null)
            {
                var connectorSource = new InfoConnServiceReference.ConnectorSource
                {
                    UserId = (user != null ? user.CustID : -1),
                    AccessToken = conectorAuth.AccessToken,
                    RefressToken = conectorAuth.RefressToken,
                    ExpiresDate = conectorAuth.ExpiresDate,
                    ConnectorSourceType = (int)ConnectorSourceType.Google,
                    LastModified = DateTime.Now,
                    IsUpdateRunning = false,
                    CacheLastUpdate_Short = DateTime.Now,
                    CacheLastUpdate_Longer = DateTime.Now,
                    CacheLastUpdate_Medium = DateTime.Now,
                    DoCacheUpdate_Longer = false,
                    DoCacheUpdate_Medium = false,
                    DoCacheUpdate_Short = true,
                    Username = conectorAuth.Username
                };

                var lstcalendar = client.AddConnectorSource(connectorSource);
            }

            return RedirectToAction("Index", "CalendarSetting");
            /*
            Cust user = MySession.Cust;
            InfoConnServiceReference.InfoConnSoapClient client = new InfoConnServiceReference.InfoConnSoapClient();

            if (user != null)
            {

                var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);
                var connectorSource = new InfoConnServiceReference.ConnectorSource
                {
                    UserId = user.CustID,
                    AccessToken = conectorAuth.AccessToken,
                    RefressToken = conectorAuth.RefressToken,
                    ExpiresDate = conectorAuth.ExpiresDate,
                    ConnectorSourceType = (int)ConnectorSourceType.Google,
                    LastModified = DateTime.Now,
                    IsUpdateRunning = false,
                    CacheLastUpdate_Short = DateTime.Now,
                    CacheLastUpdate_Longer = DateTime.Now,
                    CacheLastUpdate_Medium = DateTime.Now,
                    DoCacheUpdate_Longer = false,
                    DoCacheUpdate_Medium = false,
                    DoCacheUpdate_Short = true,
                    Username = conectorAuth.Username
                };
                client.AddConnectorSource(connectorSource);
                return RedirectToAction("ggCalendar");
            }
            else
            {
                if (Request["code"] == null)
                {
                    return new RedirectResult(oAuthGoogle.AuthorizationLinkGet(), true);
                }
                else
                {
                    var conectorAuth = oAuthGoogle.AccessTokenGet(Request["code"]);
                    var connectorSource = new InfoConnServiceReference.ConnectorSource
                    {
                        UserId = -1,
                        AccessToken = conectorAuth.AccessToken,
                        RefressToken = conectorAuth.RefressToken,
                        ExpiresDate = conectorAuth.ExpiresDate,
                        ConnectorSourceType = (int)ConnectorSourceType.Google,
                        LastModified = DateTime.Now,
                        IsUpdateRunning = false,
                        CacheLastUpdate_Short = DateTime.Now,
                        CacheLastUpdate_Longer = DateTime.Now,
                        CacheLastUpdate_Medium = DateTime.Now,
                        DoCacheUpdate_Longer = false,
                        DoCacheUpdate_Medium = false,
                        DoCacheUpdate_Short = true,
                        Username = conectorAuth.Username
                    };
                    MySession.GoogleConnectorSource = connectorSource;
                }
                return RedirectToAction("RegisterName", "Account");
            }
            */
        }

        public ActionResult logout()
        {
            FormsAuthentication.SignOut();

            return View();
        }

        [ActionName("privacy-security")]
        public ActionResult privacy_security(bool useLayout = false)
        {
            return View(useLayout);
        }

        public ActionResult about()
        {
            return View();
        }

        [ActionName("our-story")]
        public ActionResult our_story()
        {
            return RedirectToAction("about");
        }

        public ActionResult team()
        {
            return View();
        }

        public ActionResult faq()
        {
            return View();
        }

        [ActionName("learn-more")]
        public ActionResult learn_more()
        {
            return View();
        }

        public ActionResult terms(bool useLayout = false)
        {
            return View(useLayout);
        }

        public ActionResult nda(bool useLayout = false)
        {
            return View(useLayout);
        }

        public ActionResult contact()
        {
            return View();
        }

        public ActionResult api()
        {
            return View();
        }

        public ActionResult NoService()
        {
            return View();
        }

        public ActionResult SignUp(bool useLayout = false)
        {
            return View();
        }

        public ActionResult MeetTheTeamJackie()
        {
            return View();
        }

        public ActionResult MeetTheTeamKourosh()
        {
            return View();
        }

        public ActionResult MeetTheTeamTodd()
        {
            return View();
        }

        public ActionResult MeetTheTeamTony()
        {
            return View();
        }

        public ActionResult MeetTheTeamMatt()
        {
            return View();
        }

        public ActionResult MeetTheTeamCory()
        {
            return View();
        }

        public ActionResult BusinessSolutions()
        {
            return View();
        }

        public ActionResult HairCut()
        {
            return View();
        }

        public ActionResult howitworks()
        {
            return View();
        }
        public ActionResult lifestyle()
        {
            return View();
        }
        public ActionResult lifestyle2()
        {
            return View();
        }
        public ActionResult lifestyle3()
        {
            return View();
        }
        public ActionResult OurStory()
        {
            return View();
        }

        public ActionResult lifestyle4()
        {
            return View();
        }
        public ActionResult howitworks_pricing()
        {
            return View();
        }
        public ActionResult howitworks_user()
        {
            return View();
        }
        public ActionResult LoginHeart()
        {
            return View();
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        public ActionResult SignUpConfirm()
        {
            string email = Request.Params["email"];
            Kuyam.Database.Invite inviteCode = Kuyam.Domain.AccountHelper.GetInviteCode(email);
            if (email != null && inviteCode != null)
            {
                try
                {
                    string template = string.Empty;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/thankssignup.cshtml")))
                    {
                        template = reader.ReadToEnd();
                    }

                    // create template data
                    dynamic myObject = new
                    {
                        Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.UtcNow),
                        Host = EmailHelper.GetStoreHost(),
                        Email = email,
                        UserName = inviteCode.Name.ToString()
                    };

                    // generate the content using razor engine
                    string templateResult = RazorEngine.Razor.Parse(template, myObject);

                    //Kuyam.Domain.AccountHelper.UpdateInviteStatus(email);

                    System.Threading.Thread oThread = new System.Threading.Thread(() => EmailHelper.SendThanksSignUpEmail(email, templateResult));

                    oThread.Start();
                    //Kuyam.Domain.AccountHelper.UpdateInviteStatus(email);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex);
                }
            }

            #region
            //Kuyam.Domain.AccountHelper.UpdateInviteStatus(email);
            //Kuyam.Database.Invite inviteCode = Kuyam.Domain.AccountHelper.GetInviteCode(email);

            //if (email != null && inviteCode != null)
            //{
            //    try
            //    {
            //        string template = string.Empty;
            //        using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/thankssignup.cshtml"))){
            //            template = reader.ReadToEnd();
            //        }

            //        string templateInvitecode = string.Empty;
            //        using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Templates/invitecode.cshtml")))
            //        {
            //            templateInvitecode = reader.ReadToEnd();
            //        }

            //        // create template data
            //        dynamic myObject = new{

            //            Date = String.Format("{0:dddd, MMMM d, yyyy}", DateTime.Now),
            //            Host = EmailHelper.GetStoreHost(),
            //            Email = email,
            //            InviteCode = inviteCode.Key,
            //            UserName = inviteCode.Name.ToString()
            //        };

            //        // generate the content using razor engine
            //        string templateResult = RazorEngine.Razor.Parse(template, myObject);
            //        string templateResultInvitecode = RazorEngine.Razor.Parse(templateInvitecode, myObject);

            //        Kuyam.Domain.AccountHelper.UpdateInviteStatus(email);

            //        System.Threading.Thread oThreadInvitecode = new System.Threading.Thread(() => EmailHelper.SendEmailInviteCodeSignUp(email, templateResultInvitecode));
            //        System.Threading.Thread oThread = new System.Threading.Thread(() => EmailHelper.SendThanksSignUpEmail(email, templateResult));

            //        oThreadInvitecode.Start();
            //        oThread.Start();
            //    }
            //    catch (Exception ex)
            //    {
            //        ModelState.AddModelError("", ex);
            //    }
            //}
            #endregion
            return View();
            //return Json("true",JsonRequestBehavior.AllowGet);
        }

        public ActionResult teammonika()
        {
            return View();
        }
        public ActionResult teamkourosh()
        {
            return View();
        }
        public ActionResult teamjackie()
        {
            return View();
        }
        public ActionResult teamcory()
        {
            return View();
        }
        public ActionResult teamtodd()
        {
            return View();
        }
        public ActionResult teamtony()
        {
            return View();
        }
        public ActionResult contactnew()
        {
            return View();
        }

        public ActionResult termsofuse()
        {
            return View();
        }
        public ActionResult privacypolicy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult contactnew(string name, string email, string subject, string message)
        {
            EmailHelper.SendContactEmail(name, email, subject, message);
            return View();
        }

        public ActionResult ToogleShowLiveChat()
        {
            MySession.ShowLiveChat = !MySession.ShowLiveChat;
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult InitAppointmentReview()
        {

            if (MySession.AppointmentReview == null)
            {
                return Json("null", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.AppointmentReview = MySession.AppointmentReview;
                return PartialView("_AppointmentReview");
            }
        }


        public int CheckIsDevice()
        {
            if (Session["downloadmobileapp"] == null)
            {
                Session["downloadmobileapp"] = true;
                return 1;
            }
            return 2;
        }
        public ActionResult DownloadMobileApp()
        {
            return View();
        }

    }


}
