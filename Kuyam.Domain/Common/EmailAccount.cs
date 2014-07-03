using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Kuyam.Domain
{
    public class EmailAccount
    {
        public EmailAccount() {
            this.Host = ConfigurationManager.AppSettings["Smtp.Host"];
            this.Port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
            this.UseDefaultCredentials = false;
            this.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["Smtp.UseSSL"]);
            this.Username = ConfigurationManager.AppSettings["Smtp.UserName"];
            this.Password = ConfigurationManager.AppSettings["Smtp.Password"];
        }
        
        public virtual string Email { get; set; }
               
        public virtual string DisplayName { get; set; }
       
        public virtual string Host { get; set; }
       
        public virtual int Port { get; set; }
       
        public virtual string Username { get; set; }
      
        public virtual string Password { get; set; }
        
        public virtual bool EnableSsl { get; set; }
       
        public virtual bool UseDefaultCredentials { get; set; }       
        
    }
}
