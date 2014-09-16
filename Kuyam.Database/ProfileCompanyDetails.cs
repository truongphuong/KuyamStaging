using Kuyam.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database
{
    public class ProfileCompanyDetails
    {
        public int ProfileID { get; set; }
        public int CompanyTypeID { get; set; }
        public int CompanyStatusID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string YoutubeLink { get; set; }
        public Nullable<int> PreferredContact { get; set; }
        public Nullable<int> FirstAlert { get; set; }
        public Nullable<int> SecondAlert { get; set; }
        public string PaymentOptions { get; set; }
        public Nullable<int> PaymentMethod { get; set; }
        public Nullable<int> PayAfter { get; set; }
        public string MapUrl { get; set; }
        public string Neighborhood { get; set; }
        public string CrossStreet { get; set; }
        public string PublicTransportation { get; set; }
        public string Parking { get; set; }
        public string Notes { get; set; }
        public bool ApptAutoConfirm { get; set; }
        public int ApptDefaultSlotDuration { get; set; }
        public int ApptDefaultPeoplePerSlot { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Nullable<System.DateTime> ExpiredDate { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public Nullable<int> CancelPolicy { get; set; }
        public Nullable<int> CancelHour { get; set; }
        public Nullable<int> CancelRefundPercent { get; set; }
        public string MobileCarrier { get; set; }
        public Nullable<bool> IsShowCatagory { get; set; }
        public Nullable<bool> IsEvent { get; set; }
        public Nullable<bool> IsClass { get; set; }
        private string CategoriesXml { get; set; }
        public CompanyGoogleMap locations { get; set; }
        public int CategoryId { get; set; }
        public List<Service> Categories
        {
            get
            {
                var servicesXml = UtiHelper.Deserialize<ServicesXml>("<ServicesXml>" + this.CategoriesXml + "</ServicesXml>");

                return servicesXml.Services.Select(o => new Service { ServiceID = o.ServiceID, ServiceName = o.ServiceName }).ToList();
            }

        }

        private string ServiceCompaniesXml { get; set; }
        public List<ServiceCompany> ServiceCompanies
        {
            get
            {
                var servicesXml = UtiHelper.Deserialize<ServiceCompaniesXml>("<ServiceCompaniesXml>" + this.ServiceCompaniesXml + "</ServiceCompaniesXml>");

                return servicesXml.ServiceCompanies.Select(o => new
                ServiceCompany
                {
                    ServiceCompanyID = o.ServiceCompanyID,
                    ServiceID = o.ServiceID,
                    ServiceTypeId = o.ServiceTypeId,
                    ServiceName = o.ServiceName,
                    ToDateTime = o.ToDateTime,
                    FromDateTime = o.FromDateTime,
                    Price = o.Price,
                    Duration = o.Duration,
                    Description = o.Description,
                    EmployeeName = o.EmployeeName,
                    IsPerDay = o.IsPerDay,
                    Modified = o.Modified,
                    Created = o.Created,
                    ProfileID = o.ProfileID,
                    Status = o.Status
                }).ToList();
            }

        }

        private string CompanyEmployeeXml { get; set; }
        public List<CompanyEmployee> CompanyEmployees
        {
            get
            {
                var companyEmployeeXml = UtiHelper.Deserialize<CompanyEmployeesXml>("<CompanyEmployeesXml>" + this.CompanyEmployeeXml + "</CompanyEmployeesXml>");

                return companyEmployeeXml.CompanyEmployees.Select(o =>
                    new CompanyEmployee
                    {
                        EmployeeID = o.EmployeeID,
                        EmployeeName = o.EmployeeName,
                        Email = o.Email,
                        EmployeeTypeId = o.EmployeeTypeId,
                        IsDefault = o.IsDefault,
                        Phone = o.Phone,
                        ProfileCompanyID = o.ProfileCompanyID

                    }
                    ).ToList();
            }

        }


        private string ImageIdStr { get; set; }

        public List<string> ImageIds
        {
            get
            {
                var list= ImageIdStr.Split(',').ToList();
                return list;

            }
        }

    }
}
