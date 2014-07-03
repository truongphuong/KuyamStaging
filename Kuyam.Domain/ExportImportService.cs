using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Interface;
using Kuyam.Database;
using Kuyam.Domain.Company;
using System.Web.Mvc;
using Kuyam.Database.Extensions;
using Kuyam.Utility;

namespace Kuyam.Domain
{
    public class ExportImportService
    {
        #region Fields

        private readonly IRepository<Profile> _profileRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<ProfileHour> _profileHourRepository;
        private readonly IRepository<CompanyHour> _companyHourRepository;

        private System.Object lockthis = new System.Object();

        #endregion

        #region Ctor

        public ExportImportService(IRepository<Profile> profileRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<Service> serviceRepository,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<ProfileHour> profileHourRepository,
            IRepository<CompanyHour> companyHourRepository
           )
        {
            this._profileCompanyRepository = profileCompanyRepository;
            this._profileRepository = profileRepository;
            this._serviceRepository = serviceRepository;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._profileHourRepository = profileHourRepository;
            this._companyHourRepository = companyHourRepository;
        }
        #endregion




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


        private string GetRandomKey()
        {
            string key = "";
            Invite invite = new Invite();
            while (invite != null)
            {
                Random random = new Random();
                key = random.Next(1, 16777215).ToString("x").ToUpper(); //max FFFFFF value
                invite = DAL.GetInvite(key);
            }
            return key;
        }

        public List<Service> GetAllService()
        {
            lock (lockthis)
            {
                return _serviceRepository.Table.Where(m => m.ParentServiceID != null).ToList();
            }

        }

        public List<Service> GetListServiceByCategory(string categoryName, out int parentId)
        {
            lock (lockthis)
            {
                parentId = 0;
                var category = _serviceRepository.Table.FirstOrDefault(m => m.ServiceName == categoryName && !m.ParentServiceID.HasValue);
                int categoryId = 0;
                if (category != null)
                {
                    categoryId = category.ServiceID;
                    parentId = categoryId;
                }

                return _serviceRepository.Table.Where(m => m.ParentServiceID == categoryId).ToList();
            }

        }


        public void InsertService(Service service)
        {
            lock (lockthis)
            {
                _serviceRepository.Insert(service);
            }

        }

        public List<CompanyHour> GetCompanyHourProfileID(int profileId)
        {
            lock (lockthis)
            {
                return _companyHourRepository.Table.Where(m => m.ProfileCompanyID == profileId).ToList();
            }
        }

        public List<ServiceCompany> GetCategoryByProfileID(int profileId)
        {
            lock (lockthis)
            {
                var query = from svcpn in _serviceCompanyRepository.Table
                            join sv in _serviceRepository.Table on svcpn.ServiceID equals sv.ServiceID
                            where svcpn.ProfileID == profileId && !sv.ParentServiceID.HasValue
                            select svcpn;
                return query.ToList();
            }


        }
        

        // insert
        public void InsertProfile(Profile profile)
        {
            lock (lockthis)
            {
                _profileRepository.Insert(profile);
            }

        }


        public void InsertServiceCompany(List<ServiceCompany> serviceCompany)
        {
            lock (lockthis)
            {
                if (serviceCompany != null && serviceCompany.Count > 0)
                {
                    foreach (ServiceCompany item in serviceCompany)
                    {
                        _serviceCompanyRepository.Insert(item);
                    }
                }
            }
        }


        //Update
        public void UpdateProfile(Profile profile)
        {
            lock (lockthis)
            {
                _profileRepository.Update(profile);
            }
        }


        public void UpdateProfileCompany(ProfileCompany profileCompany)
        {
            lock (lockthis)
            {
                _profileCompanyRepository.Update(profileCompany);
            }
        }

        public void UpdateServiceCompany(List<ServiceCompany> serviceNew, List<ServiceCompany> serviceOld)
        {
            lock (lockthis)
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
        }

        public bool IsHasCategory(int ProfileID, int SetviceId)
        {
            lock (lockthis)
            {
                return _serviceCompanyRepository.Table.Any(m => m.ServiceID == SetviceId && m.ProfileID == ProfileID);
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
            lock (lockthis)
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
        }



    }
}

