// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;

namespace M2.Util
{
    public static class ObjectExt
    {
        public static Int32 ToInt32(this object o)
        {
            if (o == null)
                return 0;
            else
                return o.ToString().ToInt32();
        }

        public static bool ToBool(this object o)
        {
            if (o == null)
                return false;
            else
                return o.ToString().ToBool();
        }

        public static void CopyFrom<T>(this T destination, T source)
        {
            PropertyInfo[] propertyInfos = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {

                PropertyInfo destinationPropertyInfo = destination.GetType().GetProperty(propertyInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (destinationPropertyInfo != null)
                {

                    if (destinationPropertyInfo.CanWrite && propertyInfo.CanRead && (destinationPropertyInfo.PropertyType == propertyInfo.PropertyType))
                        destinationPropertyInfo.SetValue(destination, propertyInfo.GetValue(source, null), null);
                }
            }
        }

		public static object GetPropValue(this object src, string propName)
		{
			return src.GetType().GetProperty(propName).GetValue(src, null);
		}
    }
}
