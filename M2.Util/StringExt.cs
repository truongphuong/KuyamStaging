// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
using System.Text.RegularExpressions;

namespace M2.Util
{
	public static class StringExt
	{
		public static string[] CRLF = new string[] { "\r\n" };
		public static string[] SPACES = new string[] { " " };
		public static char[] CHARSPACE = new char[] { ' ' };
		public static string[] UNDERSCORE = new string[] { "_" };

		public static string GetFilename(this string path)
		{
			string filename = "";
			int ix = path.LastIndexOf('\\');
			if (ix < 0)
				filename = path;
			else
				filename = path.Substring(ix + 1);

			return filename;
		}

		public static Int32 ToInt32(this string s)
		{
			Int32 ix;
			Int32.TryParse(s, out ix);
			return ix;
		}

		public static bool ToBool(this string s)
		{
			if (s == null)
				return false;

			s = s.ToLower().Trim();
			return (s.ToInt32() != 0 || s == "y" || s == "yes" || s == "t" || s == "true");
		}

		public static DateTime ToDateTime(this string s)
		{
			DateTime dt;
			DateTime.TryParse(s, out dt);
			return dt;
		}

		public static double ToDouble(this string s)
		{
			double d;
			double.TryParse(s, out d);
			return d;
		}

		public static string ToHex(string alpha)
		{
			string ret = "";
			foreach (char c in alpha)
			{
				ret += ((int)c).ToString("X");
			}

			return ret.ToLower();
		}

		public static Guid ToGuid(this string s)
		{
			return new Guid(s);
		}

