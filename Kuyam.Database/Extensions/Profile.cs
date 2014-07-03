using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	public partial class Profile
	{
		public static Profile Load(int id)
		{
			return DAL.GetProfile(id);
		}

		public static Profile Create(Types.CustType ptype, int custID, string name = null, int? relationshipID = 0)
		{
			Profile p = new Profile();
			p.CustID = custID;
            p.Created = DateTime.Now;
			if (ptype == Types.CustType.Personal)
			{
				p.Name = "Personal";
				p.PrivacyTypeID = (int)Types.PrivacyType.Private;
				p.ProfileTypeID = (int)Types.CustType.Personal;
				p.RelationshipTypeID = (int)Types.RelationshipType.Other;
			}
			else
			{
				p.Name = "Company";
				p.PrivacyTypeID = (int)Types.PrivacyType.Public;
				p.ProfileTypeID = (int)Types.CustType.Company;
				p.RelationshipTypeID = (int)Types.RelationshipType.Company;
			}

			if (name != null)
				p.Name = name;

			if (relationshipID.HasValue && relationshipID.Value > 0)
				p.RelationshipTypeID = relationshipID.Value;

			p.IsDefault = false;

			DAL.CreateProfile(p);
			return p;
		}

		public static ProfileCompany LoadCompany(int id)
		{
			return DAL.GetProfile(id).ProfileCompany;
		}

		public static Calendar GetDefaultCalendar(int profileID)
		{
			return DAL.GetCalendarsForProfile(profileID).FirstOrDefault();
		}

		public const int UNVERIFIED_COMPANY_CUSTID = 63;

        public ProfileCompany CreateCompany(Types.CompanyCategory coType, string name, string phone, Types.CompanyStatus coStatus, string zipCode)
		{
			ProfileCompany pc = new ProfileCompany();
			pc.ProfileID = ProfileID;
			pc.CompanyTypeID = (int)coType;
			pc.CompanyStatusID = (int)coStatus;
			pc.Name = name;
			pc.Phone = phone;
            pc.Zip = zipCode;
            pc.Created = DateTime.Now;
			pc.ApptDefaultPeoplePerSlot = 1;
			pc.ApptDefaultSlotDuration = 60;
			DAL.CreateCompany(pc);

			return pc;
		}

		public static ProfileCompany CreateUnverifiedCompany(ProfileCompany pc)
		{
			Profile p = Profile.Create(Types.CustType.Company, UNVERIFIED_COMPANY_CUSTID, pc.Name);
			Calendar c = Calendar.Create(Types.CustType.Company, p.ProfileID, pc.Name, true);
			pc.ProfileID = p.ProfileID;
			DAL.CreateCompany(pc);

			return pc;
		}

		public static List<Cust_Company> GetInactiveAppointmentCompanies()
		{
			List<Cust_Company> ret = DAL.GetInactiveAppointmentCompanies();
			return ret;
		}

		public void SetFeaturedCompany()
		{
			FeaturedCompany fc = new FeaturedCompany();
			fc.ProfileID = ProfileID;
			fc.StartDT = DateTime.Now;
			DAL.AddFeaturedCompany(fc);
		}

		public List<Calendar> GetCalendars()
		{
			return DAL.GetCalendarsForProfile(ProfileID).ToList();
		}

		public Calendar GetDefaultCalendar()
		{
			return DAL.GetCalendarsForProfile(ProfileID).FirstOrDefault();
		}
	}
}
