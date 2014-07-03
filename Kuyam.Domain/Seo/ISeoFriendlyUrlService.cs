using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Seo
{
    public interface ISeoFriendlyUrlService
    {
        SeoFriendlyUrl GetSeoFriendlyUrlById(int friendlyUrlId, string entityName);
        SeoFriendlyUrl GetBySlug(string slug);
        string GetActiveSlug(int entityId, string entityName);
        void InsertFriendlyUrl(SeoFriendlyUrl urlRecord);
        void UpdateFriendlyUrl(SeoFriendlyUrl urlRecord);
        void SaveSlug(int entityId, string entityName, string slug);               
    }
}
