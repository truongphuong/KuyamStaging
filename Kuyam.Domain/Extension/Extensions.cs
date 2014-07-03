using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain
{
    public static class Extensions
    {
        public static void Encrypt(this aspnet_Membership membership)
        {
            //membership.Password = SecurityHelper.CreatePasswordHash(membership.Password, membership.PasswordSalt);
        }       
    }
}
