using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;

namespace Kuyam.WebUI.Models.Company
{
    public class CompanyAjaxSearchModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CompanySearchModel"/> class.
        /// </summary>
        public CompanyAjaxSearchModel()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanySearchModel"/> class.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <exception cref="System.Exception">Company is null</exception>
        public CompanyAjaxSearchModel(ProfileCompany company)
        {
            if (company == null)
                throw new Exception("Company is null");
            Id = company.ProfileID;
            Name = company.Name;
        }
        #endregion


        #region Public Properties
        public int Id { get; set; }
        public string Name { get; set; }
        #endregion

    }
}