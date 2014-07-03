using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using Kuyam.Utility;


namespace Kuyam.Domain
{
    public class CustService
    {
        #region Fields

        private readonly IRepository<Cust> _custRepository;
        private readonly IRepository<aspnet_Membership> _membershipRepository;
        private readonly IRepository<aspnet_Users> _usersRepository;
        private readonly IRepository<RegularClient> _regularClientRepository;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<DiscountRegularClient> _discountRegularClientRepository;
        private readonly IRepository<AppointmentNotify> _appointmentNotifyRepository;
        private readonly IRepository<AppointmentLog> _appointmentLogRepository;


        #endregion

        #region Ctor

        public CustService(IRepository<Cust> custRepository,
           IRepository<aspnet_Membership> membershipRepository,
           IRepository<aspnet_Users> usersRepository,
           IRepository<RegularClient> regularClientRepository,
           IRepository<Appointment> appointmentRepository,
           IRepository<DiscountRegularClient> discountRegularClientRepository, IRepository<AppointmentNotify> appointmentNotify, IRepository<AppointmentLog> appointmentLogRepository)
        {
            this._custRepository = custRepository;
            this._membershipRepository = membershipRepository;
            this._usersRepository = usersRepository;
            this._regularClientRepository = regularClientRepository;
            this._appointmentRepository = appointmentRepository;
            this._discountRegularClientRepository = discountRegularClientRepository;
            _appointmentNotifyRepository = appointmentNotify;
            _appointmentLogRepository = appointmentLogRepository;
        }

        #endregion

        public Cust GetCustomerCustID(int custId)
        {
            if (custId == 0)
                return null;
            var cust = _custRepository.Table.First(x => x.CustID == custId);
            return cust.Decrypt();
        }

        public aspnet_Membership GetMembershipByUserID(Guid UserId)
        {
            if (UserId == null)
                return null;
            var membership = _membershipRepository.Table.First(x => x.UserId == UserId);
            return membership;
        }

        public aspnet_Users GetUsersByUserID(Guid UserId)
        {
            if (UserId == null)
                return null;
            var users = _usersRepository.Table.First(x => x.UserId == UserId);
            return users;
        }

        public Cust GetUsersByUserID(string userName)
        {
            if (userName == null)
                return null;
            var query = from c in _custRepository.Table
                        join u in _usersRepository.Table on c.AspUserID equals u.UserId
                        where u.UserName == userName
                        select c;
            return query.FirstOrDefault();
        }

        public void UpdateCustomer(Cust cust)
        {
            //if (cust == null)
            //    throw new ArgumentNullException("customer");
            try
            {
                _custRepository.Update(cust);
                LogHelper.Info(string.Format("Updated customer: CustID= {0}", cust.CustID));
            }
            catch (ArgumentNullException arg)
            {
                LogHelper.Error("Update customer fail: ", arg);
            }
        }

        public void UpdateMembership(aspnet_Membership membership)
        {
            if (membership == null)
                throw new ArgumentNullException("customer");
            _custRepository.Update(new Cust());
        }

        public void UpdateUser(aspnet_Users user)
        {


        }

        #region Manage Calendar
        public List<Calendar> GetActiveCalendarsbyCustId(int custId)
        {
            return DAL.GetActiveCalendarsbyCustId(custId);
        }

        public void AddDefaultCalendar(int custID, string name)
        {
            DAL.AddDefaultCalendar(custID, name);
        }

        public void DeleteCalendar(int custID, int calendarId)
        {
            DAL.DeleteCalendar(custID, calendarId);
        }
        public void UpdateCalendar(int calendarId, string name)
        {
            DAL.UpdateCalendar(calendarId, name);
        }
        #endregion Manage Calendar

        //Trong Edit

        #region Regular Client

        /// <summary>
        /// Get List Regular Client by company profile id
        /// </summary>
        /// <param name="CompanyProfileId">int:Company ID</param>
        /// <returns>List<RegularClient></returns>
        public List<RegularClient> GetRegularClientListByCompanyProfileId(int CompanyProfileId)
        {
            return _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId && r.Status == true).ToList();
        }

