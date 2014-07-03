using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Spatial;
using Kuyam.Repository.Interface;
using Kuyam.Database;
using Kuyam.Database.Extensions;

namespace Kuyam.Domain.BlogServices
{
    public class BlogPostService : IBlogPostService
    {
        #region Fields

        private readonly IRepository<be_Blogs> _blogRepository;
        private readonly IRepository<be_Posts> _postRepository;
        private readonly IRepository<be_PostComment> _postCommentRepository;
        private readonly IRepository<be_Profiles> _profilesRepository;
        private readonly IRepository<be_PostCategory> _postCategorysRepository;
        private readonly IRepository<be_Categories> _categoriesRepository;
        private readonly IRepository<be_PostCompanies> _postCompanyRepository;
        private readonly IRepository<be_PostTag> _postTagRepository;
        #endregion

        #region Ctor

        public BlogPostService(IRepository<be_Blogs> blogRepository,
            IRepository<be_Posts> postRepository,
            IRepository<be_PostComment> postCommentRepository,
            IRepository<be_Profiles> profilesRepository,
            //IBlogPostCategoryService postCategoryService, IBlogPostService postService, 
            IRepository<be_PostCategory> postCategorysRepository,
            IRepository<be_PostCompanies> postCompanyRepository,
            IRepository<be_PostTag> postTagRepository,
            IRepository<be_Categories> categoriesRepository)
        {
            this._blogRepository = blogRepository;
            this._postRepository = postRepository;
            this._postCommentRepository = postCommentRepository;
            this._profilesRepository = profilesRepository;
            _postCategorysRepository = postCategorysRepository;
            _postCompanyRepository = postCompanyRepository;
            this._postTagRepository = postTagRepository;
            this._categoriesRepository = categoriesRepository;
        }

        #endregion


        public be_Posts GetById(Guid id)
        {
            return _postRepository.Table.Where(t => t.PostID == id).FirstOrDefault();
        }

        public be_Posts GetById(int id)
        {
            return GetAll().FirstOrDefault(t => t.PostRowID == id);
        }

        public IQueryable<be_Posts> GetAll()
        {
            return _postRepository.Table.Where(t => t.IsDeleted == false && t.IsPublished.HasValue && t.IsPublished.Value);
        }

        public IQueryable<PostExt> GetAll(double? lat, double? lng, DbGeography geoGraphy = null)
        {
            var query = GetPosts();
            if (lat.HasValue && lng.HasValue)
            {
                var dbGeography = LocatorHelper.CreatePoint(lat.Value, lng.Value);
                //var query1 = query.Where(a => !a.IsLocation);
                query = query.Where(a => (!a.IsLocation || (a.GeoLocation == null || (a.Latitude == 0.0 && a.Longitude == 0.0) || a.GeoLocation.Distance(dbGeography) <= (a.Radius * 1609.344))));


                //query = query.Where(a => (!a.IsLocation || (a.GeoLocation == null || (a.Latitude == 0.0 && a.Longitude == 0.0) || a.GeoLocation.Distance(dbGeography) <= (a.Radius * 1609.344)))
                //    || (geoGraphy != null && a.GeoLocation.Distance(geoGraphy) <= (a.Radius * 1609.344)));
                //query = query.Where(a => (!a.IsLocation || (lat == 0.0 && lng == 0.0) || (a.Latitude == 0.0 && a.Longitude == 0.0)
                //            || ((SqlSpatialFunctions.PointGeography(a.Latitude, a.Longitude, 4326).Distance(dbGeography) / 1609.344) <= a.Radius)));
            }

            return query;
        }


        public PostExt GetPreviousById(DateTime dt, int id, double? lat, double? lng, DbGeography geoGraphy = null)
        {
            var queryAble = GetPosts(lat, lng, geoGraphy);
            var querry = queryAble.Where(t => t.DateCreated == dt).OrderByDescending(m => m.DateCreated);

            if (querry.Count() > 1)
            {
                var result = querry.OrderBy(m => m.PostRowID).FirstOrDefault(m => m.PostRowID > id);
                if (result != null)
                    return result;
            }
            return queryAble.OrderByDescending(m => m.DateCreated).FirstOrDefault(t => t.DateCreated < dt);
        }

