using Kuyam.Database;
using Kuyam.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.DemoServices
{
    public interface ITestsService
    {
        IList<PostMapModel> GetAll();

    }
}
