using Kuyam.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database.BlogModels
{
    public class BlogSpecification : Specification<PostMapModel>
    {
        public override IQueryable<PostMapModel> SatisfyingElementsFrom(IQueryable<PostMapModel> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return from post in candidates                  
                   select post;
        }
    }


    public class BlogMapSpecification : Specification<be_Posts>
    {
        public override IQueryable<be_Posts> SatisfyingElementsFrom(IQueryable<be_Posts> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return from post in candidates
                   select post;
        }
    }
}
