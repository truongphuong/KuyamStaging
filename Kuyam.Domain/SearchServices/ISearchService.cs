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
        List<CompanyProfileSearch> CompanySearchForWeb(
            out int totalRecords ,
            string keySearch = null,
            int? categoryId = null,
            double? currentLat = null,
            double? currentLon = null,
            double? distance = null,
            int? custId = null,
            int pageIndex = 0,
            int pageSize = 2147483647);
    }
}
