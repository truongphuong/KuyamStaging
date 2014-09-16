using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Domain.Company;
using Kuyam.Domain.MessageServcies;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using System.Web.Mvc;
using Kuyam.Database.Extensions;
using Kuyam.Utility;
using System.Data.Entity;
using Kuyam.Domain.ClassModel;
using System.Data.SqlClient;
using Kuyam.Domain.AppointmentModel;

namespace Kuyam.Domain.CompanyProfileServices
{
    public class CompanyProfileService
    {
        #region Fields

        private readonly DbContext _dbContext;
        private readonly IRepository<Profile> _profileRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<Medium> _mediaRepository;
        private readonly IRepository<CompanyMedia> _companyMediaRepository;
        private readonly IRepository<ProfileHour> _profileHourRepository;
        private readonly IRepository<Invite> _inviteRepository;
        private readonly IRepository<CompanyHour> _companyHourRepository;
        private readonly IRepository<GeneralTimeSlot> _generalTimeSlotRepository;
        private readonly IRepository<EmployeeHour> _employeeHourRepository;
        private readonly IRepository<EmployeeService> _employeeServiceRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;
        private readonly IRepository<ZipCode> _zipCodeRepository;
        private readonly IRepository<UserPackagePurchase> _userPackagePurchaseRepository;
        private readonly IRepository<CompanyPackageService> _companyPackageServiceRepository;
        private readonly IRepository<CompanyPackageImage> _companyPackageImageRepository;
        private readonly IRepository<UserPackagePurchaseHistory> _UserPackagePurchaseHistoryServiceRepository;
        private readonly IRepository<CompanyPackage> _companyPackageRepository;
        private readonly IRepository<Calendar> _calendarRepository;
        private readonly IRepository<CalendarShare> _calendarShareRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountRegularClient> _discountRegularClientRepository;
        private readonly IRepository<RegularClient> _regularClientRepository;
        private readonly IRepository<DiscountService> _discountServiceRepositRepository;
        private readonly IRepository<UserDiscount> _userDiscountRepository;
        private readonly IRepository<DiscountInvite> _discountInviteRepository;
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Cust> _custRepository;
        private readonly IRepository<aspnet_Users> _userRepository;

        #endregion

        #region Ctor

        public CompanyProfileService(DbContext dbContext,
            IRepository<Profile> profileRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<Service> serviceRepository,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<EmployeeHour> employeeHourRepository,
            IRepository<Medium> mediaRepository,
            IRepository<CompanyMedia> companyMediaRepository,
            IRepository<CompanyEmployee> companyEmployeeRepository,
            IRepository<ProfileHour> profileHourRepository,
            IRepository<Invite> inviteRepository,
            IRepository<CompanyHour> companyHourRepository,
            IRepository<GeneralTimeSlot> generalTimeSlotRepository,
            IRepository<ZipCode> zipCodeRepository,
            IRepository<UserPackagePurchase> userPackagePurchaseRepository,
            IRepository<CompanyPackageService> companyPackageServiceRepository,
            IRepository<UserPackagePurchaseHistory> userPackagePurchaseHistoryServiceRepository,
            IRepository<CompanyPackageImage> companyPackageImageRepository,
            IRepository<CompanyPackage> companyPackageRepository,
            IRepository<Calendar> calendarRepository,
            IRepository<CalendarShare> calendarShareRepository,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRegularClient> discountRegularClientRepository,
            IRepository<RegularClient> regularClientRepository,
            IRepository<DiscountService> discountServiceRepositRepository,
            IRepository<UserDiscount> userDiscountRepository,
            IRepository<DiscountInvite> discountInviteRepository,
            IRepository<EmployeeService> employeeServiceRepository,
            IRepository<Favorite> favoriteRepository,
            IRepository<Cust> custRepository,
            IRepository<aspnet_Users> userRepository)
        {
            this._dbContext = dbContext;
            this._profileCompanyRepository = profileCompanyRepository;
            this._profileRepository = profileRepository;
            this._serviceRepository = serviceRepository;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._mediaRepository = mediaRepository;
            this._companyMediaRepository = companyMediaRepository;
            this._profileHourRepository = profileHourRepository;
            this._inviteRepository = inviteRepository;
            this._companyHourRepository = companyHourRepository;
            this._generalTimeSlotRepository = generalTimeSlotRepository;
            this._zipCodeRepository = zipCodeRepository;
            this._userPackagePurchaseRepository = userPackagePurchaseRepository;
            this._companyPackageServiceRepository = companyPackageServiceRepository;
            this._UserPackagePurchaseHistoryServiceRepository = userPackagePurchaseHistoryServiceRepository;
            this._companyPackageImageRepository = companyPackageImageRepository;
            this._companyPackageRepository = companyPackageRepository;
            this._calendarRepository = calendarRepository;
            this._calendarShareRepository = calendarShareRepository;
            this._employeeHourRepository = employeeHourRepository;
            this._companyEmployeeRepository = companyEmployeeRepository;
            this._discountRepository = discountRepository;
            this._discountRegularClientRepository = discountRegularClientRepository;
            this._regularClientRepository = regularClientRepository;
            this._discountServiceRepositRepository = discountServiceRepositRepository;
            this._userDiscountRepository = userDiscountRepository;
            this._employeeServiceRepository = employeeServiceRepository;
            this._discountInviteRepository = discountInviteRepository;
            this._favoriteRepository = favoriteRepository;
            this._custRepository = custRepository;
            this._userRepository = userRepository;
        }

        #endregion


        public List<SelectListItem> GetListAlertTime(int? selected)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (Types.AlertTime item in Enum.GetValues(typeof(Types.AlertTime)))
            {
                list.Add(new SelectListItem()
                {
                    Text = GetNameAlert(item),//UtilityHelper.ConvertEnum(item.ToString()),
                    Value = ((int)item).ToString(),
                    Selected = (selected.HasValue && (item == (Types.AlertTime)selected))
                });
            }
            return list;
        }

        private string GetNameAlert(Types.AlertTime type)
        {
            switch (type)
            {
                case Types.AlertTime.none:
                    return "none";
                case Types.AlertTime.AtTimeOfEvent:
                    return "at time of event ";
                case Types.AlertTime.FifteenMinBefore:
                    return "15 min before";
                case Types.AlertTime.ThirtyMinBefore:
                    return "30 min before";
                case Types.AlertTime.OneHourBefore:
                    return "1 hour before";
                case Types.AlertTime.TwoHoursBefore:
                    return "2 hours before";
                case Types.AlertTime.OneDayBefore:
                    return "1 day before";
                case Types.AlertTime.TwoDaysBefore:
                    return "2 days before";
            }
            return string.Empty;
        }

        public string AddInviteCode(int custId, string email = "", string name = "", string lname = "")
        {
            Invite invite = new Invite();
            invite.CustID = custId;
            invite.Key = GetRandomKey();
            invite.MaxUses = 1;
            invite.Uses = 0;
            invite.AccountTypeID = (int)Types.CustType.Company;
            invite.Email = email;
            invite.Name = name;
            invite.LName = lname;
            invite.CreateDate = DateTime.UtcNow;
            invite.Active = false;
            invite.InviteType = (int)Types.InviteType.Company;
            _inviteRepository.Insert(invite);
            return invite.Key;
        }

        public Invite GetInvite(string code, int custID)
        {
            return _inviteRepository.Table.Single(m => m.Key.ToUpper() == code.ToUpper()
            && m.AccountTypeID == (int)Types.CustType.Company
            && m.CustID == custID
            && m.InviteType == (int)Types.InviteType.Company
            );
        }

