using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Domain.SupEntity
{
    public class ConnectorSource 
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public string RefressToken { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime ExpiresDate { get; set; }
    
    }
}
