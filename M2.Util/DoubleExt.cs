using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public static class DoubleExt
	{
		public static DateTime? FromExcelDate(this double ed)
		{
			DateTime? ret = null;
			if (ed > 1000) // 1900's ????
				ret = DateTime.FromOADate(ed);
			else
				ret = null;

			return ret;
		}
	}
}
