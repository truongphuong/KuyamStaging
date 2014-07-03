using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Domain;
using OfficeOpenXml;
using Kuyam.WebUI.Models;
using System.Web.Security;
using Kuyam.Database;
using System.IO;
using Kuyam.Utility;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using Kuyam.Repository.Infrastructure;

namespace Kuyam.WebUI.Controllers
{
    [Authorize(Roles = "admin")]
    public class ExportImportController : KuyamBaseController
    {
        //private readonly ExportImportService _companyProfileService;
        ////private readonly IMembershipService _membershipService;
        ////private readonly CustService _custService;

        //public ExportImportController(ExportImportService companyProfileService)
        //{
        //    this._companyProfileService = companyProfileService;
        //    //this._membershipService = membershipService;
        //    //this._custService = custService;
        //}

        #region Utilitis

        private List<CompanySetupModel> _companySetupList = new List<CompanySetupModel>();

        protected int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            if (columnName == null)
                throw new ArgumentNullException("columnName");

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }
        /*
        private int CreateCustomer(Cust model)
        {
            bool isAdmin = false;
            MembershipCreateStatus createStatus = _membershipService.CreateUser(model.UserAcount, model.Password, model.UserAcount);
            if (createStatus != MembershipCreateStatus.Success)
            {
                throw new Exception(AccountValidation.ErrorCodeToString(createStatus));
            }

            Guid userid = DAL.GetAspUserID(model.UserAcount);
            int accountID = 0;
            try
            {
                accountID = AccountHelper.CreateAccount(true, (int)Types.AccountStatus.Ok);

            }
            catch (Exception ex)
            {
                DAL.DeleteAspUser(model.UserAcount);
                throw ex.GetBaseException();
            }

            model.CompanyName = string.Format("{0}'s company", model.CompanyName);
            if (model.Birthday == DateTime.MinValue)
            {
                model.Birthday = DateTime.Now;
            }

            model.CustType = 115; //115 = Personal; 116 = Vendor
            int custID = 0;
            try
            {
                double lat;
                double lon;
                Kuyam.Domain.BusinessService.GetLatAndLonByAreaCode(int.Parse(model.Zip), out lat, out lon);
                custID = Cust.Create(model.UserAcount, userid, accountID, model.FirstName, model.LastName,
                    model.CompanyName, model.MobilePhone, model.MobileCarrier, isAdmin, model.CustType, model.Zip, model.PreferredPhone,
                    model.Birthday, true, false, false, string.Empty, lat, lon, 0);

                _custService.AddDefaultCalendar(custID, model.FirstName);
            }
            catch (Exception ex)
            {
                DAL.DeleteUser(model.UserAcount);
                throw ex;
            }

            AccountHelper.UpdateInviteUsage(model.InviteCode, accountID);

            if (model.CustType == (int)Types.CustType.Company)
            {
                Roles.AddUserToRole(model.UserAcount, "Company");
            }
            else
            {
                Roles.AddUserToRole(model.UserAcount, "Personal");
            }

            return custID;
        }
        */
        private void CreateCompany(ExportImportService _companyProfileService, CompanySetupModel model, IEnumerable<string> listhour = null, string categorieName = "", string listServices = "")
        {

            int parentId = 0;
            List<Service> lstCategoryService = _companyProfileService.GetListServiceByCategory(categorieName.Trim().ToLower(), out parentId);
            List<Service> lstService = _companyProfileService.GetAllService();

            string[] listservice = listServices.Split(',');


            if (!string.IsNullOrEmpty(categorieName) && parentId == 0)
            {

                Service servcie = new Service()
                {
                    ParentServiceID = null,
                    ServiceName = categorieName,
                    Desc = string.Empty
                };
                _companyProfileService.InsertService(servcie);
                parentId = servcie.ServiceID;

            }


            model.Listcategory = categorieName.Split(',');
            foreach (string item in model.Listcategory)
            {
                Category category = new Category();
                category.CategoryID = parentId.ToString();
                category.NamCategory = item;
                model.Categories.Add(category);
            }


            foreach (string item in listservice)
            {
                if (listservice != null)
                {
                    var service = lstService.Where(m => m.ServiceName == item).FirstOrDefault();
                    if (service == null && parentId > 0 && item != "N/A" && item.Trim().Length <= 150)
                    {
                        Service servcie = new Service()
                        {
                            ParentServiceID = parentId,
                            ServiceName = item.Trim(),
                            Desc = string.Empty
                        };
                        _companyProfileService.InsertService(servcie);
                    }
                }
            }


            string strAddress = string.Empty;
            GeoClass.Coordinate coordinate;
            if (model.Latitude == 0.0 && model.Longitude == 0.0)
            {
                if (!string.IsNullOrEmpty(model.Street1))
                {
                    strAddress += model.Street1;
                }
                if (!string.IsNullOrEmpty(model.Street2))
                {
                    if (strAddress != string.Empty)
                        strAddress += ",";
                    strAddress += model.Street2;
                }

                if (!string.IsNullOrEmpty(model.City))
                {
                    if (strAddress != string.Empty)
                        strAddress += ",";
                    strAddress += model.City;
                }

                if (!string.IsNullOrEmpty(model.Zip))
                {
                    if (strAddress != string.Empty)
                        strAddress += " ";
                    strAddress += model.Zip;
                }

                if (!string.IsNullOrEmpty(strAddress))
                    coordinate = GeoClass.GetCoordinates(strAddress);
                else
                    coordinate = new GeoClass.Coordinate(0, 0);

                model.Latitude = (double)coordinate.Latitude;
                model.Longitude = (double)coordinate.Longitude;
            }

            var profileCompany = new ProfileCompany
            {
                CompanyTypeID = (int)Types.CompanyType.NonKuyamBookIt,
                CompanyStatusID = (int)Types.CompanyStatus.Active,
                Name = model.Name,
                Street1 = model.Street1,
                Street2 = model.Street2,
                City = model.City,
                State = model.State,
                Zip = model.Zip,
                Email = string.Empty,
                Url = model.Url,
                PaymentOptions = model.PaymentOptions,
                YoutubeLink = model.Youtubelink,
                Phone = Kuyam.Domain.UtilityHelper.CleanPhone(model.Phone),
                ContactName = model.ContactName,
                ApptAutoConfirm = true,
                ApptDefaultSlotDuration = 0,
                ApptDefaultPeoplePerSlot = 0,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ExpiredDate = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow

            };
            int preferredContact = 0;
            if (model.EmailType)
            {
                preferredContact |= (int)Types.PreferredPhone.Email;
            }
            if (model.TextType)
            {
                preferredContact |= (int)Types.PreferredPhone.Text;
            }
            profileCompany.PreferredContact = preferredContact;
            var profile = new Profile
            {
                CustID = model.CustID,
                PrivacyTypeID = (int)Types.PrivacyType.Private,
                RelationshipTypeID = (int)Types.RelationshipType.Company,
                ProfileTypeID = 116,
                Name = model.Name,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                ProfileCompany = profileCompany
            };


            _companyProfileService.InsertProfile(profile);


            model.ProfileID = profile.ProfileID;

            InserHour(_companyProfileService, profile, listhour);



            List<ServiceCompany> lstServicenOld = _companyProfileService.GetCategoryByProfileID(profile.ProfileID);
            if (model.Categories != null && profile.ProfileID != 0)
            {
                var lstServicenNew = new List<ServiceCompany>();
                foreach (Category item in model.Categories)
                {
                    int serviceId = 0;
                    int.TryParse(item.CategoryID, out serviceId);
                    var serviceCompany = new ServiceCompany
                    {
                        ProfileID = profile.ProfileID,
                        ServiceID = serviceId,
                        Status = (int)Types.ServiceCompanyStatus.Active,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow

                    };
                    lstServicenNew.Add(serviceCompany);
                }

                _companyProfileService.UpdateServiceCompany(lstServicenNew, lstServicenOld);


            }


        }