		// ", ", true: 531 W Virginia St, San Bernardino, CA 92405-4520
		// "|", false: 531 W Virginia St|San Bernardino|CA|92405-4520
		public static string ToAddressCase(this string addrIn, string separator, bool pretty)
		{
			string addr = addrIn.ToLowerInvariant().Replace("\r\n", ", ").Replace("  ", " ").Trim();
			addr = ToTitleCase(addr);

			char[] sep = new char[1] { ',' };

			string[] tok = addr.Split(sep, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder sb = new StringBuilder();
			int n = tok.Count() - 1;

			sep[0] = ' ';
			string[] lastLine = tok[tok.Count() - 1].Split(sep, StringSplitOptions.RemoveEmptyEntries);
			int cll = lastLine.Count() - 1;
			sb.Append(lastLine[cll--]); // zip
			if (pretty)
				sb.Insert(0, String.Format("{0}{1} ", separator, lastLine[cll--].ToUpper()));  // state
			else
				sb.Insert(0, String.Format("{0}{1}{0}", separator, lastLine[cll--].ToUpper(), separator));  // state

			// city
			int iy = 0;
			for (iy = cll; iy >= 0; iy--)
				sb.Insert(0, String.Format(" {0}", lastLine[iy]));
			if (sb[0] == ' ')
				sb.Remove(0, 1);

			// street
			for (iy = n - 1; iy >= 0; iy--)
				sb.Insert(0, String.Format("{0}{1}", tok[iy], separator));

			return sb.ToString();
		}

		private static char[] wsDelim = new char[] { ' ' };

		private static List<String> noCasing = new List<string>() { "USA", "LLC", "ABC", "NO", "SO", "P.O.", "PO" };

		public static string ToTitleCase(this string textIn, int minSize = 0)
		{
			string text = textIn;

			string[] toks = text.Split(wsDelim, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder sb = new StringBuilder();
			string s = null;
			bool handled = false;
			foreach (string tok in toks)
			{
				handled = false;
				if (tok.Length >= minSize)
				{
					s = tok.TrimSymbols();
					if (!noCasing.Contains(s))
					{
						s = tok.ToLower();
						int ix = 0;
						while (ix < s.Length && !s[ix].IsLetter())
							sb.Append(s[ix++]);

						if (ix < s.Length)
							sb.Append(s[ix].ToUpper());

						if (s.Length > ix)
							sb.Append(s.Substring(ix + 1));

						handled = true;
					}
				}

				if (!handled)
				{
					sb.Append(tok);
				}

				sb.Append(" ");
			}

			return sb.ToString().Trim();
		}

		public static string FirstCapOnly(this string textIn)
		{
			if (textIn.IsNullOrEmpty())
				return textIn;

			string textOut = textIn.Substring(0, 1).ToUpper();
			if (textIn.Length > 1)
				textOut += textIn.Substring(1).ToLower();

			return textOut;
		}

		public static bool IsValidGuid(this string s)
		{
			bool ret = false;

			try
			{
				Guid g = new Guid(s);
				ret = true;
			}
			catch (Exception)
			{
				ret = false;
			}

			return ret;
		}

		public static Bitmap ToBitmap(this string s, Color forecolor, Color backcolor, string fontName, int pointSize)
		{
			Font font = new Font(fontName, pointSize);
			return ToBitmap(s, forecolor, backcolor, font);
		}

		public static Bitmap ToBitmap(this string s, Color forecolor, Color backcolor, Font font)
		{
			Rectangle rect = new Rectangle(0, 0, (int)(font.Size * s.Length), (int)(font.GetHeight() * 1.20));
			Bitmap bmp = new Bitmap(rect.Width, rect.Height);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.Clear(backcolor);
				using (Brush b = new SolidBrush(forecolor))
				{
					g.DrawString(s, font, b, 0, (int)((rect.Height - font.GetHeight()) / 2));
				}
			}

			return bmp;
		}

		public static String RemoveWhitespace(this string s)
		{
			StringBuilder sb = new StringBuilder();
			string s2 = s.Trim();
			foreach (char c in s2)
			{
				if (c != ' ' && c != '\r' && c != '\n' && c != '\t')
					sb.Append(c);
			}

			return sb.ToString();
		}

		public static string[] TrimAll(this string[] ary)
		{
			for (int ix = 0; ix < ary.Length; ix++)
				ary[ix] = ary[ix].Trim();

			return ary;
		}

		public static string TrimSymbols(this string s)
		{
			string s2 = s.Trim();

			int ix = 0;
			while (ix < s2.Length && !s2[ix].IsLetter() && !s2[ix].IsNumeric())
				ix++;

			int iy = s2.Length - 1;
			while (iy >= 0 && !s2[iy].IsLetter() && !s2[iy].IsNumeric())
				iy--;

			if (iy < ix)
				return "";
			else
				return s.Substring(ix, iy - ix + 1);
		}

		public static string EscapeApostrophes(this string str)
		{
			return str.Replace("'", "''");
		}

		public static string ToSQL(this string str)
		{
			if (str == null)
				return null;
			else
				return str.Replace("\"", "\\\"");
		}

		public static string ToSQL2(this string str)
		{
			if (str == null)
				return null;
			else
				return str.Replace("\"", "'").Replace("''", "'");
		}

		public static string ToSQL3(this string str, string repl)
		{
			if (str == null)
				return null;
			else
				return str.ToSQL2().Replace("'", repl);
		}

		public static string TrimLFCR(this string str)
		{
			if (str != null && str.Length >= 2)
			{
				if (str.StartsWith("\r\n"))
					str = str.Substring(2, str.Length - 2);
				if (str.EndsWith("\r\n"))
					str = str.Substring(0, str.Length - 2);
			}

			return str;
		}

		public static string SubstringSafe(this string str, int start, int length)
		{
			if (start > str.Length - 1)
				return "";
			else if (start + length > str.Length)
				return str.Substring(start);
			else
				return str.Substring(start, length);
		}


		/// <summary>
		/// Returns a substring, not including "fromRight" characters at the end of the string.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="startIndex"></param>
		/// <param name="fromRight"></param>
		/// <returns></returns>
		public static string SubstringRight(this string str, int fromRight)
		{
			return str.SubstringRight(0, fromRight);
		}

		/// <summary>
		/// Returns a substring, not including "fromRight" characters at the end of the string.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="startIndex"></param>
		/// <param name="fromRight"></param>
		/// <returns></returns>
		public static string SubstringRight(this string str, int startIndex, int fromRight)
		{
			return str.SubstringAbsolute(startIndex, str.Length - fromRight - 1);
		}

		public static string SubstringAbsolute(this string str, int startIndex, int endIndex)
		{
			return str.Substring(startIndex, endIndex - startIndex + 1);
		}

		public static string HtmlEncode(this string str)
		{
			return HttpUtility.HtmlEncode(str);
		}

		public static string GetLine(this string src, int firstLine, int lineNo)
		{
			return src.GetLine(lineNo, firstLine, CRLF);
		}

		public static string GetLine(this string src, int lineNo, int firstLine, string[] delimiters)
		{
			if (src.IsNullOrEmpty())
				return null;

			string[] lines = src.Split(delimiters, StringSplitOptions.None);
			if (lines.Length - 1 < lineNo)
				return null;
			else
				return lines[firstLine + lineNo];
		}

		public static string Repeat(this string str, int count)
		{
			StringBuilder sb = new StringBuilder(count);
			for (int ix = 0; ix < count; ix++)
				sb.Append(str);
			return sb.ToString();
		}

		public static string[] ToStringArray(this string str)
		{
			return new string[] { str };
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str); // (str == null || str.Trim() == string.Empty || str.SuperTrim() == string.Empty);
		}

