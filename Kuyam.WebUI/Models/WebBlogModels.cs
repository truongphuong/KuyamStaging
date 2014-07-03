using System.Collections.Generic;
using Kuyam.Database;
using System;
using System.Linq;
using Kuyam.Database.BlogModels;
using Kuyam.Database.Extensions;

namespace Kuyam.WebUI.Models
{

    #region Models

    public class WebBlogModels
    {
        public PostExt FeaturedPost { get; set; }
        public List<CompanysBlogsModels> CompanysBlogsModels { get; set; }
    }

    public class CompanysBlogsModels{

        public ProfileCompany FeaturedCompany { get; set; }
        public List<PostExt> Posts { get; set; }
    }

    public class PostModels
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public BlogUser Author { get; set; }
        public DateTime DateCreated { get; set; }
        public string CoverPhoto { get; set; }
        public string PostContent { get; set; }
        public int TotalComments { get; set; }
        public List<be_Categories> Categories { get; set; }
        public string Caption { get; set; }
        public string Slug { get; set; }
        public string  Url { get; set; }
        public List<SummaryPost> OtherPosts { get; set; }
        public List<FeaturedCompanyModels> FeaturedCompanies { get; set; }
        public bool AlowChatToBook { get; set; }

        public bool IsEvent { get; set; }

        public List<be_PostCompanies> CompaniesRelated { get; set; } 
    }

    public class CommentModel
    {
        public int Id { get; set; }
        /// <summary>
        /// GUID id
        /// </summary>
        public Guid PostCommentId { get; set; }
        public Guid BlogId { get; set; }
        public Guid PostId { get; set; }
        public string Comment { get; set; }
        public Guid ParentCommentId { get; set; }
        public DateTime CommentDate { get; set; }
        public UserModel User { get; set; }
        public BlogUser Author { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SummaryPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverPhoto { get; set; }
        public string Url { get; set; }
    }

    public class FeaturedCompanyModels{
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string FeaturedUrl { get; set; }
        public string ImagesUrl { get; set; }
        public int ProfileId { get; set; }
        public double Rate { get; set; }
        public int TotalReview { get; set; }
        public bool IsAvailability { get; set; }
        public string Phone { get; set; }
        public string Zip { get; set; }
        public string Street { get; set; }
        public string Url { get; set; }
    }

    public class ChatToBookModels{
        public bool AlowChatToBook { get; set; }
        public List<ProfileCompany> Companies { get; set; }

    }

    #endregion


}
