using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;

using M2.Util;
using M2.Util.MVC;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Kuyam.Domain;
using Kuyam.WebUI.Controllers;

namespace Kuyam.WebUI.Models
{
	public class CalendarCreateModel : IModel
	{
		public Calendar Calendar { get; set; }

		public string WhoFor { get; set; }

		public SelectList ProfileList { get; set; }
		public int? NewProfileID { get; set; }
		public string NewPersonName { get; set; }
		public SelectList RelationshipList { get; set; }
		public int? RelationshipID { get; set; }

		public int? NewCompanyProfileID { get; set; }
		public string NewCompanyName { get; set; }
		public string NewCompanyPhone { get; set; }

		public List<SelectListItem> CalendarColors { get; set; }

		public void LockAndLoad()
		{
			if (Calendar == null)
				Calendar = new Calendar();

			WhoFor = "you";

			//ProfileList = new SelectList(MySession.Cust.Profiles, "profileid", "name");
			RelationshipList = Types.GetTypeList(Types.TypeGroup.RelationshipType).ToSelectList();
			CalendarColors = ControllerUtil.GetCalendarColors(Calendar.BackColor);
		}
	}

	public class CalendarEditModel : IModel
	{
		public int CalendarID { get; set; }
		public Calendar Calendar { get; set; }

		public List<SelectListItem> CalendarColors { get; set; }

		public CalendarEditModel()
		{
			Calendar = new Calendar();
		}

		public CalendarEditModel(int calID)
		{
			CalendarID = calID;
		}

		public void LockAndLoad()
		{
			Calendar = Calendar.Load(CalendarID);
			if (Calendar == null)
				throw new ApplicationException("Calendar ID " + CalendarID + " not found.");

			CalendarColors = ControllerUtil.GetCalendarColors(Calendar.BackColor);
		}
	}

	public class CalendarManageModel : IModel
	{
		public List<Calendar> Calendars { get; set; }

		public void LockAndLoad()
		{
			Calendars = MySession.Cust.GetCalendars();
		}
	}

    public class CalendarObject {
        public string id { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public double duration { get; set; }
        public string  currentAppointment { get; set; }        
        public string title { get; set; }
        public string url { get; set; }
        public string className { get; set; }
        public bool allDay { get; set; }
        public int eventType { get; set; }
        public int Status { get; set; }
    }
}