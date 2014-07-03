using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public interface IBookmarkService
    {
        Bookmark GetByPostId(int postId, int cusId);
        IQueryable<Bookmark> GetAll(int custId);
        void Insert(Bookmark bookmark);
        void Delete(Bookmark bookmark);
    }
}
