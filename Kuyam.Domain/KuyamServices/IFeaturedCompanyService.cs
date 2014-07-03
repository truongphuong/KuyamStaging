using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.KuyamServices
{
    public interface IFeaturedCompanyService
    {
        IQueryable<FeaturedCompany> Get();
        Medium GetCompanyPhotoByCompanyId(int profileId);
        List<Medium> GetCompanyPhotoByCompanyId(List<int> profileId);
    }
}
