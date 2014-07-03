using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;

using M2.Util.MVC;
using M2.Util.AspNet;
using System.ComponentModel.DataAnnotations;
using Kuyam.Database.Extensions;

namespace Kuyam.WebUI.Models
{
    public class FCODSetupModel : IModel
    {
        public int SelectedProfileID { get; set; }
        public SelectList Companies { get; set; }

        public void LockAndLoad()
        {
            Companies = DAL.GetAllCompanies().ToSelectList();
            foreach (SelectListItem i in Companies)
            {
                i.Text = i.Text + " (" + i.Value + ")";
            }
        }
    }

    public class CompanyAdminModel
    {
        public string CompanyAction { get; set; }
        public int SelectedProfileID { get; set; }
        public SelectList Companies { get; set; }

        public void LockAndLoad()
        {
            Companies = DAL.GetAllCompanies().ToSelectList();
            foreach (SelectListItem i in Companies)
            {
                i.Text = i.Text + " (" + i.Value + ")";
            }
        }
    }

    public class VerifyCompanyListModel : IModel
    {
        public List<ProfileCompany> Companies { get; set; }

        public void LockAndLoad()
        {
            Companies = DAL.GetUnverifiedCompanies();
        }
    }

    public class InactiveCompanyAppointmentListModel : IModel
    {
        public List<Cust_Company> CustCompanies { get; set; }

        public void LockAndLoad()
        {
            CustCompanies = Profile.GetInactiveAppointmentCompanies();
            //foreach (Appointment_Company av in AppointmentCompanies)
            //    av.Setup();
        }
    }

    public class UserAdminModel : IModel
    {
        public List<string> Gods { get; set; }
        public List<string> Admins { get; set; }
        public List<string> Supports { get; set; }

        public string Username { get; set; }
        public string UserAction { get; set; }

        public void LockAndLoad()
        {
            Gods = MembershipHelper.GetUsersInRole("god");
            Admins = MembershipHelper.GetUsersInRole("admin");
            Supports = MembershipHelper.GetUsersInRole("support");
        }
    }

