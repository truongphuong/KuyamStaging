using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Kaltura;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using Kuyam.Domain;
using Kuyam.WebUI.Models.Media;
using M2.Util.MVC;
using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using Kuyam.WebUI.InfoConnServiceReference;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Kuyam.WebUI.Helpers;
using System.Web;
using Kuyam.Utility;
using M2.Util;
using Kuyam.GettyImagesClient.Services;
using Kuyam.GettyImagesClient.Domain;
using System.Net;
using System.Globalization;
using System.Text;
using System.Configuration;
using Kuyam.Domain.Payments;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    public class CompanySetupController : KuyamBaseController
    {
        private readonly GettyImageService _gettyImageService;
        private readonly CompanyProfileService _companyProfileService;
        private readonly OrderService _orderService;
        public CompanySetupController(GettyImageService gettyImageService,
            CompanyProfileService companyProfileService,
            OrderService orderService)
        {
            this._gettyImageService = gettyImageService;
            this._companyProfileService = companyProfileService;
            this._orderService = orderService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Image()
        {
            string key = string.Empty;

            int profileId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            Profile profile = _companyProfileService.GetProfileByID(profileId);

            if (profile == null)
                return RedirectToAction("SetupBasic", "Company", new { companyId = profileId });

            var serviceCompany = _companyProfileService.GetCategoryByProfileID(profileId).FirstOrDefault();
            if (string.IsNullOrEmpty(key) && serviceCompany != null && serviceCompany.Service != null)
            {
                key = serviceCompany.Service.ServiceName;
            }
            else
            {
                key = profile.Name;
            }
            IList<Image> imgList = GetGettyImages(string.Empty, 1);
            //if (MySession.GettyImages == null){
            //    imgList = GetGettyImages(key);
            //}
            //else {
            //    imgList = MySession.GettyImages;
            //}
            ViewBag.ImageList = imgList;
            ViewBag.ServiceName = key;
            ViewBag.CompanyID = profileId;
            ViewBag.ItemStartNumber = 1;
            //
            List<CompanyMedia> companyMedia = _companyProfileService.GetCompanyMediaByProfileID(profileId);
            ViewBag.ImagesDownloaded = _companyProfileService.GetCompanyMediaByProfileID(profileId).Where(x => x.IsBanner).OrderByDescending(x=>x.MediaID).ToList();
            ViewBag.ImagesPending = _gettyImageService.GetGettyImages(profileId, (int)Types.GettyImageStatus.Pending);
            ViewBag.Logo = _companyProfileService.GetCompanyMediaByProfileID(profileId).Where(x => x.IsVideo != true && x.IsLogo).FirstOrDefault();
            ViewBag.IsActiveCompany = profile.ProfileCompany.CompanyStatusID == (int)Types.CompanyStatus.Active;
            return View();
        }

        [HttpPost]
        public ActionResult GetGettyImagesByKey(string key, int itemStartNumber)
        {
            MySession.GettyImages = null;
            if (itemStartNumber == 0)
                itemStartNumber = 1;
            int profileId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            ViewBag.ImageList = GetGettyImages(key, itemStartNumber);
            ViewBag.ItemStartNumber = itemStartNumber;
            ViewBag.ImagesPending = _gettyImageService.GetGettyImages(profileId, (int)Types.GettyImageStatus.Pending);
            return PartialView("_GettyImageListResults");
        }

        /// <summary>
        /// Get getty images list by key
        /// </summary>
        /// <param name="key">String: key for search</param>
        /// <returns>IList<Image></returns>
        public IList<Image> GetGettyImages(string key, int itemStartNumber)
        {
            IList<Image> result = null;
            int profileId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            IList<Image> images = null;
            List<GettyImage> gettyImages = _gettyImageService.GetGettyImages(profileId, (int)Types.GettyImageStatus.Pending);

            if (MySession.GettyImages == null || MySession.GettyImages.Count == 0)
            {
                GetImageDetailsResult GetImageDetail = GetImageDetails(key, itemStartNumber);

                //var imagesResult = SearchForImagesResult(key, itemStartNumber);
                if (GetImageDetail != null)
                {
                    images = GetImageDetail.Images;
                }
            }
            else
            {
                images = MySession.GettyImages;
            }

            //List<string> imgId = images.Select(a => a.ImageId).ToList();
            //List<string> gettyImageId = gettyImage.Select(a => a.GettyImageId).ToList();

            //var query = imgId.Except(gettyImageId);

            //var queryImg = from n in images
            //             join m in query on n.ImageId equals m
            //             select n;

            if (images != null && images.Count > 0)
            {
                var query = from n in images
                            join m in gettyImages on n.ImageId equals m.GettyImageId into grp
                            from m in grp.DefaultIfEmpty()
                            where m == null
                            select n;
                result = query.ToList();
            }
            MySession.GettyImages = images;

            return result;
        }

        [HttpPost]
        public ActionResult ChangeGettyImageStatus(int status)
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult InsertGettyImage(string imageId, int companyId)
        //{
        //    bool result=false;
        //    CreateSessionResult createSessionResult = CreateSession();
        //    List<string> imageIds = new List<string>();
        //    int profileId = companyId != 0 ? companyId : MySession.ProfileID;

        //    if (imageId != null)
        //    {
        //        imageIds.Add(imageId);
        //    }

        //    if (createSessionResult != null){

        //        string token = createSessionResult.Token;
        //        GetImageDetailsResult imageDetailsResult = GetImageDetails(token, imageIds);

        //        if (imageDetailsResult != null && imageDetailsResult.Images.Count>0)
        //        {
        //            Image image = imageDetailsResult.Images.FirstOrDefault();

        //            string tags = string.Empty;
        //            for (int i = 0; i < 5; i++)
        //            {
        //                if (i == 4)
        //                {
        //                    tags += string.Format("{0}", image.Keywords[i].Text);
        //                }
        //                else
        //                {
        //                    tags += string.Format("{0}, ", image.Keywords[i].Text);
        //                }

        //            }

        //            GettyImage gettyImage = new GettyImage
        //            {
        //                GettyImageId = image.ImageId,
        //                Status = (int)Types.GettyImageStatus.Pending,
        //                Title = image.Title,
        //                PixelHeight = image.SizesDownloadableImages.FirstOrDefault().PixelHeight,
        //                PixelWidth = image.SizesDownloadableImages.FirstOrDefault().PixelWidth,
        //                UrlThumb = image.UrlThumb,
        //                UrlPreview = image.UrlPreview,
        //                ProfileId = profileId,
        //                Tags = tags,
        //                Type=(int)Types.ImageType.GettyImage,
        //                IsDefault=false,
        //                IsHidden=false
        //            };
        //            result = _gettyImageService.InsertGettyImage(gettyImage);
        //        }
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public ActionResult InsertGettyImage(GettyImageModel image)
        {
            bool result = false;

            GettyImage gettyImage = new GettyImage
            {

                GettyImageId = image.GettyImageId,
                Status = (int)Types.GettyImageStatus.Pending,
                Title = image.Title,
                PixelHeight = image.PixelHeight,
                PixelWidth = image.PixelWidth,
                UrlThumb = image.UrlThumb,
                UrlPreview = image.UrlPreview,
                ProfileId = image.ProfileId,
                Tags = image.Tags,
                Type = (int)Types.ImageType.GettyImage,
                IsDefault = false,
                IsHidden = false
            };
            result = _gettyImageService.InsertGettyImage(gettyImage);

            ViewBag.ImagesPending = _gettyImageService.GetGettyImages(image.ProfileId, (int)Types.GettyImageStatus.Pending);
            return PartialView("_GettyImageListCartResults");
        }

        [HttpPost]
        public ActionResult DeleteGettyImage(int imageId)
        {

            bool result = _gettyImageService.DeleteGettyImage(imageId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDefaultImage(int imageId)
        {
            int profileId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            GettyImage gettyImage = _gettyImageService.GetGettyImage(profileId, imageId, (int)Types.GettyImageStatus.Paid);
            if (gettyImage.IsDefault == false)
            {
                _gettyImageService.ResetDefaultGettyImage(profileId);
                gettyImage.IsDefault = true;
                bool result = _gettyImageService.SetDefaultGettyImage(gettyImage);
            }
            else
            {
                _gettyImageService.ResetDefaultGettyImage(profileId);
            }

            return RedirectToAction("Image");
        }

        public ActionResult UpdateHideImage(int imageId)
        {
            int profileId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            GettyImage gettyImage = _gettyImageService.GetGettyImage(profileId, imageId, (int)Types.GettyImageStatus.Paid);
            if (gettyImage.IsHidden == true)
            {
                gettyImage.IsHidden = false;
            }
            else
            {
                gettyImage.IsHidden = true;
            }
            bool result = _gettyImageService.UpdateGettyImage(gettyImage);
            return RedirectToAction("Image");
        }

        [HttpPost]
        public ActionResult SaveCompanyImages(List<GettyImageModels> images)
        {
            //List<GettyImage> images=_gettyImageService.GetGettyImages(MySession.ProfileID, (int)Types.GettyImageStatus.Paid);
            //_gettyImageService.SaveToCompanyMedia(images);
            List<CompanyMedia> companyMedium = new List<CompanyMedia>();
            if (images != null && images.Count > 0)
            {
                foreach (GettyImageModels image in images)
                {
                    CompanyMedia companyMedia = new CompanyMedia
                    {
                        MediaID = image.Id,
                        IsDefault = image.Default,
                        IsHidden = image.Hidden
                    };
                    _companyProfileService.UpdateCompanyMedia(companyMedia);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CropImageForLogo(int companyId, string url)
        {
            KalturaMediaEntry kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(new Uri(url), Kaltura.KalturaMediaType.IMAGE, "Crop logo");

            
            Medium media = new Medium
            {
                CustID = MySession.CustID,
                CustTypeID = (int)Types.CustType.Company,
                MediaLocationTypeID = (int)Types.MediaLocation.Kaltura,
                LocationData = kalturaMediaEntry.Id,
                LocationPath = kalturaMediaEntry.DataUrl,
                MediaTypeID = (int)Types.MediaType.Image,
                Desc = kalturaMediaEntry.Description
            };
            _companyProfileService.InsertMedia(media);
            int mediaid = media.MediaID;

            int profileId = companyId;

            if (profileId == 0)
            {
                profileId = MySession.ProfileID;
            }

            List<CompanyMedia> lstcompanyMedia = new List<CompanyMedia>();
            if (mediaid != 0)
            {
                CompanyMedia companyMedialogo = new CompanyMedia()
                {
                    ProfileID = profileId,
                    MediaID = mediaid,
                    IsLogo = true
                };
                lstcompanyMedia.Add(companyMedialogo);
            }

            Medium logo = _companyProfileService.GetCompanyLogoByProfileID(profileId);
            if (logo != null)
            {
                _companyProfileService.DeleteMediaById(logo.MediaID);
            }
            _companyProfileService.InsertCompanyMedia(lstcompanyMedia);
            return Json(new {location = kalturaMediaEntry.Id}, JsonRequestBehavior.AllowGet);
        }

        #region Getty Imges API

        public GetImageDetailsResult GetImageDetails(string token, List<string> imageIds)
        {
            ImageDetail imageDetail = new ImageDetail();
            GetImageDetailsResponse getImageDetailsResponse = imageDetail.GetImageDetails(token, imageIds);

            if (getImageDetailsResponse != null)
            {
                return getImageDetailsResponse.GetImageDetailsResult;
            }
            else
            {
                return null;
            }
        }

        public CreateSessionResult CreateSession()
        {
            CreateSession session = new CreateSession();
            CreateSessionTokenResponse createSessionTokenResponse = session.GetCreateSessionResponse();
            if (createSessionTokenResponse != null)
            {
                return createSessionTokenResponse.CreateSessionResult;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Search For Images Result
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public SearchForImagesResult SearchForImagesResult(string key, int itemStartNumber)
        {
            var createSessionResult = CreateSession();
            var searchForCreativeImage = new SearchForCreativeImages();
            if (createSessionResult != null)
            {
                var token = createSessionResult.Token;
                var searchForImagesResult = searchForCreativeImage.Search(token, key, itemStartNumber).SearchForImagesResult;
                return searchForImagesResult;
            }
            return new SearchForImagesResult();
        }

        public GetImageDetailsResult GetImageDetails(string key, int itemStartNumber)
        {
            CreateSessionResult createSessionResult = CreateSession();
            SearchForCreativeImages searchForCreativeImage = new SearchForCreativeImages();
            List<string> imageIds = new List<string>();

            if (createSessionResult != null)
            {
                string token = createSessionResult.Token;
                //string accountId = createSessionResult.AccountId;
                SearchForImagesResult searchForImagesResult = searchForCreativeImage.Search(token, key, itemStartNumber).SearchForImagesResult; ;
               
                if (searchForImagesResult != null)
                {
                    //imageIds = searchForImagesResult.Images.Select(m => m.ImageId).ToList();
                    foreach (Image item in searchForImagesResult.Images)
                    {
                        imageIds.Add(item.ImageId.ToString());
                    }
                }

                return GetImageDetails(token, imageIds);
            }
            else
            {
                return null;
            }
        }

        public CreateDownloadRequestResult CreateDownload(List<Image> images)
        {
            CreateSessionResult session = CreateSession();
            if (session != null)
            {
                string token = session.Token;

                string secureToken = session.SecureToken;

                CreateDownloadRequests createDownload = new CreateDownloadRequests();
                List<DownloadItem> downloadItems = new List<DownloadItem>();

                GetLargestImageDownloadAuthorizations downImageList = new GetLargestImageDownloadAuthorizations();
                List<ImageAuth> imageAuth = downImageList.GetLargestDownloadForImages(token, images).GetLargestImageDownloadAuthorizationsResult.Images;

                foreach (var item in imageAuth)
                {

                    DownloadItem downloadItem = new DownloadItem
                    {
                        DownloadToken = item.Authorizations.FirstOrDefault().DownloadToken
                    };
                    downloadItems.Add(downloadItem);
                }

                CreateDownloadResponse createDownloadResponse = createDownload.CreateRequest(secureToken, downloadItems);
                if (createDownloadResponse != null)
                {
                    return createDownloadResponse.CreateDownloadRequestResult;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Checkout Paypal

        public ActionResult Checkout()
        {
            var postProcessPaymentRequest = new PostProcessPaymentRequest();
            int companyId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            var images = _gettyImageService.GetGettyImageByProfile(companyId, (int)Types.GettyImageStatus.Pending);
            double price = 0.99;
            if (images == null || images.Count == 0)
                return RedirectToAction("image");

            double totaldue = images.Count * 0.99;

            var order = new Order()
            {
                OrderGuID = Guid.NewGuid().ToString(),
                CustID = MySession.CustID,
                ProfileID = companyId,
                PaymentMethodID = (int)Types.PaymentMethod.Paypal,
                PaymentStatusID = (int)Types.PaymentStatus.Paid,
                OrderStatusID = (int)Types.AppointmentStatus.Pending,
                AppointmentAdditionalFee = MyApp.Settings.PaySetting.AppointmentAdditionalFee,
                PercentPaymentFee = MyApp.Settings.PaySetting.PercentPaymentFee,
                TransactionAdditionalFee = MyApp.Settings.PaySetting.TransactionAdditionalFee,
                PurchaseOrderNumber = UtilityHelper.GenerateRandomDigitCode(8),
                OrderTotal = (decimal)totaldue,
                OrderSubtotal = (decimal)totaldue,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow
            };
            _orderService.InsertOrder(order);
            foreach (var item in images)
            {
                var obj = new OrderGettyImageDetail
                {
                    GettyImageId = item.Id,
                    OrderID = order.OrderID,
                    PixelHeight = item.PixelHeight,
                    PixelWidth = item.PixelWidth,
                    OrderDetailGuid = Guid.NewGuid().ToString(),
                    Price = (decimal)price,
                    UnitPrice = (decimal)price,
                    UrlAttachment = item.UrlAttachment,
                    UrlPreview = item.UrlPreview,
                    UrlThumb = item.UrlThumb,
                    Tags = item.Tags
                };
                _orderService.InsertOrderGettyimage(obj);
            }

            postProcessPaymentRequest.Order = order;
            return Redirect(this.PostProcessPayment(postProcessPaymentRequest));
        }

        public ActionResult Success()
        {
            int companyId = this.ProfileId != 0 ? this.ProfileId : MySession.ProfileID;
            var images = _gettyImageService.GetGettyImageByProfile(companyId, (int)Types.GettyImageStatus.Pending);
            foreach (var item in images)
            {
                item.Status = (int)Types.GettyImageStatus.Paid;
                _gettyImageService.UpdateGettyImage(item);
            }
            _gettyImageService.SaveToCompanyMedia(images);

            return RedirectToAction("image", new { companyId = companyId });
        }

        private string GetPaypalUrl()
        {
            return ConfigurationManager.AppSettings["UseSandbox"].ToBool() ?
                "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
                "https://www.paypal.com/us/cgi-bin/webscr";
        }

        private string BusinessEmail()
        {
            return MyApp.Settings.PaySetting.PaypalAccount;
        }

        public string PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var builder = new StringBuilder();
            builder.Append(GetPaypalUrl());
            string cmd = string.Empty;
            bool PassProductNamesAndTotals = false;
            if (PassProductNamesAndTotals)
            {
                cmd = "_cart";
            }
            else
            {
                cmd = "_xclick";
            }
            builder.AppendFormat("?cmd={0}&business={1}", cmd, HttpUtility.UrlEncode(this.BusinessEmail()));
            if (PassProductNamesAndTotals)
            {
                builder.AppendFormat("&upload=1");

                //get the items in the cart
                decimal cartTotal = decimal.Zero;
                var cartItems = postProcessPaymentRequest.Order.OrderGettyImageDetails;
                int x = 1;
                if (cartItems != null)
                {
                    foreach (var item in cartItems)
                    {
                        var unitPriceExclTax = item.UnitPrice;
                        var priceExclTax = item.Price;
                        //round
                        var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax.Value, 2);
                        
                        builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(item.Title));
                        builder.AppendFormat("&amount_" + x + "={0}", unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                        builder.AppendFormat("&quantity_" + x + "={0}", item.Quantity);
                        x++;
                        cartTotal += priceExclTax.Value;
                    }
                }
                               
            }
            else
            {               
                builder.AppendFormat("&item_name=Order Number {0}", postProcessPaymentRequest.Order.OrderID);
                var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal.Value, 2);
                builder.AppendFormat("&amount={0}", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            builder.AppendFormat("&custom={0}", postProcessPaymentRequest.Order.OrderGuID);
            builder.AppendFormat("&charset={0}", "utf-8");
            builder.Append(string.Format("&no_note=1&currency_code={0}", HttpUtility.UrlEncode(ConfigManager.currencyCode)));
            builder.AppendFormat("&invoice={0}", postProcessPaymentRequest.Order.OrderID);
            builder.AppendFormat("&rm=2", new object[0]);
           
            string returnUrl = EmailHelper.GetStoreHost() + "companysetup/Success?companyId=" + postProcessPaymentRequest.Order.ProfileID;
            string cancelReturnUrl = EmailHelper.GetStoreHost() + "companysetup/image";
            builder.AppendFormat("&return={0}&cancel_return={1}", HttpUtility.UrlEncode(returnUrl), HttpUtility.UrlEncode(cancelReturnUrl));
                       
            return builder.ToString();
        }


        #endregion checkout

    }
}