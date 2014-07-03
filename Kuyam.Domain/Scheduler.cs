using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using M2.Util;

namespace Kuyam.Domain
{
	public class Scheduler
	{
		TimeLine _timeline = null;

		public TimeLine TimeLine { get { return _timeline; } }

		public TimeLine.Intervals GetAvailableSlots(DateTime targetDate, int daysOut, int count, int ProfileID, DateTime minDate)
		{
			// Get open hours, appointments and exceptions for Company; Map them to determine open periods
			
			// build byte array of every 5 minutes starting at min date
			// check if start <= end && end >= start
			//     Appt is 10-15, start=8, end=12: in; end=9: out; end=16: in
			//     start = 11, end=12: in; end=16: in;
			//     start = 16: out
			// 0 = closed
			// 1 = avail
			// >1 = value-1 = # of appt's booked for 5 minute intervals

			if (count == 0)
				count = 1;

			Profile pro = Profile.Load(ProfileID);
			if (pro == null)
				return null;

			ProfileCompany proc = pro.ProfileCompany;

			// duration = (count/defaultpplperappt)*defaultduration
			int duration = (int)Math.Ceiling((decimal)(count / proc.ApptDefaultPeoplePerSlot)) * proc.ApptDefaultSlotDuration;

			LoadHours(targetDate, daysOut, pro, minDate);

			// Get open slots
			TimeLine.Intervals slots = _timeline.FindIntervals(duration, proc.ApptDefaultSlotDuration);
			return slots;
		}

		public void LoadHours(DateTime targetDate, int daysOut, Profile pro, DateTime minDate)
		{
			// Setup base reference
			_timeline = new TimeLine(targetDate, daysOut, 15, minDate);

			// Set open hours
			List<ProfileHour> openHours = pro.ProfileHours.ToList();
			foreach (DateTime d in _timeline.GetDates())
			{
				foreach (ProfileHour h in openHours.Where(h => h.Day == 121 + (int)d.DayOfWeek))
				{
					_timeline.SetIntervalValue((int)TimeLine.BusinessHours.Open,
						new DateTime(d.Year, d.Month, d.Day, h.Start.Hours, h.Start.Minutes, 0),
						new DateTime(d.Year, d.Month, d.Day, h.End.Hours, h.End.Minutes, 0));
				}
			}

			// Book hours until now (today only)
			DateTime now = DateTime.Now;
			_timeline.SetIntervalValue((int)TimeLine.BusinessHours.Closed, DateTime.Now.Date, DateTime.Now);

			// Set current appointment bookings
			Cust c = Cust.Load(pro.CustID);
            List<Appointment> appts = c.Appointments2().ToList();
            foreach (Appointment a in appts)
            {
                _timeline.SetIntervalValue((int)TimeLine.BusinessHours.Booked,
                    a.Start,
                    a.End);
            }

			// Set exception hours
			List<ProfileHoursException> exs = pro.ProfileHoursExceptions.ToList();
			foreach (ProfileHoursException e in exs)
			{
				_timeline.SetIntervalValue((byte)(e.IsOpen ? (int)TimeLine.BusinessHours.Open : (int)TimeLine.BusinessHours.Closed),
					e.Start,
					e.End);
			}
		}

	}
}
