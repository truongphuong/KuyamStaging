using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public static class ExceptionExt
	{
		public static Exception Innermost(this Exception ex)
		{
			return ex.GetBaseException();
		}
	}

}