        public void ImportCompanyXlsx(Stream newstream, Stream oldstream)
        {

            using (var xlPackage = new ExcelPackage(newstream, oldstream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.ToList();
                //int n = 0;
                //Parallel.ForEach(worksheet, () => 0, (file, loopState, localCount) =>
                //{
                //    //action(file);
                //    return (int)++localCount;
                //},
                //(c) =>
                //{
                //    Interlocked.Exchange(ref n, n + c);
                //});

                Parallel.ForEach(worksheet,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 4

                    },
                    ProcessData
                    );
                /*
                foreach (var item in worksheet)
                {
                    if (item == null)
                        throw new Exception("No worksheet found");

                    var properties = new string[]
                    {
                        "name",
                        "Address",
                        "City",
                        "State",
                        "Zip",
                        "Location",
                        "Phone",
                        "Website",                       
                        "Email" ,
                        "Contac Person",
                        "Service",
                        "Hours Mon",
                        "Hours Tue",
                        "Hours Wed",
                        "Hours Thurs",
                        "Hours Fri",
                        "Hours Sat",
                        "Hours Sun",
                        "CompanyId",
                        "Status",
                        "ErrorMessage"
                                
                    };

                    int cusId = 0;

                    var cust = DAL.xGetCust("kuyam1@kuyam.com");
                    if (cust == null)
                    {
                        return;
                        //Cust custobj = new Cust
                        //{
                        //    FirstName = "kuyam",
                        //    LastName = "kuyam",
                        //    Street1 = string.Empty,
                        //    City = "Santa Monica",
                        //    Zip = "90401",
                        //    MobilePhone = string.Empty,
                        //    UserAcount = "kuyma@kuyam.com",
                        //    Password = "123456",
                        //    CompanyName = string.Empty

                        //};
                        //cusId = CreateCustomer(custobj);
                    }
                    else
                    {
                        cusId = cust.CustID;
                    }

                    int iRow = 2;
                    item.Cells[1, GetColumnIndex(properties, "CompanyId")].Value = "CompanyId";
                    item.Cells[1, GetColumnIndex(properties, "Status")].Value = "Status";
                    item.Cells[1, GetColumnIndex(properties, "ErrorMessage")].Value = "ErrorMessage";
                    while (true)
                    {
                        bool allColumnsAreEmpty = true;
                        for (var i = 1; i <= properties.Length; i++)
                            if (item.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(item.Cells[iRow, i].Value.ToString()))
                            {
                                allColumnsAreEmpty = false;
                                break;
                            }
                        if (allColumnsAreEmpty)
                            break;

                        var companyName = item.Cells[iRow, GetColumnIndex(properties, "name")].Value as string;
                        var address = item.Cells[iRow, GetColumnIndex(properties, "Address")].Value as string;
                        var city = item.Cells[iRow, GetColumnIndex(properties, "City")].Value as string;
                        var state = item.Cells[iRow, GetColumnIndex(properties, "State")].Value as string;
                        var zipCode = string.Empty;
                        try
                        {
                            zipCode = Convert.ToInt32(item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value).ToString();
                        }
                        catch (Exception)
                        {

                            zipCode = item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value as string;
                        }


                        var location = item.Cells[iRow, GetColumnIndex(properties, "Location")].Value as string;
                        var phoneNumber = item.Cells[iRow, GetColumnIndex(properties, "Phone")].Value as string;
                        var website = item.Cells[iRow, GetColumnIndex(properties, "Website")].Value as string;
                        var email = item.Cells[iRow, GetColumnIndex(properties, "Email")].Value as string;
                        //string userAccount = item.Cells[iRow, GetColumnIndex(properties, "User Account")].Value as string;
                        //string passWord = item.Cells[iRow, GetColumnIndex(properties, "Password")].Value.ToString();
                        var category = item.Cells[iRow, GetColumnIndex(properties, "Service")].Value as string;
                        //string service = item.Cells[iRow, GetColumnIndex(properties, "Service")].Value as string;

                        if (category != null)
                            category += "," + item.Name;
                        else
                            category = item.Name;

                        double lat = 0, lon = 0;
                        if (location != null && !string.IsNullOrEmpty(location))
                        {
                            string[] lc = location.Split(',');
                            double.TryParse(lc[0], out lat);
                            double.TryParse(lc[1], out lon);
                        }
                        try
                        {


                            if (cusId > 0)
                            {
                                var model = new CompanySetupModel
                                {
                                    CustID = cusId,
                                    Name = companyName,
                                    Street1 = address,
                                    City = city,
                                    State = state,
                                    Zip = zipCode,
                                    Email = email,
                                    Phone = UtilityHelper.CleanPhone(phoneNumber),
                                    Url = website,
                                    CompanyStatusID = 7,
                                    Latitude = lat,
                                    Longitude = lon
                                };
                                //string strService = service != null ? service : string.Empty;

                                var mon = item.Cells[iRow, GetColumnIndex(properties, "Hours Mon")].Value as string;
                                var tue = item.Cells[iRow, GetColumnIndex(properties, "Hours Tue")].Value as string;
                                var wed = item.Cells[iRow, GetColumnIndex(properties, "Hours Wed")].Value as string;
                                var thu = item.Cells[iRow, GetColumnIndex(properties, "Hours Thurs")].Value as string;
                                var fri = item.Cells[iRow, GetColumnIndex(properties, "Hours Fri")].Value as string;
                                var sat = item.Cells[iRow, GetColumnIndex(properties, "Hours Sat")].Value as string;
                                var sun = item.Cells[iRow, GetColumnIndex(properties, "Hours Sun")].Value as string;

                                string strHour = string.Empty;
                                if (mon != null)
                                {
                                    strHour += string.Format("mon,{0}", mon.Replace('-', ','));
                                }
                                if (tue != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("tue,{0}", tue.Replace('-', ','));
                                }
                                if (wed != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("wed,{0}", wed.Replace('-', ','));
                                }
                                if (thu != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("thu,{0}", thu.Replace('-', ','));
                                }
                                if (fri != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("fri,{0}", fri.Replace('-', ','));
                                }
                                if (sat != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("sat,{0}", sat.Replace('-', ','));
                                }
                                if (sun != null)
                                {
                                    if (strHour != string.Empty)
                                        strHour += "|";
                                    strHour += string.Format("sun,{0}", sun.Replace('-', ','));
                                }

                                string[] hours = strHour.Split('|');

                                CreateCompany(model, hours, category);

                                item.Cells[iRow, GetColumnIndex(properties, "CompanyId")].Value = model.ProfileID;
                                item.Cells[iRow, GetColumnIndex(properties, "Status")].Value = 1;


                            }

                        }
                        catch (Exception ex)
                        {
                            item.Cells[iRow, GetColumnIndex(properties, "Status")].Value = 0;
                            item.Cells[iRow, GetColumnIndex(properties, "ErrorMessage")].Value = ex.Message;
                            //throw;
                        }

                        //next company
                        iRow++;

                    }
                }
                */
                xlPackage.Save();
            }

        }


