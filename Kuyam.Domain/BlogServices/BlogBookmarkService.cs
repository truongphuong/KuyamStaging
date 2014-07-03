using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.BlogServices
{
    public class BlogBookmarkService: IBookmarkService
    {
        #region Fields
        private readonly IRepository<be_Posts> _postRepository;
        private readonly IRepository<Bookmark> _bookmarkRepository;
        #endregion

        public BlogBookmarkService(IRepository<be_Posts> postRepository, IRepository<Bookmark> bookmarkRepository)
        {            
            this._postRepository = postRepository;
            this._bookmarkRepository = bookmarkRepository;            
        }

        public Bookmark GetByPostId(int postId, int cusId)
        {
            var post = _postRepository.Table.Where(t => t.PostRowID == postId).FirstOrDefault();
            if (post == null) return null;
            var bookmark = _bookmarkRepository.Table.Where(t => t.PostId == post.PostRowID && t.CustId == cusId).FirstOrDefault();
            return bookmark;
        }


        public void Insert(Bookmark bookmark)
        {
            _bookmarkRepository.Insert(bookmark);
        }

        public void Delete(Bookmark bookmark)
        {
            _bookmarkRepository.Delete(bookmark);
        }

        /// <summary>
        /// Only get bookmark which post has deleted status = false and isPublish = true
        /// </summary>
        /// <param name="custId"></param>
        /// <returns></returns>
        public IQueryable<Bookmark> GetAll(int custId)
        {
            var bookmarks = _bookmarkRepository.Table.Where(t => t.CustId == custId && t.be_Posts.IsDeleted == false && t.be_Posts.IsPublished.Value);
            return bookmarks;
        }
    }
}
