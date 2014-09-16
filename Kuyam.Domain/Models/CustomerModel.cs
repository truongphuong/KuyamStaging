using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Database;

namespace Kuyam.Domain.Models
{
    public class CustomerModel
    {
        public int CustId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public CustomerModel(Cust customer)
        {
            CustId = customer.CustID;
            FirstName = customer.FirstName;
            LastName = customer.LastName;
        }
    }
}
