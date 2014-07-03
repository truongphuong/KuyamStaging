using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Utility
{
    public static class KalturaHelper
    {
        public static string GetEmbedVideo(string kalturaId, string content)
        {
            string var = "<object id=\"kaltura_player_1346066766\" name=\"kaltura_player_1346066766\" type=\"application/x-shockwave-flash\" " +
                         "allowFullScreen=\"true\" allowNetworking=\"all\" allowScriptAccess=\"always\" height=\"145\" width=\"233\" bgcolor=\"#000000\"" +
                //"xmlns:dc=\"http://purl.org/dc/terms/\" xmlns:media=\"http://search.yahoo.com/searchmonkey/media/\" rel=\"media:video\"  " +
                //"resource=\"http://www.kaltura.com/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\" " +
                //"data=\"http://www.kaltura.com/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\">" +
                        "<param name=\"allowFullScreen\" value=\"true\" /><param name=\"allowNetworking\" value=\"all\" />" +
                        "<param name=\"wmode\" value=\"transparent\" />" +
                        "<param name=\"allowScriptAccess\" value=\"always\" /><param name=\"bgcolor\" value=\"#000000\" />" +
                //"<param name=\"flashVars\" value=\"&\" /><param name=\"movie\" value=\"http://www.kaltura.com/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\" />" +
                //"<a href=\"http://corp.kaltura.com/products/video-platform-features\">Video Platform</a> <a href=\"http://corp.kaltura.com/Products/Features/Video-Management\">Video Management</a>" +
                //"<a href=\"http://corp.kaltura.com/Video-Solutions\">Video Solutions</a> <a href=\"http://corp.kaltura.com/Products/Features/Video-Player\">Video Player</a> " +

                        "<a rel=\"media:thumbnail\" href=\"http://cdnbakmi.kaltura.com/p/801372/sp/80137200/thumbnail/entry_id/{0}/width/120/height/90/bgcolor/000000/type/2\"></a> " +
                        "<span property=\"dc:description\" content=\"\"></span><span property=\"media:title\" content=\"{1}\">" +
                        "</span> <span property=\"media:width\" content=\"560\"></span><span property=\"media:height\" content=\"395\"></span> <span property=\"media:type\" content=\"application/x-shockwave-flash\"></span> </object>";
            return string.Format(var, kalturaId, content);
        }

        public static string GetEmbedVideothumbnail(string kalturaId, int mediaid, int h, int w, bool useSsl = true)
        {
            string katuraDomain = "https://www.kaltura.com";
            //if (useSsl)
            //{
            //    katuraDomain = "https://www.kaltura.com";
            //}
            string var = "<object id=\"kaltura_player_1346066766\" name=\"kaltura_player_1346066766\" type=\"application/x-shockwave-flash\" " +
                        "allowFullScreen=\"true\" allowNetworking=\"all\" allowScriptAccess=\"always\" height=\"{2}\" width=\"{3}\" bgcolor=\"#000000\"" +
                //"xmlns:dc=\"http://purl.org/dc/terms/\" xmlns:media=\"http://search.yahoo.com/searchmonkey/media/\" rel=\"media:video\"  " +
                //"resource=\"http://www.kaltura.com/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\" " +
                       "data=\"{4}/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\">" +
                       "<param name=\"allowFullScreen\" value=\"true\" /><param name=\"allowNetworking\" value=\"all\" />" +
                       "<param name=\"wmode\" value=\"transparent\" />" +
                       "<param name=\"allowScriptAccess\" value=\"always\" /><param name=\"bgcolor\" value=\"#000000\" />" +
                //"<param name=\"flashVars\" value=\"&\" /><param name=\"movie\" value=\"http://www.kaltura.com/index.php/kwidget/cache_st/1346066766/wid/_1094702/uiconf_id/12896282/entry_id/{0}\" />" +
                //"<a href=\"http://corp.kaltura.com/products/video-platform-features\">Video Platform</a> <a href=\"http://corp.kaltura.com/Products/Features/Video-Management\">Video Management</a>" +
                //"<a href=\"http://corp.kaltura.com/Video-Solutions\">Video Solutions</a> <a href=\"http://corp.kaltura.com/Products/Features/Video-Player\">Video Player</a> " +

                       //"<a rel=\"media:thumbnail\" href=\"http://cdnbakmi.kaltura.com/p/801372/sp/80137200/thumbnail/entry_id/{0}/width/120/height/90/bgcolor/000000/type/2\"></a> " +
                //"<span property=\"dc:description\" content=\"\"></span><span property=\"media:title\" content=\"http://cdnbakmi.kaltura.com/p/801372/sp/80137200/flvclipper/entry_id/{0}\"></span>" +
                       "<span property=\"media:width\" content=\"560\"></span><span property=\"media:height\" content=\"395\"></span>" +
                       "<span property=\"media:type\" content=\"application/x-shockwave-flash\"></span> </object>" +
                       "<input type=\"hidden\" id=\"hdmediaid\" name=\"hdmediaid\" value=\"{1}\" />" +
                       "<input type=\"hidden\" id=\"hdmediadata\" name=\"hdmediadata\" value=\"{0}\"/>";

            return string.Format(var, kalturaId, mediaid, h, w, katuraDomain);

        }

        public static string GetKalturaUrl(string kalturaId, int width, int heigth, int type = 0)
        {
            return string.Format("https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{2}/type/{1}/quality/{5}/width/{3}/height/{4}/", ConfigManager.KULTURA_PARTNER_ID, type==0?ConfigManager.KULTURA_CROP_TYPE:type.ToString(), kalturaId, width, heigth, ConfigManager.Quality);
        }


        public static string GetKalturaUrl(string kalturaId, int width, int heigth, int srcX, int srcY, int srcW, int srcH)
        {

            return
                string.Format(
                    "https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{2}/type/{1}/quality/{5}/width/{3}/height/{4}/src_x/{6}/src_y/{7}/src_w/{8}/src_h/{9}/",
                    ConfigManager.KULTURA_PARTNER_ID, 1, kalturaId, width, heigth,
                    ConfigManager.Quality, srcX, srcY, srcW, srcH);
        }
        public static string GetKalturaUrl(int postId, string kalturaId, int mediaType = (int)Types.MediaCropType.MainPostBeforeLogin, int width = 0, int heigth = 0)
        {

            int x = 0;
            int y = 0;
            int w = width;
            int h = heigth;
            string url = string.Format("https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{1}/quality/{2}/type/3/width/{3}/height/{4}", ConfigManager.KULTURA_PARTNER_ID, kalturaId, ConfigManager.Quality, width, heigth);
            var media = DAL.GetPostMedia(postId).FirstOrDefault(m => m.MediaType == mediaType);
            if (media != null)
            {
                width = width == 0 ? media.Fr_width.Value : width;
                heigth = heigth == 0 ? media.Fr_height.Value : heigth;
                x = media.Crop_x ?? 0;
                y = media.Crop_y ?? 0;
                w = media.Rel_width ?? 0;
                h = media.Rel_height ?? 0;
                if (media.Crop_x > 0 || media.Crop_y > 0 || media.Rel_width > 0 || media.Rel_height > 0)
                    url = string.Format("https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{1}/quality/{2}/type/1/src_x/{3}/src_y/{4}/src_w/{5}/src_h/{6}/width/{7}/height/{8}", ConfigManager.KULTURA_PARTNER_ID, kalturaId, ConfigManager.Quality, x, y, w, h, width, heigth);

            }

            return url;
        }
        public static string GetKalturaLandingUrl(int postId,string kalturaId, int width = 0, int heigth = 0)
        {

            int x = 0;
            int y = 0;
            int w = width;
            int h = heigth;
            string url = string.Empty;//string.Format("https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{1}/quality/{2}/type/3/width/{3}/height/{4}", ConfigManager.KULTURA_PARTNER_ID, kalturaId, ConfigManager.Quality, width, heigth);
            var media = DAL.GetPostMedia(postId).FirstOrDefault();
            if (media != null)
            {  
                width = width == 0 ? media.Fr_width.Value : width;
                heigth = heigth == 0 ? media.Fr_height.Value : heigth;
                x = media.Crop_x ?? 0;
                y = media.Crop_y ?? 0;
                w = media.Rel_width ?? 0;
                h = media.Rel_height ?? 0;
                if (media.Crop_x > 0 || media.Crop_y > 0 || media.Rel_width > 0 || media.Rel_height > 0)
                    url = string.Format("https://cdnsecakmi.kaltura.com/p/{0}/thumbnail/entry_id/{1}/quality/{2}/type/1/src_x/{3}/src_y/{4}/src_w/{5}/src_h/{6}/width/{7}/height/{8}", ConfigManager.KULTURA_PARTNER_ID, kalturaId, ConfigManager.Quality, x, y, w, h, width, heigth);

            }

            return url;
        }
    }
}
