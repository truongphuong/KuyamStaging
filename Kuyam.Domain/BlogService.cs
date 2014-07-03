using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain
{
    public class BlogService
    {
          #region Fields

        private readonly IRepository<be_Blogs> _blogRepository;
        private readonly IRepository<be_Posts> _postRepository;
        private readonly IRepository<be_PostComment> _postCommentRepository;
        private readonly IRepository<be_Profiles> _profilesRepository;
        #endregion

        #region Ctor

        public BlogService(IRepository<be_Blogs> blogRepository,
            IRepository<be_Posts> postRepository,
            IRepository<be_PostComment> postCommentRepository,
            IRepository<be_Profiles> profilesRepository)
        {
            this._blogRepository = blogRepository;
            this._postRepository = postRepository;
            this._postCommentRepository = postCommentRepository;
            this._profilesRepository = profilesRepository;
        }

        #endregion

        #region method

        public List<be_Posts> GetListPost()
        {
            var query = _postRepository.Table.Where(m=>m.IsDeleted == false && m.IsPublished.HasValue && m.IsPublished.Value);
            return query.OrderByDescending(m => m.DateCreated).Take(20).ToList();
        } 

        #endregion
    }
}