        public List<PostExt> GetByAuthorId(string userName, double? lat, double? lng, DbGeography geoGraphy = null)
        {
            return GetPosts(lat, lng, geoGraphy).Where(m => m.Author.ToLower() == userName.ToLower()).ToList();
        }


        public IQueryable<PostExt> GetPosts()
        {
            return GetAll()
                .Select(p => new PostExt
                {
                    PostRowID = p.PostRowID,
                    PostID = p.PostID,
                    CoverPhoto = p.CoverPhoto,
                    Title = p.Title,
                    Description = p.Description,
                    Caption = p.Caption,
                    AlowChatToBook = p.AlowChatToBook,
                    Slug = p.Slug,
                    Author = p.Author,
                    DateCreated = p.DateCreated,
                    IsFeatured = p.IsFeatured,
                    IsLocation = p.IsLocation,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Radius = p.Radius,
                    GeoLocation = p.GeoLocation,
                    IsEvent = p.IsEvent 
                });
        }

        public List<PostExt> GetRecentPosts(int topCount)
        {
            return GetPosts().Where(t => t.IsFeatured.HasValue == false || t.IsFeatured.Value == false).OrderByDescending(t => t.DateCreated).Take(topCount).ToList();
        }


        public List<PostExt> GetTopLaSpotRecentPosts(int topCount)
        {
            return GetPosts().Where(t => (t.IsFeatured.HasValue == false || t.IsFeatured.Value == false) ).OrderByDescending(t => t.DateCreated).Take(topCount).ToList();
        }


        public PostExt GetFeaturedPost()
        {
            return GetPosts().FirstOrDefault(t => t.IsFeatured.HasValue && t.IsFeatured.Value);
        }

        public PostExt GetBySlug(DateTime createDateTime, string slug)
        {
            DateTime dt = createDateTime.Date.AddHours(24);
            return GetPosts().Where(t => (createDateTime == DateTime.MinValue || t.DateCreated > createDateTime.Date && t.DateCreated <= dt)
                && t.Slug.ToLower() == slug.ToLower()).OrderByDescending(m => m.DateCreated).FirstOrDefault();
        }

        public List<PostExt> GetOrtherPostsById(int id)
        {

            var currentPost = GetPosts().FirstOrDefault(t => t.PostRowID == id);

            var querryTemp = GetPosts().Where(m => m.DateCreated <= currentPost.DateCreated && m.PostRowID != currentPost.PostRowID).OrderByDescending(m => m.DateCreated).Take(20).ToList();

            var query = querryTemp.Where(m => m.DateCreated == currentPost.DateCreated).ToList();
            if (query.Count() > 0)
            {
                var listId = query.Where(m => m.PostRowID < id).Select(m => m.PostRowID).ToList();
                querryTemp.RemoveAll(m => listId.Contains(m.PostRowID));
                return querryTemp.OrderByDescending(m => m.DateCreated).ThenBy(m => m.PostRowID).Take(4).ToList();
            }
            return querryTemp.OrderByDescending(m => m.DateCreated).ThenBy(m => m.PostRowID).Take(4).ToList();

        }

        public List<PostExt> GetRecentPosts(double? lat, double? lng, DbGeography geoGraphy = null)
        {
            return GetPosts(lat, lng, geoGraphy).Where(t => t.IsFeatured.HasValue == false || t.IsFeatured.Value == false).OrderByDescending(t => t.DateCreated).Take(18).ToList();
        }


