using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using System.Linq.Expressions;

namespace Kuyam.Domain.BlogServices
{
    public class BlogCategoryService: IBlogCategoryService
    {
        #region Private Fields
        private readonly IRepository<be_Categories> _blogCategoryRepository;
        private readonly IRepository<be_PostCategory> _postCategoryRepository;
        private readonly IRepository<be_Posts> _postRepository;
        #endregion
        public BlogCategoryService(IRepository<be_Categories> blogCategoryRepository, IRepository<be_PostCategory> postCategoryRepository,
            IRepository<be_Posts> postRepository)
        {
            _blogCategoryRepository = blogCategoryRepository;
            _postCategoryRepository = postCategoryRepository;
            _postRepository = postRepository;
        }

        public be_Categories GetById(Guid id)
        {
            return _blogCategoryRepository.Table.Where(t => t.CategoryID == id).FirstOrDefault();
        }

        public IQueryable<be_Categories> Get()
        {
            return FilterByOnlyHavePosts().OrderBy(t => t.Sequence).AsQueryable();
        }

        public IQueryable<be_Categories> GetAll()
        {
            return _blogCategoryRepository.Table.OrderBy(t=>t.Sequence).AsQueryable();
        }

        public IQueryable<be_Categories> FilterByOnlyHavePosts()
        {
            var publishedPosts = _postRepository.Table.Where(t => t.IsPublished.Value && t.IsDeleted == false).Select(t=>t.PostID); 
            var postCategoies = _postCategoryRepository.Table.Where(t=>publishedPosts.Contains(t.PostID)).Select(t=>t.CategoryID).Distinct();
            return GetAll().Where(t=>postCategoies.Contains(t.CategoryID));
        }

        public List<be_Categories> GetCategoriesAtHomePage()
        {
            return Get().Take(7).ToList();
        }

    }
}
