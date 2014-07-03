// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Specialized;

using System.Text;

namespace M2.Util
{
	public class StreetAddress
	{
        public double Lat { get; set; }
        public double Lon { get; set; }

        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public StreetAddress()
        {
        }

        public StreetAddress(string data)
        {
            Parse(data);
        }

        public void Clear()
        {
            Street1 = Street2 = City = State = Zip = null;
        }
        
        public double GetDistance(StreetAddress dest)
        {
            throw new NotImplementedException("M2.Util.StreetAddress.GetDistance()");

            //return -1;
        }

        public string CleanZip5()
        {
            return CleanZip5(Zip);
        }

        public static string CleanZip5(string zip)
        {
            // Get, check and clean the zip code
            if (zip == null)
                throw new ApplicationException("Missing zip code.");

            zip = zip.Trim().Substring(0, 5);
            if (zip.Length == 0)
                throw new ApplicationException("Incorrect zip code.");

            return zip;
        }

        // Assumes "normal" setup
        // street
        // street 2
        // city[,] state zip
        public void Parse(string data)
        {
            Clear();

            if (data == null)
                return;

            string[] lines = data.SuperTrim().Split(StringExt.CRLF, StringSplitOptions.RemoveEmptyEntries);

            int count = lines.Length;
            if (count == 0)
                return;

            Street1 = lines[0].SuperTrim();
            if (count > 2)
                Street2 = lines[1].SuperTrim();

            string[] csz = lines[count-1].Split(StringExt.SPACES, StringSplitOptions.RemoveEmptyEntries);
            int ix = csz.Length;
            if (ix >= 3)
            {
                Zip = csz[ix - 1].SuperTrim();
                State = csz[ix - 2].SuperTrim();

                City = "";
                for (int iy = 0; iy < csz.Length - 2; iy++)
                    City += csz[iy] + " ";
                City = City.SuperTrim();
            }
        }

        public string GetDelimString(char delim)
        {
            return String.Format("{0}{5}{1}{5}{2}{5}{3}{5}{4}", Street1, Street2, City, State, Zip, delim);
        }

        public static string GetUSPSValidAddress(string street, string city, string state, string zip)
        {
            NameValueCollection nv = new NameValueCollection();
            nv["visited"] = "1";
            nv["pagenumber"] = "0";
            nv["firmname"] = "";
            nv["address2"] = street.Replace(' ', '+');
            nv["address1"] = "";
            nv["city"] = city.Replace(' ', '+');
            nv["state"] = state.Replace(' ', '+');
            nv["urbanization"] = "";
            nv["zip5"] = zip.Replace(' ', '+');
            nv["submit.x"] = "47";
            nv["submit.y"] = "10";
            nv["submit"] = "Find+ZIP+Code";

            string raw = M2.Util.Web.PostData("http://zip4.usps.com/zip4/zcl_0_results.jsp", nv);
            string ret = null;

            int ix = 0;
            ix = raw.IndexOf("We returned more than one result based on the information you provided.");
            if (ix > 0)
                return "ERROR: Multiple addresses found.";

            ix = raw.IndexOf("<td headers=\"full\" height=\"34\" valign=\"top\" class=\"main\" style=\"background:url(images/table_gray.gif); padding:5px 10px;\">");
            if (ix > 0)
            {
                ix += 20;

                int iy = raw.IndexOf(">", ix);
                if (iy > ix)
                {
                    iy++;

                    int iz = raw.IndexOf("</td>", iy);
                    if (iz > 0)
                    {
                        ret = raw.Substring(iy, iz - iy - 1);
                        //ret = ret.Replace("<br />", "").Replace("&nbsp;", " ").Replace("\r\n\r\n", "\r\n").Replace("\t", "").Trim();
                        ret = ret.Replace("<br />", "|").Replace("&nbsp;", "|").Replace("\r\n", "|").Replace("\t", "").Trim();
                        while (ret.IndexOf("||") >= 0)
                            ret = ret.Replace("||", "|");
                        while (ret.StartsWith("|"))
                            ret = ret.Substring(1, ret.Length - 1);
                        while (ret.EndsWith("|"))
                            ret = ret.Substring(0, ret.Length - 1);
                        ret = ret.Replace("<br />", "").Replace("&nbsp;", "").Replace("\r\n", "").Replace("\t", "").Trim();

                        while (ret.IndexOf("  ") >= 0)
                            ret.Replace("  ", " ");
                        while (ret.IndexOf(" |") > 0)
                            ret.Replace(" |", "|");
                        while (ret.IndexOf("| ") > 0)
                            ret.Replace("| ", "|");
                    }
                }
            }

            if (ret == null)
                ret = "ERROR: Address validation failed for unknown reason. See log for more information.";

            return ret;
        }

	}
}
