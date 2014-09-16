using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.CompanyProfileServices
{
    public class CompanyHourComparer: IComparer<CompanyHour>
    {
        public int Compare(CompanyHour x, CompanyHour y)
        {
            if (x.DayOfWeek < y.DayOfWeek)
            {
                if (x.DayOfWeek == 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (x.DayOfWeek > y.DayOfWeek)
                {
                    if (y.DayOfWeek == 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    //x.DayOfWeek == y.DayOfWeek
                    //can't duplicate from hour
                    if (x.FromHour < y.FromHour)
                    {
                        return -1;
                    }
                    return 1;
                }
            }
        }
    }
}
