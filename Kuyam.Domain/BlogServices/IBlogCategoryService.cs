using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using System.Linq.Expressions;

namespace Kuyam.Domain.BlogServices
{
    public interface IBlogCategoryService
    {
        be_Categories GetById(Guid id);
        IQueryable<be_Categories> Get();
        IQueryable<be_Categories> GetAll();
        IQueryable<be_Categories> FilterByOnlyHavePosts();
        List<be_Categories> GetCategoriesAtHomePage();
    }
}
