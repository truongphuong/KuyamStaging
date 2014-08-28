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
        private readonly CompanySearchService _companySearchService;
        #endregion
        public SearchService(DbContext dbContext, CompanySearchService companySearchService)
        {
            this._dbContext = dbContext;
            this._companySearchService = companySearchService;
        }

        public List<CompanyProfileSearch> CompanySearchForWeb(
            out int totalRecords,
            string keySearch = null,
            int? categoryId = null,
            double? currentLat = null,
            double? currentLon = null,
            double? distance = null,
            int? custId = null,
            int pageIndex = 0,
            int pageSize = 2147483647)
        {
            totalRecords = 0;
            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;
            var list = _dbContext.SqlQuery<CompanyProfileSearch>("CompanySearchForWeb @KeySearch,@ServiceID,@CurrentLat,@CurrentLong,@Distance,@CustID,@PageIndex,@PageSize, @TotalRecords out",
                new SqlParameter("KeySearch", keySearch),
                 new SqlParameter("ServiceID", categoryId),
                 new SqlParameter("CurrentLat", currentLat),
                 new SqlParameter("CurrentLong", currentLon),
                 new SqlParameter("Distance", distance),
                 new SqlParameter("CustID", custId),
                 new SqlParameter("PageIndex", pageIndex),
                 new SqlParameter("PageSize", pageSize),
                     pTotalRecords
                 ).ToList();

            totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;

            var appointments = _companySearchService.GetAppoinmentsByProfileIds(list.Select(a => a.ProfileID).ToList());

            foreach (var item in list)
            {                
                _companySearchService.TransformEmployeeHours(item);
                _companySearchService.TransformInstructorClassSchedulerHours(item);
                _companySearchService.TransformCompanyHours(item);
                _companySearchService.TransformEvents(item);
                item.Appointments = appointments.Where(m => m.ProfileId == item.ProfileID).ToList();
                item.CompanyAvailableTimeSlots = _companySearchService.GetCompanyAvailableTimeSlots(item);
            }

            return list;
        }
    }
}
