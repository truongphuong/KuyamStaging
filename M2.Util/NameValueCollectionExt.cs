using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace M2.Util
{
	public static class NameValueCollectionExt
	{
		public static Dictionary<string, string> ToDictionary(this NameValueCollection source)
		{
			return source.Cast<string>()
						 .Select(s => new { Key = s, Value = source[s] })
						 .ToDictionary(p => p.Key, p => p.Value);
		}
	}
}
