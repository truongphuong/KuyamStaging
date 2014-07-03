using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public class Appointment_Company
	{
		public Appointment Appointment { get; set; }
		public ProfileCompany ProfileCompany { get; set; }
		public Cust Cust { get; set; }

		public Appointment_Company()
		{
		}

		public Appointment_Company(Appointment a, ProfileCompany pc, Cust c)
		{
			Appointment = a;
			ProfileCompany = pc;
			Cust = c;
		}

		//public void Setup()
		//{
		//    List<Cust> custs = DAL.GetAppointmentCusts(Appointment, Types.AppointmentParticipantType.Owner);
		//    Cust = custs.FirstOrDefault();
		//    if (Cust != null)
		//        Cust.Decrypt();
		//}
	}

	public class Cust_Company
	{
		public ProfileCompany ProfileCompany { get; set; }
		public Cust Cust { get; set; }
	}

}
