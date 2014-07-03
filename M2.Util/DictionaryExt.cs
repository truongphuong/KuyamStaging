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
    public static class DictionaryExt
    {
        // If value = cond, set dstkey to value of copykey.  If false or copykey doesn't exist, and elseval != null, set it to elseval
        // (i.e. null means leave as-is)
        public static void CopyItem<t, u>(this Dictionary<t,u> dic, t srcKey, u cond, t dstKey, t copyKey, u elseVal)
        {
            u srcValue;
            u copyValue;

            if (dic.TryGetValue(srcKey, out srcValue) && srcValue.Equals(cond) && dic.TryGetValue(copyKey, out copyValue))
            {
                dic[dstKey] = copyValue;
            }
            else if (elseVal != null)
            {
                dic[dstKey] = elseVal;
            }
        }

        // If value = cond, set dstkey to newval.  If false, and elseval != null, set it to elseval
        // (i.e. null means leave as-is)
        public static void CopyLiteral<t, u>(this Dictionary<t,u> dic, t srcKey, u cond, t dstKey, u newVal, u elseVal)
        {
            u srcValue;

            if (dic.TryGetValue(srcKey, out srcValue) && srcValue.Equals(cond))
            {
                dic[dstKey] = newVal;
            }
            else if (elseVal != null)
            {
                dic[dstKey] = elseVal;
            }
        }

        public static void EqualizeItems<t, u>(this Dictionary<t, u> dic, t key1, t key2)
        {
            u val;

            if (dic.TryGetValue(key1, out val))
                dic[key2] = val;
            else if (dic.TryGetValue(key2, out val))
                dic[key1] = val;
        }

        public static StringBuilder ShowJSON(this Dictionary<string, object> dic, int indent, StringBuilder sb, string keySuffix = "")
        {
            foreach (string key in dic.Keys)
            {
                sb.AppendFormat("{0}{1}{2}: ", new String(' ', indent), key, keySuffix);
                if (dic[key] is Dictionary<string, object>)
                {
                    sb.AppendLine("(object)");
                    ((Dictionary<string, object>)dic[key]).ShowJSON(indent + 3, sb);
                }
                else if (dic[key] is object[])
                {
                    sb.AppendLine("(array)");
                    int ix = 0;
                    foreach (object o in (object[])dic[key])
                    {
                        if (o is Dictionary<string, object>)
                        {
                            Dictionary<string, object> keyObject = o as Dictionary<string,object>;
                            keyObject.ShowJSON(indent + 3, sb, String.Format("[{0}]", ix.ToString()));
                        }
                        else
                            sb.AppendFormat("{0}[{1}]: {2}\r\n", new String(' ', indent + 3), ix, o.ToString());
                        ix++;
                    }
                }
                else
                {
                    sb.Append(dic[key]);
                }
                sb.AppendLine();
            }

            return sb;
        }

    }
}
