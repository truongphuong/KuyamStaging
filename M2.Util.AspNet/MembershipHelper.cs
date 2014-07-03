using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace M2.Util.AspNet
{
	public static class MembershipHelper
	{
		public static void DeleteAllUsers()
		{
			foreach (MembershipUser u in Membership.GetAllUsers())
			{
				Membership.DeleteUser(u.UserName, true);
			}
		}

		public static void DeleteAllRoles()
		{
			foreach (string role in Roles.GetAllRoles())
			{
				Roles.DeleteRole(role);
			}
		}

		public static void AddUserToRole(string username, string rolename)
		{
			Roles.AddUserToRole(username, rolename);
		}

		public static List<string> GetUsersInRole(string rolename)
		{
			return Roles.GetUsersInRole(rolename).ToList();
		}
	}
}
