// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace M2.Util
{
    public class Gravatar
    {
        public bool UseSSL { get; set; }
        public int Size { get; set; }
        public string Email { get; set; }
        public string DefaultImage { get; set; }
        public bool OutputGravatarSiteLink { get; set; }
        public string MaxAllowedRating { get; set; }

        public Gravatar()
        {
            UseSSL = false;
            Size = 80;
            DefaultImage = "http://www.site.com/default.jpg";
            OutputGravatarSiteLink = true;
            MaxAllowedRating = "G";
        }

        private static Gravatar _gravatar = null;
        public static string GetUrl(string email=null, int size=80)
        {
            if (_gravatar == null)
                _gravatar = new Gravatar();

            return _gravatar.GetImageUrl(email, size);
        }

        public string GetImageUrl(string email=null, int size=80)
        {
            Email = email;
            Size = size;
            return GetGravatarUrl();
        }
            
        public string GetGravatarUrl()
        {
            try
            {
                // if it's not in the allowed range, throw an exception:
                if (Size < 1 || Size > 512)
                    throw new ArgumentOutOfRangeException();
            }
            catch
            {
                Size = 80;
            }

            // default the image url:
            string imageUrl = "http://www.gravatar.com/avatar.php?";

            if (!string.IsNullOrEmpty(Email))
            {
                // build up image url, including MD5 hash for supplied email:
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

                UTF8Encoding encoder = new UTF8Encoding();
                MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

                byte[] hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(Email));

                StringBuilder sb = new StringBuilder(hashedBytes.Length * 2);
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    sb.Append(hashedBytes[i].ToString("X2"));
                }

                // output parameters:
                imageUrl += "gravatar_id=" + sb.ToString().ToLower();
                imageUrl += "&rating=" + MaxAllowedRating.ToString();
                imageUrl += "&blockSize=" + Size.ToString();
            }
            else if (!string.IsNullOrEmpty(DefaultImage))
            {
                imageUrl += "&default=" + HttpUtility.UrlEncode(DefaultImage);
            }

            return imageUrl;
        }
    }
}
