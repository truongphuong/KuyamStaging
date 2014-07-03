using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
    public class StringModel
    {
        public string Value { get; set; }

        public StringModel()
        {

        }

        public StringModel(string value)
        {
            Value = value;
        }
    }

	public class DateTimePicker
	{
		public DateTime Value { get; set;}
		public bool ShowDate { get; set; }
		public bool ShowTime { get; set; }
		
		public void DateTimePickerModel()
		{
			ShowDate = true;
			ShowTime = true;
		}
	}
}