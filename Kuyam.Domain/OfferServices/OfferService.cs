using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.OfferServices
{
    public class OfferService : IOfferService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CompanyEvent> _companyEventRepository;
        private readonly IRepository<CompanyServiceEvent> _companyServiceEventRepository;
        public OfferService(DbContext dbContext,
            IRepository<Event> eventRepository,
            IRepository<CompanyEvent> companyEventRepository,
            IRepository<CompanyServiceEvent> companyServiceEventRepository)
        {
            this._dbContext = dbContext;
            this._eventRepository = eventRepository;
            this._companyEventRepository = companyEventRepository;
            this._companyServiceEventRepository = companyServiceEventRepository;

        }


        public List<String> GetCitiesByCompanyEventId(int companyEventId = 0, int categoryID=0)
        {
            return _dbContext.SqlQuery<string>("GetCitiesByEventID @EventID, @CategoryID",
               new SqlParameter("EventID", companyEventId), new SqlParameter("CategoryID", categoryID)).ToList();
        }
        public CompanyEvent GetCompanyEventByCompanyEventId(int companyEventId)
        {
             return _companyEventRepository.Table.Where(x => x.CompanyEventID == companyEventId).FirstOrDefault();
        }

        public List<CompanyServiceEventExt> GetListServicesEventByCompanyEventId(int companyEventId, int serviceTypeId)
        {
            return _dbContext.SqlQuery<CompanyServiceEventExt>("GetOfferServiceCompaniesByEventID @CompanyEventId, @ServiceTypeId",
                new SqlParameter("CompanyEventId", companyEventId), new SqlParameter("ServiceTypeId", serviceTypeId)).ToList();
        }
        public List<CompanyServiceEventExt> GetListClassesToEventByCompanyEventId(int companyEventId)
        {
            return GetListServicesEventByCompanyEventId(companyEventId, (int)Types.ServiceType.ClassType).OrderBy(o=>o.NewPrice).Take(3).ToList();
        }
        public List<CompanyServiceEventExt> GetListCompanyServicesToEventByCompanyEventId(int companyEventId)
        {
            return GetListServicesEventByCompanyEventId(companyEventId, (int)Types.ServiceType.ServiceType).OrderBy(o=>o.NewPrice).Take(3).ToList();
        }
    }
}
