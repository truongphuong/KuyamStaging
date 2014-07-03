using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public interface ICategoryFeaturedService
    {
        void Insert(CategoryFeatured category);
        void Update(CategoryFeatured category);
        List<CategoryFeatured> GetCategoriesFromFeaturedCompany(int featuredCompany);
        List<CategoryFeatured> GetFeaturedCompaniesFromCategory(int categoryId);
        void Delelete(CategoryFeatured category);
        CategoryFeatured GetById(int categoryId, int featuredcompanyId);
        CategoryFeatured GetByPosition(int categoryId, int order);        
    }
}
