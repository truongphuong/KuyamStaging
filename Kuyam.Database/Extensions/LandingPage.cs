using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database
{
    public partial class LandingPage
    {
        public Types.LandingPageStatus StatusEnum
        {
            get { return (Types.LandingPageStatus) Status; }
            set { Status = (int) value; }
        }
    }
}
