using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository;
using Kuyam.Repository.Interface;

using Kaltura;
using System.Net;
using System.IO;
using Kuyam.Utility;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.Domain
{
    public class GettyImageService
    {
        #region Fields

        private readonly IRepository<GettyImage> _gettyImageRepository;
        private readonly CompanyProfileService _companyProfileService;
        private readonly IRepository<be_GettyImages> _beGettyImageRepository;
        #endregion

        #region Ctor

        public GettyImageService(IRepository<GettyImage> gettyImageRepository, 
            CompanyProfileService companyProfileService,
            IRepository<be_GettyImages> beGettyImageRepository)
        {
            this._gettyImageRepository = gettyImageRepository;
            this._companyProfileService = companyProfileService;
            this._beGettyImageRepository = beGettyImageRepository;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Insert list of Gettyimages in to database
        /// </summary>
        /// <param name="images">List<GettyImage> images<></param>
        /// <returns>
        ///          true:success
        ///          false:unsuccess   
        /// </returns>
        public bool InsertGettyImages(List<GettyImage> images)
        {
            bool result = false;

            try
            {

                if (images != null && images.Count > 0)
                {
                    foreach (GettyImage image in images)
                    {
                        _gettyImageRepository.Insert(image);
                        LogHelper.Info(string.Format("Added getty image: GettyImageId= {0}", image.GettyImageId));
                    }                    
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Add getty images fail: ", ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Insert image in to database
        /// </summary>
        /// <param name="images">GettyImage: images<></param>
        /// <returns>
        ///          true:success
        ///          false:unsuccess   
        /// </returns>
        public bool InsertGettyImage(GettyImage image){

            bool result = false;

            try{

                if (image != null){

                    _gettyImageRepository.Insert(image);                   
                    LogHelper.Info(string.Format("Added getty image: GettyImageId= {0}", image.GettyImageId));
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Add getty image fail: ", ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Get all image by profile id
        /// </summary>
        /// <param name="profileID">int: profileID</param>
        /// <returns>List<GettyImage></returns>
        public List<GettyImage> GetGettyImages(int profileID, int status= (int)Types.GettyImageStatus.Pending){
            return _gettyImageRepository.Table.Where(g => g.ProfileId == profileID && g.Status == status).ToList();
        }

        public GettyImage GetGettyImage(int profileID, int imageId, int status = (int)Types.GettyImageStatus.Pending)
        {
            return _gettyImageRepository.Table.Where(g => g.ProfileId == profileID &&g.Id==imageId&& g.Status == status).FirstOrDefault();
        }

        public List<GettyImage> GetGettyImageByProfile(int profileID, int status = (int)Types.GettyImageStatus.Pending)
        {
            return _gettyImageRepository.Table.Where(g => g.ProfileId == profileID && g.Status == status).ToList();
        }

        /// <summary>
        /// Gets the getty by customer identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<GettyImage> GetGettyByCustId(int custId)
        {
            return _gettyImageRepository.Table.Where(g => g.CustId == custId).ToList();
        }

        /// <summary>
        /// Update Getty Image
        /// </summary>
        /// <param name="gettyImage">GettyImage: gettyImage</param>
        /// <returns>
        ///     true: success
        ///     false: fail
        /// </returns>
        public bool UpdateGettyImage(GettyImage gettyImage) {
            bool result = false;
            try{
                _gettyImageRepository.Update(gettyImage);
                LogHelper.Info(string.Format("Updated getty image: gettyImageID= {0}", gettyImage.GettyImageId));
                result = true;
            }
            catch (Exception ex){
                LogHelper.Error("Update getty image fail: ", ex);
            }

            return result;
        }

        /// <summary>
        /// Set image to default image
        /// </summary>
        /// <param name="gettyImage">GettyImage: gettyImage</param>
        /// <returns>
        ///      true:success
        ///      false:unsuccess   
        /// </returns>
        public bool SetDefaultGettyImage(GettyImage gettyImage) {
            bool result = false;
            try{
                _gettyImageRepository.Update(gettyImage);
                LogHelper.Info(string.Format("Set getty image is default: GettyImageId= {0}", gettyImage.GettyImageId));
                result = true;
            }
            catch (Exception ex){
                LogHelper.Error("Set default image is fail: ", ex);
            }
            return result;
        }

        /// <summary>
        /// Reset all getty image IsDefault = false
        /// </summary>
        /// <param name="profileId">int: profileId</param>
        public void ResetDefaultGettyImage(int profileId) {
            List<GettyImage> gettyImages = _gettyImageRepository.Table.Where(g => g.ProfileId == profileId).ToList();
            foreach (var img in gettyImages)
            {
                img.IsDefault = false;
            }
            _gettyImageRepository.Update(new GettyImage());
        }

        /// <summary>
        /// Delete a getty image
        /// </summary>
        /// <param name="gettyImageId">int: gettyImageId</param>
        /// <returns>
        ///  true:success
        ///  false:unsuccess   
        /// </returns>
        public bool DeleteGettyImage(int gettyImageId) {
            bool result = false;

            try{

                if (_gettyImageRepository.Table.Any(g =>g.Id  == gettyImageId)){
                    GettyImage gettyImage = _gettyImageRepository.Table.Where(g => g.Id == gettyImageId).FirstOrDefault();
                    _gettyImageRepository.Delete(gettyImage);                    
                    LogHelper.Info(string.Format("Deleted getty image: gettyImageID= {0}", gettyImage.GettyImageId));
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Update getty image fail: ", ex);
                throw;
            }

            return result;

        }

        public void DeleteGettyImages(List<GettyImage> gettyImages) {
            foreach (GettyImage gettyImage in gettyImages){
                bool result = DeleteGettyImage(gettyImage.Id);
            }
        
        }

        public void SaveToCompanyMedia(List<GettyImage> gettyImages) {
            List<CompanyMedia> lstcompanyMedia = new List<CompanyMedia>();
            try{
                if (gettyImages!=null&&gettyImages.Count>0){

                    foreach (GettyImage gettyImage in gettyImages){
                        CompanyMedia companyMedia = new CompanyMedia(){
                            ProfileID = gettyImage.ProfileId.Value,
                            MediaID = SaveToMedium(gettyImage),
                            IsBanner = true,
                            IsDefault = gettyImage.IsDefault ?? false,
                            IsHidden = gettyImage.IsHidden ?? false
                        };
                        lstcompanyMedia.Add(companyMedia);
                    }
                    if (lstcompanyMedia!=null&&lstcompanyMedia.Count>0){
                        _companyProfileService.InsertCompanyMedia(lstcompanyMedia);
                    }
                
                //Update all images have status to "Completed"
                UpdateCompanyStatusAfterCompleted(gettyImages);
                }
            }
            catch (Exception ex){
                LogHelper.Error("Save to company media is fail: ", ex);
            }
        }

        public int SaveToMedium(GettyImage gettyImage) {

            string locationData = gettyImage.GettyImageId;
            string locationPath = gettyImage.UrlPreview;

            try{

                if (gettyImage.Type != (int)Types.ImageType.Kaltura){
                    //Upload to kaltural
                    var fullPath = DownloadImage(gettyImage.UrlPreview, string.Format("{0}{1}.jpg", gettyImage.ProfileId, gettyImage.Id));
                    string fileName = string.Format("{0}{1}", gettyImage.ProfileId, gettyImage.Id);
                    FileStream _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    KalturaMediaEntry kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(_fileStream, Kaltura.KalturaMediaType.IMAGE, fileName);

                    _fileStream.Flush();
                    _fileStream.Close();
                    DeleteFile(fullPath);

                    locationData = kalturaMediaEntry.Id;
                    locationPath = kalturaMediaEntry.DataUrl;
                }

                Medium media = new Medium{

                    CustID = gettyImage.ProfileCompany.Profile.CustID,
                    CustTypeID = (int)Types.CustType.Company,
                    MediaLocationTypeID = (int)Types.MediaLocation.Kaltura,
                    LocationData = locationData,
                    LocationPath = locationPath,
                    MediaTypeID = (int)Types.MediaType.Image,
                    Desc = gettyImage.Title
                };
                _companyProfileService.InsertMedia(media);
                return media.MediaID;   
            }
            catch (Exception ex){
                LogHelper.Error("Save to media is fail: ", ex);
                return 0;
            }
            
           
        }
        // Delete file from the server
        private void DeleteFile(string fullPath)
        {
            var filePath = fullPath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private void UpdateCompanyStatusAfterCompleted(List<GettyImage> gettyImages) {
            if (gettyImages!=null&&gettyImages.Count>0){
                foreach (GettyImage gettyImage in gettyImages){
                    gettyImage.Status = (int)Types.GettyImageStatus.Completed;
                    UpdateGettyImage(gettyImage);
                }
            }
            
        }

        public string DownloadImage(string uri, string imageName){
            string fullPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/UploadMedia/"), imageName);
            try{

                WebClient client = new WebClient();
                client.DownloadFile(uri, fullPath);
                return fullPath;
            }
            catch (Exception ex){

                return string.Empty;
            }
        }


        #endregion

        public be_GettyImages GetBeGettyImageById(int imageId)
        {
            return _beGettyImageRepository.Table.Where(g => g.Id == imageId).FirstOrDefault();
        }
    }
}
