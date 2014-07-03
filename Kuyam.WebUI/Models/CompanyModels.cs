using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;

using M2.Util.MVC;
using System.Text;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;



namespace Kuyam.WebUI.Models
{
    public class CompanyViewModel : IModel
    {
        public ProfileCompany Company { get; set; }



        public MvcHtmlString Hours { get; set; }

        public void LockAndLoad()
        {
            if (Company != null)
                Hours = Util.GetDisplayableRegularHours(Company.Profile);
        }
    }

    public class CompanyProfileModel : IModel
    {
        public ProfileCompany ProfileCompany { get; set; }
        public string Title { get; set; }

        public SelectList BusinessTypes { get; set; }
        public SelectList CompanyStatusTypes { get; set; }
        public MvcHtmlString Hours { get; set; }
        public MvcHtmlString ExceptionHours { get; set; }
        public SelectList StateList { get; set; }


        public void LockAndLoad()
        {
            BusinessTypes = Types.GetTypeList(Types.TypeGroup.CompanyType).ToSelectList();
            CompanyStatusTypes = Types.GetTypeList(Types.TypeGroup.CompanyStatus).ToSelectList();
            StateList = SelectListHelper.GetStateSelectList(ProfileCompany.State);

            if (ProfileCompany.ProfileID > 0)
            {
                Hours = Util.GetDisplayableRegularHours(ProfileCompany.Profile);
                ExceptionHours = Util.GetDisplayableExceptionHours(ProfileCompany.Profile);
            }
        }
    }

    public class CompanySetupModel
    {
        public CompanySetupModel()
        {
            CategoryCompany = new List<SelectListItem>();
            Categories = new List<Category>();
            Companylist = new List<Companylist>();
            serviceCompany = new List<ServiceCompany>();
            CompanyHours = new List<CompanyHour>();
        }

        public int ProfileID { get; set; }
        public int CustID { get; set; }
        public int ProfileTypeID { get; set; }
        public int PrivacyTypeID { get; set; }
        [Required(ErrorMessage = "company name is required.")]
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int RelationshipTypeID { get; set; }
        public int CompanyTypeID { get; set; }
        public int CompanyStatusID { get; set; }

