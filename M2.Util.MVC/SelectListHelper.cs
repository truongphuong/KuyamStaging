using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace M2.Util.MVC
{
	public static class SelectListHelper
	{
		public static SelectList GetStateSelectList(string selectedState = null)
		{
			if (selectedState.IsNullOrEmpty())
				selectedState = "CA";

			string[] states = new string[] {
                "AL",
                "AK",
                "AZ",
                "AR",
                "CA",
                "CO",
                "CT",
                "DE",
                "DC",
                "FL",
                "GA",
                "HI",
                "ID",
                "IL",
                "IN",
                "IA",
                "KS",
                "KY",
                "LA",
                "ME",
                "MD",
                "MA",
                "MI",
                "MN",
                "MS",
                "MO",
                "MT",
                "NE",
                "NV",
                "NH",
                "NJ",
                "NM",
                "NY",
                "NC",
                "ND",
                "OH",
                "OK",
                "OR",
                "PA",
                "RI",
                "SC",
                "SD",
                "TN",
                "TX",
                "UT",
                "VT",
                "VA",
                "WA",
                "WV",
                "WI",
                "WY"
			};

			SelectList ret = new SelectList(states, selectedState);

			return ret;
		}

		public static SelectList ToSelectList(this IDNameDict dict)
		{
			return new SelectList(dict, "key", "value");
		}

	}
}