        public List<RegularClient> GetRegularClientListByCompanyProfileId(int CompanyProfileId, string key, int searchType)
        {
            List<RegularClient> result = new List<RegularClient>();

            if (searchType == (int)Types.RegularClientSearchKeyType.FirstName)
            {
                result = _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId
                    && r.Status == true && (key == null || key == string.Empty || r.FirstName.StartsWith(key))).OrderBy(r => r.FirstName).ToList();
            }
            else if (searchType == (int)Types.RegularClientSearchKeyType.LastName)
            {
                result = _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId
                    && r.Status == true && (key == null || key == string.Empty || r.LastName.StartsWith(key))).OrderBy(r => r.LastName).ToList();
            }
            else if (searchType == (int)Types.RegularClientSearchKeyType.Email)
            {
                result = _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId
                    && r.Status == true && (key == null || key == string.Empty || r.Email.StartsWith(key))).OrderBy(r => r.Email).ToList();
            }
            else if (searchType == (int)Types.RegularClientSearchKeyType.Newest)
            {
                result = _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId
                    && r.Status == true && (key == null || key == string.Empty || r.Email.StartsWith(key)) || r.FirstName.StartsWith(key) || r.LastName.StartsWith(key) || r.Email.StartsWith(key)).OrderBy(r => r.CreatedDate).ToList();
            }
            else
            {
                result = _regularClientRepository.Table.Where(r => r.CompanyProfileId == CompanyProfileId
                    && r.Status == true && (key == null || key == string.Empty || r.FirstName.StartsWith(key) || r.FirstName.StartsWith(key) || r.LastName.StartsWith(key) || r.Email.StartsWith(key))).OrderBy(r => r.FirstName).ToList();
            }

            return result;


        }

        public bool CheckUserIsActiveByEmail(string email)
        {            
            var cust = DAL.xGetCust(email);
            if (cust != null)
            {
                return cust.Status == (int)Types.CustStatus.Active;
            }
            return false;
        }

        public bool CheckCustInAppoinmentsByCustId(string email)
        {
            bool result = false;

            var cust = DAL.xGetCust(email);
            if (cust == null)
            {
                return false;
            }

            List<Appointment> appointmentList = _appointmentRepository.Table.Where(p => (p.CustID == cust.CustID)).ToList();
            if (appointmentList != null && appointmentList.Count > 0)
            {
                result = true;
            }
            return result;
        }

        public bool DeleteRegularClient(int regularClientId)
        {

            bool result = false;

            RegularClient regularClient = _regularClientRepository.Table.First(r => r.RegularClientId == regularClientId);

            try
            {

                regularClient.Status = false;
                _regularClientRepository.Update(regularClient);
                LogHelper.Info(string.Format("Deleted regular client: RegularClientId= {0}", regularClient.RegularClientId));
                result = true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Delete regular client fail: ", ex);
                result = false;
            }

            return result;
        }

        public bool InsertRegularClient(List<RegularClient> regularClients)
        {
            bool result = false;
            try
            {
                if (regularClients != null && regularClients.Count > 0)
                {
                    foreach (RegularClient regularClient in regularClients)
                    {
                        _regularClientRepository.Insert(regularClient);
                        LogHelper.Info(string.Format("Added regular client: RegularClientId= {0}", regularClient.RegularClientId));
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Add regular client fail: ", ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Check exest email
        /// </summary>
        /// <param name="email">string:email</param>
        /// <returns></returns>
        public bool CheckRegularClientEmail(string email)
        {
            bool result = false;
            List<RegularClient> regularClients = _regularClientRepository.Table.Where(r => r.Email == email).ToList();
            if (regularClients.Count > 0)
            {
                result = true;
            }
            return result;
        }

        public bool InsertDiscountRegularClient(DiscountRegularClient discountRegularClient)
        {
            bool result = false;
            try
            {
                if (discountRegularClient != null)
                {

                    _discountRegularClientRepository.Insert(discountRegularClient);
                    LogHelper.Info(string.Format("Added discount regular client: RegularClientId= {0}", discountRegularClient.RegularClientId));
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Add discount regular client fail: ", ex);
                result = false;
            }
            return result;

        }

        public bool UpdateRegularClient(RegularClient regularClient)
        {
            bool result = false;
            if (regularClient != null)
            {
                try
                {
                    _regularClientRepository.Update(regularClient);
                    LogHelper.Info(string.Format("Updated regular client: RegularClientId= {0}", regularClient.RegularClientId));
                    result = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Update regular client fail: ", ex);
                }
            }
            return result;
        }

        public RegularClient GetRegularClientByID(int regularClientId)
        {
            return _regularClientRepository.Table.Where(r => r.RegularClientId == regularClientId).FirstOrDefault();
        }

        public bool RegularClientCheckExistEmail(string email, int profileId)
        {
            return _regularClientRepository.Table.Where(r => r.CompanyProfileId == profileId && r.Status).Any(r => r.Email == email);
        }

        #endregion

        //-------------

        /// <summary>
        /// Gets the companies have note for cust
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public List<KeyValuePair<ProfileCompany, int>> GetCompaniesHaveNote(int custId, DateTime fromDate)
        {
            List<KeyValuePair<ProfileCompany, int>> results = new List<KeyValuePair<ProfileCompany, int>>();
            var appointments = _appointmentRepository.Table.Where(a => a.CustID == custId && a.AppointmentLogs.Any(l => l.LogDT >= fromDate.Date)).Distinct();

            //_appointmentNotifyRepository.Table.Where(
            //    a => a.CustId == custId 
            //        && a.NotifyType == (int) Types.NotificationType.AppointmentNote 
            //        && a.DateSent>=fromDate)
            //        .Select(a=>a.Appointment).Distinct();
            var bookcompanies = appointments.Select(a => a.ServiceCompany.ProfileCompany).Distinct();
            var noncompanies = appointments.Select(a => a.ProfileCompany).Distinct();
            var companies = bookcompanies.Union(noncompanies);
            foreach (var company in companies)
            {
                if (company != null && company.ServiceCompanies != null)
                {
                    var profileId = company.ProfileID;
                    var serviceIds = company.ServiceCompanies.Select(c => c.ServiceCompanyID);
                    var companyavailabilityAppointmentIds = appointments.Where(a => serviceIds.Contains(a.ServiceCompanyID.Value)).Select(a => a.AppointmentID);
                    var noncompanyAppointmentIds = appointments.Where(a => a.ProfileId == profileId).Select(a => a.AppointmentID);
                    var companyAppointmentIds = companyavailabilityAppointmentIds.Union(noncompanyAppointmentIds);
                    int totalNotes = _appointmentNotifyRepository.Table.Count(a => companyAppointmentIds.Contains(a.AppointmentId)
                                                                      && !a.IsRead
                                                                      && a.NotifyType == (int)Types.NotificationType.AppointmentNote);
                    var resultItem = new KeyValuePair<ProfileCompany, int>(company, totalNotes);
                    results.Add(resultItem);
                }

            }
            return results;
        }

        public Appointment GetAppointmentByCustId(int custId)
        {
            return _appointmentRepository.Table.Where(apt => apt.CustID == custId
                && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Cancelled
                && apt.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                && apt.Rating == null).FirstOrDefault();
        }
    }
}