        public void Acvite(Invite invite)
        {
            invite.Active = true;
            invite.Uses++;
            _inviteRepository.Update(invite);
        }

        private string GetRandomKey()
        {
            string key = "";
            Invite invite = new Invite();
            while (invite != null)
            {
                Random random = new Random();
                key = random.Next(1, 16777215).ToString("x").ToUpper(); //max FFFFFF value
                invite = GetInvite(key);

            }
            return key;
        }

        private Invite GetInvite(string code)
        {
            return _inviteRepository.Table.Where(x => x.Key.ToUpper() == code.ToUpper() && x.Active == false).FirstOrDefault();
        }

        public List<Service> GetListService()
        {
            return _serviceRepository.Table.Where(m => m.ParentServiceID == null && m.Status == true).OrderBy(m => m.ServiceName).ToList();
        }

        public List<Service> GetAllService()
        {
            return _serviceRepository.Table.Where(m => m.ParentServiceID != null).ToList();
        }

        public List<ServiceCompany> GetCompanyServiceByProfileId(int profileId)
        {

            var query = from svc in _serviceCompanyRepository.Table
                        where svc.ProfileID == profileId &&
                              svc.Status == (int)Types.ServiceCompanyStatus.Active
                              && (svc.Service != null && svc.Service.ParentServiceID != null)
                        select svc;

            return query.ToList();
        }

        public List<ServiceCompany> GetCompanyServiceByProfileId(int profileId, int catelogyId)
        {

            var query = from svc in _serviceCompanyRepository.Table
                        where svc.ProfileID == profileId &&
                              svc.Status == (int)Types.ServiceCompanyStatus.Active
                              && (svc.Service != null && svc.Service.ParentServiceID != null && svc.Service.ParentServiceID == catelogyId)
                        select svc;

            return query.ToList();
        }
        public List<CompanyService> GetProposedCompanyServiceById(int profileId)
        {

            var query = from svc in _serviceCompanyRepository.Table
                        where svc.ProfileID == profileId &&
                              svc.Status == (int)Types.ServiceCompanyStatus.Active
                              && (svc.Service != null && svc.Service.ParentServiceID != null)
                        select new CompanyService
                        {
                            ID = svc.ServiceCompanyID,
                            Price = svc.Price ?? 0,
                            Duration = svc.Duration ?? 0,
                            ServiceName = svc.Service.ServiceName,
                            EmployeeName = svc.EmployeeName
                        };

            return query.ToList();
        }

        public ServiceCompany GetCompanyServiceById(int Id)
        {
            var query = from svc in _serviceCompanyRepository.Table
                        where svc.ServiceCompanyID == Id && svc.Status == (int)Types.ServiceCompanyStatus.Active
                        select svc;

            return query.FirstOrDefault();
        }

        public CompanyEmployee GetEmployeeById(int id, int profileId)
        {
            return _companyEmployeeRepository.Table.Where(m => (id == 0 || m.EmployeeID == id) && m.ProfileCompanyID == profileId).FirstOrDefault();
        }

        public List<CompanyEmployeeExt> GetListEmployeeById(int id, int profileId)
        {
            return _companyEmployeeRepository.Table.Where(m =>
                (id == 0 || m.EmployeeID == id)
                && m.ProfileCompanyID == profileId
                && m.EmployeeHours.Any()
                && m.EmployeeServices.Any())
            .Select(m => new CompanyEmployeeExt
            {
                EmployeeID = m.EmployeeID,
                EmployeeName = m.EmployeeName,
                ProfileCompanyID = m.ProfileCompanyID
            }
            ).ToList();
        }

        public List<Service> GetListServiceByCategory(string categoryName, out int parentId)
        {
            parentId = 0;
            var category = _serviceRepository.Table.Where(m => m.ServiceName == categoryName && !m.ParentServiceID.HasValue).FirstOrDefault();
            int categoryId = 0;
            if (category != null)
            {
                categoryId = category.ServiceID;
                parentId = categoryId;
            }

            return _serviceRepository.Table.Where(m => m.ParentServiceID == categoryId).ToList();
        }

        public Service GetServiceByID(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return _serviceRepository.Table.FirstOrDefault(m => m.ServiceID == id);
        }

        public void InsertService(Service service)
        {
            _serviceRepository.Insert(service);
        }

        public ZipCode GetZipCodeByKey(string key)
        {
            return _zipCodeRepository.Table.Where(m => m.Code == key).FirstOrDefault();
        }

        public List<ZipCode> GetZipCodeAll()
        {
            return _zipCodeRepository.Table.ToList();
        }


        public void InsertZipcode(ZipCode item)
        {
            _zipCodeRepository.Insert(item);
        }

        public List<Medium> GetMediaByProfileID(int custId)
        {
            return _mediaRepository.Table.Where(m => m.CustID == custId).ToList();
        }

        public List<ProfileHour> GetProfileHourProfileID(int profileId)
        {
            return _profileHourRepository.Table.Where(m => m.ProfileID == profileId).ToList();
        }

        public List<CompanyHour> GetCompanyHourProfileID(int profileId)
        {
            return _companyHourRepository.Table.Where(m => m.ProfileCompanyID == profileId).ToList();
        }

