using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Kuyam.Repository.Interface;

namespace Kuyam.Repository.Interface
{
    public interface IUnitOfWork 
    {
        bool IsInTransaction { get; }

        void SaveChanges();

        void BeginTransaction();

        void BeginTransaction(IsolationLevel isolationLevel);

        void RollBackTransaction();

        void CommitTransaction();
    }
}
