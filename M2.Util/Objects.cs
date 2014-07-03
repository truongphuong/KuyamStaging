// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC
using System;
using System.Collections.Generic;

using System.Text;

namespace M2.Util
{
	public static class Objects
	{
		public static object GetNewObject(Type t)
		{
			try
			{
				return t.GetConstructor(new Type[] { }).Invoke(new object[] { });
			}
			catch
			{
				return null;
			}
		}

		public static T GetNewObject<T>()
		{
			try
			{
				return (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
			}
			catch
			{
				return default(T);
			}
		}
	}
}
