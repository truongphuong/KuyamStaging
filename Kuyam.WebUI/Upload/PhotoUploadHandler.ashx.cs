﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Kuyam.Domain;
using System.Net;
using Kaltura;
using Kuyam.Repository.Infrastructure;
using System.Web.SessionState;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Upload
{
    /// <summary>
    /// Summary description for UploadHandler
    /// </summary>
    public class PhotoUploadHandler : IHttpHandler, IRequiresSessionState
    {
        private readonly JavaScriptSerializer js;

        private string StorageRoot
        {
            get { return Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/UploadMedia/")); } //Path should! always end with '/'
        }

        public PhotoUploadHandler()
        {            
            js = new JavaScriptSerializer();
            js.MaxJsonLength = 41943040;
        }

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            HandleMethod(context);
        }

        // Handle request based on method
        private void HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "HEAD":
                case "GET":
                    if (GivenFilename(context)) DeliverFile(context);
                    //else ListCurrentFiles(context);
                    break;

                case "POST":
                case "PUT":
                    UploadFile(context);
                    break;

                case "DELETE":
                    //DeleteFile(context);
                    break;

                case "OPTIONS":
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private static void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
            context.Response.StatusCode = 200;
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

        // Upload file to the server
        private void UploadFile(HttpContext context)
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                UploadWholeFile(context, statuses);
            }
            else
            {
                UploadPartialFile(headers["X-File-Name"], context, statuses);
            }

            WriteJsonIframeSafe(context, statuses);
        }

        // Upload partial file
        private void UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var inputStream = context.Request.Files[0].InputStream;
            var fullName = StorageRoot + Path.GetFileName(fileName);

            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(new FilesStatus(null,new FileInfo(fullName)));
        }

        // Upload entire file
        private void UploadWholeFile(HttpContext context, List<FilesStatus> statuses)
        {
            string Id = context.Request["companyID"];
            int profileId = 0;
            int.TryParse(Id, out profileId);

            if (profileId == 0){
                profileId = MySession.ProfileID;
            }

            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                string mediaid = string.Empty;
                var fullPath = StorageRoot + Path.GetFileName(file.FileName);
                string fileName = file.FileName;
                file.SaveAs(fullPath);
                FileStream _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                KalturaMediaEntry kalturaMediaEntry = KalturaService.StartSessionAndUploadMedia(_fileStream, Kaltura.KalturaMediaType.IMAGE, fileName);
                _fileStream.Flush();
                _fileStream.Close();
                DeleteFile(fullPath);

                CompanyProfileService _companyProfileService = EngineContext.Current.Resolve<CompanyProfileService>();
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
                mediaid = media.MediaID.ToString();

                _companyProfileService.InsertCompanyMedia(media, profileId);

                string fullName = Path.GetFileName(file.FileName);
                statuses.Add(new FilesStatus(kalturaMediaEntry, mediaid, fullName, file.ContentLength, fullPath));

            }
        }

        private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                    context.Response.ContentType = "application/json";
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }

        private static bool GivenFilename(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request["f"]);
        }

        private void DeliverFile(HttpContext context)
        {
            var filename = context.Request["f"];
            var filePath = StorageRoot + filename;

            if (File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }
    }
}