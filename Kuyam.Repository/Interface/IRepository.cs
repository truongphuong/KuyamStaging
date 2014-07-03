using System.Collections.Generic;
using System.Linq;
using Kuyam.Database;

namespace Kuyam.Repository.Interface
{
    public interface IRepository<T> where T : class
    {
        T GetById(object id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> Table { get; }
        IEnumerable<T> FindBySpecification(params Specification<T>[] specifications);
    }
}
