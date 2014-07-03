using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class CompanyProfileIPhone
    {
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
        public string PaymentOptions { get; set; }
        public Nullable<int> PaymentMethod { get; set; }
        public Nullable<int> PayAfter { get; set; }
        public string MapUrl { get; set; }
        public string PublicTransportation { get; set; }
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
        public decimal Rate { get; set; }

        public string EmployeeHoursStr { get; set; }
        public List<EmployeeHour> EmployeeHours { get; set; }

        public string CompanyHoursStr { get; set; }
        public List<CompanyHour> CompanyHours { get; set; }

        private List<string> _imageUrl;

        public List<string> ImageUrl
        {
            get
            {
                if (_imageUrl == null)
                {
                    if (!string.IsNullOrWhiteSpace(ImageUrlStr))
                    {
                        _imageUrl = ImageUrlStr.Split(',').ToList();
                        return _imageUrl;
                    }
                }

                return _imageUrl;
            }
            set
            {
                _imageUrl = value;
            }
        }
        public string ImageUrlStr { get; set; }
        public string ListServices { get; set; }
        public bool IsUSerFavourite { get; set; }
        public bool isFeature { get; set; }
        public List<WorkingHour> BusinessHours { get; set; }
        public List<PackageIP> Packages { get; set; }
        public List<CompanyReview> Reviews { get; set; }
        public List<AvaibilitiyCalendartIPhone> AvailableTimeSlot { get; set; }
        public double Distance { get; set; }
        public int NumberNotesUnread { get; set; }

        private CancellationPolicy cancellationPolicy { get; set; }

        public CancellationPolicy CancellationPolicy
        {
            get
            {
                if (cancellationPolicy == null) cancellationPolicy = new CancellationPolicy(CancelPolicy, CancelHour, CancelRefundPercent);
                return cancellationPolicy;
            }
            set
            {
                cancellationPolicy = value;
            }
        }
        public bool? IsShowCatagory { get; set; }
        public CompanyProfileIPhone()
        {
            AvailableTimeSlot = new List<AvaibilitiyCalendartIPhone>();
            BusinessHours = new List<WorkingHour>();
            Packages = new List<PackageIP>();
            Reviews = new List<CompanyReview>();
        }

        public CompanyProfileIPhone(ProfileCompany pc, double currentLat = 0, double currentLon = 0, int custId = 0):this()
        {
            ProfileID = pc.ProfileID;
            CompanyTypeID = pc.CompanyTypeID;
            CompanyStatusID = pc.CompanyStatusID;
            Name = pc.Name;
            Desc = pc.Desc;
            ContactName = pc.ContactName;
            ContactTitle = pc.ContactTitle;
            Street1 = pc.Street1;
            Street2 = pc.Street2;
            City = pc.City;
            State = pc.State;
            Zip = pc.Zip;
            Phone = pc.Phone;
            Fax = pc.Fax;
            Email = pc.Email;
            Url = pc.Url;
            YoutubeLink = pc.YoutubeLink;
            PreferredContact = pc.PreferredContact;
            PaymentOptions = pc.PaymentOptions;
            PaymentMethod = pc.PaymentMethod;
            PayAfter = pc.PayAfter;
            MapUrl = pc.MapUrl;
            PublicTransportation = pc.PublicTransportation;
            Notes = pc.Notes;
            ApptAutoConfirm = pc.ApptAutoConfirm;
            ApptDefaultSlotDuration = pc.ApptDefaultSlotDuration;
            ApptDefaultPeoplePerSlot = pc.ApptDefaultPeoplePerSlot;
            Created = pc.Created;
            Modified = pc.Modified;
            Latitude = pc.Latitude;
            Longitude = pc.Longitude;
            ExpiredDate = pc.ExpiredDate;
            ContactFirstName = pc.ContactFirstName;
            ContactLastName = pc.ContactLastName;
            CancelPolicy = pc.CancelPolicy;
            CancelHour = pc.CancelHour;
            CancelRefundPercent = pc.CancelRefundPercent;
            MobileCarrier = pc.MobileCarrier;
            Distance = LocatorHelper.CalculateDistance(currentLat, currentLon, this.Latitude, this.Longitude);           
            CancellationPolicy = new CancellationPolicy(pc.CancelPolicy, pc.CancelHour, pc.CancelRefundPercent);
            Rate = Convert.ToDecimal(pc.Rate);
            IsShowCatagory = pc.IsShowCatagory;
        }

            
        public CompanyProfileIPhone(ProfileCompany profileCompany, int custId)
            : this(profileCompany, 0, 0, custId)
        {
            isFeature =  DAL.isFeatureCompany(profileCompany.ProfileID);
            IsUSerFavourite = DAL.isFavorite(custId, profileCompany.ProfileID);
            ListServices = DAL.GetTypeNameFromProfileID(profileCompany.ProfileID);
            ImageUrl = profileCompany.CompanyMedias.Select(m => m.Medium.LocationData).ToList();
        }
    }


    public class CancellationPolicy
    {
        public int? Id { get; set; }
        public int? CancelHour { get; set; }
        /// <summary>
        /// Type of Policy = custom and none refund is selected, will be return 100%
        /// </summary>
        public int? CancelRefundPercent { get; set; }

        public string CancelPolicyDescription
        {
            get
            {
                if (!Id.HasValue || Id == 0)
                    return string.Empty;

                int? totalRefund = 100 - CancelRefundPercent;
                string description = string.Format("if you modify or cancel {0} hours before this appointment or later, you will be charged {1}% of the total amount.", CancelHour, totalRefund);
                return description;
            }
        }

        public string CancelPolicyTitle
        {
            get
            {
                if (!Id.HasValue || Id == 0)
                    return string.Empty;
                return CancelHour + "-hour cancelation policy";
            }
        }

        public CancellationPolicy(int? cancelPolicy, int? cancelHour, int? cancelRefundPercent)
        {
            Id = cancelPolicy;
            CancelHour = cancelHour;
            CancelRefundPercent = cancelRefundPercent;
        }
    }

}
