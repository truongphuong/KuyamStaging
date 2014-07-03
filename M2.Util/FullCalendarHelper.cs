using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public class FullCalendarHelper
	{
		public class Event
		{
			public int id {get; set;}
			public string title { get; set; }
			public bool allDay { get; set; }
			public DateTime? start { get; set; }
			public DateTime? end { get; set; }
			public string url { get; set; }
			public string className { get; set; }
			public bool editable { get; set; }
			public string color { get; set; }
			public string backgroundColor { get; set; }
			public string borderColor { get; set; }
			public string textColor { get; set; }

			public Event()
			{
				allDay = true;
			}
		}

		public List<Event> Events { get; set; }

		public DateTime? RequestedStart { get; set; }
		public DateTime? RequestedEnd { get; set; }

		public FullCalendarHelper()
		{
			RequestedStart = null;
			RequestedEnd = null;
			Events = new List<Event>();
		}

		public FullCalendarHelper(string start, string end)
		{
			Events = new List<Event>();
			RequestedStart = Double.Parse(start).FromUnixDateTime();
			RequestedEnd = Double.Parse(end).FromUnixDateTime();
		}

		public string GetEventsJSON()
		{
			return GetEventsJSON(Events);
		}

		public string GetEventsJSON(List<Event> events)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("[");
			foreach (Event e in events)
				sb.AppendFormat("{0},", EventToJSON(e));
			sb.Remove(sb.Length - 1, 1);
			sb.Append("]");

			return sb.ToString();
		}

		private string EventToJSON(Event e)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{");

			if (e.id != 0)
				sb.AppendFormat("\"id\":{0},", e.id);
			if (e.title != null)
				sb.AppendFormat("\"title\":\"{0}\",", e.title);
			if (e.start != null)
				sb.AppendFormat("\"start\":\"{0}\",", e.start.Value.ToString("yyyy-MM-dd HH:mm:ss"));
			if (e.end != null)
				sb.AppendFormat("\"end\":\"{0}\",", e.end.Value.ToString("yyyy-MM-dd HH:mm:ss"));
			if (e.url != null)
				sb.AppendFormat("\"url\":\"{0}\",", e.url);
			if (e.className != null)
				sb.AppendFormat("\"className\":\"{0}\",", e.className);
			if (e.color != null)
				sb.AppendFormat("\"color\":\"{0}\",", e.color);
			if (e.backgroundColor != null)
				sb.AppendFormat("\"backgroundColor\":\"{0}\",", e.backgroundColor);
			if (e.borderColor != null)
				sb.AppendFormat("\"borderColor\":\"{0}\",", e.borderColor);
			if (e.textColor != null)
				sb.AppendFormat("\"textColor\":\"{0}\",", e.textColor);

			sb.AppendFormat("\"allDay\":{0},", e.allDay.ToString().ToLower());
			sb.AppendFormat("\"editable\":\"{0}\"", e.editable.ToString().ToLower());


			
			sb.Append("}");

			return sb.ToString();
		}
	}
}
