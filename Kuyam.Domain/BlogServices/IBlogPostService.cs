using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Kuyam.Database;
using System.Data.Entity.Spatial;
using Kuyam.Database.Extensions;

namespace Kuyam.Domain.BlogServices
{
    public interface IBlogPostService
    {
        be_Posts GetById(Guid id);
        be_Posts GetById(int id);
        List<be_PostTag> GetPostTagByPostId(Guid postId);
        /// <summary>
        /// Get All Posts
        /// </summary>
        /// <returns></returns>
        IQueryable<be_Posts> GetAll();

        PostExt GetPreviousById(DateTime dt, int id, double? lat, double? lng, DbGeography geoGraphy = null);
        List<PostExt> GetOrtherPostsById(int id);
        PostExt GetBySlug(DateTime createDateTime, string slug);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="geoGraphy"></param>
        /// <returns></returns>
        IQueryable<PostExt> GetAll(double? lat, double? lng, DbGeography geoGraphy = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="geoGraphy"></param>
        /// <returns></returns>
        IQueryable<PostExt> GetPosts(double? lat, double? lng, DbGeography geoGraphy = null);
        /// <summary>
        /// Get Recent posts exclude featured post
        /// </summary>
        /// <returns></returns>
        List<PostExt> GetRecentPosts(int topCount);
        List<PostExt> GetByAuthorId(string userName, double? lat, double? lon, DbGeography geoGraphy = null);
        List<PostExt> FilterByCategory(Guid categoryId, double? lat, double? lng, DbGeography geoGraphy = null);
        List<PostExt> GetListPost(double? lat, double? lng, DbGeography geoGraphy = null);
        PostExt GetFeaturedPost();
        List<PostExt> GetRecentPosts(double? lat, double? lng, DbGeography geoGraphy = null);
        List<PostExt> GetOrtherPostsById(int id, double? lat, double? lng, DbGeography geoGraphy = null);
        List<be_PostCompanies> GetPostCompanies(Guid postId);
        List<PostExt> GetPostByCategoryId(Guid categoryId);

        List<PostExt> Get20LastestPostByCategoryId(Guid categoryId, Guid featureId);

        List<be_Posts> GetTest();
      
    }
}
