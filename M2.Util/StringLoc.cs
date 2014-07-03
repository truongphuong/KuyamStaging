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
    public class StringLoc
    {
        public string String { get; set; }
        public Point Start { get; set; }

        public StringLoc()
        {
            Start = new Point();
        }
        
        public StringLoc(string str, int row, int col)
        {
            String = str;
            Start = new Point(row, col);
        }

        public static implicit operator string(StringLoc s)
        {
            return s.String;
        }
    }
}
