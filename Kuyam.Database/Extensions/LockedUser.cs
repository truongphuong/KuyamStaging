using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class LockedUser
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LockedStatus { get; set; }
    }
}
