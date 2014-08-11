using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class CompanyMediaDTO
    {
        public CompanyMediaDTO(List<CompanyMedia> companyMedias)
        {
            if (companyMedias != null)
            {
                foreach (var item in companyMedias)
                {
                    this.CompanyMediaID = item.CompanyMediaID;
                    this.ProfileID = item.ProfileID;
                    this.MediaID = item.MediaID;
                    this.IsBanner = item.IsBanner;
                    this.IsLogo = item.IsLogo;
                    this.IsVideo = item.IsVideo;
                    this.IsDefault = item.IsDefault;
                    this.IsHidden = item.IsHidden;
                }
            }
            
        }
        public int CompanyMediaID { get; set; }
        public int ProfileID { get; set; }
        public int MediaID { get; set; }
        public bool IsBanner { get; set; }
        public bool IsLogo { get; set; }
        public bool IsVideo { get; set; }
        public bool IsDefault { get; set; }
        public bool? IsHidden { get; set; }
    }
}