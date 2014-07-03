using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Kuyam.Domain
{
    public class UtilityHelper
    {
        /// <summary>
        /// Input: empty, out: empty
        /// Input: null, out: empty
        /// Input: abc, out: http://abc
        /// input: abc.com, out: http://abc.com
        /// input: http://abc.com, out: http://abc.com
        /// input: https://abc.com, out: https://abc.com
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlFormat(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) { return string.Empty; }
            url = url.ToLower();
            return url.StartsWith("http://") || url.StartsWith("https://") ? url : string.Format("http://{0}", url);
        }

        public static string ReplaceText(string str)
        {

            string result = string.Empty;

            result = str.Replace("\"", "\\\"");

            return result;
        }

        //public static string TruncateText(string input, int length)
        //{

        //    string result = string.Empty;
        //    if (string.IsNullOrWhiteSpace(input) || input == null) { return string.Empty; }
        //    if (input.Length <= length)
        //    {
        //        result = input;
        //    }
        //    else
        //    {
        //        result = string.Format("{0}...", input.Substring(0, length));
        //    }
        //    return result;
        //}
        public static string TruncateText(string input, int length)
        {
            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(input) || input == null) { return string.Empty; }

            int strNumber = 0;
            string strOutput = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                strNumber += char.IsUpper(input[i]) ? 2 : 1;
                if (strNumber <= length)
                {
                    strOutput += input[i];
                }

            }
            if (strNumber <= length)
            {
                result = input;
            }
            else
            {
                result = string.Format("{0}...", strOutput);
            }

            return result;
        }

        public static string TruncateAtWord(string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        /*
        public static string TruncateText(string input, int length)
        {

            byte[] unicodeBytes = Encoding.UTF8.GetBytes(input);

            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(input) || input == null) { return string.Empty; }

            if (unicodeBytes.Length <= length)
            {
                result = input;
            }
            else
            {
                string substring = Encoding.UTF8.GetString(unicodeBytes, 0, length);
                result = string.Format("{0}...", substring);
            }
            return result;
        }
        */
        public static string TruncateData(string input, int length)
        {

            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(input) || input == null) { return string.Empty; }
            if (input.Length <= length)
            {
                result = input;
            }
            else
            {
                result = string.Format("{0}", input.Substring(0, length));
            }
            return result;
        }

        public static string ConvertEnum(string str)
        {
            string result = string.Empty;
            char[] letters = str.ToCharArray();
            foreach (char c in letters)
                if (c.ToString() != c.ToString().ToLower())
                    result += " " + c.ToString();
                else
                    result += c.ToString();
            return result;
        }

        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }


        public static string ObjectToXml(object obj)
        {
            StringBuilder xml = new StringBuilder("");
            if (obj == null) { return xml.ToString(); ; }
            Type type = obj.GetType();
            foreach (PropertyInfo item in type.GetProperties())
            {
                xml.AppendFormat("<{0}><![CDATA[{1}]]></{0}>", item.Name, item.GetValue(obj, null));
            }
            string name = type.Name;
            Regex reg = new Regex(@"(\w+)", RegexOptions.IgnoreCase);
            if (!reg.IsMatch(name))
            {
                name = "EntityName";
            }
            else
            {
                name = reg.Match(name).Groups[1].Value;
            }
            return string.Format("<{0}>{1}</{0}>", name, xml.ToString());
        }

        public static string ToXml(object obj)
        {
            string retval = null;
            var nsSerializer = new XmlSerializerNamespaces();
            nsSerializer.Add("", "");
            if (obj != null)
            {
                StringBuilder sb = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true }))
                {
                    new XmlSerializer(obj.GetType()).Serialize(writer, obj, nsSerializer);
                }
                retval = sb.ToString();
            }
            return retval;
        }

        public static string FormatPhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;
            string results = string.Empty;
            string formatPattern = @"(\d{3})(\d{3})(\d{4})";
            results = Regex.Replace(phoneNumber, formatPattern, "($1) $2-$3");
            return results;
        }
        public static string CleanPhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;
            System.Text.RegularExpressions.Regex digitsOnly = new System.Text.RegularExpressions.Regex(@"[^\d;]");
            return digitsOnly.Replace(phoneNumber, "");
        }

        #region Convert Tools
        public static object ConvertTo<T>(Dictionary<string, object> source)
        {
            Type myType = typeof(T);
            var destination = Activator.CreateInstance(myType);
            foreach (var pair in source)
            {
                var key = pair.Key;
                var value = pair.Value;
                var propertyInfo = destination.GetType().GetProperty(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = DateTime.Parse(value.ToString());
                        propertyInfo.SetValue(destination, dt, null);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        bool tmp = bool.Parse(value.ToString());
                        propertyInfo.SetValue(destination, tmp, null);
                    }
                    else
                    {
                        propertyInfo.SetValue(destination, value, null);
                    }

                }
            }
            return destination;
        }

        public static object ChangeType(Type type, object source)
        {
            var sourceType = source.GetType();
            var destType = type;

            if (destType == typeof(string) || sourceType.IsValueType || destType.IsValueType)
                return Convert.ChangeType(source, destType);
            var destObj = Activator.CreateInstance(type);
            var properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop1 in properties)
            {

                var prop2 = destType.GetProperty(prop1.Name);

                // destType does not has a property named prop1.Name
                if (prop2 == null)
                    continue;

                var value = prop1.GetValue(source, null);

                // prop1 and prop2 are not same type
                if (prop1.PropertyType != prop2.PropertyType)
                    continue;

                prop2.SetValue(destObj, value, null);
            }
            return destObj;
        }

        #endregion

        #region remove Html, XML tags
        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            var temporary = RemoveUnExpectedWords(source, "<w:[^>]*>(.*?)</w:[^>]*>");
            temporary = RemoveUnExpectedWords(temporary, "<meta[^>]*>");
            temporary = RemoveUnExpectedWords(temporary, "<link[^>]*>");
            temporary = RemoveUnExpectedWords(temporary, @"<style[^>]*>([\w|\W|\n]*?)</style>");
            temporary = RemoveUnExpectedWords(temporary, @"<!--([\w|\W|\n]*?)-->");
            temporary = RemoveUnExpectedWords(temporary, @"<.*?>");
            temporary = RemoveUnExpectedWords(temporary, "[\"|\']");
            temporary = temporary.Trim();
            if (string.IsNullOrEmpty(temporary)) return string.Empty;
            return temporary;
        }

        static string RemoveUnExpectedWords(string source, string regex)
        {
            Regex htmlRegex = new Regex(regex, RegexOptions.Compiled);
            if (string.IsNullOrEmpty(source)) return string.Empty;
            return htmlRegex.Replace(source, string.Empty);
        }
        #endregion

        //compare 2 string
        public static int DamerauLevenshteinDistance(string source, string target)
        {

            source = source.Replace(" ", "");
            source = source.Replace(".", "");
            source = source.Replace(",", "");
            source = source.Replace("&", "");

            target = target.Replace(" ", "");
            target = target.Replace(".", "");
            target = target.Replace(",", "");
            target = target.Replace("&", "");

            source = source.ToLower();
            target = target.ToLower();

            if (String.IsNullOrEmpty(source))
            {
                return String.IsNullOrEmpty(target) ? 0 : target.Length;
            }
            if (String.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            var score = new int[source.Length + 2, target.Length + 2];
            var inf = source.Length + target.Length; score[0, 0] = inf;
            for (var i = 0; i <= source.Length; i++)
            {
                score[i + 1, 1] = i;
                score[i + 1, 0] = inf;
            }
            for (var j = 0; j <= target.Length; j++)
            {
                score[1, j + 1] = j; score[0, j + 1] = inf;
            }
            var sd = new SortedDictionary<char, int>();
            foreach (var letter in (source + target))
            {
                if (!sd.ContainsKey(letter) && letter != '.')
                    sd.Add(letter, 0);
            }
            for (var i = 1; i <= source.Length; i++)
            {
                var db = 0;
                for (var j = 1; j <= target.Length; j++)
                {
                    var i1 = sd[target[j - 1]];
                    var j1 = db;
                    if (source[i - 1] == target[j - 1])
                    {
                        score[i + 1, j + 1] = score[i, j];
                        db = j;
                    }
                    else
                    {
                        score[i + 1, j + 1] = Math.Min(score[i, j], Math.Min(score[i + 1, j], score[i, j + 1])) + 1;
                    }
                    score[i + 1, j + 1] = Math.Min(score[i + 1, j + 1], score[i1, j1] + (i - i1 - 1) + 1 + (j - j1 - 1));
                }
                sd[source[i - 1]] = i;
            }
            return score[source.Length + 1, target.Length + 1];
        }
    }
}
