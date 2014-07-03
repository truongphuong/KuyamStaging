using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
	public class AppointmentTag
	{
		public int AppointmentID { get; set; }

		public string Name { get; set; }
		public DateTime When { get; set; }
		public int AppointmentStatusID { get; set; }
	}
}