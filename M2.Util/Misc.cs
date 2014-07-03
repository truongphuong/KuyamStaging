// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace M2.Util
{
    public static class Misc
    {
        public static string GetVersion(System.Type assemblyType)
        {
            return System.Reflection.Assembly.GetAssembly(assemblyType).GetName().Version.ToString();
        }
    }

    public class DictEnum<T> : Dictionary<string, T>
    {
        //public string GetKey(T val)
        //{
        //    return this.Keys.Where(k => this[k].Equals(val)).FirstOrDefault();
        //}

        public string this[T val]
        {
            get { return this.Keys.Where(k => this[k].Equals(val)).FirstOrDefault(); }
            set { this[value] = val;  }
        }
    }

    public class StandardTypeList : DictEnum<int>
    {

    }

	public class IDNameDict : Dictionary<int,string>
	{

	}
}
