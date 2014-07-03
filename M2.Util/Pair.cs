// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
    [Serializable]
    public class Pair<T, U>
    {
        public T First { get; set; }
        public U Second { get; set; }

        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            First = first;
            Second = second;
        }

        public Pair(Pair<T, U> src)
        {
            Copy(src);
        }

        public void Clear()
        {
            First = default(T);
            Second = default(U);
        }

        public void Copy(Pair<T,U> src)
        {
            Clear();
            First = src.First;
            Second = src.Second;
        }
    }

    [Serializable]
    public class Triple<T, U, V>
    {
        public T First { get; set; }
        public U Second { get; set; }
        public V Third { get; set; }
        
        public Triple()
        {
        }

        public Triple(T first, U second, V third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public Triple(Triple<T, U, V> src)
        {
            Copy(src);
        }

        public void Clear()
        {
            First = default(T);
            Second = default(U);
        }

        public void Copy(Triple<T, U, V> src)
        {
            Clear();
            First = src.First;
            Second = src.Second;
            Third = src.Third;
        }
    }

    public class IDNamePair : Pair<int, string>
    {
        public IDNamePair(int id, string name)
            : base(id, name)
        {
        }

        public int ID
        {
            get { return First; }
            set { First = value; }
        }

        public string Name
        {
            get { return Second; }
            set { Second = value; }
        }
    }
}
