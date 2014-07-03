using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Interface;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public class CategoryFeaturedHotelService: ICategoryFeaturedHotelService
    {
        private readonly IRepository<CategoryFeaturedHotel> _categoryFeaturedHotelRepository;

        public CategoryFeaturedHotelService(IRepository<CategoryFeaturedHotel> categoryFeaturedHotelRepository)
        {
            _categoryFeaturedHotelRepository = categoryFeaturedHotelRepository;
        }

        public IQueryable<CategoryFeaturedHotel> GetCategories(int featuredHotelId)
        {
            return _categoryFeaturedHotelRepository.Table.Where(t => t.FeaturedId == featuredHotelId);
        }

        public CategoryFeaturedHotel GetById(int categoryId, int featuredId)
        {
            return _categoryFeaturedHotelRepository.Table.Where(t => t.FeaturedId == featuredId && t.BeCategoryId == categoryId).FirstOrDefault();
        }

        public void Insert(int featuredId, List<CategoryFeaturedHotel> categories)
        {
            var oldCategories = GetCategories(featuredId).ToList();
            var newBeCategoryIds = categories.Select(o => o.BeCategoryId).ToList();
            var oldBeCategoryIds = oldCategories.Select(o => o.BeCategoryId).ToList();
            var onlyOldCaterories = oldCategories.Where(o => !newBeCategoryIds.Contains(o.BeCategoryId)).ToList();
            var onlyNewCategories = categories.Where(t => !oldBeCategoryIds.Contains(t.BeCategoryId)).ToList();
            foreach (var oldCategory in onlyOldCaterories)
            {
                Delelete(oldCategory);
            }
            foreach (var newCategory in onlyNewCategories)
            {
                Insert(newCategory);
            }
        }

        public void Insert(CategoryFeaturedHotel category)
        {
            _categoryFeaturedHotelRepository.Insert(category);
        }

        public void Delelete(CategoryFeaturedHotel category)
        {
            _categoryFeaturedHotelRepository.Delete(category);
        }

        public IQueryable<CategoryFeaturedHotel> FilterByCategory(int categoryId)
        {
            return _categoryFeaturedHotelRepository.Table.Where(t =>t.BeCategoryId == categoryId);
        }
    }
}
