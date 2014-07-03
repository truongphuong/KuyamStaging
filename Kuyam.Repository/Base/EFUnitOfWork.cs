using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;
using System.Data;
using Kuyam.Repository.Interface;

namespace Kuyam.Repository.Base
{
    public class EFUnitOfWork : IUnitOfWork
    {
        public readonly DbContext _dbContext;
        private DbTransaction _transaction;

        public EFUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsInTransaction
        {
            get { return _transaction != null; }
        }

        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
            {
                throw new ApplicationException("Cannot begin a new transaction while an existing transaction is still running. " +
                                                "Please commit or rollback the existing transaction before starting a new one.");
            }
            OpenConnection();
            _transaction = _dbContext.Database.Connection.BeginTransaction(isolationLevel);
        }

        public void RollBackTransaction()
        {
            if (_transaction == null)
            {
                throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
            }

            try
            {
                _transaction.Rollback();
            }
            catch
            {
                throw;
            }
            finally
            {
                ReleaseCurrentTransaction();
            }
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
            }

            try
            {
                _dbContext.SaveChanges();
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                ReleaseCurrentTransaction();
            }
        }

        public void SaveChanges()
        {
            if (IsInTransaction)
            {
                throw new ApplicationException("A transaction is running. Call BeginTransaction instead.");
            }
            _dbContext.SaveChanges();
        }

        private void ReleaseCurrentTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        private void OpenConnection()
        {
            if (_dbContext.Database.Connection.State != ConnectionState.Open)
            {
                _dbContext.Database.Connection.Open();
            }
        }

        #region Implementation of IDisposable

        //public void Dispose()
        //{
        //    Dispose(true);
           
        //}


        //private bool _disposed;
        //private void Dispose(bool disposing)
        //{
        //    if (!disposing)
        //        return;
        //    if (_disposed)
        //        return;
        //    ReleaseCurrentTransaction();
        //    _disposed = true;
        //}

        #endregion
        
    }
}
