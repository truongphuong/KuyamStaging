using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Kuyam.Database
{
    [MetadataType(typeof(Cust_Validation))]
    public partial class Cust { }
    public class Cust_Validation
    {
        [StringLength(50, ErrorMessage = "Names must be less than 50 characters")]
        [Required]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Names must be less than 50 characters")]
        [Required]
        public string LastName { get; set; }

        [StringLength(2, ErrorMessage = "State must be two characters")]
        public string State { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        //[Required]
        public Nullable<System.DateTime> Birthday { get; set; }
    }

    //public class Validators
    //{
    //}

    ////////////////////

    [MetadataType(typeof(Company_Validation))]
    public partial class ProfileCompany { }
    public class Company_Validation
    {
        [Required]
        public string Name { get; set; }

        //[Required]
        //public string Phone { get; set; }

        //[Integer]
        //[Min(15, ErrorMessage="Appointments must be at least 15 minutes long")]
        //[Required]
        //public int ApptDefaultSlotDuration { get; set; }

        //[Integer]
        //[Min(1, ErrorMessage = "At least one person is required per appointment slot")]
        //[Required]
        //public int ApptDefaultPeoplePerSlot { get; set; }

        //[Email(ErrorMessage= "Please enter a valid email address")]
        //public int Email { get; set; }

        //[Url(ErrorMessage="Please enter a valid web address")]
        //public string Url { get; set; }

        //[Url(ErrorMessage = "Please enter a valid web address")]
        //public string MapUrl { get; set; }
    }
}