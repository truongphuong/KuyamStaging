using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using System.Data;

namespace Kuyam.Database
{
    public static class kuyamEntitiesExtensions 
    {
        public static IEnumerable<TElement> SqlQuery<TElement>(this DbContext dbcontext, string sql, params object[] parameters)
        {
            return dbcontext.Database.SqlQuery<TElement>(sql, parameters);
        }

        public static int ExecuteSqlCommand( this DbContext dbcontext, string sql, int? timeout = null, params object[] parameters)
        {
            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                //store previous timeout
                previousTimeout = ((IObjectContextAdapter)dbcontext).ObjectContext.CommandTimeout;
                ((IObjectContextAdapter)dbcontext).ObjectContext.CommandTimeout = timeout;
            }

            var result = dbcontext.Database.ExecuteSqlCommand(sql, parameters);

            if (timeout.HasValue)
            {
                //Set previous timeout back
                ((IObjectContextAdapter)dbcontext).ObjectContext.CommandTimeout = previousTimeout;
            }

            //return result
            return result;
        }
    }
}
