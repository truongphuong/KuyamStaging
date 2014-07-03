using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Database;

namespace Kuyam.Domain.HotelVisits
{
    public interface IHotelVisitService
    {
        bool CreateGuest(HotelVisit visit);

        IList<HotelVisit> GetGuestByHotelId(string key, int pageIndex, int pageSize, out int totalRecord, int searchType,
            int hotelId);

        HotelVisit GetHotelVisit(int id);

        bool UpdateHotelVisit(HotelVisit hotelVisit);

        HotelVisit GetHotelVisitByCusId(int cusId);

    }
}
