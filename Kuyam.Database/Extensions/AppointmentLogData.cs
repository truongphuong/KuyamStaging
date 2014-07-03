using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public partial class AppointmentLog
	{
		public static AppointmentLog Load(int apptLogID)
		{
			return DAL.LoadAppointmentLog(apptLogID);
		}

		public void Update()
		{
			DAL.UpdateRec(this, this.AppointmentLogID);
		}
	}
}
