using Kuyam.Database;

namespace Kuyam.WebUI.Models.Media
{
    public class GettyImageModel {
        public int Id { get; set; }
        public string GettyImageId { get; set; }
        public int ProfileId { get; set; }
        public string Title { get; set; }
        public string UrlThumb { get; set; }
        public string UrlPreview { get; set; }
        public int PixelHeight { get; set; }
        public int PixelWidth { get; set; }
        public string Tags { get; set; }
        public string LocationData { get; set; }

        public int CustId { get; set; }
        public GettyImageModel()
        {
            
        }

        public GettyImageModel(GettyImage image)
        {
            Id = image.Id;
            GettyImageId = image.GettyImageId;
            ProfileId = image.ProfileId.HasValue ? image.ProfileId.Value : 0;
            Title = image.Title;
            UrlThumb = image.UrlThumb;
            UrlPreview = image.UrlPreview;
            PixelHeight = image.PixelHeight.HasValue?image.PixelHeight.Value:0;
            PixelWidth = image.PixelWidth.HasValue ? image.PixelWidth.Value : 0;
            Tags = image.Tags;
            CustId = image.CustId.HasValue ? image.CustId.Value : 0;
            LocationData = image.LocationData;
        }
    }
}