using Kuyam.Database;
using Kuyam.Domain.PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.SearchServices
{
    public class SearchService : ISearchService
    {

        #region Fields
        private readonly DbContext _dbContext;
        #endregion
        public SearchService(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public List<ProfileCompany> SearchCompanies(
            out int totalRecords,
            string key = null,
            int? categoryId = null,
            double? distance = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            DateTime? hourMin = null,
            DateTime? hourMax = null,
            bool? isAvailable = false,
            int sortBy = 0,
            int pageIndex = 0,
            int pageSize = 2147483647)
        {
            totalRecords = 0;
            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;
            var list = _dbContext.SqlQuery<ProfileCompany>("GetAllCompaniesTest @TotalRecords out", pTotalRecords).ToList();
           
            totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<ProfileCompany>(list, pageIndex, pageSize, totalRecords);
        }
    }
}
