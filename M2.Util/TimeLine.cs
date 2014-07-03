using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using M2.Util;

namespace Kuyam.Domain
{
	// Unit = # of minutes, lowest resolution
	// Block = a group of units
	public class TimeLine
	{
		public enum BusinessHours : int
		{
			Closed=0,
			Open=1,
			Booked=2
		}

		public class Interval
		{
			public DateTime Start { get; set; }
			public DateTime End { get; set; }

			public Interval()
			{
			}

			public Interval(DateTime start, DateTime end)
			{
				Start = start;
				End = end;
			}
		}

		public class Intervals : List<Interval>
		{
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				foreach (TimeLine.Interval i in this)
					sb.AppendLine((String.Format("{0}: {1} - {2}", i.Start.ToShortDateString(), i.Start.ToShortTimeString(), i.End.ToShortTimeString())));

				return sb.ToString();
			}
		}

		private byte[] _units = null;
		private DateTime _startDate = DateTime.Now;
		private DateTime _endDate = DateTime.Now;
		private int _blockSize = 5;
		private int _daysOut = 0;
		private int _len = 0;

		public TimeLine(DateTime baseDate, int daysOut, int blockSize, DateTime minDate)
		{
			_blockSize = blockSize;
			_daysOut = daysOut;
			_startDate = (new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 0, 0, 0)).AddDays(-1 * daysOut);
			if (_startDate < minDate)
				_startDate = minDate;
			_endDate = baseDate.AddDays(daysOut+1);
			_len = (int)(_endDate.Subtract(_startDate).TotalDays) * ((24 * 60) / _blockSize);
			_units = new byte[_len];

			_units.Reset();
		}

		//public List<int> GetDays()
		//{
		//    List<int> ret = new List<int>();
		//    for (DateTime curr = _startDate; curr < _endDate; curr.AddDays(1))
		//        ret.Add((int)curr.DayOfWeek);
		//    ret = ret.Distinct().ToList();
		//    ret.Sort();
		//    return ret;
		//}

		public List<DateTime> GetDates()
		{
			List<DateTime> ret = new List<DateTime>();
			for (DateTime curr = _startDate; curr <= _endDate; curr = 
				curr.AddDays(1))
				ret.Add(curr);
			return ret;
		}

		public override string ToString()
		{
			int hourUnits = 60/_blockSize;
			StringBuilder sb = new StringBuilder();
			for (int h = 0; h < _units.Count(); h += hourUnits)
			{
				sb.AppendFormat("{0} ({1:0000}): ", _startDate.AddHours(h/hourUnits).ToString("MM/dd/yy hh:mm:ss"), h);
				for (int u = h; u < h + hourUnits; u++)
				{
					sb.Append(_units[u].ToString());
				}
				sb.AppendLine();
			}

			return sb.ToString();
		}

		public void SetIntervalValue(byte value, Interval interval, bool addValue = false)
		{
			SetIntervalValue(value, interval.Start, interval.End, addValue);
		}

		public void SetIntervalValue(byte value, DateTime start, DateTime end, bool addValue = false)
		{
			int minutes = (int)end.Subtract(start).TotalMinutes;
			SetIntervalValue(value, start, minutes, addValue);
		}

		public void SetIntervalValue(byte value, DateTime start, int duration, bool addValue = false)
		{

			// Test boundaries, adjust start if needed.
			if (duration <= 0)
				return;
			else if (start < _startDate)
			{
				if (start.AddMinutes(duration) <= _startDate)
					return;
				else
					start = _startDate;
			}
			else if (start > _endDate)
				return;

			TimeSpan ts = start.Subtract(_startDate);
			int rounding = (duration % _blockSize == 0 ? 0 : 1);
			if (addValue)
				_units.AddValueRange(value, (int)ts.TotalMinutes / _blockSize, (duration / _blockSize) + rounding);
			else
				_units.SetValueRange(value, (int)ts.TotalMinutes / _blockSize, (duration / _blockSize) + rounding);
		}

		// Not going past 10/19 although all data appears to be there.
		public Intervals FindIntervals(int intervalSize, int stepSize)
		{
			Intervals ret = new Intervals();

			// Start at first opening
			bool keepGoing = true;
			int currUnit = 0;
			Pair<int, int> blockLoc = null;
			while (keepGoing)
			{
				blockLoc = _units.FindBlock((int)BusinessHours.Open, currUnit, intervalSize/_blockSize);
				if (blockLoc != null)
				{
					ret.Add(new Interval(_startDate.AddMinutes(blockLoc.First * _blockSize), _startDate.AddMinutes(blockLoc.Second * _blockSize)));
					currUnit = ((blockLoc.First * _blockSize) + stepSize)/_blockSize;
				}
				else
				{
					currUnit = _units.FindFirstNot((int)BusinessHours.Open, currUnit);
					if (currUnit >= 0)
					{
						currUnit = _units.FindFirst((int)BusinessHours.Open, currUnit + 1);
						if (currUnit < 0)
							keepGoing = false;
					}
					else
						keepGoing = false;
				}
			}

			return ret;
		}
	}
}
