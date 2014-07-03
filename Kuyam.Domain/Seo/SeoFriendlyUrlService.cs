using Kuyam.Database;
using Kuyam.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Seo
{
    public class SeoFriendlyUrlService : ISeoFriendlyUrlService
    {

        #region Fields

        private readonly IRepository<SeoFriendlyUrl> _friendlyUrlRepository;
        private readonly DbContext _dbContext;
        #endregion

        #region Ctor

        public SeoFriendlyUrlService(IRepository<SeoFriendlyUrl> friendlyUrlRepository, DbContext dbContext)
        {
            this._friendlyUrlRepository = friendlyUrlRepository;
            this._dbContext = dbContext;
        }

        #endregion

        public SeoFriendlyUrl GetSeoFriendlyUrlById(int friendlyUrlId, string entityName)
        {
            return _friendlyUrlRepository.Table.Where(m => m.EntityId == friendlyUrlId && m.EntityName == entityName).FirstOrDefault();
        }

        public SeoFriendlyUrl GetBySlug(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return null;

            var query = from ur in _friendlyUrlRepository.Table
                        where ur.Slug == slug
                        select ur;
            var friendlyUrl = query.FirstOrDefault();
            return friendlyUrl;
        }

        public string GetActiveSlug(int entityId, string entityName)
        {
            var query = from ur in _friendlyUrlRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName &&
                        ur.IsActive
                        orderby ur.Id descending
                        select ur.Slug;
            var slug = query.FirstOrDefault();
            if (slug == null)
                slug = "";
            return slug;
        }

        public void InsertFriendlyUrl(SeoFriendlyUrl friendlyUrl)
        {
            if (friendlyUrl == null)
                throw new ArgumentNullException("friendlyUrl");

            _friendlyUrlRepository.Insert(friendlyUrl);
        }

        public void UpdateFriendlyUrl(SeoFriendlyUrl friendlyUrl)
        {
            if (friendlyUrl == null)
                throw new ArgumentNullException("friendlyUrl");

            _friendlyUrlRepository.Update(friendlyUrl);
        }
       

        public void CreateFriendlyUrl(SeoFriendlyUrl friendlyUrl)
        {
            if (friendlyUrl == null)
                throw new ArgumentNullException("friendlyUrl");

            var param1 = new SqlParameter();
            param1.ParameterName = "entityId";
            param1.Value = friendlyUrl.EntityId;
            param1.DbType = DbType.Int32;
            var param2 = new SqlParameter();
            param2.ParameterName = "slug";
            param2.Value = friendlyUrl.Slug;
            param2.DbType = DbType.String;
            var param3 = new SqlParameter();
            param3.ParameterName = "entityName";
            param3.Value = friendlyUrl.EntityName;
            param3.DbType = DbType.String;
            _dbContext.ExecuteSqlCommand("exec [InsertFriendlyUrlForAll]  @entityId, @slug, @entityName", null, param1, param2, param3);
        }

        public void SaveSlug(int companyId, string slug, string entityName = "")
        {
            if (companyId == 0 || slug == null)
                throw new ArgumentNullException("entity");
            var friendlyUrl = new SeoFriendlyUrl()
            {
                EntityId = companyId,
                EntityName = entityName,
                Slug = slug,
                IsActive = true
            };
            CreateFriendlyUrl(friendlyUrl);

        }

    }
}
