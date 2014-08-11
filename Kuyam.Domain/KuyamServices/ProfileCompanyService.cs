using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.KuyamServices
{
    public class ProfileCompanyService : IProfileCompanyService
    {

        public ProfileCompanyService(IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<Service> serviceRepository)
        {
            this._profileCompanyRepository = profileCompanyRepository;
            this._serviceRepository = serviceRepository;
        }

        private readonly IRepository<ProfileCompany> _profileCompanyRepository;

        private readonly IRepository<Service> _serviceRepository;

        public List<Service> GetParentService()
        {
            return _serviceRepository.Table.Where(x => (x.ParentServiceID == null) && (x.Status == null || x.Status == true)).OrderBy(o => o.ServiceName).ToList();
        }

        public ProfileCompany GetByProfileId(int profileId)
        {
            var profileCompany = _profileCompanyRepository.Table.FirstOrDefault(t => t.ProfileID == profileId);
            int totalReviews = 0;
            double rating = 0;
            if (profileCompany != null)
            {
                var serviceCompany = profileCompany.ServiceCompanies;
                totalReviews = serviceCompany.Select(m => m.Ratings.Count).Sum();
                rating = serviceCompany.Select(m => m.Ratings.Sum(r => r.RatingValue)).Sum().Value;             
            }

            if (totalReviews > 0)
            {
                profileCompany.TotalReview = totalReviews;
                profileCompany.Rate = Math.Round((rating / totalReviews));
            }
            return profileCompany;
        }

        public IQueryable<ProfileCompany> GetAll()
        {
            return _profileCompanyRepository.Table.Where(p => p.CompanyStatusID == (int)Types.CompanyStatus.Active).OrderBy(c => c.Name);
        }

    }
}
