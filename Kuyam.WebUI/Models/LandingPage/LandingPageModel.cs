using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain.LandingePageServices;
using Kuyam.Repository.Infrastructure;
using Kuyam.Utility;
using Type = System.Type;

namespace Kuyam.WebUI.Models.LandingPage
{
    public class LandingPageModel:IValidatableObject
    {
        #region Constructors

        public LandingPageModel()
        {
            StatusEnum = Types.LandingPageStatus.Draft;
            FrameW = 762;
            FrameH = 386;
            Zoom = 1;
            LinkReturn = "/AdminLandingPage/Index";
            CompanyAvaiableList = new List<LandingPageCompanyModel>();
        }

        public LandingPageModel(Database.LandingPage landingPage):this()
        {
            Id = landingPage.Id;
            Name = landingPage.Name;
            UrlName = landingPage.UrlName;
            Banner = landingPage.Banner.HasValue?landingPage.Banner.Value:0;
            MainContent = landingPage.MainContent;
            Status = landingPage.Status;
            LastUpdated = landingPage.LastUpdated;
            ImageCrop = landingPage.ImageCrop;
            Scripts = landingPage.Scripts;
            if (landingPage.ImageCrop != null)
            {
                SetImageCrop(ImageCrop);
            }

            if (landingPage.LandingPageCompanies.Any())
            {
                foreach (var company in landingPage.LandingPageCompanies.OrderBy(c=>c.SortOrder))
                {
                    CompanyAvaiableList.Add(new LandingPageCompanyModel
                    {
                        CompanyId = company.ProfileId,
                        CompanyName = company.ProfileCompany.Name,
                        Order = company.SortOrder
                    });
                }
            }
        }

        public void SetImageCrop(ImageCrop imageCrop)
        {
            if (imageCrop != null)
            {
                this.ImageCrop = imageCrop;
                KalturaId = imageCrop.LocationData;
                CropX = imageCrop.Crop_x.GetValueOrDefault(0);
                CropY = imageCrop.Crop_y.GetValueOrDefault(0);
                FrameW = imageCrop.Fr_width.GetValueOrDefault(FrameW);
                FrameH = imageCrop.Fr_height.GetValueOrDefault(FrameH);
                RelW = imageCrop.Rel_width.GetValueOrDefault(FrameW);
                RelH = imageCrop.Rel_height.GetValueOrDefault(FrameH);
                Zoom = imageCrop.ZoomPercent.HasValue ? imageCrop.ZoomPercent.Value : 1;
            }
        }

        public void SetCompaniesRelated(int[] companiesRelated)
        {
            CompanyAvaiableList = new List<LandingPageCompanyModel>();
            if (companiesRelated != null)
            {
                int index = 1;
                foreach (int profileId in companiesRelated)
                {
                    CompanyAvaiableList.Add(new LandingPageCompanyModel()
                    {
                        CompanyId = profileId,
                        CompanyName = DAL.GetProfileCompany(profileId).Name,
                        Order = index++
                    });
                }    
            }
        }
        #endregion


        #region Public Properties
        public int Id { get; set; }

        [Required]
        [DisplayName("name")]
        [StringLength(50, ErrorMessage = "name must not exceed 50 characters")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "url name must not exceed 100 characters")]
        [DisplayName("url name")]
        public string UrlName { get; set; }

        [Required]
        [DisplayName("banner")]
        public int Banner { get; set; }

        [AllowHtml]
        [UIHint("tinymce_basic")]
        [Required]
        [DisplayName("content")]
        public string MainContent { get; set; }

        public int Status { get; set; }

        public System.DateTime LastUpdated { get; set; }

        public ImageCrop ImageCrop { get; set; }

        public Types.LandingPageStatus StatusEnum
        {
            get { return (Types.LandingPageStatus)Status; }
            set { Status = (int)value; }
        }

        [AllowHtml]
        [DataType(DataType.Html)]
        public string Scripts { get; set; }

        public string Submit { get; set; }

        public string KalturaId { get; set; }
        public int CropX { get; set; }
        public int CropY { get; set; }
        public int FrameW { get; set; }
        public int FrameH { get; set; }
        public int RelW { get; set; }
        public int RelH { get; set; }
        public double Zoom { get; set; }
        public string LinkReturn { get; set; }

        public List<LandingPageCompanyModel> CompanyAvaiableList { get; set; } 
        public string BannerOriginalImage
        {
            get
            {
                if (ImageCrop != null && !string.IsNullOrEmpty(ImageCrop.LocationData))
                {
                    return KalturaHelper.GetKalturaUrl(ImageCrop.LocationData, 0, 0, 1);
                }
                return string.Empty;
            }
        }

        public string BannerDisplayImage
        {
            get
            {
                if (ImageCrop != null && !string.IsNullOrEmpty(ImageCrop.LocationData))
                {
                    //if (ImageCrop.ZoomPercent ==1)
                    //    return KalturaHelper.GetKalturaUrl(ImageCrop.LocationData, FrameW, FrameH, 1);
                    //else
                        return KalturaHelper.GetKalturaUrl(ImageCrop.LocationData, FrameW, FrameH, CropX, CropY, RelW, RelH);
                }
                return string.Empty;
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <returns></returns>
        public Database.LandingPage GetLandingPage(int[] companiesRelated=null)
        {
            if (!string.IsNullOrEmpty(Submit))
            {
                if (Submit.Equals("publish"))
                {
                    StatusEnum = Types.LandingPageStatus.Published;
                }
                else if (Submit.Equals("unpublish"))
                {
                    StatusEnum = Types.LandingPageStatus.Unpublished;
                }
                else
                {
                    StatusEnum = Types.LandingPageStatus.Draft;
                }
            }

            CompanyAvaiableList = new List<LandingPageCompanyModel>();
            if (companiesRelated != null)
            {
                for (int i = 0; i < companiesRelated.Length; i++)
                {
                    CompanyAvaiableList.Add(new LandingPageCompanyModel()
                    {
                        CompanyId = companiesRelated[i],
                        Order = i+1
                    });
                }
            }

            var landingPage = new Database.LandingPage
            {
                Id = Id,
                LastUpdated = DateTime.UtcNow,
                MainContent = MainContent,
                Name = Name,
                Scripts = Scripts,
                Status = Status,
                UrlName = UrlName.Trim(),
                Banner = Banner==0?(int?) null:Banner
            };

            if (CompanyAvaiableList.Any())
            {
                foreach (var company in CompanyAvaiableList)
                {
                    landingPage.LandingPageCompanies.Add(new LandingPageCompany()
                    {
                        LandingPageId = landingPage.Id,
                        ProfileId = company.CompanyId,
                        SortOrder = company.Order
                    });
                }
            }
            return landingPage;
        }
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Submit) && Submit.Equals("publish") && Banner == 0)
                yield return new ValidationResult("banner is required.", new[] {"Banner"});

            var service = EngineContext.Current.Resolve<ILandingPageServices>();
            if (!service.ValidateUrlName(UrlName, Id))
                yield return new ValidationResult("url name is invalid.", new[] { "UrlName" });
        }
    }
}