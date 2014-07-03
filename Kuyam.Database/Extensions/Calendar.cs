using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public partial class Calendar
	{
		public static Calendar Create(Types.CustType ptype, int profileID, string name = null, bool isDefault = false)
		{
			Calendar cal = new Calendar();
			cal.ProfileID = profileID;
			cal.CalendarDisplayTypeID = (int)Types.CalendarDisplayType.Selected;
			if (ptype == Types.CustType.Personal)
			{
				cal.Name = "Personal";
			}
			else
			{
				cal.Name = "Company";
			}
			if (name != null)
				cal.Name = name;

			cal.IsDefault = isDefault;
            cal.Created = DateTime.Now;
			DAL.CreateCalendar(cal);

			return cal;
		}

		public static Calendar Load(int calID)
		{
			return DAL.GetCalendar(calID);
		}

		public static Calendar Create(Calendar cal)
		{
			cal.CalendarDisplayTypeID = (int)Types.CalendarDisplayType.Selected;
			DAL.CreateCalendar(cal);
			return cal;
		}

		public void Update()
		{
			DAL.UpdateRec(this, CalendarID);
		}

		public void SetCalendarDisplayType(Kuyam.Database.Types.CalendarDisplayType typeID)
		{
			DAL.SetCalendarDisplayType(CalendarID, typeID);
		}
	}
}
