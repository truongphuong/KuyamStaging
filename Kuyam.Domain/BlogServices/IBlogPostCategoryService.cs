using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public interface IBlogPostCategoryService
    {
        IQueryable<be_PostCategory> GetByPostId(Guid id);
        IQueryable<be_PostCategory> GetAll();
        IQueryable<be_PostCategory> FilterByCategoryId(Guid categoryId);
    }
}
