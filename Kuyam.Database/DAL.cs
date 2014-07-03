using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using M2.Util;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Kuyam.Database.Extensions;
using System.Configuration;
using System.Data;
using System.Web;
using System.Collections;
using System.Threading;
using System.Data.Entity;

// DAL: Must directly access DBContext.  If not (or it only references other functions in DAL) then it should be in the domain.

namespace Kuyam.Database
{
    public static class kuyamEntitiesExt
    {
        public static void HandleException(DbEntityValidationException dbEx)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    sb.AppendFormat("Property \"{0}\": {1}\r\n", validationError.PropertyName,
                                    validationError.ErrorMessage);
                }
            }

            throw new ApplicationException("[kuyamEntitiesExt.Save] ERROR! " + sb.ToString(), dbEx);
        }

        public static void HandleException(DbUpdateException ex)
        {
            throw ex.Innermost();
        }

        public static void Save(this kuyamEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                HandleException(dbEx);
            }
            catch (Exception ex)
            {
                throw ex.Innermost();
            }
        }
    }

    // TODO: Move to domain and make "internal" - UI should be calling domain methods, NOT directly to DAL which can change
    public static class DAL
    {
        private static string _connectionString = string.Empty;

        public static string GetConnectionString()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                var conectionString = ConfigurationManager.ConnectionStrings["kuyamEntities"].ConnectionString;
                var connectionStringBuilder = new SqlConnectionStringBuilder(conectionString);
                _connectionString = string.Format("metadata=res://*/KuyamDBModels.csdl|res://*/KuyamDBModels.ssdl|res://*/KuyamDBModels.msl;provider=System.Data.SqlClient;provider connection string=\"data source={0};initial catalog={1}; User ID={2};Password={3};multipleactiveresultsets=True;App=EntityFramework\"",
                connectionStringBuilder.DataSource, connectionStringBuilder.InitialCatalog,
                connectionStringBuilder.UserID, connectionStringBuilder.Password);
            }
            return _connectionString;
        }


        //public static kuyamEntities DBContext
        //{
        //    get
        //    {
        //        return new kuyamEntities(GetConnectionString());
        //    }

        //}


        private static readonly string Key = "Kuyam.Repository.Base.HttpContext.Key";
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


        static DAL()
        {
        }

        private static Dictionary<int, StandardTypeList> _typeCache = new Dictionary<int, StandardTypeList>();

        public static void FlushCache()
        {
            _typeCache = new Dictionary<int, StandardTypeList>();
        }


        public static StandardTypeList GetTypes(int typeGroup)
        {
            StandardTypeList ret = null;
            if (!_typeCache.TryGetValue(typeGroup, out ret))
            {
                ret = new StandardTypeList();

                foreach (var t in DBContext.Types.Where(g => g.TypeGroupID == (int)typeGroup).OrderBy(g => g.Sequence))
                {
                    ret.Add(t.Name, t.TypeID);
                }

                _typeCache[typeGroup] = ret;
            }

            return ret;
        }

        public static List<Type> GetCarrier(int typeGroup)
        {
            return DBContext.Types.Where(g => g.TypeGroupID == (int)typeGroup).OrderBy(g => g.Sequence).ToList();
        }

        public static StandardTypeList GetTypeGroups()
        {
            StandardTypeList ret = new StandardTypeList();

            foreach (var t in DBContext.TypeGroups.OrderBy(g => g.Name))
            {
                ret.Add(t.Name, t.TypeGroupID);
            }

            return ret;
        }

        public static Setting GetSetting(string name)
        {
            return DBContext.Settings.SingleOrDefault(setting => setting.Name == name);
        }

        public static Dictionary<string, string> GetSettings()
        {
            var dbContext = DBContext;
            return dbContext.Settings.ToDictionary(k => k.Name, v => v.Value);
        }

        public static void SaveSettings(Dictionary<string, string> settings)
        {
            foreach (string key in settings.Keys)
            {
                Setting s = GetSetting(key);
                s.Value = settings[key];
                UpdateRec(s, s.Name);
            }
        }

        public static void SetSetting(string name, string value)
        {
            throw new NotImplementedException();
        }

        /*
        public static Account GetAccount(string username)
        {
            //kuyamEntities dbContext = DBContext;
            //var query = from u in dbContext.aspnet_Users
            //            join c in dbContext.Custs on u.UserId equals c.AspUserID
            //            join a in dbContext.Accounts on c.AccountID equals a.AccountID
            //            where u.UserName == username
            //             && c.Status == (int)Types.UserStatusType.Active
            //            select a;
            //return query.FirstOrDefault();

            Cust c = xGetCust(username);
            if (c == null)
                return null;
            Account acct = (from a in DBContext.Accounts where a.AccountID == c.AccountID select a).FirstOrDefault();
            return acct;
        }
        */
        public static Cust xGetCust(string username)
        {
            kuyamEntities dbContext = DBContext;
            Guid g = GetAspUserID(username);
            if (g == Guid.Empty)
                return null;
            return DBContext.Custs.SingleOrDefault(cu => cu.AspUserID == g && cu.Status == (int)Types.UserStatusType.Active);
        }

        public static Cust GetCustByGuid(Guid userID)
        {
            return DBContext.Custs.SingleOrDefault(cu => cu.AspUserID == userID);
        }

        public static List<Cust> xGetCusts()
        {
            kuyamEntities ctx = DBContext;

            //// get email, but email in other table and full object for this.  time for bed.
            //List<Cust> custs = 
            //    (from c in ctx.Custs
            //         join u in ctx.aspnet_Users on u.userid == c.AspUserID

            //             ).ToList();

            List<Cust> custs = ctx.Custs.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
            return custs;
        }

        public static Cust xGetCust(int custID)
        {
            var query = DBContext.Custs.Where(c => c.CustID == custID);
            var cust = query.FirstOrDefault();
            return cust;
        }

        public static HotelStaff GetConcierge(int custID, int hotelId = 0)
        {
            var query = DBContext.HotelStaffs.Where(h => h.CustID == custID && (hotelId == 0 || h.HotelID == hotelId));
            var cust = query.OrderByDescending(m => m.Id).FirstOrDefault();
            return cust;
        }

        public static HotelStaff GetConcierge(int custID)
        {
            var query = DBContext.HotelStaffs.Where(h => h.CustID == custID);
            var cust = query.OrderByDescending(m => m.Id).FirstOrDefault();
            return cust;
        }

        public static HotelVisit GetHotelVisit(int custID)
        {
            return DBContext.HotelVisits.Where(h => h.CustID == custID).FirstOrDefault();
        }

        public static int GetCustIDFromUserID(Guid userID)
        {
            Cust cust = DBContext.Custs.Where(x => x.AspUserID == userID).FirstOrDefault();
            return cust.CustID;
        }

        public static Profile GetDefaultProfile(int custid)
        {
            return
                DBContext.Profiles.Where(
                    x => x.CustID == custid && x.ProfileCompany.CompanyStatusID != (int)Types.CompanyStatus.Deleted).
                    FirstOrDefault();
        }

        public static Calendar GetDefaultCalendarForProfile(int profileid)
        {
            return DBContext.Calendars.Where(x => x.ProfileID == profileid && x.IsDefault == true).FirstOrDefault();
        }

        public static Profile GetProfile(int pid)
        {
            return DBContext.Profiles.Where(x => x.ProfileID == pid).FirstOrDefault();
        }

        public static List<int> GetCustCalendarIDs(int custID)
        {
            kuyamEntities ctx = DBContext;
            var ret = from cal in ctx.Calendars
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID
                      select cal.CalendarID;
            return ret.ToList();
        }

        public static AppointmentParticipant GetCustAppointmentParticipant(int custID, int apptID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from ap in ctx.AppointmentParticipants
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       where ap.AppointmentID == apptID && pro.CustID == custID
                       select ap).FirstOrDefault();

            return ret;
        }

        public static AppointmentParticipant GetAppointmentParticipantByProfile(int apptID, int profileID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from ap in ctx.AppointmentParticipants
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       where ap.AppointmentID == apptID && pro.ProfileID == profileID
                       select ap).FirstOrDefault();

            return ret;
        }


        public static void DeleteAppointmentParticipant(int apID)
        {
            DBContext.Database.ExecuteSqlCommand("delete from appointmentparticipant where appointmentparticipantid=" +
                                                 apID);
        }


        public static IQueryable<Appointment> GetCustAppointments(int custID)
        {
            return GetCustAppointments(custID, DateTime.MinValue, DateTime.MaxValue);
        }

        public static IQueryable<Appointment> GetCustAppointments(int custID, DateTime start, DateTime end)
        {
            List<int> calIDs = GetCustCalendarIDs(custID);
            kuyamEntities ctx = DBContext;
            var ret =
                (from a in ctx.Appointments
                 join t in ctx.Types on a.AppointmentStatusID equals t.TypeID
                 join ap in ctx.AppointmentParticipants on a.AppointmentID equals ap.AppointmentID
                 orderby t.Sequence ascending, a.Start, a.Title
                 where
                     calIDs.Contains(ap.CalendarID) && a.Start >= start && a.Start <= end &&
                     a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                 select a);

            return ret;
            //return DBContext.Appointments.Where(x => x.CustID == custID).ToList();
        }

        public static List<Appointment> GetCalendarAppointments(int calID, DateTime start, DateTime end)
        {
            kuyamEntities ctx = DBContext;
            List<Appointment> ret =
                (from a in ctx.Appointments
                 join t in ctx.Types on a.AppointmentStatusID equals t.TypeID
                 join ap in ctx.AppointmentParticipants on a.AppointmentID equals ap.AppointmentID
                 orderby t.Sequence ascending, a.Start, a.Title
                 where
                     ap.CalendarID == calID && a.Start >= start && a.Start <= end &&
                     a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                 select a).ToList();

            return ret;
        }

        public static Calendar GetCalendar(int calID)
        {
            return DBContext.Calendars.FirstOrDefault(x => x.CalendarID == calID);
        }

        public static void CreateCompany(ProfileCompany pc)
        {
            kuyamEntities ctx = DBContext;
            ctx.ProfileCompanies.Add(pc);
            ctx.Save();
        }

        public static void AddFeaturedCompany(FeaturedCompany fc)
        {
            kuyamEntities ctx = DBContext;
            ctx.FeaturedCompanies.Add(fc);
            ctx.Save();
        }

        public static int GetCustIDFromCalendarID(int calID)
        {
            kuyamEntities ctx = DBContext;
            int custid = (from cal in ctx.Calendars
                          join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                          where cal.CalendarID == calID
                          select p.CustID).FirstOrDefault();
            return custid;
        }

        public static Appointment GetAppointment(int apptID)
        {
            return DBContext.Appointments.Where(x => x.AppointmentID == apptID).FirstOrDefault();
        }

        public static void CreateAppointment(Appointment a)
        {
            kuyamEntities ctx = DBContext;
            a.Created = DateTime.Now;
            a.Modified = DateTime.Now;
            a.StatusChangeDate = DateTime.Now;
            ctx.Appointments.Add(a);
            ctx.Save();
        }

        public static void LogAppointmentChange(Appointment appt, string msg)
        {
            List<Cust> custs = DAL.GetAppointmentCusts(appt);
            kuyamEntities ctx = DBContext;

            foreach (Cust c in custs)
            {
                AppointmentLog log = new AppointmentLog();
                log.AppointmentID = appt.AppointmentID;
                log.Message = msg;
                log.CustID = c.CustID;
                ctx.AppointmentLogs.Add(log);
            }

            ctx.Save();
        }


        public static ProfileCompany GetFeaturedCompany()
        {
            var fcs = DBContext.FeaturedCompanies;

            if (fcs != null && fcs.Count() > 0)
            {
                FeaturedCompany fc = fcs.OrderByDescending(x => x.CompanyFeaturedID).First();
                return fc.Profile.ProfileCompany;
            }
            else
                return null;
        }

        public static List<FeaturedCompany> GetFeatureCompanies()
        {
            kuyamEntities ctx = DBContext;
            return ctx.FeaturedCompanies.ToList();
        }

        public static List<ProfileCompany> GetFeaturedCompanies()
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                       select pc).Distinct().ToList();
            return ret;
        }

        public static List<ProfileCompany> GetFeaturedCompaniesAtHomePage()
        {
            kuyamEntities ctx = DBContext;
            //var ret = (from fc in ctx.FeaturedCompanies
            //           where fc.priority > 0
            //           select fc).Distinct().OrderBy(x => x.priority).ToList();
            //List<int> fcIDs = ret.Select(x => x.ProfileID).ToList();

            //List<ProfileCompany> pcList = new List<ProfileCompany>();

            //foreach (int profileID in fcIDs)
            //{
            //    pcList.Add(ctx.ProfileCompanies.Where(x => x.ProfileID == profileID).FirstOrDefault());
            //}

            var query = from cpf in ctx.ProfileCompanies
                        join fcp in ctx.FeaturedCompanies on cpf.ProfileID equals fcp.ProfileID
                        where fcp.priority > 0 && cpf.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                        orderby fcp.priority
                        select cpf;

            foreach (ProfileCompany profile in query)
            {
                int totalReview = 0;
                double valueRanting = 0;


                if (profile.ServiceCompanies != null && profile.ServiceCompanies.Count > 0)
                {
                    foreach (ServiceCompany item in profile.ServiceCompanies)
                    {
                        totalReview += item.Ratings.Count;
                        valueRanting += item.Ratings.Sum(m => m.RatingValue).Value;
                    }
                }

                if (totalReview > 0)
                {
                    profile.TotalReview = totalReview;
                    profile.Rate = Math.Round((valueRanting / totalReview));
                }
            }

            return query.ToList();
        }

        public static List<ProfileCompany> GetCompanySearchResults(string terms)
        {
            if (terms.IsNullOrEmpty())
                return null;

            var ret =
                DBContext.ProfileCompanies.Where(
                    c =>
                    c.Name.ToLower().Contains(terms.ToLower()) &&
                    c.CompanyStatusID != (int)Types.CompanyStatus.Pending &&
                    c.CompanyStatusID != (int)Types.CompanyStatus.Deleted).ToList();
            return ret;
        }

        public static List<Appointment_Company> GetCalendarSearchResults(string terms, int custID)
        {
            if (terms.IsNullOrEmpty())
                return null;

            kuyamEntities ctx = DBContext;
            var ret = (from a in ctx.Appointments
                       join ap in ctx.AppointmentParticipants on a.AppointmentID equals ap.AppointmentID
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join proc in ctx.ProfileCompanies on cal.ProfileID equals proc.ProfileID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       where pro.CustID == custID
                       select new Appointment_Company() { Appointment = a, ProfileCompany = proc }).Distinct().ToList();

            if (ret != null)
                return ret.ToList();
            else
                return null;
        }

        public static IQueryable<Calendar> GetCalendarsForProfile(int profileID)
        {
            return DBContext.Calendars.Where(x => x.ProfileID == profileID);
        }

        public static IQueryable<Calendar> GetCustCalendars(int custID)
        {
            return GetCustCalendars(custID, DBContext);
        }

        public static IQueryable<Calendar> GetAppointmentCalendarsForCust(int apptID, int custID)
        {
            kuyamEntities ctx = DBContext;

            var ret = from ap in ctx.AppointmentParticipants
                      join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID && ap.AppointmentID == apptID
                      select cal;
            return ret;
        }

        public static int GetCustCompanyProfileID(int custID)
        {
            kuyamEntities ctx = DBContext;

            var ret = (from p in ctx.Profiles
                       join pc in ctx.ProfileCompanies on p.ProfileID equals pc.ProfileID
                       where p.CustID == custID && pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                       select pc.ProfileID).FirstOrDefault();

            return ret;
        }

        public static void DeleteOpenHour(int id)
        {
            DBContext.Database.ExecuteSqlCommand("delete from profilehours where profilehoursid=" + id);
        }

        public static void DeleteExceptionHour(int id)
        {
            DBContext.Database.ExecuteSqlCommand("delete from profilehoursexceptions where profilehoursexceptionid=" +
                                                 id);
        }

        public static IQueryable<Calendar> GetCustCalendars(int custID, kuyamEntities ctx)
        {
            if (ctx == null)
                ctx = DBContext;

            var ret = from cal in ctx.Calendars
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID
                      select cal;
            return ret;
        }

        public static IQueryable<Calendar> GetCustCalendars(int custID, List<int> calIDs)
        {
            kuyamEntities ctx = DBContext;
            var ret = from cal in ctx.Calendars
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID
                            && calIDs.Contains(cal.CalendarID)
                      select cal;

            return ret;
        }

        public static Calendar GetDefaultCalendarForCust(int custID)
        {
            kuyamEntities ctx = DBContext;
            var ret = from cal in ctx.Calendars
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID
                            && cal.IsDefault == true
                      select cal;

            if (ret != null)
                return ret.FirstOrDefault();
            else
                return null;
        }

        public static List<Calendar> GetSelectedCalendarsForCust(int custID)
        {
            kuyamEntities ctx = DBContext;
            var ret = from cal in ctx.Calendars
                      join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                      where p.CustID == custID
                            && cal.CalendarDisplayTypeID == (int)Types.CalendarDisplayType.Selected
                      select cal;

            if (ret != null)
                return ret.ToList();
            else
                return null;
        }

        public static IDNameDict GetAllCompanies()
        {
            IDNameDict ret = new IDNameDict();
            foreach (var t in DBContext.ProfileCompanies)
            {
                ret.Add(t.ProfileID, t.Name);
            }

            return ret;
        }

        public static List<ProfileCompany> GetUnverifiedCompanies()
        {
            var ret = DBContext.ProfileCompanies.Where(v => v.CompanyStatusID == (int)Types.CompanyStatus.Pending);

            if (ret != null)
                return ret.ToList();
            else
                return null;
        }

        public static List<Cust_Company> GetInactiveAppointmentCompanies()
        {
            kuyamEntities ctx = DBContext;

            var ret = (from ap in ctx.AppointmentParticipants
                       join a in ctx.Appointments on ap.AppointmentID equals a.AppointmentID
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join proc in ctx.ProfileCompanies on cal.ProfileID equals proc.ProfileID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       join cust in ctx.Custs on pro.CustID equals cust.CustID
                       where proc.CompanyStatusID != (int)Types.CompanyStatus.Active
                             && a.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
                       select new Cust_Company() { ProfileCompany = proc, Cust = cust }).Distinct();

            if (ret != null)
                return ret.ToList();
            else
                return null;
        }

        public static bool isFavorite(int custID, int profileID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Favorites.Any(x => x.CustID == custID && x.ProfileID == profileID))
            {
                return true;
            }
            return false;
        }

        //public static List<Appointment_Company> GetInactiveCompanyAppointments()
        //{
        //    kuyamEntities ctx = DBContext;

        //    var ret = (from ap in ctx.AppointmentParticipants
        //               join a in ctx.Appointments on ap.AppointmentID equals a.AppointmentID
        //               join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
        //               join proc in ctx.ProfileCompanies on cal.ProfileID equals proc.ProfileID
        //               join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
        //               join cust in ctx.Custs on pro.CustID equals cust.CustID
        //               where proc.CompanyStatusID != (int)Types.CompanyStatus.Active
        //                     && a.AppointmentStatusID == (int)Types.AppointmentStatus.Pending
        //               select new Appointment_Company() { Appointment = a, ProfileCompany = proc, Cust = cust });

        //    if (ret != null)
        //        return ret.ToList();
        //    else
        //        return null;
        //}

        /// <summary>
        /// Retrieve company names for all appointments with a company participant, for the specified cust.
        /// 
        /// </summary>
        /// <param name="custID"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static List<ProfileCompany> GetPriorCompanies(int custID, int maxCount = 10)
        {
            List<Appointment> appts = GetCustAppointments(custID).ToList();

            kuyamEntities ctx = DBContext;

            var ret = (from a in appts
                       join ap in ctx.AppointmentParticipants on a.AppointmentID equals ap.AppointmentID
                       join c in ctx.Calendars on ap.CalendarID equals c.CalendarID
                       join p in ctx.Profiles on c.ProfileID equals p.ProfileID
                       join proc in ctx.ProfileCompanies on c.ProfileID equals proc.ProfileID
                       where p.ProfileTypeID == (int)Types.CustType.Company
                       select proc).Distinct();

            if (ret == null)
                return null;
            else
                return ret.Take(maxCount).ToList();
        }

        public static ProfileHour GetProfileHour(int id)
        {
            return DBContext.ProfileHours.Where(h => h.ProfileHoursID == id).FirstOrDefault();
        }

        public static Guid GetAspUserID(string username)
        {
            var user = DBContext.aspnet_Users.Where(c => c.UserName == username).Select(n => new { UserId = n.UserId }).FirstOrDefault();
            if (user != null)
                return user.UserId;
            else
                return Guid.Empty;
        }

        // _db is an EF4.1 data entity context
        public static void UpdateRec<ENTITY_TYPE>(ENTITY_TYPE obj, params object[] keyValues) where ENTITY_TYPE : class
        {
            //if (obj.GetType() == typeof(Cust) && !custEnc)
            //    UpdateRec(obj, id);

            //ctx.Entry(ap).State = System.Data.EntityState.Detached;


            kuyamEntities ctx = DBContext;
            var org = ctx.Set<ENTITY_TYPE>().Find(keyValues);
            if (org != null)
            {
                ctx.Entry(org).CurrentValues.SetValues(obj);
            }
            else
            {
                ctx.Set<ENTITY_TYPE>().Add(obj);
            }
            ctx.Save();
        }

        // _db is an EF4.1 data entity context
        public static void DeleteRec<ENTITY_TYPE>(ENTITY_TYPE obj, params object[] keyValues) where ENTITY_TYPE : class
        {
            var org = DBContext.Set<ENTITY_TYPE>().Find(keyValues);
            kuyamEntities ctx = DBContext;
            ctx.Entry(org).State = EntityState.Deleted;
            ctx.Save();
        }

        //public static int CreateAccount(Account a)
        //{
        //    kuyamEntities ctx = DBContext;
        //    ctx.Accounts.Add(a);
        //    ctx.Save();
        //    return a.AccountID;
        //}

        public static int CreateCust(Cust c)
        {
            kuyamEntities ctx = DBContext;
            ctx.Custs.Add(c);
            ctx.Save();
            return c.CustID;
        }

        public static int CreateProfile(Profile p)
        {
            kuyamEntities ctx = DBContext;
            ctx.Profiles.Add(p);
            ctx.Save();
            return p.ProfileID;
        }

        public static int CreateCalendar(Calendar cal)
        {
            kuyamEntities ctx = DBContext;
            ctx.Calendars.Add(cal);
            ctx.Save();
            return cal.CalendarID;
        }

        public static void SaveChanges(kuyamEntities db = null)
        {
            if (db == null)
                db = DBContext;

            db.Save();
        }

        public static Invite Add(Invite invite)
        {
            //Neither of the two fucking EF calls work, so we manually insert this fucking stupid record
            //DBContext.Invites.Add(invite);
            //DBContext.Invites.Attach(invite);
            //DBContext.SaveChanges();

            decimal newId =
                DBContext.Database.SqlQuery<decimal>(
                    "insert into Invite (CustID, [Key], Email, Name, LName, MaxUses, Uses,Active, AccountTypeID,InviteType,CreateDate,  FacebookToken, Note,PhoneNumber) " +
                    "values (@CustID, @Key, @Email, @Name, @LName, @MaxUses, @Uses,@Active, @AccountTypeID,@InviteType,@CreateDate,  @FacebookToken, @Note,@PhoneNumber) " +
                    "select SCOPE_IDENTITY() ",
                    new SqlParameter("CustID", invite.CustID ?? 0),
                    new SqlParameter("Key", invite.Key),
                    new SqlParameter("Email", invite.Email),
                    new SqlParameter("Name", invite.Name),
                    new SqlParameter("LName", invite.LName),
                    new SqlParameter("MaxUses", invite.MaxUses),
                    new SqlParameter("Uses", invite.Uses),
                    new SqlParameter("Active", invite.Active),
                    new SqlParameter("InviteType", invite.InviteType),
                    new SqlParameter("FacebookToken", invite.FacebookToken),
                    new SqlParameter("CreateDate", invite.CreateDate),
                    new SqlParameter("Note", invite.Note),
                    new SqlParameter("PhoneNumber", invite.PhoneNumber),
                    new SqlParameter("AccountTypeID", invite.AccountTypeID)).FirstOrDefault();
            invite.InviteID = Convert.ToInt32(newId);
            return invite;
        }

        public static Invite GetInvite(string code, int inviteType = (int) Types.InviteType.User)
        {
            return
                DBContext.Invites.Where(
                    x => x.Key.ToUpper() == code.ToUpper() && x.Active == false && x.InviteType == inviteType).
                    FirstOrDefault();
        }

        public static Invite GetInviteCodeByEmail(string email)
        {

            return
                DBContext.Invites.Where(
                    x =>
                    x.Email.ToUpper() == email.ToUpper() && x.Active == false &&
                    (!x.Status.HasValue || x.Status == 0) && x.InviteType == (int)Types.InviteType.User).
                    FirstOrDefault();
        }

        public static Invite GetInviteByPhoneNumber(string phoneNumber, bool active = true)
        {
            return
                DBContext.Invites.Where(
                    x =>
                    x.PhoneNumber.ToUpper() == phoneNumber.ToUpper() && x.Active == active &&
                    x.InviteType == (int)Types.InviteType.SMSVerify).FirstOrDefault();
        }
        //Check invite code for SMS verify
        public static Invite GetInviteForSMSVerify(string phone, string email)
        {
            Invite invite = DBContext.Invites.Where(i => i.Email.ToLower() == email.ToLower()
                && i.PhoneNumber.ToLower() == phone.ToLower()
                && i.InviteType == (int)Types.InviteType.SMSVerify).FirstOrDefault();

            //if (invite!=null){
            //    //Random key
            //    string key = string.Empty;
            //    Invite i = new Invite();
            //    while (i != null){
            //        Random random = new Random();
            //        key = random.Next(1, 16777215).ToString("x").ToUpper(); //max FFFFFF value
            //        i = DAL.GetInvite(key);
            //    }
            //    //Update Invite code 
            //    invite.PhoneNumber = phone;
            //    invite.Key = key;
            //    UpdateRec(invite, invite.InviteID);

            //    return invite;
            //}
            return invite;
        }
        public static Invite GetInviteCodeByEmailForCheck(string email)
        {

            return
               DBContext.Invites.FirstOrDefault(
                   x =>
                   x.Email.ToUpper() == email.ToUpper()
                   && x.InviteType == (int)Types.InviteType.User
                   && (x.Active == true
                        || x.Status == (int)Types.UserInviteCodeStatusType.Pending
                        || x.Status == (int)Types.UserInviteCodeStatusType.Approved
                        || x.Status == (int)Types.UserInviteCodeStatusType.Active
                        )
                    );
        }

        public static Invite GetInviteByEmail(string email)
        {
            return
                DBContext.Invites.FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper() && x.Active == true &&
                                                      x.InviteType == (int)Types.InviteType.User);
        }

        public static Invite GetInviteByEmailForLoadData(string email)
        {
            return
                DBContext.Invites.FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper() &&
                                                      x.InviteType == (int)Types.InviteType.User);
        }

        public static Invite GetInviteByEmailForSignup(string email)
        {
            return
                DBContext.Invites.FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper() && x.Active == false &&
                                                      x.InviteType == (int)Types.InviteType.User);
        }


        public static Invite GetInvite(string code, string email)
        {
            return
                DBContext.Invites.Where(x => x.Key.ToUpper() == code.ToUpper() && x.Email.ToUpper() == email.ToUpper()).
                    FirstOrDefault();
        }

        public static bool VerifyZipCode(string zip)
        {
            //Todo: comment below code for camp test, need update after
            return zip != null && zip.Length == 5 || !Regex.IsMatch(zip, "^[0-9]+$");

            bool ret = false;
            ret = DBContext.ZipCodes.Where(z => z.Code == zip).Select(z => z.Active).FirstOrDefault();
            return ret;
        }

        public static Medium GetMedia(int mediaID)
        {
            return DBContext.Media.FirstOrDefault(m => m.MediaID == mediaID);
        }

        public static void DeleteAccount(int accountID)
        {
            DBContext.Database.ExecuteSqlCommand("delete from account where accountid=" + accountID);
        }

        public static void DeleteAspUser(string username)
        {
            DBContext.Database.ExecuteSqlCommand("exec DeleteAspUser '" + username + "'");
        }

        public static void DeleteUser(string username)
        {
            try
            {
                DeleteAspUser(username);
            }
            catch (Exception)
            {
            }

        }

        public static void AddUserToRoles(string userNames, string roleNames)
        {
            DBContext.Database.ExecuteSqlCommand("exec aspnet_usersinroles_adduserstoroles '/', '" + userNames + "', '" +
                                                 roleNames + "', '" + DateTime.UtcNow + "'");
        }

        public static string GetUsername(Guid aspguid)
        {
            aspnet_Users user = DBContext.aspnet_Users.Where(c => c.UserId == aspguid).FirstOrDefault();
            if (user != null)
                return user.UserName;
            else
                return null;
        }

        public static void LogError(string msg)
        {
            kuyamEntities ctx = DBContext;
            ctx.Errors.Add(new Error() { Message = msg });
            ctx.Save();
        }

        public static Dictionary<string, string> GetStats()
        {
            var stats = new Dictionary<string, string>
            {               
                {"customers", DBContext.Custs.Count().ToString()},
                {
                    "companies (unverified/verified/active)", string.Format("{0}/{1}/{2}",
                        DBContext.ProfileCompanies.Count(v => v.CompanyStatusID == 5),
                        DBContext.ProfileCompanies.Count(v => v.CompanyStatusID == 6),
                        DBContext.ProfileCompanies.Count(v => v.CompanyStatusID == 7))
                },
                {"errors (unresolved)", DBContext.Errors.Count(e => e.Resolved == null).ToString()}
            };

            DateTime aDayAgo = DateTime.Now.Subtract(new TimeSpan(24, 0, 0)).ToUniversalTime();
            stats.Add("logins today",
                      DBContext.aspnet_Membership.Count(m => m.LastLoginDate >= aDayAgo).ToString());

            return stats;
        }

        public static int GetUnviewedNotificationCount(int custid)
        {
            return
                DBContext.AppointmentLogs.Count(
                    x => x.CustID == custid && x.Viewed == false && x.Appointment.End > DateTime.Now);
        }

        public static AppointmentLog LoadAppointmentLog(int apptLogID)
        {
            return DBContext.AppointmentLogs.Where(x => x.AppointmentLogID == apptLogID).FirstOrDefault();
        }

        public static void SetNotificationsViewedForAppointment(int apptID, int custid)
        {
            kuyamEntities ctx = DBContext;
            IEnumerable<AppointmentLog> logs =
                ctx.AppointmentLogs.Where(x => x.AppointmentID == apptID && x.CustID == custid);
            foreach (AppointmentLog log in logs)
            {
                log.Viewed = true;
            }
            ctx.SaveChanges();
        }

        public static void ResetSelectedCalendars(int custID)
        {
            kuyamEntities ctx = DBContext;
            IQueryable<Calendar> cals = GetCustCalendars(custID, ctx);
            foreach (Calendar cal in cals)
            {
                if (cal.CalendarDisplayTypeID == (int)Types.CalendarDisplayType.Selected)
                    cal.CalendarDisplayTypeID = (int)Types.CalendarDisplayType.Selectable;
            }
            ctx.Save();
            //DBContext.Database.ExecuteSqlCommand("update calendar set calendardisplaytypeid=" + Types.CalendarDisplayType.Selectable + " where custid=" + custID + " and calendardisplaytypeid=" + Types.CalendarDisplayType.Selected);
        }

        public static void SetCalendarDisplayType(int calID, Types.CalendarDisplayType typeID)
        {
            DBContext.Database.ExecuteSqlCommand("update calendar set calendardisplaytypeid=" + (int)typeID +
                                                 " where calendarid=" + calID);
        }

        public static Profile GetAppointmentCompany(Appointment a)
        {
            kuyamEntities ctx = DBContext;
            Profile ret = (from ap in ctx.AppointmentParticipants
                           join c in ctx.Calendars on ap.CalendarID equals c.CalendarID
                           join p in ctx.Profiles on c.ProfileID equals p.ProfileID
                           where ap.AppointmentID == a.AppointmentID
                                 && p.ProfileTypeID == (int)Types.CustType.Company
                           select p).FirstOrDefault();

            return ret;
        }

        public static List<Profile> GetAppointmentProfiles(Appointment appt, Cust cust = null,
                                                           Types.CustType profileType = Types.CustType.Unknown)
        {
            kuyamEntities ctx = DBContext;
            var res = (from ap in ctx.AppointmentParticipants
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       where ap.AppointmentID == appt.AppointmentID
                       select pro);

            if (res == null)
                return null;

            if (cust != null)
                res = res.Where(x => x.CustID == cust.CustID);

            if (profileType != Types.CustType.Unknown)
                res = res.Where(x => x.ProfileTypeID == (int)profileType);

            return res.ToList();
        }

        public static List<Cust> GetAppointmentCusts(Appointment appt)
        {
            List<Cust> ret = null;

            kuyamEntities ctx = DBContext;
            ret = (from ap in ctx.AppointmentParticipants
                   join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                   join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                   join cu in ctx.Custs on pro.CustID equals cu.CustID
                   where ap.AppointmentID == appt.AppointmentID
                   select cu).Distinct().ToList();

            return ret;
        }

        /// <summary>
        /// Retrieve a list of all participants for an appointment key by cust with a value of profile name
        /// </summary>
        /// <param name="appt"></param>
        /// <returns></returns>
        public static Dictionary<Cust, string> GetAppointmentParticipantNames(Appointment appt)
        {
            Dictionary<Cust, string> ret = new Dictionary<Cust, string>();

            kuyamEntities ctx = DBContext;
            var foo = (from ap in ctx.AppointmentParticipants
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                       join proc in ctx.ProfileCompanies on cal.ProfileID equals proc.ProfileID
                       join cu in ctx.Custs on pro.CustID equals cu.CustID
                       where ap.AppointmentID == appt.AppointmentID
                       select new { Key = cu, Value = proc.Name }); //.Distinct().ToDictionary(c => c.Key, c => c.Value);

            foreach (var foo2 in foo)
            {
                if (!ret.Keys.Contains(foo2.Key))
                    ret.Add(foo2.Key, foo2.Value);
            }

            return ret;
        }

        public static AppointmentParticipant GetAppointmentParticipant(int apptPartID)
        {
            return
                DBContext.AppointmentParticipants.FirstOrDefault(x => x.AppointmentParticipantID == apptPartID);
        }

        public static AppointmentParticipant GetAppointmentParticipant(int apptid, int calid)
        {
            return
                DBContext.AppointmentParticipants.FirstOrDefault(x => x.AppointmentID == apptid && x.CalendarID == calid);
        }

        public static int GetCalendarIDForCustID(int apptid, int custid)
        {
            kuyamEntities ctx = DBContext;
            return (from ap in ctx.AppointmentParticipants
                    join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                    join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                    join cu in ctx.Custs on pro.CustID equals cu.CustID
                    where ap.AppointmentID == apptid && cu.CustID == custid
                    select cal.CalendarID).FirstOrDefault();
        }

        public static List<Cust> GetAppointmentCusts(Appointment appt, Types.AppointmentParticipantType participantType)
        {
            List<Cust> ret = null;

            kuyamEntities ctx = DBContext;
            ret = (from ap in ctx.AppointmentParticipants
                   join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                   join pro in ctx.Profiles on cal.ProfileID equals pro.ProfileID
                   join cu in ctx.Custs on pro.CustID equals cu.CustID
                   where ap.AppointmentID == appt.AppointmentID
                         && ap.ParticipantTypeID == (int)participantType
                   select cu).Distinct().ToList();

            return ret;
        }

        public static List<Calendar> GetAppointmentCalendars(Appointment appt,
                                                             Types.AppointmentParticipantType participantType)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from ap in ctx.AppointmentParticipants
                       join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                       where ap.AppointmentID == appt.AppointmentID
                             && ap.ParticipantTypeID == (int)participantType
                       select cal).Distinct().ToList();

            return ret;
        }

        /// <summary>
        /// Set all participants to non-owner, change cust to owner
        /// </summary>
        /// <param name="appt"></param>
        /// <param name="cust"></param>
        public static void SetAppointmentOwner(Appointment appt, Calendar cal)
        {
            kuyamEntities ctx = DBContext;

            string sql = String.Format(
                "update appointmentparticipant set participanttypeid={0} where appointmentid={1}",
                (int)Types.AppointmentParticipantType.Invitee, appt.AppointmentID);
            ctx.Database.ExecuteSqlCommand(sql);

            sql =
                String.Format(
                    "update appointmentparticipant set participanttypeid={0} where appointmentid={1} and calendarid={2}",
                    (int)Types.AppointmentParticipantType.Owner, appt.AppointmentID, cal.CalendarID);
            ctx.Database.ExecuteSqlCommand(sql);
        }

        public static List<AppointmentLogData> GetAppointmentLogData(string username)
        {
            Cust c = xGetCust(username);
            DateTime minDate = DateTime.Now.AddDays(-30);
            kuyamEntities ctx = DBContext;
            var logData = (from l in ctx.AppointmentLogs
                           join a in ctx.Appointments on l.AppointmentID equals a.AppointmentID
                           join ap in ctx.AppointmentParticipants on a.AppointmentID equals ap.AppointmentID
                           join cal in ctx.Calendars on ap.CalendarID equals cal.CalendarID
                           join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
                           join v in ctx.ProfileCompanies on cal.ProfileID equals v.ProfileID into tempv
                           from vreal in tempv.DefaultIfEmpty()
                           where l.CustID == c.CustID && l.Viewed == false && a.End > DateTime.Now
                           orderby l.LogDT descending
                           select new AppointmentLogData()
                                      {
                                          Log = l,
                                          Appointment = a,
                                          CompanyName = (vreal == null ? String.Empty : vreal.Name),
                                          ProfileName = p.Name
                                      }).ToList();
            return logData;
        }

        // appointment
        // phuong
        public static List<Appointment> GetListAppointmentByCustID(int custID)
        {
            kuyamEntities ctx = DBContext;
            if (custID == 0)
                return null;
            var query = from apt in ctx.Appointments where apt.CustID == custID select apt;
            var result = query.ToList();
            return result;
        }


        /// <summary>
        /// Gets the list appointment by employee id.
        /// </summary>
        /// <param name="employeeId">The employee id.</param>
        /// <returns></returns>
        public static IQueryable<Appointment> GetListAppointmentByEmployeeId(int employeeId, DateTime startDate, DateTime endDate)
        {
            kuyamEntities ctx = DBContext;
            return
                ctx.Appointments.Where(
                    p => (employeeId <= 0 || p.EmployeeID == employeeId) && p.Start >= startDate && p.Start < endDate
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                    );
        }


        public static bool ChangeStatus(int appointmentID, int AppointmentStatusID)
        {
            kuyamEntities ctx = DBContext;
            Appointment apt = ctx.Appointments.Where(x => x.AppointmentID == appointmentID).FirstOrDefault();
            if (apt == null)
                return false;
            if (AppointmentStatusID == (int)Types.AppointmentStatus.Modified ||
                AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
            {
                apt.AppointmentStatusID = (int)Types.AppointmentStatus.Confirmed;
            }
            else
            {
                apt.AppointmentStatusID = (int)Types.AppointmentStatus.Cancelled;
            }
            ctx.SaveChanges();
            return true;
        }

        //end

        public static bool ChangeStatusAppointmentIP(int appointmentID, int AppointmentStatusID)
        {
            kuyamEntities ctx = DBContext;
            Appointment apt = ctx.Appointments.Where(x => x.AppointmentID == appointmentID).FirstOrDefault();
            if (apt == null)
                return false;
            apt.AppointmentStatusID = AppointmentStatusID;
            if (AppointmentStatusID == (int)Types.AppointmentStatus.Modified ||
                AppointmentStatusID == (int)Types.AppointmentStatus.Pending)
            {
                apt.AppointmentStatusID = (int)Types.AppointmentStatus.Confirmed;
            }

            ctx.SaveChanges();
            return true;
        }

        //public static void AddMediaNew(MediaNew media)
        //{
        //    kuyamEntities ctx = DBContext;
        //    ctx.MediaNews.Add(media);
        //    ctx.Save();
        //}

        public static void AddCompanyMedia(CompanyMedia cm)
        {
            kuyamEntities ctx = DBContext;
            ctx.CompanyMedias.Add(cm);
            ctx.Save();
        }

        public static List<Medium> GetBannerFeatureCompanyList()
        {
            kuyamEntities ctx = DBContext;
            var ret = (from fc in ctx.FeaturedCompanies
                       join pc in ctx.ProfileCompanies on fc.ProfileID equals pc.ProfileID
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           (cm.IsDefault == true || cm.IsBanner == true)
                           && m.LocationPath != string.Empty
                           && m.LocationPath != null
                           && fc.priority > 0
                           && pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                       select m).Distinct().ToList();

            foreach (Medium medium in ret)
            {
                int totalReview = 0;
                double valueRanting = 0;

                var profile = ctx.CompanyMedias.Where(m => m.MediaID == medium.MediaID).Select(m => m.ProfileCompany).First();
                if (profile.ServiceCompanies != null && profile.ServiceCompanies.Count > 0)
                {
                    foreach (ServiceCompany item in profile.ServiceCompanies)
                    {
                        totalReview += item.Ratings.Count;
                        valueRanting += item.Ratings.Sum(m => m.RatingValue).Value;
                    }
                }

                if (totalReview > 0)
                {
                    medium.TotalReview = totalReview;
                    medium.Rate = Math.Round((double)(valueRanting / totalReview));
                }
            }
            return ret;

        }

        public static int GetProfileIdFromBannerID(int mediaID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.CompanyMedias.Any(x => x.MediaID == mediaID))
            {
                CompanyMedia cm = ctx.CompanyMedias.Where(x => x.MediaID == mediaID).FirstOrDefault();
                return cm.ProfileID;
            }
            return 0;
        }
        public static Medium GetCompanyLogoByProfileID(int profileID)
        {
            kuyamEntities ctx = DBContext;
            //int profileID = GetProfileIdFromBannerID(bannerID);
            var ret = (from pc in ctx.ProfileCompanies
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           cm.IsLogo == true && m.LocationPath != string.Empty && m.LocationPath != null &&
                           pc.ProfileID == profileID && pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                       select m).FirstOrDefault();
            return ret;
        }
        public static Medium GetCompanyLogoFromBannerID(int bannerID)
        {
            kuyamEntities ctx = DBContext;
            int profileID = GetProfileIdFromBannerID(bannerID);
            var ret = (from pc in ctx.ProfileCompanies
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           cm.IsLogo == true && m.LocationPath != string.Empty && m.LocationPath != null &&
                           pc.ProfileID == profileID && pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                       select m).FirstOrDefault();
            return ret;
        }

        public static Medium GetCompanyLogoFromProfileCompanyID(int id)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           cm.IsLogo == true && m.LocationPath != string.Empty && m.LocationPath != null &&
                           pc.ProfileID == id
                       select m).FirstOrDefault();
            return ret;
        }

        public static List<Medium> GetListPhotoByCompanyID(int id)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from cm in ctx.CompanyMedias
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where cm.IsBanner == true && (cm.IsHidden == false || !cm.IsHidden.HasValue) && cm.ProfileID == id
                       select m);
            return ret.ToList();
        }

        public static List<Medium> GetListLogoByCompanyID(int id)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from cm in ctx.CompanyMedias
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where cm.IsLogo == true && cm.ProfileID == id
                       select m);
            return ret.ToList();
        }

        public static List<Medium> GetListVideByCompanyID(int id)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from cm in ctx.CompanyMedias
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where cm.IsVideo == true && cm.ProfileID == id
                       select m);
            return ret.ToList();
        }

        public static ProfileCompany GetProfileCompanyFromBannerID(int bannerID)
        {
            kuyamEntities ctx = DBContext;
            int profileID = GetProfileIdFromBannerID(bannerID);
            ProfileCompany pc = ctx.ProfileCompanies.Where(x => x.ProfileID == profileID && x.CompanyStatusID != (int)Types.CompanyStatus.Deleted).FirstOrDefault();
            return pc;
        }

        public static Medium GetCompanyImageFromProfileCompanyID(int profileCompanyID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           cm.IsLogo == false && cm.IsBanner == false && m.LocationPath != string.Empty &&
                           m.LocationPath != null && pc.ProfileID == profileCompanyID
                       select m).FirstOrDefault();
            return ret;
        }

        public static Medium GetCompanyPhotoByCompanyID(int companyID)
        {
            kuyamEntities ctx = DBContext;
            Medium media = null;

            List<CompanyMedia> companyMedia = (from cm in ctx.CompanyMedias
                                               join m in ctx.Media on cm.MediaID equals m.MediaID
                                               join cp in ctx.ProfileCompanies on cm.ProfileID equals cp.ProfileID
                                               where cm.IsBanner && (cm.IsHidden == false || !cm.IsHidden.HasValue) && !string.IsNullOrEmpty(m.LocationData)
                                                     && cm.ProfileID == companyID && cp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                                               select cm).ToList();

            if (companyMedia != null && companyMedia.Count > 0)
            {
                CompanyMedia tempCompanyMedia = companyMedia.Where(m => m.IsDefault).FirstOrDefault();
                if (tempCompanyMedia == null)
                {
                    tempCompanyMedia = companyMedia.FirstOrDefault();
                }
                media = tempCompanyMedia.Medium;
            }

            return media;
        }


        public static List<Medium> GetCompanyPhotoByCompanyID(List<int> companyID)
        {
            kuyamEntities ctx = DBContext;

            var companyMedia = (from cm in ctx.CompanyMedias
                                join m in ctx.Media on cm.MediaID equals m.MediaID
                                join cp in ctx.ProfileCompanies on cm.ProfileID equals cp.ProfileID
                                where cm.IsBanner && (cm.IsHidden == false || !cm.IsHidden.HasValue) && !string.IsNullOrEmpty(m.LocationData)
                                      && companyID.Contains(cm.ProfileID) && cp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                                select m).ToList();


            return companyMedia;
        }

        public static int GetFeatureCompanyPriority(int profileID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.FeaturedCompanies.Any(x => x.ProfileID == profileID))
            {
                FeaturedCompany fc = ctx.FeaturedCompanies.SingleOrDefault(x => x.ProfileID == profileID);
                return fc.priority;
            }
            return 0;

        }

        public static List<Type> GetParentCategories()
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Types.Any(x => x.TypeGroupID == 2))
            {
                List<Type> list = ctx.Types.Where(x => x.TypeGroupID == 2).Distinct().OrderBy(x => x.Name).ToList();
                return list;
            }
            return null;
        }

        public static List<Type> GetCategories()
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Services.Any(x => x.ParentServiceID == null))
            {
                List<Type> list = ctx.Types.Where(x => x.TypeGroupID == 2).Distinct().OrderBy(x => x.Name).ToList();
                return list;
            }
            return null;
        }

        public static List<Type> GetChildCategories(int parentID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Types.Any(x => x.ParentTypeID == parentID))
            {
                List<Type> list = ctx.Types.Where(x => x.ParentTypeID == parentID).Distinct().ToList();
                return list;
            }
            return null;
        }

        public static List<ProfileCompany> GetFeatureCompaniesFromTypeID(int typeID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                       where pc.CompanyTypeID == typeID || typeID == 0
                       select pc).Distinct().ToList();
            return ret;
        }

        public static List<ProfileCompany> GetCompaniesFromTypeID(int typeID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                       where pc.CompanyTypeID == typeID || typeID == 0
                       select pc)
                .Union
                (from pc1 in ctx.ProfileCompanies
                 where (pc1.CompanyTypeID == typeID || typeID == 0)
                       && !(from o in ctx.ProfileCompanies
                            join fc1 in ctx.FeaturedCompanies on o.ProfileID equals fc1.ProfileID
                            where o.CompanyTypeID == typeID || typeID == 0
                            select o.ProfileID)
                               .Contains(pc1.ProfileID)
                 select pc1
                ).Distinct().ToList();
            return ret;
        }

        public static List<ProfileCompany> GetCompaniesFromTypeID(int typeID, out int totalRecords)
        {
            totalRecords = 0;
            kuyamEntities ctx = DBContext;

            var fPCs = (from pc in ctx.ProfileCompanies
                        join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                        where (pc.CompanyTypeID == typeID || typeID == 0)
                        select pc).Distinct().ToList();

            var fPCsID = (from o in ctx.ProfileCompanies
                          join fc1 in ctx.FeaturedCompanies on o.ProfileID equals fc1.ProfileID
                          where (o.CompanyTypeID == typeID || typeID == 0)
                          select o.ProfileID).Distinct().ToList();

            var pCs = (from pc1 in ctx.ProfileCompanies
                       where (pc1.CompanyTypeID == typeID || typeID == 0)
                             && fPCsID.Contains(pc1.ProfileID) == false
                       select pc1).Distinct().ToList();
            var ret = fPCs.Union(pCs).Distinct().ToList();
            totalRecords = ret.Count;
            return ret.Skip(0).Take(10).ToList();
        }

        public static Type GetTypeFromTypeID(int typeID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Types.Any(x => x.TypeID == typeID))
            {
                Type type = ctx.Types.Where(x => x.TypeID == typeID).FirstOrDefault();
                return type;
            }
            return null;

        }

        public static string GetServiceNameFromServiceID(int serviceID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Services.Any(x => x.ServiceID == serviceID))
            {
                Service service = ctx.Services.Where(x => x.ServiceID == serviceID).FirstOrDefault();
                return service.ServiceName;
            }
            return string.Empty;

        }

        public static ProfileCompany GetProfileCompany(int profileCompanyID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.ProfileCompanies.Any(x => x.ProfileID == profileCompanyID))
            {
                return ctx.ProfileCompanies.Where(x => x.ProfileID == profileCompanyID).FirstOrDefault();
            }
            return null;
        }

        //public static void AddCustomerSchedule(CustomerSchedule cs)
        //{
        //    kuyamEntities ctx = DBContext;
        //    ctx.CustomerSchedules.Add(cs);
        //    ctx.Save();
        //}

        //public static void AddCustomerScheduleLog(CustomerScheduleLog csl)
        //{
        //    kuyamEntities ctx = DBContext;
        //    ctx.CustomerScheduleLogs.Add(csl);
        //    ctx.Save();

        //}

        public static List<ProfileCompany> GetProfileCompanies(int typeID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.ProfileCompanies.Any(x => x.CompanyTypeID == typeID))
            {
                List<ProfileCompany> pcList =
                    ctx.ProfileCompanies.Where(x => x.CompanyTypeID == typeID).Distinct().ToList();
                return pcList;
            }
            return null;
        }

        //public static void ConfirmCustomerSchedule(int csID)
        //{
        //    kuyamEntities ctx = DBContext;
        //    if (ctx.CustomerSchedules.Any(x => x.CustomerScheduleID == csID))
        //    {
        //        CustomerSchedule cs = ctx.CustomerSchedules.Where(x => x.CustomerScheduleID == csID).FirstOrDefault();
        //        cs.IsConfirm = true;
        //        ctx.SaveChanges();
        //    }
        //}

        //public static List<CustomerSchedule> GetCustomerShedulesBySessionID(Guid sessionID)
        //{
        //    kuyamEntities ctx = DBContext;
        //    if (ctx.CustomerSchedules.Any(x => x.SessionID == sessionID))
        //    {
        //        return ctx.CustomerSchedules.Where(x => x.SessionID == sessionID).Distinct().ToList();
        //    }
        //    return null;
        //}



        public static void updateCompanyLatandLong(int profileID, double lat, double lon)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.ProfileCompanies.Any(x => x.ProfileID == profileID))
            {
                ProfileCompany pc = ctx.ProfileCompanies.Where(x => x.ProfileID == profileID).FirstOrDefault();
                pc.Modified = DateTime.Now;
                pc.Latitude = lat;
                pc.Longitude = lon;
                ctx.SaveChanges();
            }
        }

        public static bool isFeatureCompany(int profileID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.FeaturedCompanies.Any(x => x.ProfileID == profileID))
            {
                return true;
            }
            return false;
        }

        public static bool isViewAvailability(int profileCompanyID)
        {
            /*
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                       join s in ctx.Services on scp.ServiceID equals s.ServiceID
                       where (pc.ProfileID == profileCompanyID)
                       select pc).Distinct().ToList();
            if (ret != null && ret.Count > 0)
                return true;
            return false;
             * */

            kuyamEntities ctx = DBContext;
            var ret = (from eh in ctx.EmployeeHours
                       join ce in ctx.CompanyEmployees on eh.CompanyEmployeeID equals ce.EmployeeID
                       where ce.ProfileCompanyID == profileCompanyID
                       select eh).Count();
            if (ret > 0)
                return true;
            return false;
        }

        public static bool isAvailableToday(int profileCompanyId)
        {
            kuyamEntities ctx = DBContext;
            //var shList = (from sh in ctx.ServiceHours
            //              where sh.ServiceCompany.ProfileID == profileCompanyId
            //              select sh).Distinct().ToList();
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
            /*
            var shList = (from sh in ctx.ServiceHours 
                          join s in ctx.Services on  sh.ServiceID equals s.ServiceID
                          join scp in ctx.ServiceCompanies on sh.ServiceID equals scp.ServiceID
                          where scp.ProfileID == profileCompanyID && DateTime.Now < sh.ToDateTime
                       select sh).Distinct().ToList();
            if (shList.Count == 0)
                return false;
            foreach(ServiceHour sh in shList)
            {
                Service s = ctx.Services.Where(x => x.ServiceID == sh.ServiceID).SingleOrDefault();
                double minuteToEnd = (sh.ToDateTime - DateTime.Now ).TotalMinutes;
                if (minuteToEnd > 10)
                {
                    return true;
                }                
            }

            return false;
             * */
        }


        public static bool isAvailableFromPriceToPrice(int profileCompanyID, decimal fromPrice, decimal toPrice)
        {
            if (fromPrice == 0 && toPrice == 0)
                return true;
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                       join s in ctx.Services on scp.ServiceID equals s.ServiceID
                       where pc.ProfileID == profileCompanyID && (scp.Price > fromPrice && scp.Price < toPrice)
                       select pc).Distinct().ToList();
            if (ret != null && ret.Count > 0)
                return true;
            return false;
        }

        //public static bool isAvailableFromHourToHour(int profileCompanyID, DateTime fromHour, DateTime toHour)
        //{
        //    if (fromHour == DateTime.Today && toHour == DateTime.Today)
        //        return true;
        //    kuyamEntities ctx = DBContext;
        //    var ret = (from pc in ctx.ProfileCompanies                       
        //               join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
        //               join s in ctx.Services on scp.ServiceID equals s.ServiceID
        //               join sh in ctx.ServiceHours on s.ServiceID equals sh.ServiceID
        //               where pc.ProfileID == profileCompanyID && ((sh.FromDateTime > fromHour && sh.FromDateTime < toHour) || (sh.ToDateTime > fromHour && sh.ToDateTime < toHour) || (sh.FromDateTime < fromHour && sh.ToDateTime > toHour))
        //               select sh).Distinct().ToList();
        //    if (ret != null && ret.Count > 0)
        //        return true;
        //    return false;
        //}

        public static decimal GetMinCompanyervicePrice(int profileID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                       join s in ctx.Services on scp.ServiceID equals s.ServiceID
                       where pc.ProfileID == profileID && scp.Status != 1
                       select scp.Price).Min();
            if (ret > 0)
                return ret.Value;
            return 0;
        }

        public static List<ProfileCompany> GetCompaniesFromPriceToPrice(int typeID, decimal fromPrice, decimal toPrice)
        {
            kuyamEntities ctx = DBContext;
            if (fromPrice == 0 && toPrice == 0)
            {
                var ret = (from pc in ctx.ProfileCompanies
                           where pc.CompanyTypeID == typeID || typeID == 0
                           select pc).Distinct().ToList();
                return ret;
            }

            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        where (pc.CompanyTypeID == typeID || typeID == 0)
                              && (scp.Price >= fromPrice && scp.Price <= toPrice) && scp.Status != 1
                        select pc).Distinct().ToList();
            return ret1;
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

        /*
        public static List<ProfileCompany> GetCompaniesFromHourToHour(int typeID, DateTime fromHour, DateTime toHour)
        {
            kuyamEntities ctx = DBContext;
            if (fromHour == DateTime.Today && toHour == DateTime.Today)
            {
                var ret = (from pc in ctx.ProfileCompanies
                           where pc.CompanyTypeID == typeID || typeID == 0
                           select pc).Distinct().ToList();
                return ret;
            }
            
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                       join sh in ctx.ServiceHours on s.ServiceID equals sh.ServiceID
                       where (pc.CompanyTypeID ==  typeID || typeID == 0)
                       && ((sh.FromDateTime > fromHour && sh.FromDateTime < toHour) || (sh.ToDateTime > fromHour && sh.ToDateTime < toHour) || (sh.FromDateTime < fromHour && sh.ToDateTime > toHour))
                       select pc).Distinct().ToList();
            return ret1;
        }
        */


        public static List<int> GetCompanyIDsFromHourToHour(int serviceID, DateTime fromHour, DateTime toHour)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join es in ctx.EmployeeServices on scp.ServiceCompanyID equals es.ServiceCompanyID
                        join ce in ctx.CompanyEmployees on es.CompanyEmployeeID equals ce.EmployeeID
                        join eh in ctx.EmployeeHours on ce.EmployeeID equals eh.CompanyEmployeeID
                        //join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        //join sh in ctx.ServiceHours on scp.ServiceCompanyID equals sh.ServiceCompanyID
                        where (scp.ServiceID == serviceID || serviceID == 0)
                              &&
                              ((eh.FromHour.Hours >= fromHour.Hour && eh.FromHour.Hours < toHour.Hour) ||
                               (eh.ToHour.Hours > fromHour.Hour && eh.ToHour.Hours <= toHour.Hour) ||
                               (eh.FromHour.Hours <= fromHour.Hour && eh.ToHour.Hours >= toHour.Hour))
                        select pc.ProfileID).Distinct().ToList();
            return ret1;
        }

        public static List<int> GetCompanyIdsFromCompanyName(int serviceID, string key)
        {
            kuyamEntities ctx = DBContext;
            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        //join sh in ctx.ServiceHours on scp.ServiceCompanyID equals sh.ServiceCompanyID
                        where (scp.ServiceID == serviceID || serviceID == 0)
                              && pc.Name.Contains(key) == true
                        select pc.ProfileID).Distinct().ToList();
            return ret1;
        }


        public static List<ProfileCompany> GetCompaniesAvailableToday(int typeID, bool isToday)
        {
            kuyamEntities ctx = DBContext;
            if (!isToday)
            {
                var ret = (from pc in ctx.ProfileCompanies
                           where pc.CompanyTypeID == typeID || typeID == 0
                           select pc).Distinct().ToList();
                return ret;
            }

            var ret1 = (from pc in ctx.ProfileCompanies
                        join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                        join s in ctx.Services on scp.ServiceID equals s.ServiceID
                        //join sh in ctx.ServiceHours on s.ServiceID equals sh.ServiceID
                        where pc.CompanyTypeID == typeID || typeID == 0
                        select pc).Distinct().ToList();
            List<ProfileCompany> pcList = new List<ProfileCompany>();
            foreach (ProfileCompany pc in ret1)
            {
                if (isAvailableToday(pc.ProfileID))
                    pcList.Add(pc);
            }
            return pcList;
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


        public static List<ProfileCompany> GetCompanies(int serviceId,
                                                        decimal priceFrom,
                                                        decimal priceTo,
                                                        DateTime hourFrom,
                                                        DateTime hourTo,
                                                        bool isToday,
                                                        int sortBy,
                                                        string key)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                int endDay = DateTime.Now.AddDays(1).Day;

                List<int> profileIdWithPrice = new List<int>();
                List<int> profileIdWithHour = new List<int>();
                List<int> profileIdToday = new List<int>();
                List<int> profileIDName = new List<int>();


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

                var fPCs = (from pc in ctx.ProfileCompanies
                            join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                            join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                            where
                                (pcs.ServiceID == serviceId || serviceId == 0) &&
                                pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                                &&
                                ((priceFrom == 0 && priceTo == 0) || (profileIdWithPrice.Contains(pc.ProfileID) == true))
                                &&
                                ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                 (profileIdWithHour.Contains(pc.ProfileID) == true))
                                && (!isToday || (profileIdToday.Contains(pc.ProfileID) == true))
                                && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                            select pc).Distinct().ToList();

                List<int> fpcIDs = fPCs.Select(x => x.ProfileID).ToList();

                var pCs = (from pc in ctx.ProfileCompanies
                           join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                           from s in ctx.Services
                           from sh in ctx.ServiceHours
                           where
                               (pcs.ServiceID == serviceId || serviceId == 0) &&
                               pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                               &&
                               ((priceFrom == 0 && priceTo == 0) || (profileIdWithPrice.Contains(pc.ProfileID) == true))
                               &&
                               ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                (profileIdWithHour.Contains(pc.ProfileID) == true))
                               && (!isToday || (profileIdToday.Contains(pc.ProfileID) == true))
                               && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                               && fpcIDs.Contains(pc.ProfileID) == false
                           select pc).Distinct().ToList();
                var ret = fPCs.Union(pCs).Distinct().ToList();
                if (sortBy == (int)Types.SortBy.CompanyName)
                {
                    return ret.OrderBy(x => x.Name).ToList();
                }
                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<ProfileCompany> GetListCompanyIDs(int serviceId, decimal priceFrom, decimal priceTo,
                                                             DateTime hourFrom, DateTime hourTo, bool isToday,
                                                             string key)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                int endDay = DateTime.Now.AddDays(1).Day;

                List<int> profileIdWithPrice = new List<int>();
                List<int> profileIdWithHour = new List<int>();
                List<int> profileIdToday = new List<int>();
                List<int> profileIDName = new List<int>();

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
                //-----------------Trong Edit--------------------

                //var fpcs = (from pc in ctx.ProfileCompanies
                //            join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                //            join fc in ctx.FeaturedCompanies on pcs.ProfileID equals fc.ProfileID
                //            where
                //                (pcs.ServiceID == serviceId || serviceId == 0) &&
                //                pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                //                &&
                //                ((priceFrom == 0 && priceTo == 0) ||
                //                 (profileIdWithPrice.Contains(pcs.ProfileID) == true))
                //                &&
                //                ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                //                 (profileIdWithHour.Contains(pcs.ProfileID) == true))
                //                && (!isToday || (profileIdToday.Contains(pcs.ProfileID) == true))
                //                && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                //            select pc).Distinct().OrderBy(x => x.Name).ToList();
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
                List<ProfileCompany> tempFeatureListWithIsToday = new List<ProfileCompany>();
                List<ProfileCompany> tempFeatureListWithViewAvailable = new List<ProfileCompany>();
                List<ProfileCompany> tempFeatureList = new List<ProfileCompany>();
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

                var pCs = (from pc in ctx.ProfileCompanies
                           //join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                           join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID into joinProfileCompanies
                           from j in joinProfileCompanies.DefaultIfEmpty()

                           //from s in ctx.Services
                           //from sh in ctx.ServiceHours
                           where
                               (serviceId == 0 || j.ServiceID == 0 || j.ServiceID == serviceId) &&

                               (pc.CompanyStatusID == (int)Types.CompanyStatus.Active)

                               &&
                               ((priceFrom == 0 && priceTo == 0) || (profileIdWithPrice.Contains(j.ProfileID) == true))
                               &&
                               ((hourFrom == DateTime.Today && hourTo == DateTime.Today) ||
                                (profileIdWithHour.Contains(j.ProfileID) == true))
                               && (!isToday || (profileIdToday.Contains(j.ProfileID) == true))
                               //&& fpcIDs.Contains(j.ProfileID) == false
                               //&& aPCIDs.Contains(j.ProfileID) == false
                               //&& avPCIDs.Contains(j.ProfileID) == false
                               && (string.IsNullOrEmpty(key) || profileIDName.Contains(j.ProfileID) == true)
                           select pc).Distinct().OrderBy(x => x.Name).ToList();

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

        public static List<ProfileCompany> GetListCompaniesByListCompanyIDs(List<int> ids)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                var ret = (from id in ids
                           join pc in ctx.ProfileCompanies on id equals pc.ProfileID
                           select pc).Distinct().ToList();
                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static string GetTypeName(int typeID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.Types.Any(x => x.TypeID == typeID))
                return ctx.Types.Where(x => x.TypeID == typeID).FirstOrDefault().Name;
            return string.Empty;
        }

        public static string GetTypeNameFromProfileID(int profileID)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from s in ctx.Services
                       join cs in ctx.ServiceCompanies on s.ServiceID equals cs.ServiceID
                       where cs.ProfileID == profileID && s.ParentServiceID == null
                       select s.ServiceName).Distinct().ToList();

            //if (ret != null && ret.Count > 0)
            //{
            //    string CategoryNameList = string.Empty;

            //    for (int i = 0; i < ret.Count; i++)
            //    {
            //        if (i < ret.Count - 1)
            //        {
            //            CategoryNameList = CategoryNameList + ret[i].ServiceName + ", ";
            //        }
            //        else
            //        {
            //            CategoryNameList = CategoryNameList + ret[i].ServiceName;
            //        }
            //    }
            //    return CategoryNameList;

            //}


            return string.Join(",", ret);
        }

        //public static List<UIFieldDef> UIFieldDefs
        //{
        //    get
        //    {
        //        ObjectCache cache = MemoryCache.Default;
        //        List<UIFieldDef> ret = cache["UIFieldDef"] as List<UIFieldDef>;

        //        if (ret == null)
        //        {
        //            ret = DBContext.UIFieldDefs.OrderBy(f => f.UIFieldDefID).ToList();
        //            cache["UIFieldDef"] = ret;
        //        }

        //        return ret;
        //    }
        //}

        //public static Cust GetCustFromAccountID(int accountID)
        //{
        //    return DBContext.Custs.Where(c => c.AccountID == accountID).FirstOrDefault();
        //}

        //// Company->Account->Cust
        //// TODO: Username needs to get saved with account?  No: vendors have ONE cust account
        //public static Cust xGetCustFromProfileID(int vid)
        //{
        //    Company v = xGetCompany(vid);
        //    Profile p = GetProfile(v.ProfileID);
        //    Cust c = xGetCust(p.CustID);
        //    return c;
        //}

        //public static Dictionary<int, Company> GetCompanyDict(List<Appointment> appts)
        //{
        //    Dictionary<int, Company> ret = new Dictionary<int, Company>();

        //    //foreach (Appointment a in appts)
        //    foreach (int vid in appts.Select(x => x.CalendarID2).Distinct())
        //    {
        //        ProfileCompany v = xGetCompany(vid);
        //        ret.Add(v.ProfileID, v);
        //    }

        //    return ret;
        //}

        //// Want profile name for each calendar
        //// Returns calendar->profilename dict
        //public static Dictionary<int, string> GetProfileNameDict(List<Appointment> appts)
        //{
        //    if (appts == null)
        //        return null;

        //    Dictionary<int, string> ret = new Dictionary<int, string>();

        //    kuyamEntities ctx = DBContext;

        //    // Get list of calendars


        //    string name;
        //    foreach (int cid in appts.Select(x => x.CalendarID1).Distinct())
        //    {
        //        name = (from cal in ctx.Calendars
        //                join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
        //                where cal.CalendarID == cid
        //                select p.Name).FirstOrDefault();
        //        ret.Add(cid, name);
        //    }

        //    return ret;
        //}

        //public static ProfileCompany xGetCompany(int ProfileID)
        //{
        //    return DBContext.ProfileCompanies.SingleOrDefault(x => x.ProfileID == ProfileID);
        //}

        //public static string GetCompanyName(int ProfileID)
        //{
        //    return DBContext.ProfileCompanies.Where(x => x.ProfileID == ProfileID).Select(x => x.Name).FirstOrDefault();
        //}

        //public static List<ProfileHour> GetProfileHours(int ProfileID)
        //{
        //    return DBContext.ProfileHours.Where(v => v.ProfileID == ProfileID).OrderBy(v => v.Day).ThenBy(v => v.Start).ToList();
        //}

        //public static List<Appointment> GetCompanyAppointments(int ProfileID)
        //{
        //    kuyamEntities ctx = DBContext;
        //    List<Appointment> ret =
        //        (from a in ctx.Appointments
        //         join t in ctx.Types on a.AppointmentStatusID equals t.TypeID
        //         orderby t.Sequence descending
        //         where a.CalendarID2 == ProfileID
        //         && a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
        //         select a).ToList();

        //    return ret;
        //    //return DBContext.Appointments.Where(x => x.CustID == custID).ToList();
        //}

        //public static List<ProfileHoursException> GetProfileHoursExceptions(int ProfileID)
        //{
        //    return DBContext.ProfileHoursExceptions.Where(v => v.ProfileID == ProfileID).OrderBy(v => v.Start).ToList();
        //}

        //public static ProfileHoursException GetProfileHoursException(int id)
        //{
        //    return DBContext.ProfileHoursExceptions.Where(h => h.ProfileHoursExceptionID == id).FirstOrDefault();
        //}



        //public static List<AppointmentLogData> GetAppointmentLogs(string username)
        //{
        //    Cust c = xGetCust(username);
        //    DateTime minDate = DateTime.Now.AddDays(-30);
        //    kuyamEntities ctx = DBContext;
        //    var logData = (from l in ctx.AppointmentLogs
        //                   join a in ctx.Appointments on l.AppointmentID equals a.AppointmentID
        //                   join v in ctx.Companies on l.ProfileID equals v.ProfileID
        //                   join cal in ctx.Calendars on a.CalendarID1 equals cal.CalendarID
        //                   join p in ctx.Profiles on cal.ProfileID equals p.ProfileID
        //                   where l.CustID == c.CustID && l.LogDT > minDate && l.Viewed == false
        //                   orderby l.LogDT descending
        //                   select new AppointmentLogData() { Log = l, Appointment = a, ProfileCompany = v, Profile = p }).ToList();
        //    return logData;
        //}

        public static CompanyProfile GetCompanyProfile(int profileID)
        {
            kuyamEntities _context = DBContext;
            CompanyProfile company = null;

            var profile = (from pc in _context.ProfileCompanies
                           where pc.ProfileID == profileID
                           select pc).FirstOrDefault();
            if (profile != null)
            {
                company = new CompanyProfile()
                              {
                                  ID = profileID,
                                  Name = profile.Name,
                                  Phone = profile.Phone,
                                  State = profile.State,
                                  Street = profile.Street1,
                                  Url = profile.Url,
                                  Zip = profile.Zip,
                                  Latitude = profile.Latitude,
                                  Longitude = profile.Longitude,
                                  BusinessHours = GetCompanyHourList(profileID),
                                  Services = GetListServicesByProfileCompanyID(profileID),
                                  Employees = GetEmployeesListByProfileCompanyID(profileID)
                              };
                Medium media = GetCompanyImageFromProfileCompanyID(profileID);

                if (media != null && profile != null)
                {
                    company.ImageUrl = media.LocationData;
                }
            }
            return company;

        }

        public static List<CompanyHour> GetCompanyHourList(int proflieID)
        {
            kuyamEntities _context = DBContext;

            List<CompanyHour> result = _context.CompanyHours
                .Where(c => c.ProfileCompanyID == proflieID)
                .OrderBy(c => c.CompanyHourID)
                .ToList();
            return result;
        }

        public static List<ServiceHour> GetServiceHourList(int serviceID)
        {

            //kuyamEntities _context = DBContext;

            //List<ServiceHour> result = _context.ServiceHours
            //                    .Where(s => s.ServiceID == serviceID)
            //                    .OrderBy(s => s.ServiceHoursID)
            //                    .ToList();
            //return result;

            return new List<ServiceHour>();
        }

        public static List<ServiceHour> GetServicesHourListByServiceIDAndEmployeeID(int serviceId, int employeeID)
        {
            /*
            kuyamEntities _context = DBContext;

            List<ServiceHour> result = _context.ServiceHours
                                .Where(s => s.ServiceID == serviceId &&
                                    (employeeID == 0 || s.EmployeeID == employeeID)
                                )
                                .OrderBy(s => s.ServiceHoursID)
                                .ToList();
            return result;
            */
            return new List<ServiceHour>();
        }

        public static List<ServiceHour> GetServiceHourListByEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;

            List<ServiceHour> result = _context.ServiceHours
                .Where(s => s.EmployeeID == employeeID)
                .OrderBy(s => s.ServiceHoursID)
                .ToList();
            return result;


        }

        public static CompanyHour GetCompanyHourByCompanyProfile(int profileID)
        {

            kuyamEntities _context = DBContext;

            CompanyHour company = (from h in _context.CompanyHours
                                   where h.ProfileCompanyID == profileID
                                   select h).FirstOrDefault();
            return company;
        }

        public static List<CompanyEmployee> GetListEmployeesByCompanyID(int companyID)
        {

            kuyamEntities _context = DBContext;

            List<CompanyEmployee> result = _context.CompanyEmployees
                .Where(e => e.ProfileCompanyID == companyID)
                .OrderBy(e => e.EmployeeID)
                .ToList();
            return result;

        }

        public static List<Service> GetListServicesByProfileCompanyID(int companyID)
        {
            kuyamEntities _context = DBContext;
            //List<Service> result = _context.Services
            //                    .Where(s => s.ProfileCompanyID == companyID)
            //                    .OrderBy(s => s.ServiceID)
            //                    .ToList();
            var query = (from scp in _context.ServiceCompanies
                         join s in _context.Services on scp.ServiceID equals s.ServiceID
                         where scp.ProfileID == companyID
                         select s).Distinct();
            var result = query.ToList();
            return result;
        }

        public static CompanyService GetServiceByServiceID(int serviceID)
        {

            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           where s.ServiceID == serviceID
                           select new CompanyService()
                                      {
                                          ID = s.ServiceID,
                                          Price = sc.Price.Value,
                                          ServiceName = s.ServiceName,
                                          AttendeesNumber = sc.AttendeesNumber.Value,
                                          Description = s.Desc,
                                          Duration = sc.Duration.Value
                                      });
            return service.FirstOrDefault();
        }

        public static List<CompanyEmployee> GetEmployeesList(int serviceID)
        {
            /*
            kuyamEntities _context = DBContext;

            var result = (from s in _context.Services
                          join sh in _context.ServiceHours on s.ServiceID equals sh.ServiceID
                          join ey in _context.CompanyEmployees on sh.EmployeeID equals ey.EmployeeID
                          where s.ServiceID == serviceID
                          select ey).Distinct().ToList();
            return result;
             * */
            return new List<CompanyEmployee>();
        }

        public static List<CompanyEmployee> GetEmployeesListByProfileCompanyID(int profileCompanyID)
        {

            kuyamEntities _context = DBContext;

            List<CompanyEmployee> result = _context.CompanyEmployees
                .Where(e => e.ProfileCompanyID == profileCompanyID)
                .OrderBy(e => e.EmployeeID)
                .ToList();
            return result;
        }

        public static List<CompanyEmployee> GetEmployeesListByProfileIDandPackageID(int profileCompanyID, int packageID)
        {

            kuyamEntities _context = DBContext;

            var query = (from ce in _context.CompanyEmployees
                         join es in _context.EmployeeServices on ce.EmployeeID equals es.CompanyEmployeeID
                         join cps in _context.CompanyPackageServices on es.ServiceCompanyID equals cps.ServiceCompanyId
                         join cp in _context.CompanyPackages on cps.CompanyPackageId equals cp.PackageId
                         where cp.PackageId == packageID && cp.ProfileCompanyId == profileCompanyID
                         select ce).Distinct().ToList();

            return query;
        }

        public static bool AddFavorite(Favorite favorite)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (!_context.Favorites.Any(m => m.ProfileID == favorite.ProfileID && m.CustID == favorite.CustID))
                {
                    _context.Favorites.Add(favorite);
                    _context.Save();

                }
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);                
            }

            return true;
        }

        public static bool DeleteFavorite(Favorite favorite)
        {
            try
            {
                bool result = false;

                kuyamEntities _context = DBContext;
                DBContext.Database.ExecuteSqlCommand("delete from Favorites where FavoriteID=" + favorite.FavoriteID);

                //try
                //{         

                //    _context.Favorites.Remove(favorite);
                //    _context.Save();
                //    result = true;
                //}
                //catch (Exception ex)
                //{
                //    LogHelper.ProcessError(typeof(DAL), ex);
                //    result = false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static List<ProfileCompany> GetFavoriteListByCustID(int custID)
        {
            kuyamEntities _context = DBContext;

            var result = (from f in _context.Favorites
                          join pc in _context.ProfileCompanies on f.ProfileID equals pc.ProfileID
                          where f.CustID == custID && pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                          select pc).Distinct().ToList();
            return result;
        }

        public static Favorite GetFavoriteByCustIDProfileID(int custID, int profileID)
        {
            kuyamEntities _context = DBContext;
            Favorite fav =
                _context.Favorites.Where(x => x.CustID == custID && x.ProfileID == profileID).FirstOrDefault();
            if (fav != null)
                return fav;
            return null;

        }

        public static bool CheckFavoriteByProfileID(int profileID, int custID)
        {
            bool result = false;

            kuyamEntities _context = DBContext;

            List<Favorite> favorites = _context.Favorites
                .Where(f => f.CustID == custID)
                .OrderBy(f => f.FavoriteID)
                .ToList();

            foreach (var favorite in favorites)
            {
                if (favorite.ProfileID == profileID)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static List<ServiceHour> GetServiceHoursByCompanyProfile(int profileId)
        {

            kuyamEntities _context = DBContext;

            var result = (from sh in _context.ServiceHours
                          join e in _context.CompanyEmployees on sh.EmployeeID equals e.EmployeeID
                          join c in _context.ProfileCompanies on e.ProfileCompanyID equals c.ProfileID
                          where c.ProfileID == profileId
                          select sh).Distinct().ToList();
            return result;

        }


        public static ServiceCompany GetFirstServiceByCompanyId(int companyId)
        {
            return DBContext.ServiceCompanies.FirstOrDefault(s => s.ProfileID == companyId);
            //return null;
        }

        public static List<EmployeeHour> GetEmployeeHour(int profileId)
        {
            kuyamEntities context = DBContext;
            var res = new List<EmployeeHour>();

            var result =
                context.EmployeeHours.Where(c => c.CompanyEmployee.ProfileCompanyID == profileId).OrderByDescending(
                    c => c.LastedUpdate).FirstOrDefault();
            if (result != null)
            {
                res =
                    context.EmployeeHours.Where(
                        c => c.CompanyEmployee.ProfileCompanyID == profileId && c.LastedUpdate == result.LastedUpdate).
                        ToList();

            }

            return res;
        }

        public static List<ServiceHour> GetServiceHourListByCompanyProfileId(int profileCompanyId)
        {
            kuyamEntities context = DBContext;

            var result = context.ServiceHours.Where(c => c.ServiceCompany.ProfileID == profileCompanyId).ToList();

            return result;
        }

        public static List<EmployeeHour> GetEmployeeHour(int profileId, int employeeId, int serviceId)
        {
            kuyamEntities context = DBContext;
            //Phuong modify           
            var query = (from emh in context.EmployeeHours
                         join emp in context.CompanyEmployees on emh.CompanyEmployeeID equals emp.EmployeeID
                         join ems in context.EmployeeServices on emp.EmployeeID equals ems.CompanyEmployeeID
                         where emp.ProfileCompanyID == profileId
                         && (serviceId == 0 || ems.ServiceCompanyID == serviceId)
                         && (employeeId == 0 || emp.EmployeeID == employeeId)
                         && !emh.IsPreview // this field use for review or not
                         select emh).Distinct();
            //var result = context.EmployeeHours.Where(c => c.CompanyEmployee.ProfileCompanyID == profileId
            //        && (serviceId == 0 || c.CompanyEmployee.EmployeeServices.Any(s => s.ServiceCompanyID == serviceId))
            //        && (employeeId == 0 || employeeId == -1 || c.CompanyEmployeeID == employeeId)
            //        && !c.IsDaily);
            return query.OrderBy(m => m.DayOfWeek).ToList();
        }

        public static ServiceCompany GetServiceByID(int serviceId)
        {
            kuyamEntities context = DBContext;

            var result =
                context.ServiceCompanies.FirstOrDefault(s => s.ServiceCompanyID == serviceId);

            return result;
        }

        public static CompanyEmployee GetCompanyEmployee(int employeeId)
        {
            kuyamEntities context = DBContext;

            var result =
                context.CompanyEmployees.FirstOrDefault(s => s.EmployeeID == employeeId);

            return result;
        }

        public static void AddAppointment(Appointment appointment, int calendarId)
        {
            kuyamEntities _context = DBContext;

            var query = _context.AppointmentParticipants.Where(m => m.CalendarID == calendarId
                && m.Appointment.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending
                && m.Appointment.CustID == appointment.CustID);
            // for web
            if (appointment.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending)
            {
                appointment.Created = DateTime.UtcNow;
                appointment.Modified = DateTime.UtcNow;
                appointment.StatusChangeDate = DateTime.UtcNow;
                _context.Appointments.Add(appointment);
                AppointmentParticipant appointmentParticipant = new AppointmentParticipant
                {
                    AppointmentID = appointment.AppointmentID,
                    CalendarID = calendarId,
                    ParticipantStatusID = 0,
                    ParticipantTypeID = 0
                };
                _context.AppointmentParticipants.Add(appointmentParticipant);

            }// for mobile
            else
            {
                if (!query.Any())
                {
                    appointment.Created = DateTime.UtcNow;
                    appointment.Modified = DateTime.UtcNow;
                    appointment.StatusChangeDate = DateTime.UtcNow;
                    _context.Appointments.Add(appointment);
                    AppointmentParticipant appointmentParticipant = new AppointmentParticipant
                    {
                        AppointmentID = appointment.AppointmentID,
                        CalendarID = calendarId,
                        ParticipantStatusID = 0,
                        ParticipantTypeID = 0
                    };
                    _context.AppointmentParticipants.Add(appointmentParticipant);

                }

                else
                {
                    var apptId = query.FirstOrDefault().AppointmentID;
                    var appt = query.FirstOrDefault().Appointment;

                    appt.ServiceCompanyID = appointment.ServiceCompanyID;
                    appointment.ServiceName = appointment.ServiceName;
                    appt.AppointmentStatusID = (int)Types.AppointmentStatus.TemporaryPending;
                    appt.StatusChangeDate = DateTime.UtcNow;
                    appt.Title = appointment.Title;
                    appt.ContactPerson = appointment.ContactPerson;
                    appt.AllDay = appointment.AllDay;
                    appt.Duration = appointment.Duration;
                    appt.Price = appointment.Price;
                    appt.Start = appointment.Start;
                    appt.End = appointment.End;
                    appt.Url = appointment.Url;
                    appt.PersonCount = 1;
                    appt.Notes = appointment.Notes;
                    appt.Created = DateTime.UtcNow;
                    appt.Modified = DateTime.UtcNow;
                    appt.CustID = appointment.CustID;
                    appt.EmployeeID = appointment.EmployeeID;
                    appt.EmployeeName = appointment.EmployeeName;
                    appt.HotelID = appointment.HotelID;
                    appointment.AppointmentID = apptId;

                }
            }
            _context.SaveChanges();

        }



        public static void UpdateAppointment(Appointment appointment)
        {
            kuyamEntities _context = DBContext;
            _context.Save();
        }

        public static void AddAppointmentTemp(AppointmentTemp appointmentTemp)
        {
            kuyamEntities _context = DBContext;

            appointmentTemp.Created = DateTime.UtcNow;
            _context.AppointmentTemps.Add(appointmentTemp);

            _context.Save();
        }

        public static void DeleteAppointmentTemp(int appointmentTempId)
        {
            kuyamEntities _context = DBContext;
            var appointmentTemp = _context.AppointmentTemps.FirstOrDefault(c => c.AppointmentID == appointmentTempId);
            if (appointmentTemp != null)
            {
                appointmentTemp.AppointmentStatusID = (int)Types.AppointmentStatus.Delete;
                _context.SaveChanges();
            }
        }


        public static List<KuyamEvent> GetAvailableKuyamEvents(int custId)
        {
            kuyamEntities _context = DBContext;

            return
                _context.KuyamEvents.Where(
                    e =>
                    e.Appointment.CustID == custId && e.Appointment.Start > DateTime.Today &&
                    e.Appointment.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled).
                    ToList();
        }

        public static bool IsExistEmailAddress(string email)
        {
            kuyamEntities _context = DBContext;
            return _context.Invites.Count(i => i.Email == email && (i.Active == true || i.Status == (int)Types.UserInviteCodeStatusType.Active)) > 0;
        }

        public static bool IsExistCust(string email)
        {
            kuyamEntities _context = DBContext;
            return _context.Custs.Count(m => m.aspnet_Users.UserName == email) > 0;
        }

        public static bool IsExistEmailAddressInUsers(string email)
        {
            kuyamEntities _context = DBContext;
            var cust = (from u in _context.aspnet_Users
                        join c in _context.Custs on u.UserId equals c.AspUserID
                        where u.UserName == email && c.Status != (int)Types.UserStatusType.Deleted
                        select u);

            return cust.Count() > 0;
        }
        public static List<Service> GetServicebyEmployeeId(int employeeId)
        {
            kuyamEntities _context = DBContext;
            var lst = (from s in _context.EmployeeServices
                       where s.CompanyEmployeeID == employeeId
                       select s.ServiceCompany.Service).Distinct();
            return lst.ToList();
        }

        public static List<CompanyService> GetServiceCompanybyEmployeeId(int profileId, int employeeId, int serviceId, int categoryId = 0)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           join e in _context.EmployeeServices on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where (profileId == 0 || sc.ProfileID == profileId)
                                 && (categoryId == 0 || categoryId == null || s.ParentServiceID == categoryId)
                                 && (employeeId == 0 || e.CompanyEmployeeID == employeeId)
                                 && (serviceId == 0 || sc.ServiceCompanyID == serviceId)
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                                      {
                                          ID = sc.ServiceCompanyID,
                                          Price = sc.Price ?? 0,
                                          ServiceID = s.ServiceID,
                                          ServiceName = s.ServiceName,
                                          AttendeesNumber = sc.AttendeesNumber ?? 0,
                                          Description = s.Desc,
                                          Duration = sc.Duration ?? 0,
                                          EmployeeID = e.CompanyEmployeeID,
                                          EmployeeName = string.Empty,
                                      });
            return service.ToList();
        }


        public static List<CompanyService> GetGeneralServiceCompanybyEmployeeId(int employeeId, int profileId, int categoryId = 0)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           where (profileId == 0 || sc.ProfileID == profileId)
                                 && (categoryId == 0 || categoryId == null || s.ParentServiceID == categoryId)
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price ?? 0,
                               ServiceID = s.ServiceID,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber ?? 0,
                               Description = s.Desc,
                               Duration = sc.Duration ?? 0,
                               EmployeeID = 0,
                               EmployeeName = string.Empty,
                           });
            return service.ToList();
        }

        public static List<CompanyService> GetServiceCompanybyEmployeeIds(List<int> employeeIds)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           join e in _context.EmployeeServices on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where employeeIds.Contains(e.CompanyEmployeeID)
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price ?? 0,
                               ServiceID = s.ServiceID,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber ?? 0,
                               Description = s.Desc,
                               Duration = sc.Duration ?? 0
                           }).Distinct();
            return service.ToList();
        }


        public static List<CompanyService> GetServiceCompanybyPackageID(int packageID)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           join cps in _context.CompanyPackageServices on sc.ServiceCompanyID equals cps.ServiceCompanyId
                           where sc.Status == (int)Types.ServiceCompanyStatus.Active && cps.CompanyPackageId == packageID
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.Value,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.Value,
                               Description = s.Desc,
                               Duration = sc.Duration.Value
                           }).Distinct();
            return service.ToList();
        }


        public static CompanyService GetServiceCompanybyServiceCompanyID(int serviceCompanyID)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           where sc.ServiceCompanyID == serviceCompanyID
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.Value,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.Value,
                               Description = s.Desc,
                               Duration = sc.Duration.Value
                           }).FirstOrDefault();
            return service;
        }

        public static List<CompanyService> GetServiceCompanybyProfileId(int profileId)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           join e in _context.EmployeeServices on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where sc.ProfileID == profileId
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.Value,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.Value,
                               Description = s.Desc,
                               Duration = sc.Duration.Value
                           }).OrderBy(x => x.ServiceName).Distinct();
            return service.ToList();
        }



        public static List<CompanyService> GetServiceEmployeeByPackage(int employeeId, int packageId, int profileId, int categoryId = 0)
        {
            kuyamEntities _context = DBContext;

            var service = (from s in _context.Services
                           join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                           join e in _context.EmployeeServices on sc.ServiceCompanyID equals e.ServiceCompanyID
                           join pkg in _context.CompanyPackageServices on sc.ServiceCompanyID equals pkg.ServiceCompanyId
                           join upkg in _context.UserPackagePurchases on pkg.CompanyPackageId equals upkg.CompanyPackageId
                           where (employeeId == 0 || e.CompanyEmployeeID == employeeId) && upkg.UserPackagePurchaseId == packageId
                           && sc.ProfileID == profileId
                           && (categoryId == 0 || categoryId == null || s.ParentServiceID == categoryId)
                           && sc.Status == (int)Types.ServiceCompanyStatus.Active
                           && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.Value,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.Value,
                               Description = s.Desc,
                               Duration = sc.Duration.Value,
                               EmployeeID = e.CompanyEmployeeID,
                               EmployeeName = e.CompanyEmployee.EmployeeName
                           });
            return service.ToList();
        }

        public static List<Service> GetParentService(bool getActiveOnly = true)
        {
            kuyamEntities _context = DBContext;
            //var query = _context.Services.Where(x => x.ParentServiceID == null);
            //if (getActiveOnly)
            //    query = query.Where(x => x.Status == null || x.Status == true);
            return _context.Services.Where(x => (x.ParentServiceID == null) && (x.Status == null || x.Status == true)).OrderBy(o => o.ServiceName).ToList();
        }

        public static List<CategoryIP> GetCategoriesIP()
        {
            try
            {
                kuyamEntities _context = DBContext;
                var query = (from s in _context.Services
                             where s.ParentServiceID == null && s.Sequence != null
                             && (s.Status == null || s.Status == true)
                             select new CategoryIP()
                             {
                                 ServiceID = s.ServiceID,
                                 ServiceName = s.ServiceName,
                                 Desc = s.Desc,
                                 ParentServiceID = s.ParentServiceID,
                                 Sequence = s.Sequence,
                                 Status = s.Status,
                                 KalturaId = s.kalturaId
                             }).OrderBy(x => x.Sequence).Take(12).ToList();
                return query;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Service> GetServicesByCategoryID(int categoryID, bool getActiveOnly = true)
        {
            kuyamEntities _context = DBContext;
            List<Service> services = new List<Service>();
            var query = (from sc in _context.ServiceCompanies
                             join s in _context.Services on sc.ServiceID equals s.ServiceID
                             where s.ParentServiceID == categoryID &&( sc.ServiceTypeId != (int)Types.ServiceType.ClassType)
                             select s).Distinct().ToList();
            if (query != null && query.Count > 0)
                return query;
            return null;
           
        }

        public static Service GetService(int serviceID)
        {
            kuyamEntities _context = DBContext;
            if (_context.Services.Any(x => x.ServiceID == serviceID))
            {
                return _context.Services.Where(x => x.ServiceID == serviceID).FirstOrDefault();
            }
            return null;
        }

        public static bool AddServiceCompany(ServiceCompany serviceCompany)
        {
            kuyamEntities _context = DBContext;
            try
            {
                _context.ServiceCompanies.Add(serviceCompany);
                _context.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<EmployeeService> GetEmployeeServicesFromEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;
            if (_context.EmployeeServices.Any(x => x.CompanyEmployeeID == employeeID))
            {
                return _context.EmployeeServices.Where(x => x.CompanyEmployeeID == employeeID).ToList();
            }

            return null;


        }

        public static bool UpdateServiceCompany(ServiceCompany serviceCompany)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.ServiceCompanies.Any(x => x.ServiceCompanyID == serviceCompany.ServiceCompanyID))
                {
                    ServiceCompany sc =
                        _context.ServiceCompanies.Where(x => x.ServiceCompanyID == serviceCompany.ServiceCompanyID).
                            FirstOrDefault();
                    sc.Duration = serviceCompany.Duration;
                    sc.Price = serviceCompany.Price;
                    sc.AttendeesNumber = serviceCompany.AttendeesNumber;
                    sc.Modified = DateTime.UtcNow;
                    sc.ServiceID = serviceCompany.ServiceID;
                    sc.FromDateTime = serviceCompany.FromDateTime;
                    sc.ToDateTime = serviceCompany.ToDateTime;
                    _context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static bool UpdateCompanyEmployeeInfo(CompanyEmployee companyEmployee)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.CompanyEmployees.Any(x => x.EmployeeID == companyEmployee.EmployeeID))
                {
                    CompanyEmployee ce =
                        _context.CompanyEmployees.Where(x => x.EmployeeID == companyEmployee.EmployeeID).FirstOrDefault();
                    ce.EmployeeName = companyEmployee.EmployeeName;
                    ce.Email = companyEmployee.Email;
                    ce.Phone = companyEmployee.Phone;
                    ce.IsDefault = companyEmployee.IsDefault;
                    _context.SaveChanges();
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool DeleteEmployeeServicesByEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.EmployeeServices.Any(x => x.CompanyEmployeeID == employeeID))
                {
                    List<EmployeeService> esList =
                        _context.EmployeeServices.Where(x => x.CompanyEmployeeID == employeeID).ToList();
                    foreach (EmployeeService es in esList)
                    {
                        _context.EmployeeServices.Remove(es);
                        _context.SaveChanges();
                    }
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static bool DeleteEmployeeByEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.CompanyEmployees.Any(x => x.EmployeeID == employeeID))
                {
                    CompanyEmployee employee =
                        _context.CompanyEmployees.Where(x => x.EmployeeID == employeeID).FirstOrDefault();
                    _context.CompanyEmployees.Remove(employee);
                    _context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<CompanyEmployee> GetCompanyEmployeesByProfileID(int profileID)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.CompanyEmployees.Any(x => x.ProfileCompanyID == profileID))
                {
                    List<CompanyEmployee> ceList =
                        _context.CompanyEmployees.Where(x => x.ProfileCompanyID == profileID).ToList();
                    return ceList;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool DeleteServiceCompany(int serviceCompanyID)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.ServiceCompanies.Any(x => x.ServiceCompanyID == serviceCompanyID))
                {
                    ServiceCompany sc =
                        _context.ServiceCompanies.Where(x => x.ServiceCompanyID == serviceCompanyID).FirstOrDefault();
                    sc.Status = (int)Types.ServiceCompanyStatus.Delete;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<ServiceCompany> GetCompanySevicesByProfileID(int profileID)
        {
            kuyamEntities _context = DBContext;
            List<ServiceCompany> scList = new List<ServiceCompany>();
            if (_context.ServiceCompanies.Any(x => x.ProfileID == profileID))
            {
                scList =
                    _context.ServiceCompanies.Where(
                        x =>
                        x.ProfileID == profileID && x.Service.ParentServiceID != null &&  x.ServiceTypeId != (int)Types.ServiceType.ClassType && 
                        x.Status != (int)Types.ServiceCompanyStatus.Delete).
                        ToList();
                return scList;
            }
            return scList;
        }

        public static List<ServiceCompany> GetCompanyServiceByEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;
            var ret = (from sc in _context.ServiceCompanies
                       join es in _context.EmployeeServices on sc.ServiceCompanyID equals es.ServiceCompanyID
                       where es.CompanyEmployeeID == employeeID
                       select sc).Distinct().ToList();

            return ret;
        }


        public static ServiceCompany GetServiceCompany(int serviceCompanyID)
        {
            kuyamEntities _context = DBContext;
            if (_context.ServiceCompanies.Any(x => x.ServiceCompanyID == serviceCompanyID))
            {
                return
                    _context.ServiceCompanies.Where(
                        x =>
                        x.ServiceCompanyID == serviceCompanyID && x.Status != (int)Types.ServiceCompanyStatus.Delete).
                        FirstOrDefault();
            }
            return null;

        }

        public static int AddEmployee(CompanyEmployee employee)
        {
            kuyamEntities _context = DBContext;
            try
            {
                _context.CompanyEmployees.Add(employee);
                _context.Save();
                return employee.EmployeeID;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public static bool AddEmployeeService(EmployeeService employeeService)
        {
            kuyamEntities _context = DBContext;
            try
            {
                _context.EmployeeServices.Add(employeeService);
                _context.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Determines whether is employee hour editable
        /// </summary>
        /// <param name="oldHour">The old hour.</param>
        /// <param name="newStart">The new start time.</param>
        /// <param name="newEnd">The new end time.</param>
        /// <returns>
        ///   <c>true</c> if [is employee hour editable] [the specified old hour]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmployeeHourEditable(EmployeeHour oldHour, TimeSpan newStart, TimeSpan newEnd)
        {
            DateTime date = DateTime.UtcNow.Date;
            int day = (int)date.DayOfWeek;
            date = oldHour.DayOfWeek < day ? date.AddDays(7 - day + oldHour.DayOfWeek) : date.AddDays(oldHour.DayOfWeek - day);
            DateTime oldStartTime = date.AddMinutes(oldHour.FromHour.TotalMinutes);
            DateTime oldEndTime = date.AddMinutes(oldHour.ToHour.TotalMinutes);
            DateTime newStartTime = date.AddMinutes(newStart.TotalMinutes);
            DateTime newEndTime = date.AddMinutes(newEnd.TotalMinutes);

            // get appointments is booked within old employee hour 
            var appointments = GetAppoinmentsByEmployeeId(oldHour.CompanyEmployeeID, oldStartTime, oldEndTime);

            // get all employee hours except which need to change
            var employeeHours = GetEmployeeHoursFromEmployeeID(oldHour.CompanyEmployeeID)
                    .Where(h => h.ID != oldHour.ID && h.DayOfWeek == oldHour.DayOfWeek).ToList();

            // add employee hour after update time
            employeeHours.Add(new EmployeeHour
            {
                CompanyEmployeeID = oldHour.CompanyEmployeeID,
                FromHour = newStart,
                ToHour = newEnd,
                DayOfWeek = oldHour.DayOfWeek
            });


            // loop all appointments
            foreach (Appointment appointment in appointments)
            {

                if (appointment.Start >= newStartTime && appointment.End <= newEndTime)
                    continue;

                bool isValid = true;

                TimeSpan appoinmentStart = appointment.Start.TimeOfDay;
                TimeSpan appoinmentEnd = appointment.End.TimeOfDay;
                while (true)
                {
                    // get employee hours intersec with appointment
                    var employeeHoursCoverAppointment =
                        employeeHours.Where(h => h.ToHour > appoinmentStart && h.FromHour < appoinmentEnd).ToList();

                    // if not exist any employee hour, return false
                    if (!employeeHoursCoverAppointment.Any())
                    {
                        isValid = false;
                        break;
                    }


                    // get employee hour cover start time
                    var employeeHourCoverStart =
                        employeeHoursCoverAppointment.Where(h => h.FromHour <= appoinmentStart)
                                                     .OrderByDescending(h => h.ToHour)
                                                     .FirstOrDefault();
                    if (employeeHourCoverStart == null)
                    {
                        isValid = false;
                        break;
                    }

                    // update appointment time, subtract time is covered
                    appoinmentStart = employeeHourCoverStart.ToHour;
                    if (appoinmentStart >= appoinmentEnd)
                        break;
                }

                if (!isValid)
                    return false;

            }
            return true;
        }


        public static bool AddEmployeeHour(EmployeeHour employeeHour, string[] listDay)
        {
            try
            {
                kuyamEntities _context = DBContext;

                // Parse list day
                List<int> listDays = new List<int>();
                foreach (string day in listDay)
                {
                    int dayofweek = -1;
                    switch (day)
                    {
                        case "Monday":
                            {
                                dayofweek = (int)Types.Day.Monday;
                                break;
                            }
                        case "Tuesday":
                            {
                                dayofweek = (int)Types.Day.Tuesday;
                                break;
                            }
                        case "Wednesday":
                            {
                                dayofweek = (int)Types.Day.Wednesday;
                                break;
                            }
                        case "Thursday":
                            {
                                dayofweek = (int)Types.Day.Thursday;
                                break;
                            }
                        case "Friday":
                            {
                                dayofweek = (int)Types.Day.Friday;
                                break;
                            }
                        case "Saturday":
                            {
                                dayofweek = (int)Types.Day.Saturday;
                                break;
                            }
                        case "Sunday":
                            {
                                dayofweek = (int)Types.Day.Sunday;
                                break;
                            }
                        case "Isdaily":
                            {
                                dayofweek = (int)Types.Day.Isdaily;
                                break;
                            }
                        default:
                            {
                                int.TryParse(day, out dayofweek);
                                break;
                            }
                    }
                    listDays.Add(dayofweek);
                }


                // Validate new employee hours

                CompanyEmployee employee = DAL.GetCompanyEmployee(employeeHour.CompanyEmployeeID);
                List<CompanyHour> companyHours = DAL.GetCompanyHourList(employee.ProfileCompanyID);
                if (companyHours == null || companyHours.Count == 0)
                    return false;

                bool isValid = true;
                foreach (int day in listDays)
                {
                    TimeSpan start = employeeHour.FromHour;
                    TimeSpan end = employeeHour.ToHour;

                    // Get company hours of day
                    var companyHoursOfDay = companyHours.Where(h => h.DayOfWeek == day || h.IsDaily == true);

                    while (true)
                    {
                        // get company hours intersec with new preview hour
                        companyHoursOfDay = companyHoursOfDay.Where(h => h.ToHour > start && h.FromHour < end);

                        // if not exist any company hour, return false
                        if (companyHoursOfDay == null || companyHoursOfDay.Count() == 0)
                        {
                            isValid = false;
                            break;
                        }


                        // get company hour cover start time
                        var companyHourCoverStart =
                            companyHoursOfDay.Where(h => h.FromHour <= start)
                                             .OrderByDescending(h => h.ToHour)
                                             .FirstOrDefault();
                        if (companyHourCoverStart == null)
                        {
                            isValid = false;
                            break;
                        }

                        start = companyHourCoverStart.ToHour;

                        if (start >= end)
                            break;
                    }

                    if (!isValid)
                    {
                        return false;
                    }
                }


                // Delete all preview hours of employee
                List<EmployeeHour> listemployeeHourPreview =
                    _context.EmployeeHours.Where(
                        m => m.CompanyEmployeeID == employeeHour.CompanyEmployeeID && m.IsPreview == true).ToList();
                foreach (EmployeeHour item in listemployeeHourPreview)
                {
                    _context.EmployeeHours.Remove(item);
                    _context.Save();
                }


                foreach (var dayofweek in listDays)
                {
                    employeeHour.DayOfWeek = dayofweek;
                    List<EmployeeHour> listemployeeHour =
                        _context.EmployeeHours.Where(
                            m =>
                            m.CompanyEmployeeID == employeeHour.CompanyEmployeeID &&
                            (dayofweek == -1 || m.DayOfWeek == dayofweek) && m.IsPreview == false)
                                .OrderBy(m => m.FromHour)
                                .ToList();

                    if (employeeHour.IsPreview || listemployeeHour == null || listemployeeHour.Count <= 0)
                    {

                        _context.EmployeeHours.Add(employeeHour);
                        _context.Save();
                    }
                    else
                    {
                        listemployeeHour.Add(employeeHour);

                        var listRanger = CollapseEmployeeTime(TranslateFromEmployeeHourToRangEmployeeHour(listemployeeHour));

                        //Remove all old EmployeeHours
                        foreach (EmployeeHour item in listemployeeHour)
                        {
                            if (_context.EmployeeHours.Any(x => x.ID == item.ID))
                            {
                                _context.EmployeeHours.Remove(item);
                            }
                        }
                        _context.Save();

                        //Add new EmployeeHours
                        foreach (var rItem in listRanger)
                        {
                            EmployeeHour eh = new EmployeeHour();
                            eh.CompanyEmployeeID = employeeHour.CompanyEmployeeID;
                            eh.FromHour = rItem.Start;
                            eh.ToHour = rItem.End;
                            eh.IsPreview = employeeHour.IsPreview;
                            eh.LastedUpdate = DateTime.UtcNow;
                            eh.DayOfWeek = dayofweek;
                            _context.EmployeeHours.Add(eh);
                        }
                        _context.Save();

                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static List<RangeEmployeeHour> TranslateFromEmployeeHourToRangEmployeeHour(List<EmployeeHour> listEmployeeHour)
        {
            if (listEmployeeHour == null || listEmployeeHour.Count == 0)
                return null;
            var query = from eh in listEmployeeHour
                        select new RangeEmployeeHour
                        {
                            Start = eh.FromHour,
                            End = eh.ToHour
                        };
            return query.ToList();
        }

        private static List<RangeEmployeeHour> CollapseEmployeeTime(this List<RangeEmployeeHour> me)
        {
            List<RangeEmployeeHour> orderdList = me.OrderBy(r => r.Start).ToList();
            List<RangeEmployeeHour> newList = new List<RangeEmployeeHour>();

            var max = orderdList[0].End;
            var min = orderdList[0].Start;

            foreach (var item in orderdList.Skip(1))
            {
                if ((item.End > max) && (item.Start > max))
                {
                    newList.Add(new RangeEmployeeHour { Start = min, End = max });
                    min = item.Start;
                }
                max = max > item.End ? max : item.End;
            }
            newList.Add(new RangeEmployeeHour { Start = min, End = max });

            return newList;
        }


        public static string GetCompanyMediaURL(int companyId, Types.CompanyMediaType companyMediaType)
        {
            kuyamEntities _context = DBContext;
            string strUrl = string.Empty;
            if (companyMediaType == Types.CompanyMediaType.IsBanner)
            {
                var service = (from s in _context.CompanyMedias
                               where s.ProfileID == companyId && s.IsBanner
                               select s).FirstOrDefault();
                if (service != null)
                    strUrl = service.Medium.LocationPath;
            }
            else if (companyMediaType == Types.CompanyMediaType.IsLogo)
            {
                var service = (from s in _context.CompanyMedias
                               where s.ProfileID == companyId && s.IsLogo
                               select s).FirstOrDefault();
                if (service != null)
                    strUrl = service.Medium.LocationPath;
            }
            else if (companyMediaType == Types.CompanyMediaType.IsVideo)
            {
                var service = (from s in _context.CompanyMedias
                               where s.ProfileCompany.ProfileID == companyId && s.IsVideo
                               select s).FirstOrDefault();
                if (service != null)
                    strUrl = service.Medium.LocationData;
            }
            return strUrl;
        }

        internal static bool RemoveFromFavorite(int companyProfileId)
        {
            bool result = false;

            kuyamEntities _context = DBContext;

            try
            {
                var favor = _context.Favorites.FirstOrDefault(p => p.ProfileID == companyProfileId);
                if (favor != null)
                {
                    _context.Favorites.Remove(favor);
                    _context.SaveChanges();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                result = false;
            }

            return result;
        }

        internal static bool RemoveFromFavorite(int companyProfileId, int custId)
        {
            bool result = false;

            kuyamEntities _context = DBContext;

            try
            {
                var favor = _context.Favorites.FirstOrDefault(p => p.ProfileID == companyProfileId && p.CustID == custId);
                if (favor != null)
                {
                    _context.Favorites.Remove(favor);
                    _context.SaveChanges();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                result = false;
            }

            return result;
        }



        internal static List<CompanyEmployee> GetEmployeeByProfileId(int profileId)
        {
            kuyamEntities _context = DBContext;

            try
            {
                return _context.CompanyEmployees.Where(e => e.ProfileCompanyID == profileId).ToList();

            }
            catch (Exception ex)
            {
                return new List<CompanyEmployee>();
            }

        }

        public static bool UpdateLastedTimeForEmployeeHour(int employeeID, DateTime lasted)
        {
            try
            {
                kuyamEntities _context = DBContext;
                DateTime start = DateTime.Now.AddDays(-6).Date;
                DeleteEmployeeHourPreview(employeeID);
                DeleteEmployeeHourExpired(employeeID);
                if (_context.EmployeeHours.Any(x => x.CompanyEmployeeID == employeeID && x.LastedUpdate >= start))
                {
                    List<EmployeeHour> ehList =
                        _context.EmployeeHours.Where(x => x.CompanyEmployeeID == employeeID && x.LastedUpdate >= start).
                            ToList();
                    if (ehList != null && ehList.Count > 0)
                    {
                        foreach (EmployeeHour eh in ehList)
                        {
                            if (eh.DayOfWeek > 0)
                            {
                                string tempDayOfWeek = eh.DayOfWeek.ToString();
                                string stringDayOfWeek = string.Empty;
                                int subDay = DateTime.Now.Day - eh.LastedUpdate.Day;
                                for (int i = 0; i < tempDayOfWeek.Length; i++)
                                {
                                    if ((int.Parse(tempDayOfWeek[i].ToString()) % 7) >= subDay)
                                    {
                                        int addDay = (int.Parse(tempDayOfWeek[i].ToString()) % 7) - subDay;
                                        if (addDay == 0)
                                            addDay = 7;
                                        stringDayOfWeek = stringDayOfWeek + addDay.ToString();
                                    }
                                }
                                int days = 0;
                                int.TryParse(stringDayOfWeek, out days);
                                eh.DayOfWeek = days;
                                eh.LastedUpdate = lasted;
                                _context.SaveChanges();
                            }
                        }
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool DeleteEmployeeHourExpired(int employeeID)
        {
            try
            {
                kuyamEntities _context = DBContext;
                if (_context.EmployeeHours.Any(x => x.CompanyEmployeeID == employeeID && x.DayOfWeek <= 0))
                {
                    List<EmployeeHour> ehList =
                        _context.EmployeeHours.Where(x => x.CompanyEmployeeID == employeeID && x.DayOfWeek <= 0).ToList();

                    if (ehList != null && ehList.Count > 0)
                    {
                        foreach (EmployeeHour eh in ehList)
                        {
                            _context.EmployeeHours.Remove(eh);
                            _context.SaveChanges();
                        }
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool DeleteEmployeeHourPreview(int employeeID)
        {
            try
            {
                kuyamEntities _context = DBContext;
                if (_context.EmployeeHours.Any(x => x.CompanyEmployeeID == employeeID && x.IsPreview))
                {
                    List<EmployeeHour> ehList =
                        _context.EmployeeHours.Where(x => x.CompanyEmployeeID == employeeID && x.IsPreview).ToList();

                    if (ehList != null && ehList.Count > 0)
                    {
                        foreach (EmployeeHour eh in ehList)
                        {
                            _context.EmployeeHours.Remove(eh);
                            _context.SaveChanges();
                        }
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<EmployeeHour> GetEmployeeHourPreview(int employeeID)
        {
            try
            {
                kuyamEntities _context = DBContext;
                if (_context.EmployeeHours.Any(x => x.CompanyEmployeeID == employeeID && x.IsPreview))
                {
                    var eh = _context.EmployeeHours.Where(x => x.CompanyEmployeeID == employeeID && x.IsPreview);

                    return eh.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public static bool DeleteEmployeeHour(int employeeHourID, int employeeID)
        {
            try
            {
                kuyamEntities _context = DBContext;
                if (_context.EmployeeHours.Any(x => x.ID == employeeHourID))
                {
                    EmployeeHour ehList =
                        _context.EmployeeHours.Where(x => x.ID == employeeHourID && x.CompanyEmployeeID == employeeID).
                            FirstOrDefault();
                    _context.EmployeeHours.Remove(ehList);
                    _context.SaveChanges();
                    //if (ehList != null && ehList.Count > 0)
                    //{
                    //    foreach (EmployeeHour eh in ehList)
                    //    {
                    //        int subDays = (dateDelete - eh.LastedUpdate.Date).Days;
                    //        if (subDays == 0)
                    //            subDays = 7;
                    //        if (eh.DayOfWeek > 0)
                    //        {
                    //            string tempDayOfWeek = eh.DayOfWeek.ToString();
                    //            string stringDayOfWeek = string.Empty;
                    //            for (int i = 0; i < tempDayOfWeek.Length; i++)
                    //            {
                    //                if (int.Parse(tempDayOfWeek[i].ToString()) != subDays)
                    //                {
                    //                    stringDayOfWeek = stringDayOfWeek + tempDayOfWeek[i];
                    //                }
                    //            }
                    //            int days = 0;
                    //            int.TryParse(stringDayOfWeek, out days);
                    //            eh.DayOfWeek = days;
                    //            _context.SaveChanges();
                    //        }
                    //    }
                    //}
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<EmployeeHour> GetEmployeeHoursFromEmployeeID(int employeeID)
        {
            kuyamEntities _context = DBContext;
            if (_context.EmployeeHours.Any(x => x.CompanyEmployeeID == employeeID))
            {
                List<EmployeeHour> ehList =
                    _context.EmployeeHours.Where(x => x.CompanyEmployeeID == employeeID).ToList();
                return ehList;
            }
            return null;
        }

        public static EmployeeHour GetEmployeeHourByemployeeHourID(int employeeHourID)
        {
            kuyamEntities _context = DBContext;
            if (_context.EmployeeHours.Any(x => x.ID == employeeHourID))
            {
                EmployeeHour ehList = _context.EmployeeHours.Where(x => x.ID == employeeHourID).FirstOrDefault();
                return ehList;
            }
            return null;
        }


        internal static List<Appointment> GetAppointmentByProfileId(int profileId)
        {
            kuyamEntities _context = DBContext;
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now).Date;
            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.ProfileId == profileId || a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId)
                        && a.Start >= today
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete).OrderBy(a => a.Start)
                        .ToList();
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return new List<Appointment>();
            }
        }

        /// <summary>
        /// Paging
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        internal static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId)
        {
            var _context = DBContext;
            var today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.ProfileId == profileId || a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId)
                        && a.Start >= today
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete).OrderBy(a => a.Start);
                //.ToList();
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return null;
            }
        }

        public static Appointment GetAppointmentById(int Id)
        {
            kuyamEntities _context = DBContext;
            try
            {
                //var favor = DBContext.Appointments.Where(m => m.AppointmentID == Id).FirstOrDefault();
                return DBContext.Appointments.Where(m => m.AppointmentID == Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static List<Appointment> GetAppointmentByProfileId(int profileId, int employeeId)
        {
            kuyamEntities _context = DBContext;

            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId) &&
                        a.Start >= DateTime.Now && a.EmployeeID == employeeId).
                        ToList();
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return new List<Appointment>();
            }
        }

        internal static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId, int employeeId)
        {
            kuyamEntities _context = DBContext;

            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId) &&
                        a.Start >= DateTime.Now && a.EmployeeID == employeeId).OrderBy(a => a.Start);
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return null;
            }
        }

        internal static IQueryable<Appointment> GetAppoinmentsByEmployeeId(int employeeId, DateTime startDate, DateTime endDate)
        {
            kuyamEntities _context = DBContext;
            return _context.Appointments.Where(
                    p => (employeeId <= 0 || p.EmployeeID == employeeId) && p.Start >= startDate && p.Start < endDate
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                         && p.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                    );
        }

        internal static void CancelAppointment(int appId, string reason)
        {
            kuyamEntities _context = DBContext;
            var app = _context.Appointments.FirstOrDefault(a => a.AppointmentID == appId);
            if (app != null)
            {
                app.AppointmentStatusID = (int)Types.AppointmentStatus.Cancelled;
                app.Desc = reason;
                _context.SaveChanges();
            }
        }

        internal static List<Appointment> GetKuyamEventsByEmployeeId(int employeeId, DateTime start, DateTime end)
        {
            kuyamEntities _context = DBContext;
            return
                _context.Appointments.Where(
                    e =>
                    e.EmployeeID == employeeId && e.Start > start && e.Start < end &&
                    e.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled).
                    ToList();
        }

        internal static void RemoveAppointment(int appId)
        {
            kuyamEntities _context = DBContext;
            var app = _context.Appointments.Where(
                e =>
                e.AppointmentID == appId).FirstOrDefault();
            if (app != null)
            {
                app.AppointmentStatusID = (int)Types.AppointmentStatus.Delete;
                /*
                List<KuyamEvent> lst = app.KuyamEvents.ToList();
                for (int i = 0; i < lst.Count; i++)
                {
                    _context.KuyamEvents.Remove(lst[0]);
                }
                List<AppointmentLog> logs = app.AppointmentLogs.ToList();
                for (int i = 0; i < logs.Count; i++)
                {
                    _context.AppointmentLogs.Remove(logs[i]);
                }

                _context.Appointments.Remove(app);
                 */
                _context.SaveChanges();
            }
        }


        internal static void Confirm(int appId)
        {
            kuyamEntities _context = DBContext;
            var app = _context.Appointments.Where(
                e =>
                e.AppointmentID == appId).FirstOrDefault();
            if (app != null)
            {
                app.AppointmentStatusID = (int)Types.AppointmentStatus.Confirmed;
                _context.SaveChanges();
            }
        }

        internal static List<Appointment> GetAppointmentByProfileId(int profileId, List<int> lstStatus)
        {
            kuyamEntities _context = DBContext;
            DateTime today = DateTimeUltility.ConvertToUserTime(DateTime.Now).Date;
            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.ProfileId == profileId || a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId) &&
                        a.Start >= today && lstStatus.Contains(a.AppointmentStatusID)).OrderBy(a => a.Start)
                        .ToList();
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return new List<Appointment>();
            }
        }

        internal static IQueryable<Appointment> GetAppointmentByProfileIdV2(int profileId, List<int> lstStatus)
        {
            var _context = DBContext;
            var today = DateTimeUltility.ConvertToUserTime(DateTime.Now);
            try
            {
                var favor =
                    _context.Appointments.Where(
                        a =>
                        (a.ProfileId == profileId || a.CompanyEmployee.ProfileCompanyID == profileId || a.ServiceCompany.ProfileID == profileId) &&
                        a.Start >= today && lstStatus.Contains(a.AppointmentStatusID)).OrderBy(a => a.Start);
                return favor;
            }
            catch (Exception ex)
            {
                //LogHelper.ProcessError(typeof(DAL), ex);
                return null;
            }
        }

        //------Trong Edit------
        /// <summary>
        /// Get All company profiles
        /// </summary>
        /// <returns>Company profile list</returns>
        public static List<ProfileCompany> GetListCompanyAll()
        {
            kuyamEntities _context = DBContext;
            return
                _context.ProfileCompanies.Where(
                    p => p.CompanyStatusID == (int)Kuyam.Database.Types.CompanyStatus.Active).OrderBy(c => c.Name).
                    ToList();
        }

        public static bool UpdateCompanyFeatured(int profileID, int priority)
        {

            bool result = false;
            kuyamEntities _context = DBContext;
            if (!_context.FeaturedCompanies.Any(x => x.ProfileID == profileID))
            {
                FeaturedCompany fc = _context.FeaturedCompanies.Where(f => f.priority == priority).FirstOrDefault();
                fc.ProfileID = profileID;
                fc.StartDT = DateTime.Now;

                _context.SaveChanges();
                result = true;
            }
            return result;
        }

        internal static List<CompanyPackage> GetActiveCompanyPackages(int profileId)
        {
            kuyamEntities _context = DBContext;
            var packages = from p in _context.CompanyPackages
                           where p.ProfileCompanyId == profileId && p.Status == (int)Types.CompanyPackageStatus.Active
                           select p;
            return packages.ToList();
        }

        public static List<Service> GetServiceListByProfileCompanyId(int profileCompanyId)
        {
            kuyamEntities _context = DBContext;
            var packages = from s in _context.Services
                           join p in _context.ServiceCompanies on s.ServiceID equals p.ServiceID
                           where p.ProfileID == profileCompanyId && p.Status == (int)Types.ServiceCompanyStatus.Active
                           select s;
            return packages.ToList();
        }

        public static List<CompanyPackageExt> GetNonUsingPackageByProfileID(int profileId, Types.CompanyPackageStatus status)
        {
            kuyamEntities _context = DBContext;
            //var usingpackages = (from s in _context.UserPackagePurchases
            //                     join c in _context.CompanyPackages on s.CompanyPackageId equals c.PackageId
            //                     where c.ProfileCompanyId == profileId
            //                     select c.PackageId).Distinct().ToList();

            var packages = (from c in _context.CompanyPackages
                            where c.ProfileCompanyId == profileId
                            && c.Status == (int)status
                            select new CompanyPackageExt
                                       {
                                           PackageId = c.PackageId,
                                           PackageName = c.PackageName,
                                           Description = c.Description,
                                           ProfileCompanyId = c.ProfileCompanyId,
                                           PackageType = c.PackageType,
                                           NumberOfBooking = c.NumberOfBooking,
                                           Price = c.Price,
                                           Status = c.Status,
                                           DurationInMonth = c.DurationInMonth,
                                           KalturaImageId = c.KalturaImageId,
                                           StartDate = c.StartDate,
                                           EndDate = c.EndDate,
                                           ModifiedDate = c.ModifiedDate,
                                           CreateDate = c.CreateDate,
                                           UnitPrice = c.UnitPrice
                                       }).ToList();

            foreach (var companyPackageExt in packages)
            {
                CompanyPackageExt ext = companyPackageExt;
                companyPackageExt.Services =
                    _context.CompanyPackageServices.Where(c => c.CompanyPackageId == ext.PackageId).Select
                        (c => c.ServiceCompanyId).ToList();
                companyPackageExt.KalturaImages =
                    _context.CompanyPackageImages.Where(c => c.CompanyProfileId == profileId).OrderBy(c => c.CreatedDate)
                        .Select
                        (c => c.LocationData).ToList();
            }

            return packages.ToList();
        }

        public static List<CompanyService> GetServiceCompanyByProfileID(int profileId, int statusId)
        {
            kuyamEntities _context = DBContext;
            var service = (from s in _context.ServiceCompanies
                           join p in _context.Services on s.ServiceID equals p.ServiceID
                           where s.ProfileID == profileId && s.Status == statusId && p.ParentServiceID.HasValue
                           select new CompanyService
                                      {
                                          ID = s.ServiceCompanyID,
                                          ServiceName = p.ServiceName,
                                          Duration = s.Duration == null ? 0 : s.Duration.Value,
                                          Price = s.Price == null ? 0 : s.Price.Value
                                      }).ToList();
            return service;
        }

        public static CompanyPackageImage GetSoonestPackageImageByProfileId(int profileId)
        {
            kuyamEntities _context = DBContext;
            var service = (from s in _context.CompanyPackageImages
                           where s.CompanyProfileId == profileId
                           select s).OrderBy(s => s.CreatedDate).ToList();
            if (service.Count >= 6)
                return service.FirstOrDefault();
            else
                return null;
        }

        public static int GetCountPurchaseByPackageId(int packageId)
        {
            kuyamEntities _context = DBContext;
            var query = from up in _context.UserPackagePurchases
                        where up.CompanyPackageId == packageId
                        select up;
            return query.Count();
        }
        public static void CreateCompanyPackageImage(int profileID, string imageId)
        {
            kuyamEntities _context = DBContext;
            var service = new CompanyPackageImage
                              {
                                  CompanyProfileId = profileID,
                                  LocationData = imageId,
                                  CreatedDate = DateTime.Now
                              };
            _context.CompanyPackageImages.Add(service);
            _context.SaveChanges();
        }

        public static void UpdateCompanyPackageImage(int companyPackageImageId, string imageId)
        {
            kuyamEntities _context = DBContext;
            var service =
                _context.CompanyPackageImages.FirstOrDefault(s => s.CompanyPackageImageId == companyPackageImageId);
            if (service != null)
                service.LocationData = imageId;
            _context.SaveChanges();
        }

        public static void CreateCompanyAdminPackage(int profileID, string name, decimal packPrice, int packQuantity,
                                                     int packDuration, List<int> packServices, string imageUrl)
        {
            kuyamEntities _context = DBContext;
            Types.CompanyPackageType packageType = Types.CompanyPackageType.ByUnlimited;
            if (packQuantity > 0)
                packageType = Types.CompanyPackageType.ByQuanlity;

            //if (packQuantity == -1 && packDuration > 0)
            //    packageType = Types.CompanyPackageType.ByDuration;
            //else if (packQuantity > -1 && packDuration == -1)
            //    packageType = Types.CompanyPackageType.BySlot;
            var service = new CompanyPackage
                              {
                                  PackageName = name,
                                  Description = string.Format("applies to: {0} service(s)", packServices.Count),
                                  ProfileCompanyId = profileID,
                                  PackageType = (int)packageType,
                                  NumberOfBooking = packQuantity,
                                  Price = packPrice,
                                  UnitPrice = packQuantity > 0 ? (packPrice / packQuantity) : 0,
                                  Status = (int)Types.CompanyPackageStatus.Active,
                                  DurationInMonth = packDuration,
                                  KalturaImageId = imageUrl,
                                  StartDate = DateTime.UtcNow,
                                  EndDate = DateTime.UtcNow.AddMonths(packDuration),
                                  ModifiedDate = DateTime.UtcNow,
                                  CreateDate = DateTime.UtcNow
                              };
            _context.CompanyPackages.Add(service);
            foreach (var id in packServices)
            {
                var companyPackageService = new CompanyPackageService
                                                {
                                                    CompanyPackage = service,
                                                    ServiceCompanyId = id
                                                };
                _context.CompanyPackageServices.Add(companyPackageService);
            }
            _context.SaveChanges();
        }

        public static void UpdateCompanyAdminPackage(int profileID, int packageId, string name, decimal packPrice,
                                                     int packQuantity, int packDuration, List<int> packServices,
                                                     string imageUrl)
        {
            kuyamEntities _context = DBContext;
            Types.CompanyPackageType packageType = Types.CompanyPackageType.ByUnlimited;
            CompanyPackage companyPackage = _context.CompanyPackages.FirstOrDefault(c => c.PackageId == packageId);
            if (companyPackage != null)
            {
                //if (packQuantity == -1 && packDuration > 0)
                //    packageType = Types.CompanyPackageType.ByDuration;
                //else if (packQuantity > -1 && packDuration == -1)
                //    packageType = Types.CompanyPackageType.BySlot;

                if (packQuantity > 0)
                    packageType = Types.CompanyPackageType.ByQuanlity;

                companyPackage.PackageName = name;
                companyPackage.Description = string.Format("applies to: {0} service(s)", packServices.Count);
                companyPackage.ProfileCompanyId = profileID;
                companyPackage.PackageType = (int)packageType;
                companyPackage.NumberOfBooking = packQuantity;
                companyPackage.Price = packPrice;
                companyPackage.Status = (int)Types.CompanyPackageStatus.Active;
                companyPackage.DurationInMonth = packDuration;
                companyPackage.KalturaImageId = imageUrl;

                var lstPackService =
                    _context.CompanyPackageServices.Where(c => c.CompanyPackageId == packageId).ToList();

                for (int i = lstPackService.Count - 1; i >= 0; i--)
                {
                    _context.CompanyPackageServices.Remove(lstPackService[i]);
                }

                foreach (var id in packServices)
                {
                    var companyPackageService = new CompanyPackageService
                                                    {
                                                        CompanyPackageId = packageId,
                                                        ServiceCompanyId = id
                                                    };
                    _context.CompanyPackageServices.Add(companyPackageService);
                }
                _context.SaveChanges();
            }
        }

        public static void DeleteCompanyAdminPackage(int packageId)
        {
            kuyamEntities _context = DBContext;
            CompanyPackage companyPackage = _context.CompanyPackages.FirstOrDefault(c => c.PackageId == packageId);
            if (companyPackage != null)
            {
                var lstPackService =
                    _context.CompanyPackageServices.Where(c => c.CompanyPackageId == packageId).ToList();

                for (int i = lstPackService.Count - 1; i >= 0; i--)
                {
                    _context.CompanyPackageServices.Remove(lstPackService[i]);
                }
                _context.CompanyPackages.Remove(companyPackage);
                _context.SaveChanges();
            }
        }

        public static List<string> GetCompanyImagesbyProfileID(int profileId)
        {
            kuyamEntities _context = DBContext;
            var lstPackImage =
                _context.CompanyPackageImages.Where(c => c.CompanyProfileId == profileId).Select(s => s.LocationData);
            return lstPackImage.ToList();
        }



        public static List<Calendar> GetActiveCalendarsbyCustId(int custId)
        {
            kuyamEntities _context = DBContext;
            var lstCalendar = from c in _context.Calendars
                              join cs in _context.CalendarShares on c.CalendarID equals cs.CalendarID
                              where cs.CustID == custId && c.CalendarDisplayTypeID != (int)Types.CalendarDisplayType.Hidden
                              select c;
            return lstCalendar.ToList();
        }

        public static void AddDefaultCalendar(int custID, string name)
        {
            kuyamEntities _context = DBContext;
            var calendar = new Calendar()
                               {
                                   ProfileID = null,
                                   Name = name,
                                   BackColor = "87CEEB",
                                   ForeColor = "0",
                                   IsDefault = true,
                                   CalendarDisplayTypeID = (int)Types.CalendarType.Default,
                                   Created = DateTime.UtcNow,
                                   Modified = null
                               };
            _context.Calendars.Add(calendar);
            var calendarshare = new CalendarShare()
                                    {
                                        Calendar = calendar,
                                        CustID = custID,
                                        ShareTypeID = (int)Types.ShareType.Reader,
                                        Created = DateTime.UtcNow,
                                        Modified = DateTime.UtcNow
                                    };
            _context.CalendarShares.Add(calendarshare);
            _context.SaveChanges();
        }

        public static void DeleteCalendar(int custID, int calendarId)
        {
            kuyamEntities _context = DBContext;
            var calendar = _context.Calendars.FirstOrDefault(c => c.CalendarID == calendarId);
            if (calendar != null)
            {
                var shareCalendar =
                    _context.CalendarShares.FirstOrDefault(s => s.CalendarID == calendarId && s.CustID == custID);
                if (shareCalendar != null)
                {
                    _context.CalendarShares.Remove(shareCalendar);
                    _context.Calendars.Remove(calendar);
                    _context.SaveChanges();
                }
            }
        }

        public static void UpdateCalendar(int calendarId, string name)
        {
            kuyamEntities _context = DBContext;
            var calendar = _context.Calendars.FirstOrDefault(c => c.CalendarID == calendarId);
            if (calendar != null)
            {
                calendar.Name = name;
                calendar.Modified = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }


        #region Company Invoices
        public static List<PaymentMethod> GetPaymentMethodsByProfileId(int profileId)
        {
            kuyamEntities _context = DBContext;
            return _context.PaymentMethods.Where(pm => pm.ProfileCompany.ProfileID == profileId).ToList();

        }

        public static string GetReceiptNumber(int profileId)
        {
            kuyamEntities _context = DBContext;
            string companyName = _context.ProfileCompanies.Where(prf => prf.ProfileID == profileId).Select(prf => prf.Name).SingleOrDefault();
            if (companyName.Length > 4)
            {
                companyName = companyName.Substring(0, 4);
            }

            return companyName;
        }

        public static List<CompanyInvoices> GetCompanyInvoicesInfo(DateTime? serviceStartDate, int? serviceId, string empName, int? paymentMethod, int profileCompanyId)
        {
            kuyamEntities _context = DBContext;

            var result = (
               from ap in _context.Appointments
               join sc in _context.ServiceCompanies
               on ap.ServiceCompanyID equals sc.ServiceCompanyID
               join s in _context.Services
               on sc.ServiceID equals s.ServiceID
               join es in _context.EmployeeServices
               on sc.ServiceCompanyID equals es.ServiceCompanyID
               join ce in _context.CompanyEmployees
               on es.CompanyEmployeeID equals ce.EmployeeID
               join c in _context.Custs
               on ap.CustID equals c.CustID
               join cprf in _context.ProfileCompanies
               on sc.ProfileID equals cprf.ProfileID
               //join pm in _context.PaymentMethods
               //on cprf.ProfileID equals pm.CompanyID
               // where (ap.Start == serviceStartDate && s.ServiceID == serviceId && ce.EmployeeName == empName && pm.PaymentMethodID == paymentMethod && cprf.ProfileID == profileCompanyId)
               select new
               {
                   ap.Start,
                   sc.Description,
                   ce.EmployeeName,
                   IsRegular = false,
                   ServiceAmmount = ap.Price,
                   ClientFirstName = c.FirstName,
                   ClientLastName = c.LastName,
                   CompanyName = cprf.Name,
                   ap.AppointmentID,
                   ap.AppointmentStatusID//, ap.PaymentTypeID
               }

              ).ToList<dynamic>();

            List<CompanyInvoices> companyInvoicesList = new List<CompanyInvoices>();
            for (int a = 0; a < result.Count; a++)
            {
                CompanyInvoices invoice = new CompanyInvoices()
                {
                    ClientName = result[a].ClientFirstName + " " + result[a].ClientLastName,
                    ServiceStartDate = result[a].Start,
                    ServiceDescription = result[a].Description,
                    EmployeeName = result[a].EmployeeName,
                    IsRegular = result[a].IsRegular,
                    ServiceAmmount = result[a].ServiceAmmount,
                    CompanyName = result[a].CompanyName,
                    AppointmentStatus = Enum.GetName(typeof(Types.AppointmentStatus), result[a].AppointmentStatusID),
                    PaymentMethod = Enum.GetName(typeof(Types.AppointmentStatus), result[a].AppointmentStatusID),
                    ReceiptNumber = GetReceiptNumber(profileCompanyId) + result[a].AppointmentID

                };

                companyInvoicesList.Add(invoice);
            }

            return companyInvoicesList;
        }
        #endregion

        #region Company Discount
        public static void CreateCompanyDiscount(int profileId, string name, string code, bool isAmount, decimal amount, bool isPercent, decimal percent, int quantity, int serviceId, DateTime startDate, DateTime endDate)
        {
            kuyamEntities _context = DBContext;
            Discount discount = new Discount()
                                    {
                                        Name = name,
                                        Code = code,
                                        Amount = isAmount == true ? amount : 0,
                                        Percent = isPercent == true ? percent : 0,
                                        Quantity = quantity,
                                        StartDate = startDate,
                                        EndDate = endDate,
                                        ApplyToAllServices = serviceId <= 0,
                                        ProfileCompanyId = profileId,
                                        CreatedDate = DateTime.UtcNow,
                                        ModifiedDate = DateTime.UtcNow,
                                        Status = (int)Types.DiscountStatus.Active
                                    };
            _context.Discounts.Add(discount);
            if (serviceId > 0)
            {
                DiscountService discountService = new DiscountService()
                                                      {
                                                          Discount = discount,
                                                          ServiceCompanyId = serviceId
                                                      };
                _context.DiscountServices.Add(discountService);

            }
            _context.SaveChanges();
        }

        public static void UpdateCompanyDiscount(int profileId, int discountId, string name, string code, bool isAmount, decimal amount, bool isPercent, decimal percent, int quantity, int serviceId, DateTime startDate, DateTime endDate)
        {
            kuyamEntities _context = DBContext;
            Discount discount = _context.Discounts.FirstOrDefault(d => d.DiscountId == discountId);
            discount.Name = name;
            discount.Code = code;
            discount.Amount = isAmount == true ? amount : 0;
            discount.Percent = isPercent == true ? percent : 0;
            discount.Quantity = quantity;
            discount.StartDate = startDate;
            discount.EndDate = endDate;
            discount.ApplyToAllServices = serviceId <= 0;
            discount.ProfileCompanyId = profileId;
            discount.CreatedDate = DateTime.UtcNow;
            discount.ModifiedDate = DateTime.UtcNow;
            discount.Status = (int)Types.DiscountStatus.Active;

            var delDiscountService = _context.DiscountServices.FirstOrDefault(d => d.DiscountId == discountId);
            if (delDiscountService != null)
            {
                _context.DiscountServices.Remove(delDiscountService);
            }
            if (serviceId > 0)
            {
                DiscountService discountService = new DiscountService()
                {
                    Discount = discount,
                    ServiceCompanyId = serviceId
                };
                _context.DiscountServices.Add(discountService);

            }
            _context.SaveChanges();
        }

        public static List<CompanyDiscountExt> GetDiscountCompanyByProfileID(int profileID, int status)
        {
            kuyamEntities _context = DBContext;
            var lstCompanyDiscount =
                _context.Discounts.Where(d => d.ProfileCompanyId == profileID && d.Status == status).
                    Select(d => new CompanyDiscountExt
                                    {
                                        DiscountId = d.DiscountId,
                                        Name = d.Name,
                                        Code = d.Code,
                                        Amount = d.Amount,
                                        Percent = d.Percent,
                                        Quantity = d.Quantity,
                                        StartDate = d.StartDate,
                                        EndDate = d.EndDate,
                                        ApplyToAllServices = d.ApplyToAllServices,
                                        ProfileCompanyId = d.ProfileCompanyId ?? 0,
                                        CreatedDate = d.CreatedDate,
                                        ModifiedDate = d.ModifiedDate,
                                        Description = string.Empty,
                                        NumberofSent = 0,
                                        Status = status,
                                        NumberOfUsage = 0
                                    })
                    .ToList();
            for (int i = 0; i < lstCompanyDiscount.Count; i++)
            {
                CompanyDiscountExt companyDiscountExt = lstCompanyDiscount[i];
                companyDiscountExt.IsSent = _context.DiscountRegularClients.Any(d => d.DiscountId == companyDiscountExt.DiscountId);
                if (companyDiscountExt.ApplyToAllServices)
                {
                    companyDiscountExt.ServiceId = 0;
                    companyDiscountExt.Description = "all services";
                }
                else
                {
                    var service = (from sc in _context.ServiceCompanies
                                   join ds in _context.DiscountServices on sc.ServiceCompanyID equals ds.ServiceCompanyId
                                   join s in _context.Services on sc.ServiceID equals s.ServiceID
                                   where sc.ProfileID == profileID && ds.DiscountId == companyDiscountExt.DiscountId
                                   select new
                                              {
                                                  ID = sc.ServiceCompanyID,
                                                  Name = s.ServiceName,
                                                  Duration = sc.Duration
                                              }).FirstOrDefault();
                    if (service != null)
                    {
                        companyDiscountExt.Description = string.Format("{0}min {1}",
                                                                       service.Duration != null
                                                                           ? service.Duration.Value
                                                                           : 0, service.Name);
                        companyDiscountExt.NumberOfUsage =
                            _context.UserDiscounts.Count(u => u.DiscountId == companyDiscountExt.DiscountId);
                        companyDiscountExt.NumberofSent =
                            _context.DiscountRegularClients.Count(r => r.DiscountId == companyDiscountExt.DiscountId);
                        companyDiscountExt.ServiceId = service.ID;
                    }
                }
            }

            return lstCompanyDiscount;
        }


        public static bool IsExistDiscountCode(int profileId, string code)
        {
            kuyamEntities _context = DBContext;
            return
                _context.Discounts.Any(
                    d =>
                    d.ProfileCompanyId == profileId && d.Code == code && d.Status == (int)Types.DiscountStatus.Active);
        }

        public static bool IsExistDiscountCode(int profileId, int discountId, string code)
        {
            kuyamEntities _context = DBContext;
            return
                _context.Discounts.Any(
                    d =>
                    d.ProfileCompanyId == profileId && d.Code == code && d.Status == (int)Types.DiscountStatus.Active &&
                    d.DiscountId != discountId);
        }


        public static bool IsUsedDiscount(int discountId)
        {
            kuyamEntities _context = DBContext;
            return _context.UserDiscounts.Any(d => d.DiscountId == discountId) ||
                   _context.DiscountRegularClients.Any(d => d.DiscountId == discountId);
        }

        public static void DeleteDiscountCompany(int discountId)
        {
            kuyamEntities _context = DBContext;
            var discount = _context.Discounts.FirstOrDefault(d => d.DiscountId == discountId);
            if (discount != null)
            {
                discount.Status = (int)Types.DiscountStatus.Inactive;
                _context.SaveChanges();
            }
        }

        public static List<RegularClient> GetRegularClientsbyProfileId(int profileID)
        {
            kuyamEntities _context = DBContext;

            //var query = from c in _context.Custs
            //            join u in _context.aspnet_Users on c.AspUserID equals u.UserId
            //            where u.UserName == userName
            //            select c; 

            var regulars = from r in _context.RegularClients
                           join u in _context.aspnet_Users on r.Email equals u.UserName
                           join c in _context.Custs on u.UserId equals c.AspUserID
                           where r.CompanyProfileId == profileID && r.Status
                           select r;
            return regulars.ToList();
        }

        public static List<RegularClient> GetRegularClient(int discountId)
        {
            kuyamEntities _context = DBContext;
            var regulars = from r in _context.RegularClients
                           join dr in _context.DiscountRegularClients on r.RegularClientId equals dr.RegularClientId
                           where dr.DiscountId == discountId && r.Status
                           select r;
            return regulars.Distinct().ToList();
        }
        #endregion Company Discount

        public static void DeleteCompany(int profileId, string reason)
        {
            kuyamEntities _context = DBContext;
            ProfileCompany profileCompany = _context.ProfileCompanies.FirstOrDefault(m => m.ProfileID == profileId);
            if (profileCompany != null)
            {
                profileCompany.Desc = reason;
                profileCompany.CompanyStatusID = (int)Types.CompanyStatus.Deleted;
                _context.SaveChanges();
            }
        }

        public static List<Service> GetServicesByCustomerId(int custId)
        {
            kuyamEntities _context = DBContext;
            var lstService = (from s in _context.Appointments
                              where s.CustID == custId
                              select s.ServiceCompanyID).Distinct().ToList();
            var services = from sc in _context.ServiceCompanies
                           join s in _context.Services on sc.ServiceID equals s.ServiceID
                           where lstService.Contains(sc.ServiceCompanyID)
                           select s;
            return services.Distinct().ToList();
        }
        public static CustIPhone GetCustIPhoneByCustID(int custID)
        {
            kuyamEntities ctx = DBContext;
            CustIPhone custIP = new CustIPhone();

            Cust cust = ctx.Custs.Where(x => x.CustID == custID && x.Status == 1).FirstOrDefault();
            if (cust == null)
                return custIP;

            return new CustIPhone(cust);
            /*
           aspnet_Membership user = ctx.aspnet_Membership.Where(x => x.aspnet_Users.UserName == cust.Username).FirstOrDefault();
           
           string email = user.Email;

           custIP.CustID = custID;
           custIP.AccountID = cust.AccountID;
           custIP.AspUserID = cust.AspUserID;
           custIP.Email = email;
           custIP.CustTypeID = cust.CustTypeID;
           custIP.FirstName = cust.FirstName;
           custIP.LastName = cust.LastName;
           custIP.Street1 = cust.Street1;
           custIP.Street2 = cust.Street2;
           custIP.City = cust.City;
           custIP.State = cust.State;
           custIP.Zip = cust.Zip;
           custIP.HomePhone = cust.HomePhone;
           custIP.MobilePhone = cust.MobilePhone;
           custIP.MobileCarrier = cust.MobileCarrier;
           custIP.WorkPhone = cust.WorkPhone;
           custIP.PreferredPhoneTypeID = cust.PreferredPhoneTypeID;
           custIP.FirstAlert = cust.FirstAlert;
           custIP.SecondAlert = cust.SecondAlert;
           custIP.IcalUrl = cust.IcalUrl;
           custIP.EmailHtml = cust.EmailHtml;
           custIP.GenderTypeID = cust.GenderTypeID;
           custIP.CustStatusTypeID = cust.CustStatusTypeID;
           custIP.CustStatusReasonTypeID = cust.CustStatusReasonTypeID;
           custIP.CustStatusNote = cust.CustStatusNote;
           custIP.Notes = cust.Notes;
           custIP.Birthday = cust.Birthday;
           custIP.LastLogin = cust.LastLogin;
           custIP.Created = cust.Created.ToString("yyyy-MM-dd HH:mm:ss");
           custIP.Modified = cust.Modified;
           custIP.LastVisit = cust.LastVisit;
           custIP.Latitude = cust.Latitude;
           custIP.Longitude = cust.Longitude;
           custIP.OtherCalendar = cust.OtherCalendar;
           custIP.OutlookCalendar = cust.OutlookCalendar;
           custIP.YahooCalendar = cust.YahooCalendar;
           custIP.TimeZoneId = cust.TimeZoneId;
           custIP.Status = cust.Status;
           custIP.FacebookToken = cust.FacebookToken;
           custIP.FacebookUserID = cust.FacebookUserID;
           custIP.LocationReminder = cust.LocationReminder;
           return custIP;
            */
        }

        public static List<CompanyProfileIPhone> GetFeaturedCompaniesOnIphoneAtHomePage()
        {
            kuyamEntities ctx = DBContext;
            var query = (from cpf in ctx.ProfileCompanies
                         join fcp in ctx.FeaturedCompanies on cpf.ProfileID equals fcp.ProfileID
                         where fcp.priority > 0 && cpf.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                         orderby fcp.priority
                         select new CompanyProfileIPhone
                         {
                             ProfileID = cpf.ProfileID,
                             CompanyTypeID = cpf.CompanyTypeID,
                             CompanyStatusID = cpf.CompanyStatusID,
                             Name = cpf.Name,
                             Desc = cpf.Desc,
                             ContactName = cpf.ContactName,
                             ContactTitle = cpf.ContactTitle,
                             Street1 = cpf.Street1,
                             Street2 = cpf.Street2,
                             City = cpf.City,
                             State = cpf.State,
                             Zip = cpf.Zip,
                             Phone = cpf.Phone,
                             Fax = cpf.Fax,
                             Email = cpf.Email,
                             Url = cpf.Url,
                             YoutubeLink = cpf.YoutubeLink,
                             PreferredContact = cpf.PreferredContact,
                             PaymentOptions = cpf.PaymentOptions,
                             PaymentMethod = cpf.PaymentMethod,
                             PayAfter = cpf.PayAfter,
                             MapUrl = cpf.MapUrl,
                             PublicTransportation = cpf.PublicTransportation,
                             Notes = cpf.Notes,
                             ApptAutoConfirm = cpf.ApptAutoConfirm,
                             ApptDefaultSlotDuration = cpf.ApptDefaultSlotDuration,
                             ApptDefaultPeoplePerSlot = cpf.ApptDefaultPeoplePerSlot,
                             Created = cpf.Created,
                             Modified = cpf.Modified,
                             Latitude = cpf.Latitude,
                             Longitude = cpf.Longitude,
                             ExpiredDate = cpf.ExpiredDate,
                             ContactFirstName = cpf.ContactFirstName,
                             ContactLastName = cpf.ContactLastName,
                             CancelPolicy = cpf.CancelPolicy,
                             CancelHour = cpf.CancelHour,
                             CancelRefundPercent = cpf.CancelRefundPercent,
                             MobileCarrier = cpf.MobileCarrier,

                         }).Take(3).ToList();

            return query;
        }

        public static IQueryable<CompanyProfileIPhone> GetFeaturedCompaniesForBlogService(Guid categoryId)
        {
            kuyamEntities ctx = DBContext;

            //var featuredCompanies = ctx.FeaturedCompanies.AsQueryable();
            var category = ctx.be_Categories.Where(t => t.CategoryID == categoryId).FirstOrDefault();
            if (category == null) return null;
            var categoryFeatured = ctx.CategoryFeatureds.Where(t => t.BeCategoryId == category.CategoryRowID).Select(t => t.ProfileId).ToList();
            //featuredCompanies = featuredCompanies.Where(t => categoryFeatured.Contains(t.CompanyFeaturedID));
            var query = (from cpf in ctx.ProfileCompanies
                         where categoryFeatured.Contains(cpf.ProfileID) && cpf.CompanyStatusID == (int)Types.CompanyStatus.Active
                         select new CompanyProfileIPhone
                         {
                             ProfileID = cpf.ProfileID,
                             CompanyTypeID = cpf.CompanyTypeID,
                             CompanyStatusID = cpf.CompanyStatusID,
                             Name = cpf.Name,
                             Desc = cpf.Desc,
                             ContactName = cpf.ContactName,
                             ContactTitle = cpf.ContactTitle,
                             Street1 = cpf.Street1,
                             Street2 = cpf.Street2,
                             City = cpf.City,
                             State = cpf.State,
                             Zip = cpf.Zip,
                             Phone = cpf.Phone,
                             Fax = cpf.Fax,
                             Email = cpf.Email,
                             Url = cpf.Url,
                             YoutubeLink = cpf.YoutubeLink,
                             PreferredContact = cpf.PreferredContact,
                             PaymentOptions = cpf.PaymentOptions,
                             PaymentMethod = cpf.PaymentMethod,
                             PayAfter = cpf.PayAfter,
                             MapUrl = cpf.MapUrl,
                             PublicTransportation = cpf.PublicTransportation,
                             Notes = cpf.Notes,
                             ApptAutoConfirm = cpf.ApptAutoConfirm,
                             ApptDefaultSlotDuration = cpf.ApptDefaultSlotDuration,
                             ApptDefaultPeoplePerSlot = cpf.ApptDefaultPeoplePerSlot,
                             Created = cpf.Created,
                             Modified = cpf.Modified,
                             Latitude = cpf.Latitude,
                             Longitude = cpf.Longitude,
                             ExpiredDate = cpf.ExpiredDate,
                             ContactFirstName = cpf.ContactFirstName,
                             ContactLastName = cpf.ContactLastName,
                             CancelPolicy = cpf.CancelPolicy,
                             CancelHour = cpf.CancelHour,
                             CancelRefundPercent = cpf.CancelRefundPercent,
                             MobileCarrier = cpf.MobileCarrier,

                         });

            return query;
        }


        public static List<CompanyProfileIPhone> GetFavouriteCompaniesOnIphoneAtHomePage(int custID)
        {
            kuyamEntities _context = DBContext;

            var query = (from f in _context.Favorites
                         join cpf in _context.ProfileCompanies on f.ProfileID equals cpf.ProfileID
                         where f.CustID == custID && cpf.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                         select new CompanyProfileIPhone
                         {
                             ProfileID = cpf.ProfileID,
                             CompanyTypeID = cpf.CompanyTypeID,
                             CompanyStatusID = cpf.CompanyStatusID,
                             Name = cpf.Name,
                             Desc = cpf.Desc,
                             ContactName = cpf.ContactName,
                             ContactTitle = cpf.ContactTitle,
                             Street1 = cpf.Street1,
                             Street2 = cpf.Street2,
                             City = cpf.City,
                             State = cpf.State,
                             Zip = cpf.Zip,
                             Phone = cpf.Phone,
                             Fax = cpf.Fax,
                             Email = cpf.Email,
                             Url = cpf.Url,
                             YoutubeLink = cpf.YoutubeLink,
                             PreferredContact = cpf.PreferredContact,
                             PaymentOptions = cpf.PaymentOptions,
                             PaymentMethod = cpf.PaymentMethod,
                             PayAfter = cpf.PayAfter,
                             MapUrl = cpf.MapUrl,
                             PublicTransportation = cpf.PublicTransportation,
                             Notes = cpf.Notes,
                             ApptAutoConfirm = cpf.ApptAutoConfirm,
                             ApptDefaultSlotDuration = cpf.ApptDefaultSlotDuration,
                             ApptDefaultPeoplePerSlot = cpf.ApptDefaultPeoplePerSlot,
                             Created = cpf.Created,
                             Modified = cpf.Modified,
                             Latitude = cpf.Latitude,
                             Longitude = cpf.Longitude,
                             ExpiredDate = cpf.ExpiredDate,
                             ContactFirstName = cpf.ContactFirstName,
                             ContactLastName = cpf.ContactLastName,
                             CancelPolicy = cpf.CancelPolicy,
                             CancelHour = cpf.CancelHour,
                             CancelRefundPercent = cpf.CancelRefundPercent,
                             MobileCarrier = cpf.MobileCarrier,

                         }).ToList();

            return query;
        }




        public static decimal GetFavouriteStarForCompanyProfile(int profileID)
        {
            kuyamEntities ctx = DBContext;
            decimal ratingValue = 0;

            var query = (from r in ctx.Ratings
                         join p in ctx.ServiceCompanies on r.ServiceCompanyID equals p.ServiceCompanyID
                         where p.ProfileID == profileID
                         select r).ToList();
            if (query != null && query.Count > 0)
            {
                foreach (Rating rItem in query)
                {
                    ratingValue = ratingValue + rItem.RatingValue.Value;
                }
                decimal agvValue = Math.Round(ratingValue / (decimal)query.Count);
                return agvValue;
            }
            return 0;
        }

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithKeyword(string key)
        {
            try
            {
                kuyamEntities ctx = DBContext;


                var companies = (from pc in ctx.ProfileCompanies
                                 join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                                 join s in ctx.Services on scp.ServiceID equals s.ServiceID
                                 where
                                     pc.CompanyStatusID == (int)Types.CompanyStatus.Active &&
                                     (
                                         string.IsNullOrEmpty(key) ||
                                         s.ServiceName.ToLower().Contains(key.ToLower()) ||
                                         pc.Name.ToLower().Contains(key.ToLower()))
                                 select pc).Distinct()
                    .ToList();
                return companies.Select(c => new CompanyProfileIPhone(c)).ToList();
                // Troll by Mr.Thong ^#$&#&#$& 
                //var featureProfileComapnyIDs = (from pc in ctx.ProfileCompanies
                //              join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID 
                //                                into joinProfileCompanies
                //              from j in joinProfileCompanies.DefaultIfEmpty()

                //              join fc in ctx.FeaturedCompanies on pc.ProfileID equals fc.ProfileID
                //              where pc.CompanyStatusID == (int) Types.CompanyStatus.Active
                //                    && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                //              select pc.ProfileID).Distinct().ToList();

                ////------------------------------------------------------------------------
                //List<int> tempFeatureListWithIsToday = new List<int>();
                //List<int> tempFeatureListWithViewAvailable = new List<int>();
                //List<int> tempFeatureList = new List<int>();
                //foreach (int pc in featureProfileComapnyIDs)
                //{
                //    if (isAvailableToday(pc))
                //    {
                //        tempFeatureListWithIsToday.Add(pc);
                //    }

                //    if (isViewAvailability(pc))
                //    {
                //        tempFeatureListWithViewAvailable.Add(pc);
                //    }
                //    else
                //    {
                //        tempFeatureList.Add(pc);
                //    }
                //}
                //featureProfileComapnyIDs = tempFeatureListWithIsToday.Union(tempFeatureListWithViewAvailable).ToList();
                //featureProfileComapnyIDs = featureProfileComapnyIDs.Union(tempFeatureList).Distinct().ToList();


                //var aPCIDs = (from pc in ctx.ProfileCompanies
                //              //join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                //              where pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                //                  && featureProfileComapnyIDs.Contains(pc.ProfileID) == false
                //                  && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                //              select pc.ProfileID).Distinct().ToList();



                //var avPCIDs = (from pc in ctx.ProfileCompanies
                //               //join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                //               //join ce in ctx.CompanyEmployees on pc.ProfileID equals ce.ProfileCompanyID
                //               //join eh in ctx.EmployeeHours on ce.EmployeeID equals eh.CompanyEmployeeID
                //               where
                //                   pc.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                //                   && featureProfileComapnyIDs.Contains(pc.ProfileID) == false
                //                   && aPCIDs.Contains(pc.ProfileID) == false
                //                   && (string.IsNullOrEmpty(key) || profileIDName.Contains(pc.ProfileID) == true)
                //               select pc.ProfileID).Distinct().ToList();


                //var pCIDs = (from pc in ctx.ProfileCompanies
                //             //join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID
                //             join pcs in ctx.ServiceCompanies on pc.ProfileID equals pcs.ProfileID 
                //             into joinProfileCompanies
                //             from j in joinProfileCompanies.DefaultIfEmpty()

                //             where
                //                 (pc.CompanyStatusID == (int)Types.CompanyStatus.Active)
                //                 && (string.IsNullOrEmpty(key) || profileIDName.Contains(j.ProfileID) == true)
                //                 && featureProfileComapnyIDs.Contains(j.ProfileID) == false
                //                 && aPCIDs.Contains(j.ProfileID) == false
                //                 && avPCIDs.Contains(j.ProfileID) == false

                //             select pc.ProfileID).Distinct().ToList();


                //var fpcs = GetCompanyProfileIphonesWithListProfileIDs(featureProfileComapnyIDs);
                //var aPCs = GetCompanyProfileIphonesWithListProfileIDs(aPCIDs);
                //var avPCs = GetCompanyProfileIphonesWithListProfileIDs(avPCIDs);
                //var pCs = GetCompanyProfileIphonesWithListProfileIDs(pCIDs);


                //var ret = fpcs.Union(aPCs).Distinct().ToList();
                //ret = ret.Union(avPCs).Distinct().ToList();
                //ret = ret.Union(pCs).Distinct().ToList();

                //return ret;

            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithKeyword(string key, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                kuyamEntities ctx = DBContext;

                var companies = (from pc in ctx.ProfileCompanies
                                 join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                                 join s in ctx.Services on scp.ServiceID equals s.ServiceID
                                 where
                                     pc.CompanyStatusID == (int)Types.CompanyStatus.Active &&
                                     (
                                         string.IsNullOrEmpty(key) ||
                                         s.ServiceName.ToLower().Contains(key.ToLower()) ||
                                         pc.Name.ToLower().Contains(key.ToLower()))
                                 select pc).Distinct();
                totalItems = companies.Count();

                return companies.OrderBy(c => c.Name).Skip(skip).Take(take).ToList().Select(c => new CompanyProfileIPhone(c)).ToList();

            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithKeyword(string key, double minLat, double minLong, double maxLat, double maxLong, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                kuyamEntities ctx = DBContext;


                var companies = (from pc in ctx.ProfileCompanies
                                 join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                                 join s in ctx.Services on scp.ServiceID equals s.ServiceID
                                 where
                                     pc.CompanyStatusID == (int)Types.CompanyStatus.Active &&
                                     pc.Latitude >= minLat && pc.Latitude <= maxLat &&
                                     pc.Longitude >= minLong && pc.Longitude <= maxLong &&
                                     (
                                         string.IsNullOrEmpty(key) ||
                                         s.ServiceName.ToLower().Contains(key.ToLower()) ||
                                         pc.Name.ToLower().Contains(key.ToLower()))
                                 select pc).Distinct();
                totalItems = companies.Count();

                return companies.OrderBy(c => c.Name).Skip(skip).Take(take).ToList().Select(c => new CompanyProfileIPhone(c)).ToList();

            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithKeyword(string key, List<string> zipcodeList, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                kuyamEntities ctx = DBContext;


                var companies = (from pc in ctx.ProfileCompanies
                                 join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                                 join s in ctx.Services on scp.ServiceID equals s.ServiceID
                                 where
                                     pc.CompanyStatusID == (int)Types.CompanyStatus.Active &&
                                     zipcodeList.Contains(pc.Zip) &&
                                     (
                                         string.IsNullOrEmpty(key) ||
                                         s.ServiceName.ToLower().Contains(key.ToLower()) ||
                                         pc.Name.ToLower().Contains(key.ToLower()))
                                 select pc).Distinct();
                totalItems = companies.Count();

                return companies.OrderBy(c => c.Name).Skip(skip).Take(take).ToList().Select(c => new CompanyProfileIPhone(c)).ToList();

            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        #region SearchOptionalParams

        //public static IEnumerable<be_PostComment> GetNestedComments(Guid commentId)
        //{
        //    var db = new kuyamEntities(DAL.GetConnectionString());
        //    var param = new SqlParameter();
        //    param.ParameterName = "commentId";
        //    param.Value = commentId.ToString();
        //    param.DbType = DbType.String;
        //    return db.SqlQuery<be_PostComment>("exec [GetBlogComments] @commentId", param);
        //}

        //public static List<CompanyProfileIPhone> GetCompanyProfileSearchOptionalParams(int custId, string key, int categoryId, int skip, int take, out int totalItems, double currentLat, double currentLon, double minLat, double minLong, double maxLat, double maxLong, bool searchArea = false)
        //{
        //    totalItems = 0;
        //    try
        //    {
        //        kuyamEntities ctx = DBContext;
        //        var companies = (from pc in ctx.ProfileCompanies
        //                         join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
        //                         where
        //                             pc.CompanyStatusID == (int)Types.CompanyStatus.Active &&
        //                         (pc.Latitude >= minLat && pc.Latitude <= maxLat &&
        //                             pc.Longitude >= minLong && pc.Longitude <= maxLong) &&
        //                             (string.IsNullOrEmpty(key)
        //                             || (scp.Service.ServiceName.ToLower().Contains(key.ToLower()))
        //                             || pc.Name.ToLower().Contains(key.ToLower()))
        //                             && (categoryId == 0 || scp.ServiceID == categoryId)
        //                         select pc).Distinct();
        //        //var listcompany = companies.ToList().Where(m => (!searchArea || CheckWithinRectangle(minLat, minLong, maxLat, maxLong, m.Longitude, m.Latitude)));
        //        //totalItems = listcompany.Count();
        //        var listcompany = companies.ToList();
        //        totalItems = listcompany.Count();
        //        if (totalItems > 100)
        //        {
        //            totalItems = 100;
        //        }

        //        var listcompanyIds = listcompany.Select(m => m.ProfileID).ToList();

        //        var listServices = ctx.ServiceCompanies.Where(m => listcompanyIds.Contains(m.ProfileID)).ToList();

        //        var listFeatureCompany = ctx.FeaturedCompanies.Select(m => m.ProfileID).ToList();
        //        var listFavorites = ctx.Favorites.Select(m => new { CustId = m.CustID, ProfileId = m.ProfileID }).ToList();

        //        var services = (from s in ctx.Services
        //                        join cs in ctx.ServiceCompanies on s.ServiceID equals cs.ServiceID
        //                        where listcompanyIds.Contains(cs.ProfileID) && s.ParentServiceID.HasValue
        //                        select new { ProfileId = cs.ProfileID, ServiceName = s.ServiceName }).Distinct().ToList();



        //        var newlist = new List<CompanyProfileIPhone>();
        //        foreach (var item in listcompany)
        //        {
        //            newlist.Add(new CompanyProfileIPhone(item, currentLat, currentLon, custId)
        //            {
        //                isFeature = listFeatureCompany.Any(m => m == item.ProfileID),
        //                IsUSerFavourite = listFavorites.Any(m => m.ProfileId == item.ProfileID && m.CustId == custId),
        //                ListServices = string.Join(",", services.Where(m => m.ProfileId == item.ProfileID).Select(m => m.ServiceName).ToList()),
        //                Rate = CalculatorRanting(listServices.Where(m => m.ProfileID == item.ProfileID).ToList()),
        //                ImageUrl = item.CompanyMedias.Select(m => m.Medium.LocationData).ToList(),
        //                IsShowCatagory = item.IsShowCatagory
        //            });
        //        }
        //        var takelist = newlist.OrderByDescending(m => m.isFeature).ThenBy(m => m.Distance).Take(100).ToList();
        //        return takelist.OrderByDescending(m => m.isFeature).ThenBy(m => m.Distance).Skip(skip).Take(take).ToList();

        //        // return listcompany.Select(c => new CompanyProfileIPhone(c, currentLat, currentLon, custId)).OrderByDescending(m => m.isFeature).ThenBy(m => m.Distance).Skip(skip).Take(take).ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<CompanyProfileIPhone>();
        //    }
        //}

        public static List<CompanyProfileIPhone> GetCompanyProfileSearchOptionalParams(int custId, string key, int categoryId
            , int skip, int take, out int totalItems, double currentLat, double currentLon
            , double minLat, double minLong, double maxLat, double maxLong, bool searchArea = false)
        {
            totalItems = 0;
            try
            {
                if (key == null) key = string.Empty;

                kuyamEntities ctx = DBContext;
                var totalItemParam = new SqlParameter("TotalItems", 0);
                totalItemParam.Direction = ParameterDirection.Output;
                var data = ctx.Database.SqlQuery<CompanyProfileIPhone>(
                    "GetProfileCompanies @MinLat, @MaxLat, @MinLong, @MaxLong, @KeySearch, @CategoryID, @CustID, @CurrentLat, @CurrentLong, @Skip, @Take, @TotalItems out",
                    new SqlParameter("MinLat", Convert.ToSingle(minLat)),
                    new SqlParameter("MaxLat", Convert.ToSingle(maxLat)),
                    new SqlParameter("MinLong", Convert.ToSingle(minLong)),
                    new SqlParameter("MaxLong", Convert.ToSingle(maxLong)),
                    new SqlParameter("KeySearch", key),
                    new SqlParameter("CategoryID", categoryId),
                    new SqlParameter("CustID", custId),
                    new SqlParameter("CurrentLat", Convert.ToSingle(currentLat)),
                    new SqlParameter("CurrentLong", Convert.ToSingle(currentLon)),
                    new SqlParameter("Skip", skip),
                    new SqlParameter("Take", take),
                    //new SqlParameter("searchArea", searchArea),
                    totalItemParam
                    );

                var result = data.ToList();
                totalItems = Convert.ToInt32(totalItemParam.Value);

                return result;
            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        private static decimal CalculatorRanting(List<ServiceCompany> serviceCompanys)
        {
            if (serviceCompanys == null)
                return 0;
            decimal totalReview = 0;
            decimal valueRanting = 0;
            decimal agvValue = 0;
            foreach (ServiceCompany item in serviceCompanys)
            {
                totalReview += item.Ratings.Count;
                valueRanting += item.Ratings.Sum(m => m.RatingValue).Value;
            }
            if (totalReview > 0)
            {
                agvValue = Math.Round(valueRanting / totalReview);
            }

            return agvValue;
        }


        private static bool CheckWithinRectangle(double minLat, double minLon, double maxLat, double maxLon, double pointX, double pointY)
        {
            try
            {

                kuyamEntities ctx = DBContext;

                var param1 = new SqlParameter();
                param1.ParameterName = "minX";
                param1.Value = minLat;
                param1.DbType = DbType.String;

                var param2 = new SqlParameter();
                param2.ParameterName = "minY";
                param2.Value = minLon;
                param2.DbType = DbType.String;

                var param3 = new SqlParameter();
                param3.ParameterName = "maxX";
                param3.Value = maxLat;
                param3.DbType = DbType.String;

                var param4 = new SqlParameter();
                param4.ParameterName = "maxY";
                param4.Value = maxLon;
                param4.DbType = DbType.String;

                var param5 = new SqlParameter();
                param5.ParameterName = "pointX";
                param5.Value = pointX;
                param5.DbType = DbType.String;

                var param6 = new SqlParameter();
                param6.ParameterName = "pointY";
                param6.Value = pointY;
                param6.DbType = DbType.String;

                var result = ctx.Database.SqlQuery<bool>("EXEC [sp_CheckWithinRectangle] @minX, @minY, @maxX, @maxY, @pointX, @pointY", param1, param2, param3, param4, param5, param6);
                return result.FirstOrDefault();

            }
            catch (Exception)
            {

                return false;
            }
        }

        public static MapPoint GetMapPointMinMax(string polygon)
        {
            kuyamEntities ctx = DBContext;

            var param = new SqlParameter();
            param.ParameterName = "polygon";
            param.Value = polygon;
            param.DbType = DbType.String;
            var result = ctx.Database.SqlQuery<MapPoint>("EXEC [sp_GetMinMax] @polygon", param);
            return result.FirstOrDefault();
        }

        #endregion

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithCategoryID(int categoryID, int skip, int take, out int totalItems)
        {
            totalItems = 0;
            try
            {
                kuyamEntities ctx = DBContext;

                var companies = (from pc in ctx.ProfileCompanies
                                 join scp in ctx.ServiceCompanies on pc.ProfileID equals scp.ProfileID
                                 where
                                     scp.ServiceID == categoryID &&
                                     pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                                 select pc).Distinct();
                totalItems = companies.Count();
                return companies.OrderBy(c => c.Name).Skip(skip).Take(take).ToList().Select(c => new CompanyProfileIPhone(c)).ToList();

            }
            catch (Exception ex)
            {
                return new List<CompanyProfileIPhone>();
            }
        }

        public static List<CompanyProfileIPhone> GetCompanyProfileIphonesWithListProfileIDs(List<int> pIDs)
        {
            kuyamEntities ctx = DBContext;
            var query = (
                            from cpf in ctx.ProfileCompanies
                            where cpf.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                            && pIDs.Contains(cpf.ProfileID)
                            select new CompanyProfileIPhone
                            {
                                ProfileID = cpf.ProfileID,
                                CompanyTypeID = cpf.CompanyTypeID,
                                CompanyStatusID = cpf.CompanyStatusID,
                                Name = cpf.Name,
                                Desc = cpf.Desc,
                                ContactName = cpf.ContactName,
                                ContactTitle = cpf.ContactTitle,
                                Street1 = cpf.Street1,
                                Street2 = cpf.Street2,
                                City = cpf.City,
                                State = cpf.State,
                                Zip = cpf.Zip,
                                Phone = cpf.Phone,
                                Fax = cpf.Fax,
                                Email = cpf.Email,
                                Url = cpf.Url,
                                YoutubeLink = cpf.YoutubeLink,
                                PreferredContact = cpf.PreferredContact,
                                PaymentOptions = cpf.PaymentOptions,
                                PaymentMethod = cpf.PaymentMethod,
                                PayAfter = cpf.PayAfter,
                                MapUrl = cpf.MapUrl,
                                PublicTransportation = cpf.PublicTransportation,
                                Notes = cpf.Notes,
                                ApptAutoConfirm = cpf.ApptAutoConfirm,
                                ApptDefaultSlotDuration = cpf.ApptDefaultSlotDuration,
                                ApptDefaultPeoplePerSlot = cpf.ApptDefaultPeoplePerSlot,
                                Created = cpf.Created,
                                Modified = cpf.Modified,
                                Latitude = cpf.Latitude,
                                Longitude = cpf.Longitude,
                                ExpiredDate = cpf.ExpiredDate,
                                ContactFirstName = cpf.ContactFirstName,
                                ContactLastName = cpf.ContactLastName,
                                CancelPolicy = cpf.CancelPolicy,
                                CancelHour = cpf.CancelHour,
                                CancelRefundPercent = cpf.CancelRefundPercent,
                                MobileCarrier = cpf.MobileCarrier,
                                IsShowCatagory = cpf.IsShowCatagory

                            }).ToList();

            return query;

        }


        public static List<string> GetCompanyImagesPathbyProfileID(int profileId)
        {
            kuyamEntities ctx = DBContext;
            var ret = (from pc in ctx.ProfileCompanies
                       join cm in ctx.CompanyMedias on pc.ProfileID equals cm.ProfileID
                       join m in ctx.Media on cm.MediaID equals m.MediaID
                       where
                           (cm.IsBanner) && m.LocationPath != string.Empty && m.LocationPath != null &&
                           pc.ProfileID == profileId
                       orderby cm.IsDefault descending
                       select m.LocationData).ToList();
            return ret.ToList();
        }

        public static CompanyProfileIPhone GetProfileCompanyWithHourAndPackage(int ProfileID, int custID)
        {
            kuyamEntities ctx = DBContext;
            //ctx.Configuration.ProxyCreationEnabled = false;
            var cpf = ctx.ProfileCompanies.Where(m => m.ProfileID == ProfileID).FirstOrDefault();
            if (cpf != null)
            {
                CompanyProfileIPhone cpi = new CompanyProfileIPhone()
                {
                    ProfileID = cpf.ProfileID,
                    CompanyTypeID = cpf.CompanyTypeID,
                    CompanyStatusID = cpf.CompanyStatusID,
                    Name = cpf.Name,
                    Desc = cpf.Desc,
                    ContactName = cpf.ContactName,
                    ContactTitle = cpf.ContactTitle,
                    Street1 = cpf.Street1,
                    Street2 = cpf.Street2,
                    City = cpf.City,
                    State = cpf.State,
                    Zip = cpf.Zip,
                    Phone = cpf.Phone,
                    Fax = cpf.Fax,
                    Email = cpf.Email,
                    Url = cpf.Url,
                    YoutubeLink = cpf.YoutubeLink,
                    PreferredContact = cpf.PreferredContact,
                    PaymentOptions = cpf.PaymentOptions,
                    PaymentMethod = cpf.PaymentMethod,
                    PayAfter = cpf.PayAfter,
                    MapUrl = cpf.MapUrl,
                    PublicTransportation = cpf.PublicTransportation,
                    Notes = cpf.Notes,
                    ApptAutoConfirm = cpf.ApptAutoConfirm,
                    ApptDefaultSlotDuration = cpf.ApptDefaultSlotDuration,
                    ApptDefaultPeoplePerSlot = cpf.ApptDefaultPeoplePerSlot,
                    Created = cpf.Created,
                    Modified = cpf.Modified,
                    Latitude = cpf.Latitude,
                    Longitude = cpf.Longitude,
                    ExpiredDate = cpf.ExpiredDate,
                    ContactFirstName = cpf.ContactFirstName,
                    ContactLastName = cpf.ContactLastName,
                    CancelPolicy = cpf.CancelPolicy,
                    CancelHour = cpf.CancelHour,
                    CancelRefundPercent = cpf.CancelRefundPercent,
                    MobileCarrier = cpf.MobileCarrier,
                    BusinessHours = GetWorkingHour(cpf.CompanyHours.ToArray()),
                    IsUSerFavourite = isFavorite(custID, cpf.ProfileID),
                    Rate = GetFavouriteStarForCompanyProfile(cpf.ProfileID),
                    ImageUrl = GetCompanyImagesPathbyProfileID(cpf.ProfileID),
                    IsShowCatagory = cpf.IsShowCatagory,
                    Packages = cpf.CompanyPackages.Where(x => x.Status == (int)Types.CompanyPackageStatus.Active).Select(n => new PackageIP()
                    {
                        CreateDate = n.CreateDate,
                        Description = n.Description,
                        DurationInMonth = n.DurationInMonth,
                        EndDate = n.EndDate,
                        KalturaImageId = n.KalturaImageId,
                        ModifiedDate = n.ModifiedDate,
                        NumberOfBooking = n.NumberOfBooking,
                        PackageId = n.PackageId,
                        PackageName = n.PackageName,
                        PackageType = n.PackageType,
                        Price = n.Price,
                        ProfileCompanyId = n.ProfileCompanyId,
                        StartDate = n.StartDate,
                        Status = n.Status,
                        UnitPrice = n.UnitPrice,
                        Services = GetCompanyServicesFromPackageID(n.PackageId)
                    }).ToList()
                };

                var query = (from sc in ctx.ServiceCompanies
                             join r in ctx.Ratings on sc.ServiceCompanyID equals r.ServiceCompanyID
                             join c in ctx.Custs on r.CustID equals c.CustID
                             where sc.ProfileID == ProfileID && r.CreateDate != null
                             select new { Cust = c, Rating = r }).ToArray().OrderByDescending(r => r.Rating.CreateDate);
                var query1 = query.Select(n => new CompanyReview()
                             {
                                 FirstName = n.Cust.FirstName,
                                 LastName = n.Cust.LastName,
                                 Review = n.Rating.Content,
                                 RatingValue = n.Rating.RatingValue.Value,
                                 CreatedDate = n.Rating.CreateDate.Value.ToString("MM/dd/yyyy HH:mm:ss")
                             });
                cpi.Reviews = query1.ToList();
                return cpi;
            }

            return null;
        }

        public static List<WorkingHour> GetWorkingHour(CompanyHour[] lstCompanyHour)
        {
            var query = from n in lstCompanyHour
                        group n by new { n.FromHour, n.ToHour } into g
                        select new { g.Key };

            List<WorkingHour> lstWorkingHours = new List<WorkingHour>();

            if (lstCompanyHour.Any(n => n.IsDaily.HasValue && n.IsDaily.Value))
            {
                var daily = lstCompanyHour.Where(n => n.IsDaily.HasValue && n.IsDaily.Value);
                foreach (CompanyHour companyHour in daily)
                {
                    lstWorkingHours.Add(new WorkingHour()
                    {
                        FromHour = companyHour.FromHour.Hours,
                        FromMinute = companyHour.FromHour.Minutes,
                        ToHour = companyHour.ToHour.Hours,
                        ToMinute = companyHour.ToHour.Minutes,
                        DayRange = "mon-sun",
                        FirstDay = 0,
                        LastDay = 6
                    });
                }
            }
            else
            {
                if (lstCompanyHour.Length > 0)
                {
                    for (int i = 0; i < lstCompanyHour.Count(); i++)
                    {
                        lstWorkingHours.Add(new WorkingHour()
                        {
                            FromHour = lstCompanyHour[i].FromHour.Hours,
                            FromMinute = lstCompanyHour[i].FromHour.Minutes,
                            ToHour = lstCompanyHour[i].ToHour.Hours,
                            ToMinute = lstCompanyHour[i].ToHour.Minutes,
                            DayRange = GetNameOfDay(lstCompanyHour[i].DayOfWeek),
                            FirstDay = lstCompanyHour[i].DayOfWeek,
                            LastDay = lstCompanyHour[i].DayOfWeek
                        });
                    }

                }
            }

            return lstWorkingHours.ToList();
        }



        private static string GetNameOfDay(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case 0:
                    return "sun";
                    break;
                case 1:
                    return "mon";
                    break;
                case 2:
                    return "tue";
                    break;
                case 3:
                    return "wed";
                    break;
                case 4:
                    return "thu";
                    break;
                case 5:
                    return "fri";
                    break;
                case 6:
                    return "sat";
                    break;
                default:
                    return "";
                    break;
            }
        }

        public static bool IsInvited(string firstName, string lastName, string email, out string message)
        {
            kuyamEntities ctx = DBContext;
            var ret = ctx.Invites.Count(n => n.Email == email &&
            n.Active.HasValue && (n.Active.Value == true));
            if (ret > 0)
            {
                message = "the email has been used, please try again!";
                return false;
            }

            ret = ctx.Invites.Count(n => n.Email == email);
            if (ret == 0)
            {
                message = "Whoops, looks like there is an error with your network. Request is time out. Wait a few seconds and try us again.!";
                return false;
            }

            ret = ctx.Invites.Count(n => n.Email == email &&
            n.Active.HasValue && n.Active.Value == false && n.Status == 2);
            if (ret > 1)
            {
                message = " User is invited";
                return true;
            }

            message = "Whoops, looks like there is an error with your network. Request is time out. Wait a few seconds and try us again.!";
            return false;
        }

        public static List<CalendarIphone> GetCalendarByCustID(int custID)
        {
            kuyamEntities ctx = DBContext;
            var query = (from c in ctx.Calendars
                         join cs in ctx.CalendarShares on c.CalendarID equals cs.CalendarID
                         where cs.CustID == custID
                         select new CalendarIphone
                         {
                             CalendarID = c.CalendarID,
                             Name = c.Name,
                             BackColor = c.BackColor,
                             ForeColor = c.ForeColor,
                             IsDefault = c.IsDefault,
                             CalendarDisplayTypeID = c.CalendarDisplayTypeID
                         });
            return query.ToList();
        }


        public static List<CompanyEmployee> GetEmployeesListByServiceID(int serviceCompanyID)
        {

            kuyamEntities _context = DBContext;

            var result = (from ce in _context.CompanyEmployees
                          join es in _context.EmployeeServices on ce.EmployeeID equals es.CompanyEmployeeID
                          join sc in _context.ServiceCompanies on es.ServiceCompanyID equals sc.ServiceCompanyID
                          where sc.ServiceCompanyID == serviceCompanyID
                          select ce).Distinct().OrderBy(x => x.EmployeeName).ToList();
            return result;


        }

        public static string GetEmployeeNameFromEmployeeID( int employeeID)
        {
            var name = string.Empty;
            kuyamEntities _context = DBContext;
            if(_context.CompanyEmployees.Any(x => x.EmployeeID == employeeID))
            {
                name = _context.CompanyEmployees.Where(x => x.EmployeeID == employeeID).First().EmployeeName;
            }
            return name;
        }


        public static string GetServiceNameFromServiceCompanyId(int serviceCompanyId)
        {
            var name = string.Empty;
            kuyamEntities _context = DBContext;
            if (_context.ServiceCompanies.Any(x => x.ServiceCompanyID == serviceCompanyId))
            {
                name = _context.ServiceCompanies.Where(x => x.ServiceCompanyID == serviceCompanyId).First().ServiceName;
            }
            return name;
        }

        public static List<AppointmentLog> GetAppointmentNoteIPhonesByAppointmentID(int appointmentID)
        {
            kuyamEntities _context = DBContext;

            var query = (from al in _context.AppointmentLogs
                         where al.AppointmentID == appointmentID
                         select al).ToArray().OrderByDescending(x => x.LogDT);

            /*
            var result = query.Select(n => new AppointmentLogIPhone()
                         {
                             AppointmentLogID = n.AppointmentID,
                             CustID = n.CustID,
                             Message = n.Message.Replace("<br/>", "\n"),
                             Viewed = n.Viewed,
                             DateString = n.LogDT.ToString("yyyy-MM-dd HH:mm:ss"),
                             LogDT = n.LogDT
                         }).ToList();
             */
            return query.ToList();
        }


        public static bool UpdateAppointmentInfo(Appointment appointment)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.Appointments.Any(x => x.AppointmentID == appointment.AppointmentID))
                {
                    Appointment app = _context.Appointments.Where(x => x.AppointmentID == appointment.AppointmentID).FirstOrDefault();
                    app.ContactType = appointment.ContactType;
                    app.Price = appointment.Price;
                    app.AppointmentStatusID = appointment.AppointmentStatusID;
                    app.SenderEmail = appointment.SenderEmail;
                    app.PreapprovalKey = appointment.PreapprovalKey;
                    _context.SaveChanges();
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static int GetCountOfAppointmentHasSameStatus(int appointmentID, DateTime date)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.Appointments.Any(x => x.AppointmentID == appointmentID))
                {
                    Appointment app = _context.Appointments.Where(x => x.AppointmentID == appointmentID).FirstOrDefault();
                    if (_context.Appointments.Any(x => x.CustID == app.CustID && x.AppointmentStatusID == app.AppointmentStatusID && x.Start >= date))
                    {
                        return _context.Appointments.Where(x => x.CustID == app.CustID && x.AppointmentStatusID == app.AppointmentStatusID && x.Start >= date).Count();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static string GetCategoryNameFromCategoryId(int categoryId)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.Services.Any(x => x.ServiceID == categoryId && x.ParentServiceID == null))
                {
                    return _context.Services.Where(x => x.ServiceID == categoryId && x.ParentServiceID == null).First().ServiceName;

                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the duration available of employee.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="employeeId">The employee id.</param>
        /// <returns></returns>
        public static TimeSpan GetDurationAvailableOfEmployee(DateTime startTime, int employeeId)
        {
            kuyamEntities context = DBContext;

            int dateOfWeek = (int)startTime.DayOfWeek;

            // get time available of user, which cover start time
            var employeeHour =
                context.EmployeeHours.FirstOrDefault(
                    e =>
                    e.CompanyEmployeeID == employeeId && e.DayOfWeek == dateOfWeek && e.FromHour <= startTime.TimeOfDay &&
                    e.ToHour > startTime.TimeOfDay);

            if (employeeHour == null)
                return TimeSpan.Zero;

            DateTime endTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, employeeHour.ToHour.Hours, employeeHour.ToHour.Minutes, 0);

            DateTime temp = DateTime.UtcNow.AddMinutes(-10);

            // get appointment inside start end and end time           
            var appointment = context.Appointments.Where(
                        a =>
                            (a.EmployeeID == employeeId
                            //  || context.AppointmentParticipants.Where(ap=>ap.CalendarID==calendarId).Select(ap=>ap.AppointmentID).Contains(a.AppointmentID)
                            )
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                        && a.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                        && (a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending || temp < a.Created)
                        && startTime <= a.Start && endTime > a.Start)
                        .OrderBy(a => a.Start).FirstOrDefault();
            if (appointment != null)
                endTime = appointment.Start;

            // Check appointment temp
            //var appointmentTemp = context.AppointmentTemps.Where(
            //            a => a.EmployeeID == employeeId
            //            && a.AppointmentStatusID == (int)Types.AppointmentStatus.TemporaryPending
            //            && temp < a.Created
            //            && startTime <= a.Start && endTime > a.Start)
            //            .OrderBy(a => a.Start).FirstOrDefault();
            //if (appointmentTemp != null && endTime > appointmentTemp.Start)
            //    endTime = appointmentTemp.Start;



            return endTime - startTime;
        }

        public static bool IsAppointmentPaid(int appointmentID, int payMethodID)
        {
            kuyamEntities _context = DBContext;
            if (_context.Appointments.Any(x => x.AppointmentID == appointmentID && (payMethodID == (int)Types.PaymentMethod.Paypal || x.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed)))
                return true;
            return false;
        }


        public static List<AppointmentIPhone> GetAppoinmentsIPhoneByCustId(int custID, DateTime startDate, DateTime endDate)
        {
            kuyamEntities _context = DBContext;
            try
            {
                var query = (from ap in _context.Appointments
                             join app in _context.AppointmentParticipants on ap.AppointmentID equals app.AppointmentID
                             where ap.CustID == custID && ap.Start >= startDate && ap.Start <= endDate
                             && ap.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                             && ap.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending
                             && ap.AppointmentStatusID != (int)Types.AppointmentStatus.Unknown
                             select new { Appointment = ap, AppointmentParticipant = app }).ToArray();

                var query1 = query.Select(n => new AppointmentIPhone(n.Appointment, n.AppointmentParticipant)).OrderBy(x => x.Start).ToList();

                foreach (AppointmentIPhone appointmentIPhone in query1)
                {
                    appointmentIPhone.NumberNotesUnread = DAL.GetUnreadNotes(appointmentIPhone.AppointmentID).Count;
                }
                return query1;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static AppointmentIPhone GetAppointmentIPByAppointmentId(int Id, DateTime? currenTime = null)
        {
            kuyamEntities _context = DBContext;

            if (currenTime == null)
                currenTime = DateTime.UtcNow;
            try
            {
                var appointment = (from ap in _context.Appointments
                                   join app in _context.AppointmentParticipants on ap.AppointmentID equals app.AppointmentID
                                   where ap.AppointmentID == Id
                                   select new { Appointment = ap, AppointmentParticipant = app }).First();


                return new AppointmentIPhone(appointment.Appointment, appointment.AppointmentParticipant, currenTime.Value, true);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<UserPackagePurchaseIP> GetListPurchasedPackage(int custID)
        {
            try
            {
                kuyamEntities _context = DBContext;
                var query = (from upp in _context.UserPackagePurchases
                             where upp.CustID == custID &&
                             (upp.UserPackageStatus == null ||
                             upp.UserPackageStatus != (int)Types.UserPackageStatus.Delete)
                             select upp).ToArray();

                var query1 = query.Select(n => new UserPackagePurchaseIP()
                {
                    UserPackagePurchaseId = n.UserPackagePurchaseId,
                    CompanyPackageId = n.CompanyPackageId,
                    PurchaseDate = n.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    OrginalPrice = n.OrginalPrice,
                    PurchasePrice = n.PurchasePrice,
                    CustID = n.CustID,
                    ExpiredDate = n.ExpiredDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    MaxUses = n.MaxUses,
                    UserPackageStatus = n.UserPackageStatus,
                    PackageDetail = GetPackageIPDetail(n.CompanyPackageId),
                    CompanyName = n.CompanyPackage.ProfileCompany.Name
                }).ToList();
                return query1;
            }
            catch
            {
                return null;
            }
        }

        public static PackageIP GetPackageIPDetail(int packageID)
        {
            kuyamEntities _context = DBContext;
            try
            {
                if (_context.CompanyPackages.Any(x => x.PackageId == packageID))
                {
                    CompanyPackage cp = _context.CompanyPackages.Where(x => x.PackageId == packageID).FirstOrDefault();
                    PackageIP pIP = new PackageIP()
                    {
                        PackageId = cp.PackageId,
                        PackageName = cp.PackageName,
                        Description = cp.Description,
                        ProfileCompanyId = cp.ProfileCompanyId,
                        PackageType = cp.PackageType,
                        NumberOfBooking = cp.NumberOfBooking,
                        Price = cp.Price,
                        Status = cp.Status,
                        DurationInMonth = cp.DurationInMonth,
                        KalturaImageId = cp.KalturaImageId,
                        StartDate = cp.StartDate,
                        EndDate = cp.EndDate,
                        CreateDate = cp.CreateDate,
                        ModifiedDate = cp.ModifiedDate,
                        UnitPrice = cp.UnitPrice,
                        Services = GetCompanyServicesFromPackageID(packageID)
                    };
                    return pIP;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<CompanyService> GetCompanyServicesFromPackageID(int packageID)
        {
            kuyamEntities _context = DBContext;

            try
            {
                var service = (from s in _context.Services
                               join sc in _context.ServiceCompanies on s.ServiceID equals sc.ServiceID
                               join cps in _context.CompanyPackageServices on sc.ServiceCompanyID equals cps.ServiceCompanyId
                               where sc.Status == (int)Types.ServiceCompanyStatus.Active && cps.CompanyPackageId == packageID
                                     && s.ParentServiceID.HasValue
                               select new CompanyService()
                               {
                                   ID = sc.ServiceCompanyID,
                                   Price = sc.Price.Value,
                                   ServiceName = s.ServiceName,
                                   AttendeesNumber = sc.AttendeesNumber.Value,
                                   Description = s.Desc,
                                   Duration = sc.Duration.Value
                               }).Distinct();
                return service.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static CustIPhone GetCustIPhoneByFaceBookUserID(string facebookUserID)
        {
            kuyamEntities ctx = DBContext;
            var custIp = new CustIPhone();

            Cust cust = ctx.Custs.FirstOrDefault(x => x.FacebookUserID == facebookUserID && x.Status == (int)Types.UserStatusType.Active);
            if (cust == null)
                return custIp;

            return new CustIPhone(cust);
            /*
            aspnet_Membership user = ctx.aspnet_Membership.Where(x => x.aspnet_Users.UserName == cust.Username).FirstOrDefault();
            string email = user.Email;

            custIP.CustID = cust.CustID;
            custIP.AccountID = cust.AccountID;
            custIP.AspUserID = cust.AspUserID;
            custIP.Email = email;
            custIP.CustTypeID = cust.CustTypeID;
            custIP.FirstName = cust.FirstName;
            custIP.LastName = cust.LastName;
            custIP.Street1 = cust.Street1;
            custIP.Street2 = cust.Street2;
            custIP.City = cust.City;
            custIP.State = cust.State;
            custIP.Zip = cust.Zip;
            custIP.HomePhone = cust.HomePhone;
            custIP.MobilePhone = cust.MobilePhone;
            custIP.MobileCarrier = cust.MobileCarrier;
            custIP.WorkPhone = cust.WorkPhone;
            custIP.PreferredPhoneTypeID = cust.PreferredPhoneTypeID;
            custIP.FirstAlert = cust.FirstAlert;
            custIP.SecondAlert = cust.SecondAlert;
            custIP.IcalUrl = cust.IcalUrl;
            custIP.EmailHtml = cust.EmailHtml;
            custIP.GenderTypeID = cust.GenderTypeID;
            custIP.CustStatusTypeID = cust.CustStatusTypeID;
            custIP.CustStatusReasonTypeID = cust.CustStatusReasonTypeID;
            custIP.CustStatusNote = cust.CustStatusNote;
            custIP.Notes = cust.Notes;
            custIP.Birthday = cust.Birthday;
            custIP.LastLogin = cust.LastLogin;
            custIP.Created = cust.Created.ToString("yyyy-MM-dd HH:mm:ss");
            custIP.Modified = cust.Modified;
            custIP.LastVisit = cust.LastVisit;
            custIP.Latitude = cust.Latitude;
            custIP.Longitude = cust.Longitude;
            custIP.OtherCalendar = cust.OtherCalendar;
            custIP.OutlookCalendar = cust.OutlookCalendar;
            custIP.YahooCalendar = cust.YahooCalendar;
            custIP.TimeZoneId = cust.TimeZoneId;
            custIP.Status = cust.Status;
            custIP.FacebookToken = cust.FacebookToken;
            custIP.FacebookUserID = cust.FacebookUserID;
            custIP.LocationReminder = cust.LocationReminder;
            return custIP;
             */
        }


        public static Cust GetCustByCustId(int custId)
        {
            kuyamEntities ctx = DBContext;

            Cust cust = ctx.Custs.Where(x => x.CustID == custId).FirstOrDefault();
            if (cust == null)
                return null;

            return cust;
        }

        public static CustIPhone GetCustIphoneByCustId(int custId)
        {
            kuyamEntities ctx = DBContext;

            Cust cust = ctx.Custs.Where(x => x.CustID == custId).FirstOrDefault();
            if (cust == null)
                return null;

            return new CustIPhone(cust);
        }

        public static bool SetAppointmentIsView(int appointmentID)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                Appointment apt = ctx.Appointments.Where(x => x.AppointmentID == appointmentID).FirstOrDefault();
                if (apt == null)
                    return false;
                apt.IsViewDetail = true;
                ctx.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUserDeviceExist(int custID, string deviceID)
        {
            kuyamEntities ctx = DBContext;
            if (ctx.UserDevices.Any(x => x.CustId == custID && x.DeviceID == deviceID))
                return true;
            return false;
        }


        public static bool AddUserDevice(int custID, string deviceID, int deviceType, int appId = 0, int serviceVersion = (int)Types.ServiceVersion.AppointmentStatus)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                if (ctx.UserDevices.Any(x => x.CustId == custID
                                             && x.DeviceID == deviceID
                                             && x.AppId == appId
                                             && x.DeviceType == deviceType
                                             && x.ServiceVersion == serviceVersion))
                    return true;

                var oldDevice = ctx.UserDevices.FirstOrDefault(x => x.CustId == custID
                                                                    && x.DeviceType == deviceType
                                                                    && x.AppId == appId);
                if (oldDevice != null)
                {
                    oldDevice.DeviceID = deviceID;
                    oldDevice.ServiceVersion = serviceVersion;
                    oldDevice.DateUsage = DateTime.UtcNow;
                }
                else
                {
                    UserDevice ud = new UserDevice();
                    ud.CustId = custID;
                    ud.DeviceID = deviceID;
                    ud.DeviceType = deviceType;
                    ud.AppId = appId;
                    ud.DateUsage = DateTime.UtcNow;
                    ud.ServiceVersion = serviceVersion;
                    ctx.UserDevices.Add(ud);
                }
                ctx.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteUserDevice(int custID, int deviceType, int appId)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                var device = ctx.UserDevices.FirstOrDefault(x => x.CustId == custID && x.DeviceType == deviceType && x.AppId == appId);
                if (device != null)
                {
                    ctx.UserDevices.Remove(device);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool UpdateUserReminderSetting(int custID, int firstAlert, int secondAlert, int methodAlert, bool locationReminder = false)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                if (ctx.Custs.Any(x => x.CustID == custID))
                {
                    Cust cust = ctx.Custs.Where(x => x.CustID == custID).FirstOrDefault();
                    if (cust != null && cust.Status == (int)Types.CustStatus.Active)
                    {
                        cust.FirstAlert = firstAlert;
                        cust.SecondAlert = secondAlert;
                        cust.PreferredPhoneTypeID = methodAlert;
                        cust.LocationReminder = locationReminder;
                        ctx.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static List<CityState> GetCities(string city, string state)
        {

            kuyamEntities ctx = DBContext;
            var query = (from zc in ctx.ZipCodes
                         where zc.State != string.Empty && zc.State != null
                         && zc.City != string.Empty && zc.City != null
                         && (zc.City.ToLower().Contains(city.ToLower())
                            || zc.State.ToLower().Contains(state.ToLower()))
                         select new CityState()
                         {
                             City = zc.City,
                             State = zc.State
                         }).Distinct().ToList();
            return query;
        }

        public static List<string> GetListZipCodesFromCityName(string cityName)
        {
            kuyamEntities ctx = DBContext;
            var query = (from zc in ctx.ZipCodes
                         where zc.State != string.Empty && zc.State != null
                         && zc.City == cityName
                         select zc.Code).Distinct().ToList();
            return query;
        }

        public static List<string> GetListZipByCityName(string cityName)
        {
            kuyamEntities ctx = DBContext;
            var query = (from zc in ctx.ZipCodes
                         where zc.City == cityName
                         select zc.Code).Distinct().ToList();
            return query;
        }

        public static Discount GetDiscountByDiscountCode(string code, int profileID)
        {
            kuyamEntities ctx = DBContext;
            var query = (from d in ctx.Discounts
                         where d.ProfileCompanyId == profileID && d.Code == code
                         select d).FirstOrDefault();
            return query;
        }

        public static bool RemoveUserPackagePurchased(int userPackageID)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                if (ctx.UserPackagePurchases.Any(x => x.UserPackagePurchaseId == userPackageID))
                {
                    UserPackagePurchase upp = ctx.UserPackagePurchases.Where(x => x.UserPackagePurchaseId == userPackageID).FirstOrDefault();
                    upp.UserPackageStatus = (int)Types.UserPackageStatus.Delete;
                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static List<AppointmentNotify> GetUnreadNotes(int appointmentId)
        {
            try
            {
                kuyamEntities ctx = DBContext;
                return ctx.AppointmentNotifies.Where(a => a.AppointmentId == appointmentId && !a.IsRead && a.NotifyType == (int)Types.NotificationType.AppointmentNote).ToList();
            }
            catch
            {
            }
            return new List<AppointmentNotify>();
        }
        public static Appointment GetAppointmentByCustId(int custId)
        {
            kuyamEntities _ctx = DBContext;
            return _ctx.Appointments.FirstOrDefault(apt => apt.CustID == custId
                                                           && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                                                           && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                                                           && apt.Rating == null);
        }

        public static List<be_PostMedia> GetPostMedia(int postId)
        {
            kuyamEntities _ctx = DBContext;
            return _ctx.be_PostMedia.Where(m => m.PostID == postId).ToList();
        }


        //public static List<EmployeeHour> GetInstructorHoursFromClassId(int classId)
        //{
        //    kuyamEntities _ctx = DBContext;
        //    var query = (from cs in _ctx.ClassSchedulers
        //                 join sc in _ctx.ServiceCompanies on cs.ServiceCompanyID equals sc.ServiceCompanyID
        //                 join cis in _ctx.ClassInstructorSchedulers on cs.ClassSchedulerID equals cis.ClassSchedulerID
        //                 where sc.ServiceCompanyID == classId && !cs.IsPreview
        //                 select new EmployeeHour
        //                 {
        //                     ID = cs.ClassSchedulerID,
        //                     FromHour = cs.FromHour,
        //                     ToHour = cs.ToHour,
        //                     CompanyEmployeeID = cis.EmployeeID,
        //                     DayOfWeek = cs.DayOfWeek,
        //                     IsDaily = cs.IsPreview,
        //                     LastedUpdate = cs.LastedUpdate,
        //                     CompanyEmployee = null
        //                 }).ToList();

        //    return query;
        //}


        //public static List<EmployeeHour> GetInstructorHourPreview(int employeeId)
        //{
        //    try
        //    {
        //        kuyamEntities _ctx = DBContext;
        //        var query = (from cs in _ctx.ClassSchedulers
        //                     join cis in _ctx.ClassInstructorSchedulers on cs.ClassSchedulerID equals cis.ClassSchedulerID
        //                     where cis.EmployeeID == employeeId && cs.IsPreview
        //                     select new EmployeeHour
        //                     {
        //                         ID = cs.ClassSchedulerID,
        //                         FromHour = cs.FromHour,
        //                         ToHour = cs.ToHour,
        //                         CompanyEmployeeID = cis.EmployeeID,
        //                         DayOfWeek = cs.DayOfWeek,
        //                         IsDaily = cs.IsPreview,
        //                         LastedUpdate = cs.LastedUpdate,
        //                         CompanyEmployee = null
        //                     }).ToList();

        //        return query;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}

        //public static List<EmployeeHour> GetInstructorHoursFromInstructorId(int employeeId)
        //{
        //    kuyamEntities _ctx = DBContext;
        //    var query = (from cs in _ctx.ClassSchedulers
        //                 join cis in _ctx.ClassInstructorSchedulers on cs.ClassSchedulerID equals cis.ClassSchedulerID
        //                 where cis.EmployeeID == employeeId
        //                 select new EmployeeHour
        //                 {
        //                     ID = cs.ClassSchedulerID,
        //                     FromHour = cs.FromHour,
        //                     ToHour = cs.ToHour,
        //                     CompanyEmployeeID = cis.EmployeeID,
        //                     DayOfWeek = cs.DayOfWeek,
        //                     IsDaily = cs.IsPreview,
        //                     LastedUpdate = cs.LastedUpdate,
        //                     CompanyEmployee = null
        //                 }).ToList();

        //    return query;
        //}


    }
}

