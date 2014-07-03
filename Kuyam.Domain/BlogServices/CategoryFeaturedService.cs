using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.BlogServices
{
    public class CategoryFeaturedService: ICategoryFeaturedService
    {
        private readonly IRepository<CategoryFeatured> _categoryFeature;

        public CategoryFeaturedService(IRepository<CategoryFeatured> categoryFeature)
        {
            _categoryFeature = categoryFeature;
        }

        public void Delelete(CategoryFeatured category)
        {
            _categoryFeature.Delete(category);
        }

        public List<CategoryFeatured> GetCategoriesFromFeaturedCompany(int featuredCompany)
        {
            return _categoryFeature.Table.Where(t => t.ProfileId == featuredCompany).ToList();
        }

        public void Insert(CategoryFeatured category)
        {
            _categoryFeature.Insert(category);
        }

        public CategoryFeatured GetById(int categoryId, int featuredcompanyId)
        {
            return _categoryFeature.Table.Where(t => t.ProfileId == featuredcompanyId && t.BeCategoryId == categoryId).FirstOrDefault();
        }

       public List<CategoryFeatured> GetFeaturedCompaniesFromCategory(int categoryId)
        {
            return _categoryFeature.Table.Where(t => t.BeCategoryId == categoryId).ToList();
        }

        public CategoryFeatured GetByPosition(int categoryId, int order)
        {
            return _categoryFeature.Table.Where(t => t.Order == order && t.BeCategoryId == categoryId).FirstOrDefault();
        }

        public void Update(CategoryFeatured category)
        {
            _categoryFeature.Update(category);
        }
    }
}
