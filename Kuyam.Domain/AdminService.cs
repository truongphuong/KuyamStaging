using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using Kuyam.Domain.Admin;
using Kuyam.Repository.Interface;
using Kuyam.Database;
using System.Web.Security;
using Kuyam.Database.Extensions;
using Kuyam.Utility;



namespace Kuyam.Domain
{
    public class AdminService
    {
        #region Fields

        private readonly IRepository<AccessKeyManagement> _accessKeyRepository;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<FeaturedCompany> _featuredCompanyRepository;
        private readonly IRepository<Invite> _inviteRepository;
        private readonly IRepository<Cust> _custRepository;
        private readonly IRepository<aspnet_Users> _userRepository;

        private readonly IRepository<aspnet_Membership> _membershipRepository;
        private readonly IRepository<aspnet_Roles> _rolesRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<ZipCode> _zipCodeRepository;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<Rating> _ratingRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<State> _stateRepository;
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IRepository<HotelCode> _hotelCodeRepository;
        private readonly IRepository<HotelStaff> _hotelStaffRepository;
        private readonly IRepository<FeaturedHotel> _featuredHotelRepository;
        private readonly IRepository<HotelMedia> _hotelMediaRepository;
        private readonly IRepository<HotelVisit> _hotelVisitRepository;
        private readonly IRepository<NonKuyamAppointment> _nonKuyamAppointmentRepository;
        private readonly IRepository<ProfileCompanyTemp> _profileCompanyTempRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CompanyEvent> _companyEventRepository;
        private readonly IRepository<CompanyServiceEvent> _companyServiceEventRepository;
        private readonly DbContext _dbContext;
        #endregion

        #region Ctor

        public AdminService(IRepository<AccessKeyManagement> accessKeyRepository,
            IRepository<Setting> settingRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<FeaturedCompany> featuredCompanyRepository,
            IRepository<Invite> inviteRepository,
            IRepository<Cust> custRepository,
            IRepository<aspnet_Users> userRepository,
            IRepository<aspnet_Membership> membershipRepository,
            IRepository<Service> serviceRepository,
            IRepository<ZipCode> zipCodeRepository,
            IRepository<Appointment> appointmentRepository,
            IRepository<Rating> ratingRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<State> stateRepository,
            IRepository<Order> orderRepository,
            IRepository<Hotel> hotelRepository,
            IRepository<aspnet_Roles> rolesRepository,
            IRepository<HotelCode> hotelCodeRepository,
            IRepository<HotelStaff> hotelStaffRepository,
            IRepository<FeaturedHotel> featuredHotelRepository,
            IRepository<HotelMedia> hotelMediaRepository,
            IRepository<NonKuyamAppointment> nonKuyamAppointmentRepository,
            IRepository<ProfileCompanyTemp> profileCompanyTempRepository,
            DbContext dbContext,
            IRepository<HotelVisit> hotelVisitRepository,
            IRepository<Event> eventRepository,
            IRepository<CompanyEvent> companyEventRepository,
            IRepository<CompanyServiceEvent> companyServiceEventRepository
            )
        {
            this._accessKeyRepository = accessKeyRepository;
            this._settingRepository = settingRepository;
            this._profileCompanyRepository = profileCompanyRepository;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._featuredCompanyRepository = featuredCompanyRepository;
            this._inviteRepository = inviteRepository;
            this._custRepository = custRepository;
            this._userRepository = userRepository;
            this._membershipRepository = membershipRepository;
            this._serviceRepository = serviceRepository;
            this._eventRepository = eventRepository;
            this._zipCodeRepository = zipCodeRepository;
            this._orderRepository = orderRepository;
            this._appointmentRepository = appointmentRepository;
            this._ratingRepository = ratingRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._stateRepository = stateRepository;
            this._hotelRepository = hotelRepository;
            this._rolesRepository = rolesRepository;
            this._hotelCodeRepository = hotelCodeRepository;
            this._hotelStaffRepository = hotelStaffRepository;
            this._featuredHotelRepository = featuredHotelRepository;
            this._hotelMediaRepository = hotelMediaRepository;
            this._nonKuyamAppointmentRepository = nonKuyamAppointmentRepository;
            this._profileCompanyTempRepository = profileCompanyTempRepository;
            this._dbContext = dbContext;
            this._hotelVisitRepository = hotelVisitRepository;
            this._companyEventRepository = companyEventRepository;
            this._companyServiceEventRepository = companyServiceEventRepository;
        }
        #endregion

        public AccessKeyManagement CreateAccessKeyManagement(AccessKeyManagement accessKeyManagement)
        {
            AccessKeyManagement accesskey = _accessKeyRepository.Table.Where(m => m.EmailAdmin == accessKeyManagement.EmailAdmin && m.EmailUser == accessKeyManagement.EmailUser).FirstOrDefault();
            if (accesskey != null)
            {
                accesskey.Key = accessKeyManagement.Key;
                accesskey.Active = true;
                _accessKeyRepository.Update(accesskey);
                return accesskey;
            }
            _accessKeyRepository.Insert(accessKeyManagement);
            return accessKeyManagement;
        }

        public void DeleteAccessKeyManagement(string emailAdmin, string emailUser)
        {
            AccessKeyManagement accesskey = _accessKeyRepository.Table.Where(m => m.EmailAdmin == emailAdmin && m.EmailUser == emailUser).FirstOrDefault();
            if (accesskey != null)
            {
                accesskey.Active = false;
                _accessKeyRepository.Update(accesskey);
            }
        }

        public void DeleteAccessKeyManagement(int acessKeyid)
        {
            AccessKeyManagement accesskey = _accessKeyRepository.Table.Where(m => m.AccessKeyID == acessKeyid).FirstOrDefault();
            if (accesskey != null)
            {
                _accessKeyRepository.Delete(accesskey);
            }
        }

        public AccessKeyManagement GetAccessKeyManagementByUserAndkey(string emailUser, string key)
        {
            var query = _accessKeyRepository.Table.Where(m => m.EmailUser == emailUser && m.Key == key && m.Active == true).FirstOrDefault();
            return query;
        }

        public List<AccessKeyManagement> GetAccessKeyManagementByAdminAndkey(string emailAdmin)
        {
            var query = _accessKeyRepository.Table.Where(m => m.EmailAdmin.ToLower() == emailAdmin.ToLower());
            return query.ToList();
        }

        public Setting GetSettingByKey(string key)
        {
            return _settingRepository.Table.First(m => m.Name.ToLower() == key.ToLower());
        }

