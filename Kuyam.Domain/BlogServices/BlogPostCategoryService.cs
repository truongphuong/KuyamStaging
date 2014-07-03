using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.BlogServices
{
    public class BlogPostCategoryService: IBlogPostCategoryService
    {
        #region Private Fields
        private readonly IRepository<be_PostCategory> _postCategoryRepository;
        #endregion
        public BlogPostCategoryService(IRepository<be_PostCategory> postCategoryRepository)
        {
            _postCategoryRepository = postCategoryRepository;
        }

        public IQueryable<be_PostCategory> GetByPostId(Guid id)
        {
            return _postCategoryRepository.Table.Where(t => t.PostID == id);
        }

        public IQueryable<be_PostCategory> GetAll()
        {
            return _postCategoryRepository.Table.AsQueryable();
        }

        public IQueryable<be_PostCategory> FilterByCategoryId(Guid categoryId)
        {
            return _postCategoryRepository.Table.Where(t => t.CategoryID == categoryId);
        }
    }
}
