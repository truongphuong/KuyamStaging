using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database.BlogModels;

namespace Kuyam.Domain.BlogServices
{
    public interface IBlogUserService
    {
        BlogUser GetById(string userName);
        BlogUser GetByEmail(string email);
        Dictionary<string, dynamic> GetProfile(string userName);

    }
}
