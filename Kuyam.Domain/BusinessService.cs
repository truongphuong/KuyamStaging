using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Caching;
//using M2.Util;
using System.Security.Principal;
using System.Diagnostics;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Net;
using System.Data;
using System.Web;
using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.Domain;
using Kuyam.Utility;
using System.ComponentModel;
using OfficeOpenXml;
using System.Drawing;

namespace Kuyam.Domain
{
    public static class BusinessService
    {

        public static string GetGeoCoords(string address)
        {

            string[] Chunks = null;
            string outString = "";
            int i = 0;

            string xmlString = GetHTML("http://maps.google.com/maps/geo?output=xml&key=abcdefg&q=" + address, 1);
            Chunks = Regex.Split(xmlString, "coordinates>", RegexOptions.Multiline);
            if (Chunks.Count() > 1)
            {
                outString = Chunks[1].Replace(",0</", "");
            }
            else
            {
                outString = "0,0";
                while (i <= 5)
                {
                    if (Chunks.Count() > 1)
                    {
                        outString = Chunks[1].Replace(",0</", "");
                        break;
                    }
                    i++;
                }

            }
            return outString;
        }

        public static string GetHTML(string sURL, int e)
        {
            System.Net.HttpWebRequest oHttpWebRequest = null;
            System.IO.Stream oStream = null;
            string sChunk = null;
            oHttpWebRequest = (HttpWebRequest)System.Net.HttpWebRequest.Create(sURL);
            System.Net.WebResponse oHttpWebResponse = oHttpWebRequest.GetResponse();
            oStream = oHttpWebResponse.GetResponseStream();
            sChunk = new System.IO.StreamReader(oStream).ReadToEnd();
            oStream.Close();
            oHttpWebResponse.Close();

            if (e == 0)
            {
                return HttpContext.Current.Server.HtmlEncode(sChunk);
            }
            else
            {
                return HttpContext.Current.Server.HtmlDecode(sChunk);
            }
        }

