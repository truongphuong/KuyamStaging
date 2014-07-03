// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using M2.Util;

namespace M2.Util
{
    public class TextFileMgr
    {
        public string Path { get; set; }
        public string[] Lines { get; set; }
        public int CurrentLineNo { get; set; }

        public TextFileMgr(string path = null)
        {
            Init(path);
        }

        public void Init(string path)
        {
            Path = path;
            CurrentLineNo = 0;

            if (!path.IsNullOrEmpty())
            {
                Lines = File.ReadAllLines(path);
            }
        }

        public void ResetCurrentLineNo()
        {
            CurrentLineNo = 0;
        }

        public string GetNextLine()
        {
            if (CurrentLineNo < Lines.Count() && Lines.Count() > 0)
                return Lines[CurrentLineNo++];
            else
                return null;
        }

        public string GetCurrentLine()
        {
            if (CurrentLineNo >= 0 && Lines.Count() > 0 && CurrentLineNo < Lines.Count())
                return Lines[CurrentLineNo];
            else
                return null;
        }

    }
}
