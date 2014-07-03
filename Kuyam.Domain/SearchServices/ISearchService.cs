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
        List<ProfileCompany> SearchCompanies(
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
            int pageSize = 2147483647);
    }
}
