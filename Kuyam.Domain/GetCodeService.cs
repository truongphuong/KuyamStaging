using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace Kuyam.Domain
{    
    public static class GetCodeService
    {
        private const string _googleUri = "http://maps.google.com/maps/geo?q=";
        private const string _outputType = "csv";

        private static Uri GetGeocodeUri(string address, string googleKey = "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY")
        {
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&output={2}&key={3}", _googleUri, address, _outputType, googleKey));
        }

        public static Coordinate GetCoordinates(string address, string googleKey)
        {
            WebClient client = new WebClient();
            Uri uri = GetGeocodeUri(address, googleKey);

            string[] geocodeInfo = client.DownloadString(uri).Split(',');

            return new Coordinate(Convert.ToDouble(geocodeInfo[2]), Convert.ToDouble(geocodeInfo[3]));
        }

        public class Coordinate
        {
            private double _latitude;
            private double _longitude;

            public Coordinate(double latitude, double longitude)
            {
                _latitude = latitude;
                _longitude = longitude;
            }

            #region ISpatialCoordinate Members

            public double Latitude
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

            public double Longitude
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
    }

}
