using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.WebUI.Models;
using Kuyam.Database;
using Kuyam.Database.BlogModels;
using Kuyam.WebUI.Helpers;
using Kuyam.Repository.Infrastructure;
using Kuyam.Domain;
using System.Globalization;

namespace Kuyam.WebUI.WebMapper
{
    public class Mapper
    {
        public PostModels Map(be_Posts post, List<be_Categories> categories, List<be_PostComment> comments, List<FeaturedCompanyModels> featuredCompanies, IQueryable<SummaryPost> otherPosts, int width, int height, int mediaType)
        {
            var blogAuthorHelper = EngineContext.Current.Resolve<BlogAuthorHelper>();
            var author = blogAuthorHelper.GetByUserName(post.Author);
            var postModel = new PostModels
            {
                Id = post.PostRowID,
                Author = author,
                CoverPhoto = string.IsNullOrEmpty(post.CoverPhoto) ? string.Format("/images/photo_company_image.png") : Utility.KalturaHelper.GetKalturaUrl(post.PostRowID, post.CoverPhoto, mediaType, width, height),
                DateCreated = post.DateCreated.Value,
                Title = post.Title,
                PostContent = post.PostContent,
                Categories = categories,
                Caption = post.Caption,
                Slug = post.Slug,
                TotalComments = comments.Count(),
                OtherPosts = otherPosts.ToList(),
                FeaturedCompanies = featuredCompanies,
                AlowChatToBook = post.AlowChatToBook != null && post.AlowChatToBook.Value,
                Url = string.Format("/Blog/Post/{0}", post.PostRowID),
                IsEvent = post.IsEvent.HasValue && post.IsEvent.Value
            };
            return postModel;
        }

        public CommentModel Map(be_PostComment source)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            var blogAuthorHelper = EngineContext.Current.Resolve<BlogAuthorHelper>();
            BlogUser author;
            UserModel user;
            if (string.IsNullOrEmpty(source.Author))
            {
                author = null;
                var cust = blogAuthorHelper.GetKuyamUser(source.Email);
                user = cust.ToUserModel();
                if (user == null)
                {
                    user = new UserModel();
                    user.FirstName = " ";
                    user.LastName = " ";
                    user.Email = source.Email;
                    user.PhotoUrl = "/Images/placeholder.png";
                }
                user.FirstName = myTI.ToTitleCase(user.FirstName);
                user.LastName = myTI.ToTitleCase(user.LastName);
            }
            else
            {
                user = null;
                author = blogAuthorHelper.GetByUserName(source.Author);
                //Undefined Email. Error occurs when user post spam information in blog engine
                if (string.IsNullOrEmpty(author.FirstName) || string.IsNullOrEmpty(author.LastName))
                {
                    author = new BlogUser();
                    author.FirstName = source.Author;
                    author.LastName = " ";
                    author.EmailAddress = source.Email;
                    author.PhotoUrl = "/Images/placeholder.png";
                }
                author.FirstName = myTI.ToTitleCase(author.FirstName);
                author.LastName = myTI.ToTitleCase(author.LastName);
            }
            var destination = new CommentModel
            {
                Id = source.PostCommentRowID,
                BlogId = source.BlogID,
                Comment = source.Comment,
                PostCommentId = source.PostCommentID,
                PostId = source.PostID,
                Author = author,
                User = user,
                ParentCommentId = source.ParentCommentID,
                CommentDate = source.CommentDate,
                IsDeleted = source.IsDeleted
            };
            return destination;
        }
    }
}