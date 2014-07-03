using Kuyam.Database;
using Kuyam.Repository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Seo
{
    public static class SeoExtensions
    {
        #region General

        public static string GetSeName<T>(this T entity, int entityId, string entityName="company")
            where T : class
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            string result = string.Empty;

            var friendlyUrlService = EngineContext.Current.Resolve<ISeoFriendlyUrlService>();

            var friendly = friendlyUrlService.GetSeoFriendlyUrlById(entityId, entityName);

            if (friendly != null)
                result = friendly.Slug;
            return result;
        }


        #endregion
    }
}
