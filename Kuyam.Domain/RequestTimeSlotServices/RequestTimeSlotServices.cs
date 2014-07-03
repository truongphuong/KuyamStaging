using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain.RequestTimeSlotServices
{
    public class RequestTimeSlotServices:IRequestTimeSlotServices
    {
        #region Private Fields
        private readonly IRepository<GeneralTimeSlot> _generalTimeSlotRepository;
        private readonly DbContext _dbContext;
        #endregion

        public RequestTimeSlotServices(IRepository<GeneralTimeSlot> generalTimeSlotRepository,
            DbContext dbContext)
        {
            _generalTimeSlotRepository = generalTimeSlotRepository;
            _dbContext = dbContext;
        }
        public IList<GeneralTimeSlot> GetAllTimeSlot(int? companyId)
        {
            var query = from slot in _generalTimeSlotRepository.Table
                select slot;
            if (companyId.HasValue&& companyId!=0)
                query = query.Where(a => a.ProfileId == companyId.Value);
            return query.OrderBy(a => a.DayOfWeek).ThenBy(a=>a.FromHour).ToList();
        }

        public bool SaveTimeSlot(IList<GeneralTimeSlot> timeSlots)
        {
            try
            {
                foreach (var generalTimeSlot in timeSlots)
                {
                    _generalTimeSlotRepository.Insert(generalTimeSlot);
                }
                
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }


        public bool EditTimeSlot(int profileId, IList<GeneralTimeSlot> timeSlots)
        {
            try
            {
                var sql = "DELETE FROM GeneralTimeSlot WHERE ProfileId = " + profileId;
                _dbContext.ExecuteSqlCommand(sql); 
                foreach (var generalTimeSlot in timeSlots)
                {
                    _generalTimeSlotRepository.Insert(generalTimeSlot);
                }

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }


        public bool CheckProfileExistedTimeSlot(int id)
        {
            var query = from slot in _generalTimeSlotRepository.Table
                        where slot.ProfileId==id
                        select slot;
            if (query.Any())
                return false;
            return true;
        }


        public bool DeleteTimeSlot(int id)
        {
            try
            {
                var sql = "DELETE FROM GeneralTimeSlot WHERE Id = " + id;
                _dbContext.ExecuteSqlCommand(sql);
                
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }


        public GeneralTimeSlot GetTimeSlotByCompanyAndId(int companyId, int Id)
        {
            var query = from slot in _generalTimeSlotRepository.Table
                        where slot.ProfileId == companyId && slot.Id==Id
                        select slot;
            if (query.Any())
            {
                return query.SingleOrDefault();
            }
            return null;
        }


        public bool UpdateTimeslot(int Id, TimeSpan fromHour, TimeSpan toHour)
        {
            try
            {
                var generalTimeSlot = _generalTimeSlotRepository.Table.SingleOrDefault(a => a.Id == Id);
                generalTimeSlot.FromHour = fromHour;
                generalTimeSlot.ToHour = toHour;
                _generalTimeSlotRepository.Update(generalTimeSlot);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }


        public IList<GeneralTimeSlot> GetAllTimeSlot(int? companyId, int index, int pageSize, out int totalRecord)
        {
            var query = from slot in _generalTimeSlotRepository.Table
                        select slot;
            if (companyId.HasValue && companyId != 0)
                query = query.Where(a => a.ProfileId == companyId.Value);
            totalRecord = query.Count();
            return query.OrderBy(a => a.DayOfWeek).ThenBy(a => a.FromHour).Skip((index - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
