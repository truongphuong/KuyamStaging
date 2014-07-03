using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.LandingePageServices
{
    public class LandingPageServices:ILandingPageServices
    {
        #region Contructors

        public LandingPageServices(IRepository<LandingPage> landingPageRepository, IRepository<be_PostMedia> postMediaRepository, IRepository<LandingPageCompany> landingPageCompanyRepository)
        {
            _landingPageRepository = landingPageRepository;
            _postMediaRepository = postMediaRepository;
            _landingPageCompanyRepository = landingPageCompanyRepository;
        }

        #endregion


        #region Private Properties

        private readonly IRepository<LandingPage> _landingPageRepository;
        private readonly IRepository<be_PostMedia> _postMediaRepository;
        private readonly IRepository<LandingPageCompany> _landingPageCompanyRepository;
        #endregion


        #region Public Methods

        /// <summary>
        /// Gets the landing pages.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <param name="status">The status. Default is 0 ~ get all</param>
        /// <returns></returns>
        public IQueryable<LandingPage> GetLandingPages(string searchKey, int status = 0)
        {
            var query = _landingPageRepository.Table;
            if (!string.IsNullOrEmpty(searchKey))
                query = query.Where(l => l.Name.Contains(searchKey) || l.UrlName.Contains(searchKey) || l.MainContent.Contains(searchKey));
            if (status > 0)
                query = query.Where(l => l.Status==status);
            return query;
        }

        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public LandingPage GetLandingPage(int id)
        {
            return _landingPageRepository.GetById(id);
        }


        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <param name="urlName">Name of the URL.</param>
        /// <returns></returns>
        public LandingPage GetLandingPage(string urlName)
        {
            return _landingPageRepository.Table.FirstOrDefault(l => l.UrlName.Equals(urlName));
        }


        /// <summary>
        /// Validates the name of the URL.
        /// </summary>
        /// <param name="urlName">Name of the URL.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool ValidateUrlName(string urlName, int id)
        {
            urlName = urlName.Trim();
            bool valid = Uri.IsWellFormedUriString(urlName, UriKind.Relative);
            if (!valid)
                return false;

            return Regex.IsMatch(urlName, "[a-zA-Z0-9|-|_]") && !_landingPageRepository.Table.Any(l => l.Id != id && l.UrlName.Equals(urlName));
        }


        /// <summary>
        /// Creates the or update landing page.
        /// </summary>
        /// <param name="landingPage">The landing page.</param>
        /// <returns></returns>
        public LandingPage CreateLandingPage(LandingPage landingPage)
        {
            landingPage.LastUpdated = DateTime.UtcNow;
            if (landingPage.StatusEnum == Types.LandingPageStatus.Published)
                landingPage.PublishDate = DateTime.UtcNow;
            _landingPageRepository.Insert(landingPage);

            //if (landingPage.LandingPageCompanies != null)
            //{
            //    var companies = landingPage.LandingPageCompanies.ToList();
            //    foreach (var company in companies)
            //    {
            //         _landingPageCompanyRepository.Insert(new LandingPageCompany()
            //        {
            //            LandingPageId = landingPage.Id,
            //            ProfileId = company.ProfileId,
            //            SortOrder = company.SortOrder
            //        });
            //    }
            //}

            return landingPage;
        }

        /// <summary>
        /// Updates the landing page.
        /// </summary>
        /// <param name="landingPage">The landing page.</param>
        /// <returns></returns>
        public LandingPage UpdateLandingPage(LandingPage landingPage)
        {
            var oldLandingPage = _landingPageRepository.GetById(landingPage.Id);
            if (landingPage.LandingPageCompanies==null)
                landingPage.LandingPageCompanies = new List<LandingPageCompany>();

            // insert or update company
            foreach (var company in landingPage.LandingPageCompanies)
            {
                UpdateLandingPageCompany(oldLandingPage, company);
            }

            // remove company not exist in new landing page
            if (oldLandingPage.LandingPageCompanies.Any())
            {
                var newCompanies = landingPage.LandingPageCompanies.Select(c => c.ProfileId);
                var companiesNotExist = oldLandingPage.LandingPageCompanies.Where(c => !newCompanies.Contains(c.ProfileId)).ToList();
                if (companiesNotExist.Any())
                {
                    for (var i = 0; i < companiesNotExist.Count(); i++)
                    {
                        var company = companiesNotExist[i];
                        _landingPageCompanyRepository.Delete(company);
                    }
                }
            }

            oldLandingPage.Banner = landingPage.Banner;
            oldLandingPage.LastUpdated = DateTime.UtcNow;
            oldLandingPage.MainContent = landingPage.MainContent;
            oldLandingPage.Name = landingPage.Name;
            oldLandingPage.Scripts = landingPage.Scripts;
            oldLandingPage.Status = landingPage.Status;
            oldLandingPage.UrlName = landingPage.UrlName;
            if (landingPage.StatusEnum == Types.LandingPageStatus.Published &&
                oldLandingPage.StatusEnum != Types.LandingPageStatus.Published)
                oldLandingPage.PublishDate = DateTime.UtcNow;
            _landingPageRepository.Update(oldLandingPage);

            return landingPage;
        }
        #endregion


        public void UpdateLandingPageCompany(LandingPage oldLandingPage,
            LandingPageCompany landingPageCompany)
        {
            var oldLandingPageCompany =
                oldLandingPage.LandingPageCompanies.FirstOrDefault(c => c.ProfileId == landingPageCompany.ProfileId);
            if (oldLandingPageCompany == null)
            {
                _landingPageCompanyRepository.Insert(new LandingPageCompany()
                {
                    LandingPageId = oldLandingPage.Id,
                    ProfileId = landingPageCompany.ProfileId,
                    SortOrder = landingPageCompany.SortOrder
                });
            }
            else
            {
                oldLandingPageCompany.SortOrder = landingPageCompany.SortOrder;
                _landingPageCompanyRepository.Update(oldLandingPageCompany);
            }
        }
    }
}
