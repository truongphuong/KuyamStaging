using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kaltura;
using Kuyam.Database;
using Kuyam.GettyImagesClient.Domain;
using Kuyam.GettyImagesClient.Services;
using Kuyam.Repository.Interface;
using M2.Util;

namespace Kuyam.Domain.MediaServices
{
    public class CustGettyImageService
    {
        #region Contructors

        public CustGettyImageService(IRepository<GettyImage> gettyImageRepository, IRepository<ImageCrop> imageCrop)
        {
            _gettyImageRepository = gettyImageRepository;
            _imageCrop = imageCrop;
        }

        #endregion

        #region Private Properties
        private IRepository<ImageCrop> _imageCrop; 
        private IRepository<GettyImage> _gettyImageRepository; 
        #endregion

        #region Getty Imges API

        /// <summary>
        /// Get image detail by api
        /// </summary>
        /// <param name="token">string: token</param>
        /// <param name="imageIds">List image id as string.</param>
        /// <returns>GetImageDetails</returns>
        public GetImageDetailsResult GetImageDetails(string token, List<string> imageIds)
        {
            var imageDetail = new ImageDetail();
            var getImageDetailsResponse = imageDetail.GetImageDetails(token, imageIds);

            return getImageDetailsResponse != null ? getImageDetailsResponse.GetImageDetailsResult : null;
        }

        /// <summary>
        /// Create session for getty images api
        /// </summary>
        /// <returns>CreateSessionResult</returns>
        private CreateSessionResult CreateSession()
        {
            var session = new CreateSession();
            var createSessionTokenResponse = session.GetCreateSessionResponse();
            return createSessionTokenResponse != null ? createSessionTokenResponse.CreateSessionResult : null;
        }


        /// <summary>
        /// Search For Images Result
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public SearchForImagesResult SearchForImagesResult(string key, int pageIndex)
        {
            var createSessionResult = CreateSession();
            var searchForCreativeImage = new SearchForCreativeImages();
            if (createSessionResult != null)
            {
                var token = createSessionResult.Token;
                var searchForImagesResult = searchForCreativeImage.Search(token, key, pageIndex).SearchForImagesResult;

                if (searchForImagesResult != null)
                {
                    var imageIds = searchForImagesResult.Images.Select(m => m.ImageId).ToList();
                    var imageDetails = GetImageDetails(token, imageIds);
                    searchForImagesResult.Images = imageDetails.Images;
                    return searchForImagesResult;
                }


            }
            return new SearchForImagesResult();
        }

        /// <summary>
        /// Get images detail by key
        /// </summary>
        /// <param name="key">string: key</param>
        /// <returns>GetImageDetailsResult</returns>
        public GetImageDetailsResult GetImageDetails(string key, int pageIndex)
        {
            var createSessionResult = CreateSession();
            var searchForCreativeImage = new SearchForCreativeImages();
            var imageIds = new List<string>();
            if (createSessionResult != null)
            {
                var token = createSessionResult.Token;
                var searchForImagesResult = searchForCreativeImage.Search(token, key, pageIndex).SearchForImagesResult;
                if (searchForImagesResult != null)
                {
                    imageIds = searchForImagesResult.Images.Select(m => m.ImageId).ToList();
                }

                return GetImageDetails(token, imageIds);
            }
            return null;
        }