    public class HotelModel
    {
        public int HotelID { get; set; }
        public int CustID { get; set; }
        public List<Cust> CustList { get; set; }
        public Cust Cust { get; set; }
        public int? HotelType { get; set; }
        [Required(ErrorMessage = "name is required.")]
        public string Name { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    public class StaffModel
    {
        public int StaffID { get; set; }
        public int HotelID { get; set; }
        public string ConciergeName { get; set; }
    }
    public class HotelListModel
    {
        public int HotelID { get; set; }
        public int CustID { get; set; }
        public string UserName { get; set; }
        public string LogoId { get; set; }
        public int? HotelType { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }

    }

    public class FeatureCompanyHotelModel
    {
        public int HotelId { get; set; }

        public List<Hotel> Hotel { get; set; }

        public List<FeaturedHotel> FeaturedHotel { get; set; }

        public List<ProfileCompany> ProfileCompany { get; set; }
    }


    public class AppointmentFeedbackModel
    {
        public int ID { get; set; }
        public int? EmployeeID { get; set; }
        public int ServiceCompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Content { get; set; }
        public int? RatingValue { get; set; }
        public int CustID { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? AppointmentID { get; set; }
        public Appointment Appointment { get; set; }
        public string PrivateContent { get; set; }

    }

    public class NokuyamAppointmentModel
    {
        public NokuyamAppointmentModel()
        {
            Date = string.Empty;
            Start = string.Empty;
            End = string.Empty;
            ConciergeList = new List<StaffModel>();
            HotelList = new List<Hotel>();
        }

        public int AppointmentID { get; set; }
        public int ProfileId { get; set; }
        public List<ProfileCompany> ProfileCompanys { get; set; }
        public int? ServiceId { get; set; }
        public List<Service> ServiceList { get; set; }
        public int AppointmentStatusID { get; set; }

        [Required(ErrorMessage = "description is required.")]
        [Display(Name = "password")]
        public string Description { get; set; }

        [Required(ErrorMessage = "date is required.")]
        public string Date { get; set; }

        [Required(ErrorMessage = "start is required.")]
        [DataType(DataType.Time, ErrorMessage = "invalid time.")]
        public string Start { get; set; }

        public string End { get; set; }
        public string Notes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int CustID { get; set; }       
        public List<CustExtension> CustList { get; set; }
        public int? HotelId { get; set; }
        public List<Hotel> HotelList { get; set; }
        public int? ConciergeId { get; set; }
        public List<StaffModel> ConciergeList { get; set; }

        [Required(ErrorMessage = "calendar name is required.")]
        public int? CalendarId { get; set; }
        public List<Calendar> CalendarList { get; set; }

        [Required(ErrorMessage = "employee name is required.")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "price is required.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "duration is required.")]
        public int Duration { get; set; }

        public int? AttendeesNumber { get; set; }

    }

    public class NokuyamAppointmentListModel
    {
        public int AppointmentID { get; set; }
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ServiceName { get; set; }
        public int AppointmentStatusID { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string UserName { get; set; }
        public string CalendarName { get; set; }
        public string EmployeeName { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int? AttendeesNumber { get; set; }
    }

    public class RequestTimeSlot
    {
        public ProfileCompany ProfileCompany { get; set; }
        public int Id { get; set; }
        public string FromHour { get; set; }
        public string ToHour { get; set; }
        public string CompanyName { get; set; }
        public int ProfileId { get; set; }
        public string DateOfWeek { get; set; }
        public bool IsDaily { get; set; }

        public string htmlTimeSlots { get; set; }
        public string StringListDays { get; set; }
    }


    public class ProposedAppointmentModel
    {
        public ProposedAppointmentModel()
        {
            Date = string.Empty;
            Start = string.Empty;
            End = string.Empty;
            ConciergeList = new List<StaffModel>();
            HotelList =new List<Hotel>();
        }

        public int AppointmentID { get; set; }
        public int ProfileId { get; set; }
        public List<ProfileCompany> ProfileCompanys { get; set; }

        [Required(ErrorMessage = "service is required.")]
        public int? ServiceCompanyId { get; set; }

        public int? ServiceId { get; set; }
        public List<Service> ServiceList { get; set; }

        public List<CompanyService> ServiceCompanyList { get; set; }
        public int AppointmentStatusID { get; set; }

        [Required(ErrorMessage = "description is required.")]
        [Display(Name = "password")]
        public string Description { get; set; }

        [Required(ErrorMessage = "date is required.")]
        public string Date { get; set; }

        [Required(ErrorMessage = "start time  is required.")]
        [DataType(DataType.Time, ErrorMessage = "invalid time.")]
        public string Start { get; set; }

        public string End { get; set; }
        public string Notes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int CustID { get; set; }
        //public List<Cust> CustList { get; set; }
        public List<CustExtension> CustList { get; set; }

        [Required(ErrorMessage = "calendar name is required.")]
        public int? CalendarId { get; set; }
        public List<Calendar> CalendarList { get; set; }
        public int? HotelId { get; set; }
        public List<Hotel> HotelList { get; set; }
        public int? ConciergeId { get; set; }
        public List<StaffModel> ConciergeList { get; set; }

        [Required(ErrorMessage = "employee name is required.")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "price is required.")]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "price must be number and greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "duration is required.")]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "duration must be number and greater than 0.")]
        public int Duration { get; set; }

        public int? AttendeesNumber { get; set; }

        public bool IsHasDevice { get; set; }
        public string ServiceName { get; set; }

        public string ProfileCompanyName { get; set; }

        public string CusFullName { get; set; }
        public string CalendarName { get; set; }

        public Cust Cus { set; get; }

        public string ApptTime { get; set; }
    
    }
}