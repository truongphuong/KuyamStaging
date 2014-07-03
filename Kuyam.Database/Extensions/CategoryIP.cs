using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class CategoryIP
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string Desc { get; set; }
        public Nullable<int> ParentServiceID { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<bool> Status { get; set; }
        public string KalturaId { get; set; }
    
    }
}
