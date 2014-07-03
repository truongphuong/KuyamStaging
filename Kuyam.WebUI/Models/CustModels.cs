using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using M2.Util;
using M2.Util.MVC;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Kuyam.Domain;
using DataAnnotationsExtensions;

namespace Kuyam.WebUI.Models
{
    public class CustHomeModel : IModel
    {
        public int CustID { get; set; }

        public FeaturedCompanyModel FeaturedCompanyModel { get; set; }
        public int CustAge { get; set; }

        public CustHomeModel()
        {
            FeaturedCompanyModel = new FeaturedCompanyModel();
        }

        public void LockAndLoad()
        {
            FeaturedCompanyModel.Company = DAL.GetFeaturedCompany();
            FeaturedCompanyModel.LockAndLoad();
            Cust c = Cust.Load(CustID);
            CustAge = c.Age;
        }
    }
    /*	public class CustHomeModel : IModel
        {
            public Cust Cust { get; set; }
            public FeaturedCompanyModel FeaturedCompanyModel { get; set; }
            public NotepadModel NotepadModel { get; set; }
            public ScheduleAppointmentModel ScheduleAppointmentModel { get; set; }
            public AppointmentFeedbackModel AppointmentFeedbackModel { get; set; }
            public PanelTypes LHSPanel { get; set; }

            public enum PanelTypes
            {
                FCOD,
                Notepad,
                ScheduleAppointment,
                Feedback
            }

            public CustHomeModel()
            {
                FeaturedCompanyModel = new FeaturedCompanyModel();
                NotepadModel = new NotepadModel();
                ScheduleAppointmentModel = new ScheduleAppointmentModel();
                AppointmentFeedbackModel = new AppointmentFeedbackModel();
            }

            public void LockAndLoad()
            {
                FeaturedCompanyModel.LockAndLoad();
                NotepadModel.LockAndLoad();
                ScheduleAppointmentModel.LockAndLoad();
                AppointmentFeedbackModel.LockAndLoad();
            }
        }
    */

    
    public class UserEditModel
    {
        public UserEditModel()
        {
          FirstListItem=  new List<SelectListItem>();
          SecondListItem = new List<SelectListItem>();
        }
        
        public int CustID { get; set; }
      
        public int AccountID { get; set; }
       
        public Guid AspUserID { get; set; }
       
        public string UserName { get; set; }
        
        [DataType(DataType.Password)]     
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
       
        [ValidatePasswordLength( ErrorMessage= "Password at least 6 letters.")]
        [DataType(DataType.Password)]       
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [EqualToAttribute("NewPassword", ErrorMessage = "password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "email is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "email")]
        [Email(ErrorMessage = "invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Zip is required.")]
        [StringLength(10)]
        [DataType(DataType.Text)]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip")]
        public string Zip { get; set; }       
       
        public string HomePhone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //[Required(ErrorMessage = "Phone is required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\s*([\(]?)\[?\s*\d{3}\s*\]?[\)]?\s*[\-]?[\.]?\s*\d{3}\s*[\-]?[\.]?\s*\d{4}$", ErrorMessage = "Invalid Phone")]
        public string MobilePhone { get; set; }
      
        public int? PreferredPhoneTypeID { get; set; }

        public bool EmailType { get; set; }

        public bool TextType { get; set; }

        public bool CallType { get; set; }
       
        public DateTime? Modified { get; set; }
        
        public int? FirstAlert { get; set; }
        
        public int? SecondAlert { get; set; }

        public IList<SelectListItem> FirstListItem { get; set; }
        public IList<SelectListItem> SecondListItem { get; set; }
        public string SelectCarrier { get; set; }
        public List<Kuyam.Database.Type> CarrierList { get; set; }

    }

    public  class UserModel
    {
        public UserModel()
        {
            FirstListItem = new List<SelectListItem>();
            SecondListItem = new List<SelectListItem>();
        }

        public int CustID { get; set; }

        public int AccountID { get; set; }

        public Guid AspUserID { get; set; }

        public string UserName { get; set; }
        
        public string Email { get; set; }
        
        public string Zip { get; set; }
        
        public string HomePhone { get; set; }

        public string MobilePhone { get; set; }

        public int? PreferredPhoneTypeID { get; set; }

