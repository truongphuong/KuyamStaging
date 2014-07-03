using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
    public class ProfileCompanyRating
    {
        public double Rate { get; set; }
        public int TotalReview { get; set; }
        public int ProfileId { get; set; }
        public bool IsBookDirect { get; set; }
        public ProfileCompany ProfileCompany { get; set; } 
        public ProfileCompanyRating()
        {
            Rate = 0;
            TotalReview = 0;
        }

        public ProfileCompanyRating(ProfileCompany profile, double rate, int totalReview, bool isBookDirect=false)
        {
            Rate = rate;
            TotalReview = totalReview;
            ProfileId = profile.ProfileID;
            ProfileCompany = profile;
            IsBookDirect = isBookDirect;
        }
    }
}