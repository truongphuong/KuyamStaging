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
        private readonly IRepository<ProfileCompany> _profileCompany;

        public ProfileCompanyService(IRepository<ProfileCompany> profileCompany)
        {
            _profileCompany = profileCompany;
        }

        public ProfileCompany GetByProfileId(int profileId)
        {
            var profileCompany = _profileCompany.Table.FirstOrDefault(t => t.ProfileID == profileId);
            int totalReviews = 0;
            double rating = 0;
            if (profileCompany != null)
            {
                var serviceCompany = profileCompany.ServiceCompanies;
                totalReviews = serviceCompany.Select(m => m.Ratings.Count).Sum();
                rating = serviceCompany.Select(m => m.Ratings.Sum(r => r.RatingValue)).Sum().Value;

                //if (serviceCompany != null && serviceCompany.Count() > 0)
                //{
                //    foreach (ServiceCompany item in serviceCompany)
                //    {
                //        totalReviews += item.Ratings.Count;
                //        rating += item.Ratings.Sum(m => m.RatingValue).Value;
                //    }
                //}
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
            return _profileCompany.Table.Where(p => p.CompanyStatusID == (int)Types.CompanyStatus.Active).OrderBy(c => c.Name);
        }

    }
}
