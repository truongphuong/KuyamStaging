using System;
using System.Collections.Generic;

namespace Kuyam.Database
{
    public class CompanyProfileSearch
    {        
        public CompanyProfileSearch()
        {
            ListsCategoriesds = new List<string>();
        }
        public int IndexId { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ProfileID { get; set; }
        public int CompanyTypeID { get; set; }
        public int CompanyStatusID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string YoutubeLink { get; set; }
        public Nullable<int> PreferredContact { get; set; }
        public Nullable<int> FirstAlert { get; set; }
        public Nullable<int> SecondAlert { get; set; }
        public string PaymentOptions { get; set; }
        public Nullable<int> PaymentMethod { get; set; }
        public Nullable<int> PayAfter { get; set; }
        public string MapUrl { get; set; }
        public string Neighborhood { get; set; }
        public string Notes { get; set; }
        public bool ApptAutoConfirm { get; set; }
        public int ApptDefaultSlotDuration { get; set; }
        public int ApptDefaultPeoplePerSlot { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Nullable<System.DateTime> ExpiredDate { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public Nullable<int> CancelPolicy { get; set; }
        public Nullable<int> CancelHour { get; set; }
        public Nullable<int> CancelRefundPercent { get; set; }
        public string MobileCarrier { get; set; }
        public Nullable<bool> IsShowCatagory { get; set; }
        public Nullable<bool> IsEvent { get; set; }
        public Nullable<bool> IsClass { get; set; }
        public bool IsFeature { get; set; }
        public double Distance { get; set; }
        public bool IsUserFavorite { get; set; }      
        public string ListServiceIds { get; set; }
        public List<string>  ListsCategoriesds { get; set; }   
        public string ListServices { get; set; }
        public bool IsVideo { get; set; }
        public int Rate { get; set; }
        public int TotalReviews { get; set; }

        public string LogoMediaId { get; set; }

        public List<CompanyMedia> CompanyMedias { get; set; }
        /*
        public List<CompanyMedia> _companyMedias { get; set; }

        public List<CompanyMedia> CompanyMedias
        {
            get
            {
                if (_companyMedias == null) _companyMedias = new List<CompanyMedia>();
                return _companyMedias;
            }
            set
            {
                _companyMedias = value;
            }
        }
        */

        public bool HasClassBooking
        {
            get
            {
                return (this.IsClass.HasValue && this.IsClass.Value
                    && this.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt);
            }
        }

        public List<ServiceCompany> _serviceCompanies;

        public List<ServiceCompany> ServiceCompanies
        {
            get
            {
                if (_serviceCompanies == null) _serviceCompanies = new List<ServiceCompany>();
                return _serviceCompanies;
            }
            set
            {
                _serviceCompanies = value;
            }
        }


        public List<Appointment> Appointments { get; set; }

        public TimeSlots CompanyAvailableTimeSlots { get; set; }

        public string EmployeeHoursStr { get; set; }

        public List<EmployeeHour> EmployeeHours { get; set; }

        public string CompanyHoursStr { get; set; }

        public List<CompanyHour> CompanyHours { get; set; }

        public string CompanyEventsStr { get; set; }

        public List<EventDTO> CompanyEvents { get; set; }

        public string InstructorClassSchedulerStr { get; set; }

        public List<EmployeeHour> InstructorClassSchedulerHours { get; set; }
        
    }
}
