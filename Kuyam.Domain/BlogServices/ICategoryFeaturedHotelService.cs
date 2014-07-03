using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public interface ICategoryFeaturedHotelService
    {
        IQueryable<CategoryFeaturedHotel> GetCategories(int featuredHotelId);
        CategoryFeaturedHotel GetById(int categoryId, int featuredId);
        IQueryable<CategoryFeaturedHotel> FilterByCategory(int categoryId);
        void Insert(int featuredId, List<CategoryFeaturedHotel> categories);
        void Insert(CategoryFeaturedHotel category);
        void Delelete(CategoryFeaturedHotel category);
    }
}