		public static string SuperTrim(this string str)
		{
			if (str == null)
				return "";

			string s = str.Trim();

			int ix = 0;
			while (ix < s.Length && Char.IsWhiteSpace(s[ix]))
				ix++;

			int iy = s.Length - 1;
			while (iy >= 0 && Char.IsWhiteSpace(s[iy]))
				iy--;

			if (iy - ix < 0)
				return "";
			else
				return s.Substring(ix, iy - ix + 1);
		}

		public static StringLoc[] SplitLoc(this string str, string[] delim, StringSplitOptions options)
		{
			string[] toks = str.Split(delim, StringSplitOptions.None);

			List<StringLoc> locs = new List<StringLoc>();
			int start = 0;
			foreach (string tok in toks)
			{
				if (options == StringSplitOptions.None ||
					(options == StringSplitOptions.RemoveEmptyEntries && tok.Length > 0))
				{
					locs.Add(new StringLoc(tok, 0, start));
				}

				start += tok.Length + delim[0].Length;
			}

			return locs.ToArray();
		}

		public static Pair<int, string> IndexOf(this string str, string[] search, int start)
		{
			Pair<int, string> ret = null;

			int pos = 0;
			foreach (string key in search)
			{
				if ((pos = str.IndexOf(key, start)) > 0)
				{
					ret = new Pair<int, string>(pos, key);
					break;
				}
			}

			return ret;
		}

		public static string Unquote(this string str)
		{
			return str.Unsymbol('\"');
		}

		public static string Unsymbol(this string str, char symbol)
		{
			if (str.IsNullOrEmpty())
				return str;

			int len = str.Length;

			int start = 0;
			while (start < str.Length && str[start] == symbol)
				start++;

			int end = str.Length;
			while (end > 0 && str[end - 1] == symbol)
				end--;

			if (end < start)
				return "";
			else if (end == start)
				return str.Substring(start, 1);
			else
				return str.Substring(start, end - start);
		}

		public static void Clear(this string str)
		{
			str = String.Empty;
		}

		public static string LastString(this string str)
		{
			if (str.IsNullOrEmpty())
				return "";

			string[] toks = str.Split(SPACES, StringSplitOptions.RemoveEmptyEntries);
			if (toks.Length > 0)
				return toks[toks.Length - 1];
			else
				return "";
		}

		public static string NullEmpty(this string str)
		{
			return str.IsNullOrEmpty() ? "" : str;
		}

		public static DateTime? FromExcelDate(this string ed)
		{
			DateTime? ret = null;

			if (!ed.IsNullOrEmpty())
				ret = ed.ToDouble().FromExcelDate();

			return ret;
		}

		// http://jarrettmeyer.com/post/2995732471/nested-collection-models-in-asp-net-mvc-3
		private static string JsEncode(this string s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			int i;
			int len = s.Length;
			StringBuilder sb = new StringBuilder(len + 4);
			string t;

			for (i = 0; i < len; i += 1)
			{
				char c = s[i];
				switch (c)
				{
					case '>':
					case '"':
					case '\\':
						sb.Append('\\');
						sb.Append(c);
						break;
					case '\b':
						sb.Append("\\ary");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					case '\n':
						//sb.Append("\\n");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\r':
						//sb.Append("\\r");
						break;
					default:
						if (c < ' ')
						{
							//t = "000" + Integer.toHexString(c); 
							string tmp = new string(c, 1);
							t = "000" + int.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
							sb.Append("\\u" + t.Substring(t.Length - 4));
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			return sb.ToString();
		}

		public static string CollapseSpaces(this string value)
		{
			return Regex.Replace(value, @"\s+", " ");
		}

		public static string RemoveSpaces(this string str)
		{
			return str.Replace(" ", "");
		}

		public static string ToEnumCase(this string str)
		{
			string ret = str.ToTitleCase();
			return ret.Replace(" ", "").Replace("&", "And").Replace("/", "").Replace('-', '_');
		}

		public static string EscapeJS(this string str)
		{
			return str.Replace("'", "\\'").Replace("\"", "\\\"");
		}

		public static string JSONStringify(this string str)
		{
			return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}

		public static Dictionary<string,string> ParseQueryString(this string qs)
		{
			return HttpUtility.ParseQueryString(qs).ToDictionary();
		}
	}
}
