﻿using Kuyam.Database;
using Kuyam.Domain.PagedList;
using Kuyam.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.SearchServices
{
    public class SearchService : ISearchService
    {

        #region Fields
        private readonly DbContext _dbContext;
        private readonly CompanySearchService _companySearchService;
        private readonly IRepository<EmployeeService> _employeeServiceRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;
        #endregion
        public SearchService(DbContext dbContext, CompanySearchService companySearchService,
            IRepository<CompanyEmployee> companyEmployeeRepository,
            IRepository<EmployeeService> employeeServiceRepository)
        {
            this._dbContext = dbContext;
            this._companySearchService = companySearchService;
            this._companyEmployeeRepository = companyEmployeeRepository;
            this._employeeServiceRepository = employeeServiceRepository;
        }

        public List<CompanyProfileSearch> CompanySearchForWeb(
            out int totalRecords,
            List<string> categoriesId,
            string keySearch = null,
            int? categoryId = null,
            double? currentLat = null,
            double? currentLon = null,
            double? distance = null,
            int? custId = null,
            int pageIndex = 0,
            int pageSize = 2147483647)
        {
            totalRecords = 0;
            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;
            var list = _dbContext.SqlQuery<CompanyProfileSearch>("CompanySearchForWeb @KeySearch,@ServiceID,@CurrentLat,@CurrentLong,@Distance,@CustID,@PageIndex,@PageSize, @TotalRecords out",
                new SqlParameter("KeySearch", keySearch),
                 new SqlParameter("ServiceID", categoryId),
                 new SqlParameter("CurrentLat", currentLat),
                 new SqlParameter("CurrentLong", currentLon),
                 new SqlParameter("Distance", distance),
                 new SqlParameter("CustID", custId),
                 new SqlParameter("PageIndex", pageIndex),
                 new SqlParameter("PageSize", pageSize),
                     pTotalRecords
                 ).ToList();

            totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;

            var appointments = _companySearchService.GetAppoinmentsByProfileIds(list.Select(a => a.ProfileID).ToList());

            foreach (var item in list)
            {
                if (item.ListServiceIds != null)
                {
                    categoriesId.AddRange(item.ListServiceIds.Split(',').ToList());
                    item.ListsCategoriesds.AddRange(item.ListServiceIds.Split(',').ToList());
                }
                _companySearchService.TransformEmployeeHours(item);
                _companySearchService.TransformInstructorClassSchedulerHours(item);
                _companySearchService.TransformCompanyHours(item);
                _companySearchService.TransformEvents(item);
                item.Appointments = appointments.Where(m => m.ProfileId == item.ProfileID).ToList();
                item.CompanyAvailableTimeSlots = _companySearchService.GetCompanyAvailableTimeSlots(item);
            }

            return list;
        }


        public ProfileCompanyDetails GetCompanyProfileDetials(int profileId, int categoryId = 0)
        {
            var result = _dbContext.SqlQuery<ProfileCompanyDetails>("GetCompanyProfileDetials @ProfileID,@CategoryId",
                new SqlParameter("ProfileID", profileId),
                new SqlParameter("CategoryId", categoryId));

            return result.FirstOrDefault();

        }

        public List<CompanyEmployee> GetEmployeeByServiceCompanyId(int serviceComapanyId)
        {
            var query = from ce in _companyEmployeeRepository.Table
                        join ehs in _employeeServiceRepository.Table on ce.EmployeeID equals ehs.CompanyEmployeeID
                        where ehs.ServiceCompanyID == serviceComapanyId                       
                        orderby ce.EmployeeName      
                        select ce;
            return query.Distinct().ToList();
        }

        public List<ServiceCompany> GetServiceCompanyByCategoryId(int profileId, int categoryId)
        {
            var result = _dbContext.SqlQuery<ServiceCompany>("GetServiceCompanyByCategoryId @ProfileID,@CategoryId",
                new SqlParameter("ProfileID", profileId),
                new SqlParameter("CategoryId", categoryId));

            return result.ToList();

        }
    }
}