        // caculator
        public static double DistanceInMeters(string fromAddress, string goalAddress)
        {
            double EARTH_RADIUS_IN_METERS = 6378137;
            //string coords1 = GetGeoCoords(fromAddress);
            //string coords2 = GetGeoCoords(goalAddress);
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 0;
            //GetLatandLon(coords1, out lat1, out lon1);
            //GetLatandLon(coords2, out lat2, out lon2);
            lat1 = GetCodeService.GetCoordinates(fromAddress, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
            lon1 = GetCodeService.GetCoordinates(fromAddress, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;
            lat2 = GetCodeService.GetCoordinates(goalAddress, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
            lon2 = GetCodeService.GetCoordinates(goalAddress, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;

            return EARTH_RADIUS_IN_METERS * ArcInRadians(lat1, lon1, lat2, lon2);
        }

        public static void GetLatandLon(string coord, out double latitude, out double longitude)
        {
            string[] temp = coord.Split(',');
            latitude = double.Parse(temp[1]);
            longitude = double.Parse(temp[0]);
        }


        public static double ArcInRadians(double lat1, double lon1, double lat2, double lon2)
        {
            //double DEG_TO_RAD = 0.0174532925;
            //double latitudeArc = (lat1 - lat2) * DEG_TO_RAD;
            //double longitudeArc = (lon1 - lon2) * DEG_TO_RAD;
            //double latitudeH = Math.Sin(latitudeArc * 0.5);
            //latitudeH *= latitudeH;
            //double lontitudeH = Math.Sin(longitudeArc * 0.5);
            //lontitudeH *= lontitudeH;
            //double tmp = Math.Cos(lat1 * DEG_TO_RAD) * Math.Cos(lat2 * DEG_TO_RAD);
            //return 2.0 * Math.Asin(Math.Sqrt(latitudeH + tmp * lontitudeH));
            double DEG_TO_RAD = 0.0174532925;
            double dLat = (lat2 - lat1) * DEG_TO_RAD;
            double dLon = (lon2 - lon1) * DEG_TO_RAD;
            lat1 = lat1 * DEG_TO_RAD;
            lat2 = lat2 * DEG_TO_RAD;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return c;
        }

        // end caculator distance between 2 location 

        // compare distance between from address to goal address         
        //public static bool FilterCompaniesByDistance(string fromAddress, string goalAddress, double compare)
        //{
        //    double distance = DistanceInMeters(fromAddress, goalAddress)/1000; 

        //    if (distance <= compare && distance!= 0)
        //        return true;
        //    return false;
        //}

        public static bool FilterCompaniesByDistance(double lat1, double lon1, double lat2, double lon2, double compare)
        {
            if (compare > 0)
            {
                double distance = LocatorHelper.CalculateDistance(lat1, lon1, lat2, lon2);//DistanceInMeters(lat1, lon1, lat2, lon2) / 1000;
                if (distance <= compare && distance != 0)
                    return true;
                return false;
            }
            else
            {
                return true;
            }

        }

        public static bool FilterCompaniesByPrice(int profileID, decimal priceFrom, decimal priceTo)
        {
            return DAL.isAvailableFromPriceToPrice(profileID, priceFrom, priceTo);
        }


        public static bool FilterCompaniesByIsToDay(int profileID)
        {
            return DAL.isAvailableToday(profileID);
        }

        public static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double EARTH_RADIUS_IN_METERS = 6378137;
            return EARTH_RADIUS_IN_METERS * ArcInRadians(lat1, lon1, lat2, lon2);
        }

        //public static void ConfirmCustomerSchedules(Guid sessionID)
        //{
        //    List<CustomerSchedule> csList = DAL.GetCustomerShedulesBySessionID(sessionID);
        //    foreach (CustomerSchedule cs in csList)
        //    {
        //        DAL.ConfirmCustomerSchedule(cs.CustomerScheduleID);
        //    }
        //}

        public static List<ProfileCompany> GetCompaniesFromTypeIDWithDistance(int typeId, double distance)
        {
            List<ProfileCompany> pcList = DAL.GetCompaniesFromTypeID(typeId);
            if (pcList != null)
            {
                List<ProfileCompany> resultList = new List<ProfileCompany>();
                foreach (ProfileCompany company in pcList)
                {
                    string address = company.Street1 + ' ' + company.Street2 + "+ " + company.City + "+ " + company.State;
                    if (HttpContext.Current.User.Identity.IsAuthenticated)
                    {
                        Cust cust = DAL.xGetCust(HttpContext.Current.User.Identity.Name);
                        if (BusinessService.FilterCompaniesByDistance(cust.Latitude, cust.Longitude, company.Latitude, company.Longitude, distance))
                            resultList.Add(company);
                    }
                    else
                    {
                        resultList = pcList;
                    }


                }
                return resultList;

            }
            return null;
        }

        public static List<CompanyProfileIPhone> SearchCompanyProfileIphonesWithKeyword(string key, int pageIndex, int itemPerPage, out int totalRecord)
        {
            try
            {
                List<CompanyProfileIPhone> pcList = new List<CompanyProfileIPhone>();
                pcList = DAL.GetCompanyProfileIphonesWithKeyword(key);
                totalRecord = pcList.Count;

                int skipItem = (pageIndex - 1) * itemPerPage;
                List<CompanyProfileIPhone> returnList = pcList.Skip(skipItem).Take(itemPerPage).ToList();
                foreach (CompanyProfileIPhone ip in returnList)
                {
                    ip.Rate = DAL.GetFavouriteStarForCompanyProfile(ip.ProfileID);
                    ip.ListServices = DAL.GetTypeNameFromProfileID(ip.ProfileID);
                    ip.ImageUrl = DAL.GetCompanyImagesPathbyProfileID(ip.ProfileID);
                    ip.isFeature = DAL.isFeatureCompany(ip.ProfileID);
                }
                return returnList;
            }
            catch (Exception ex)
            {
                totalRecord = 0;
                return null;
            }
        }

        public static List<CompanyProfileIPhone> SearchCompanyProfileIphonesWithKeyword(string key, int pageIndex, int itemPerPage, out int totalRecord, int custID)
        {
            try
            {
                List<CompanyProfileIPhone> pcList = new List<CompanyProfileIPhone>();
                pcList = DAL.GetCompanyProfileIphonesWithKeyword(key);
                totalRecord = pcList.Count;

                int skipItem = (pageIndex - 1) * itemPerPage;
                List<CompanyProfileIPhone> returnList = pcList.Skip(skipItem).Take(itemPerPage).ToList();
                foreach (CompanyProfileIPhone ip in returnList)
                {
                    ip.Rate = DAL.GetFavouriteStarForCompanyProfile(ip.ProfileID);
                    ip.ListServices = DAL.GetTypeNameFromProfileID(ip.ProfileID);
                    ip.ImageUrl = DAL.GetCompanyImagesPathbyProfileID(ip.ProfileID);
                    ip.isFeature = DAL.isFeatureCompany(ip.ProfileID);
                    if (custID > 0)
                    {
                        ip.IsUSerFavourite = DAL.isFavorite(custID, ip.ProfileID);
                    }
                }
                return returnList;
            }
            catch (Exception ex)
            {
                totalRecord = 0;
                return null;
            }
        }

        public static List<CompanyProfileIPhone> SearchCompanyProfileIphones(string key, int pageIndex, int itemPerPage, out int totalRecord, int custID, double latitude, double longitude, string city, double distance)
        {
            try
            {

                List<CompanyProfileIPhone> resultList = new List<CompanyProfileIPhone>();

                totalRecord = 0;
                int skipItem = (pageIndex - 1) * itemPerPage;

                List<string> zcList = new List<string>();
                zcList = DAL.GetListZipCodesFromCityName(city);


                if (latitude != 0.0 && longitude != 0.0 && distance > 0)
                {
                    var bound = LocatorHelper.GetBoundingBox(
                        new LocatorHelper.MapPoint()
                        {
                            Latitude = latitude,
                            Longitude = longitude
                        }, distance / 1000
                        );

                    resultList = DAL.GetCompanyProfileIphonesWithKeyword(key, bound.MinPoint.Latitude,
                                                                     bound.MinPoint.Longitude,
                                                                     bound.MaxPoint.Latitude, bound.MaxPoint.Longitude,
                                                                     skipItem, itemPerPage, out totalRecord);
                }
                else if (zcList != null && zcList.Count() > 0)
                {
                    resultList = DAL.GetCompanyProfileIphonesWithKeyword(key, zcList, skipItem, itemPerPage, out totalRecord);
                }
                else
                {
                    resultList = DAL.GetCompanyProfileIphonesWithKeyword(key, skipItem, itemPerPage, out totalRecord);
                }


                foreach (CompanyProfileIPhone ip in resultList)
                {
                    ip.Rate = DAL.GetFavouriteStarForCompanyProfile(ip.ProfileID);
                    ip.ListServices = DAL.GetTypeNameFromProfileID(ip.ProfileID);
                    ip.ImageUrl = DAL.GetCompanyImagesPathbyProfileID(ip.ProfileID);
                    ip.isFeature = DAL.isFeatureCompany(ip.ProfileID);
                    if (custID > 0)
                    {
                        ip.IsUSerFavourite = DAL.isFavorite(custID, ip.ProfileID);
                    }
                }
                return resultList;
            }
            catch (Exception ex)
            {
                totalRecord = 0;
                return null;
            }
        }

        #region SearchOptionalParams

        public static List<CompanyProfileIPhone> SearchCompanyProfileOptionalParams(string key, int categoryId, int custID, double lat, double lon, string pointRect, double distance, int pageIndex, int itemPerPage, out int totalRecord)
        {
            try
            {
                List<CompanyProfileIPhone> resultList = new List<CompanyProfileIPhone>();
                totalRecord = 0;
                int skipItem = (pageIndex - 1) * itemPerPage;
                if (pageIndex == -1)
                {
                    skipItem = 0;
                    itemPerPage = 100;
                }
                bool isSearchArea = false;
                var bound = new LocatorHelper.BoundingBox();

                if (lat == 0 && lon == 0)
                {
                    double custlat = 0.0;
                    double custlon = 0.0;

                    //check user registered Santa Monica
                    var cust = DAL.xGetCust(custID);
                    if (cust != null)
                    {
                        GeoClass.Coordinate coordinate;
                        string strAddress = string.Empty;
                        if (!string.IsNullOrEmpty(cust.Street1))
                        {
                            strAddress += cust.Street1;
                        }
                        if (!string.IsNullOrEmpty(cust.Street2))
                        {
                            if (strAddress != string.Empty)
                                strAddress += ",";
                            strAddress += cust.Street2;
                        }

                        if (!string.IsNullOrEmpty(cust.City))
                        {
                            if (strAddress != string.Empty)
                                strAddress += ",";
                            strAddress += cust.City;
                        }

                        if (!string.IsNullOrEmpty(cust.Zip))
                        {
                            if (strAddress != string.Empty)
                                strAddress += " ";
                            strAddress += cust.Zip;
                        }

                        coordinate = GeoClass.GetCoordinates(strAddress);
                        custlat = (double)coordinate.Latitude;
                        custlon = (double)coordinate.Longitude;
                    }

                    bound = LocatorHelper.GetBoundingBox(
                                new LocatorHelper.MapPoint()
                                {
                                    Latitude = custlat,
                                    Longitude = custlon
                                }, distance
                                );
                    lat = custlat;
                    lon = custlon;
                }
                else
                {
                    bound = LocatorHelper.GetBoundingBox(
                                new LocatorHelper.MapPoint()
                                {
                                    Latitude = lat,
                                    Longitude = lon
                                }, distance
                                );

                }

                if (!string.IsNullOrWhiteSpace(pointRect))
                {
                    isSearchArea = true;
                    string[] coor = pointRect.Split(';');

                    var polygon = string.Format("polygon(({0} {1},{2} {3},{4} {5},{6} {7},{0} {1}))",
                        coor[0], coor[1], coor[2], coor[3], coor[4], coor[5], coor[6], coor[7], coor[0], coor[1]);

                    var point = DAL.GetMapPointMinMax(polygon);

                    //bound = new LocatorHelper.BoundingBox(
                    //   new LocatorHelper.MapPoint()
                    //   {
                    //       Latitude = point.Minx,
                    //       Longitude = point.Miny
                    //   }, new LocatorHelper.MapPoint()
                    //   {
                    //       Latitude = point.Maxx,
                    //       Longitude = point.Maxy
                    //   }
                    //); 
                    /*
                    double lat1 = 0;
                    double lon1 = 0;
                    double lat2 = 0;
                    double lon2 = 0;
                    double.TryParse(coor[0], out lon1);
                    double.TryParse(coor[1], out lat1);
                    double.TryParse(coor[4], out lon2);
                    double.TryParse(coor[5], out lat2);

                    lon = (lon1 + lon2) / 2;
                    lat = (lat1 + lat2) / 2;

                    distance = LocatorHelper.CalculateDistance(lat, lon, lat1, lon1);
                    bound = LocatorHelper.GetBoundingBox(
                                new LocatorHelper.MapPoint()
                                {
                                    Latitude = lat,
                                    Longitude = lon
                                }, distance
                                );

                    */
                    bound = new LocatorHelper.BoundingBox(new LocatorHelper.MapPoint()
                    {
                        Latitude = point.Miny,
                        Longitude = point.Minx
                    }, new LocatorHelper.MapPoint()
                    {
                        Latitude = point.Maxy,
                        Longitude = point.Maxx
                    });

                }


                resultList = DAL.GetCompanyProfileSearchOptionalParams(custID, key, categoryId,
                                                                        skipItem, itemPerPage, out totalRecord,
                                                                        lat,
                                                                        lon,
                                                                        bound.MinPoint.Latitude,
                                                                        bound.MinPoint.Longitude,
                                                                        bound.MaxPoint.Latitude,
                                                                        bound.MaxPoint.Longitude, isSearchArea);

                //foreach (CompanyProfileIPhone ip in resultList)
                //{
                //    ip.Rate = DAL.GetFavouriteStarForCompanyProfile(ip.ProfileID);
                //    ip.ListServices = DAL.GetTypeNameFromProfileID(ip.ProfileID);
                //    ip.ImageUrl = DAL.GetCompanyImagesPathbyProfileID(ip.ProfileID);
                //    ip.isFeature = DAL.isFeatureCompany(ip.ProfileID);
                //    if (custID > 0)
                //    {
                //        ip.IsUSerFavourite = DAL.isFavorite(custID, ip.ProfileID);
                //    }
                //}

                return resultList;
            }
            catch (Exception ex)
            {
                totalRecord = 0;
                return null;
            }
        }


        #endregion

        public static List<CompanyProfileIPhone> SearchCompanyProfileIphonesWithCategoryID(int categoryID, int pageIndex, int itemPerPage, out int totalRecord, int custID)
        {
            try
            {
                List<CompanyProfileIPhone> pcList = new List<CompanyProfileIPhone>();
                int skipItem = (pageIndex - 1) * itemPerPage;
                pcList = DAL.GetCompanyProfileIphonesWithCategoryID(categoryID, skipItem, itemPerPage, out totalRecord);


                List<CompanyProfileIPhone> returnList = pcList.Skip(skipItem).Take(itemPerPage).ToList();
                foreach (CompanyProfileIPhone ip in returnList)
                {
                    ip.Rate = DAL.GetFavouriteStarForCompanyProfile(ip.ProfileID);
                    ip.ListServices = DAL.GetTypeNameFromProfileID(ip.ProfileID);
                    ip.ImageUrl = DAL.GetCompanyImagesPathbyProfileID(ip.ProfileID);
                    ip.isFeature = DAL.isFeatureCompany(ip.ProfileID);
                    if (custID > 0)
                    {
                        ip.IsUSerFavourite = DAL.isFavorite(custID, ip.ProfileID);
                    }
                }
                return returnList;
            }
            catch (Exception ex)
            {
                totalRecord = 0;
                return null;
            }
        }

        public static List<ProfileCompany> GetCompaniesFromTypeIDWithDistance(int serviceId, double distance, decimal priceFrom, decimal priceTo, DateTime hourFrom, DateTime hourTo, bool isToday, int page, int sortBy, out int totalRecord, string key)
        {

            List<ProfileCompany> pcList = new List<ProfileCompany>();
            if (sortBy == (int)Types.SortBy.CompanyName)
            {
                pcList = DAL.GetCompanies(serviceId, priceFrom, priceTo, hourFrom, hourTo, isToday, sortBy, key);
            }
            else
            {
                pcList = DAL.GetListCompanyIDs(serviceId, priceFrom, priceTo, hourFrom, hourTo, isToday, key);

            }

            if (pcList != null)
            {
                List<ProfileCompany> resultList = new List<ProfileCompany>();

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    Cust cust = DAL.xGetCust(HttpContext.Current.User.Identity.Name);
                    foreach (ProfileCompany pc in pcList)
                    {
                        pc.Distance = LocatorHelper.CalculateDistance(cust.Latitude, cust.Longitude, pc.Latitude, pc.Longitude);
                    }
                }
                else
                {
                    double defaultLatitude = ConfigManager.DefaultLatitude;
                    double defaultLongitude = ConfigManager.Defaultlongitude;
                    foreach (ProfileCompany pc in pcList)
                    {
                        pc.Distance = LocatorHelper.CalculateDistance(defaultLatitude, defaultLongitude, pc.Latitude, pc.Longitude);
                    }
                }

                pcList = pcList.OrderBy(pc => pc.Distance).ToList();

                resultList = distance > 0 ? pcList.Where(pc => pc.Distance <= distance).ToList() : pcList;

                var featureCompanies = DAL.GetFeaturedCompanies().Select(c => c.ProfileID);
                var topResults = resultList.Where(c => featureCompanies.Contains(c.ProfileID));
                var bottomResults = resultList.Where(c => !featureCompanies.Contains(c.ProfileID));


                if (resultList != null && resultList.Count() > 100)
                {
                    totalRecord = 100;
                    resultList = topResults.Union(bottomResults).Skip(0).Take(100).ToList();
                }
                else
                {
                    resultList = topResults.Union(bottomResults).ToList();
                    totalRecord = resultList.Count;
                }
                return resultList.Skip((page - 1) * 10).Take(10).ToList();

            }
            totalRecord = 0;
            return null;
        }

        public static void UpdateCustLatAndLon(int typeID)
        {
            List<ProfileCompany> pcList = DAL.GetProfileCompanies(typeID);
            foreach (ProfileCompany company in pcList)
            {
                string url = company.Street1 + ' ' + company.Street2 + ' ' + company.City + ' ' + company.State;
                if (url.Trim() != string.Empty)
                {
                    double lat = Kuyam.Domain.GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
                    double lon = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;
                    while (lat == 0 || lon == 0)
                    {
                        lat = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
                        lon = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;
                    }
                    DAL.updateCompanyLatandLong(company.ProfileID, lat, lon);
                }
            }
        }

        public static void GetLatAndLonByAreaCode(int areaCode, out double lat, out double lon)
        {
            lat = 0.0;
            lon = 0.0;
            string url = areaCode.ToString();
            if (url.Trim() != string.Empty)
            {
                lat = Kuyam.Domain.GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
                lon = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;
                while (lat == 0 || lon == 0)
                {
                    lat = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Latitude;
                    lon = GetCodeService.GetCoordinates(url, "AIzaSyCxHakIap5-WTcE27Tmu1qSHMwifluRhwY").Longitude;
                }

            }
        }

        public static string GetEventCalendar(List<EmployeeHour> employeeHourList)
        {
            string stringEvent = string.Empty;
            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            int dayOfWeek = (int)dtnow.DayOfWeek;
            int detDay = 7 - dayOfWeek;
            int day = 0;
            bool flag = false;
            employeeHourList = employeeHourList.OrderBy(m => m.FromHour).ToList();
            foreach (EmployeeHour item in employeeHourList)
            {
                if (item.DayOfWeek >= dayOfWeek)
                {
                    day = item.DayOfWeek - dayOfWeek;
                }
                else
                {
                    day = item.DayOfWeek + detDay;
                }

                int fromHour = item.FromHour.Hours;
                int fromminute = item.FromHour.Minutes;

                //if (!flag && item.DayOfWeek == dayOfWeek)
                //{
                //    if (dtnow.Hour >= item.FromHour.Hours)
                //    {
                //        fromHour = dtnow.Hour;
                //        fromminute = dtnow.Minute;
                //    }

                //    flag = true;
                //}

                string temp = "{title:" + '\'' + ' ' + '\'' + ',';
                temp += "id:" + item.ID.ToString() + ',';
                temp += "start: new Date(y, m, d + " + day.ToString() + ',' + fromHour + "," + fromminute + "),";
                temp += "end: new Date(y,m, d + " + day.ToString() + ',' + item.ToHour.Hours + "," + item.ToHour.Minutes + "),";
                if (item.IsPreview)
                    temp += "className:\"fc-preview\",";
                temp += "allDay: false},";
                stringEvent = stringEvent + temp;


                /* for (int i = 0; i < tempDayOfWeek.Length; i++)
                 {
                     if ((int.Parse(tempDayOfWeek[i].ToString()) % 7) >= subDay)
                     {
                         if (!eh.IsDaily)
                         {
                             int addDay = (int.Parse(tempDayOfWeek[i].ToString()) % 7) - subDay;
                             string temp = "{title:" + '\'' + ' ' + '\'' + ',';
                             temp += "id:" + eh.ID.ToString() + ',';
                             temp += "start: new Date(y, m, d + " + addDay.ToString() + ',' + eh.FromHour.Hours + ", 0),";
                             temp += "end: new Date(y,m, d + " + addDay.ToString() + ',' + eh.ToHour.Hours + ", 0),";
                             temp += "allDay: false},";
                             stringEvent = stringEvent + temp;
                         }
                         else
                         {
                             int addDay = (int.Parse(tempDayOfWeek[i].ToString()) % 7) - subDay;
                             string temp = "{title:" + '\'' + "preview" + '\'' + ',';
                             temp += "id:" + eh.ID.ToString() + ',';
                             temp += "start: new Date(y, m, d + " + addDay.ToString() + ',' + eh.FromHour.Hours + ", 0),";
                             temp += "end: new Date(y,m, d + " + addDay.ToString() + ',' + eh.ToHour.Hours + ", 0),";
                             temp += "className:\"fc-preview\",";
                             temp += "allDay: false},";
                             stringEvent = stringEvent + temp;
                         }
                     }
                 }
                 */

            }
            return stringEvent;
        }

        //Get class list hours is set to instructor
        public static string GetListClassHourCalendar(List<EmployeeHour> employeeHourList)
        {
            string stringEvent = string.Empty;
            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            int dayOfWeek = (int)dtnow.DayOfWeek;
            int detDay = 7 - dayOfWeek;
            int day = 0;
            bool flag = false;
            employeeHourList = employeeHourList.OrderBy(m => m.FromHour).ToList();
            foreach (EmployeeHour item in employeeHourList)
            {
                if (item.DayOfWeek >= dayOfWeek)
                {
                    day = item.DayOfWeek - dayOfWeek;
                }
                else
                {
                    day = item.DayOfWeek + detDay;
                }

                int fromHour = item.FromHour.Hours;
                int fromminute = item.FromHour.Minutes;



                string temp = "{title:" + '\'' + ' ' + '\'' + ',';
                temp += "id:" + item.ID + ',';
                temp += "start: new Date(y, m, d + " + day + ',' + fromHour + "," + fromminute + "),";
                temp += "end: new Date(y,m, d + " + day + ',' + item.ToHour.Hours + "," + item.ToHour.Minutes + "),";
                temp += "className:\"fc-classHour\",";
                temp += "allDay: false},";
                stringEvent = stringEvent + temp;
                

            }
            return stringEvent;
        }

        /// <summary>
        /// Gets the available hours of company to display on background.
        /// </summary>
        /// <param name="companyProfileId">The company profile id.</param>
        /// <returns></returns>
        public static string GetCompanyHoursJson(int companyProfileId)
        {
            string result = string.Empty;

            DateTime dtnow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            int dayOfWeek = (int)dtnow.DayOfWeek;

            List<CompanyHour> companyHours = DAL.GetCompanyHourList(companyProfileId);
            foreach (CompanyHour companyHour in companyHours)
            {
                string dayOfWeekStr = companyHour.DayOfWeek.ToString();
                foreach (char d in dayOfWeekStr)
                {
                    int day = Convert.ToInt16(d.ToString());
                    day = day >= dayOfWeek ? day - dayOfWeek : 7 - dayOfWeek + day;


                    string temp = "{start: new Date(y, m, d + " + day.ToString() + ',' + companyHour.FromHour.Hours + "," + companyHour.ToHour.Minutes + "),";
                    temp += "end: new Date(y,m, d + " + day.ToString() + ',' + companyHour.ToHour.Hours + "," + companyHour.ToHour.Minutes + "),";
                    temp += "background: '#ffffff'},";
                    result += temp;
                }

            }

            return result;
        }

        //export to excel file 
        public static void GenerateInvoicesListasExcel(IList<CompanyInvoices> dataToExcel, string downloadedFileDName)
        {

            //Step 1 : Create object of ExcelPackage class and pass file path to constructor.
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(downloadedFileDName);
                worksheet.Column(1).Style.Numberformat.Format = "MM/dd/yyyy hh:mm";
                worksheet.Cells["A1"].LoadFromCollection<CompanyInvoices>(dataToExcel, true);


                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.AddHeader("Content-disposition", "attachment; filename=" + downloadedFileDName);
                HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.BinaryWrite(package.GetAsByteArray());
                HttpContext.Current.Response.End();
            }
        }


    }
}
