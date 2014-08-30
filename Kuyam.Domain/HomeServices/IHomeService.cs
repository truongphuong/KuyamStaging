using Kuyam.Database;
using Kuyam.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.HomeServices
{
    public interface IHomeService
    {
        List<ProfileCompany> GetFeaturedCompaniesAtHomePage(int categoryId = 0);
        List<PostExt> GetPostsAtHomePage(Guid categoryId, Guid? featurePostID, double? lat, double? lng);
        PostExt GetFeaturedPostAtHomePage(Guid? featuredPostID, double? lat, double? lng);
        bool CheckCompanyAvailability(int profileId);
        be_Categories GeCategoryById(int id);

        List<Service> GetListCategoryForHomePage();

        List<CompanyProfileSearch> GetCompaniesAtHomePage(double lat, double lon, int categoryId = 0, double distance=80.467);
    }
}