        public bool EmailType { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool TextType { get; set; }

        public bool CallType { get; set; }

        public DateTime? Modified { get; set; }

        public int? FirstAlert { get; set; }

        public int? SecondAlert { get; set; }

        public string PhotoUrl { get; set; }

        public string FacebookToken { get; set; }
        public string FacebookUserID { get; set; }

        public string IcalUrl { get; set; }
        public IList<SelectListItem> FirstListItem { get; set; }
        public IList<SelectListItem> SecondListItem { get; set; }

    }

    public class FeaturedCompanyModel : IModel
    {
        public ProfileCompany Company { get; set; }
        public int MediaID { get; set; }

        public MediaDisplayModel MediaDisplayModel { get; set; }

        public FeaturedCompanyModel()
        {
            MediaDisplayModel = new MediaDisplayModel();
        }

        public FeaturedCompanyModel(ProfileCompany profileCompany, int mediaID)
        {
            Company = profileCompany;
            MediaID = mediaID;
            LockAndLoad();
        }

        public void LockAndLoad()
        {
            MediaDisplayModel = new MediaDisplayModel(MediaID, 187, 280);
        }
    }

    public class NotepadModel : IModel
    {
        public string Notes { get; set; }

        public void LockAndLoad()
        { }
    }

    //public class AppointmentFeedbackModel : IModel
    //{
    //    public class APC
    //    {
    //        public Appointment Appointment { get; set; }
    //        public ProfileCompany ProfileCompany { get; set; }
    //        public List<Calendar> Calendars { get; set; }
    //    }

    //    public Cust Cust { get; set; }
    //    public List<APC> APCs { get; set; }


    //    public void Init(Cust cust)
    //    {
    //        Cust = cust;
    //        APCs = new List<APC>();
    //    }

    //    public void LockAndLoad()
    //    {
    //        List<Appointment> appts = Cust.Appointments2().Distinct().ToList();
    //        foreach (Appointment a in appts)
    //        {
    //            APC apc = new APC();
    //            apc.Appointment = a;
    //            apc.ProfileCompany = a.GetProfileCompany();
    //            apc.Calendars = a.GetCalendars(Cust.CustID);
    //            APCs.Add(apc);
    //        }
    //    }
    //}

    public class CustProfileModel : IModel
    {
        public Cust Cust { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        public SelectList GenderList { get; set; }
        public SelectList PreferredPhoneList { get; set; }
        public SelectList StatusReasonList { get; set; }
        public SelectList StateList { get; set; }

        public int ProfileCompanyID { get; set; }

        public void LockAndLoad()
        {
            GenderList = Types.GetTypeList(Types.TypeGroup.Gender).ToSelectList(Cust.GenderTypeID);
            PreferredPhoneList = Types.GetTypeList(Types.TypeGroup.PreferredPhone).ToSelectList(Cust.PreferredPhoneTypeID);
            StatusReasonList = Types.GetTypeList(Types.TypeGroup.CustStatus).ToSelectList(Cust.CustStatusReasonTypeID);

            StateList = SelectListHelper.GetStateSelectList(Cust.State);

            Birthday = Cust.Birthday;

            ProfileCompanyID = Cust.GetCompanyProfileID();
        }

        public void PostProcess()
        {
            if (Cust != null)
                Cust.Birthday = Birthday;
        }
    }

    public class CompanySearchModel : IModel
    {
        public string SearchTerms { get; set; }
        public List<ProfileCompany> Results { get; set; }
        public List<ProfileCompany> PriorCompanies { get; set; }
        public SelectList StateList { get; set; }

        public ProfileCompany NewCompany { get; set; }

        public void LockAndLoad()
        {
            NewCompany = new ProfileCompany();
            StateList = SelectListHelper.GetStateSelectList();
            PriorCompanies = DAL.GetPriorCompanies(MySession.CustID, 10);
        }

        public string GetCityState(int profileID)
        {
            Profile p = Profile.Load(profileID);
            return GetCityState(p.ProfileCompany.City, p.ProfileCompany.State);
        }

        public string GetCityState(string city, string state)
        {
            StringBuilder sb = new StringBuilder();
            if (!city.IsNullOrEmpty())
                sb.Append(city);
            if (!state.IsNullOrEmpty())
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(state);
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, "(");
                sb.Append(")");
            }

            return sb.ToString();
        }
    }

    public class CalendarSearchModel : IModel
    {
        public string SearchTerms { get; set; }
        public List<Appointment_Company> Results { get; set; }

        public void LockAndLoad()
        {
        }
    }
}
