using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Interface;
using Kuyam.Database;

namespace Kuyam.Domain.KuyamServices
{
    public class FeaturedCompanyService: IFeaturedCompanyService
    {
        #region Private fields
        private readonly  IRepository<FeaturedCompany> _featuredCompanyService;
        #endregion

        public FeaturedCompanyService(IRepository<FeaturedCompany> featuredCompanyService)
        {
            _featuredCompanyService = featuredCompanyService;
        }

        public IQueryable<FeaturedCompany> Get()
        {
            return _featuredCompanyService.Table;
        }
        public  Medium GetCompanyPhotoByCompanyId(int profileId){
            return DAL.GetCompanyPhotoByCompanyID(profileId); 
        }

        public List<Medium> GetCompanyPhotoByCompanyId(List<int> profileId)
        {
            return DAL.GetCompanyPhotoByCompanyID(profileId);
        }
    }
}
