using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.WebUI.Models;
using System.Web.Security;
using System.Text;
using M2.Util;
using Kuyam.Domain;

namespace Kuyam.WebUI.Controllers
{
	[Authorize(Roles = "personal, admin, support, god")]
    public partial class CustController : KuyamBaseController
	{
		public void SetupCustMaster()
		{
			ViewBag.CustID = MySession.CustID;
		}

		// GET: /Cust/
		public ActionResult Index()
		{
			CustHomeModel m = new CustHomeModel();
			m.CustID = MySession.CustID;
			m.FeaturedCompanyModel.MediaID = 4;
			m.LockAndLoad();
			SetupCustMaster();
			return View(m);
		}

		public ActionResult Notepad()
		{
			try
			{
				NotepadModel m = new NotepadModel();
				m.Notes = MySession.Cust.Notes;
				m.LockAndLoad();
				SetupCustMaster();
				return View(m);
			}
			catch
			{
				return View();
			}
		}

		public ActionResult Help()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Notepad(NotepadModel model)
		{
			try
			{
				Cust c = MySession.Cust;
				c.Notes = model.Notes;
				c.Update();
				return View(model); // RedirectToAction("index", "cust");
			}
			catch
			{
				SetupCustMaster();
				return View(model);
			}
		}

		// ProfileCompany view for search results, ec.
		[ActionName("company-view")]
		public ActionResult Company_View(int id)
		{
			ProfileCompany pc = Kuyam.Database.Profile.LoadCompany(id);
			CompanyViewModel model = new CompanyViewModel();
			model.Company = pc;
			model.LockAndLoad();
			return View("company-view", model);
		}

		[ActionName("company-search")]
		public ActionResult Company_Search()
		{
			CompanySearchModel model = new CompanySearchModel();
			model.LockAndLoad();
			SetupCustMaster();
			return View(model);
		}

		[HttpPost]
		[ActionName("company-search")] 
		public ActionResult Company_Search(string searchTerms)
		{
			CompanySearchModel model = new CompanySearchModel();
			model.SearchTerms = searchTerms;
            //main page doesn't need to load search anymore
			//model.Results = DAL.GetCompanySearchResults(searchTerms);
			//model.LockAndLoad();
			return View(model);
		}

		[ActionName("company-search2")]
		public ActionResult Company_Search2(string searchTerms)
		{
			CompanySearchModel model = new CompanySearchModel();
			model.SearchTerms = searchTerms;
			model.Results = DAL.GetCompanySearchResults(model.SearchTerms);
			//model.LockAndLoad();
            return GetJsonCompanies(model.Results);
			//return View("Company_search", model);
		}

        public ActionResult Company(int id)
        {
            ProfileCompany pc = Kuyam.Database.Profile.LoadCompany(id);
            return Json(GetStrippedCompany(pc), JsonRequestBehavior.AllowGet);
        }

