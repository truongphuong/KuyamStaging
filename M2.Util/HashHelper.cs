// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace M2.Util
{
    public static class HashHelper
    {
        public static string GetHashCode(string text, string algorithm = "SHA1")
        {
            string hashStr = null;
            if (text != null)
            {
                using (HashAlgorithm alg = HashAlgorithm.Create("SHA1"))
                {
                    byte[] textData = Encoding.Default.GetBytes(text);
                    byte[] hash = alg.ComputeHash(textData);
                    hashStr = Convert.ToBase64String(hash); // BitConverter.ToString(hash);
                }
            }

            return hashStr;
        }
    }
}
