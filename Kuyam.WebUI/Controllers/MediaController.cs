using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Services;
using Kuyam.Database;
using Kuyam.Domain;
using Kuyam.Domain.MediaServices;
using Kuyam.GettyImagesClient.Domain;
using Kuyam.Utility;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.Models.Media;

namespace Kuyam.WebUI.Controllers
{
    public class MediaController : KuyamBaseController
    {
        #region Constructors

        public MediaController()
        {
            
        }

        public MediaController(CustGettyImageService custGettyImageService)
        {
            _custGettyImageService = custGettyImageService;
        }
        #endregion


        #region Private Properties
        private readonly CustGettyImageService _custGettyImageService;
        #endregion


        public ActionResult LoadGetttyImages(string key, int pageIndex)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Json(new SearchForImagesResult(), JsonRequestBehavior.AllowGet);
            }
            
            var imageResults = _custGettyImageService.SearchForImagesResult(key, pageIndex);
            return Json(imageResults, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadGetttyImagesClient()
        {
            var imageResults = _custGettyImageService.LoadGetttyImagesClient(MySession.CustID).Select(i=>new GettyImageModel(i));
            return Json(imageResults, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void GetTemplate(string id)
        {
            string filePath = Server.MapPath("/Media/") + id;
            
            if (System.IO.File.Exists(filePath))
            {
                var encoding = new System.Text.UTF8Encoding();
                var htm = System.IO.File.ReadAllText(Server.MapPath("/Media/") + id, encoding);
                byte[] data = encoding.GetBytes(htm);
                Response.OutputStream.Write(data, 0, data.Length);
                Response.OutputStream.Flush();
            }
        }

        public ActionResult UploadImageToKaltura(string urlPreview, string gettyImageId, string title = null)
        {
            //Get download url
            //var downloadResult = gettyImages.DownloadResult(gettyImageId);

           
            //For testting
            var downloadResult = urlPreview;

            var gettyImage = new GettyImage
            {
                UrlPreview = urlPreview,
                GettyImageId = gettyImageId,
                CustId = MySession.CustID,
                UrlAttachment = downloadResult,
                Title = title
            };
            var kalturaId = _custGettyImageService.UploadToKaltural(gettyImage);
            //Utils.Log("Error setting current theme: " + kalturaId);
            if (!string.IsNullOrEmpty(kalturaId))
            {
                gettyImage.LocationData = kalturaId;
                _custGettyImageService.LoadGetttyImagesClient(MySession.CustID);
                _custGettyImageService.InsertGetttyImagesClient(gettyImage);
            }
            return Json(kalturaId, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AddOrUpdateCropImage(int imageId, string kalturaId, int cropX, int cropY, int frameW, int frameH, int relW, int relH, double zoom)
        {
            if (!string.IsNullOrEmpty(kalturaId))
            {
                try
                {
                    var cropImage = new ImageCrop
                    {
                        Id = imageId,
                        Crop_x = cropX,
                        Crop_y = cropY,
                        Fr_width = frameW,
                        Fr_height = frameH,
                        LocationData = kalturaId,
                        Rel_height = relH,
                        Rel_width = relW,
                        ZoomPercent = zoom
                    };
                    if (imageId == 0)
                        cropImage = _custGettyImageService.AddImageCrop(cropImage);
                    else
                        cropImage = _custGettyImageService.UpdateImageCrop(cropImage);

                    var link = KalturaHelper.GetKalturaUrl(cropImage.LocationData, frameW, frameH, cropX, cropY, relW, relH);
                    return Json(new { result = true, id = cropImage.Id, url = link }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    
                }
            }
            return Json(new {result = false}, JsonRequestBehavior.AllowGet);
        }
    }
}
