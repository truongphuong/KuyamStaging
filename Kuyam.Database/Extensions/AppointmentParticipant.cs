using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public partial class AppointmentParticipant
	{
		public static AppointmentParticipant Load(int apptPartID)
		{
			return DAL.GetAppointmentParticipant(apptPartID);
		}

		public static AppointmentParticipant Load(int apptid, int calid)
		{
			return DAL.GetAppointmentParticipant(apptid, calid);
		}

		public void Save()
		{
			Update();
		}

		public void Update()
		{
			DAL.UpdateRec(this, AppointmentParticipantID);
		}

		public static AppointmentParticipant FindByProfile(int apptID, int profileID)
		{
			return DAL.GetAppointmentParticipantByProfile(apptID, profileID);
		}
	}
}
