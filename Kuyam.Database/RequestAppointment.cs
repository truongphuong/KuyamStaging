//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Kuyam.Database
{
    public partial class RequestAppointment
    {
        public int Id { get; set; }
        public Nullable<int> ServiceCompanyId { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public System.DateTime Start { get; set; }
        public System.DateTime End { get; set; }
        public string Notes { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public int CustID { get; set; }
        public Nullable<int> CalendarId { get; set; }
        public int ProfileId { get; set; }
        public Nullable<int> HotelID { get; set; }
        public Nullable<int> StaffID { get; set; }
    
        public virtual Calendar Calendar { get; set; }
        public virtual ProfileCompany ProfileCompany { get; set; }
        public virtual Cust Cust { get; set; }
        public virtual Hotel Hotel { get; set; }
        public virtual HotelStaff HotelStaff { get; set; }
        public virtual ServiceCompany ServiceCompany { get; set; }
    }
    
}
