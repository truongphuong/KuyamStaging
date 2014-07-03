using Kuyam.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.CompanyProfileServices
{
 
    public class CompanyServiceComparer : IEqualityComparer<CompanyService>
    {
        public bool Equals(CompanyService x, CompanyService y)
        {
            if (x.ID == y.ID
                &&
                x.ServiceID == y.ServiceID
                &&
                x.ServiceName == y.ServiceName
                &&
                x.AttendeesNumber == y.AttendeesNumber
                &&
                x.Price == y.Price
                &&
                x.Duration == y.Duration
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(CompanyService obj)
        {
            return obj.ID.GetHashCode();
        }
    }

  
}
