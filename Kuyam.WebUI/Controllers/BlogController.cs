using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Database;
using Kuyam.Domain.BlogServices;
using Kuyam.Database.BlogModels;
using Kuyam.Domain;
using System.Configuration;
using Kuyam.Domain.KuyamServices;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.WebMapper;
using Kuyam.WebUI.Extension;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Helpers;
using Kuyam.Domain.HomeServices;
using Kuyam.Database.Extensions;

namespace Kuyam.WebUI.Controllers
{
    public class BlogController : KuyamBaseController
    {
        private readonly IBlogPostService _postService;
        private readonly IBlogUserService _blogUserService;
        private readonly IBlogPostCategoryService _postCategoryService;
        private readonly IBlogCategoryService _categoryService;
        private readonly IBlogCommentService _commentService;
        private readonly IFeaturedCompanyService _featuredCompanyService;
        private readonly IHomeService _homeService;

        public BlogController(IBlogPostService postService,
            IBlogUserService blogUserService,
            IBlogPostCategoryService postCategoryService,
            IBlogCategoryService categoryService,
            IBlogCommentService commentService,
            IFeaturedCompanyService featuredCompanyService,
            IHomeService homeService)
        {
            _postService = postService;
            _blogUserService = blogUserService;
            _postCategoryService = postCategoryService;
            _categoryService = categoryService;
            _commentService = commentService;
            _featuredCompanyService = featuredCompanyService;
            _homeService = homeService;
        }

        #region For Mobile
        /// <summary>
        /// Don't use temporary design: KUYAMWEB-2579
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PostDetails(int? id)
        {
            var postDetails = _postService.GetById(id ?? 0);
            //if (postDetails = null)
            //    return InvokeHttp404();
            var user = _blogUserService.GetProfile(postDetails.Author);
            if (user != null)
                user.Add("UserName", postDetails.Author);
            var author = (BlogUser)UtilityHelper.ConvertTo<BlogUser>(user);
            if (author.PhotoUrl != null)
            {
                var photoUrl = (author.PhotoUrl.StartsWith("http://", StringComparison.CurrentCulture) || author.PhotoUrl.StartsWith("https://", StringComparison.CurrentCulture))
                     ? author.PhotoUrl
                     : ConfigurationManager.AppSettings["blogHost"] + "/image.axd?picture=/avatars/" + author.PhotoUrl;
                author.PhotoUrl = photoUrl;
            }
            ViewBag.User = author;
            return View(postDetails);
        }

        #endregion

        #region For Website

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">author name</param>
        /// <returns></returns>
        public ActionResult Author(string id)
        {
            double? lat = null;
            double? lng = null;
            if (MySession.Cust != null)
            {
                lat = MySession.Cust.Latitude;
                lng = MySession.Cust.Longitude;
            }
            var user = _blogUserService.GetProfile(id);
            if (user != null)
                user.Add("UserName", id);
            var author = (BlogUser)UtilityHelper.ConvertTo<BlogUser>(user);
            var posts = _postService.GetByAuthorId(id, lat, lng);
            var categories = _categoryService.GetAll().ToList();
            string previousLink = "";
            if (Request.UrlReferrer != null)
            {
                previousLink = Request.UrlReferrer.OriginalString;
            }
            if (author.PhotoUrl != null)
            {
                var photoUrl = (author.PhotoUrl.StartsWith("http://", StringComparison.InvariantCulture) || author.PhotoUrl.StartsWith("https://", StringComparison.InvariantCulture))
                     ? author.PhotoUrl
                     : ConfigurationManager.AppSettings["blogHost"] + "/image.axd?picture=/avatars/" + author.PhotoUrl;
                author.PhotoUrl = photoUrl;
            }
            ViewBag.Posts = posts;
            ViewBag.Categories = categories;
            ViewBag.PreviousLink = previousLink;
            return View(author);
        }

