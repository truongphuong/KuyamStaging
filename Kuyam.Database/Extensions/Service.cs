using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public partial class Service
    {
        public string StatusStr
        {
          
            get
            {
                if (this.Status == null)
                    Status = true;
                return Status.Value? "active":"inactive";
            }
       
        }

        public string ParentName
        {
            get
            {
                return DAL.GetCategoryNameFromCategoryId(ParentServiceID ?? 0);
            }
        }
    }
}
