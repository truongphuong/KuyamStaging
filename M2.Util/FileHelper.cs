// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.IO;

namespace M2.Util
{
    public class FileHelper
    {
        public static void DeleteAll(string path)
        {
            Delete(path, "*.*", true);
        }

        public static void Delete(string path, string spec, bool subdirectories)
        {
            if (!Directory.Exists(path))
                return;
                //throw new DirectoryNotFoundException();

            string[] files = Directory.GetFiles(path, spec, subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (string f in files)
            {
                File.Delete(f);
            }
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            string src = sourceFileName;
            string dst = destFileName;

            // If copying from a directory
            if (Directory.Exists(sourceFileName))
            {
                throw new NotImplementedException();
            }

            // If copying to a directory
            if (Directory.Exists(destFileName))
            {
                dst += src.Substring(src.LastIndexOf("\\"));
            }

            // Do the copy
            File.Copy(src, dst, overwrite);
        }

        public static DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

		public static void SetLastWriteTime(string path)
		{
			File.SetLastWriteTime(path, DateTime.Now);
		}
    }
}
