using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database.Extensions;


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

        public Profile GetProfile()
        {
            return DAL.GetProfile(this.ProfileID);
        }

        public Calendar GetDefaultCalendar()
        {
            return DAL.GetCalendarsForProfile(ProfileID).FirstOrDefault();
        }

        public static void DeleteOpenHour(int id)
        {
            DAL.DeleteOpenHour(id);
        }

        public static void DeleteExceptionHour(int id)
        {
            DAL.DeleteExceptionHour(id);
        }
        //---------------------------Trong Edit---------------------------------
        public static CompanyProfile GetCompanyProfile(int profileID)
        {
            return DAL.GetCompanyProfile(profileID);
        }

        public static List<CompanyHour> GetCompanyHourList(int profileID)
        {

            return DAL.GetCompanyHourList(profileID);
        }

        public static CompanyService GetServiceByServiceID(int serviceID)
        {

            return DAL.GetServiceByServiceID(serviceID);
        }

        public static List<CompanyEmployee> GetEmployeesList(int serviceID)
        {
            return DAL.GetEmployeesList(serviceID);
        }

        public static List<ServiceHour> GetServiceHourList(int serviceId, int employeeID)
        {
            return DAL.GetServicesHourListByServiceIDAndEmployeeID(serviceId, employeeID);
        }

        public static List<ServiceHour> GetServiceHourListByCompanyProfileId(int profileCompanyId)
        {
            return DAL.GetServiceHourListByCompanyProfileId(profileCompanyId);
        }


        public static List<ProfileCompany> GetFavoriteListByCustID(int custID)
        {
            return DAL.GetFavoriteListByCustID(custID);
        }

        public static bool AddToFavorite(Favorite fav)
        {

            return DAL.AddFavorite(fav);
        }

        public static bool RemoveFavorite(Favorite fav)
        {

            return DAL.DeleteFavorite(fav);
        }

        public static bool CheckFavoriteByProfileID(int profileID, int custID)
        {
            return DAL.CheckFavoriteByProfileID(profileID, custID);
        }
        public static List<ServiceHour> GetServiceHoursByCompanyProfile(int profileID)
        {
            return DAL.GetServiceHoursByCompanyProfile(profileID);
        }
        public static List<ProfileCompany> GetListCompanyAll()
        {
            return DAL.GetListCompanyAll();
        }
        //-----------------------------xXx--------------------------------------

        public static ServiceCompany GetFirstServiceByCompanyId(int companyId)
        {
            return DAL.GetFirstServiceByCompanyId(companyId);
        }

        public static List<ServiceCompany> GetListServiceByCompanyId(int companyId)
        {
            return DAL.GetCompanySevicesByProfileID(companyId);
        }

        public List<ServiceCompany> GetCompanySevicesByCompanyId(int companyId)
        {
            return DAL.GetCompanySevicesByProfileID(companyId);
        }


        public static List<EmployeeHour> GetEmployeeHour(int profileId)
        {
            return DAL.GetEmployeeHour(profileId);
        }

        public static List<EmployeeHour> GetEmployeeHour(int profileId, int employeeId, int serviceId)
        {
            return DAL.GetEmployeeHour(profileId, employeeId, serviceId);
        }

        public static ProfileCompany GetProfileCompany(int profileId)
        {
            return DAL.GetProfileCompany(profileId);
        }

        public static List<Medium> GetMediaCompany(int profileId, Types.CompanyMediaType mediatype)
        {
            if (mediatype == Types.CompanyMediaType.IsBanner)
            {
                return DAL.GetListPhotoByCompanyID(profileId);
            }
            else if (mediatype == Types.CompanyMediaType.IsLogo)
            {
                return DAL.GetListLogoByCompanyID(profileId);
            }
            else if (mediatype == Types.CompanyMediaType.IsVideo)
            {
                return DAL.GetListVideByCompanyID(profileId);
            }
            return null;
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


        public static void UpdateAppointment(Appointment _appointment)
        {
            DAL.UpdateAppointment(_appointment);
        }

        public static void AddAppointmentTemp(AppointmentTemp appointmentTemp)
        {
            DAL.AddAppointmentTemp(appointmentTemp);
        }

        public static void DeleteAppointmentTemp(int _appointmentTempId)
        {
            DAL.DeleteAppointmentTemp(_appointmentTempId);
        }

        public static List<KuyamEvent> GetKuyamEvents(int custId)
        {
            return DAL.GetAvailableKuyamEvents(custId);
        }

        public static List<Service> GetServicebyEmployeeId(int employeeId)
        {
            return DAL.GetServicebyEmployeeId(employeeId);
        }

        public static List<CompanyService> GetServiceCompanybyEmployeeId(int profileId, int employeeId, int serviceId = 0, int cateloryId = 0)
        {
            return DAL.GetServiceCompanybyEmployeeId(profileId, employeeId, serviceId, cateloryId);
        }

        public static List<CompanyService> GetGeneralServiceCompanybyEmployeeId(int employeeId, int profileId = 0, int cateloryId = 0)
        {
            return DAL.GetGeneralServiceCompanybyEmployeeId(employeeId, profileId, cateloryId);
        }


        public static List<CompanyService> GetServiceEmployeeByPackage(int employeeId, int packageId, int profileId, int cateloryId = 0)
        {
            return DAL.GetServiceEmployeeByPackage(employeeId, packageId, profileId, cateloryId);
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

        public static bool RemoveFromFavorite(int companyProfileId, int custId)
        {
            return DAL.RemoveFromFavorite(companyProfileId, custId);
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

        public static List<CompanyPackage> GetCompanyPackages(int profileId)
        {
            return DAL.GetActiveCompanyPackages(profileId);
        }

        public static List<CompanyEmployee> GetEmployeeListByProfileCompanyId(int profileCompanyId)
        {
            return DAL.GetEmployeesListByProfileCompanyID(profileCompanyId);
        }

        public static List<CompanyEmployee> GetActiveEmployeeListByProfileCompanyId(int profileCompanyId)
        {
            return DAL.GetEmployeesListByProfileCompanyID(profileCompanyId).Where(e => e.EmployeeServices.Any() && e.EmployeeHours.Any()).ToList();
        }

        public static List<Service> GetServiceListByProfileCompanyId(int profileCompanyId)
        {
            return DAL.GetServiceListByProfileCompanyId(profileCompanyId);
        }

        public static TimeSpan GetDurationAvailableOfEmployee(DateTime startTime, int employeeId)
        {
            return DAL.GetDurationAvailableOfEmployee(startTime, employeeId);
        }

        public string Address { get { return string.Format("{0}. {1},{2} {3}", Street1, City, State, Zip); } }

        public string CityAndState
        {
            get
            {
                if (!string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State))
                {
                    return string.Format("{0}, {1}", City, State);
                }
                else
                {
                    return string.Format("{0}{1}", City, State);
                }
            }
        }

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
