using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Instrumentation;
using Kuyam.Database;

namespace Kuyam.Domain.LandingePageServices
{
    public interface ILandingPageServices
    {
        /// <summary>
        /// Gets the landing pages.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <param name="status">The status. Default is 0 ~ get all </param>
        /// <returns></returns>
        IQueryable<LandingPage> GetLandingPages(string searchKey, int status = 0);

        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        LandingPage GetLandingPage(int id);

        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <param name="urlName">Name of the URL.</param>
        /// <returns></returns>
        LandingPage GetLandingPage(string urlName);

        /// <summary>
        /// Validates the name of the URL.
        /// </summary>
        /// <param name="urlName">Name of the URL.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        bool ValidateUrlName(string urlName, int id);

        /// <summary>
        /// Creates the or update landing page.
        /// </summary>
        /// <param name="landingPage">The landing page.</param>
        /// <returns></returns>
        LandingPage CreateLandingPage(LandingPage landingPage);

        /// <summary>
        /// Updates the landing page.
        /// </summary>
        /// <param name="landingPage">The landing page.</param>
        /// <returns></returns>
        LandingPage UpdateLandingPage(LandingPage landingPage);
    }
}
