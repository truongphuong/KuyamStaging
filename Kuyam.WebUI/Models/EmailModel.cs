using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
    public class EmailModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string Url { get; set; }
        public bool IsHidden { get; set; }
        public string PostTitle { get; set; }
    }
}