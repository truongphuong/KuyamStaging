using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public static class ArrayHelper
	{
		public static T[] Set<T>(this T[] ary, int startIndex, int endIndex, T value)
		{
			if (startIndex < 0)
				throw new ApplicationException("Starting index must be >= 0.");

			int endIndex2 = endIndex;
			if (endIndex2 > ary.Length - 1)
				endIndex2--;
			if (endIndex2 < startIndex || endIndex2 < 0)
				throw new ApplicationException("Ending index must be >= starting index and < length of array.");

			for (int ix = startIndex; ix <= endIndex2; ix++)
				ary[ix] = value;

			return ary;
		}
	}
}
