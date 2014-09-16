using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.SearchServices
{
    public interface ISearchService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="categoriesId"></param>
        /// <param name="keySearch"></param>
        /// <param name="categoryId"></param>
        /// <param name="currentLat"></param>
        /// <param name="currentLon"></param>
        /// <param name="distance"></param>
        /// <param name="custId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<CompanyProfileSearch> CompanySearchForWeb(
            out int totalRecords,
            List<string> categoriesId,
            string keySearch = null,
            int? categoryId = null,
            double? currentLat = null,
            double? currentLon = null,
            double? distance = null,
            int? custId = null,
            int pageIndex = 0,
            int pageSize = 2147483647);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        ProfileCompanyDetails GetCompanyProfileDetials(int profileId, int categoryId = 0);

        /// <summary>
        /// /
        /// </summary>
        /// <param name="serviceComapanyId"></param>
        /// <returns></returns>
        List<CompanyEmployee> GetEmployeeByServiceCompanyId(int serviceComapanyId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="CategoryId"></param>
        /// <returns></returns>
        List<ServiceCompany> GetServiceCompanyByCategoryId(int profileId, int CategoryId);

    }
}
