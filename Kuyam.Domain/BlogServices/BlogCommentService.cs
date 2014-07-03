using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using System.Data.SqlClient;
using System.Data;

namespace Kuyam.Domain.BlogServices
{
    public class BlogCommentService : IBlogCommentService
    {
        #region private Fields
        private readonly IRepository<be_PostComment> _commentRepository;

        #endregion
        public BlogCommentService(IRepository<be_PostComment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public IQueryable<Database.be_PostComment> GetCommentsByPostId(Guid postId)
        {
            return _commentRepository.Table.Where(t => t.PostID == postId && t.IsDeleted == false && t.IsSpam == false);
        }

        public IEnumerable<be_PostComment> GetNestedComments(Guid commentId)
        {
            var db = new kuyamEntities(DAL.GetConnectionString());
            var param = new SqlParameter();
            param.ParameterName = "commentId";
            param.Value = commentId.ToString();
            param.DbType = DbType.String;
            return db.SqlQuery<be_PostComment>("exec [GetBlogComments] @commentId", param);
        }

        public be_PostComment InsertComment(be_PostComment postComment)
        {
            _commentRepository.Insert(postComment);
            return postComment;
        }

        public List<be_PostComment> GetNestedComments(List<be_PostComment> parentComments)
        {
            if (parentComments.Count == 0) return new List<be_PostComment>();
            var postId = parentComments[0].PostID;
            var listParentIds = parentComments.Select(t => t.PostCommentID);
            var childrenComments = GetCommentsByPostId(postId).Where(t => listParentIds.Contains(t.ParentCommentID)).OrderBy(t => t.CommentDate).ToList();
            var result = new List<be_PostComment>();
            foreach (var parentComment in parentComments)
            {
                result.Add(parentComment);
                var children = childrenComments.Where(t => t.ParentCommentID == parentComment.PostCommentID).ToList();
                if (children.Count > 0)
                {
                    result.AddRange(children);
                }
            }
            return result;
        }

        public List<be_PostComment> GetParentComments(Guid postId)
        {
            return GetCommentsByPostId(postId).Where(t => t.ParentCommentID == Guid.Empty).ToList();
        }
    }
}
