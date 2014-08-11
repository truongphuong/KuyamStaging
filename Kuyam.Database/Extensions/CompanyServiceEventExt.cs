using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database.Extensions
{
    public class CompanyServiceEventExt
    {
        public int ID { get; set; }
        public int CompanyEventID { get; set; }
        public int ServiceCompanyID { get; set; }
        public int ServiceTypeId { get; set; }    
        public string Description { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string ServiceName { get; set; }
        public string CategoryName { get; set; }
    }
}
