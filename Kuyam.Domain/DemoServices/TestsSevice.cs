using Kuyam.Database;
using Kuyam.Database.BlogModels;
using Kuyam.Database.Extensions;
using Kuyam.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.DemoServices
{
    public class TestsSevice : ITestsService
    {

        readonly IRepository<PostMapModel> _postRepository;

        public TestsSevice(IRepository<PostMapModel> postRepository)
        {
            if (postRepository == null)
                throw new ArgumentNullException("postRepository");

            _postRepository = postRepository;
        }
        public IList<PostMapModel> GetAll()
        {
            var posts = _postRepository.FindBySpecification(new BlogSpecification());
            return posts.ToList();
        }
    }
}
