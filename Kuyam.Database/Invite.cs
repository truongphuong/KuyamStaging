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
    public partial class Invite
    {
        public int InviteID { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int MaxUses { get; set; }
        public int Uses { get; set; }
        public int AccountTypeID { get; set; }
        public Nullable<bool> Active { get; set; }
        public string LName { get; set; }
        public int InviteType { get; set; }
        public Nullable<int> Status { get; set; }
        public string FacebookToken { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Note { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<int> CustID { get; set; }
    }
    
}
