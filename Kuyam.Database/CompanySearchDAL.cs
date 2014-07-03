using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Kuyam.Database
{
    public static class CompanySearchDAL
    {
        private const string Key = "Kuyam.Repository.Base.HttpContext.Key";

        public static kuyamEntities DBContext
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Items[Key] == null)
                        HttpContext.Current.Items[Key] = new kuyamEntities(DAL.GetConnectionString());
                    return (kuyamEntities)HttpContext.Current.Items[Key];
                }

                return new kuyamEntities(DAL.GetConnectionString());
            }
        }

        public static List<ProfileCompany> GetListCompanyIDs(int serviceId, decimal priceFrom, decimal priceTo,
                                                             DateTime hourFrom, DateTime hourTo, bool isToday,
                                                             string key)
        {
            try
            {
                var ctx = DBContext;
                var endDay = DateTime.Now.AddDays(1).Day;

                var profileIdWithPrice = new List<int>();
                var profileIdWithHour = new List<int>();
                var profileIdToday = new List<int>();
                var profileIDName = new List<int>();

                if (priceFrom != 0 || priceTo != 0)
                {
                    profileIdWithPrice = GetCompanyIDsFromPriceToPrice(serviceId, priceFrom, priceTo);
                }

                if (hourFrom != DateTime.Today || hourTo != DateTime.Today)
                {
                    profileIdWithHour = GetCompanyIDsFromHourToHour(serviceId, hourFrom, hourTo);
                }

                if (isToday)
                {
                    profileIdToday = GetCompanyIDsAvailableToday(serviceId, isToday);
                }
                if (key != null && key != string.Empty)
                {
                    profileIDName = GetCompanyIdsFromCompanyName(serviceId, key);

                }

                var fpcs = (from pc in ctx.ProfileCompanies
                            join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID into joinProfileCompanies
                            from j in joinProfileCompanies.DefaultIfEmpty()

                            join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                            where
                                (serviceId == 0 || j.ServiceID == serviceId) &&
                                pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                                &&
                                ((priceFrom == 0 && priceTo == 0) ||
                                 (profileIdWithPrice.Contains(pc.ProfileID) == true))
                                &&
                                ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                 (profileIdWithHour.Contains(pc.ProfileID) == true))
                                && (!isToday || (profileIdToday.Contains(pc.ProfileID) == true))
                                && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                            select pc).Distinct().OrderBy(x => x.Name).ToList();

                //------------------------------------------------------------------------
                var tempFeatureListWithIsToday = new List<ProfileCompany>();
                var tempFeatureListWithViewAvailable = new List<ProfileCompany>();
                var tempFeatureList = new List<ProfileCompany>();
                foreach (ProfileCompany pc in fpcs)
                {
                    if (isAvailableToday(pc.ProfileID))
                    {
                        tempFeatureListWithIsToday.Add(pc);
                    }

                    if (isViewAvailability(pc.ProfileID))
                    {
                        tempFeatureListWithViewAvailable.Add(pc);
                    }
                    else
                    {
                        tempFeatureList.Add(pc);
                    }
                }
                fpcs = tempFeatureListWithIsToday.Union(tempFeatureListWithViewAvailable).ToList();
                fpcs = fpcs.Union(tempFeatureList).Distinct().ToList();

                List<int> fpcIDs = fpcs.Select(x => x.ProfileID).ToList();

                var aPCs = (from pc in ctx.ProfileCompanies
                            join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                            where
                                (pcs.ServiceID == serviceId || serviceId == 0) &&
                                pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                                &&
                                ((priceFrom == 0 && priceTo == 0) ||
                                 (profileIdWithPrice.Contains(pcs.ProfileID) == true))
                                &&
                                ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                 (profileIdWithHour.Contains(pcs.ProfileID) == true))
                                && (profileIdToday.Contains(pcs.ProfileID) == true)
                                && fpcIDs.Contains(pcs.ProfileID) == false
                                && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                            select pc).Distinct().OrderBy(x => x.Name).ToList();
                List<int> aPCIDs = aPCs.Select(x => x.ProfileID).ToList();


                var avPCs = (from pc in ctx.ProfileCompanies
                             join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                             join ce in ctx.CompanyEmployees on pc.ProfileID equals ce.ProfileCompanyID
                             join eh in ctx.EmployeeHours on ce.EmployeeID equals eh.CompanyEmployeeID
                             where
                                 (pcs.ServiceID == serviceId || serviceId == 0) &&
                                 pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                                 &&
                                 ((priceFrom == 0 && priceTo == 0) ||
                                  (profileIdWithPrice.Contains(pcs.ProfileID) == true))
                                 &&
                                 ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                  (profileIdWithHour.Contains(pcs.ProfileID) == true))
                                 && (!isToday || (profileIdToday.Contains(pcs.ProfileID) == true))
                                 && fpcIDs.Contains(pcs.ProfileID) == false
                                 && aPCIDs.Contains(pcs.ProfileID) == false
                                 && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                             select pc).Distinct().OrderBy(x => x.Name).ToList();
                List<int> avPCIDs = avPCs.Select(x => x.ProfileID).ToList();

                var pCsQuery = (from pc in ctx.ProfileCompanies
                    join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID into joinProfileCompanies
                    from j in joinProfileCompanies.DefaultIfEmpty()

                    where
                        (serviceId == 0 || j.ServiceID == 0 || j.ServiceID == serviceId) &&

                        (pc.CompanyStatusID == (int) Types.CompanyStatus.Active)

                        &&
                        ((priceFrom == 0 && priceTo == 0) || (profileIdWithPrice.Contains(j.ProfileID) == true))
                        &&
                        ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                         (profileIdWithHour.Contains(j.ProfileID) == true))
                        && (!isToday || (profileIdToday.Contains(j.ProfileID) == true))
                        && (string.IsNullOrEmpty(key) || profileIDName.Contains(j.ProfileID) == true)
                    select pc).Distinct().OrderBy(x => x.Name);
                var pCs = pCsQuery.ToList();

                var ret = fpcs.Union(aPCs).Distinct().ToList();
                ret = ret.Union(avPCs).Distinct().ToList();
                ret = ret.Union(pCs).Distinct().ToList();

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool isAvailableToday(int profileCompanyId)
        {
            var ctx = DBContext;
            var shList = (from eh in ctx.EmployeeHours
                          join ce in ctx.CompanyEmployees on eh.CompanyEmployeeID equals ce.EmployeeID
                          where ce.ProfileCompanyID == profileCompanyId
                          select eh).Distinct().ToList();
            if (shList.Count == 0)
                return false;
            DateTime dtNow = DateTime.UtcNow;
            DateTime dtInit = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 1);
            var svHours =
                shList.Where(
                    s => s.DayOfWeek.ToString().Contains(((int)dtNow.DayOfWeek).ToString(CultureInfo.InvariantCulture)) && (!s.IsPreview)).ToList();

            foreach (var svHour in svHours)
            {
                double minuteToEnd = (dtInit.AddHours(svHour.ToHour.Hours) - dtNow).TotalMinutes;
                if (minuteToEnd > 10)
                    return true;
            }
            return false;
        }

        public static bool isViewAvailability(int profileCompanyID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from eh in ctx.EmployeeHours
                       join ce in ctx.CompanyEmployees on eh.CompanyEmployeeID equals ce.EmployeeID
                       where ce.ProfileCompanyID == profileCompanyID
                       select eh).Count();
            if (ret > 0)
                return true;
            return false;
        }

        public static List<int> GetCompanyIDsFromPriceToPrice(int serviceId, decimal fromPrice, decimal toPrice)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        where (scp.ServiceID == serviceId || serviceId == 0)
                              && (scp.Price > fromPrice && scp.Price <= toPrice) && scp.Status != 1
                        select pc.ProfileID).Distinct().ToList();
            return ret1;
        }

        public static List<int> GetCompanyIDsFromHourToHour(int serviceID, DateTime fromHour, DateTime toHour)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join es in ctx.EmployeeServices on scp.ServiceCompanyID equals es.ServiceCompanyID
                        join ce in ctx.CompanyEmployees on es.CompanyEmployeeID equals ce.EmployeeID
                        join eh in ctx.EmployeeHours on ce.EmployeeID equals eh.CompanyEmployeeID
                        where (scp.ServiceID == serviceID || serviceID == 0)
                              &&
                              ((eh.FromHour.Hours >= fromHour.Hour && eh.FromHour.Hours < toHour.Hour) ||
                               (eh.ToHour.Hours > fromHour.Hour && eh.ToHour.Hours <= toHour.Hour) ||
                               (eh.FromHour.Hours <= fromHour.Hour && eh.ToHour.Hours >= toHour.Hour))
                        select pc.ProfileID).Distinct().ToList();
            return ret1;
        }

        public static List<int> GetCompanyIDsAvailableToday(int serviceId, bool isToday)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        where scp.ServiceID == serviceId || serviceId == 0
                        select pc.ProfileID).Distinct().ToList();
            List<int> idList = new List<int>();
            foreach (int id in ret1)
            {
                if (isAvailableToday(id))
                    idList.Add(id);
            }
            return idList;
        }

        public static List<int> GetCompanyIdsFromCompanyName(int serviceID, string key)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        where (scp.ServiceID == serviceID || serviceID == 0)
                              && pc.Name.Contains(key) == true
                        select pc.ProfileID).Distinct().ToList();
            return ret1;
        }

        public static List<CompanyProfileSearch> GetProfileCompaniesWebSite(int serviceID, decimal? fromPrice, decimal? toPrice
            , DateTime? fromDate, DateTime? toDate, bool isToday, string keySearch, double currentLat, double currentLong
            , double distance, int custID, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                if (keySearch == null) keySearch = string.Empty;

                var ctx = DBContext;
                var totalItemParam = new SqlParameter("TotalItems", 0);
                totalItemParam.Direction = ParameterDirection.Output;
                var data = ctx.Database.SqlQuery<CompanyProfileSearch>(
                    "GetProfileCompaniesWebSite @ServiceID, @FromPrice, @ToPrice, @FromDate, @ToDate, @IsToDay, @KeySearch, @CurrentLat, @CurrentLong, @Distance, @CustID, @Skip, @Take, @TotalItems out",
                    new SqlParameter("ServiceID", serviceID),
                    new SqlParameter("FromPrice", fromPrice),
                    new SqlParameter("ToPrice", fromPrice),
                    new SqlParameter("FromDate", fromDate),
                    new SqlParameter("ToDate", toDate),
                    new SqlParameter("IsToday", isToday),
                    new SqlParameter("KeySearch", keySearch),
                    new SqlParameter("CurrentLat", Convert.ToSingle(currentLat)),
                    new SqlParameter("CurrentLong", Convert.ToSingle(currentLong)),
                    new SqlParameter("Distance", Convert.ToSingle(distance)),
                    new SqlParameter("CustID", custID),
                    new SqlParameter("Skip", skip),
                    new SqlParameter("Take", take),
                    totalItemParam
                    );

                var result = data.ToList();
                totalItems = Convert.ToInt32(totalItemParam.Value);

                return result;
            }
            catch (Exception ex)
            {
                return new List<CompanyProfileSearch>();
            }
        }

        public static List<Appointment> GetAppoinmentsByProfileId(int profileId, DateTime startTime, DateTime endTime)
        {
            var ctx = DBContext;
            DateTime temp = DateTime.UtcNow.AddMinutes(-10);
            return ctx.Appointments.Where(a => a.CompanyEmployee.ProfileCompanyID == profileId
                && ((a.Start >= startTime && a.Start < endTime) || (a.End > startTime && a.End <= endTime))
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Unknown
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled // valid appointment does not have cancel , delete and temporary pending which have  the created time more than 10 mins 
                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && (a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp <= a.Created)
                ).ToList();
        }
    }
}
