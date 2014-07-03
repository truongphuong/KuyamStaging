using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Repository.Interface
{
    public interface IUnitOfWorkFactory : IDisposable
    {
        IUnitOfWork Create();
      
    }
}
