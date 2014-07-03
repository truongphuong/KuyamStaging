using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public class AppointmentLogData
	{
		public AppointmentLog Log { get; set; }
		public Appointment Appointment { get; set; }
		public string CompanyName { get; set; }
		public string ProfileName { get; set; }
	}
}