        /// <summary>
        /// Create download request by image list
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns>CreateDownloadRequestResult</returns>
        public CreateDownloadRequestResult CreateDownload(string imageId)
        {
            var images = new List<Image>();
            var image = new Image
            {
                ImageId = imageId
            };
            images.Add(image);
            var session = CreateSession();
            if (session != null)
            {
                var token = session.Token;

                var secureToken = session.SecureToken;

                var createDownload = new CreateDownloadRequests();

                var downImageList = new GetLargestImageDownloadAuthorizations();
                var imageAuth = downImageList.GetLargestDownloadForImages(token, images).GetLargestImageDownloadAuthorizationsResult.Images;

                var downloadItems = imageAuth.Select(item =>
                {
                    var firstOrDefault = item.Authorizations.FirstOrDefault();
                    return firstOrDefault != null ? new DownloadItem
                    {
                        DownloadToken = firstOrDefault.DownloadToken
                    } : null;
                }).ToList();

                var createDownloadResponse = createDownload.CreateRequest(secureToken, downloadItems);
                return createDownloadResponse != null ? createDownloadResponse.CreateDownloadRequestResult : null;
            }
            return null;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Delete file from the server
        /// </summary>
        /// <param name="fullPath"></param>
        public void DeleteFile(string fullPath)
        {
            var filePath = fullPath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Download image from sever
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public string DownloadImage(string uri, string imageName)
        {
            var fullPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/UploadMedia/"), imageName);
            try
            {

                var client = new WebClient();
                client.DownloadFile(uri, fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                Logger.Error("Download image is error: " + ex);
                return string.Empty;
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Get all Getty Images by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<GettyImage> GetGettyImages(string key, int pageIndex, out int totalItem)
        {

            totalItem = 0;
            var imageResults = GetImageDetails(key, pageIndex);
            return imageResults == null ? null : imageResults.Images.Select(CreateJsonGettyImage).ToList();

            // convert each blog into smaller Json friendly object 
        }

        /// <summary>
        /// Converts the Getty images data into a JSON-serializable object.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public GettyImage CreateJsonGettyImage(Image img)
        {
            var ji = new GettyImage
            {
                GettyImageId = img.ImageId,
                Title = img.Title,
                Tags = img.ImageFamily,
                UrlThumb = img.UrlThumb,
                UrlPreview = img.UrlPreview
            };

            return ji;
        }

        
        /// <summary>
        /// Upload image to Kaltural
        /// </summary>
        /// <param name="gettyImage"></param>
        /// <returns></returns>
        public string UploadToKaltural(GettyImage gettyImage)
        { 
            try
            {
                //Upload to kaltural
                string urlAttachment = DownloadResult(gettyImage.GettyImageId);
                var fullPath = DownloadImage(urlAttachment, string.Format("{0}_{1}.jpg", gettyImage.CustId, DateTime.UtcNow.ToString("ddMMyyyyhmmss")));
                var fileName = string.Format("{0}{1}_{2}", gettyImage.CustId, gettyImage.Id, DateTime.UtcNow.ToString("ddMMyyyyhmmss"));
                var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                var kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(fileStream, KalturaMediaType.IMAGE, fileName);

                fileStream.Flush();
                fileStream.Close();
                DeleteFile(fullPath);

                return kalturaMediaEntry.Id;
            }
            catch (Exception ex)
            {
                //Write log here
                Logger.Error("Error upload to Kaltura: " + ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<GettyImage> LoadGetttyImagesClient(int userId)
        {
            return _gettyImageRepository.Table.Where(c => c.CustId == userId).ToList();

        }

        /// <summary>
        /// Insert Gettty Images Client
        /// </summary>
        /// <param name="gettyImage"></param>
        /// <returns></returns>
        public int InsertGetttyImagesClient(GettyImage gettyImage)
        {
            _gettyImageRepository.Insert(gettyImage);
            return gettyImage.Id;
        }


        /// <summary>
        /// Download result
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        public string DownloadResult(string imageId)
        {
            var createDownload = CreateDownload(imageId);
            var downloadUrls = createDownload.DownloadUrls.FirstOrDefault();
            return downloadUrls != null ? downloadUrls.UrlAttachment : null;
        }

        /// <summary>
        /// Adds the image crop.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public ImageCrop AddImageCrop(ImageCrop image)
        {
            _imageCrop.Insert(image);
            return image;
        }

        /// <summary>
        /// Updates the image crop.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public ImageCrop UpdateImageCrop(ImageCrop image)
        {
            var oldImage = _imageCrop.GetById(image.Id);
            oldImage.Crop_x = image.Crop_x;
            oldImage.Crop_y = image.Crop_y;
            oldImage.Fr_height = image.Fr_height;
            oldImage.Fr_width = image.Fr_width;
            oldImage.Rel_height = image.Rel_height;
            oldImage.Rel_width = image.Rel_width;
            oldImage.ZoomPercent = image.ZoomPercent;
            oldImage.LocationData = image.LocationData;
            _imageCrop.Update(oldImage);
            return image;
        }

        /// <summary>
        /// Gets the image crop.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ImageCrop GetImageCrop(int id)
        {
            return _imageCrop.GetById(id);
        }
        #endregion
    }
}
