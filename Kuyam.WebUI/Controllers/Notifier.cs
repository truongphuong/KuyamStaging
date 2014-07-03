using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;
using M2.Util;
using Kuyam.WebUI.Models;
using Kuyam.Domain;

namespace Kuyam.WebUI.Controllers
{
	internal class MessageData
	{
		public Dictionary<Cust, string> Custs { get; set; }
		public Appointment Appointment { get; set; }
		public string Action { get; set; }
		public string BodyAppend { get; set; }
		public string Subject { get; private set; }
		public string Body { get; private set; }

		public MessageData(Appointment a, string action, string bodyAppend)
		{
			Appointment = a;
			Action = action;
			BodyAppend = bodyAppend;
			Custs = Appointment.GetParticipants();
		}

		public void CreateMessage(int fromCustID)
		{
			List<string> toCusts = (from c in Custs
									where c.Key.CustID != fromCustID
									select c.Value).ToList();

			string custNames = toCusts.JoinAsString(", ");

			Subject = String.Format("{0}Appointment {1} with {2} on {3}",
				MyApp.Platform.DB != "prod" ? "[TEST - PLEASE IGNORE] " : "",
				Action,
				custNames,
				Appointment.Start.ToShortDateString()
				);

			Body = String.Format("{0}Appointment with {1} on {2} at {3} was {4} at {5}.  {6}",
				MyApp.Platform.DB != "prod" ? "[TEST - PLEASE IGNORE] " : "",
				custNames,
				Appointment.Start.ToShortDateString(),
				Appointment.Start.ToShortTimeString(),
				Action,
				DateTime.Now.ToString("M/d/yyyy h:mm tt"),
				BodyAppend
				);
		}
	}

	// TODO: Needs to move to domain
	public static class Notifier
	{
		//----------- By the owner -------------------
		public static void ApptSubmitted(Appointment a)
		{
			NotifyUsers(a, "requested");
		}

		public static void ApptDeclined(Appointment a)
		{
			NotifyUsers(a, "declined");
			//NotifyCompany(data, a);
		}

		public static void ApptConfirmed(Appointment a)
		{
			NotifyUsers(a, "confirmed");
			//NotifyCompany(data, a);
		}

		//------------- By the participants ---------------

		public static void ApptAccepted(Appointment a)
		{
			NotifyUsers(a, "accepted", false, "  Please log in and confirm the appointment.");
		}

		public static void ApptDeclined(Appointment a, string notes)
		{
			NotifyUsers(a, "declined", false, "  Additional information from company: " + notes);
		}

		public static void ApptAltProposed(Appointment a, string notes)
		{
			NotifyUsers(a, "declined", false, "  Alternatively, the company proposes the following: " + notes);
		}

		//------------- System ----------------------------------

		public static void NewCustomerSignedUp()
		{
			Cust c = MySession.Cust;
			SendEmail(MyApp.Settings.Email.KuyamApptNotificationEmail, null, "new customer " + MySession.Username,
				String.Format("{2}A new customer signed-up!<br>{0}<br>{1}", MySession.Username, c.FirstName + "  " + c.LastName,
				MyApp.Platform.DB != "prod" ? "[TEST - PLEASE IGNORE] " : ""));
		}

		//---------------------------------------------------------------------------

		public static void NotifyUsers(Appointment a, string action, bool fromUser = true, string bodyAppend = "")
		{
			MessageData data = new MessageData(a, action, bodyAppend);

			if (fromUser)
			{
				data.CreateMessage(MySession.CustID);
				MySession.Messages.Add(data.Body.ToLower());
			}

			// For each cust, if active (check if company), send an email
			foreach (Cust c in a.GetCusts())
			{
				if (c.CustStatusTypeID == (int)Types.CustStatus.Active)
				{
					data.CreateMessage(c.CustID);
					SendEmail(c.aspnet_Users.UserName, data.Subject, data.Body);
				}
			}

		}

		//public static void NotifyCompany(string[] data, Appointment a)
		//{
		//    ProfileCompany pc = ProfileManager.Get.GetCompany(a.CalendarID2);
		//    if (v.CompanyStatusID == (int)Types.CompanyStatus.Active)
		//        SendEmail(v.Email, data[0], data[1]);
		//    else
		//    {
		//        data[1] += "  From " + MySession.Username + ".";
		//        SendEmail(MyApp.Settings.Email.KuyamApptNotificationEmail, data[0], data[1]);
		//    }
		//}

		//---------------------------------------------------------------------------

		public static void SendEmail(string to, string subject, string body)
		{
			SendEmail(to, null, subject, body);
		}

		public static void SendEmail(string to, string from, string subject, string body)
		{
			body += "<br><br><a href=\"http://www.kuyam.com\">Click here</a> to login to your account at Kuyam.";
			SMTPClient.SendMessage((from + "").ToLower(), (to + "").ToLower(), (subject + "").ToLower(), body);
		}

	}
}