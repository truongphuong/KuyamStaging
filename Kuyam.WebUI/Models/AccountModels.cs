using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DataAnnotationsExtensions;
using M2.Util.MVC;
using Kuyam.Database;
using System.Text;
using Kuyam.Domain;
using System.IO;
using Kuyam.WebUI.validation;

namespace Kuyam.WebUI.Models
{

    #region Models

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "current password")]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "new password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "confirm new password")]
        [EqualToAttribute("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }

        [Display(Name = "keep me logged in")]
        public bool RememberMe { get; set; }

        public LoginModel()
        {
            RememberMe = true;
        }
    }

    public class LogOutModel
    {
        public int? IsOriginalPage { get; set; }
        public string RedirectUrl { get; set; }

    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "first name is required.")]
        [Display(Name = "test key")]
        public string TestKey { get; set; }
        public bool TestKeyIsValid { get; set; }

        [Required]
        [Display(Name = "first name")]
        [StringLength(50, ErrorMessage = "first name must be less than 50 characters")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "last name")]
        [StringLength(50, ErrorMessage = "last name must be less than 50 characters")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "zip code")]
        [StringLength(50, ErrorMessage = "zip code must be 5 characters long.")]
        public string ZipCode { get; set; }

        [Display(Name = "company name")]
        [StringLength(100, ErrorMessage = "company name must be less than 100 characters")]
        public string CompanyName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "email")]
        [Email]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "re-enter email")]
        [Email]
        [EqualToAttribute("UserName", ErrorMessage = "The email and confirmation email do not match.")]
        public string ConfirmEmail { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "confirm password")]
        [EqualToAttribute("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        [Display(Name = "Account Type")]
        public int CustType { get; set; }
        [Display(Name = "gender")]
        public int? GenderTypeID { get; set; }
        public bool IsGuest { get; set; }

        [Display(Name = "Other Calendar")]
        public string OtherCalendar { get; set; }
        public bool DoOutlookCalendar { get; set; }
        public bool DoYahooCalendar { get; set; }
        public DateTime? Birthday { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string Website { get; set; }
        public string SocialTwitter { get; set; }
        public string SocialPinterest { get; set; }
        public string SocialFacebook { get; set; }
        public string SocialBlogger { get; set; }
        public string SocialTumblr { get; set; }
        public string SocialWordpress { get; set; }
        public bool DoContactEmail { get; set; }
        public bool DoContactText { get; set; }
        public string ContactEmail { get; set; }
        public string ContactText { get; set; }
        public string Notes { get; set; }
        public bool DoSubmit { get; set; }
        public bool IsCompany { get; set; }
        public int PreferredPhone { get; set; }
        public string Phone { get; set; }
        public string Carrier { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public HttpPostedFileBase UploadUrl { get; set; }
        public bool IsFacebookRegister { get; set; }
        public string FacebookUserId { get; set; }
        public InfoConnServiceReference.ConnectorSource FacebookConnectorSource { get; set; }
        public InfoConnServiceReference.ConnectorSource GoogleConnectorSource { get; set; }
        public SelectList CustTypeList = Types.GetTypeList(Types.TypeGroup.CustType).ToSelectList();
        public SelectList GenderList = Types.GetTypeList(Types.TypeGroup.Gender).ToSelectList();

        //public SelectList CustTypeList = (int)Types.CustTypes.ToSelectList();

        //public Cust Customer
        //{
        //    get
        //    {
        //        Cust cust = new Cust();

        //        cust.Birthday = this.Birthday;
        //        cust.FirstName = this.FullName;
        //        cust.GenderTypeID = this.GenderTypeID;
        //        cust.MobilePhone = this.ContactText;
        //        cust.Notes = this.Notes;
        //        cust.Zip = this.ZipCode;

        //        if (IsFacebookRegister)
        //        {
        //            cust.FacebookUserID = FacebookUserId;
        //        }
        //        return cust;
        //    }
        //}

    }


    public class AddUserModel 
    {
        public AddUserModel()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.UserName = string.Empty;
            this.ContactEmail = string.Empty;
            this.IsCompany = false;
            this.GenderTypeID = 0;
            this.Latitude = 0;
            this.Longitude = 0;
            this.FacebookUserId = string.Empty;
            this.AllRoles = new List<aspnet_Roles>();

        }

        public int CustID { get; set; }

        [Required]
        [Display(Name = "first name")]
        [StringLength(50, ErrorMessage = "first name must be less than 50 characters")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "last name")]
        [StringLength(50, ErrorMessage = "last name must be less than 50 characters")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "email")]
        [Email]
        public string UserName { get; set; }

        [Required]
        [ValidatePasswordLength(ErrorMessage = "Password at least 6 letters.")]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }
                
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [EqualToAttribute("Password", ErrorMessage = "password do not match.")]        
        public string ConfirmPassword { get; set; }

        public string RoleName { get; set; }

        public List<aspnet_Roles> AllRoles { get; set; }

        [Concierge("RoleName", "Please choose a hotel")]
        public int? HotelId { get; set; }
        public List<Hotel> HotelList { get; set; }

        [Display(Name = "Account Type")]
        public int CustType { get; set; }

        [Display(Name = "gender")]
        public int GenderTypeID { get; set; }
        public DateTime? Birthday { get; set; }
        public bool IsCompany { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FacebookUserId { get; set; }
    }

    public class RegisterInviteCode
    {
        [Required(ErrorMessage = "invite code is required.")]
        [Display(Name = "invite code")]
        public string TestKey { get; set; }

    }

    public class RegisterEmail
    {

        [Required(ErrorMessage = "first name is required.")]
        [Display(Name = "first name")]
        [StringLength(50, ErrorMessage = "first name must be less than 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "last name is required.")]
        [Display(Name = "last name")]
        [StringLength(50, ErrorMessage = "last name must be less than 50 characters")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "gender")]
        public int? selectgender { get; set; }

        public bool IsGuest { get; set; }
        public string Birthday { get; set; }

        //[Required(ErrorMessage = "email required.")]
        [DataType(DataType.EmailAddress)]
        [Email(ErrorMessage = "invalid e-mail address.")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "confirm email")]
        [EqualToAttribute("Email", ErrorMessage = "e-mail addresses do not match.")]
        public string ConfirmEmail { get; set; }

        //[Required(ErrorMessage = "password required.")]
        //[ValidatePasswordLength(ErrorMessage = "least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        [Display(Name = "confirm password")]
        [EqualToAttribute("Password", ErrorMessage = "passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "zip code is required.")]
        [Display(Name = "zip code")]
        [StringLength(50, ErrorMessage = "zip code must be 5 characters long.")]
        public string ZipCode { get; set; }

        //[Required(ErrorMessage = "phone number is required.")]
        //[StringLength(12)]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\s*([\(]?)\[?\s*\d{3}\s*\]?[\)]?\s*[\-]?[\.]?\s*\d{3}\s*[\-]?[\.]?\s*\d{4}$", ErrorMessage = "Invalid Phone")]
        [Display(Name = "phone number")]
        public string Phone { get; set; }

        [Required]
        public bool AgreeToTerms { get; set; }

        [Required]
        public bool AgreeToNonDisclosure { get; set; }
        public SelectList GenderList = Types.GetTypeList(Types.TypeGroup.Gender).ToSelectList();

        public string SelectCarrier { get; set; }
        public List<Kuyam.Database.Type> CarrierList { get; set; }

        public bool IsFacebookRegister { get; set; }
    }

    public class RegisterName
    {
        //[Required(ErrorMessage = "full name required.")]
        //[Display(Name = "Full Name")]
        //public string FullName { get; set; }

        public bool DoOutlookCalendar { get; set; }
        public bool DoYahooCalendar { get; set; }

        [Required(ErrorMessage = "name of other calendar is required.")]
        [Display(Name = "Other Calendar")]
        public string OtherCalendar { get; set; }

        public bool IsCompany { get; set; }
        public HttpPostedFileBase UploadUrl { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        [Display(Name = "email")]
        public string UserName { get; set; }

        public string Message { get; set; }
    }

    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);

        bool CreateCustomer(AddUserModel model);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;
        private readonly CustService _custService;
        private readonly IFormsAuthenticationService _formsService;
        //public AccountMembershipService()
        //    : this(null)
        //{
        //}

        public AccountMembershipService(CustService custService,
            IFormsAuthenticationService formsService)
        {
            this._provider = Membership.Provider;
            this._custService = custService;
            this._formsService = formsService;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName) || userName == "enter e-mail address")
                throw new ArgumentException("value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("value cannot be null or empty.", "password");
            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public bool CreateCustomer(AddUserModel model)
        {
            bool isAdmin = false;
            var user = Membership.GetUser(model.UserName);
            if (user != null)
                return false;
            // Attempt to register the user (ASP.NET stuff)
            MembershipCreateStatus createStatus = this.CreateUser(model.UserName, model.Password, model.UserName);
            if (createStatus != MembershipCreateStatus.Success)
            {
                DAL.DeleteAspUser(model.UserName);
                return false;
            }

            if (model.Birthday == DateTime.MinValue)
            {
                model.Birthday = DateTime.Now;
            }
            model.CustType = 115; //115 = Personal;
            int custID = 0;
            Guid userid = DAL.GetAspUserID(model.UserName);

            Roles.AddUserToRole(model.UserName, "Personal");
            try
            {
                custID = Cust.Create(model.UserName, userid, model.FirstName, model.LastName,
                    string.Empty, model.PhoneNumber, string.Empty, isAdmin, model.CustType, string.Empty,
                    0, model.Birthday, model.IsCompany, false, false, string.Empty, model.Latitude, model.Longitude, model.GenderTypeID, model.FacebookUserId);

                model.CustID = custID;
                _custService.AddDefaultCalendar(custID, model.FirstName);
            }
            catch (Exception ex)
            {
                DAL.DeleteUser(model.UserName);
                return false;
                //throw ex;
            }
            if (!string.IsNullOrEmpty(model.RoleName))
            {
                Roles.AddUserToRole(model.UserName, model.RoleName);
            }

            return true;
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
        string CreatePasswordHash(string pass, string fomat);
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("value cannot be null or empty.", "userName");
            createPersistentCookie = true;
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public string CreatePasswordHash(string pass, string fomat)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(pass, fomat);
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters long.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            if (valueAsString == null)
                return true;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
				new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
			};
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateZipCodeLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "";
        private readonly int _minCharacters = 5;

        public ValidateZipCodeLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            if (valueAsString == null)
                return true;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
				new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), 5, 5),
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), 10, 10)
			};
        }
    }
    #endregion

}
