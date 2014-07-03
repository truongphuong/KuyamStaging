using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;

namespace Kuyam.WebUI.Models
{
	public class ProfileHoursExceptionModel : IModel
	{
		ProfileHoursException ProfileHoursException { get; set; }

		public void LockAndLoad()
		{

		}
	}
}