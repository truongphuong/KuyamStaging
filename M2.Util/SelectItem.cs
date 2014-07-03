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
    public class SelectItem<T>
    {
        public T Item { get; set; }
        public bool Selected { get; set; }
    }

    public class SelectItem
    {
        public bool Selected { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }

        public SelectItem()
        {
        }

        public SelectItem(string text, string value, bool selected)
        {
            Text = text;
            Value = value;
            Selected = selected;
        }
    }
}
