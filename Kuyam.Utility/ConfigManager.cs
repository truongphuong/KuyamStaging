using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace Kuyam.Utility
{
    public static class ConfigManager
    {
        const string _DefaultDistance = "defaultDistance";
        const string _Defaultlongitude = "defaultLongitude";
        const string _DefaultLatitude = "defaultLatitude";

        #region Encrypt
        private const string _CryptKey = "CryptKey";
        private const string _IVKey = "IVKey";
        #endregion Encrypt

        #region Kaltura
        private const string _KULTURA_PARTNER_ID = "PARTNER_ID";
        private const string _KULTURA_SECRET = "SECRET";
        private const string _KULTURA_ADMIN_SECRET = "ADMIN_SECRET";
        private const string _KULTURA_SERVICE_URL = "SERVICE_URL";
        private const string _KULTURA_USER_ID = "USER_ID";
        private const string _KULTURA_CROP_TYPE = "CROP_TYPE";
        private const string _Quality = "Quality";
        #endregion Kaltura

        #region Encrypt
        public static string CryptKey
        {
            get
            {
                if (ConfigurationManager.AppSettings[_CryptKey] == null)
                {
                    return "kuyamKey@1957.com";
                }
                else
                {
                    return ConfigurationManager.AppSettings[_CryptKey];
                }
            }
        }

        public static string IVKey
        {
            get
            {
                if (ConfigurationManager.AppSettings[_IVKey] == null)
                {
                    return "kuyamIV@1957.com";
                }
                else
                {
                    return ConfigurationManager.AppSettings[_IVKey];
                }
            }
        }

        public static int CookieExpires
        {
            get
            {
                if (ConfigurationManager.AppSettings["CookieExpires"] == null)
                    return 24 * 365;
                int cookieExpires = Convert.ToInt32(ConfigurationManager.AppSettings["CookieExpires"]);
                return cookieExpires;
            }
        }

        #endregion Encrypt

        #region Kaltura

        public static string StorageRoot
        {
            get { return Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/UploadMedia/")); }
        }

        public static void DeleteFile(string fullPath)
        {
            var filePath = fullPath;
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public static int KULTURA_PARTNER_ID
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_PARTNER_ID] == null)
                {
                    return 0;
                }
                else
                {
                    int partnerId = 0;
                    int.TryParse(ConfigurationManager.AppSettings[_KULTURA_PARTNER_ID], out partnerId);
                    return partnerId;
                }
            }
        }

        public static int Quality
        {
            get
            {
                if (ConfigurationManager.AppSettings[_Quality] == null)
                {
                    return 0;
                }
                else
                {
                    int value = 0;
                    int.TryParse(ConfigurationManager.AppSettings[_Quality], out value);
                    return value;
                }
            }
        }

        public static string KULTURA_SECRET
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_SECRET] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ConfigurationManager.AppSettings[_KULTURA_SECRET];
                }
            }
        }

        public static string KULTURA_ADMIN_SECRET
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_ADMIN_SECRET] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ConfigurationManager.AppSettings[_KULTURA_ADMIN_SECRET];
                }
            }
        }

        public static string KULTURA_SERVICE_URL
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_SERVICE_URL] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ConfigurationManager.AppSettings[_KULTURA_SERVICE_URL];
                }
            }
        }

        public static string KULTURA_USER_ID
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_USER_ID] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ConfigurationManager.AppSettings[_KULTURA_USER_ID];
                }
            }
        }

        public static string KULTURA_CROP_TYPE
        {
            get
            {
                if (ConfigurationManager.AppSettings[_KULTURA_CROP_TYPE] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ConfigurationManager.AppSettings[_KULTURA_CROP_TYPE];
                }
            }
        }

        #endregion Kaltura

        #region Google API

        public static double DefaultDistance
        {
            get
            {
                double defaultDistance = 80.467;
                if (ConfigurationManager.AppSettings[_DefaultDistance] == null)
                {
                    return defaultDistance;
                }
                else
                {
                    double distance = 0;
                    if (double.TryParse(ConfigurationManager.AppSettings[_DefaultDistance], out distance))
                    {
                        return distance;
                    }
                    else
                    {
                        return defaultDistance;
                    }
                }
            }
        }
        public static double Defaultlongitude
        {
            get
            {
                double defaultLongitude = 34.0194543;
                if (ConfigurationManager.AppSettings[_Defaultlongitude] == null)
                {
                    return defaultLongitude;
                }
                else
                {
                    double dLongitude = 0;
                    if (double.TryParse(ConfigurationManager.AppSettings[_Defaultlongitude], out dLongitude))
                    {
                        return dLongitude;
                    }
                    else
                    {
                        return defaultLongitude;
                    }
                }
            }
        }

        public static double DefaultLatitude
        {
            get
            {
                double defaultLatitude = -118.4911912;
                if (ConfigurationManager.AppSettings[_DefaultLatitude] == null)
                {
                    return defaultLatitude;
                }
                else
                {
                    double dLatitude = 0;
                    if (double.TryParse(ConfigurationManager.AppSettings[_DefaultLatitude], out dLatitude))
                    {
                        return dLatitude;
                    }
                    else
                    {
                        return defaultLatitude;
                    }
                }
            }
        }
        #endregion Google API


        #region paypal
        public static string cancelUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["CancelUrlPreApprove"] + string.Empty;
            }
        }

        public static string returnUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ReturnUrlPreApprove"] + string.Empty;
            }
        }

        public static string PayUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["PayUrl"] + string.Empty;
            }
        }

        public static string currencyCode
        {
            get
            {
                return ConfigurationManager.AppSettings["CurrencyCode"] + string.Empty;
            }
        }

        public static string ActionType
        {
            get
            {
                return ConfigurationManager.AppSettings["ActionType"] + string.Empty;
            }
        }

        public static string EmailReceive1
        {
            get
            {
                return ConfigurationManager.AppSettings["Email_Receive1"].ToString();
            }
        }

        public static string EmailReceive2
        {
            get
            {
                return ConfigurationManager.AppSettings["Email_Receive2"].ToString();
            }
        }

        public static string PayDate
        {
            get
            {
                return ConfigurationManager.AppSettings["PayPreDate"].ToString();
            }
        }

        public static string PaypalSigupAccount
        {
            get
            {
                return ConfigurationManager.AppSettings["PaypalSigupAccount"];
            }
        }

        public static int FailedCount
        {
            get
            {
                int failedCount = 0;
                int.TryParse(ConfigurationManager.AppSettings["AttemptCount"], out failedCount);
                return failedCount;
            }
        }

        public static int LockedTime
        {
            get
            {
                int time = 0;
                int.TryParse(ConfigurationManager.AppSettings["GiftCardLockedTime"], out time);
                return time;
            }
        }

        #endregion

    }
}
