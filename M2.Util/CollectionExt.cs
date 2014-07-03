using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public static class CollectionExt
	{
		public static string JoinAsString(this ICollection<string> strList, string sep)
		{
			if (sep == null)
				sep = "";

			StringBuilder sb = new StringBuilder();
			foreach (string str in strList)
			{
				sb.AppendFormat("{0}{1}", str, sep);
			}

			if (sb.Length > 2)
				return sb.ToString().Substring(0, sb.Length - sep.Length);
			else
				return null;
		}

		public static string JoinAsString<T>(this ICollection<T> col, string propName, string sep)
		{
			List<String> strs = new List<String>();

			foreach (T obj in col)
				strs.Add(obj.GetPropValue(propName).ToString());

			return JoinAsString(strs, sep);
		}
	}
}