        public Profile GetProfileByID(int profileId)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pf.ProfileID == profileId
                        && pfcp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                        select pf;
            return query.FirstOrDefault();
        }

        public Profile GetProfileByNameAndCity(string name)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pfcp.Name.ToLower().Contains(name.ToLower())
                        && pfcp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                        select pf;
            return query.FirstOrDefault();
        }

        public Profile GetProfileByCustID(int CustID)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pf.CustID == CustID
                        && pfcp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                        select pf;
            return query.FirstOrDefault();
        }

        public Profile IsVerified(int CustID)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pf.CustID == CustID
                        && pfcp.CompanyStatusID == (int)Types.CompanyStatus.Active

                        select pf;
            return query.FirstOrDefault();
        }

        public ProfileCompany GetProfileCompanyByID(int profileId)
        {
            var query = from pfcp in _profileCompanyRepository.Table
                        where pfcp.ProfileID == profileId
                        select pfcp;
            return query.FirstOrDefault();
        }

        public List<ProfileCompany> GetAllProfileCompany(int CompanyType = -1)
        {
            var query = _profileCompanyRepository.Table.Where(m => (CompanyType == -1 || m.CompanyTypeID == CompanyType || m.CompanyTypeID == (int)Types.CompanyType.HybridKuyamBookIt)
                && m.CompanyStatusID == (int)Types.CompanyStatus.Active).OrderBy(m => m.Name).ToList();
            return query;
        }

        public ProfileCompany GetProfileCompanyByID(int profileId, int custId)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pf.ProfileID == profileId && pf.CustID == custId
                        && pfcp.CompanyStatusID == (int)Types.CompanyStatus.Active
                        select pfcp;
            return query.FirstOrDefault();
        }

        public ProfileCompany GetProfileCompanyByCustID(int custId)
        {
            var query = from pf in _profileRepository.Table
                        join pfcp in _profileCompanyRepository.Table on pf.ProfileID equals pfcp.ProfileID
                        where pf.CustID == custId
                        && pfcp.CompanyStatusID != (int)Types.CompanyStatus.Deleted
                        select pfcp;
            return query.FirstOrDefault();
        }

        public List<ServiceCompany> GetServiceCompanyByProfileID(int profileId)
        {
            var query = from svcpn in _serviceCompanyRepository.Table
                        join sv in _serviceRepository.Table
                        on svcpn.ServiceID equals sv.ServiceID
                        where svcpn.ProfileID == profileId
                        && svcpn.ServiceTypeId == (int)Types.ServiceType.ServiceType
                        && sv.ParentServiceID.HasValue
                        && (svcpn.Status == 0)
                        select svcpn;
            return query.Distinct().ToList();

        }

        public List<Kuyam.Database.Extensions.CompanyService> GetServiceCompanyByProfileID(int profileId, int statusId)
        {
            var service = (from s in _serviceCompanyRepository.Table
                           join p in _serviceRepository.Table on s.ServiceID equals p.ServiceID
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

        public List<ServiceCompany> GetCategoryByProfileID(int profileId)
        {
            var query = from svcpn in _serviceCompanyRepository.Table
                        join sv in _serviceRepository.Table on svcpn.ServiceID equals sv.ServiceID
                        where svcpn.ProfileID == profileId && !sv.ParentServiceID.HasValue
                        select svcpn;
            return query.ToList();

        }

        public bool IsHasCategory(int ProfileID, int SetviceId)
        {
            return _serviceCompanyRepository.Table.Any(m => m.ServiceID == SetviceId && m.ProfileID == ProfileID);

        }

        public List<CompanyMedia> GetCompanyMediaByProfileID(int profileId)
        {
            var query = from cpmd in _companyMediaRepository.Table
                        join md in _mediaRepository.Table on cpmd.MediaID equals md.MediaID
                        where cpmd.ProfileID == profileId
                        select cpmd;
            return query.ToList();

        }

        public Medium GetCompanyLogoByProfileID(int profileId)
        {
            var query = from cpmd in _companyMediaRepository.Table
                        join md in _mediaRepository.Table on cpmd.MediaID equals md.MediaID
                        where cpmd.ProfileID == profileId
                        && cpmd.IsLogo
                        select md;
            return query.FirstOrDefault();

        }

        // insert
        public void InsertProfile(Profile profile)
        {
            _profileRepository.Insert(profile);
        }

        public bool CheckCompanyName(string companyName)
        {
            bool result = false;
            string name = companyName.Replace(" ", string.Empty);

            List<ProfileCompany> companies = _profileCompanyRepository.Table.Where(p => p.CompanyStatusID != (int)Types.CompanyStatus.Deleted).ToList();
            foreach (ProfileCompany company in companies)
            {
                if (company.Name.Trim().Replace(" ", string.Empty).Equals(name))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool CheckCompanyName(string companyName, string street)
        {
            bool result = false;
            string name = companyName.Replace(" ", string.Empty);

            List<ProfileCompany> companies = _profileCompanyRepository.Table.Where(p => p.CompanyStatusID != (int)Types.CompanyStatus.Deleted).ToList();
            foreach (ProfileCompany company in companies)
            {
                if (company.Name.Trim().Replace(" ", string.Empty).Equals(name) && UtilityHelper.DamerauLevenshteinDistance(company.Street1, street) < 1)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void InsertServiceCompany(List<ServiceCompany> serviceCompany)
        {
            if (serviceCompany != null && serviceCompany.Count > 0)
            {
                foreach (ServiceCompany item in serviceCompany)
                {
                    _serviceCompanyRepository.Insert(item);
                }
            }
        }

        public void InsertProfileHour(List<ProfileHour> profileHour)
        {
            if (profileHour != null && profileHour.Count > 0)
            {
                foreach (ProfileHour item in profileHour)
                {
                    _profileHourRepository.Insert(item);
                }
            }
        }

        public void InsertCompanyHour(List<CompanyHour> companyHour)
        {
            if (companyHour != null && companyHour.Count > 0)
            {
                foreach (CompanyHour item in companyHour)
                {
                    _companyHourRepository.Insert(item);
                }
            }
        }

        public void InsertCompanyMedia(List<CompanyMedia> CompanyMedia)
        {
            if (CompanyMedia != null && CompanyMedia.Count > 0)
            {
                foreach (CompanyMedia item in CompanyMedia)
                {

                    _companyMediaRepository.Insert(item);
                }
            }
        }
        public void InsertCompanyMedia(Medium media, int ProfileId)
        {
            try
            {
                if (media != null)
                {
                    CompanyMedia companyMedia = new CompanyMedia()
                    {
                        ProfileID = ProfileId,
                        MediaID = media.MediaID,
                        IsBanner = true,
                        IsDefault = false,
                        IsHidden = false
                    };
                    _companyMediaRepository.Insert(companyMedia);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Insert media to company media is fail: ", ex);
            }
        }
        public void InsertMedia(Medium media)
        {
            _mediaRepository.Insert(media);
        }

        //Update
        public void UpdateProfile(Profile profile)
        {
            _profileRepository.Update(profile);
        }

        public List<int> GetEmployeeByProfileId(int profileID)
        {
            var query = from elp in _companyEmployeeRepository.Table
                        where elp.ProfileCompanyID == profileID
                        && elp.EmployeeServices.Any()
                        && elp.EmployeeHours.Any()
                        select elp.EmployeeID;
            return query.ToList();
        }

        public void UpdateProfileCompany(ProfileCompany profileCompany)
        {
            _profileCompanyRepository.Update(profileCompany);
        }

        public void UpdateServiceCompany(List<ServiceCompany> serviceNew, List<ServiceCompany> serviceOld)
        {
            if (serviceOld != null && serviceOld.Count > 0)
            {
                foreach (ServiceCompany item in serviceOld)
                {
                    if (serviceNew == null || !serviceNew.Any() || serviceNew.All(s => s.ServiceID != item.ServiceID))
                        _serviceCompanyRepository.Delete(item);
                }
            }


            foreach (ServiceCompany item in serviceNew)
            {
                if (!IsHasCategory(item.ProfileID, item.ServiceID))
                {
                    _serviceCompanyRepository.Insert(item);
                }
            }
        }

        public void UpdateProfileHour(List<ProfileHour> lstProfileHournew, List<ProfileHour> lstProfileHourOld)
        {
            if (lstProfileHourOld != null && lstProfileHourOld.Count > 0)
            {
                foreach (ProfileHour item in lstProfileHourOld)
                {
                    _profileHourRepository.Delete(item);
                }

            }

            if (lstProfileHournew != null && lstProfileHournew.Count > 0)
            {
                foreach (ProfileHour item in lstProfileHournew)
                {
                    _profileHourRepository.Insert(item);
                }
            }
        }

        public void UpdateCompanyHour(List<CompanyHour> lstProfileHournew, List<CompanyHour> lstProfileHourOld)
        {
            if (lstProfileHourOld != null && lstProfileHourOld.Count > 0)
            {
                foreach (CompanyHour item in lstProfileHourOld)
                {
                    _companyHourRepository.Delete(item);
                }

            }

            if (lstProfileHournew != null && lstProfileHournew.Count > 0)
            {
                foreach (CompanyHour item in lstProfileHournew)
                {
                    _companyHourRepository.Insert(item);
                }
            }
        }

        public void UpdateCompanyVideo(CompanyMedia companyMediaNew, CompanyMedia companyMediaOld)
        {
            if (companyMediaOld != null)
            {
                companyMediaOld.MediaID = companyMediaNew.MediaID;
                _companyMediaRepository.Update(companyMediaOld);
            }
            else
            {
                _companyMediaRepository.Insert(companyMediaNew);
            }
        }

        public void UpdateCompanyMedia(List<CompanyMedia> lstcompanyMediaNew, List<CompanyMedia> lstcompanyMediaOld)
        {
            if (lstcompanyMediaOld != null && lstcompanyMediaOld.Count > 0)
            {
                foreach (CompanyMedia item in lstcompanyMediaOld)
                {
                    _companyMediaRepository.Delete(item);
                }
            }

            if (lstcompanyMediaNew != null && lstcompanyMediaNew.Count > 0)
            {
                foreach (CompanyMedia item in lstcompanyMediaNew)
                {
                    _companyMediaRepository.Insert(item);
                }
            }
        }
        public void UpdateCompanyMedia(CompanyMedia companyMedia)
        {
            try
            {

                CompanyMedia media = (from cpmd in _companyMediaRepository.Table
                                      join md in _mediaRepository.Table on cpmd.MediaID equals md.MediaID
                                      where cpmd.MediaID == companyMedia.MediaID
                                      select cpmd).FirstOrDefault();

                if (media != null)
                {

                    media.IsDefault = companyMedia.IsDefault;
                    media.IsHidden = companyMedia.IsHidden;
                    _companyMediaRepository.Update(media);
                    LogHelper.Info(string.Format("Updated company image: CompanyMediaID= {0}", companyMedia.CompanyMediaID));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Update company image fail: ", ex);
            }
        }
        public void UpdateMedia(Medium media)
        {

        }

        //Delete 
        public void DeleteCompany(int profileId, string reason)
        {
            Profile profile = _profileRepository.Table.First(m => m.ProfileID == profileId);
            ProfileCompany profileCompany = _profileCompanyRepository.Table.First(m => m.ProfileID == profileId);
            profileCompany.Desc = reason;
            profileCompany.CompanyStatusID = (int)Types.CompanyStatus.Deleted;
            profile.ProfileCompany = profileCompany;
            _profileRepository.Update(profile);
        }

        public List<CompanyPackageExt> GetNonUsingPackageByProfileID(int profileId, Types.CompanyPackageStatus status)
        {
            var packages = (from c in _companyPackageRepository.Table
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
                   _companyPackageServiceRepository.Table.Where(c => c.CompanyPackageId == ext.PackageId).Select
                        (c => c.ServiceCompanyId).ToList();
                companyPackageExt.KalturaImages =
                    _companyPackageImageRepository.Table.Where(c => c.CompanyProfileId == profileId).OrderBy(c => c.CreatedDate)
                        .Select
                        (c => c.LocationData).ToList();
            }

            return packages.ToList();
        }

        //Trong Edit
        public void UpdateDefaultEmploee(int profileCompanyID)
        {

            List<CompanyEmployee> profileCompanies = _companyEmployeeRepository.Table.Where(m => m.ProfileCompanyID == profileCompanyID).ToList();

            foreach (var profileCompany in profileCompanies)
            {
                profileCompany.IsDefault = false;
            }

            _companyEmployeeRepository.Update(new CompanyEmployee());
        }

        public bool DeleteMediaById(int mediaId)
        {

            bool result = false;

            try
            {
                if (_mediaRepository.Table.Any(x => x.MediaID == mediaId))
                {
                    CompanyMedia media = _companyMediaRepository.Table.Where(x => x.MediaID == mediaId).FirstOrDefault();
                    _companyMediaRepository.Delete(media);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public bool DeleteAllMediaByProfileId(int profileId)
        {
            bool result = false;
            try
            {

                List<CompanyMedia> medium = _companyMediaRepository.Table.Where(x => x.ProfileID == profileId).ToList();
                if (medium != null && medium.Count > 0)
                {
                    foreach (CompanyMedia media in medium)
                    {
                        _companyMediaRepository.Delete(media);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Get list User Package by cust Id
        /// </summary>
        /// <param name="profileId">int:cust Id</param>
        /// <returns>List<UserPackagePurchase></returns>
        public List<UserPackagePurchase> GetUserPackagePurchaseCustID(int custId)
        {

            var query = from upp in _userPackagePurchaseRepository.Table
                        where upp.CustID == custId && upp.MaxUses != 0
                        select upp;
            return query.ToList();
        }

        /// <summary>
        /// Get list User Package by cust Id
        /// </summary>
        /// <param name="profileId">int:cust Id</param>
        /// <returns>List<UserPackagePurchase></returns>
        public List<UserPackagePurchase> GetUserAvailablePackageByCustID(int custId)
        {

            var query = from upp in _userPackagePurchaseRepository.Table
                        where upp.CustID == custId
                        && (upp.UserPackageStatus == null || upp.UserPackageStatus.Value == (int)Types.UserPackageStatus.Active)
                        && (upp.ExpiredDate > DateTime.UtcNow)
                        //&& (upp.UserPackageStatus==null || upp.UserPackageStatus.Value == (int)Types.UserPackageStatus.Active)
                        //&& ((upp.CompanyPackage.PackageType == (int)Types.CompanyPackageType.ByDuration && upp.ExpiredDate > DateTime.UtcNow)
                        //|| (upp.CompanyPackage.PackageType == (int)Types.CompanyPackageType.BySlot && upp.MaxUses >= 0)
                        //|| (upp.CompanyPackage.PackageType == (int)Types.CompanyPackageType.Mix && upp.ExpiredDate > DateTime.UtcNow && upp.MaxUses >= 0))

                        select upp;
            return query.ToList();
        }

        /// <summary>
        /// Get User Package by package ID
        /// </summary>
        /// <param name="packageId">int:packageId</param>
        /// <returns>UserPackagePurchase</returns>
        public UserPackagePurchase GetUserPackagePurchasePackageID(int packageId)
        {
            var query = from upp in _userPackagePurchaseRepository.Table
                        join cpp in _companyPackageRepository.Table on upp.CompanyPackageId equals cpp.PackageId
                        where upp.UserPackagePurchaseId == packageId
                        && ((upp.ExpiredDate >= DateTime.UtcNow && cpp.DurationInMonth != -1) || (cpp.DurationInMonth == -1))
                        select upp;
            return query.FirstOrDefault();
        }

        public CompanyPackageService GetCompanyPackageServiceByCompanyPackageId(int companyPackageId)
        {
            var query = from cpp in _companyPackageServiceRepository.Table
                        where cpp.CompanyPackageId == companyPackageId
                        select cpp;
            return query.FirstOrDefault();
        }

        public List<CompanyPackageService> GetServiceByPackageId(int packageId)
        {
            var query = from cpp in _companyPackageServiceRepository.Table
                        where cpp.CompanyPackageId == packageId
                        select cpp;
            return query.ToList();
        }

        public List<UserPackagePurchase> GetUserPackagePurchaseByPackageId(int packageId)
        {
            var query = from up in _userPackagePurchaseRepository.Table
                        where up.CompanyPackageId == packageId
                        select up;
            return query.ToList();
        }

        public IQueryable<int> GetlistCompanyPackageServiceByCompanyPackageId(int companyPackageId)
        {
            var query = from cpp in _companyPackageServiceRepository.Table
                        where cpp.CompanyPackageId == companyPackageId
                        select cpp.ServiceCompanyId;
            return query;
        }

        public CompanyPackage GetCompanyPackagebyPackageId(int CompanyPackageId)
        {
            var query = from cpp in _companyPackageRepository.Table
                        where cpp.PackageId == CompanyPackageId && cpp.Status == (int)Types.CompanyPackageStatus.Active
                        select cpp;
            return query.FirstOrDefault();
        }

        public CompanyPackageImage GetSoonestPackageImageByProfileId(int profileId)
        {
            var service = (from s in _companyPackageImageRepository.Table
                           where s.CompanyProfileId == profileId
                           select s).OrderBy(s => s.CreatedDate).ToList();
            if (service.Count >= 6)
                return service.FirstOrDefault();
            else
                return null;

        }

        public void CreateCompanyPackageImage(int profileID, string imageId)
        {
            var service = new CompanyPackageImage
            {
                CompanyProfileId = profileID,
                LocationData = imageId,
                CreatedDate = DateTime.Now
            };
            _companyPackageImageRepository.Insert(service);
        }

        public void UpdateCompanyPackageImage(int companyPackageImageId, string imageId)
        {
            var service = _companyPackageImageRepository.Table.FirstOrDefault(s => s.CompanyPackageImageId == companyPackageImageId);
            if (service != null)
                service.LocationData = imageId;
            _companyPackageImageRepository.Insert(service);
        }

        public void CreateCompanyAdminPackage(int profileID, string name, decimal packPrice, int packQuantity, int packDuration, List<int> packSerivces, string imageUrl)
        {
            Types.CompanyPackageType packageType = Types.CompanyPackageType.ByUnlimited;
            if (packQuantity > 0)
                packageType = Types.CompanyPackageType.ByQuanlity;

            var service = new CompanyPackage
            {
                PackageName = name,
                Description = string.Format("applies to: {0} service(s)", packSerivces.Count),
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
            _companyPackageRepository.Insert(service);
            foreach (var id in packSerivces)
            {
                var companyPackageService = new CompanyPackageService
                {
                    CompanyPackage = service,
                    ServiceCompanyId = id
                };
                _companyPackageServiceRepository.Insert(companyPackageService);
            }
        }

        public void UpdateCompanyAdminPackage(int profileID, int packageId, string name, decimal packPrice, int packQuantity, int packDuration, List<int> packSerivces, string imageUrl)
        {
            Types.CompanyPackageType packageType = Types.CompanyPackageType.ByUnlimited;
            CompanyPackage companyPackage = _companyPackageRepository.Table.FirstOrDefault(c => c.PackageId == packageId);
            if (companyPackage != null)
            {
                if (packQuantity > 0)
                    packageType = Types.CompanyPackageType.ByQuanlity;

                companyPackage.PackageName = name;
                companyPackage.Description = string.Format("applies to: {0} service(s)", packSerivces.Count);
                companyPackage.ProfileCompanyId = profileID;
                companyPackage.PackageType = (int)packageType;
                companyPackage.NumberOfBooking = packQuantity;
                companyPackage.Price = packPrice;
                companyPackage.Status = (int)Types.CompanyPackageStatus.Active;
                companyPackage.DurationInMonth = packDuration;
                companyPackage.KalturaImageId = imageUrl;

                var lstPackService = _companyPackageServiceRepository.Table.Where(c => c.CompanyPackageId == packageId).ToList();

                for (int i = lstPackService.Count - 1; i >= 0; i--)
                {
                    _companyPackageServiceRepository.Delete(lstPackService[i]);
                }

                foreach (var id in packSerivces)
                {
                    var companyPackageService = new CompanyPackageService
                                                    {
                                                        CompanyPackageId = packageId,
                                                        ServiceCompanyId = id
                                                    };
                    _companyPackageServiceRepository.Insert(companyPackageService);
                }

            }
        }

        public void AdminCompanyMakeInactive(int packageId)
        {
            CompanyPackage obj = GetCompanyPackagebyPackageId(packageId);
            if (obj != null)
                obj.Status = (int)Types.CompanyPackageStatus.Inactive;
            _companyPackageRepository.Update(obj);
        }

        public void DeleteCompanyAdminPackage(int packageId)
        {
            CompanyPackage companyPackage = _companyPackageRepository.Table.FirstOrDefault(c => c.PackageId == packageId);
            if (companyPackage != null)
            {
                var lstPackService = _companyPackageServiceRepository.Table.Where(c => c.CompanyPackageId == packageId).ToList();

                for (int i = lstPackService.Count - 1; i >= 0; i--)
                {
                    _companyPackageServiceRepository.Delete(lstPackService[i]);
                }
                _companyPackageRepository.Delete(companyPackage);
            }
        }

        public List<string> GetCompanyImagesbyProfileID(int profileId)
        {
            var lstPackImage = _companyPackageImageRepository.Table.Where(c => c.CompanyProfileId == profileId).Select(s => s.LocationData);
            return lstPackImage.ToList();
        }

        public void UpdateUserPackagePurchase(UserPackagePurchase userPackagePurchase)
        {
            _userPackagePurchaseRepository.Update(userPackagePurchase);
        }

        public void InsertUserPackageHistoryPurchase(UserPackagePurchaseHistory UserPurchaseHistory)
        {
            _UserPackagePurchaseHistoryServiceRepository.Insert(UserPurchaseHistory);
        }

        public void InsertUserPackagePurchase(UserPackagePurchase UserPurchase)
        {
            _userPackagePurchaseRepository.Insert(UserPurchase);
        }

        public List<Calendar> GetCalendarByCustId(int custId)
        {
            var query = from cal in _calendarRepository.Table
                        join cals in _calendarShareRepository.Table on cal.CalendarID equals cals.CalendarID
                        where cals.CustID == custId
                        select cal;
            return query.ToList();
        }

        public Calendar GetCalendarByCalendarId(int calendarId)
        {
            var query = from cal in _calendarRepository.Table
                        where cal.CalendarID == calendarId
                        select cal;
            return query.FirstOrDefault();
        }

        #region Company Discount
        public Discount GetDiscountCodeByID(int id)
        {

            Discount discount = _discountRepository.Table.Where(d => d.DiscountId == id).FirstOrDefault();
            return discount;
        }

        public void CreateCompanyDiscount(int profileId, string name, string code, bool isAmount, decimal amount, bool isPercent, decimal percent, int quantity, int serviceId,
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            startDate = startDate.Date.AddMinutes(startTime.TotalMinutes);
            endDate = endDate.Date.AddMinutes(endTime.TotalMinutes);
            //startDate = DateTimeUltility.ConvertToUtcTime(startDate, DateTimeKind.Local);
            //endDate = DateTimeUltility.ConvertToUtcTime(endDate, DateTimeKind.Local);

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
            _discountRepository.Insert(discount);

            if (serviceId > 0)
            {
                DiscountService discountService = new DiscountService()
                {
                    Discount = discount,
                    ServiceCompanyId = serviceId
                };
                _discountServiceRepositRepository.Insert(discountService);

            }


        }

        public void UpdateCompanyDiscount(int profileId, int discountId, string name, string code, bool isAmount, decimal amount, bool isPercent, decimal percent, int quantity, int serviceId,
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            startDate = startDate.Date.AddMinutes(startTime.TotalMinutes);
            endDate = endDate.Date.AddMinutes(endTime.TotalMinutes);
            //startDate = DateTimeUltility.ConvertToUtcTime(startDate, DateTimeKind.Local);
            //endDate = DateTimeUltility.ConvertToUtcTime(endDate, DateTimeKind.Local);

            Discount discount = _discountRepository.Table.FirstOrDefault(d => d.DiscountId == discountId);
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

            var delDiscountService = _discountServiceRepositRepository.Table.FirstOrDefault(d => d.DiscountId == discountId);
            if (delDiscountService != null)
            {
                _discountServiceRepositRepository.Delete(delDiscountService);
            }

            if (serviceId > 0)
            {
                DiscountService discountService = new DiscountService()
                {
                    Discount = discount,
                    ServiceCompanyId = serviceId
                };
                _discountServiceRepositRepository.Insert(discountService);

            }

        }

        public List<CompanyDiscountExt> GetDiscountCompanyByProfileID(int profileID, int status)
        {
            var lstCompanyDiscount = _discountRepository.Table.Where(d => d.ProfileCompanyId == profileID && d.Status == status).
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
                companyDiscountExt.IsSent = _discountRegularClientRepository.Table.Any(d => d.DiscountId == companyDiscountExt.DiscountId);
                if (companyDiscountExt.ApplyToAllServices)
                {
                    companyDiscountExt.ServiceId = 0;
                    companyDiscountExt.Description = "all services";
                }
                else
                {
                    var service = (from sc in _serviceCompanyRepository.Table
                                   join ds in _discountServiceRepositRepository.Table on sc.ServiceCompanyID equals ds.ServiceCompanyId
                                   join s in _serviceRepository.Table on sc.ServiceID equals s.ServiceID
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
                        companyDiscountExt.NumberOfUsage = _userDiscountRepository.Table.Count(u => u.DiscountId == companyDiscountExt.DiscountId);
                        companyDiscountExt.NumberofSent = _discountRegularClientRepository.Table.Count(r => r.DiscountId == companyDiscountExt.DiscountId);
                        companyDiscountExt.ServiceId = service.ID;
                    }
                }
            }

            return lstCompanyDiscount;
        }


        public bool IsExistDiscountCode(int profileId, string code)
        {
            return _discountRepository.Table.Any(d => d.ProfileCompanyId == profileId && d.Code == code && d.Status == (int)Types.DiscountStatus.Active);

        }

        public bool IsExistDiscountCode(int profileId, int discountId, string code)
        {
            return _discountRepository.Table.Any(d => d.ProfileCompanyId == profileId && d.Code == code && d.Status == (int)Types.DiscountStatus.Active && d.DiscountId != discountId);

        }

        public bool IsUsedDiscount(int discountId)
        {
            return _userDiscountRepository.Table.Any(d => d.DiscountId == discountId) || _discountRegularClientRepository.Table.Any(d => d.DiscountId == discountId);
        }

        public int DeleteUserPackage(int packageId)
        {
            UserPackagePurchase userPackage = _userPackagePurchaseRepository.Table.FirstOrDefault(m => m.UserPackagePurchaseId == packageId);
            if (userPackage == null)
                return 0;
            try
            {
                userPackage.UserPackageStatus = (int)Types.UserPackageStatus.Delete;
                _userPackagePurchaseRepository.Update(userPackage);
            }
            catch
            {
                return 0;
            }

            return userPackage.CompanyPackage.ProfileCompanyId;
        }

        public void DeleteDiscountCompany(int discountId)
        {
            var discount = _discountRepository.Table.FirstOrDefault(d => d.DiscountId == discountId);
            if (discount != null)
            {
                discount.Status = (int)Types.DiscountStatus.Inactive;
                _discountRepository.Update(discount);
            }
        }

        public List<RegularClient> GetRegularClient(int discountId)
        {
            var regulars = from r in _regularClientRepository.Table
                           join dr in _discountRegularClientRepository.Table on r.RegularClientId equals dr.RegularClientId
                           where dr.DiscountId == discountId && r.Status
                           select r;
            return regulars.Distinct().ToList();
        }
        public List<RegularClient> GetRegularClientsbyProfileId(int profileID)
        {
            var regulars = from r in _regularClientRepository.Table
                           join u in _userRepository.Table on r.Email equals u.UserName
                           join c in _custRepository.Table on u.UserId equals c.AspUserID
                           where r.CompanyProfileId == profileID && r.Status
                           select r;
            return regulars.ToList();

        }


        public string LoadRegularClient(int discountId)
        {
            StringBuilder result = new StringBuilder();
            List<RegularClient> regularClients = GetRegularClient(discountId);
            foreach (var regularClient in regularClients)
            {
                string name = string.Format("{0} {1}", regularClient.FirstName, regularClient.LastName);
                if (name.Length > 15)
                    name = name.Substring(0, 15);
                result.Append(string.Format("<div class=\"recipitem\">{0}</div>", name));
            }
            return result.ToString();
        }

        public void InsertUserDiscount(UserDiscount UserDiscount)
        {
            _userDiscountRepository.Insert(UserDiscount);
        }

        public void InsertUserDiscountInvite(DiscountInvite UserDiscountInvite)
        {
            _discountInviteRepository.Insert(UserDiscountInvite);
        }

        #endregion Company Discount

        public bool IsAvailableToday(int profileCompanyId)
        {
            var shList = (from eh in _employeeHourRepository.Table
                          join ce in _companyEmployeeRepository.Table on eh.CompanyEmployeeID equals ce.EmployeeID
                          where ce.ProfileCompanyID == profileCompanyId
                          select eh).Distinct().ToList();
            if (shList.Count == 0)
                return false;
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);
            DateTime dtInit = dtNow.Date;
            var svHours = shList.Where(s => s.DayOfWeek.ToString().Contains(((int)dtNow.DayOfWeek).ToString()) && (!s.IsPreview)).ToList();

            foreach (var svHour in svHours)
            {
                double minuteToEnd = (dtInit.AddHours(svHour.ToHour.Hours) - dtNow).TotalMinutes;
                if (minuteToEnd > 10)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get all the service times of company.
        /// </summary>
        /// <param name="profileId">The profile id.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IQueryable<ServiceTime> GetServiceTimes(int profileId)
        {
            List<ServiceTime> results = new List<ServiceTime>();
            return _employeeHourRepository.Table.Where(e => e.CompanyEmployee.ProfileCompanyID == profileId)
                                              .Join(_employeeServiceRepository.Table,
                                                    hour => hour.CompanyEmployeeID,
                                                    service => service.CompanyEmployeeID,
                                                    (hour, service) => new { hour, service })
                                              .Select(d => new ServiceTime()
                                                  {
                                                      EmployeeId = d.hour.CompanyEmployeeID,
                                                      DateOfWeek = d.hour.DayOfWeek,
                                                      FromHour = d.hour.FromHour,
                                                      ToHour = d.hour.ToHour,
                                                      ServiceCompanyId = d.service.ServiceCompanyID
                                                  });
        }


        /// <summary>
        /// Gets the company service times.
        /// </summary>
        /// <param name="profileId">The profile id.</param>
        /// <returns></returns>
        public List<ServiceTime> GetCompanyServiceTimes(int profileId, DateTime dtnow)
        {
            List<ServiceTime> results = new List<ServiceTime>();
            List<ServiceTime> lstEmployeeHour = _employeeHourRepository.Table.Where(e => e.CompanyEmployee.ProfileCompanyID == profileId
                && e.CompanyEmployee.EmployeeServices.Any()
                && !e.IsPreview)
                                              .Select(d => new ServiceTime()
                                              {
                                                  EmployeeId = d.CompanyEmployeeID,
                                                  DateOfWeek = d.DayOfWeek,
                                                  FromHour = d.FromHour,
                                                  ToHour = d.ToHour,
                                                  ServiceCompanyId = 0
                                              }).ToList();


            int dayOfWeek = (int)dtnow.DayOfWeek;
            int detDay = 7 - dayOfWeek;
            int day = 0;

            var companyHours = GetCompanyHourList(profileId);

            foreach (ServiceTime item in lstEmployeeHour)
            {
                DateTime dt = dtnow;

                if (item.DateOfWeek >= dayOfWeek)
                {
                    day = item.DateOfWeek - dayOfWeek;
                }
                else
                {
                    day = item.DateOfWeek + detDay;
                }

                dt = dt.AddDays(day);
                dt = dt.Date;//new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);

                var eventCustoms = GetIntersectWithCompanyHours(companyHours, item.EmployeeId, dt, item.FromHour, item.ToHour);
                foreach (ServiceTime ec in eventCustoms)
                {
                    ec.EmployeeId = item.EmployeeId;
                    results.Add(ec);
                }

            }

            return results;
        }


        /// <summary>
        /// Gets the company service times.
        /// </summary>
        /// <param name="profileId">The profile id.</param>
        /// <returns></returns>
        public List<ServiceTime> GetCompanyGeneralServiceTimes(int profileId, DateTime dtnow)
        {
            List<ServiceTime> lstEmployeeHour = _generalTimeSlotRepository.Table.Where(e => e.ProfileId == profileId)
                                              .Select(d => new ServiceTime()
                                              {
                                                  Id = d.Id,
                                                  DateOfWeek = d.DayOfWeek,
                                                  FromHour = d.FromHour,
                                                  ToHour = d.ToHour,
                                                  ServiceCompanyId = 0
                                              }).ToList();

            return lstEmployeeHour;
        }

        public ServiceCompany GetServiceCompanyByID(int serviceId)
        {
            return _serviceCompanyRepository.Table.Where(m => m.ServiceCompanyID == serviceId).FirstOrDefault();
        }
        public List<EmployeeHour> GetEmployeeHour(int profileId, int employeeId, int serviceId)
        {
            CompanyEmployee employee = new CompanyEmployee();
            if (employeeId == 0)
            {
                var queryTemp = from emh in _employeeHourRepository.Table
                                join emp in _companyEmployeeRepository.Table on emh.CompanyEmployeeID equals emp.EmployeeID
                                join ems in _employeeServiceRepository.Table on emp.EmployeeID equals ems.CompanyEmployeeID                              
                                where ems.ServiceCompanyID == serviceId 
                                && !emh.IsPreview
                                orderby emp.EmployeeName 
                                select emp ;
                employee = queryTemp.FirstOrDefault();
            }

            var query = (from emh in _employeeHourRepository.Table
                         join emp in _companyEmployeeRepository.Table on emh.CompanyEmployeeID equals emp.EmployeeID
                         join ems in _employeeServiceRepository.Table on emp.EmployeeID equals ems.CompanyEmployeeID
                         where emp.ProfileCompanyID == profileId
                         && ((serviceId == 0 || ems.ServiceCompanyID == serviceId) && ems.ServiceCompany.ServiceTypeId == (int)Types.ServiceType.ServiceType)
                         && (((employeeId == 0 && emp.IsDefault) || (emp.EmployeeID == employee.EmployeeID)) || emp.EmployeeID == employeeId)
                         && !emh.IsPreview // this field use for review or not
                         select emh).Distinct();

            return query.OrderBy(m => m.DayOfWeek).ToList();
        }

        public List<CompanyHour> GetCompanyHourList(int proflieID)
        {
            var result = _companyHourRepository.Table
              .Where(c => c.ProfileCompanyID == proflieID)
              .OrderBy(c => c.CompanyHourID)
              .ToList();
            return result;
        }

        public List<CompanyService> GetServiceCompanybyEmployeeId(int profileId, int employeeId, int serviceId = 0, int categoryId = 0)
        {

            var service = (from s in _serviceRepository.Table
                           join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                           join e in _employeeServiceRepository.Table on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where (profileId == 0 || sc.ProfileID == profileId)
                                 && (categoryId == 0 || categoryId == null || s.ParentServiceID == categoryId)
                                 && (employeeId == 0 || e.CompanyEmployeeID == employeeId)
                                 && (serviceId == 0 || sc.ServiceCompanyID == serviceId)
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active
                                 && sc.ServiceTypeId == (int)Types.ServiceType.ServiceType
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
                               ServiceTypeId = sc.ServiceTypeId
                           });
            return service.ToList();
        }

        public List<CompanyService> GetServiceEmployeeByPackage(int employeeId, int packageId, int profileId, int categoryId = 0)
        {
            var service = (from s in _serviceRepository.Table
                           join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                           join e in _employeeServiceRepository.Table on sc.ServiceCompanyID equals e.ServiceCompanyID
                           join pkg in _companyPackageServiceRepository.Table on sc.ServiceCompanyID equals pkg.ServiceCompanyId
                           join upkg in _userPackagePurchaseRepository.Table on pkg.CompanyPackageId equals upkg.CompanyPackageId
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

        public List<CompanyService> GetGeneralServiceCompanybyEmployeeId(int employeeId, int profileId = 0, int categoryId = 0)
        {
            var service = (from s in _serviceRepository.Table
                           join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
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
        public List<CompanyEmployee> GetEmployeeListByProfileCompanyId(int profileCompanyId)
        {
            var companyEmployees = _companyEmployeeRepository.Table.Where(e => e.ProfileCompanyID == profileCompanyId && e.EmployeeServices.Any() && e.EmployeeHours.Any()).ToList();
            return companyEmployees;
        }

        public bool CheckFavoriteByProfileID(int profileID, int custID)
        {
            return _favoriteRepository.Table.Any(a => a.CustID == custID && a.ProfileID == profileID);
        }

        public List<Service> GetServiceListByProfileCompanyId(int profileCompanyId)
        {
            var services = from s in _serviceRepository.Table
                           join p in _serviceCompanyRepository.Table on s.ServiceID equals p.ServiceID
                           where p.ProfileID == profileCompanyId && p.Status == (int)Types.ServiceCompanyStatus.Active
                           select s;
            return services.ToList();
        }

        public List<CompanyPackage> GetCompanyPackages(int profileId)
        {
            var packages = from p in _companyPackageRepository.Table
                           where p.ProfileCompanyId == profileId && p.Status == (int)Types.CompanyPackageStatus.Active
                           select p;
            return packages.ToList();
        }

        public List<Medium> GetCompanyMediums(int profileId, Types.CompanyMediaType mediatype)
        {
            if (mediatype == Types.CompanyMediaType.IsLogo)
            {
                var ret = (from cm in _companyMediaRepository.Table
                           join m in _mediaRepository.Table on cm.MediaID equals m.MediaID
                           where cm.IsLogo && cm.ProfileID == profileId
                           select m);
                return ret.ToList();
            }
            else if (mediatype == Types.CompanyMediaType.IsBanner)
            {
                var ret = (from cm in _companyMediaRepository.Table
                           join m in _mediaRepository.Table on cm.MediaID equals m.MediaID
                           where cm.IsBanner && cm.ProfileID == profileId
                           select m);
                return ret.ToList();
            }
            else if (mediatype == Types.CompanyMediaType.IsVideo)
            {
                var ret = (from cm in _companyMediaRepository.Table
                           join m in _mediaRepository.Table on cm.MediaID equals m.MediaID
                           where cm.IsVideo && cm.ProfileID == profileId
                           select m);
                return ret.ToList();
            }

            var isDefault = (from cm in _companyMediaRepository.Table
                             join m in _mediaRepository.Table on cm.MediaID equals m.MediaID
                             where cm.IsDefault && cm.ProfileID == profileId
                             select m);

            return isDefault.ToList();
        }

        public List<ProfileCompany> GetFavoriteListByCustID(int custID)
        {
            var result = (from f in _favoriteRepository.Table
                          join pc in _profileCompanyRepository.Table on f.ProfileID equals pc.ProfileID
                          where f.CustID == custID && pc.CompanyStatusID == (int)Types.CompanyStatus.Active
                          select pc).Distinct().ToList();
            return result;
        }

        public Favorite GetFavoriteByProfileIdAndCustID(int profileId, int custID)
        {
            var result = _favoriteRepository.Table.Where(a => a.ProfileID == profileId && a.CustID == custID).FirstOrDefault();

            return result;
        }
        public void AddFavorite(Favorite favorite)
        {
            if (!_favoriteRepository.Table.Any(m => m.ProfileID == favorite.ProfileID && m.CustID == favorite.CustID))
            {
                _favoriteRepository.Insert(favorite);
            }
        }

        public void RemoveFromFavorite(Favorite favorite)
        {
            if (_favoriteRepository.Table.Any(m => m.ProfileID == favorite.ProfileID && m.CustID == favorite.CustID))
            {
                _favoriteRepository.Delete(favorite);
            }
        }

        public List<ServiceTime> GetIntersectWithCompanyHours(List<CompanyHour> companyHours, int employeeId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            List<ServiceTime> results = new List<ServiceTime>();
            int dateOfWeek = (int)date.DayOfWeek;

            // Get company hours of day
            var companyHoursOfDay = companyHours.Where(h => h.DayOfWeek == dateOfWeek || h.IsDaily == true);

            while (true)
            {
                // get company hours intersec
                companyHoursOfDay = companyHoursOfDay.Where(h => h.ToHour > startTime && h.FromHour < endTime);

                // if not exist any company hour, return false
                if (companyHoursOfDay == null || companyHoursOfDay.Count() == 0)
                {
                    return results;
                }

                if (companyHoursOfDay.Any(h => h.FromHour <= startTime && h.ToHour >= endTime))
                {
                    results.Add(new ServiceTime
                    {
                        DateOfWeek = dateOfWeek,
                        FromDateTime = date.Date.AddMinutes(startTime.TotalMinutes),
                        ToDateTime = date.Date.AddMinutes(endTime.TotalMinutes),
                        FromHour = startTime,
                        ToHour = endTime
                    });
                    return results;
                }

                // get company hour cover start time
                var companyHourCoverStart = companyHoursOfDay.Where(h => h.FromHour <= startTime).OrderByDescending(h => h.ToHour).FirstOrDefault();

                // if not cover, truncate start time
                if (companyHourCoverStart == null)
                {
                    startTime = companyHoursOfDay.Where(h => h.FromHour > startTime).OrderBy(h => h.FromHour).First().FromHour;
                }
                // if cover, get time cover
                else
                {
                    var newEndTime = companyHourCoverStart.ToHour;
                    results.Add(new ServiceTime
                        {
                            DateOfWeek = dateOfWeek,
                            FromDateTime = date.Date.AddMinutes(startTime.TotalMinutes),
                            ToDateTime = date.Date.AddMinutes(newEndTime.TotalMinutes),
                            FromHour = startTime,
                            ToHour = newEndTime
                        });

                    startTime = newEndTime;
                }
            }
        }

        public List<Service> GetCategoryByServiceAvailability(List<int> lstServiceId, int profileId)
        {
            var query = (from svc in _serviceCompanyRepository.Table
                         join sv in _serviceRepository.Table
                         on svc.ServiceID equals sv.ServiceID
                         where svc.ProfileID == profileId
                         && (!lstServiceId.Any() || lstServiceId.Contains(sv.ServiceID)
                         && svc.Status == (int)Types.ServiceCompanyStatus.Active)
                         select sv.ParentServiceID).ToList();

            return _serviceRepository.Table.Where(m => query.Contains(m.ServiceID)).ToList();
        }

        public List<ServiceCompany> GetServiceByProfileCateloryID(int profileId, int catelogryId)
        {
            var query = from svcpn in _serviceCompanyRepository.Table
                        join sv in _serviceRepository.Table on svcpn.ServiceID equals sv.ServiceID
                        where svcpn.ProfileID == profileId
                              && sv.ParentServiceID.HasValue
                              && sv.ParentServiceID == catelogryId
                              && svcpn.Status == (int)Types.ServiceCompanyStatus.Active
                        select svcpn;
            return query.ToList();
        }

        public List<Service> GetCategories()
        {
            var catelogries = from ser in _serviceRepository.Table
                              where !ser.ParentServiceID.HasValue
                              && (ser.Status == null || ser.Status == true)
                              select ser;
            return catelogries.OrderBy(o => o.ServiceName).ToList();
        }

        public List<Service> GetCategoryByProfileID(int profileId, DateTime startDate)
        {
            try
            {
                var endDate = startDate.AddMinutes(30);
                var startTime = new TimeSpan(startDate.Hour, startDate.Minute, startDate.Second);
                var endTime = new TimeSpan(endDate.Hour, endDate.Minute, endDate.Second);
                var dayOfWeek = GetIndexDayOfWeek(startDate.DayOfWeek);
                //get all catagories of profileId and avilible time
                var categories = from s in _serviceRepository.Table
                                 join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                                 where sc.ProfileID == profileId
                                       && s.ParentServiceID == null
                                 select s;

                //var categories = from cate in cates.ToList()
                //                 where
                //                     cate.ProfileCompany.CompanyHours != null &&
                //                     cate.ProfileCompany.CompanyHours.Any(
                //                         a => a.FromHour <= startTime && endTime <= a.ToHour && a.DayOfWeek == dayOfWeek)
                //                 select cate.Service;
                return categories.ToList();
            }
            catch (Exception ezxException)
            {
                LogHelper.Error("GetCategoryByProfileID:" + ezxException.StackTrace.ToString());
            }
            return null;
        }

        public int GetIndexDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 0;
                    break;
                case DayOfWeek.Monday:
                    return 1;
                    break;
                case DayOfWeek.Tuesday:
                    return 2;
                    break;
                case DayOfWeek.Wednesday:
                    return 3;
                    break;
                case DayOfWeek.Thursday:
                    return 4;
                    break;
                case DayOfWeek.Friday:
                    return 5;
                    break;
                case DayOfWeek.Saturday:
                    return 6;
                    break;
            }
            return 0;
        }

        public Service GeServiceByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return _serviceRepository.Table.FirstOrDefault(m => m.ServiceName.Trim().ToUpper() == name.Trim().ToUpper()
                && !m.ParentServiceID.HasValue);
        }

        #region Class base
        public List<SchedulerAvailability> GetSchedulerAvailabilityOfClass(int profileId, int ServiceId, int employeeId, string fromDate, string toDate)
        {
            return _dbContext.SqlQuery<SchedulerAvailability>("GetSchedulerAvailabilityOfClass @ProfileId, @ClassId, @EmployeeId,  @FromDateTime, @ToDateTime",
                new SqlParameter("ProfileId", profileId),
                new SqlParameter("ClassId", ServiceId),
                new SqlParameter("EmployeeId", employeeId),
                new SqlParameter("FromDateTime", fromDate),
                new SqlParameter("ToDateTime", toDate)).ToList();
        }
        #endregion

        /// <summary>
        /// Example: DayOfWeek = 234 => [2, 3, 4]
        /// Purpose: create simple model to display data. Take care to use for other purpose.
        /// </summary>
        /// <param name="hours"></param>
        /// <returns></returns>
        public List<CompanyHour> SplitCompanyHours(List<CompanyHour> hours)
        {
            var companyHours = new List<CompanyHour>();
            foreach (var hour in hours)
            {
                var daysOfWeek = hour.DayOfWeek.ToString();
                foreach (var day in daysOfWeek)
                {
                    var no = int.Parse(day.ToString());
                    var companyHour = new CompanyHour
                    {
                        DayOfWeek = no,
                        CompanyHourID = hour.CompanyHourID,
                        FromHour = hour.FromHour,
                        ToHour = hour.ToHour,
                        IsDaily = hour.IsDaily,
                        ProfileCompanyID = hour.ProfileCompanyID
                    };
                    companyHours.Add(companyHour);
                }
            }
            return companyHours;
        }

        public List<CompanyHour> SortCompanyHours(List<CompanyHour> hours)
        {
            var companyHourComparer = new CompanyHourComparer();
            hours.Sort(companyHourComparer);
            return hours;
        }

    }
}
