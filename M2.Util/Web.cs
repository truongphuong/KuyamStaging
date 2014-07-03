// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Web;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace M2.Util
{
    public static class Web
    {
        public static string GetPage(string url)
        {
            string ret = null;

            using (WebClient cli = new WebClient())
            {
                // Try up to 5 times and then throw an error
                for (int ix = 0; ix < 5; ix++)
                {
                    try
                    {
                        ret = cli.DownloadString(url).Trim();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ix == 4)
                        {
                            // Tried 5 times over 5 seconds - give it up
                            throw ex;
                        }
                        else
                        {
                            // Wait and try again
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
            }

            return ret;
        }

        public static string PostData(string url, string data)
        {
            return PostData(url, null, data, false);
        }

        public static string PostData(string url, string name, string value, bool encode)
        {
            NameValueCollection nv = new NameValueCollection();
            if (encode)
                nv[name] = HttpUtility.HtmlEncode(value);
            else
                nv[name] = value;

            return PostData(url, nv);
        }

        public static string PostData(string url, NameValueCollection data)
        {
            string response = null;

            using (WebClient cli = new WebClient())
            {
                cli.Proxy = null;
                cli.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.0.3705; .NET CLR 1.1.4322)");
                cli.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                // Try up to 5 times and then throw an error
                for (int ix = 0; ix < 5; ix++)
                {
                    try
                    {
                        byte[] by = cli.UploadValues(url, data);
                        response = Encoding.ASCII.GetString(by);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ix == 2 || ex.Message.ToLower().EndsWith("internal server error."))
                        {
                            // Tried 3 times over 3 seconds - give it up
                            throw ex;
                        }
                        else
                        {
                            // Wait and try again
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
            }

            return response;
        }

        public static bool GetAuth(string tag)
        {
            try
            {
                string ret = GetPage("http://www.markiisoftware.com/auth?id=" + tag);
                return (ret != "stop");
            }
            catch (Exception)
            {
                return true;
            }
        }
        
        /*
                public static bool GetAuth(string tag)
                {
                    try
                    {
                        WebRequest request = WebRequest.Create("http://www.markiisoftware.com/auth?id=" + tag);
                        request.Credentials = CredentialCache.DefaultCredentials;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();
                        response.Close();

                        return !(responseFromServer == "stop");
                    }
                    catch (Exception)
                    {
                        return true;
                    }
                }
        */

        //[OperationContract]
        // TODO: Use the WebClient class?
        // From http://morewally.com/cs/blogs/wallym/archive/2009/03/20/twitter-api-submit-a-post-in-c.aspx 
        public static void Tweet(string username, string password, string tweet)
        {
            // encode the username/password
            string user = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
            // determine what we want to upload as a status
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("status=" + tweet);
            // connect with the update page
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://twitter.com/statuses/update.xml");
            // set the method to POST
            request.Method = "POST";
            // thanks to argodev for this recent change!
            request.ServicePoint.Expect100Continue = false;
            // set the authorisation levels
            request.Headers.Add("Authorization", "Basic " + user);
            request.ContentType = "application/x-www-form-urlencoded";
            // set the length of the content
            request.ContentLength = bytes.Length;
            // set up the stream
            Stream reqStream = request.GetRequestStream();
            // write to the stream
            reqStream.Write(bytes, 0, bytes.Length);
            // close the stream
            reqStream.Close();
        }

        /// <summary>
        /// Given a web page url, it will retrieve the Html from that page and parse the image tags in that page
        /// </summary>
        /// <param name="url">The Web page url in this format "http;//www.msn.com"</param>
        /// <returns>Returns a list of image urls as strings based on the url of a Web page</returns>
        public static List<string> GetAllImagesFromUrl(string url)
        {
            List<string> urlList = new List<string>();
            string rawHtml = String.Empty;

            //read the contents of the web page into a string
            using (StreamReader sr = new StreamReader(new WebClient().OpenRead(url)))
            {
                rawHtml = sr.ReadToEnd();
            }

            //regular expression to part out <img> tags from the html
            string regExPattern = @"< \s* img [^\>]* src \s* = \s* [\""\']? ( [^\""\'\s>]* )";

            Regex r = new Regex(regExPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            MatchCollection matches = r.Matches(rawHtml);

            foreach (Match m in matches)
            {
                urlList.Add(m.Groups[1].Value);
            }

            return urlList;
        }

        //    public static bool DoesFileAlreadyExist(string imgUrl, string destPath)
        //    {
        //        if (File.Exists(destPath))
        //        {
        //            WebResponse imgResp = null;
        //            DateTime lastMod;
        //try
        //{
        //        WebRequest imgRequest = WebRequest.Create(imgUrl.TrimStart);
        //            imgResp = imgRequest.GetResponse;
        //            lastMod = DateTime.Parse(imgResp.Headers("Last-Modified"));

        //            imgInfo = new FileInfo(destPath);
        //            If DateTime.Compare(imgInfo.CreationTime, lastMod) > 0
        //            {
        //                Console.WriteLine("SKIP - " & destPath);
        //                    continue;
        //        }
        //}
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("ERROR - " + imgPath);
        //    }
        //        finally
        //    {
        //        if (imgResp !=null)
        //                imgResp.Close();
        //    }
        //    }
        //}
        //}
    }
}
