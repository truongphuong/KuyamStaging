using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database.BlogModels;
using System.Configuration;
using Kuyam.Domain;
using Kuyam.Domain.BlogServices;
using Kuyam.Database;
using Kuyam.Domain.KuyamServices;

namespace Kuyam.WebUI.Helpers
{
    public class BlogAuthorHelper
    {
        private readonly IBlogUserService _blogUserService;
        public BlogAuthorHelper(IBlogUserService blogUserService)
        {
            _blogUserService = blogUserService;
        }

        public BlogUser GetByUserName(string username)
        {
            var user = _blogUserService.GetProfile(username);
            if (user != null)
                user.Add("UserName", username);
            var author = (BlogUser)UtilityHelper.ConvertTo<BlogUser>(user);
            if (author.PhotoUrl != null)
            {
                var photoUrl = author.PhotoUrl.StartsWith("http://") || author.PhotoUrl.StartsWith("https://")
                     ? author.PhotoUrl
                     : ConfigurationManager.AppSettings["blogHost"] + "/image.axd?picture=/avatars/" + author.PhotoUrl;
                author.PhotoUrl = photoUrl;
            }
            else
            {
                author.PhotoUrl = "/images/placeholder.png";
            }
            return author;
        }

        /// <summary>
        /// 1: Fb account, 2: Kuyam account
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Cust GetKuyamUser(string email)
        {
            return Cust.Load(email);
        }
    }
}