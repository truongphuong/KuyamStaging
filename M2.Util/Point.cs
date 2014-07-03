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
    public class Point
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Point()
        {
        }

        public Point(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public Point(Point src)
        {
            Copy(src);
        }

        public void Clear()
        {
            Row = default(int);
            Col = default(int);
        }

        public void Copy(Point src)
        {
            Clear();
            Row = src.Row;
            Col = src.Col;
        }
    }
}
