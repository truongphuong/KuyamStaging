using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Entity;
using Kuyam.Repository.Interface;
using System.Data.Entity.Validation;
using Kuyam.Database;

namespace Kuyam.Repository.Base
{
    public class EFRepository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _context;
        private IDbSet<T> _dbSet;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">db context</param>
        public EFRepository(DbContext context)
        {
            this._context = context;
        }


        private IDbSet<T> Entities
        {
            get
            {
                if (_dbSet == null)
                {
                    _dbSet = _context.Set<T>();
                }

                return _dbSet;
            }
        }

        public T GetById(object id)
        {
            return this.Entities.Find(id);
        }

        public void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                this.Entities.Add(entity);

                this._context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                HandleException(dbEx);
            }
        }

        public void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                HandleException(dbEx);
            }
        }

        public void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                this.Entities.Remove(entity);

                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                HandleException(dbEx);
            }
        }

        public virtual IQueryable<T> Table
        {
            get
            {
                return this.Entities;
            }
        }

        public IEnumerable<T> FindBySpecification(params Specification<T>[] specifications)
        {
            if (specifications == null || specifications.Any(s => s == null))
                throw new ArgumentNullException("specifications");

            return specifications.Aggregate(Table, (current, specification) => specification.SatisfyingElementsFrom(current));
        }

        public void HandleException(DbEntityValidationException dbEx)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    sb.AppendFormat("Property \"{0}\": {1}\r\n", validationError.PropertyName, validationError.ErrorMessage);
                }
            }
            throw new Exception("[kuyamEntitiesExt.Save] ERROR! " + sb.ToString(), dbEx);
            //throw new ApplicationException("[kuyamEntitiesExt.Save] ERROR! " + sb.ToString(), dbEx);
        }
    }
}