        private void ProcessData(ExcelWorksheet item)
        {
            var _companyProfileService = EngineContext.Current.Resolve<ExportImportService>();
            if (item == null)
                throw new Exception("No worksheet found");

            var properties = new string[]
                    {
                        "name",
                        "Address",
                        "City",
                        "State",
                        "Zip",
                        "Location",
                        "Phone",
                        "Website",                       
                        "Email" ,
                        "Contac Person",
                        "Service",
                        "Hours Mon",
                        "Hours Tue",
                        "Hours Wed",
                        "Hours Thurs",
                        "Hours Fri",
                        "Hours Sat",
                        "Hours Sun",
                        "CompanyId",
                        "Status",
                        "ErrorMessage"
                                
                    };

            int cusId = 0;

            var cust = DAL.xGetCust("kuyam1@kuyam.com");
            if (cust == null)
            {
                return;
                /*
                Cust custobj = new Cust
                {
                    FirstName = "kuyam",
                    LastName = "kuyam",
                    Street1 = string.Empty,
                    City = "Santa Monica",
                    Zip = "90401",
                    MobilePhone = string.Empty,
                    UserAcount = "kuyma@kuyam.com",
                    Password = "123456",
                    CompanyName = string.Empty

                };
                cusId = CreateCustomer(custobj);
                 */
            }

            cusId = cust.CustID;

            int iRow = 2;
            item.Cells[1, GetColumnIndex(properties, "CompanyId")].Value = "CompanyId";
            item.Cells[1, GetColumnIndex(properties, "Status")].Value = "Status";
            item.Cells[1, GetColumnIndex(properties, "ErrorMessage")].Value = "ErrorMessage";
            while (true)
            {
                bool allColumnsAreEmpty = true;
                for (var i = 1; i <= properties.Length; i++)
                    if (item.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(item.Cells[iRow, i].Value.ToString()))
                    {
                        allColumnsAreEmpty = false;
                        break;
                    }
                if (allColumnsAreEmpty)
                    break;

                var companyName = item.Cells[iRow, GetColumnIndex(properties, "name")].Value as string;
                var address = item.Cells[iRow, GetColumnIndex(properties, "Address")].Value as string;
                var city = item.Cells[iRow, GetColumnIndex(properties, "City")].Value as string;
                var state = item.Cells[iRow, GetColumnIndex(properties, "State")].Value as string;
                string zipCode;
                try
                {
                    zipCode = Convert.ToInt32(item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value).ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {

                    zipCode = item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value as string;
                }

                var location = item.Cells[iRow, GetColumnIndex(properties, "Location")].Value as string;
                var phoneNumber = item.Cells[iRow, GetColumnIndex(properties, "Phone")].Value as string;
                var website = item.Cells[iRow, GetColumnIndex(properties, "Website")].Value as string;
                var email = item.Cells[iRow, GetColumnIndex(properties, "Email")].Value as string;
                var contactPerson = item.Cells[iRow, GetColumnIndex(properties, "Contact Person")].Value as string;


                double lat = 0, lon = 0;
                if (location != null && !string.IsNullOrEmpty(location))
                {
                    string[] lc = location.Split(',');
                    double.TryParse(lc[0], out lat);
                    double.TryParse(lc[1], out lon);
                }
                try
                {
                    if (cusId > 0)
                    {
                        var model = new CompanySetupModel
                        {
                            CustID = cusId,
                            Name = companyName,
                            Street1 = address,
                            City = city,
                            State = state,
                            Zip = zipCode,
                            Email = email,
                            ContactName = contactPerson,
                            Phone = UtilityHelper.CleanPhone(phoneNumber),
                            Url = website,
                            CompanyStatusID = 7,
                            Latitude = lat,
                            Longitude = lon
                        };

                        if (!ValidateDataCompany(model))
                        {
                            item.Cells[iRow, GetColumnIndex(properties, "Status")].Value = 0;
                            item.Cells[iRow, GetColumnIndex(properties, "ErrorMessage")].Value = "Company Name is Require";
                        }

                        var categoryName = item.Name.Trim().ToLower();

                        int parentId = 0;
                        List<Service> listCategoryService = _companyProfileService.GetListServiceByCategory(categoryName, out parentId);

                        bool isAddnewCategory = false;

                        if (parentId == 0 && isAddnewCategory)
                        {
                            var servcie = new Service()
                            {
                                ParentServiceID = null,
                                ServiceName = categoryName,
                                Desc = string.Empty
                            };
                            _companyProfileService.InsertService(servcie);
                            parentId = servcie.ServiceID;
                        }

                        if (parentId > 0)
                        {
                            var services = item.Cells[iRow, GetColumnIndex(properties, "Service")].Value as string;

                            if (string.IsNullOrEmpty(services))
                            {
                                string[] listservice = services.Trim().Split(',');

                                foreach (string service in listservice)
                                {
                                    var serviceName = listCategoryService.FirstOrDefault(m => m.ServiceName.ToLower() == service.ToLower());
                                    int serviceId = 0;
                                    if (serviceName == null)
                                    {
                                        var servcie = new Service()
                                        {
                                            ParentServiceID = parentId,
                                            ServiceName = service.Trim().ToLower(),
                                            Desc = string.Empty
                                        };
                                        _companyProfileService.InsertService(servcie);
                                        serviceId = servcie.ServiceID;
                                    }

                                    var serviceCompany = new ServiceCompany
                                    {
                                        ServiceID = serviceId,
                                        Status = (int)Types.ServiceCompanyStatus.Active,
                                        Created = DateTime.UtcNow,
                                        Modified = DateTime.UtcNow

                                    };
                                    model.serviceCompany.Add(serviceCompany);
                                }

                            }

                        }

                        var mon = item.Cells[iRow, GetColumnIndex(properties, "Hours Mon")].Value as string;
                        var tue = item.Cells[iRow, GetColumnIndex(properties, "Hours Tue")].Value as string;
                        var wed = item.Cells[iRow, GetColumnIndex(properties, "Hours Wed")].Value as string;
                        var thu = item.Cells[iRow, GetColumnIndex(properties, "Hours Thurs")].Value as string;
                        var fri = item.Cells[iRow, GetColumnIndex(properties, "Hours Fri")].Value as string;
                        var sat = item.Cells[iRow, GetColumnIndex(properties, "Hours Sat")].Value as string;
                        var sun = item.Cells[iRow, GetColumnIndex(properties, "Hours Sun")].Value as string;

                        string strHour = string.Empty;
                        if (mon != null)
                        {
                            strHour += string.Format("mon,{0}", mon.Replace('-', ','));
                        }
                        if (tue != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("tue,{0}", tue.Replace('-', ','));
                        }
                        if (wed != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("wed,{0}", wed.Replace('-', ','));
                        }
                        if (thu != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("thu,{0}", thu.Replace('-', ','));
                        }
                        if (fri != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("fri,{0}", fri.Replace('-', ','));
                        }
                        if (sat != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("sat,{0}", sat.Replace('-', ','));
                        }
                        if (sun != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("sun,{0}", sun.Replace('-', ','));
                        }

                        string[] hours = strHour.Split('|');

                       // CreateCompany(_companyProfileService, model, hours, category);

                        item.Cells[iRow, GetColumnIndex(properties, "CompanyId")].Value = model.ProfileID;
                        item.Cells[iRow, GetColumnIndex(properties, "Status")].Value = 1;


                    }

                }
                catch (Exception ex)
                {
                    item.Cells[iRow, GetColumnIndex(properties, "Status")].Value = 0;
                    item.Cells[iRow, GetColumnIndex(properties, "ErrorMessage")].Value = ex.Message;
                    //throw;
                }

                //next company
                iRow++;

            }

        }

        private bool ValidateDataCompany(CompanySetupModel model)
        {
            if (model != null && string.IsNullOrEmpty(model.Name))
                return false;
            return true;
        }



        private void InserHour(ExportImportService _companyProfileService, Profile profile, IEnumerable<string> hours)
        {
            List<CompanyHour> lstProfileHourOld = _companyProfileService.GetCompanyHourProfileID(profile.ProfileID);
            if (profile.ProfileID != 0)
            {
                var lstProfileHourNew = new List<CompanyHour>();
                foreach (string item in hours)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        string strday = string.Empty;
                        string fromday = string.Empty;
                        string today = string.Empty;
                        int day = 0;
                        TimeSpan start;
                        TimeSpan end;
                        try
                        {
                            strday = item.Split(',')[0].Trim();
                            fromday = item.Split(',')[1].Trim();
                            today = item.Split(',')[2].Trim();
                            start = DateTime.Parse(fromday, CultureInfo.InvariantCulture).TimeOfDay;
                            end = DateTime.Parse(today, CultureInfo.InvariantCulture).TimeOfDay;
                        }
                        catch (Exception)
                        {

                            throw new InvalidDataException("Invalid format hour on :" + strday);
                        }


                        switch (strday)
                        {
                            case "mon": { day = (int)Types.Day.Monday; break; }
                            case "tue": { day = (int)Types.Day.Tuesday; break; }
                            case "wed": { day = (int)Types.Day.Wednesday; break; }
                            case "thu": { day = (int)Types.Day.Thursday; break; }
                            case "fri": { day = (int)Types.Day.Friday; break; }
                            case "sat": { day = (int)Types.Day.Saturday; break; }
                            case "sun": { day = (int)Types.Day.Sunday; break; }
                            case "isdaily": { day = (int)Types.Day.Isdaily; break; }
                        }
                        var profileHour = new CompanyHour()
                        {
                            ProfileCompanyID = profile.ProfileID,
                            DayOfWeek = day,
                            FromHour = start,
                            ToHour = end

                        };
                        if (day == (int)Types.Day.Isdaily)
                        {
                            profileHour.IsDaily = true;
                        }
                        lstProfileHourNew.Add(profileHour);
                    }
                }

                _companyProfileService.UpdateCompanyHour(lstProfileHourNew, lstProfileHourOld);


            }
            else
            {

                _companyProfileService.UpdateCompanyHour(null, lstProfileHourOld);


            }

        }


        #region test
        /*
        public void ImportZipCodeXlsx(Stream stream)
        {

            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.ToList();
                foreach (var item in worksheet)
                {
                    if (item == null)
                        throw new Exception("No worksheet found");

                    var properties = new string[]
                    {
                        "name",
                        "Address",
                        "City",
                        "State",
                        "Zip",
                        "Location",
                        "Phone",
                        "Website",                       
                        "Email" ,
                        "Contac Person",
                        "CATEGORY",
                        "Hours Mon",
                        "Hours Tue",
                        "Hours Wed",
                        "Hours Thurs",
                        "Hours Fri",
                        "Hours Sat",
                        "Hours Sun"       
                                
                    };


                    int iRow = 2;
                    while (true)
                    {
                        bool allColumnsAreEmpty = true;
                        for (var i = 1; i <= properties.Length; i++)
                            if (item.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(item.Cells[iRow, i].Value.ToString()))
                            {
                                allColumnsAreEmpty = false;
                                break;
                            }
                        if (allColumnsAreEmpty)
                            break;

                        string city = item.Cells[iRow, GetColumnIndex(properties, "City")].Value as string;
                        string state = item.Cells[iRow, GetColumnIndex(properties, "State")].Value as string;
                        string zipCode = string.Empty;
                        try
                        {
                            zipCode = Convert.ToInt32(item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value).ToString();
                        }
                        catch (Exception)
                        {

                            zipCode = item.Cells[iRow, GetColumnIndex(properties, "Zip")].Value as string;
                        }

                        var zipcodeList = _companyProfileService.GetZipCodeAll();

                        if (!zipcodeList.Any(m => m.Code == zipCode))
                        {
                            var zipcode = new ZipCode
                            {
                                City = city,
                                State = state,
                                Code = zipCode,
                                Active = true

                            };
                            _companyProfileService.InsertZipcode(zipcode);
                        }

                        //next company
                        iRow++;
                    }


                }

            }
        }

        public void ImportHourXlsx(Stream stream)
        {

            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.ToList();
                foreach (var item in worksheet)
                {
                    if (item == null)
                        throw new Exception("No worksheet found");

                    var properties = new string[]
                    {
                        "name",
                        "Address",
                        "City",
                        "State",
                        "Zip",
                        "Location",
                        "Phone",
                        "Website",                       
                        "Email" ,
                        "Contac Person",
                        "CATEGORY",
                        "Hours Mon",
                        "Hours Tue",
                        "Hours Wed",
                        "Hours Thurs",
                        "Hours Fri",
                        "Hours Sat",
                        "Hours Sun"                      
                                
                    };


                    int iRow = 2;
                    while (true)
                    {
                        bool allColumnsAreEmpty = true;
                        for (var i = 1; i <= properties.Length; i++)
                            if (item.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(item.Cells[iRow, i].Value.ToString()))
                            {
                                allColumnsAreEmpty = false;
                                break;
                            }
                        if (allColumnsAreEmpty)
                            break;

                        string companyName = item.Cells[iRow, GetColumnIndex(properties, "name")].Value as string;
                        string city = item.Cells[iRow, GetColumnIndex(properties, "City")].Value as string;
                        var profile = _companyProfileService.GetProfileByNameAndCity(companyName);

                        string mon = item.Cells[iRow, GetColumnIndex(properties, "Hours Mon")].Value as string;
                        string tue = item.Cells[iRow, GetColumnIndex(properties, "Hours Tue")].Value as string;
                        string wed = item.Cells[iRow, GetColumnIndex(properties, "Hours Wed")].Value as string;
                        string thu = item.Cells[iRow, GetColumnIndex(properties, "Hours Thurs")].Value as string;
                        string fri = item.Cells[iRow, GetColumnIndex(properties, "Hours Fri")].Value as string;
                        string sat = item.Cells[iRow, GetColumnIndex(properties, "Hours Sat")].Value as string;
                        string sun = item.Cells[iRow, GetColumnIndex(properties, "Hours Sun")].Value as string;

                        string strHour = string.Empty;
                        if (mon != null)
                        {
                            strHour += string.Format("mon,{0}", mon.Replace('-', ','));
                        }
                        if (tue != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("tue,{0}", tue.Replace('-', ','));
                        }
                        if (wed != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("wed,{0}", wed.Replace('-', ','));
                        }
                        if (thu != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("thu,{0}", thu.Replace('-', ','));
                        }
                        if (fri != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("fri,{0}", fri.Replace('-', ','));
                        }
                        if (sat != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("sat,{0}", sat.Replace('-', ','));
                        }
                        if (sun != null)
                        {
                            if (strHour != string.Empty)
                                strHour += "|";
                            strHour += string.Format("sun,{0}", sun.Replace('-', ','));
                        }

                        string[] hours = strHour.Split('|');

                        if (profile != null)
                            InserHour(profile, hours);

                        //next company
                        iRow++;
                    }


                }

            }
        }

        */
        #endregion end test


        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportCompany()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportCompany(FormCollection form)
        {
            var file = Request.Files["importexcelfile"];
            if (file != null && file.ContentLength > 0)
            {
                var fileStream = file.InputStream;
                byte[] bytes = null;

                using (var stream = new MemoryStream())
                {
                    //fileStream.CopyTo(stream);
                    ImportCompanyXlsx(stream, fileStream);
                    bytes = stream.ToArray();
                }

                return File(bytes, "text/xls", "files.xlsx");


                //ImportZipCodeXlsx(files);
                //ImportHourXlsx(files);

            }
            return View();
        }
    }
}
