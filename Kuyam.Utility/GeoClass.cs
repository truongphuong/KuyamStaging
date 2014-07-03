using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Drawing;

namespace Kuyam.Utility
{
    public class GeoClass
    {
        //private const string _googleUri = "http://maps.google.com/maps/geo?q=";
        private const string _outputType = "csv";

        private const string _googleUri = "http://maps.googleapis.com/maps/api/geocode/json?address=";

        public static string ApiKey
        {
            get { return ConfigurationManager.AppSettings["googleMapApiKey"]; }
        }

        public static string Sign(string url, string keyString)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();

            // converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
            string usablePrivateKey = keyString.Replace("-", "+").Replace("_", "/");
            byte[] privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

            Uri uri = new Uri(url);
            byte[] encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

            // compute the hash
            HMACSHA1 algorithm = new HMACSHA1(privateKeyBytes);
            byte[] hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

            // convert the bytes to string and make url-safe by replacing '+' and '/' characters
            string signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

            // Add the signature to the existing URI.
            return signature;
        }


        //private static Uri GetGeocodeUri(string address, string googleKey)
        //{
        //    address = HttpUtility.UrlEncode(address);
        //    string sign = Sign(string.Format("http://maps.googleapis.com/maps/api/geocode/json?address={0}&client={1}&sensor=true", address, "gme-221504819465-5d1bsrvqnt5d828vc5upb33ftb8hilpe.apps.googleusercontent.com"), "NLrVS11agvg2uTkBzfJBPyQm");

        //    string url1 = string.Format("http://maps.googleapis.com/maps/api/geocode/json?address={0}&client={1}&signature={2}&sensor=true", address, "gme-221504819465-5d1bsrvqnt5d828vc5upb33ftb8hilpe.apps.googleusercontent.com", sign);
        //    return new Uri(url1);
        //}
        /*
        private static Uri GetGeocodeUri(string address, string googleKey)
        {
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&output={2}&key={3}", _googleUri, address, _outputType, googleKey));
        }

        public static Coordinate GetCoordinates(string address, string googleKey = "")
        {
            if (googleKey == "")
                googleKey = ApiKey;
            WebClient client = new WebClient();
            Uri uri = GetGeocodeUri(address, googleKey);

            string[] geocodeInfo = client.DownloadString(uri).Split(',');

            return new Coordinate(Convert.ToDecimal(geocodeInfo[2]), Convert.ToDecimal(geocodeInfo[3]));
        }
        */

        private static Uri GetGeocodeUri(string address)
        {
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&sensor=true", _googleUri, address));
        }


        public static Coordinate GetCoordinates(string address)
        {
            try
            {
                WebClient client = new WebClient();
                Uri uri = GetGeocodeUri(address);

                string geocodeInfo = client.DownloadString(uri);

                var json_serializer = new JavaScriptSerializer();
                IDictionary<string, dynamic> list = (IDictionary<string, dynamic>)json_serializer.DeserializeObject(geocodeInfo);

                var location = list["results"][0]["geometry"]["location"];

                return new Coordinate(Convert.ToDecimal(location["lat"]), Convert.ToDecimal(location["lng"]));
            }
            catch (Exception)
            {

                return new Coordinate(0, 0);
            }

        }



        public class Coordinate
        {
            private decimal _latitude;
            private decimal _longitude;

            public Coordinate(decimal latitude, decimal longitude)
            {
                _latitude = latitude;
                _longitude = longitude;
            }

            #region ISpatialCoordinate Members

            public decimal Latitude
            {
                get
                {
                    return _latitude;
                }
                set
                {
                    this._latitude = value;
                }
            }

            public decimal Longitude
            {
                get
                {
                    return _longitude;
                }
                set
                {
                    this._longitude = value;
                }
            }

            #endregion
        }


        public static bool CheckWithinRectangle(PointF pt, PointF a, PointF b, PointF c, PointF d)
        {
            bool ab = (pt.Y <= (pt.X * heso(a, b)[0] + heso(a, b)[1]));
            bool ad = (pt.Y >= (pt.X * heso(a, d)[0] + heso(a, d)[1]));
            bool bc = (pt.Y > (pt.X * heso(b, c)[0] + heso(b, c)[1]));
            bool cd = (pt.Y > (pt.X * heso(c, d)[0] + heso(c, d)[1]));
            return (ab && ad && bc && cd);
        }

        private static float[] heso(PointF M, PointF N)
        {
            float[] a = new float[2];
            a[0] = (M.Y - N.Y) / (M.X - N.X);//a
            a[1] = (N.Y * M.X - N.X * M.Y) / (M.X - N.X);//b
            return a;
        }
    }
}
