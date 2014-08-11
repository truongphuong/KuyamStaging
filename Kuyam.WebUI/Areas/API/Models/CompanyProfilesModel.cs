using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class CompanyProfilesModel
    {
        public int ProfileID { get; set; }
        public int CompanyTypeID { get; set; }
        public int CompanyStatusID { get; set; }
        public string Name { get; set; }
        public string SlugName {get;set; }
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
        public int? PreferredContact { get; set; }
        public int? FirstAlert { get; set; }
        public int? SecondAlert { get; set; }
        public string PaymentOptions { get; set; }
        public int? PaymentMethod { get; set; }
        public int? PayAfter { get; set; }
        public string MapUrl { get; set; }
        public string Neighborhood { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public int? CancelPolicy { get; set; }
        public int? CancelHour { get; set; }
        public int? CancelRefundPercent { get; set; }
        public string MobileCarrier { get; set; }
        public bool? IsShowCatagory { get; set; }
        public bool? IsEvent { get; set; }
        public bool? IsClass { get; set; }
        public bool IsFeature { get; set; }
        public string ListServices { get; set; }
        public bool IsVideo { get; set; }
        public int Rate { get; set; }
        public int TotalReviews { get; set; }
        public string LogoMediaId { get; set; }
        public List<CompanyMediaDTO> CompanyMedias { get; set; }

        public TimeSlotsDTO AvailableTimeSlots { get; set; }

        public List<EmployeeHourDTO> EmployeeHours { get; set; }

        public List<CompanyHourDTO> CompanyHours { get; set; }

        public List<EventDTO> CompanyEvents { get; set; }

        public List<ServiceTimeDTO> CompanyGenreralTimes { get; set; }

        public List<EmployeeHourDTO> ClassSchedulerHours { get; set; }
    }
}