        //[MaxLength(1500, ErrorMessage = "1500 characters max")]
        public string Desc { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        [Required(ErrorMessage = "street is required.")]
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        [Required(ErrorMessage = "city is required.")]
        [Remote("IscityAndState", "Company")]
        public string City { get; set; }
        public string State { get; set; }
        [Required(ErrorMessage = "zip code is required.")]
        [Remote("IsZipCode", "Company")]
        public string Zip { get; set; }
        [Required(ErrorMessage = "phone is required.")]
        [RegularExpression(@"^\s*([\(]?)\[?\s*\d{3}\s*\]?[\)]?\s*[\-]?[\.]?\s*\d{3}\s*[\-]?[\.]?\s*\d{4}$", ErrorMessage = "invalid phone")]
        public string Phone { get; set; }
        public string Fax { get; set; }
        [Required(ErrorMessage = "e-mail is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "email")]
        [Email(ErrorMessage = "invalid email")]
        public string Email { get; set; }
        public string Url { get; set; }
        public string Youtubelink { get; set; }
        public int? FirstAlert { get; set; }
        public int? SecondAlert { get; set; }
        public bool EmailType { get; set; }
        public bool TextType { get; set; }
        public string PaymentOptions { get; set; }
        public string MapUrl { get; set; }
        public string Neighborhood { get; set; }
        public string CrossStreet { get; set; }
        public string PublicTransportation { get; set; }
        public string Parking { get; set; }
        public string Notes { get; set; }
        public bool ApptAutoConfirm { get; set; }
        public int ApptDefaultSlotDuration { get; set; }
        public int ApptDefaultPeoplePerSlot { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int ServiceID { get; set; }
        public IList<SelectListItem> CategoryCompany { get; set; }

        public string[] Listcategory { get; set; }
        public string[] Listcategoryname { get; set; }
        public string[] Listhour { get; set; }
        public List<Category> Categories { get; set; }
        public List<ServiceCompany> serviceCompany { get; set; }
        public List<CompanyHour> CompanyHours { get; set; }
        public List<Companylist> Companylist { get; set; }
        public List<CompanyMedia> CompanyMedias { get; set; }
        public int LogoID { get; set; }
        public int PhotoID { get; set; }
        public int VideoID { get; set; }
        public string LogoData { get; set; }
        public string PhotoData { get; set; }
        public string VideoData { get; set; }
        public MvcHtmlString FlashData { get; set; }

        public int PhotoID2 { get; set; }
        public string PhotoData2 { get; set; }
        public int PhotoID3 { get; set; }
        public string PhotoData3 { get; set; }

        public ProfileCompany ProfileCompany { get; set; }
    }

    public class CompanySettingModel
    {
        public CompanySettingModel()
        {
            FirstListItem = new List<SelectListItem>();
            SecondListItem = new List<SelectListItem>();
        }
        public int ProfileID { get; set; }
        public int CustID { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\s*([\(]?)\[?\s*\d{3}\s*\]?[\)]?\s*[\-]?[\.]?\s*\d{3}\s*[\-]?[\.]?\s*\d{4}$", ErrorMessage = "invalid Phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "e-mail is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "email")]
        [Email(ErrorMessage = "invalid email")]
        public string Email { get; set; }
        public string PaymentOptions { get; set; }
        public int? PaymentMethod { get; set; }
        public bool EmailType { get; set; }
        public bool TextType { get; set; }
        public int? FirstAlert { get; set; }
        public int? SecondAlert { get; set; }
        public int? Policy { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public bool IsAdmin { get; set; }
        public IList<SelectListItem> FirstListItem { get; set; }
        public IList<SelectListItem> SecondListItem { get; set; }

        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }

        public int CancelPolicy { get; set; }
        public int CompanyType { get; set; }

        public string anytime { get; set; }
        public IEnumerable<SelectListItem> Hours
        {
            get
            {
                return new List<SelectListItem>
                           {                               
                               new SelectListItem {Value = "1", Text = "1 hour"},
                               new SelectListItem {Value = "2", Text = "2 hours"},
                               new SelectListItem {Value = "3", Text = "3 hours"},
                               new SelectListItem {Value = "6", Text = "6 hours"},
                               new SelectListItem {Value = "12", Text = "12 hours"},
                               new SelectListItem {Value = "24", Text = "24 hours"},
                               new SelectListItem {Value = "36", Text = "36 hours"},
                               new SelectListItem {Value = "48", Text = "48 hours"},
                               new SelectListItem {Value = "72", Text = "72 hours"}
                           };
            }

        }

        public string norefund { get; set; }
        public IEnumerable<SelectListItem> Funds
        {
            get
            {
                return new List<SelectListItem>
                           {
                               new SelectListItem {Value = "0", Text = "none"},
                               new SelectListItem {Value = "25", Text = "25%"},
                               new SelectListItem {Value = "50", Text = "50%"},
                               new SelectListItem {Value = "75", Text = "75%"}                              
                           };
            }
        }

        public string SelectCarrier { get; set; }
        public List<Kuyam.Database.Type> CarrierList { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustEmail { get; set; }
    }

    public class VerificationCode
    {
        public VerificationCode()
        {
        }
        [Required(ErrorMessage = "invite code is required.")]
        [DataType(DataType.Text)]
        public string Code { get; set; }
    }

    public class Category
    {
        public string CategoryID { get; set; }
        public string NamCategory { get; set; }
    }

    public class cRegularClient
    {
        public int RegularClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }


    public class Companylist
    {
        public string CompanyID { get; set; }
        public string CompanyNam { get; set; }
        public int CompanyIndex { get; set; }
    }

    public class ProfileHoursEditModel : IModel
    {
        public ProfileHour Hour { get; set; }

        public SelectList DayList { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public void LockAndLoad()
        {
            DayList = Types.GetTypeList(Types.TypeGroup.Day).ToSelectList(Hour.Day);
            Start = new DateTime(Hour.Start.Ticks);
            End = new DateTime(Hour.End.Ticks);
        }

        // branch change
        public void PostProcess()
        {
            Hour.Start = new TimeSpan(Start.Hour, Start.Minute, 0);
            Hour.End = new TimeSpan(End.Hour, End.Minute, 0);
        }
    }

    public class ProfileHoursExceptionEditModel : IModel
    {
        public ProfileHoursException Hour { get; set; }

        public void LockAndLoad()
        {
        }
    }

    public class CompanyEmployees : IModel
    {
        public string EmployeeID { get; set; }
        public string stringServiceCompanyIDs { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public HttpPostedFileBase fileUpload { get; set; }

        public void LockAndLoad()
        {
        }
    }
}