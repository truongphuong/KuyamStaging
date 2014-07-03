using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Kuyam.Database.BlogModels
{
    public class BlogUser
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string EmailAddress { get; set; }
        public string PhotoUrl { get; set; }
        public string AboutMe { get; set; }
        public bool IsPrivate { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Pinterest { get; set; }
        public string Website { get; set; }
    }
}
