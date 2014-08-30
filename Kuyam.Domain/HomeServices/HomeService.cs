using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using Kuyam.Domain.BlogServices;
using Kuyam.Database.Extensions;
using Kuyam.Utility;

namespace Kuyam.Domain.HomeServices
{
    public class HomeService : IHomeService
    {
        #region Fields
        private IRepository<FeaturedCompany> _featuredCompanyRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<CategoryFeatured> _categoryFeaturedRepository;
        private readonly IRepository<be_Posts> _postRepository;
        private readonly IRepository<be_PostCategory> _postCategorysRepository;
        private readonly IRepository<be_Categories> _categoriesRepository;
        private readonly IRepository<EmployeeHour> _employeeHourRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;
        private readonly IBlogPostService _postService;
        private readonly CompanySearchService _companySearchService;
        #endregion

        #region Ctor

        public HomeService(IRepository<FeaturedCompany> featuredCompanyRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<be_Posts> postRepository,
            IRepository<be_PostCategory> postCategorysRepository,
            IRepository<be_Categories> categoriesRepository,
            IRepository<CategoryFeatured> categoryFeaturedRepository,
            IRepository<EmployeeHour> employeeHourRepository,
            IRepository<CompanyEmployee> companyEmployeeRepository,
            IRepository<Service> serviceRepository,
            IBlogPostService postService,
            CompanySearchService companySearchService)
        {
            _featuredCompanyRepository = featuredCompanyRepository;
            _profileCompanyRepository = profileCompanyRepository;
            _postRepository = postRepository;
            _postCategorysRepository = postCategorysRepository;
            _categoriesRepository = categoriesRepository;
            _categoryFeaturedRepository = categoryFeaturedRepository;
            _employeeHourRepository = employeeHourRepository;
            _companyEmployeeRepository = companyEmployeeRepository;
            _serviceRepository = serviceRepository;
            this._postService = postService;
            _companySearchService = companySearchService;
        }
        #endregion

        #region Public Functions

        public List<ProfileCompany> GetFeaturedCompaniesAtHomePage(int categoryId = 0)
        {
            var query = _profileCompanyRepository.Table;

            if (categoryId > 0)
            {
                query = (from cpf in _profileCompanyRepository.Table
                         join cf in _categoryFeaturedRepository.Table on cpf.ProfileID equals cf.ProfileId
                         where cf.BeCategoryId == categoryId && cpf.CompanyStatusID == (int)Types.CompanyStatus.Active
                         select cpf).Take(3);
            }
            else
            {
                query = (from cpf in _profileCompanyRepository.Table
                         join fcp in _featuredCompanyRepository.Table on cpf.ProfileID equals fcp.ProfileID
                         where fcp.priority > 0 && cpf.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                         orderby fcp.priority
                         select cpf).Take(3);
            }


            foreach (var profile in query)
            {
                int totalReview = 0;
                double valueRanting = 0;

                if (profile.ServiceCompanies != null)
                {
                    var serviceCompany = profile.ServiceCompanies;
                    if (serviceCompany != null)
                    {
                        totalReview = serviceCompany.Select(m => m.Ratings.Count).Sum();
                        valueRanting = serviceCompany.Select(m => m.Ratings.Sum(r => r.RatingValue)).Sum().Value;
                    }
                    //foreach (var item in profile.ServiceCompanies)
                    //{
                    //    totalReview += item.Ratings.Count;
                    //    valueRanting += item.Ratings.Sum(m => m.RatingValue).Value;
                    //}
                }
                if (totalReview <= 0) continue;
                profile.TotalReview = totalReview;
                profile.Rate = Math.Round((valueRanting / totalReview));
            }
            return query.ToList();
        }

        public List<PostExt> GetPostsAtHomePage(Guid categoryId, Guid? featurePostID, double? lat, double? lng)
        {
            var query = _postService.GetPosts(lat, lng);
            var postList = _postCategorysRepository.Table.Where(t => t.CategoryID == categoryId).Select(p => p.PostID);
            var posts = query.Where(p => postList.Contains(p.PostID) && p.PostID != featurePostID).OrderByDescending(t => t.DateCreated).Take(18).ToList();
            return posts;
        }

        public PostExt GetFeaturedPostAtHomePage(Guid? featuredPostID, double? lat, double? lng)
        {
            var query = _postService.GetPosts(lat, lng);
            return query.FirstOrDefault(p => p.PostID == featuredPostID);
        }

        public bool CheckCompanyAvailability(int profileId)
        {
            var query = (from eh in _employeeHourRepository.Table
                         join ce in _companyEmployeeRepository.Table on eh.CompanyEmployeeID equals ce.EmployeeID
                         where ce.ProfileCompanyID == profileId
                         select eh).Count();
            return query > 0;
        }

        public be_Categories GeCategoryById(int id)
        {
            return _categoriesRepository.Table.FirstOrDefault(t => t.CategoryRowID == id);
        }

        public List<Service> GetListCategoryForHomePage()
        {
            return _serviceRepository.Table.Where(x => x.ParentServiceID == null && x.Sequence.HasValue && x.Sequence.Value > 0).OrderBy(x => x.Sequence).ToList();
        }

        public List<CompanyProfileSearch> GetCompaniesAtHomePage(double lat, double lon, int categoryId = 0, double distance = 80.467)
        {
            var take = 8;
            var totalItems = 0;

            var pcList = _companySearchService.GetProfileCompaniesWebSite(categoryId, 0, decimal.MaxValue, DateTime.Today, DateTime.Today,
                    false, string.Empty, lat, lon, distance, 0, 0, take, out totalItems);


            var appointments = _companySearchService.GetAppoinmentsByProfileIds(pcList.Select(a => a.ProfileID).ToList());

            foreach (var companyProfileSearch in pcList)
            {
                _companySearchService.TransformEmployeeHours(companyProfileSearch);
                _companySearchService.TransformInstructorClassSchedulerHours(companyProfileSearch);
                _companySearchService.TransformCompanyHours(companyProfileSearch);
                _companySearchService.TransformEvents(companyProfileSearch);
                companyProfileSearch.Appointments = appointments.Where(m => m.ProfileId == companyProfileSearch.ProfileID).ToList();
                companyProfileSearch.CompanyAvailableTimeSlots = _companySearchService.GetCompanyAvailableTimeSlots(companyProfileSearch);
            }


            return pcList;
        }

        #endregion



    }
}
