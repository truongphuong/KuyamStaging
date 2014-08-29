using Kuyam.Database;
using Kuyam.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        #region Fields
        private readonly DbContext _dbContext;
        private readonly IRepository<Service> _serviceRepository;
        #endregion

        #region Ctor

        public CategoryService(DbContext dbContext,IRepository<Service> serviceRepository)
        {
            this._dbContext = dbContext;
            this._serviceRepository = serviceRepository;
        }

        #endregion

        /// <summary>
        ///  Get list active categories
        /// </summary>
        /// <returns></returns>
        public List<Service> GetSequenceCategories()
        {
            var query = _serviceRepository.Table.Where(m => !m.ParentServiceID.HasValue
                && (m.Status.HasValue && m.Status.Value)
                && m.Sequence.HasValue);
            return query.OrderBy(o=>o.Sequence).ToList();
        }


        public List<Service> GetActiveCategories()
        {            
            var categories = _dbContext.SqlQuery<Service>("GetSequenceCategories");
            return categories.ToList();
        }
    }
}
