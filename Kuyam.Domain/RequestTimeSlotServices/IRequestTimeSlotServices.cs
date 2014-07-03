using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Database;

namespace Kuyam.Domain.RequestTimeSlotServices
{
    public interface IRequestTimeSlotServices
    {
        IList<GeneralTimeSlot> GetAllTimeSlot(int? companyId);
        IList<GeneralTimeSlot> GetAllTimeSlot(int? companyId,int index, int pageSize, out int TotalRecord);
        bool SaveTimeSlot(IList<GeneralTimeSlot> timeSlots);

        bool EditTimeSlot(int profileId, IList<GeneralTimeSlot> timeSlots);

        bool CheckProfileExistedTimeSlot(int id);

        bool DeleteTimeSlot(int id);

        GeneralTimeSlot GetTimeSlotByCompanyAndId(int companyId,int Id);

        bool UpdateTimeslot(int Id, TimeSpan fromHour, TimeSpan toHour);

    }
}
