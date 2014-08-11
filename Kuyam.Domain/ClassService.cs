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
    public class ClassService
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
        private readonly IRepository<EmployeeService> _employeeServiceRepository;
        private readonly IRepository<EmployeeHour> _employeeHourRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;   
        private readonly IRepository<InstructorClassScheduler> _instructorClassSchedulerRepository;        
        private readonly DbContext _dbContext;
        #endregion

        #region Ctor

        public ClassService(IRepository<AccessKeyManagement> accessKeyRepository,
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
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<Order> orderRepository,   
            DbContext dbContext,     
            IRepository<CompanyEmployee> companyEmployeeRepository ,           
            IRepository<InstructorClassScheduler> instructorClassSchedulerRepository,
            IRepository<EmployeeService> employeeServiceRepository,
            IRepository<EmployeeHour> employeeHourRepository
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
            this._zipCodeRepository = zipCodeRepository;
            this._orderRepository = orderRepository;
            this._appointmentRepository = appointmentRepository;    
            this._orderDetailRepository = orderDetailRepository; 
            this._dbContext = dbContext;
            this._companyEmployeeRepository = companyEmployeeRepository;            
            this._instructorClassSchedulerRepository = instructorClassSchedulerRepository;
            this._employeeServiceRepository = employeeServiceRepository;
            this._employeeHourRepository = employeeHourRepository;
            
        }
        #endregion


        public  List<ServiceCompany> GetClassSevicesByProfileID(int profileID)
        {            
            List<ServiceCompany> scList = new List<ServiceCompany>();
            if (this._serviceCompanyRepository.Table.Any(x => x.ProfileID == profileID && x.ServiceTypeId == (int)Types.ServiceType.ClassType ))
            {
                scList = _serviceCompanyRepository.Table.Where(x => x.ProfileID == profileID && x.ServiceTypeId == (int)Types.ServiceType.ClassType
                    && x.Service.ParentServiceID != null &&
                        x.Status != (int)Types.ServiceCompanyStatus.Delete).
                        ToList();                   
                return scList;
            }
            return scList;
        }

        public  List<CompanyEmployee> GetInstructorByProfileID(int profileID)
        {          
            try
            {
                if (_companyEmployeeRepository.Table.Any(x => x.ProfileCompanyID == profileID          ))
                {
                    List<CompanyEmployee> ceList = _companyEmployeeRepository.Table.Where(x => x.ProfileCompanyID == profileID).ToList();
                    return ceList;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<CompanyEmployee> GetInstructorByClassId(int classId)
        {
            try
            {
                var query = (from ce in _companyEmployeeRepository.Table
                             join ic in _employeeServiceRepository.Table on ce.EmployeeID equals ic.CompanyEmployeeID   
                             where ic.ServiceCompanyID == classId
                             select ce).ToList();
                return query;
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;

        }

        public int GetInstructorClassId(int classId, int instructorId)
        {
            int i = 0;
            if ( _employeeServiceRepository.Table.Any(x => x.ServiceCompanyID == classId && x.CompanyEmployeeID == instructorId))
            {
                i = _employeeServiceRepository.Table.Where(x => x.ServiceCompanyID == classId && x.CompanyEmployeeID == instructorId).First().ID;
            }
            return i;

        }

        public bool AddServiceCompany(ServiceCompany serviceCompany)
        {
            try
            {
                _serviceCompanyRepository.Insert(serviceCompany);              
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public List<ServiceCompany> GetListClassByProfileId(int profileId)
        {
            if (_serviceCompanyRepository.Table.Any(x => x.ProfileID == profileId &&   x.ServiceTypeId == 1))
            {
                return _serviceCompanyRepository.Table.Where(x => x.ProfileID == profileId &&  x.ServiceTypeId == 1 && x.Status == (int)Types.ServiceCompanyStatus.Active).ToList();
            }

                return null;
        }

        public ServiceCompany GetClassById(int classId)
        {
            if (_serviceCompanyRepository.Table.Any(x => x.ServiceCompanyID == classId && x.ServiceTypeId == 1))
            {
                return _serviceCompanyRepository.Table.Where(x => x.ServiceCompanyID == classId && x.ServiceTypeId == 1 && x.Status == (int)Types.ServiceCompanyStatus.Active).FirstOrDefault();
            }

            return null;
        }

        public bool AddInstructorClass(EmployeeService instructorClass)
        {
            try
            {
               if(!_employeeServiceRepository.Table.Any( x=> x.ServiceCompanyID == instructorClass.ServiceCompanyID && x.CompanyEmployeeID == instructorClass.CompanyEmployeeID))
               {
                   _employeeServiceRepository.Insert(instructorClass);
               }                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteInstructorClass(int serviceCompanyId, int employeeId)
        {
            try
            {
                if(_employeeServiceRepository.Table.Any(x=> x.ServiceCompanyID == serviceCompanyId && x.CompanyEmployeeID == employeeId))
                {
                    var es = _employeeServiceRepository.Table.Where(x => x.ServiceCompanyID == serviceCompanyId && x.CompanyEmployeeID == employeeId).First();
                    if(es.InstructorClassSchedulers.Any())
                    {
                        return false;
                    }
                    _employeeServiceRepository.Delete(es);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
       

        public List<EmployeeService> GetListInstructorClassFromInstructorId(int instructorId)
        {
            return _employeeServiceRepository.Table.Where(x => x.CompanyEmployeeID == instructorId 
                && x.ServiceCompany.ServiceTypeId == (int)Types.ServiceType.ClassType).ToList();
        }

        public  List<Service> GetParentService( int profileId, bool getActiveOnly = true)
        {
            var query = (from s in _serviceRepository.Table
                         join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                         where sc.ProfileID == profileId
                         select s).Distinct().ToList();
            if (query != null && query.Count > 0)
                return query;
            return null;
        }
        public  bool DeleteInstructorClassByInstructorID(int instructorID)
        {           
        //    try
        //    {
        //        if (_instructorClassRepository.Table.Any(x => x.CompanyEmployeeID == instructorID))
        //        {
        //            List<InstructorClass> esList =
        //                _instructorClassRepository.Table.Where(x => x.CompanyEmployeeID == instructorID).ToList();
        //            foreach (var es in esList)
        //            {
        //                _instructorClassRepository.Delete(es);
        //            }
        //            return true;
        //        }
                return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        }

        public List<EmployeeHour> GetInstructorHoursFromInstructorId(int employeeId)
        {

            var query = (from ics in _instructorClassSchedulerRepository.Table
                         join ic in _employeeServiceRepository.Table on ics.InstructorClassID equals ic.ID
                         where ic.CompanyEmployeeID == employeeId
                         select new
                         {
                             FromHour = ics.FromHour,
                             ToHour = ics.ToHour,
                             CompanyEmployeeID = employeeId,
                             DayOfWeek = ics.DayOfWeek,
                         }).ToList();

            var ihHours = new List<EmployeeHour>();
            if (query != null && query.Count > 0)
            {
                foreach (var item in query)
                {
                    var ih = new EmployeeHour();
                    ih.FromHour = item.FromHour;
                    ih.ToHour = item.ToHour;
                    ih.CompanyEmployeeID = item.CompanyEmployeeID;
                    ih.DayOfWeek = item.DayOfWeek;
                    ihHours.Add(ih);
                }
            }      
            return ihHours;

        }

        //public List<EmployeeHour> GetInstructorHoursFromClassId(int classId)
        //{

        //    var query = (from cs in _classSchedulerRepository.Table
        //                 join sc in _serviceCompanyRepository.Table on cs.ServiceCompanyID equals sc.ServiceCompanyID
        //                 join cis in _classInstructorScheduerRepository.Table on cs.ClassSchedulerID equals cis.ClassSchedulerID
        //                 join ce in _companyEmployeeRepository.Table on cis.EmployeeID equals ce.EmployeeID
        //                 where sc.ServiceCompanyID == classId && !cs.IsPreview
        //                 select new 
        //                 {                             
        //                     FromHour = cs.FromHour,
        //                     ToHour = cs.ToHour,
        //                     CompanyEmployeeID = cis.EmployeeID,
        //                     DayOfWeek = cs.DayOfWeek,
        //                     IsDaily = cs.IsPreview,
        //                     LastedUpdate = cs.LastedUpdate,
        //                     CompanyEmployee = ce
        //                 }).ToList();
        //    var ihHours = new List<EmployeeHour>();
        //    if (query != null && query.Count > 0)
        //    {
        //        foreach (var item in query)
        //        {
        //            var ih = new EmployeeHour();
        //            ih.FromHour = item.FromHour;
        //            ih.ToHour = item.ToHour;
        //            ih.CompanyEmployeeID = item.CompanyEmployeeID;
        //            ih.DayOfWeek = item.DayOfWeek;
        //            ih.IsDaily = item.IsDaily;
        //            ih.LastedUpdate = item.LastedUpdate;
        //            ih.CompanyEmployee = item.CompanyEmployee;

        //            ihHours.Add(ih);
        //        }
        //    }

        //    return ihHours;
        //}

        
        //public List<ClassScheduler> GetInstructorHourPreview(int employeeId)
        //{
        //    try
        //    {
        //        var query = (from cs in _classSchedulerRepository.Table
        //                     join cis in _classInstructorScheduerRepository.Table on cs.ClassSchedulerID equals cis.ClassSchedulerID
        //                     join ce in _companyEmployeeRepository.Table on cis.EmployeeID equals ce.EmployeeID
        //                     where cis.EmployeeID == employeeId && cs.IsPreview
        //                     select cs).ToList();

                

        //        return query;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}


        //public bool AddClassScheduler(int csId)
        //{
        //    try
        //    {
        //        if (_classSchedulerRepository.Table.Any(x => x.ClassSchedulerID == csId))
        //        {
        //            var oriCS = _classSchedulerRepository.Table.Where(x => x.ClassSchedulerID == csId).First();
        //            oriCS.IsPreview = false;
        //            _classSchedulerRepository.Update(oriCS);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //}

        public bool AddClassScheduler( InstructorClassScheduler cHour, string[] listDay, int instructorId, out string errorMessage)
        {
            try
            {
                // Parse list day
                errorMessage = string.Empty;
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

                CompanyEmployee employee = DAL.GetCompanyEmployee(instructorId);
                List<CompanyHour> companyHours = DAL.GetCompanyHourList(employee.ProfileCompanyID);
                if (companyHours == null || companyHours.Count == 0)
                {
                    errorMessage = "please recheck company hours";
                    return false;
                }
                    

                bool isValid = true;
                foreach (int day in listDays)
                {
                    TimeSpan start = cHour.FromHour;
                    TimeSpan end = cHour.ToHour;

                    // Get company hours of day
                    var companyHoursOfDay = companyHours.Where(h => h.DayOfWeek == day || h.IsDaily == true);

                    while (true)
                    {
                        // get company hours intersec with new preview hour
                        companyHoursOfDay = companyHoursOfDay.Where(h => h.ToHour > start && h.FromHour < end);

                        // if not exist any company hour, return false
                        if (companyHoursOfDay == null || companyHoursOfDay.Count() == 0)
                        {
                            errorMessage = "please recheck company hours";
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
                                
                foreach (var dayofweek in listDays)
                {
                    cHour.DayOfWeek = dayofweek;
                    var listClassScheduler = _instructorClassSchedulerRepository.Table.Where(m => m.EmployeeService.CompanyEmployeeID == instructorId
                        && m.DayOfWeek == dayofweek
                        && ((m.FromHour <= cHour.FromHour && m.ToHour > cHour.FromHour) || (m.FromHour < cHour.ToHour && m.ToHour >= cHour.ToHour))).ToList();

                    if (listClassScheduler != null && listClassScheduler.Count > 0)
                    {
                        errorMessage = "this instructor already joined another class";
                        return false;
                    }

                    var dow = (DayOfWeek)dayofweek;
                    var listAppointment = _appointmentRepository.Table.Where(m => m.EmployeeID == instructorId && m.Start >= DateTime.Now
                        // &&  SqlFunctions.DatePart("dw", m.Start)== dow
                        // &&  ((m.Start. <= cHour.FromHour && m.End.TimeOfDay >= cHour.FromHour) || (m.Start.TimeOfDay <= cHour.ToHour && m.End.TimeOfDay >= cHour.ToHour))
                         ).ToList();
                    if (listAppointment != null && listAppointment.Count > 0)
                    {
                        foreach( var p in listAppointment)
                        {
                            if(p.Start.DayOfWeek == dow
                                && ((p.Start.TimeOfDay <= cHour.FromHour && p.End.TimeOfDay > cHour.FromHour) 
                                || (p.Start.TimeOfDay < cHour.ToHour && p.End.TimeOfDay >= cHour.ToHour)))
                            {
                                errorMessage = "this instructor already have appointment";
                                return false;
                            }
                        }                        
                    }    

                    InstructorClassScheduler newItem = new InstructorClassScheduler();
                    newItem.InstructorClassID = cHour.InstructorClassID;
                    newItem.FromHour = cHour.FromHour;
                    newItem.ToHour = cHour.ToHour;
                    newItem.DayOfWeek = cHour.DayOfWeek;                    

                    _instructorClassSchedulerRepository.Insert(newItem);
                    
                                        
                }


                return true;
            }
            catch (Exception ex)
            {
                errorMessage = string.Empty;
                return false;
            }
        }

        public List<InstructorClassScheduler> GetInstructorHourByInstructorIdClassId(int classId, int instructorId)
        {
            var query = (from ics in _instructorClassSchedulerRepository.Table
                         join ic in _employeeServiceRepository.Table on ics.InstructorClassID equals ic.ID
                         where ic.CompanyEmployeeID == instructorId && ic.ServiceCompanyID == classId
                         select ics).ToList();
            return query;
           
        }

        public bool DeleteClassSchedulerById(int id)
        {
            bool result = false;
            try
            {
                if (_instructorClassSchedulerRepository.Table.Any(x => x.ID == id))
                {
                    var cs = _instructorClassSchedulerRepository.Table.Where(x => x.ID == id).FirstOrDefault();

                    _instructorClassSchedulerRepository.Delete(cs);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }


            return result;
        }

        public  bool DeleteEmployeeServicesNonClassByEmployeeID(int employeeID)
        {          
            try
            {
                if (_employeeServiceRepository.Table.Any(x => x.CompanyEmployeeID == employeeID && !x.InstructorClassSchedulers.Any()))
                {
                    List<EmployeeService> esList =
                        _employeeServiceRepository.Table.Where(x => x.CompanyEmployeeID == employeeID && !x.InstructorClassSchedulers.Any()).ToList();
                    foreach (EmployeeService es in esList)
                    {
                        _employeeServiceRepository.Delete(es);
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

        public bool AddEmployeeServicesNonClass(EmployeeService employeeService)
        {
            try
            {
                if (!_employeeServiceRepository.Table.Any(x => x.ServiceCompanyID == employeeService.ServiceCompanyID && x.CompanyEmployeeID == employeeService.CompanyEmployeeID))
                {
                    _employeeServiceRepository.Insert(employeeService);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public  bool DeleteClass(int serviceCompanyID)
        {            
            try
            {
                if(_serviceCompanyRepository.Table.Any(x=> x.ServiceCompanyID == serviceCompanyID))
                {
                    ServiceCompany sc =
                       _serviceCompanyRepository.Table.Where(x => x.ServiceCompanyID == serviceCompanyID).FirstOrDefault();

                    //remove all instructorclassscheduler
                    var icsQuery = (from ics in _instructorClassSchedulerRepository.Table
                                    join es in _employeeServiceRepository.Table on ics.InstructorClassID equals es.ID
                                    where es.ServiceCompanyID == serviceCompanyID
                                    select ics).ToList();
                    if(icsQuery != null && icsQuery.Count >0)
                    {
                        foreach(var icsItem in icsQuery)
                        {
                            _instructorClassSchedulerRepository.Delete(icsItem);
                        }
                    }

                    // remove all employeeService 
                    var esQuery = _employeeServiceRepository.Table.Where(x => x.ServiceCompanyID == serviceCompanyID).ToList();
                    if(esQuery != null && esQuery.Count > 0)
                    {
                        foreach( var esItem in esQuery)
                        {
                            _employeeServiceRepository.Delete(esItem);
                        }
                    }

                    sc.Status = (int)Types.ServiceCompanyStatus.Delete;
                    sc.Modified = DateTime.Now;
                    _serviceCompanyRepository.Update(sc);                   
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        
        public bool UpdateTimeForInstructorClassScheduler(int servicecompanyId, int duration)
        {
            try
            {
                var icsQuery = (from ics in _instructorClassSchedulerRepository.Table
                                join es in _employeeServiceRepository.Table on ics.InstructorClassID equals es.ID
                                where es.ServiceCompanyID == servicecompanyId
                                select ics).ToList();
                if(icsQuery != null && icsQuery.Count >0)
                {
                    foreach(var icsItem in icsQuery)
                    {
                        icsItem.ToHour = new DateTime(icsItem.FromHour.Ticks).AddMinutes(duration).TimeOfDay;
                        _instructorClassSchedulerRepository.Update(icsItem);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<CompanyService> GetServiceCompanybyProfileId(int profileId)
        {          

            var service = (from s in _serviceRepository.Table
                           join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                           join e in _employeeServiceRepository.Table on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where sc.ProfileID == profileId
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active && (sc.ServiceTypeId == (int)Types.ServiceType.ServiceType)
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.HasValue ? sc.Price.Value : 0,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.HasValue ? sc.AttendeesNumber.Value : 0,
                               Description = s.Desc,
                               Duration = sc.Duration.HasValue ? sc.Duration.Value : 0
                           }).OrderBy(x => x.ServiceName).Distinct();
            return service.ToList();
        }

        public List<CompanyService> GetClassesbyProfileId(int profileId)
        {

            var service = (from s in _serviceRepository.Table
                           join sc in _serviceCompanyRepository.Table on s.ServiceID equals sc.ServiceID
                           join e in _employeeServiceRepository.Table on sc.ServiceCompanyID equals e.ServiceCompanyID
                           where sc.ProfileID == profileId
                                 && sc.Status == (int)Types.ServiceCompanyStatus.Active && (sc.ServiceTypeId == (int)Types.ServiceType.ClassType)
                                 && s.ParentServiceID.HasValue
                           select new CompanyService()
                           {
                               ID = sc.ServiceCompanyID,
                               Price = sc.Price.HasValue ? sc.Price.Value : 0,
                               ServiceName = s.ServiceName,
                               AttendeesNumber = sc.AttendeesNumber.HasValue ? sc.AttendeesNumber.Value : 0,
                               Description = s.Desc,
                               Duration = sc.Duration.HasValue ? sc.Duration.Value : 0
                           }).OrderBy(x => x.ServiceName).Distinct();
            return service.ToList();
        }
    }

}
