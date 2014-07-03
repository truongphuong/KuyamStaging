using Kuyam.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Authentication
{
    public class AuthenticationHipmob
    {
        #region Properties
        private readonly SettingService _settingService;
        #endregion

        #region Authentication variables


        #endregion

        public AuthenticationHipmob(SettingService settingService)
        {
            this._settingService = settingService;
            this.AppId = _settingService.GetSetting("hmb_appid");
            this.UserName = _settingService.GetSetting("hmb_user");
            this.APIKey = _settingService.GetSetting("hmb_api_key");
            this.BaseUrl = _settingService.GetSetting("hmb_baseurl");

        }

        public string AppId { get; set; }
        public string UserName { get; set; }

        public string APIKey { get; set; }

        public string BaseUrl { get; set; }

        public string AccessToken
        {
            get
            {
                string keytext = string.Format("{0}:{1}", this.UserName, this.APIKey);
                return Base64Encode(keytext);
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

    }
}