        private JsonResult GetJsonCompanies(List<ProfileCompany> companies)
        {
            //return the stripped down object
            List<Object> result = new List<object>();
            foreach (ProfileCompany company in companies)
                result.Add(GetStrippedCompany(company));

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private object GetStrippedCompany(ProfileCompany company)
        {
            return new
            {
                Name = company.Name,
                ProfileID = company.ProfileID,
                AddressLine = GetCompanyAddressLine(company),
                //ProfileHours = GetStrippedHours(company.Profile.ProfileHours),
                CityState = company.City + ", " + company.State,
            };
        }

        private List<object> GetStrippedHours(ICollection<ProfileHour> hours)
        {
            List<object> result = new List<object>();

            ProfileHour sunday = hours.FirstOrDefault(hour => hour.Day == 121);
            ProfileHour monday = hours.FirstOrDefault(hour => hour.Day == 122);
            ProfileHour tuesday = hours.FirstOrDefault(hour => hour.Day == 123);
            ProfileHour wednesday = hours.FirstOrDefault(hour => hour.Day == 124);
            ProfileHour thursday = hours.FirstOrDefault(hour => hour.Day == 125);
            ProfileHour friday = hours.FirstOrDefault(hour => hour.Day == 126);
            ProfileHour saturday = hours.FirstOrDefault(hour => hour.Day == 127);
            ProfileHour weekdays = hours.FirstOrDefault(hour => hour.Day == 128);
            ProfileHour weekend = hours.FirstOrDefault(hour => hour.Day == 129);
            ProfileHour everyday = hours.FirstOrDefault(hour => hour.Day == 130);

            if (everyday != null)
            {
                result.Add(new { Day = "m", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "t", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "w", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "th", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "f", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "sa", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
                result.Add(new { Day = "su", Time = everyday.Start.ToHHMMString() + " - " + everyday.End.ToHHMMString()});
            }
            else 
            {
                if (weekdays != null)
                {
                    result.Add(new { Day = "m", Time = weekdays.Start.ToHHMMString() + " - " + weekdays.End.ToHHMMString() });
                    result.Add(new { Day = "t", Time = weekdays.Start.ToHHMMString() + " - " + weekdays.End.ToHHMMString() });
                    result.Add(new { Day = "w", Time = weekdays.Start.ToHHMMString() + " - " + weekdays.End.ToHHMMString() });
                    result.Add(new { Day = "th", Time = weekdays.Start.ToHHMMString() + " - " + weekdays.End.ToHHMMString() });
                    result.Add(new { Day = "f", Time = weekdays.Start.ToHHMMString() + " - " + weekdays.End.ToHHMMString() });
                }
                else
                {
                    if (monday != null) result.Add(new { Day = "m", Time = monday.Start.ToHHMMString() + " - " + monday.End.ToHHMMString() });
                    if (tuesday != null) result.Add(new { Day = "t", Time = tuesday.Start.ToHHMMString() + " - " + tuesday.End.ToHHMMString() });
                    if (wednesday != null) result.Add(new { Day = "w", Time = wednesday.Start.ToHHMMString() + " - " + wednesday.End.ToHHMMString() });
                    if (thursday != null) result.Add(new { Day = "th", Time = thursday.Start.ToHHMMString() + " - " + thursday.End.ToHHMMString() });
                    if (friday != null) result.Add(new { Day = "f", Time = friday.Start.ToHHMMString() + " - " + friday.End.ToHHMMString() });
                }

                if (weekend != null)
                {
                    result.Add(new { Day = "sa", Time = weekend.Start.ToHHMMString() + " - " + weekend.End.ToHHMMString() });
                    result.Add(new { Day = "su", Time = weekend.Start.ToHHMMString() + " - " + weekend.End.ToHHMMString() });
                }
                else
                {
                    if (saturday != null) result.Add(new { Day = "sa", Time = saturday.Start.ToHHMMString() + " - " + saturday.End.ToHHMMString() });
                    if (sunday != null) result.Add(new { Day = "su", Time = sunday.Start.ToHHMMString() + " - " + sunday.End.ToHHMMString() });
                }
            }

            return result;
        }

        private string GetCompanyAddressLine(ProfileCompany company)
        {
            string address = company.Street1;
            if (!String.IsNullOrEmpty(company.Street2))
                address += " " + company.Street2;
            if (!String.IsNullOrEmpty(company.City))
            {
                if (!String.IsNullOrEmpty(company.State))
                    address += " " + company.City + ",";
                else
                    address += " " + company.City;
            }
            if (!String.IsNullOrEmpty(company.State))
                address += " " + company.State;

            address += " " + company.Phone;

            return address;
        }

        [HttpPost]
		[ActionName("calendar-search")] 
		public ActionResult Calendar_Search(string searchTerms)
		{
			CalendarSearchModel model = new CalendarSearchModel();
			model.SearchTerms = searchTerms;
			model.Results = DAL.GetCalendarSearchResults(model.SearchTerms, MySession.CustID);
			model.LockAndLoad();
			return View("calendar_search", model);
		}

        [ActionName("dashboardviewcompanysearch")]
        public ActionResult dashboardviewcompanysearch(string companysearch)
        {
            CompanySearchModel model = new CompanySearchModel();
            model.SearchTerms = companysearch;
            //model.Results = DAL.GetCompanySearchResults(companysearch);
            //model.LockAndLoad();
            return View(model);
            //return View(useLayout);
        }


        [ActionName("dashboardviewMyAppointment")]
        public ActionResult dashboardviewMyAppointment(int id)
        {
            Appointment appt = DAL.GetAppointment(id);
            return View(appt);
        }
        //public ActionResult dashboardviewcompanysearch(bool useLayout = false)
        //{
        //    return View();
        //}

		public ActionResult Calendar(int id)
		{
			return RedirectToAction("index");
		}	

	}
}
