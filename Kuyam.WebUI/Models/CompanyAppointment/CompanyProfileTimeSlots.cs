using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;

namespace Kuyam.WebUI.Models.CompanyAppointment
{
    public class CompanyProfileTimeSlots
    {

        public CompanyProfileTimeSlots()
        {
            TimeSlots = new CompanyAvailableTimeSlots();
        }
        public ProfileCompany Company
        {
            get { return _profileCompany; }
            set
            {
                _profileCompany = value;
                IsFeatureCompany = DAL.isFeatureCompany(_profileCompany.ProfileID);
                Logo = DAL.GetCompanyLogoFromProfileCompanyID(_profileCompany.ProfileID);
                if (Logo != null && Logo.LocationPath != null && Logo.LocationPath != string.Empty)
                    UrlLogo = Types.KaturaDoman + "/p/811441/thumbnail/entry_id/" + Logo.LocationData + "/width/85/height/82";

                Image = DAL.GetCompanyImageFromProfileCompanyID(_profileCompany.ProfileID);
                IsViewAvailability = _profileCompany.CompanyTypeID != (int)Types.CompanyType.NonKuyamBookIt
                                     && _profileCompany.CompanyTypeID != (int)Types.CompanyType.GeneralAvailability;

                HasVideo = _profileCompany.CompanyMedias.Any(m => m.IsVideo);
                IsFavorite = DAL.isFavorite(Kuyam.WebUI.Models.MySession.CustID, _profileCompany.ProfileID);

                TotalReviews = _profileCompany.TotalReview;
                Rate = Convert.ToInt32(_profileCompany.Rate);

                Categories = DAL.GetTypeNameFromProfileID(_profileCompany.ProfileID);
                if (Categories.Length > 46)
                {
                    Categories = Kuyam.Domain.UtilityHelper.TruncateText(Categories, 46) + "...";
                }
            }
        }

        public CompanyAvailableTimeSlots TimeSlots { get; set; }
        public bool IsFeatureCompany { get; set; }

        public Medium Logo { get; set; }
        public string UrlLogo { get; set; }

        public Medium Image { get; set; }
        public bool IsViewAvailability { get; set; }

        public bool HasVideo { get; set; }
        public bool IsFavorite { get; set; }        
        public int Rate { get; set; }
        public int TotalReviews { get; set; }
        public string Categories { get; set; }

        private ProfileCompany _profileCompany;
    }
}