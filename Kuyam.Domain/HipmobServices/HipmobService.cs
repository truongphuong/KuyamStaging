using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Kuyam.Domain.HipmobServices
{
    public class HipmobService : IHipmobService
    {

        public bool SendTextMessage(string deviceId, string text)
        {
            Stream postStream = null;
            try
            {
                var auth = new AuthenticationHipmob(new SettingService());
                var url = string.Format("{0}apps/{1}/devices/{2}/messages", auth.BaseUrl, auth.AppId, deviceId);
                HttpWebRequest contextRequest = (HttpWebRequest)WebRequest.Create(url);
                contextRequest.Headers.Add("Authorization", "Basic " + auth.AccessToken);
                contextRequest.Method = "POST";


                string parameters = "text=" + HttpContext.Current.Server.UrlEncode(text) +"&autocreate=true";
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] postBytes = encoding.GetBytes(parameters);

                contextRequest.ContentLength = postBytes.Length;
                string contentType = "application/x-www-form-urlencoded";
                contextRequest.ContentType = contentType;

                postStream = contextRequest.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);

                postStream.Close();               
                HttpWebResponse contextResponse = (HttpWebResponse)contextRequest.GetResponse();

                if (contextResponse.StatusCode == HttpStatusCode.OK)
                    return true;                
               
            }
            catch (Exception)
            {

                return false;
            }
            return false;

        }
    }
}
