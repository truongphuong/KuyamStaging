using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database.Extensions;
using Kuyam.Utility;


namespace Kuyam.Database
{
    public partial class ProfileCompany
    {
        private double _rate = -1;
        private int _totalReview = -1;

        public double? Distance { get; set; }

        public bool HasClassBooking
        {
            get
            {
                return (this.IsClass.HasValue && this.IsClass.Value
                    && this.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt);
            }
        }
        public double Rate
        {
            get
            {
                if (_rate < 0)
                    CalculatorRating();
                return _rate;
            }
            set { _rate = value; }
        }
        
        public int TotalReview
        {
            get
            {
                if (_totalReview < 0)
                    CalculatorRating();
                return _totalReview;
            }
            set { _totalReview = value; }
        }

        public static CompanyProfile GetCompanyProfile(int profileID)
        {
            return DAL.GetCompanyProfile(profileID);
        }

        public static CompanyService GetServiceByServiceID(int serviceID)
        {

            return DAL.GetServiceByServiceID(serviceID);
        }

        public static List<CompanyEmployee> GetEmployeesList(int serviceID)
        {
            return DAL.GetEmployeesList(serviceID);
        }

        public static List<ProfileCompany> GetFavoriteListByCustID(int custID)
        {
            return DAL.GetFavoriteListByCustID(custID);
        }

        public static bool AddToFavorite(Favorite fav)
        {

            return DAL.AddFavorite(fav);
        }

        public static ProfileCompany GetProfileCompany(int profileId)
        {
            return DAL.GetProfileCompany(profileId);
        }

        public static ServiceCompany GetServiceByID(int serviceId)
        {
            return DAL.GetServiceByID(serviceId);
        }

        public static CompanyEmployee GetCompanyEmployee(int employeeId)
        {
            return DAL.GetCompanyEmployee(employeeId);
        }

        public static void AddAppointment(Appointment _appointment, int calendarId)
        {
            DAL.AddAppointment(_appointment, calendarId);
        }

        public static void DeleteAppointmentTemp(int _appointmentTempId)
        {
            DAL.DeleteAppointmentTemp(_appointmentTempId);
        }

        public static List<KuyamEvent> GetKuyamEvents(int custId)
        {
            return DAL.GetAvailableKuyamEvents(custId);
        }

        public static List<CompanyService> GetServiceCompanybyEmployeeId(int profileId, int employeeId, int serviceId = 0, int cateloryId = 0)
        {
            return DAL.GetServiceCompanybyEmployeeId(profileId, employeeId, serviceId, cateloryId);
        }

        public static string GetCompanyMediaURL(int companyId, Types.CompanyMediaType companyMediaType, out string kalturaId)
        {
            string str = DAL.GetCompanyMediaURL(companyId, companyMediaType);
            kalturaId = str;
            //if (companyMediaType == Types.CompanyMediaType.IsVideo)
            //{
            //    str = KalturaHelper.GetEmbedVideo(str, "Company Video");
            //}
            return str;
        }

        public static List<CompanyEmployee> GetEmployeeByProfileId(int profileId)
        {
            return DAL.GetEmployeeByProfileId(profileId);
        }

        public static Appointment GetAppointmentById(int Id)
        {
            return DAL.GetAppointmentById(Id);
        }

        public static List<Appointment> GetAppointmentByProfileId(int profileId)
        {
            return DAL.GetAppointmentByProfileId(profileId);
        }

        public static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId)
        {
            return DAL.GetAppointmentByProfileIdV2(profileId);
        }

        public static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId, int employeeId)
        {
            return DAL.GetAppointmentByProfileIdV2(profileId, employeeId);
        }

        public static List<Appointment> GetAppointmentByProfileId(int profileId, int employeeId)
        {
            return DAL.GetAppointmentByProfileId(profileId, employeeId);
        }

        public static void CancelAppointment(int appId, string reason, bool sendNotification = false)
        {
            DAL.CancelAppointment(appId, reason);
        }

        public static List<Appointment> GetKuyamEventsByEmployeeId(int employeeId, DateTime start, DateTime end)
        {
            return DAL.GetKuyamEventsByEmployeeId(employeeId, start, end);
        }

        public static void RemoveAppointment(int appId, bool sendNotification = false)
        {
            DAL.RemoveAppointment(appId);
        }

        public static void Confirm(int appId, bool sendNotification = false)
        {
            DAL.Confirm(appId);
        }

        public static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId, List<int> lstStatus)
        {
            return DAL.GetAppointmentByProfileIdV2(profileId, lstStatus);
        }

        public static List<Appointment> GetAppointmentByProfileId(int profileId, List<int> lstStatus)
        {
            return DAL.GetAppointmentByProfileId(profileId, lstStatus);
        }

        public static List<CompanyEmployee> GetActiveEmployeeListByProfileCompanyId(int profileCompanyId)
        {
            return DAL.GetEmployeesListByProfileCompanyID(profileCompanyId).Where(e => e.EmployeeServices.Any() && e.EmployeeHours.Any()).ToList();
        }

        public static List<Service> GetServiceListByProfileCompanyId(int profileCompanyId)
        {
            return DAL.GetServiceListByProfileCompanyId(profileCompanyId);
        }

        public string Address { get { return string.Format("{0}. {1},{2} {3}", Street1, City, State, Zip); } }

        public void CalculatorRating()
        {
            _totalReview = 0;
            _rate = 0;
            //ServiceCompanies = GetListServiceByCompanyId(this.ProfileID);
            if (ServiceCompanies == null)
                return;

            double valueRanting = 0;
            foreach (ServiceCompany item in ServiceCompanies)
            {
                if (item.Ratings != null)
                {
                    _totalReview += item.Ratings.Count;
                    valueRanting += item.Ratings.Sum(m => m.RatingValue.HasValue ? m.RatingValue.Value : 0);
                }
            }
            if (_totalReview > 0)
            {
                _rate = Math.Round(valueRanting / _totalReview);
            }
        }

    }
}
