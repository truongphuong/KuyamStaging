using System;
using System.IO;
using Kaltura;
using System.Text;
using Kuyam.Utility;

namespace Kuyam.WebUI
{
    public class FilesStatus
    {
        public const string HandlerPath = "/Upload/";

        public string group { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public string progress { get; set; }
        public string url { get; set; }
        public string thumbnail_url { get; set; }
        public string dataurl { get; set; }
        public string delete_url { get; set; }
        public string delete_type { get; set; }
        public string error { get; set; }
        public string mediaid { get; set; }
        public string kalturaid { get; set; }

        public FilesStatus() { }

        public FilesStatus(KalturaMediaEntry kalturaMediaEntry, FileInfo fileInfo)
        {
            SetValues(kalturaMediaEntry, string.Empty, fileInfo.Name, (int)fileInfo.Length, fileInfo.FullName);
        }

        public FilesStatus(KalturaMediaEntry kalturaMediaEntry, string mediaid, string fileName, int fileLength, string fullPath)
        {
            SetValues(kalturaMediaEntry, mediaid, fileName, fileLength, fullPath);
        }

        private void SetValues(KalturaMediaEntry kalturaMediaEntry, string mediaId, string fileName, int fileLength, string fullPath)
        {
            name = fileName;
            type = "image/png";
            size = fileLength;
            progress = "1.0";
            url = HandlerPath + "UploadHandler.ashx?f=" + fileName;
            delete_url = HandlerPath + "UploadHandler.ashx?f=" + fileName;
            delete_type = "DELETE";
            mediaid = mediaId;
            kalturaid = kalturaMediaEntry.Id;
            var ext = Path.GetExtension(fullPath);
            if (kalturaMediaEntry.MediaType == KalturaMediaType.VIDEO)
            {
                thumbnail_url = kalturaMediaEntry.DataUrl;
                int id = 0;
                int.TryParse(mediaId, out id);
                dataurl = KalturaHelper.GetEmbedVideothumbnail(kalturaMediaEntry.Id, id, 216, 302);
            }
            else if (kalturaMediaEntry.MediaType == KalturaMediaType.IMAGE)
            {
                thumbnail_url = kalturaMediaEntry.DataUrl;
            }
        }

        private bool IsImage(string ext)
        {
            return ext == ".gif" || ext == ".jpg" || ext == ".png";
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}