        /// <summary>
        /// Get all company by key name
        /// </summary>
        /// <param name="key">string: key</param>
        /// <param name="pageIndex">int: pageIndex</param>
        /// <param name="pageSize">int: pageSize</param>]
        /// /// <param name="pageSize">int: totalRecord</param>
        /// <returns>Profile company list</returns>
        public List<ProfileCompany> AdminGetListCompanyByKeyName(string key, int searchType, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            if (searchType == (int)Types.CompanyStatus.Unknown)
            {

                var query = _profileCompanyRepository.Table.Where(p => (key == null || key == string.Empty || p.Name.ToLower().Contains(key.ToLower()) || p.Email.ToLower().Contains(key.ToLower()) || p.Phone.ToLower().Contains(key.ToLower()) || p.Street1.ToLower().Contains(key.ToLower()) || p.City.ToLower().Contains(key.ToLower()))
                            && p.CompanyStatusID != (int)Kuyam.Database.Types.CompanyStatus.Deleted
                            );
                totalRecord = query.Count();
                return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var query = _profileCompanyRepository.Table.Where(p => (key == null || key == string.Empty || p.Name.ToLower().Contains(key.ToLower()) || p.Email.ToLower().Contains(key.ToLower()) || p.Phone.ToLower().Contains(key.ToLower()) || p.Street1.ToLower().Contains(key.ToLower()) || p.City.ToLower().Contains(key.ToLower()))
                            && p.CompanyStatusID != (int)Kuyam.Database.Types.CompanyStatus.Deleted
                            && p.CompanyStatusID == searchType
                            );
                totalRecord = query.Count();
                return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public List<ProfileCompany> AdminGetListCompanyByKeyName(string key, int searchType, int companyType, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            var isClass = Convert.ToBoolean(companyType);
            if (searchType == (int)Types.CompanyStatus.Unknown)
            {

                var query = _profileCompanyRepository.Table.Where(p => (key == null || key == string.Empty || p.Name.ToLower().Contains(key.ToLower()) || p.Email.ToLower().Contains(key.ToLower()) || p.Phone.ToLower().Contains(key.ToLower()) || p.Street1.ToLower().Contains(key.ToLower()) || p.City.ToLower().Contains(key.ToLower()))
                            && p.CompanyStatusID != (int)Kuyam.Database.Types.CompanyStatus.Deleted
                            && ((!isClass && (!p.IsClass.HasValue || p.IsClass.Value == isClass)) || (p.IsClass.HasValue && p.IsClass.Value == isClass))
                            );
                totalRecord = query.Count();
                return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var query = _profileCompanyRepository.Table.Where(p => (key == null || key == string.Empty || p.Name.ToLower().Contains(key.ToLower()) || p.Email.ToLower().Contains(key.ToLower()) || p.Phone.ToLower().Contains(key.ToLower()) || p.Street1.ToLower().Contains(key.ToLower()) || p.City.ToLower().Contains(key.ToLower()))
                            && p.CompanyStatusID != (int)Kuyam.Database.Types.CompanyStatus.Deleted
                            && p.CompanyStatusID == searchType
                             && ((!isClass && (!p.IsClass.HasValue || p.IsClass.Value == isClass)) || (!p.IsClass.HasValue && p.IsClass.Value == isClass))
                            );
                totalRecord = query.Count();
                return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }



        /// <summary>
        /// Get all profile company
        /// </summary>
        /// <returns>Profile company list</returns>
        public List<ProfileCompany> AdminGetListCompany()
        {
            return _profileCompanyRepository.Table.Where(p => p.CompanyStatusID == (int)Types.CompanyStatus.Active).OrderBy(c => c.Name).ToList();
        }

        /// <summary>
        /// Get all profile company
        /// </summary>
        /// <param name="key">The key for search</param>
        /// <returns>
        /// Profile company list
        /// </returns>
        public List<ProfileCompany> AdminGetListCompany(string key, int take = 0, int skip = 0)
        {
            key = key.Trim();
            //key = key.Replace(" ", "%");
            var query = _profileCompanyRepository.Table.Where(p => p.CompanyStatusID == (int)Types.CompanyStatus.Active && p.Name.Contains(key)).OrderBy(c => c.Name);
            if (take > 0 || skip > 0)
                return query.Skip(skip).Take(take).ToList();
            return query.ToList();
        }

        /// <summary>
        /// Admin: Get all featured.
        /// </summary>
        /// <returns>Featured list</returns>
        public List<FeaturedCompany> AdminGetListFeaturedCompanies()
        {
            return _featuredCompanyRepository.Table.OrderBy(f => f.priority).Take(3).ToList();
        }

        public FeaturedCompany GetFeatureCompany(int featureId)
        {
            return _featuredCompanyRepository.Table.Where(t => t.CompanyFeaturedID == featureId).FirstOrDefault();
        }

        /// <summary>
        /// Admin: Update company featured
        /// </summary>
        /// <param name="profileID">int: profileID</param>
        /// <param name="priority">int: priority</param>
        /// <returns>bool:
        ///             true: update success,
        ///             false: update fautl
        /// </returns>
        public bool AdminUpdateCompanyFeatured(int profileID, int priority, int featureType = 0)
        {

            bool result = false;

            if (!_featuredCompanyRepository.Table.Any(x => x.ProfileID == profileID && x.priority != priority))
            {

                FeaturedCompany fc = _featuredCompanyRepository.Table.Where(f => f.priority == priority).FirstOrDefault();
                fc.ProfileID = profileID;
                fc.StartDT = DateTime.Now;
                fc.FeatureType = featureType;

                _featuredCompanyRepository.Update(fc);
                result = true;
            }
            return result;
        }

        public bool AdminAddCompanyFeatured(int profileID, int priority, int featureType = 0)
        {
            bool result = false;

            FeaturedCompany fc = new FeaturedCompany();
            fc.priority = priority;
            fc.ProfileID = profileID;
            fc.StartDT = DateTime.Now;
            fc.FeatureType = featureType;
            try
            {
                _featuredCompanyRepository.Insert(fc);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool AdminDelateCompanyFeatured(int priority)
        {

            bool result = false;

            FeaturedCompany featured = _featuredCompanyRepository.Table.Where(f => f.priority == priority).FirstOrDefault();

            if (featured != null)
            {
                _featuredCompanyRepository.Delete(featured);
                result = true;
            }

            return result;
        }

        public bool CheckFeatured(int priority)
        {
            bool result = false;
            if (_featuredCompanyRepository.Table.Any(x => x.priority == priority))
            {

                result = true;
            }
            return result;
        }

        public bool CheckFeaturedProfileId(int profileId)
        {
            bool result = false;
            if (_featuredCompanyRepository.Table.Any(x => x.ProfileID == profileId))
            {
                result = true;
            }
            return result;
        }

        public bool CompayAddDayExtend(int profileId, int day)
        {
            bool result = false;
            if (_profileCompanyRepository.Table.Any(x => x.ProfileID == profileId))
            {

                ProfileCompany pc = _profileCompanyRepository.Table.FirstOrDefault(f => f.ProfileID == profileId);
                DateTime dtNextday = DateTime.UtcNow.AddDays(1);
                DateTime dtUTC = new DateTime(dtNextday.Year, dtNextday.Month, dtNextday.Day, 0, 0, 0,
                                              DateTimeKind.Utc);
                pc.Modified = DateTime.UtcNow;
                if (day >= 0)
                {
                    if (pc.ExpiredDate == null)
                    {
                        pc.ExpiredDate = dtUTC.AddDays(day);
                    }
                    else
                    {
                        if (DateTime.UtcNow.Date == pc.ExpiredDate.Value.Date)
                        {
                            dtNextday = pc.ExpiredDate.Value.AddDays(1);
                            DateTime dtExpiredDate = new DateTime(dtNextday.Year, dtNextday.Month, dtNextday.Day, 0, 0,
                                                                  0,
                                                                  DateTimeKind.Utc);
                            pc.ExpiredDate = dtExpiredDate.AddDays(day);
                        }
                        else
                        {
                            pc.ExpiredDate = pc.ExpiredDate.Value.AddDays(day);
                        }
                    }
                }
                else
                {
                    if (pc.ExpiredDate == null)
                    {
                        dtUTC = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0,
                                              DateTimeKind.Utc);
                        pc.ExpiredDate = dtUTC.AddDays(day);
                    }
                    else
                    {
                        pc.ExpiredDate = pc.ExpiredDate.Value.AddDays(day);
                    }
                }
                _profileCompanyRepository.Update(pc);
                result = true;
            }

            return result;
        }

        private bool CheckRole(string username)
        {
            string[] roleNames = Roles.GetRolesForUser(username);
            if (roleNames != null && roleNames.Contains("Personal"))
            {
                if (roleNames.Contains("Agent") || roleNames.Contains("Admin"))
                {
                    return false;
                }
                return true;
            }
            return false;
        }




        public List<Cust> AdminGetListUserByKeyName(string key, int pageIndex, int pageSize, out int totalRecord, int searchType, string role = "Personal")
        {
            totalRecord = 0;
            if (searchType == (int)Types.UserStatusType.Unknown)
            {
                var query = _custRepository.Table.Where(u => (key == null || key == string.Empty || u.FirstName.ToUpper().Contains(key.ToUpper())
                    || u.LastName.ToUpper().Contains(key.ToUpper()) || u.aspnet_Users.UserName.ToUpper().Contains(key.ToUpper())
                    || u.aspnet_Users.aspnet_Membership.Email.Contains(key.ToUpper())) && u.Status != (int)Types.UserStatusType.Deleted && u.Status != (int)Types.UserStatusType.Unknown).ToList();
                query = query.Where(m => (role == "Admin" || CheckRole(m.aspnet_Users.UserName))).ToList();
                totalRecord = query.Count();
                return query.OrderByDescending(m => m.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var query = _custRepository.Table.Where(u => (key == null || key == string.Empty || u.FirstName.ToUpper().Contains(key.ToUpper())
                    || u.LastName.ToUpper().Contains(key.ToUpper()) || u.aspnet_Users.UserName.ToUpper().Contains(key.ToUpper())
                    || u.aspnet_Users.aspnet_Membership.Email.Contains(key.ToUpper())) && u.Status == searchType && u.Status != (int)Types.UserStatusType.Deleted && u.Status != (int)Types.UserStatusType.Unknown).ToList();
                query = query.Where(m => (role == "Admin" || CheckRole(m.aspnet_Users.UserName))).ToList();
                totalRecord = query.Count();
                return query.OrderByDescending(m => m.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        private bool CheckRole(string username, string role)
        {
            string[] roleNames = Roles.GetRolesForUser(username);
            if (roleNames == null && roleNames.Count() == 0)
                return false;

            //if (role != "Admin")
            //{
            //    if (roleNames.Contains(role) && !roleNames.Contains("Admin"))
            //        return true;
            //    else
            //        return false;
            //}

            if (roleNames.Contains(role))
                return true;

            return false;
        }

        public List<Cust> AdminGetListUserByRole(string roleName, string keySearch, int pageIndex, int pageSize, out int totalRecord)
        {
            var query = _custRepository.Table.Where(u =>
                                                    (keySearch == null || keySearch == string.Empty ||
                                                     u.FirstName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.LastName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.aspnet_Users.UserName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.aspnet_Users.aspnet_Membership.Email.Contains(keySearch.ToUpper())
                                                    )
                                                    && u.Status != (int)Types.UserStatusType.Deleted
                                                    && u.Status != (int)Types.UserStatusType.Unknown
                                                    ).ToList();
            var query1 = query.Where(m => ((roleName == null || roleName.Contains("all")) || CheckRole(m.aspnet_Users.UserName, roleName))).ToList();
            totalRecord = query1.Count();
            return query1.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public List<UserStaff> AdminGetListUserByRoleAndHotelId(int custId, int hotelId, string keySearch, int pageIndex, int pageSize, out int totalRecord)
        {

            var query = from u in _custRepository.Table
                        join stf in _hotelStaffRepository.Table on u.CustID equals stf.CustID
                        join h in _hotelRepository.Table on stf.HotelID equals h.HotelID
                        where (keySearch == null || keySearch == string.Empty || keySearch == "" ||
                                                     u.FirstName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.LastName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.aspnet_Users.UserName.ToUpper().Contains(keySearch.ToUpper()) ||
                                                     u.aspnet_Users.aspnet_Membership.Email.Contains(keySearch.ToUpper())
                                                    )
                                                    && ((hotelId == 0 && h.CustID == custId) || h.HotelID == hotelId)
                                                    && u.Status != (int)Types.UserStatusType.Deleted
                                                    && u.Status != (int)Types.UserStatusType.Unknown
                        select new UserStaff
                        {
                            CustId = u.CustID,
                            AspUserID = u.AspUserID,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Status = u.Status,
                            id = stf.Id,
                            Hotel = h
                        };
            totalRecord = query.Count();
            return query.OrderByDescending(m => m.id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }


        public List<Cust> AdminGetListUserByRoleAndHotelId(int hotelId, string role = "Personal")
        {
            var query1 = _hotelStaffRepository.Table.Where(m => m.HotelID == hotelId).Select(m => m.CustID).ToList();

            var query = _custRepository.Table.Where(u =>
                     !query1.Contains(u.CustID)
                     && u.Status == (int)Types.UserStatusType.Active).ToList();

            //query = query.Where(m =>m.GetRole.Contains("")||).ToList();

            return query;

        }

        /// <summary>
        /// Change company by company id
        /// </summary>
        /// <param name="id">int: company id</param>
        /// <param name="companyType">int: company type</param>
        /// <returns>bool:
        ///             true: change success,
        ///             false: change fault
        /// </returns>
        public bool ChangeCompanyTypeByCompanyId(int companyId, int companyType)
        {
            bool result = false;
            ProfileCompany profileCompany = _profileCompanyRepository.Table.Where(c => c.ProfileID == companyId).FirstOrDefault();
            if (profileCompany != null)
            {
                profileCompany.CompanyTypeID = companyType;
                _profileCompanyRepository.Update(profileCompany);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public List<NewsletterInfoDetail> GetNewsletterData()
        {
            List<NewsletterInfoDetail> results = new List<NewsletterInfoDetail>();
            AdminService adminService = Kuyam.Repository.Infrastructure.EngineContext.Current.Resolve<AdminService>();

            List<FeaturedCompany> companyFeatured = adminService.AdminGetListFeaturedCompanies();
            foreach (FeaturedCompany featuredCompany in companyFeatured)
            {
                NewsletterInfoDetail infoDetail = new NewsletterInfoDetail();
                infoDetail.CompanyId = featuredCompany.ProfileID.ToString();
                infoDetail.CompanyDescripton = featuredCompany.Profile.ProfileCompany.Desc;
                infoDetail.CompanyName = featuredCompany.Profile.ProfileCompany.Name;
                infoDetail.FeatureCompanyType = (Types.FeatureCompanyType)featuredCompany.FeatureType;

                Medium mediaImage = DAL.GetCompanyPhotoByCompanyID(featuredCompany.ProfileID);
                if (mediaImage != null)
                {
                    infoDetail.CompanyImage = Types.KaturaDoman + "/p/811441/thumbnail/entry_id/" +
                                              mediaImage.LocationData + "/width/109/height/107";
                }

                results.Add(infoDetail);
            }

            return results;
        }

        #region Category Service

        public List<Service> AdminGetListServiceAll()
        {
            return _serviceRepository.Table.Where(m => m.ParentServiceID.HasValue && (m.Status == null || m.Status == true)).ToList();
        }

        public List<Service> AdminGetListServiceByName(string key, int pageIndex, int pageSize, out int totalRecord, int type)
        {
            totalRecord = 0;
            if (type == 0)
            {
                var query =
                    _serviceRepository.Table.Where(
                        u => (key == null || key == string.Empty || u.ServiceName.ToUpper().Contains(key.ToUpper())));
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else if (type == 1)
            {
                var query =
                    _serviceRepository.Table.Where(
                        u =>
                        ((key == null || key == string.Empty ||
                         u.ServiceName.ToUpper().Contains(key.ToUpper())) && !u.ParentServiceID.HasValue));
                //ToList();
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else if (type == 3)
            {
                var query =
                    _serviceRepository.Table.Where(
                        u =>
                        ((key == null || key == string.Empty ||
                         u.ServiceName.ToUpper().Contains(key.ToUpper())) && u.Sequence.HasValue));
                //ToList();
                totalRecord = query.Count();
                return query.OrderBy(u => u.Sequence).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var query =
                    _serviceRepository.Table.Where(
                        u =>
                        ((key == null || key == string.Empty || u.ServiceName.ToUpper().Contains(key.ToUpper())) &&
                         u.ParentServiceID != null));
                //ToList();
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public void CreateService(Service service)
        {
            string errorMessage;
            if (service.ParentServiceID.HasValue)
            {
                if (!ValidateAddingService(service, out errorMessage))
                    throw new InvalidDataException(errorMessage);
            }
            else
            {
                if (!ValidateAddingCategory(service, out errorMessage))
                    throw new InvalidDataException(errorMessage);
            }
            _serviceRepository.Insert(service);
        }

        public bool ValidateAddingCategory(Service service, out string errorMessage)
        {
            errorMessage = "";

            if (_serviceRepository.Table.Any(n => n.ServiceName == service.ServiceName && !n.ParentServiceID.HasValue))
            {
                errorMessage = "this category name has been used, please enter another name.";
                return false;
            }

            return true;
        }

        public bool ValidateAddingService(Service service, out string errorMessage)
        {
            errorMessage = "";

            if (_serviceRepository.Table.Any(n => n.ServiceName == service.ServiceName &&
                                                  n.ParentServiceID.HasValue &&
                                                  n.ParentServiceID == service.ParentServiceID))
            {
                errorMessage = "this service name has been used, please enter another name.";
                return false;
            }

            return true;
        }

        public void ModifyService(Service service)
        {
            //_serviceRepository.Attach(service);
            var originalService = _serviceRepository.Table.Single(n => n.ServiceID == service.ServiceID);
            //if (_serviceRepository.Table.Any(n => n.ServiceName == service.ServiceName && n.ServiceID != service.ServiceID))
            //    throw new InvalidDataException("this name has already been used, please enter another name.");

            //string errorMessage;
            //if (service.ParentServiceID.HasValue)
            //{
            //    if (!ValidateModifyingService(service, out errorMessage))
            //        throw new InvalidDataException(errorMessage);
            //}
            //else
            //{
            //    if (!ValidateModifyingCategory(service, originalService, out errorMessage))
            //        throw new InvalidDataException(errorMessage);
            //}

            //if (!originalService.ParentServiceID.HasValue && service.ParentServiceID.HasValue)
            //{
            //    if (originalService.Service1.Any())
            //        throw new InvalidDataException("this category already has service(s), you could not change it to service.");
            //}
            originalService.ServiceName = service.ServiceName;
            originalService.Desc = service.Desc;
            originalService.ParentServiceID = service.ParentServiceID;
            originalService.Status = service.Status;
            originalService.Sequence = service.Sequence;
            if (service.kalturaId != null)
                originalService.kalturaId = service.kalturaId;

            _serviceRepository.Update(originalService);
        }

        public bool ValidateModifyingCategory(Service service, Service oldService, out string errorMessage)
        {
            errorMessage = "";

            if (_serviceRepository.Table.Any(n => n.ServiceName == service.ServiceName &&
                                                  !n.ParentServiceID.HasValue &&
                                                  n.ServiceID != service.ServiceID))
            {
                errorMessage = "this category name has been used, please enter another name.";
                return false;
            }

            if (oldService.ServiceCompanies.Any() && service.Status.HasValue && service.Status.Value == false)
            {
                errorMessage = "this " + (service.ParentServiceID.HasValue ? "service" : "category") + " has already been used by company service, you can't change the status to inactive.";
                return false;
            }
            return true;
        }

        public bool ValidateModifyingService(Service service, out string errorMessage)
        {
            errorMessage = "";

            if (_serviceRepository.Table.Any(n => n.ServiceName == service.ServiceName &&
                                                  n.ParentServiceID.HasValue &&
                                                  n.ParentServiceID == service.ParentServiceID &&
                                                  n.ServiceID != service.ServiceID))
            {
                errorMessage = "this service name has been used, please enter another name.";
                return false;
            }

            if (service.ServiceCompanies.Any() && service.Status.HasValue && service.Status.Value == false)
            {
                errorMessage = "this " + (service.ParentServiceID.HasValue ? "service" : "category") + " has already been used by company service, you can't change the status to inactive.";
                return false;
            }
            return true;
        }

        public bool CanDeleteService(Service service, out string message)
        {
            message = "";
            if (!service.ParentServiceID.HasValue && service.Service1.Any())
            {
                message = "you should delete all services belong to this category first.";
                return false;
            }

            if (service.ServiceCompanies.Any())
            {
                message = "this " + (service.ParentServiceID.HasValue ? "service" : "category") + " has already been used by company service.";
                return false;
            }

            return true;
        }

        public void DeleteService(int serviceId)
        {
            Service service = _serviceRepository.Table.Single(n => n.ServiceID == serviceId);
            string message = "";
            if (!CanDeleteService(service, out message))
                throw new InvalidDataException(message);
            _serviceRepository.Delete(service);
        }

        public Service[] ListServicesByCategory(int categoryId)
        {
            return _serviceRepository.Table
                .Single(n => n.ServiceID == categoryId && !n.ParentServiceID.HasValue).Service1
                .OrderBy(n => n.ServiceName)
                .Select(n => new Service()
                                {
                                    ServiceID = n.ServiceID,
                                    ServiceName = n.ServiceName,
                                    Desc = n.Desc,
                                    Status = (n.Status == null) ? true : n.Status,
                                    ParentServiceID = n.ParentServiceID
                                }).ToArray();
        }

        #endregion

        #region Pending Users
        /// <summary>
        /// Get all invite code 
        /// </summary>
        /// <param name="key">string: key for search</param>
        /// <param name="pageIndex">int: cerrent page</param>
        /// <param name="pageSize">int: number of page</param>
        /// <param name="totalRecord">int out: total record</param>
        /// <returns></returns>
        public List<Invite> GetAllPendingUsers(string key, int searchType, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            IQueryable<Invite> query;
            if (searchType == -1)
            {
                query = _inviteRepository.Table.Where(u => (key == null || key == string.Empty || u.Email.Contains(key) || u.LName.Contains(key) || u.Name.Contains(key))
                                                        && u.Active == false && u.InviteType == (int)Types.InviteType.User
                                                        && (u.Status == (int)Types.UserInviteCodeStatusType.Approved
                                                        || u.Status == (int)Types.UserInviteCodeStatusType.Pending
                                                        || u.Status == (int)Types.UserInviteCodeStatusType.Denied));
            }
            else
            {
                query = _inviteRepository.Table.Where(u => (key == null || key == string.Empty || u.Email.Contains(key) || u.LName.Contains(key) || u.Name.Contains(key))
                                                        && u.Active == false && u.InviteType == (int)Types.InviteType.User
                                                        && u.Status == searchType);
            }
            totalRecord = query.Count();
            return query.OrderByDescending(c => c.CreateDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public Invite GetInviteCodeById(int id)
        {
            return _inviteRepository.Table.Where(i => i.InviteID == id && i.Active == false &&
                    i.InviteType == (int)Types.InviteType.User)
                    .FirstOrDefault();
        }

        public void UpdateInviteStatus(int id, int status)
        {
            Invite i = GetInviteCodeById(id);

            if (i != null)
            {
                try
                {
                    i.Status = status;
                    i.CreateDate = DateTime.UtcNow;
                    DAL.UpdateRec(i, i.InviteID);
                    LogHelper.Info(string.Format("Update invite status:InviteID= {0}", i.InviteID));
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Update invite status fail:", ex);
                }

            }
        }

        public Cust GetUserById(int id)
        {
            return _custRepository.Table.Where(c => c.CustID == id).FirstOrDefault();
        }

        public bool ChangeUserStatus(int id, int status)
        {
            bool result = false;
            Cust cust = _custRepository.Table.Where(c => c.CustID == id).FirstOrDefault();
            if (cust != null)
            {
                try
                {
                    cust.Status = status;
                    _custRepository.Update(cust);
                    LogHelper.Info(string.Format("Change user status:CustID= {0}", cust.CustID));
                    result = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Change user status fail:", ex);
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public List<LockedUser> GetAllLockedUsers(string key, int pageIndex, int pageSize, out int totalRecord)
        {
            if (key == null) key = "";
            totalRecord = 0;
            //List<Invite> lockedUsers = new List<Invite>();
            var query =
                    from user in _userRepository.Table
                    join cust in _custRepository.Table on user.UserId equals cust.AspUserID
                    join mem in _membershipRepository.Table on user.UserId equals mem.UserId
                    where mem.IsLockedOut == true &&
                    (
                       (cust.FirstName.Contains(key) || cust.FirstName == "" || cust.FirstName == null) ||
                       (cust.LastName.Contains(key) || cust.LastName == "" || cust.LastName == null) ||
                       (mem.Email.Contains(key) || mem.Email == "" || mem.Email == null)
                    )

                    select new LockedUser
                    {
                        UserId = user.UserId,
                        FirstName = cust.FirstName,
                        LastName = cust.LastName,
                        Email = mem.Email,
                        LockedStatus = "Locked"
                    };

            //join apt in _appointmentRepository.Table on ord.AppointmentID equals apt.AppointmentID

            //_inviteRepository.Table.Where(u => (key == null || key == string.Empty || u.Email.Contains(key) || u.LName.Contains(key) || u.Name.Contains(key))
            //                                        && u.Active == false && u.InviteType == (int)Types.InviteType.User
            //                                        && u.Status == searchType);

            totalRecord = query.Count();

            return query.OrderBy(c => c.FirstName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public aspnet_Membership UnLockUserById(Guid id)
        {
            aspnet_Membership membership = _membershipRepository.Table.Where(m => m.UserId == id).FirstOrDefault();
            if (membership != null)
            {
                try
                {
                    membership.IsLockedOut = false;
                    membership.FailedPasswordAttemptCount = 0;
                    _membershipRepository.Update(membership);
                    LogHelper.Info(string.Format("UnLock user: UserId={0}", membership.UserId));
                }
                catch (Exception ex)
                {
                    LogHelper.Error("UnLock user fail:", ex);
                }

            }
            return membership;
        }

        public List<ZipCode> GetAllZipCodes(string key, int pageIndex, int pageSize, out int totalRecord)
        {
            if (key == null) key = "";
            totalRecord = 0;
            var query =
                    from zipCode in _zipCodeRepository.Table
                    where
                    (
                        (key == null || key == string.Empty || (zipCode.Code != string.Empty && zipCode.Code.Contains(key)) || zipCode.Code == null) ||
                        (key == null || key == string.Empty || (zipCode.City != string.Empty && zipCode.City.Contains(key)) || zipCode.City == null) ||
                        (key == null || key == string.Empty || (zipCode.County != string.Empty && zipCode.County.Contains(key)) || zipCode.County == null)
                    )
                    select zipCode;

            totalRecord = query.Count();

            return query.OrderBy(c => c.City).ThenBy(c => c.County).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public ZipCode GetZipCodeById(int id)
        {
            return _zipCodeRepository.Table.Where(c => c.ZipCodeId == id).FirstOrDefault();
        }

        public List<State> GetAllState()
        {
            return _stateRepository.Table.ToList();
        }

        public bool DeleteZipCodeById(int id)
        {
            bool suceeded = false;
            ZipCode zipCode = _zipCodeRepository.Table.Where(c => c.ZipCodeId == id).FirstOrDefault();
            try
            {
                _zipCodeRepository.Delete(zipCode);
                LogHelper.Info(string.Format("Delete zip code: ZipCodeId= {0}", zipCode.ZipCodeId));
                suceeded = true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Delete zip code fail: ", ex);
                suceeded = false;
            }

            return suceeded;
        }

        public bool SaveZipCode(ZipCode zipCodeUpdate, bool isAddMode, int oldZipCodeId)
        {
            bool suceeded = false;
            ZipCode zipCode = null;
            if (!isAddMode) // Edit
            {
                try
                {
                    zipCode = _zipCodeRepository.Table.Where(c => c.ZipCodeId == oldZipCodeId).FirstOrDefault();
                    zipCode.Code = zipCodeUpdate.Code;
                    zipCode.City = zipCodeUpdate.City == null ? "" : zipCodeUpdate.City;
                    zipCode.County = zipCodeUpdate.County == null ? "" : zipCodeUpdate.County;
                    zipCode.Active = zipCodeUpdate.Active;
                    zipCode.State = zipCodeUpdate.State;
                    _zipCodeRepository.Update(zipCode);
                    string message = string.Format("Update zip code: ZipCodeId= {0}", zipCode.ZipCodeId);
                    LogHelper.Info(message);
                    suceeded = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Save zip code fail: ", ex);
                    suceeded = false;
                }
            }
            else // Add new 
            {
                try
                {
                    zipCodeUpdate.Active = true;
                    _zipCodeRepository.Insert(zipCodeUpdate);
                    suceeded = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Add zip code fail:", ex);
                    suceeded = false;
                }
            }

            return suceeded;
        }


        public bool ChangeZipcodeStatus(int id, bool status)
        {
            bool result = false;
            ZipCode zipCode = _zipCodeRepository.Table.Where(c => c.ZipCodeId == id).FirstOrDefault();
            if (zipCode != null)
            {
                try
                {
                    zipCode.Active = status;
                    _zipCodeRepository.Update(zipCode);

                    LogHelper.Info(string.Format("Change zip code status: Active= {0}", zipCode.Active));
                    result = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Change zip code status fail:", ex);
                }

            }
            else
            {
                result = false;
            }
            return result;
        }

        public List<ZipCode> SearchZipCodes(string key, int? searchType, int pageIndex, int pageSize, out int totalRecord)
        {
            List<ZipCode> query = null;
            searchType = searchType == null ? -1 : searchType.Value;
            bool searchCriteria = false;
            if (key == null) key = "";
            if (searchType == 1) searchCriteria = true;
            totalRecord = 0;

            //Trong edit
            query =
                  _zipCodeRepository.Table.Where(u =>
                      (key == null || key == string.Empty ||
                      u.Code.ToLower().Contains(key.ToLower()) ||
                      u.City.ToLower().Contains(key.ToLower()) ||
                      u.State.ToLower().Contains(key.ToLower()) ||
                      u.County.ToLower().Contains(key.ToLower()))
                      && ((searchType == -1 && (u.Active == true || u.Active == false)) || u.Active == searchCriteria)
                  ).ToList();

            //if (searchType == -1) //Select all
            //{
            //    query =
            //           _zipCodeRepository.Table.Where( u =>                       
            //           (
            //              (key == "" ||  (u.Code != "" && u.Code.Contains(key)) || u.Code == null) ||
            //              (key == "" || (u.City != "" && u.City.Contains(key)) || u.City == null) ||
            //              (key == "" || (u.County != "" && u.County.Contains(key)) || u.County == null)
            //           )).ToList();

            //}
            //else
            //{
            //    query =
            //         _zipCodeRepository.Table.Where( u =>                       
            //           (
            //              (key == "" || (u.Code != "" && u.Code.Contains(key)) || u.Code == null) ||
            //              (key == "" || (u.City != "" && u.City.Contains(key)) || u.City == null) ||
            //              (key == "" || (u.County != "" && u.County.Contains(key)) || u.County == null)
            //           ) && u.Active == searchCriteria).ToList();                   
            //}

            totalRecord = query.Count();

            return query.OrderBy(c => c.City).ThenBy(c => c.County).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public bool IsZipCodeExisted(ZipCode zipCodeUpdate)
        {
            ZipCode zipCode = _zipCodeRepository.Table.Where(c => c.Code == zipCodeUpdate.Code && c.ZipCodeId != zipCodeUpdate.ZipCodeId).FirstOrDefault();
            return zipCode != null;
        }
        #endregion

        #region admin invite

        public List<Invite> AdminGetListInvites(string key, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            var query =
                _inviteRepository.Table.Where(n => (key == null || key == string.Empty || n.Email.ToUpper().Contains(key.ToUpper())) && n.InviteType == 2);

            totalRecord = query.Count();
            return query.OrderByDescending(a => a.CreateDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            /*if (type == 0)
            {
                var query =
                    _serviceRepository.Table.Where(
                        u => (key == null || key == string.Empty || u.ServiceName.ToUpper().Contains(key.ToUpper())));
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else if (type == 1)
            {
                var query =
                    _serviceRepository.Table.Where(
                        u =>
                        ((key == null || key == string.Empty ||
                         u.ServiceName.ToUpper().Contains(key.ToUpper())) && !u.ParentServiceID.HasValue));
                //ToList();
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var query =
                    _serviceRepository.Table.Where(
                        u =>
                        ((key == null || key == string.Empty || u.ServiceName.ToUpper().Contains(key.ToUpper())) &&
                         u.ParentServiceID != null));
                //ToList();
                totalRecord = query.Count();
                return query.OrderBy(u => u.ServiceName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }*/
        }

        #endregion

        #region Appointments

        public Appointment GetAppointmentById(int appointmentId)
        {
            return _appointmentRepository.Table.Where(m => m.AppointmentID == appointmentId).FirstOrDefault();
        }

        public List<Appointment> GetAppointments(string key, int type, int hotelId, int status, int pageIndex, int pageSize, out int totalRecord, int isAgent = (int)Types.Role.Unknown)
        {

            totalRecord = 0;
            var query =
                _appointmentRepository.Table.Where(a => (key == null || key == string.Empty ||
                                                a.Cust.FirstName.ToUpper().Contains(key.ToUpper()) || a.Cust.LastName.ToUpper().Contains(key.ToUpper()) ||
                                                a.ServiceCompany.ProfileCompany.Name.ToUpper().Contains(key.ToUpper()))
                                                && (status == (int)Types.AppointmentStatus.Unknown || (status == (int)Types.AppointmentStatus.Modified && (a.AppointmentStatusID == (int)Types.AppointmentStatus.Modified || a.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified)) || a.AppointmentStatusID == status)
                                                && (type == (int)Types.CompanyType.Unknown || a.ServiceCompany.ProfileCompany.CompanyTypeID == type)

                                                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                                                && a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending);
            if (isAgent == (int)Types.Role.Admin)
            {
                query = query.Where(a => (hotelId == 0 || a.HotelID == hotelId));
            }
            else
            {
                query = query.Where(a => hotelId == null);
            }


            totalRecord = query.Count();

            return query.OrderByDescending(a => a.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public List<Rating> GetAppointmentFeedback(string key, int companyId, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            var query = from rat in _ratingRepository.Table
                        join svc in _serviceCompanyRepository.Table on rat.ServiceCompanyID equals svc.ServiceCompanyID
                        join pro in _profileCompanyRepository.Table on svc.ProfileID equals pro.ProfileID
                        where (companyId == 0 || pro.ProfileID == companyId)
                               && (key == null || key == string.Empty || svc.Service.ServiceName == key)
                        select rat;
            totalRecord = query.Count();
            return query.OrderByDescending(a => a.CreateDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<NonKuyamAppointment> GetlistNoAppointment(int profileId, int pageIndex, int pageSize, out int totalRecord)
        {
            var query = _nonKuyamAppointmentRepository.Table.Where(m => (profileId == 0 || m.ProfileId == profileId) && m.AppointmentStatusID == 0);
            totalRecord = query.Count();
            return query.OrderByDescending(a => a.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public NonKuyamAppointment GetNoAppointment(int appointmentId)
        {
            var query = _nonKuyamAppointmentRepository.Table.Where(m => m.AppointmentID == appointmentId);
            return query.FirstOrDefault();
        }

        public void InsertAppointment(NonKuyamAppointment entity)
        {
            _nonKuyamAppointmentRepository.Insert(entity);
        }

        public void UpdateAppointment(NonKuyamAppointment entity)
        {
            _nonKuyamAppointmentRepository.Update(entity);
        }

        #endregion


        #region Hotel

        public List<aspnet_Roles> GetAllRoles()
        {
            return _rolesRepository.Table.Where(m => m.RoleName != "Personal").ToList();
        }

        private bool CheckHotelAdminRole(string username)
        {
            string[] roleNames = Roles.GetRolesForUser(username);
            if (roleNames != null && roleNames.Contains("HotelAdmin"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///  get all user by role AdminHotel
        /// </summary>
        /// <param name="role">role</param>
        /// <returns>list user with role admin hotel </returns>
        public List<Cust> AdminGetListUserByRole(string role = "HotelAdmin")
        {
            var query = _custRepository.Table.Where(u => u.Status != (int)Types.UserStatusType.Deleted && u.Status != (int)Types.UserStatusType.Unknown).ToList();
            query = query.Where(m => (role == "Admin" || role == "Agent" || m.aspnet_Users.aspnet_Roles.Any(n => n.RoleName == role))).ToList();
            return query;
        }

        public List<CustExtension> GetAllUser()
        {
            var query = from c in _custRepository.Table
                        join mps in _membershipRepository.Table on c.AspUserID equals mps.UserId
                        where c.Status == (int)Types.UserStatusType.Active
                        orderby mps.Email
                        select new CustExtension
                        {
                            CustID = c.CustID,
                            AspUserID = c.AspUserID,
                            Email = !string.IsNullOrEmpty(c.FacebookUserID) ? c.FacebookUserID : mps.Email,
                            Username = c.FacebookUserID,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            City = c.City,
                            Street1 = c.Street1,
                            Street2 = c.Street2,
                            State = c.State,
                            Status = c.Status,
                            MobilePhone = c.MobilePhone,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude,
                            TimeZoneId = c.TimeZoneId
                        };

            return query.ToList();
        }

        public List<Hotel> GetListHotelByAll()
        {
            return _hotelRepository.Table.ToList();
        }
        public List<Hotel> AdminGetListHotelByKey(string keyName, int pageIndex, int pageSize, out int totalRecord, int custId = 0)
        {
            var query = _hotelRepository.Table.Where(m => (string.IsNullOrEmpty(keyName) || m.Name.Contains(keyName)) && (custId == 0 || m.CustID == custId));
            totalRecord = query.Count();
            return query.OrderBy(u => u.HotelID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<Hotel> GetListHotelOfAdminByCustId(string keyName, int pageIndex, int pageSize, out int totalRecord, int custId)
        {
            var query = from h in _hotelRepository.Table
                        where h.CustID == custId && (string.IsNullOrEmpty(keyName) || h.Name.Contains(keyName))
                        select h;
            totalRecord = query.Count();
            return query.OrderBy(u => u.HotelID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<Hotel> GetListHotelOfStaffByCustId(int custId)
        {
            var query = from stf in _hotelStaffRepository.Table
                        join h in _hotelRepository.Table on stf.HotelID equals h.HotelID
                        where stf.CustID == custId
                        select h;
            return query.ToList();
        }

        public Hotel GetHotelByName(string name)
        {
            return _hotelRepository.Table.FirstOrDefault(m => m.Name.ToLower() == name.ToLower());
        }


        public Hotel GetHotelById(int id)
        {
            return _hotelRepository.Table.FirstOrDefault(m => m.HotelID == id);
        }

        public List<Hotel> GetAllHotel()
        {
            return _hotelRepository.Table.ToList();
        }

        public void InsertHotel(Hotel hotel)
        {
            _hotelRepository.Insert(hotel);
        }

        public void UpdateHotel(Hotel hotel)
        {
            _hotelRepository.Update(hotel);
        }


        public List<HotelCode> GetHotelCodeByHotelId(int id, string codeNumber, int pageIndex, int pageSize, out int totalRecord, int custId = 0)
        {
            //var query = _hotelCodeRepository.Table.Where(m => ((id == 0 && m.Hotel.HotelStaffs.Any(n => n.CustID == custId)) || m.HotelID == id) && (codeNumber == null || codeNumber == "" || m.CodeNumber.Contains(codeNumber)));
            var query = from htc in _hotelCodeRepository.Table
                        join htff in _hotelStaffRepository.Table on htc.HotelID equals htff.HotelID
                        where htff.CustID == custId
                         && (codeNumber == null || codeNumber == "" || htc.CodeNumber.Contains(codeNumber))
                         && (id == 0 || htc.HotelID == id)
                        select htc;

            totalRecord = query.Count();
            return query.OrderByDescending(u => u.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public HotelCode GetHotelCodeById(int id)
        {
            return _hotelCodeRepository.Table.Where(m => m.Id == id).FirstOrDefault();
        }

        public void InsertHotelCode(HotelCode code)
        {
            _hotelCodeRepository.Insert(code);
        }

        public void DeleteHotelCode(HotelCode code)
        {
            _hotelCodeRepository.Delete(code);
        }

        public HotelStaff GetHotelStaffById(int id)
        {
            return _hotelStaffRepository.Table.Where(m => m.Id == id).FirstOrDefault();
        }

        public HotelStaff GetHotelStaffByCustIdAndHotelId(int custId, int hotelId)
        {
            return _hotelStaffRepository.Table.Where(m => m.CustID == custId && m.HotelID == hotelId).FirstOrDefault();
        }

        public List<HotelStaff> GetConciergeByHotelId(int hotelId)
        {
            var query = from c in _custRepository.Table
                        join cie in _hotelStaffRepository.Table on c.CustID equals cie.CustID
                        where cie.HotelID == hotelId
                        select cie;
            return query.ToList();
        }


        public void InsertHotelStaff(HotelStaff staff)
        {
            _hotelStaffRepository.Insert(staff);
        }

        public void DeleteHotelStaff(HotelStaff staff)
        {
            _hotelStaffRepository.Delete(staff);
        }


        /// <summary>
        /// Admin: Get all featured.
        /// </summary>
        /// <returns>Featured list</returns>
        public List<FeaturedHotel> GetFeaturedHotelByHotelId(int hotelId)
        {
            return _featuredHotelRepository.Table.Where(m => m.HotelID == hotelId).OrderBy(m => m.Priority).ToList();
        }

        public List<FeaturedHotel> GetFeaturedHotelByProfileIdAndHotelId(int hotelId)
        {
            return _featuredHotelRepository.Table.Where(m => m.HotelID == hotelId).ToList();
        }

        public FeaturedHotel CheckFeaturedHotel(int hotelId, int prfileId)
        {

            return _featuredHotelRepository.Table.Where(x => x.HotelID == hotelId && x.ProfileID == prfileId).FirstOrDefault();

        }

        public bool AddFeatureHotel(FeaturedHotel model)
        {
            bool result = false;
            try
            {

                _featuredHotelRepository.Insert(model);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool UpdateFeatureHotel(FeaturedHotel model)
        {
            bool result = false;
            try
            {
                _featuredHotelRepository.Update(model);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool DeleteFeatureHotel(int featureId)
        {
            bool result = false;
            try
            {
                var feature = _featuredHotelRepository.Table.Where(m => m.FeaturedID == featureId).FirstOrDefault();
                _featuredHotelRepository.Delete(feature);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Gets list company profiles.
        /// </summary>
        /// <param name="companyProfiles">List company profiles id.</param>
        /// <returns></returns>
        public IQueryable<ProfileCompany> GetListCompanyProfiles(List<int> companyProfiles)
        {
            return _profileCompanyRepository.Table.Where(p => companyProfiles.Contains(p.ProfileID));
        }


        /// <summary>
        /// Gets the list customer profiles.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public IQueryable<Cust> GetListCustomerProfiles(List<int> custId)
        {
            return _custRepository.Table.Where(c => custId.Contains(c.CustID));
        }

        public bool AddPhoneNumberforUser(int id, string number)
        {
            bool result = false;
            Cust cust = _custRepository.Table.Where(c => c.CustID == id).FirstOrDefault();
            if (cust != null)
            {
                try
                {
                    cust.MobilePhone = number;
                    _custRepository.Update(cust);
                    LogHelper.Info(string.Format("Add phone number:CustID= {0}", cust.CustID));
                    result = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Add number fail:", ex);
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        public bool CheckPhoneNumberExisted(string number)
        {
            bool result = false;
            Cust cust = _custRepository.Table.Where(c => c.MobilePhone == number).FirstOrDefault();
            if (cust != null)
            {
                try
                {

                    LogHelper.Info(string.Format("phone number existed:CustID= {0}", cust.CustID));
                    result = false;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("check number fail:", ex);
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public List<Cust> GetCustomerExport()
        {
            var query = _custRepository.Table;
            if (query == null)
                return null;
            return query.ToList();
        }

        public List<ProfileCompanyTemp> GetListProfileCompany(int pageIndex, int pageSize, out int totalRecord, int status = -1)
        {
            var query = _profileCompanyTempRepository.Table;
            query = query.Where(a => (status == -1 || a.Status == status));
            totalRecord = query.Count();
            return query.OrderByDescending(m => m.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(); ;
        }

        public bool DeleteProfileCompanyTemp()
        {
            try
            {
                var sql = "TRUNCATE TABLE ProfileCompanyTemp";
                _dbContext.ExecuteSqlCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public bool InsertProfileCompanyTemp(List<ProfileCompanyTemp> temps, int custId)
        {
            var sql = string.Empty;
            var sqlInsert = "INSERT INTO ProfileCompanyTemp " +
                            "VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}" +
                            ",{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}" +
                            ",{27}, {28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40})";
            var exeSQL = string.Empty;
            foreach (var temp in temps)
            {
                var companyName = GetStrValid(temp.Name);
                var contactName = GetStrValid(temp.ContactName);
                var street1 = GetStrValid(temp.Street1);
                var street2 = GetStrValid(temp.Street2);
                var city = GetStrValid(temp.City);
                var state = GetStrValid(temp.State);
                var zip = GetStrValid(temp.Zip);
                var phone = GetStrValid(temp.Phone);
                var email = GetStrValid(temp.Email);
                var url = GetStrValid(temp.Url);

                var iSql = string.Format(sqlInsert,
                    companyName,
                    contactName,
                    street1,
                    street2,
                    city,
                    state,
                    zip,
                    phone,
                    email,
                    url,
                    temp.Latitude.HasValue ? temp.Latitude : -1,
                    temp.Longitude.HasValue ? temp.Longitude : -1,
                    temp.PaymentMethod.HasValue ? temp.PaymentMethod : -1,
                    !string.IsNullOrEmpty(temp.HoursMon) ? "'" + temp.HoursMon + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursTue) ? "'" + temp.HoursTue + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursWed) ? "'" + temp.HoursWed + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursThur) ? "'" + temp.HoursThur + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursFri) ? "'" + temp.HoursFri + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursSat) ? "'" + temp.HoursSat + "'" : "''",
                    !string.IsNullOrEmpty(temp.HoursSun) ? "'" + temp.HoursSun + "'" : "''",
                    temp.Status,
                    !string.IsNullOrEmpty(temp.ErrorMessage) ? "'" + temp.ErrorMessage.Replace("'", "''") + "'" : "''",
                    "'" + DateTime.Now + "'",
                    GetStrValid(temp.CategoryName),
                    temp.CategoryId,
                    GetStrHourValid(temp.MonFrom),
                    GetStrHourValid(temp.MonTo),
                    GetStrHourValid(temp.TusFrom),
                    GetStrHourValid(temp.TusTo),
                    GetStrHourValid(temp.WedFrom),
                    GetStrHourValid(temp.WedTo),
                    GetStrHourValid(temp.ThurFrom),
                    GetStrHourValid(temp.ThurTo),
                    GetStrHourValid(temp.FriFrom),
                    GetStrHourValid(temp.FriTo),
                    GetStrHourValid(temp.SatFrom),
                    GetStrHourValid(temp.SatTo),
                    GetStrHourValid(temp.SunFrom),
                    GetStrHourValid(temp.SunTo),
                    (temp.IsDaily.HasValue && temp.IsDaily.Value) ? 1 : 0,
                    custId);
                var strSQL = iSql + "\n";
                exeSQL += strSQL.Replace(@"\n", Environment.NewLine);
            }
            try
            {
                _dbContext.ExecuteSqlCommand(exeSQL);

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
            return false;
        }


        public bool IsHaveTempDate()
        {
            return _profileCompanyTempRepository.Table.Any();
        }

        public string GetStrValid(string name)
        {
            return !string.IsNullOrEmpty(name) ? "'" + name.Replace("'", "''") + "'" : "''";
        }

        public string GetStrHourValid(string hour)
        {
            return !string.IsNullOrEmpty(hour) ? "'" + hour + "'" : "''";
        }

        public void ImportData()
        {
            try
            {
                var count = _dbContext.ExecuteSqlCommand("EXEC ImportCompany");
            }
            catch (Exception)
            {
            }
        }

        public List<Appointment> GetAppointmentsConcierge(string key, int type, int hotelId, int status, int pageIndex,
            int pageSize, out int totalRecord, int isAgent = (int)Types.Role.Unknown)
        {
            var hotleQuery = _hotelVisitRepository.Table.Where(m => m.RoomNumber.Contains(key.ToUpper())).Select(n => n.HotelID);
            totalRecord = 0;
            var query =
                _appointmentRepository.Table.Where(a => (key == null || key == string.Empty ||
                                                a.Cust.FirstName.ToUpper().Contains(key.ToUpper()) || a.Cust.LastName.ToUpper().Contains(key.ToUpper()) ||
                                                a.ServiceCompany.ProfileCompany.Name.ToUpper().Contains(key.ToUpper())
                                                || hotleQuery.Contains(a.HotelID.Value))
                                                && (status == (int)Types.AppointmentStatus.Unknown ||
                                                (status == (int)Types.AppointmentStatus.Modified
                                                && (a.AppointmentStatusID == (int)Types.AppointmentStatus.Modified
                                                || a.AppointmentStatusID == (int)Types.AppointmentStatus.CompanyModified))
                                                || a.AppointmentStatusID == status)
                                                && a.AppointmentStatusID != (int)Types.AppointmentStatus.Delete
                                                && a.AppointmentStatusID != (int)Types.AppointmentStatus.TemporaryPending);
            if (isAgent == (int)Types.Role.Agent)
            {
                query = query.Where(a => a.HotelID == null);
            }
            else
            {
                query = query.Where(a => hotelId == 0 || a.HotelID == hotelId);
            }
            if (type == 1)
            {
                query = query.Where(a => a.Start >= DateTime.Now);
            }
            else
            {
                if (type == 2)
                {
                    query = query.Where(a => a.Start <= DateTime.Now);
                }
            }

            totalRecord = query.Count();

            return query.OrderBy(a => a.Start).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }

        public Hotel GetDefaultHotelOfStaffByCustId(int custId)
        {
            var query = from stf in _hotelStaffRepository.Table
                        join h in _hotelRepository.Table on stf.HotelID equals h.HotelID
                        where stf.CustID == custId && stf != null && stf.IsDefault.Value
                        select h;
            return query.FirstOrDefault();
        }

        public void RemoveDefaultHotelStaffByCustId(int custId)
        {
            if (_hotelStaffRepository.Table.Any(m => m.CustID == custId
                && m.IsDefault.HasValue ? m.IsDefault.Value : false))
            {
                HotelStaff staff = _hotelStaffRepository.Table.Where(m => m.CustID == custId && m.IsDefault.Value).FirstOrDefault();
                DeleteHotelStaff(staff);
            }
        }

        public bool IsCompanyEventAvailable(int profileId)
        {
            var end = DateTime.Now.AddDays(-1);
            return _companyEventRepository.Table.
                Any(x => x.ProfileCompanyID == profileId
                    && x.Event.StartDate <= DateTime.UtcNow && x.Event.EndDate >= end);

        }

        #region Event
        public void CreateEvent(Event eventDetail)
        {
            eventDetail.StartDate = eventDetail.StartDate.Value;
            eventDetail.EndDate = eventDetail.EndDate.Value;
            eventDetail.Created = DateTime.UtcNow;
            eventDetail.Modified = DateTime.UtcNow;
            _eventRepository.Insert(eventDetail);
        }

        private DateTime ConvertToPacificTimeZone(DateTime time)
        {
            return TimeZoneInfo.ConvertTime(time, DateTimeUltility.DefaultStoreTimeZone);
        }



        public List<Event> AdminGetListEventByKeyName(string key, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            var query = _eventRepository.Table.Where(p => (key == null || key == string.Empty || p.Name.ToLower().Contains(key.ToLower())));
            totalRecord = query.Count();
            return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public void ModifyEvent(Event eventItem)
        {

            var originalEvent = _eventRepository.Table.Single(n => n.EventID == eventItem.EventID);

            originalEvent.Name = eventItem.Name;
            originalEvent.Description = eventItem.Description;
            originalEvent.StartDate = eventItem.StartDate.Value;
            originalEvent.EndDate = eventItem.EndDate.Value;
            originalEvent.Modified = DateTime.UtcNow;

            _eventRepository.Update(originalEvent);
        }

        public List<int> GetListCompanyIdsByEventId(int eventId)
        {
            if (_companyEventRepository.Table.Any(x => x.EventID == eventId))
            {
                return _companyEventRepository.Table.Where(x => x.EventID == eventId).Select(x => x.ProfileCompanyID).ToList();
            }
            else
            {
                return null;
            }
        }

        public string GetEventNameById(int eventId)
        {
            return _eventRepository.Table.Any(x => x.EventID == eventId) ?
                _eventRepository.Table.Where(x => x.EventID == eventId).First().Name : string.Empty;
        }

        public Event GetEventById(int eventId)
        {
            return _eventRepository.Table.Any(x => x.EventID == eventId) ?
                _eventRepository.Table.Where(x => x.EventID == eventId).First() : null;
        }

        public List<CompanyServiceEvent> GetListCompanyServicesToEventByCompanyEventId(int Id)
        {
            return _companyServiceEventRepository.Table.Any(x => x.CompanyEventID == Id) ?
                _companyServiceEventRepository.Table.Where(x => x.CompanyEventID == Id).ToList() : null;
        }

        public int CreateCompanyEvent(CompanyEvent companyEvent)
        {
            try
            {
                _companyEventRepository.Insert(companyEvent);
                return companyEvent.CompanyEventID;
            }
            catch
            {
                return 0;
            }

        }

        public void AddCompanyServiceToEvent(CompanyServiceEvent companyServiceEvent)
        {
            if (_companyServiceEventRepository.Table.Any(x => x.CompanyEventID == companyServiceEvent.CompanyEventID && x.ServiceCompanyID == companyServiceEvent.ServiceCompanyID))
            {
                var originCSE = _companyServiceEventRepository.Table.Where(x => x.CompanyEventID == companyServiceEvent.CompanyEventID && x.ServiceCompanyID == companyServiceEvent.ServiceCompanyID).First();
                originCSE.NewPrice = companyServiceEvent.NewPrice;
                companyServiceEvent.ID = originCSE.ID;
                _companyServiceEventRepository.Update(originCSE);
            }
            else
            {
                _companyServiceEventRepository.Insert(companyServiceEvent);
            }


        }

        public void DeleteCompanyServiceToEvent(int id)
        {
            if (_companyServiceEventRepository.Table.Any(x => x.ID == id))
            {
                var originCSE = _companyServiceEventRepository.Table.Where(x => x.ID == id).First();

                _companyServiceEventRepository.Delete(originCSE);
            }
        }

        public void RemoveCompanyEvent(int profileId, int eventId)
        {
            try
            {
                //Remove all CompanyServiceToEvent
                if (_companyEventRepository.Table.Any(x => x.ProfileCompanyID == profileId && x.EventID == eventId))
                {
                    CompanyEvent ce = _companyEventRepository.Table.Where(x => x.ProfileCompanyID == profileId && x.EventID == eventId).First();
                    if (_companyServiceEventRepository.Table.Any(x => x.CompanyEventID == ce.CompanyEventID))
                    {
                        var cseList = _companyServiceEventRepository.Table.Where(x => x.CompanyEventID == ce.CompanyEventID).ToList();
                        foreach (var cseItem in cseList)
                        {
                            _companyServiceEventRepository.Delete(cseItem);
                        }
                    }
                    _companyEventRepository.Delete(ce);
                }
            }
            catch (Exception ex)
            {

            }

        }

        public int GetCompanyEventIdByProfileIdEventId(int profileId, int eventId)
        {
            if (_companyEventRepository.Table.Any(x => x.ProfileCompanyID == profileId && x.EventID == eventId))
            {
                return _companyEventRepository.Table.Where(x => x.ProfileCompanyID == profileId && x.EventID == eventId).First().CompanyEventID;
            }
            return 0;
        }

        public CompanyEvent GetCompanyEventByCompanyEventId(int companyEventId)
        {
            if (_companyEventRepository.Table.Any(x => x.CompanyEventID == companyEventId))
            {
                return _companyEventRepository.Table.Where(x => x.CompanyEventID == companyEventId).First();
            }
            return null;
        }

        public CompanyServiceEvent GetAvalableCompanyServiceEventFromCompanyServiceId(int companyServiceId, DateTime startDate)
        {
            var endCompare = startDate.AddDays(-1);
            var startCompare = startDate.AddDays(3);
            var query = (from cse in _companyServiceEventRepository.Table
                         join ce in _companyEventRepository.Table on cse.CompanyEventID equals ce.CompanyEventID
                         join e in _eventRepository.Table on ce.EventID equals e.EventID
                         where e.StartDate.Value <= startCompare && e.EndDate.Value >= endCompare && cse.ServiceCompanyID == companyServiceId
                         select cse).FirstOrDefault();
            return query;
        }

        public List<ProfileCompany> AdminGetListCompanyJoinEvent(int eventId, out int totalRecord, int pageIndex, int pageSize)
        {
            totalRecord = 0;

            var query = (from pc in _profileCompanyRepository.Table
                         join cv in _companyEventRepository.Table on pc.ProfileID equals cv.ProfileCompanyID
                         where cv.EventID == eventId && pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                         select pc).ToList();
            totalRecord = query.Count();
            return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<ProfileCompany> AdminGetListCompanyNotJoinEvent(int eventId, out int totalRecord, int pageIndex, int pageSize)
        {
            totalRecord = 0;
            var query = _profileCompanyRepository.Table.
                Where(x => x.CompanyStatusID == (int)Types.CompanyStatus.Active &&
                    !x.CompanyEvents.Any(a => a.ProfileCompanyID != x.ProfileID)).ToList();

            totalRecord = query.Count();
            return query.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public double CalculateOfferPercent(int cseId)
        {
            if (_companyServiceEventRepository.Table.Any(x => x.ID == cseId))
            {
                var originCSE = _companyServiceEventRepository.Table.Where(x => x.ID == cseId).First();

                return (double)(((originCSE.ServiceCompany.Price.Value - originCSE.NewPrice.Value) * 100) / originCSE.ServiceCompany.Price.Value);

            }
            return 0;
        }

        #endregion

    }
}
