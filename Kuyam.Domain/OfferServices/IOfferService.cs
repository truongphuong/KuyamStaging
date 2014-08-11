using Kuyam.Database;
using Kuyam.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.OfferServices
{
    public interface IOfferService
    {
        CompanyEvent GetCompanyEventByCompanyEventId(int companyEventId);
        List<String> GetCitiesByCompanyEventId(int companyEventId = 0, int categoryID = 0);
        List<CompanyServiceEventExt> GetListServicesEventByCompanyEventId(int companyEventId, int serviceTypeId);
        List<CompanyServiceEventExt> GetListClassesToEventByCompanyEventId(int companyEventId);
        List<CompanyServiceEventExt> GetListCompanyServicesToEventByCompanyEventId(int companyEventId);
    }
}
