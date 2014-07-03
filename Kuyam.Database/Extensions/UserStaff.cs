using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class UserStaff
    {
        public int id { get; set; }
        public Guid AspUserID { get; set; }
        public int CustId { get; set; }
        public string Username { get { return DAL.GetUsername(this.AspUserID); } }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Status { get; set; }
        public Hotel Hotel { get; set; }
    }
}