        public ActionResult Post(int? id)
        {
            var mapper = new Mapper();
            var post = _postService.GetById(id ?? 0);
            //if (post == null)
            //    return InvokeHttp404();
            //Get comments
            var comments = _commentService.GetParentComments(post.PostID);
            //Get orther post
            //var ortherPosts = _postService.GetOrtherPostsById(id);
            List<PostExt> ortherPosts = null;
            if (MySession.Cust != null)
            {
                var localLat = MySession.Cust.Latitude;
                var localLong = MySession.Cust.Longitude;
                ortherPosts = _postService.GetOrtherPostsById(id ?? 0, localLat, localLong);
            }
            else
            {
                ortherPosts = _postService.GetOrtherPostsById(id ?? 0);
            }


            if (ortherPosts == null || !ortherPosts.Any())
            {
                ortherPosts = new List<PostExt>();
            }

            var oPosts = ortherPosts.Select(ortherPost => new SummaryPost
            {
                Id = ortherPost.PostRowID,
                CoverPhoto = string.IsNullOrEmpty(ortherPost.CoverPhoto) ? string.Format("/images/photo_company_image.png") : Utility.KalturaHelper.GetKalturaUrl(ortherPost.CoverPhoto, 233, 230),
                Title = ortherPost.Title,
                Url = string.Format("/Blog/Post/{0}", ortherPost.PostRowID)
            }).AsQueryable();
            //Get featured companies
            var featuredCompanies = _homeService.GetFeaturedCompaniesAtHomePage();
            var featuredCompaniesModels = new List<FeaturedCompanyModels>();
            foreach (var featuredCompany in featuredCompanies)
            {
                if (featuredCompany.Profile == null || featuredCompany.Profile.ProfileCompany == null)
                {
                    continue;
                }
                var media = _featuredCompanyService.GetCompanyPhotoByCompanyId(featuredCompany.Profile.ProfileID);

                var featuredCompaniesModel = new FeaturedCompanyModels
                {
                    Id = featuredCompany.ProfileID,
                    CompanyName = featuredCompany.Profile.ProfileCompany.Name,
                    City = featuredCompany.Profile.ProfileCompany.City,
                    State = featuredCompany.Profile.ProfileCompany.State,
                    ProfileId = featuredCompany.ProfileID,
                    FeaturedUrl = string.Format("/companyprofile/availability/{0}", featuredCompany.ProfileID),
                    ImagesUrl = media == null ? string.Format("/images/photo_company_image.png") : Utility.KalturaHelper.GetKalturaUrl(media.LocationData, 400, 400),
                    Rate = featuredCompany.Profile.ProfileCompany.Rate,
                    TotalReview = featuredCompany.Profile.ProfileCompany.TotalReview,
                    IsAvailability = _homeService.CheckCompanyAvailability(featuredCompany.ProfileID),
                    Street = featuredCompany.Profile.ProfileCompany.Street1,
                    Url = featuredCompany.Profile.ProfileCompany.Url,
                    Phone = featuredCompany.Profile.ProfileCompany.Phone,
                    Zip = featuredCompany.Profile.ProfileCompany.Zip

                };
                featuredCompaniesModels.Add(featuredCompaniesModel);
            }

            //Get category
            var postCategories = _postCategoryService.GetByPostId(post.PostID).ToList();
            var postCategoryStr = postCategories.Select(postCategory => _categoryService.GetById(postCategory.CategoryID)).Select(category => category).ToList();


            PostModels postModel;
            if (Request.IsAuthenticated)
            {
                postModel = mapper.Map(post, postCategoryStr, comments, featuredCompaniesModels, oPosts, 412, 230, (int)Types.MediaCropType.MainPostAfterLogin);
                ViewBag.UserName = MySession.Cust.FirstName;
                ViewBag.Email = MySession.Cust.Email;
            }
            else
            {
                postModel = mapper.Map(post, postCategoryStr, comments, featuredCompaniesModels, oPosts, 522, 283, (int)Types.MediaCropType.MainPostBeforeLogin);
            }

            // Get companies related
            postModel.CompaniesRelated = _postService.GetPostCompanies(post.PostID).ToList();

            var companyAppointmentService = (CompanyAppointmentController)EngineContext.Current.ContainerManager.ResolveOptional(typeof(CompanyAppointmentController));// (CompanyAppointmentController)DependencyResolver.Current.GetService(typeof(CompanyAppointmentController));
            string tagName = string.Empty;
            if (post != null)
            {
                var postTag = _postService.GetPostTagByPostId(post.PostID);
                if (postTag.Any(m => m.Tag=="thelatest"))
                {
                    tagName = "thelatest";
                }
            }
            postModel.PostContent = companyAppointmentService.RenderCompanyProfileTimeSlots(this, postModel.PostContent, tagName);

            var user = _blogUserService.GetProfile(post.Author);
            if (user != null)
                user.Add("UserName", post.Author);
            var author = (BlogUser)UtilityHelper.ConvertTo<BlogUser>(user);
            if (author.PhotoUrl != null)
            {
                var photoUrl = (author.PhotoUrl.StartsWith("http://") || author.PhotoUrl.StartsWith("https://"))
                     ? author.PhotoUrl
                     : ConfigurationManager.AppSettings["blogHost"] + "/image.axd?picture=/avatars/" + author.PhotoUrl;
                author.PhotoUrl = photoUrl;
            }
            ViewBag.User = author;
            return Request.IsAuthenticated ? View("PostLogin", postModel) : View(postModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Post(be_PostComment postComment, int id)
        {
            var post = _postService.GetById(id);
            postComment.BlogID = post.BlogID;
            postComment.PostID = post.PostID;
            Cust currentUser = MySession.Cust;
            postComment.Email = currentUser.Username;
            postComment.Comment = postComment.Comment.Replace("\r\n", "&lt;br/>");
            postComment.Author = "";
            postComment.CommentDate = DateTime.UtcNow;
            postComment.IsApproved = true;
            postComment.IsDeleted = false;
            postComment.IsSpam = false;
            postComment.PostCommentID = Guid.NewGuid();
            _commentService.InsertComment(postComment);
            return RedirectToAction("Post", new { id = id });
        }
        #endregion

        public IsoDateJsonResult GetComments(int? postId, int pageIndex, int limit)
        {
            var mapper = new Mapper();
            var post = _postService.GetById(postId ?? 0);
            var parentCommentQuery = _commentService.GetParentComments(post.PostID);
            var parentComments = parentCommentQuery.OrderByDescending(t => t.CommentDate).Skip(pageIndex * limit).Take(limit).ToList();
            var commentsV2 = _commentService.GetNestedComments(parentComments);
            //Get comments
            var commentQuery = _commentService.GetCommentsByPostId(post.PostID);
            var comments = commentQuery.OrderByDescending(t => t.CommentDate).Skip(pageIndex * limit).Take(limit);
            var totalComments = parentCommentQuery.Count();

            var totalPages = (totalComments + limit - 1) / limit;
            List<CommentModel> listComments = new List<CommentModel>();
            foreach (var comment in commentsV2)
            {
                listComments.Add(mapper.Map(comment));
            }
            var customJsonResult = new IsoDateJsonResult(new { success = true, data = new { comments = listComments, totalComments = totalComments, totalPages = totalPages } });
            return customJsonResult;
        }
    }
}
