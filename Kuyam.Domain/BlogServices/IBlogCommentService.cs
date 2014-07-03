using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.BlogServices
{
    public interface IBlogCommentService
    {
        IQueryable<be_PostComment> GetCommentsByPostId(Guid postId);
        List<be_PostComment> GetParentComments(Guid postId);
        List<be_PostComment> GetNestedComments(List<be_PostComment> parentComments);
        IEnumerable<be_PostComment> GetNestedComments(Guid commentId);
        be_PostComment InsertComment(be_PostComment postComment);
    }
}
