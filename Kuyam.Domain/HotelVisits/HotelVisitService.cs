using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using Kuyam.Utility;

namespace Kuyam.Domain.HotelVisits
{
    public class HotelVisitService:IHotelVisitService
    {
        private readonly IRepository<HotelVisit> _hotelVisitRepository;
        public HotelVisitService(IRepository<HotelVisit> hotelVisitRepository)
        {
            _hotelVisitRepository = hotelVisitRepository;
        }
        public bool CreateGuest(HotelVisit visit)
        {
            try
            {
                _hotelVisitRepository.Insert(visit);
                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error("CreateGuest",exception);
                return false;
            }
            
        }

        public IList<HotelVisit> GetGuestByHotelId(string key, int pageIndex, int pageSize, out int totalRecord, int searchType,int hotelId)
        {
            totalRecord = 0;
            var query = _hotelVisitRepository.Table;
            query= query.Where(a => a.HotelID == hotelId);
            if (searchType == (int) Types.UserStatusType.Unknown)
            {
                query =
                    query.Where(
                        a =>
                            (key == null || key == string.Empty ||
                             (a.Cust != null && (a.Cust.FirstName.ToUpper().Contains(key.ToUpper()) 
                                                 || a.Cust.LastName.ToUpper().Contains(key.ToUpper())
                                                 || a.RoomNumber.Contains(key.ToUpper()))
                                                 && (a.Cust.Status != (int)Types.UserStatusType.Deleted && a.Cust.Status != (int)Types.UserStatusType.Unknown))));
                query = query.Where(a => a.CheckOutDate >= DateTime.Now);
            }
            else
            {
                query =
                  query.Where(
                      a =>
                          (key == null || key == string.Empty ||
                           (a.Cust != null && (a.Cust.FirstName.ToUpper().Contains(key.ToUpper())
                                               ||a.Cust.LastName.ToUpper().Contains(key.ToUpper())
                                               || a.RoomNumber.Contains(key.ToUpper()))
                                               && (a.Cust.Status != (int)Types.UserStatusType.Deleted && a.Cust.Status != (int)Types.UserStatusType.Unknown))));
                query = query.Where(a => a.CheckOutDate < DateTime.Now);

            }
            totalRecord = query.Count();
            query = query.OrderByDescending(m => m.CheckOutDate).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return query.ToList();
        }

        public HotelVisit GetHotelVisit(int id)
        {
            return _hotelVisitRepository.Table.SingleOrDefault(a => a.Id == id);
        }


        public bool UpdateHotelVisit(HotelVisit hotelVisit)
        {
            try
            {
                _hotelVisitRepository.Update(hotelVisit);
                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error("UpdateHotelVisit",exception);
                return false;

            }
        }


        public HotelVisit GetHotelVisitByCusId(int cusId)
        {
            return _hotelVisitRepository.Table.SingleOrDefault(a => a.CustID == cusId);
        }
    }
}
