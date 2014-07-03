// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace M2.Util
{
    public class XmlStringWriter
    {
        private StringWriter StringWriter { get; set; }
        private XmlTextWriter _xmlTW;

        public XmlTextWriter OpenStringWriter()
        {
            StringWriter = new StringWriter();
            _xmlTW = new XmlTextWriter(StringWriter);
            _xmlTW.Formatting = Formatting.Indented;
            _xmlTW.WriteStartDocument();
            return _xmlTW;
        }

        public void CloseStringWriter()
        {
            _xmlTW.Flush();
            _xmlTW.Close();
            StringWriter.Flush();
        }

        public override string ToString()
        {
            return StringWriter.ToString();
        }
    }
}
