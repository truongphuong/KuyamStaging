using Kuyam.Domain.Services;
using Kuyam.Domain.SupEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;

namespace Kuyam.Domain.Authentication
{
    public class AuthenticationAT
    {

        #region Properties
        private readonly SettingService _settingService;
        #endregion

        #region Authentication variables


        #endregion

        public AuthenticationAT(SettingService settingService)
        {
            this._settingService = settingService;
            this.ConsumerKey = _settingService.GetSetting("api_key");
            this.ConsumerSecret = _settingService.GetSetting("secret_key");
            this.Scope = _settingService.GetSetting("scope");
            this.EndPoint = _settingService.GetSetting("endPoint");
            this.RefreshTokenExpiresIn = int.Parse(_settingService.GetSetting("refreshTokenExpiresIn"));
            this.AccessTokenFilePath = _settingService.GetSetting("AccessTokenFilePath");
            this.SendSMSURL = _settingService.GetSetting("SendSMSURL");

        }

        public  string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }
            else
            {
                //not hosted. For example, run in unit tests
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
                return Path.Combine(baseDirectory, path);
            }
        }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string AuthorizeURl { get; set; }

        public string AccessTokenURL { get; set; }

        public string CallbackURL { get; set; }

        public string Scope { get; set; }

        public string AuthCode { get; set; }

        public string EndPoint { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenExpiryTime { get; set; }

        public string RefreshToken { get; set; }

        public int RefreshTokenExpiresIn { get; set; }

        public string RefreshTokenExpiryTime { get; set; }

        public string AccessTokenFilePath { get; set; }

        public string SendSMSURL { get; set; }


        public bool ReadAndGetAccessToken(AccessTokenType type, ref string responseString)
        {
            bool result = true;
            if (this.ReadAccessTokenFile(ref responseString) == false)
            {
                result = this.AccessTokenGet(type, ref responseString);
            }
            else
            {
                string tokenValidity = this.IsTokenValid();
                if (tokenValidity == "REFRESH_TOKEN")
                {
                    result = this.RefreshAccessToken(ref responseString);
                }
                else if (string.Compare(tokenValidity, "INVALID_ACCESS_TOKEN") == 0)
                {
                    result = this.AccessTokenGet(type, ref responseString);
                }
            }

            if (this.AccessToken == null || this.AccessToken.Length <= 0)
            {
                return false;
            }
            else
            {
                return result;
            }
        }

        private string IsTokenValid()
        {
            try
            {
                DateTime currentServerTime = DateTime.UtcNow.ToLocalTime();
                if (currentServerTime >= DateTime.Parse(this.AccessTokenExpiryTime))
                {
                    if (currentServerTime >= DateTime.Parse(this.RefreshTokenExpiryTime))
                    {
                        return "INVALID_ACCESS_TOKEN";
                    }
                    else
                    {
                        return "REFRESH_TOKEN";
                    }
                }
                else
                {
                    return "VALID_ACCESS_TOKEN";
                }
            }
            catch
            {
                return "INVALID_ACCESS_TOKEN";
            }
        }

        private bool ReadAccessTokenFile(ref string message)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            try
            {
                fileStream = new FileStream(this.MapPath(this.AccessTokenFilePath), FileMode.OpenOrCreate, FileAccess.Read);
                streamReader = new StreamReader(fileStream);
                this.AccessToken = streamReader.ReadLine();
                this.AccessTokenExpiryTime = streamReader.ReadLine();
                this.RefreshToken = streamReader.ReadLine();
                this.RefreshTokenExpiryTime = streamReader.ReadLine();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                if (null != streamReader)
                {
                    streamReader.Close();
                }

                if (null != fileStream)
                {
                    fileStream.Close();
                }
            }

            if ((this.AccessToken == null) || (this.AccessTokenExpiryTime == null) || (this.RefreshToken == null) || (this.RefreshTokenExpiryTime == null))
            {
                return false;
            }

            return true;
        }

        private bool AccessTokenGet(AccessTokenType type, ref string message)
        {
            FileStream fileStream = null;
            Stream postStream = null;
            StreamWriter streamWriter = null;

            try
            {
                DateTime currentServerTime = DateTime.UtcNow.ToLocalTime();

                WebRequest accessTokenRequest = System.Net.HttpWebRequest.Create(this.EndPoint + "/oauth/token");
                accessTokenRequest.Method = "POST";
                string oauthParameters = string.Empty;

                if (type == AccessTokenType.Authorization_Code)
                {
                    oauthParameters = "client_id=" + this.ConsumerKey + "&client_secret=" + this.ConsumerSecret + "&code=" + this.AuthCode + "&grant_type=authorization_code&scope=" + this.Scope;
                }
                else if (type == AccessTokenType.ClientCredential)
                {
                    oauthParameters = "client_id=" + this.ConsumerKey + "&client_secret=" + this.ConsumerSecret + "&grant_type=client_credentials&scope=" + this.Scope;
                }

                accessTokenRequest.ContentType = "application/x-www-form-urlencoded";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] postBytes = encoding.GetBytes(oauthParameters);
                accessTokenRequest.ContentLength = postBytes.Length;

                postStream = accessTokenRequest.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);

                WebResponse accessTokenResponse = accessTokenRequest.GetResponse();
                using (StreamReader accessTokenResponseStream = new StreamReader(accessTokenResponse.GetResponseStream()))
                {
                    string jsonAccessToken = accessTokenResponseStream.ReadToEnd().ToString();
                    JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();

                    AccessTokenResponse deserializedJsonObj = (AccessTokenResponse)deserializeJsonObject.Deserialize(jsonAccessToken, typeof(AccessTokenResponse));

                    this.AccessToken = deserializedJsonObj.access_token;
                    this.AccessTokenExpiryTime = currentServerTime.AddSeconds(Convert.ToDouble(deserializedJsonObj.expires_in)).ToString();
                    this.RefreshToken = deserializedJsonObj.refresh_token;

                    DateTime refreshExpiry = currentServerTime.AddHours(this.RefreshTokenExpiresIn);

                    if (deserializedJsonObj.expires_in.Equals("0"))
                    {
                        int defaultAccessTokenExpiresIn = 100; // In Yearsint yearsToAdd = 100;
                        this.AccessTokenExpiryTime = currentServerTime.AddYears(defaultAccessTokenExpiresIn).ToLongDateString() + " " + currentServerTime.AddYears(defaultAccessTokenExpiresIn).ToLongTimeString();
                    }

                    this.RefreshTokenExpiryTime = refreshExpiry.ToLongDateString() + " " + refreshExpiry.ToLongTimeString();

                    fileStream = new FileStream(this.MapPath(this.AccessTokenFilePath), FileMode.OpenOrCreate, FileAccess.Write);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine(this.AccessToken);
                    streamWriter.WriteLine(this.AccessTokenExpiryTime);
                    streamWriter.WriteLine(this.RefreshToken);
                    streamWriter.WriteLine(this.RefreshTokenExpiryTime);

                    // Close and clean up the StreamReader
                    accessTokenResponseStream.Close();
                    return true;
                }
            }
            catch (WebException we)
            {
                string errorResponse = string.Empty;

                try
                {
                    using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                    {
                        errorResponse = sr2.ReadToEnd();
                        sr2.Close();
                    }
                }
                catch
                {
                    errorResponse = "Unable to get response";
                }

                message = errorResponse + Environment.NewLine + we.ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                if (null != postStream)
                {
                    postStream.Close();
                }

                if (null != streamWriter)
                {
                    streamWriter.Close();
                }

                if (null != fileStream)
                {
                    fileStream.Close();
                }
            }
            return false;
        }


        private bool RefreshAccessToken(ref string message)
        {
            FileStream fileStream = null;
            Stream postStream = null;
            StreamWriter streamWriter = null;
            try
            {
                DateTime currentServerTime = DateTime.UtcNow.ToLocalTime();

                WebRequest accessTokenRequest = System.Net.HttpWebRequest.Create(this.EndPoint + "/oauth/token");
                accessTokenRequest.Method = "POST";

                string oauthParameters = "grant_type=refresh_token&client_id=" + this.ConsumerKey + "&client_secret=" + this.ConsumerSecret + "&refresh_token=" + this.RefreshToken;
                accessTokenRequest.ContentType = "application/x-www-form-urlencoded";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] postBytes = encoding.GetBytes(oauthParameters);
                accessTokenRequest.ContentLength = postBytes.Length;

                postStream = accessTokenRequest.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);

                WebResponse accessTokenResponse = accessTokenRequest.GetResponse();
                using (StreamReader accessTokenResponseStream = new StreamReader(accessTokenResponse.GetResponseStream()))
                {
                    string accessTokenJSon = accessTokenResponseStream.ReadToEnd().ToString();
                    JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();

                    AccessTokenResponse deserializedJsonObj = (AccessTokenResponse)deserializeJsonObject.Deserialize(accessTokenJSon, typeof(AccessTokenResponse));
                    this.AccessToken = deserializedJsonObj.access_token.ToString();
                    DateTime accessTokenExpiryTime = currentServerTime.AddMilliseconds(Convert.ToDouble(deserializedJsonObj.expires_in.ToString()));
                    this.RefreshToken = deserializedJsonObj.refresh_token.ToString();

                    fileStream = new FileStream(this.MapPath(this.AccessTokenFilePath), FileMode.OpenOrCreate, FileAccess.Write);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine(this.AccessToken);
                    streamWriter.WriteLine(this.AccessTokenExpiryTime);
                    streamWriter.WriteLine(this.RefreshToken);

                    // Refresh token valids for 24 hours
                    DateTime refreshExpiry = currentServerTime.AddHours(this.RefreshTokenExpiresIn);
                    this.RefreshTokenExpiryTime = refreshExpiry.ToLongDateString() + " " + refreshExpiry.ToLongTimeString();
                    streamWriter.WriteLine(refreshExpiry.ToLongDateString() + " " + refreshExpiry.ToLongTimeString());

                    accessTokenResponseStream.Close();
                    return true;
                }
            }
            catch (WebException we)
            {
                string errorResponse = string.Empty;

                try
                {
                    using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                    {
                        errorResponse = sr2.ReadToEnd();
                        sr2.Close();
                    }
                }
                catch
                {
                    errorResponse = "Unable to get response";
                }

                message = errorResponse + Environment.NewLine + we.ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                if (null != postStream)
                {
                    postStream.Close();
                }

                if (null != streamWriter)
                {
                    streamWriter.Close();
                }

                if (null != fileStream)
                {
                    fileStream.Close();
                }
            }

            return false;
        }        

        public class AccessTokenResponse
        {
            /// <summary>
            /// Gets or sets access token
            /// </summary>
            public string access_token { get; set; }

            /// <summary>
            /// Gets or sets refresh token
            /// </summary>
            public string refresh_token { get; set; }

            /// <summary>
            /// Gets or sets expires in
            /// </summary>
            public string expires_in { get; set; }
        }

    }


    public enum AccessTokenType
    {
        /// <summary>
        /// Access Token Type is based on Client Credential Mode
        /// </summary>
        ClientCredential,
        /// <summary>
        /// Access Token Type is based on Authorization Code
        /// </summary>
        Authorization_Code,

        /// <summary>
        /// Access Token Type is based on Refresh Token
        /// </summary>
        Refresh_Token
    }

}