        public List<PostExt> GetOrtherPostsById(int id, double? lat, double? lng, DbGeography geoGraphy = null)
        {

            var currentPost = GetPosts(lat, lng, geoGraphy).FirstOrDefault(t => t.PostRowID == id);
            if (currentPost == null)
                return null;
            var querryTemp = GetPosts(lat, lng, geoGraphy).Where(m => m.DateCreated <= currentPost.DateCreated && m.PostRowID != currentPost.PostRowID).OrderByDescending(m => m.DateCreated).Take(20).ToList();

            var query = querryTemp.Where(m => m.DateCreated == currentPost.DateCreated).ToList();
            if (query.Count() > 0)
            {
                var listId = query.Where(m => m.PostRowID < id).Select(m => m.PostRowID).ToList();
                querryTemp.RemoveAll(m => listId.Contains(m.PostRowID));
                return querryTemp.OrderByDescending(m => m.DateCreated).ThenBy(m => m.PostRowID).Take(4).ToList();
            }
            return querryTemp.OrderByDescending(m => m.DateCreated).ThenBy(m => m.PostRowID).Take(4).ToList();
        }


        public IQueryable<PostExt> GetPosts(double? lat, double? lng, DbGeography geoGraphy = null)
        {
            return GetAll(lat, lng, geoGraphy);
        }


        public List<PostExt> FilterByCategory(Guid categoryId, double? lat, double? lng, DbGeography geoGraphy = null)
        {
            var postList = _postCategorysRepository.Table.Where(t => t.CategoryID == categoryId);
            var postFeatureOfcategory = _categoriesRepository.Table.Where(m => m.CategoryID == categoryId).FirstOrDefault();
            return GetPosts(lat, lng, geoGraphy).Where(t => postList.Any(p => p.PostID == t.PostID)).OrderByDescending(f => (postFeatureOfcategory.FeaturedPostID == f.PostID)).ThenByDescending(d => d.DateCreated).ToList();
        }

        public List<PostExt> GetListPost(double? lat, double? lng, DbGeography geoGraphy = null)
        {
            var query = GetPosts(lat, lng, geoGraphy);
            return query.OrderByDescending(m => m.IsFeatured).ThenByDescending(n => n.DateCreated).Take(20).ToList();
        }

        public List<be_PostCompanies> GetPostCompanies(Guid postId)
        {
            return _postCompanyRepository.Table.Where(p => p.PostId == postId).ToList();
        }

        public List<PostExt> GetPostByCategoryId(Guid categoryId)
        {
            var query = _postCategorysRepository.Table.Where(t => t.CategoryID == categoryId);
            return GetPosts().Where(m => query.Any(n => n.PostID == m.PostID)).ToList();

            //var query = from c in _postCategorysRepository.Table
            //            join p in _postRepository.Table on c.PostID equals p.PostID
            //            select new PostExt
            //    {
            //        PostRowID = p.PostRowID,
            //        PostID = p.PostID,
            //        CoverPhoto = p.CoverPhoto,
            //        Title = p.Title,
            //        Description = p.Description,
            //        Caption = p.Caption,
            //        AlowChatToBook = p.AlowChatToBook,
            //        Slug = p.Slug,
            //        Author = p.Author,
            //        DateCreated = p.DateCreated,
            //        IsFeatured = p.IsFeatured,
            //        IsLocation = p.IsLocation,
            //        Latitude = p.Latitude,
            //        Longitude = p.Longitude,
            //        Radius = p.Radius,
            //        GeoLocation = p.GeoLocation
            //    };
            //return query.ToList();
        }

        public List<PostExt> Get20LastestPostByCategoryId(Guid categoryId, Guid featureId)
        {
            // not include feature post

            var query = _postCategorysRepository.Table.Where(t => t.CategoryID == categoryId);
            return GetPosts().Where(m => query.Any(n => n.PostID == m.PostID && 
                (featureId == Guid.Empty || n.PostID != featureId))).OrderByDescending(m => m.DateCreated).Take(20).ToList();            
        }


        public List<be_PostTag> GetPostTagByPostId(Guid postId)
        {
            return _postTagRepository.Table.Where(m => m.PostID == postId).ToList();
        }


        public List<be_Posts> GetTest()
        {
            return _postRepository.Table.OrderBy(m => m.PostRowID).Skip(0).Take(20).ToList();
        }
    }
}
