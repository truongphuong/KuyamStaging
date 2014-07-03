using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Kuyam.Utility;
using M2.Util;
using OfficeOpenXml;
using System.IO;
using System.Web.Mvc;
using Kuyam.Database;
using System.Web.Security;
using PushSharp.Core;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.Domain
{

    public class ImportService
    {
        #region "const"
        private const string COMPANY_PROFILE_COMAPANYNAME = "Name";
        private const string COMPANY_PROFILE_ADDRESS = "Address";
        private const string COMPANY_PROFILE_ADDRESS1 = "Address1";
        private const string COMPANY_PROFILE_CITY = "City";
        private const string COMPANY_PROFILE_STATE = "State";
        private const string COMPANY_PROFILE_ZIP = "Zipcode";
        private const string COMPANY_PROFILE_LOCATION = "Location";
        private const string COMPANY_PROFILE_PHONE = "Phone number";
        private const string COMPANY_PROFILE_WEBSITE = "Website";
        private const string COMPANY_PROFILE_EMAIL = "Email";
        private const string COMPANY_PROFILE_PAYMENTMETHOD = "PaymentMethod";
        private const string COMPANY_PROFILE_CONTACTPERSON = "Contact Person";
        private const string COMPANY_PROFILE_SERVICE = "Service";
        private const string COMPANY_PROFILE_HOURSMON = "Hours Mon";
        private const string COMPANY_PROFILE_HOURSTUE = "Hours Tue";
        private const string COMPANY_PROFILE_HOURSWED = "Hours Wed";
        private const string COMPANY_PROFILE_HOURSTHURS = "Hours Thurs";
        private const string COMPANY_PROFILE_HOURSFRI = "Hours Fri";
        private const string COMPANY_PROFILE_HOURSSAT = "Hours Sat";
        private const string COMPANY_PROFILE_HOURSSUN = "Hours Sun";
        private const string BREAK_LINE = "\r\n";
        #endregion
        private readonly CompanyProfileService _companyProfileService;
        private readonly AdminService _adminService;
        public ImportService(CompanyProfileService companyProfileService,
            AdminService adminService)
        {
            this._companyProfileService = companyProfileService;
            this._adminService = adminService;
        }

        #region "import data"
        protected int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            if (columnName == null)
                throw new ArgumentNullException("columnName");

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].ToUpper().Equals(columnName.ToUpper(), StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }

        public void ImportCompanyXlsx(Stream stream)
        {

            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new Exception("No worksheet found");

                //the columns
                var properties = new string[]
                                 {
                                     "Company name",
                                     "Address",
                                     "City",
                                     "Zip",
                                     "Phone",
                                     "Website",
                                     "Email",
                                     "Category",
                                     "Service",
                                     "Mon Open",
                                     "Mon Close",
                                     "Tue Open",
                                     "Tue Close",
                                     "Wed Open",
                                     "Wed Close",
                                     "Thurs Open",
                                     "Thurs Close",
                                     "Fri Open",
                                     "Fri Close",
                                     "Sat Open",
                                     "Sat Close",
                                     "Sun Open",
                                     "Sun Close"

                                 };


                int iRow = 2;
                while (true)
                {
                    bool allColumnsAreEmpty = true;
                    for (var i = 1; i <= properties.Length; i++)
                        if (worksheet.Cells[iRow, i].Value != null &&
                            !String.IsNullOrEmpty(worksheet.Cells[iRow, i].Value.ToString()))
                        {
                            allColumnsAreEmpty = false;
                            break;
                        }
                    if (allColumnsAreEmpty)
                        break;

                    string companyName =
                        worksheet.Cells[iRow, GetColumnIndex(properties, "Company name")].Value as string;
                    string address = worksheet.Cells[iRow, GetColumnIndex(properties, "Address")].Value as string;
                    string city = worksheet.Cells[iRow, GetColumnIndex(properties, "City")].Value as string;
                    string zipCode =
                        Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "Zip")].Value).ToString();
                    string phoneNumber = worksheet.Cells[iRow, GetColumnIndex(properties, "Phone")].Value as string;
                    string website = worksheet.Cells[iRow, GetColumnIndex(properties, "Website")].Value as string;
                    string email = worksheet.Cells[iRow, GetColumnIndex(properties, "Email")].Value as string;


                    string category = worksheet.Cells[iRow, GetColumnIndex(properties, "Category")].Value as string;
                    string service = worksheet.Cells[iRow, GetColumnIndex(properties, "Service")].Value as string;

                    string[] sevices = service.Split(',');

                    DateTime? monOpen = null;
                    var monOpenExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "Mon Open")].Value;
                    if (monOpenExcel != null)
                        monOpen = DateTime.Parse(monOpenExcel.ToString());

                    string monClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Mon Close")].Value.ToString();
                    string tueOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Tue Open")].Value.ToString();
                    string tueClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Tue Close")].Value.ToString();
                    string wedOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Wed Open")].Value.ToString();
                    string wedClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Wed Close")].Value.ToString();

                    string thursOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Thurs Open")].Value.ToString();
                    string thursClose =
                        worksheet.Cells[iRow, GetColumnIndex(properties, "Thurs Close")].Value.ToString();
                    string friOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Fri Open")].Value.ToString();
                    string friClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Fri Close")].Value.ToString();

                    string satOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Sat Open")].Value.ToString();
                    string satClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Sat Close")].Value.ToString();
                    string sunOpen = worksheet.Cells[iRow, GetColumnIndex(properties, "Sun Open")].Value.ToString();
                    string sunClose = worksheet.Cells[iRow, GetColumnIndex(properties, "Sun Close")].Value.ToString();

                    string name = email.Split('@')[0];
                    string invitecode = AccountHelper.AddInviteCode(email, name, name);

                    //next company
                    iRow++;
                }
            }
        }

        public string ImportCompanyToDatabase(string directoryPath)
        {
            var fileInfo = new FileInfo(directoryPath);

            List<string> errorExited = null;
            var sheets = GetAllWorkSheetAvailible(fileInfo, out errorExited);
            var sheetsAvailible = sheets[0];//list import
            var sheetsInAvailible = sheets[1];//list show error
            //Case categories is not existed
            List<string> errorInsert = new List<string>();
            List<string> errorsCheckFileExcel = null;
            var strResults = string.Empty;
            if (sheetsAvailible != null || sheetsAvailible.Any())
            {
                var companyImports = GetCompanyAvailible(sheetsAvailible, out errorsCheckFileExcel);
                var cust = DAL.xGetCust("kuyam1@kuyam.com");
                if (cust == null)
                    return string.Empty;
                var cusId = cust.CustID;
                var totalCompanyImport = companyImports.Count();
                var totalNoImport = companyImports.Count();
                foreach (var company in companyImports)
                {
                    var errorMessage = string.Empty;
                    InsertCompnay(company, cusId, out errorMessage);
                    if (!errorMessage.IsNullOrEmpty())
                    {
                        errorInsert.Add(errorMessage);
                    }
                    else
                    {
                        totalNoImport--;
                    }
                }
                strResults = "Sheet can import: ";
                var strSucc = string.Empty;
                foreach (var sheet in sheetsAvailible)
                {
                    strSucc += sheet.Name + ", ";
                }
                strResults = "sheet can import: " + strSucc + "total row import " + totalCompanyImport + ", total row can't import " +
                             totalNoImport + "<br/>";
                var strUnSucc = string.Empty;
                foreach (var nSheet in sheetsInAvailible)
                {
                    strUnSucc += nSheet.Name + ", ";
                }
                strResults = strResults + "sheet can't import: " + strUnSucc.Trim().TrimEnd(new char[] { ',' });
            }
            var strError = string.Empty;
            if (errorExited != null && errorExited.Any())
                strError += GetErrorFromList(errorExited) + BREAK_LINE;
            if (errorsCheckFileExcel != null && errorsCheckFileExcel.Any())
                strError += GetErrorFromList(errorsCheckFileExcel) + BREAK_LINE;
            if (errorInsert != null && errorInsert.Any())
                strError += GetErrorFromList(errorInsert) + BREAK_LINE;
            if (string.IsNullOrEmpty(strError))
                LogHelper.ImportExportCompanyInfo("Import completed. No error");
            else
                LogHelper.ImportExportCompanyError(strError);
            return strResults;
        }

        public string GetErrorFromList(List<string> errors)
        {
            string strError = string.Empty;
            foreach (var err in errors)
            {
                var tempError = err + BREAK_LINE;
                strError += tempError;
            }
            return strError;
        }
        public void InsertCompnay(ProfileCompany profileCompany, int cusId, out string errorMessage)
        {
            errorMessage = string.Empty;
            var profile = new Profile
            {
                CustID = cusId,
                PrivacyTypeID = (int)Types.PrivacyType.Private,
                RelationshipTypeID = (int)Types.RelationshipType.Company,
                ProfileTypeID = 116,
                Name = profileCompany.Name,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                ProfileCompany = profileCompany
            };
            try
            {
                //insert profile table
                //insert profile table
                //insert profile companye table
                //insert company hours table
                //insert services company table
                _companyProfileService.InsertProfile(profile);

            }
            catch (Exception exception)
            {
                var err = "Company: " + profile.Name + "can't insert because: " + exception.StackTrace.ToString() + BREAK_LINE;
                errorMessage = err;
            }
        }

        public List<List<ExcelWorksheet>> GetAllWorkSheetAvailible(FileInfo fileInfo, out List<string> errors)
        {
            var listWorkSheet = new List<List<ExcelWorksheet>>();
            var categories = GetCatagories();
            errors = new List<string>();
            using (var xlPackage = new ExcelPackage(fileInfo))
            {
                var worsheets = xlPackage.Workbook.Worksheets.ToList();
                var worsheetNotAvailible = xlPackage.Workbook.Worksheets.ToList();
                var worsheetAvailible = xlPackage.Workbook.Worksheets.ToList();
                if (categories == null)
                {
                    worsheetNotAvailible.Clear();
                }
                else
                {
                    foreach (var excelWorksheet in worsheets)
                    {
                        var workSheetName = excelWorksheet.Name;
                        var correctFormat = CheckExcelCorrectFormat(excelWorksheet);
                        if (correctFormat)
                        {
                            if (categories.Any(a => a.ServiceName.Trim().ToUpper() == workSheetName.Trim().ToUpper()))
                            {
                                worsheetNotAvailible.Remove(excelWorksheet);
                            }
                            else
                            {
                                worsheetAvailible.Remove(excelWorksheet);
                                string error = "Category name: " + excelWorksheet.Name + " is not existed" + BREAK_LINE;
                                errors.Add(error);
                            }
                        }
                        else
                        {
                            worsheetAvailible.Remove(excelWorksheet);
                            string error = "Category name: " + excelWorksheet.Name + " wrong format" + BREAK_LINE;
                            errors.Add(error);
                        }
                    }
                }

                listWorkSheet.Add(worsheetAvailible);
                listWorkSheet.Add(worsheetNotAvailible);
            }
            return listWorkSheet;
        }

        public List<ProfileCompany> GetCompanyAvailible(List<ExcelWorksheet> excelWorksheets, out List<string> listError)
        {
            var profileCompanies = _companyProfileService.GetAllProfileCompany();
            if (profileCompanies == null || !profileCompanies.Any())
            {
                listError = null;
                return null;
            }
            listError = new List<string>();
            var listImport = new List<ProfileCompany>();

            var properties = GetProperties();
            foreach (var excelWorksheet in excelWorksheets)
            {
                var iRow = 2;
                while (true)
                {
                    #region "Check end of file"
                    bool allColumnsAreEmpty = true;
                    for (var i = 1; i <= properties.Length; i++)
                        if (excelWorksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(excelWorksheet.Cells[iRow, i].Value.ToString()))
                        {
                            allColumnsAreEmpty = false;
                            break;
                        }
                    #endregion
                    #region "No end of file"
                    if (!allColumnsAreEmpty)
                    {
                        //get convert row to profile company object
                        string msg = string.Empty;
                        var profileCompany = ConvertRowWorkSheetToObjectProfileCompany(iRow, excelWorksheet, out msg);
                        if (!string.IsNullOrEmpty(msg))
                            listError.Add(msg);
                        if (profileCompany != null)
                        {
                            //check row data ready 
                            var errMessage = CheckProfileCorrect(profileCompany);
                            if (!string.IsNullOrEmpty(errMessage)) //row does not ready
                            {
                                errMessage = "Category name: " + excelWorksheet.Name + " ,row " + iRow +
                                             ", company name:" + profileCompany.Name + " " + errMessage +
                                             BREAK_LINE;
                                listError.Add(errMessage);
                            }
                            else //row data ready
                            {
                                //check profile company existed
                                if (!CheckCompanyExisted(profileCompanies, profileCompany, excelWorksheet.Name))
                                //profile company not existed
                                {
                                    listImport.Add(profileCompany);
                                }
                                else //profile company existed
                                {
                                    var error = "Category name: " + excelWorksheet.Name + " ,row " + iRow +
                                                ", company name:" + profileCompany.Name +
                                                " existed!" + BREAK_LINE;
                                    listError.Add(error);
                                }
                            }
                        }
                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                    #endregion

                }
            }

            return listImport;
        }

        public bool CheckExcelCorrectFormat(ExcelWorksheet excelWorksheet)
        {
            var properties = GetProperties();
            var iRow = 1;
            for (var i = 1; i <= properties.Length; i++)
                if (excelWorksheet.Cells[iRow, i].Value.ToString().ToUpper() != properties[i - 1].ToUpper())
                {
                    return false;
                }
            return true;
        }
        public string[] GetProperties()
        {
            return new string[]
                                 {
                                     COMPANY_PROFILE_COMAPANYNAME,
                                     COMPANY_PROFILE_ADDRESS,
                                     COMPANY_PROFILE_CITY,
                                     COMPANY_PROFILE_STATE,
                                     COMPANY_PROFILE_ZIP,
                                     COMPANY_PROFILE_LOCATION,
                                     COMPANY_PROFILE_PHONE,
                                     COMPANY_PROFILE_WEBSITE,
                                     COMPANY_PROFILE_EMAIL,
                                     COMPANY_PROFILE_PAYMENTMETHOD,
                                     COMPANY_PROFILE_CONTACTPERSON,
                                     COMPANY_PROFILE_HOURSMON,
                                     COMPANY_PROFILE_HOURSTUE,
                                     COMPANY_PROFILE_HOURSWED,
                                     COMPANY_PROFILE_HOURSTHURS,
                                     COMPANY_PROFILE_HOURSFRI,
                                     COMPANY_PROFILE_HOURSSAT,
                                     COMPANY_PROFILE_HOURSSUN
                                 };
        }
        public ProfileCompany ConvertRowWorkSheetToObjectProfileCompany(int iRow, ExcelWorksheet worksheet, out string msg)
        {
            var profileCompany = new ProfileCompany();
            msg = string.Empty;
            try
            {
                var properties = GetProperties();
                string companyName =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_COMAPANYNAME)].Value.ToString();
                string address =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ADDRESS)].Value as string;
                //string address1 =
                //    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ADDRESS1)].Value as string;
                string city = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CITY)].Value.ToString();
                string state = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_STATE)].Value.ToString();
                //string zipCode =
                //    Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value).ToString();
                string zipCode;
                try
                {
                    zipCode =
                        Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value)
                            .ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    zipCode = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value as string;
                }
                string location =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_LOCATION)].Value as string;
                string phoneNumber =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_PHONE)].Value as string;
                string website =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_WEBSITE)].Value as string;
                string email = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_EMAIL)].Value as string;
                //string service =
                //    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_SERVICE)].Value as string;

                var pmThod = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_PAYMENTMETHOD)].Value;
                string paymentMethod = pmThod != null ? pmThod.ToString() : null;
                string contactPerson =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CONTACTPERSON)].Value as string;

                string monHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSMON)].Value as string;
                string tueHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSTUE)].Value as string;
                string wedHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSWED)].Value as string;
                string thursHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSTHURS)].Value as string;
                string friHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSFRI)].Value as string;
                string satHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSSAT)].Value as string;
                string sunHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSSUN)].Value as string;


                profileCompany.Name = companyName;
                profileCompany.CompanyTypeID = (int)Types.CompanyType.NonKuyamBookIt;
                profileCompany.CompanyStatusID = (int)Types.CompanyStatus.Active;
                profileCompany.Street1 = address;
                //profileCompany.Street2 = address1;
                profileCompany.City = city;
                profileCompany.State = state;
                profileCompany.Zip = zipCode;
                profileCompany.Phone = !string.IsNullOrEmpty(phoneNumber) ? Kuyam.Domain.UtilityHelper.CleanPhone(phoneNumber) : null;
                profileCompany.Email = email;
                if (!string.IsNullOrEmpty(paymentMethod))
                    profileCompany.PaymentMethod = int.Parse(paymentMethod);
                profileCompany.ContactName = contactPerson;
                profileCompany.Created = DateTime.UtcNow;
                profileCompany.ApptDefaultPeoplePerSlot = 1;
                profileCompany.ApptDefaultSlotDuration = 60;
                profileCompany.ApptAutoConfirm = true;
                profileCompany.Url = website;
                if (!string.IsNullOrEmpty(location))
                {
                    var local = location.Split(new char[] { ',' });
                    if (local.Any() && local.Count() == 2)
                    {
                        profileCompany.Latitude = double.Parse(local[0]);
                        profileCompany.Longitude = double.Parse(local[1]);
                    }
                    else
                    {
                        var msg1 = "Category name:" + worksheet.Name + " ,row: " + iRow + " location incorrect format (lat,lng)" + BREAK_LINE;
                        LogHelper.ImportExportCompanyError(msg1);
                        msg = msg1;
                        return null;
                    }

                }
                else
                {
                    profileCompany.Latitude = -1;
                    profileCompany.Longitude = -1;
                }
                profileCompany.CompanyHours = new Collection<CompanyHour>();
                if (!string.IsNullOrEmpty(monHours) &&
                    !string.IsNullOrEmpty(tueHours) &&
                    !string.IsNullOrEmpty(wedHours) &&
                    !string.IsNullOrEmpty(thursHours) &&
                    !string.IsNullOrEmpty(friHours) &&
                    !string.IsNullOrEmpty(satHours) &&
                    !string.IsNullOrEmpty(sunHours)
                    && monHours.Trim().ToUpper() == tueHours.Trim().ToUpper()
                    && tueHours.Trim().ToUpper() == wedHours.Trim().ToUpper()
                    && wedHours.Trim().ToUpper() == thursHours.Trim().ToUpper()
                    && thursHours.Trim().ToUpper() == friHours.Trim().ToUpper()
                    && friHours.Trim().ToUpper() == satHours.Trim().ToUpper()
                    && satHours.Trim().ToUpper() == sunHours.Trim().ToUpper()
                    && sunHours.Trim().ToUpper() == monHours.Trim().ToUpper())
                {
                    var companyHour = CreateCompanyHour(profileCompany.ProfileID, monHours, (int)Types.Day.Isdaily);
                    if (companyHour != null)
                    {
                        companyHour.IsDaily = true;
                        profileCompany.CompanyHours.Add(companyHour);
                    }
                    //var mon = monHours.Split(new char[] {'-'});
                    //if (mon.Any() && mon.Count() == 2)
                    //{
                    //    var fromDate = DateTime.Parse("01/20/2014 " + mon[0]);
                    //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                    //    companyHour.FromHour = fromHour;
                    //    var toDate = DateTime.Parse("01/20/2014 " + mon[1]);
                    //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                    //    companyHour.ToHour = toHour;
                    //    companyHour.DayOfWeek = 1234560;
                    //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                    //    companyHour.IsDaily = true;
                    //    profileCompany.CompanyHours.Add(companyHour);
                    //}
                }
                else
                {
                    #region "insert hour"

                    if (!string.IsNullOrEmpty(monHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, monHours, (int)Types.Day.Monday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var mon = monHours.Split(new char[] {'-'});
                        //if (mon.Any()&& mon.Count() == 2)
                        //{
                        //    var fromDate = DateTime.Parse("01/20/2014 " + mon[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + mon[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Monday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }
                    if (!string.IsNullOrEmpty(tueHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, tueHours, (int)Types.Day.Tuesday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var tue = tueHours.Split(new char[] {'-'});
                        //if (tue.Any() && tue.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + tue[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + tue[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int)Types.Day.Tuesday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}

                    }
                    if (!string.IsNullOrEmpty(wedHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, wedHours, (int)Types.Day.Wednesday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var wed = wedHours.Split(new char[] {'-'});
                        //if (wed.Any() && wed.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + wed[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + wed[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Wednesday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }
                    if (!string.IsNullOrEmpty(thursHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, thursHours, (int)Types.Day.Thursday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var thurs = thursHours.Split(new char[] {'-'});
                        //if (thurs.Any() && thurs.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + thurs[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + thurs[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Thursday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }
                    if (!string.IsNullOrEmpty(friHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, friHours, (int)Types.Day.Friday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var fri = friHours.Split(new char[] {'-'});
                        //if (fri.Any() && fri.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + fri[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + fri[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Friday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }
                    if (!string.IsNullOrEmpty(satHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, satHours, (int)Types.Day.Saturday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var sat = satHours.Split(new char[] {'-'});
                        //if (sat.Any() && sat.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + sat[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + sat[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Saturday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }
                    if (!string.IsNullOrEmpty(sunHours))
                    {
                        var companyHour = CreateCompanyHour(profileCompany.ProfileID, sunHours, (int)Types.Day.Sunday);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                        //var sun = sunHours.Split(new char[] {'-'});
                        //if (sun.Any() && sun.Count() == 2)
                        //{
                        //    companyHour = new CompanyHour();
                        //    var fromDate = DateTime.Parse("01/20/2014 " + sun[0]);
                        //    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                        //    companyHour.FromHour = fromHour;
                        //    var toDate = DateTime.Parse("01/20/2014 " + sun[1]);
                        //    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                        //    companyHour.ToHour = toHour;
                        //    companyHour.DayOfWeek = (int) Types.Day.Sunday;
                        //    companyHour.ProfileCompanyID = profileCompany.ProfileID;
                        //    profileCompany.CompanyHours.Add(companyHour);
                        //}
                    }

                    #endregion
                }
                var ser = _companyProfileService.GeServiceByName(worksheet.Name);
                var serviceCompany = new ServiceCompany();
                serviceCompany.ServiceID = ser.ServiceID;
                serviceCompany.Created = DateTime.UtcNow;
                serviceCompany.Status = (int)Types.ServiceCompanyStatus.Active;
                serviceCompany.Modified = DateTime.UtcNow;
                serviceCompany.ProfileID = profileCompany.ProfileID;
                profileCompany.ServiceCompanies.Add(serviceCompany);
            }
            catch (Exception ex)
            {
                var msg1 = "Category name:" + worksheet.Name + " ,row: " + iRow + " ,exception:" + ex.StackTrace.ToString() + BREAK_LINE;
                LogHelper.ImportExportCompanyError(msg1);
                msg = msg1;
                profileCompany = null;
            }
            return profileCompany;
        }


        public List<Service> GetCatagories()
        {
            try
            {
                return _companyProfileService.GetCategories();
            }
            catch (Exception exception)
            {
                LogHelper.Error("GetCatagories error: " + exception.StackTrace.ToUpper());
            }
            return null;
        }

        /// <summary>
        /// Check company existed in database
        /// </summary>
        /// <param name="profileCompanies"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public bool CheckCompanyExisted(List<ProfileCompany> profileCompanies, ProfileCompany company, string categoryName)
        {
            return profileCompanies.Any(a => !string.IsNullOrEmpty(a.Name) &
                !string.IsNullOrEmpty(a.Address) &
                !string.IsNullOrEmpty(a.Street1) &
                !string.IsNullOrEmpty(a.City) &
                !string.IsNullOrEmpty(a.State) &
                !string.IsNullOrEmpty(a.Zip) &
                a.Name.Trim().ToUpper() == company.Name.Trim().ToUpper()
                &&
                a.Address.Trim().ToUpper() == company.Address.Trim().ToUpper()
                &&
                a.Street1.Trim().ToUpper() == company.Street1.Trim().ToUpper()
                &&
                a.City.Trim().ToUpper() == company.City.Trim().ToUpper()
                &&
                a.State.Trim().ToUpper() == company.State.Trim().ToUpper()
                &&
                a.Zip.Trim().ToUpper() == company.Zip.Trim().ToUpper()
                );
        }

        /// <summary>
        /// Check profile correct data
        /// </summary>
        /// <param name="profileCompany"></param>
        /// <returns></returns>
        public string CheckProfileCorrect(ProfileCompany profileCompany)
        {
            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(profileCompany.Name))
                errorMessage = "Company name can't blank";
            if (string.IsNullOrEmpty(profileCompany.Street1))
            {
                errorMessage = "Address can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.City))
            {
                errorMessage = "City can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.State))
            {
                errorMessage = "State can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.Zip))
            {
                errorMessage = "Zip can't blank" + BREAK_LINE;
            }
            //if (string.IsNullOrEmpty(profileCompany.Phone))
            //{
            //    errorMessage = "Phone can't blank" + BREAK_LINE;
            //}
            //if (string.IsNullOrEmpty(profileCompany.Email))
            //{
            //    errorMessage = "Email can't blank" + BREAK_LINE;
            //}
            if (profileCompany.Latitude == -1 || profileCompany.Longitude == -1)
            {
                errorMessage = "Location can't blank" + BREAK_LINE;
            }
            return errorMessage;
        }

        public CompanyHour CreateCompanyHour(int profileCompanyID, string strTime, int dayofWeek)
        {
            try
            {
                var time = strTime.Split(new char[] { '-' });
                if (time.Any() && time.Count() == 2)
                {
                    var companyHour = new CompanyHour();
                    var fromDate = DateTime.Parse("01/20/2014 " + time[0]);
                    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                    var toDate = DateTime.Parse("01/20/2014 " + time[1]);
                    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                    if (fromDate > toDate)
                    {
                        var msg = "Day of week: " + dayofWeek + " have start date > end date";
                        LogHelper.ImportExportCompanyError(msg);

                        return null;
                    }
                    else
                    {
                        companyHour.ToHour = toHour;
                        companyHour.FromHour = fromHour;
                        companyHour.DayOfWeek = dayofWeek;
                        companyHour.ProfileCompanyID = profileCompanyID;
                        return companyHour;
                    }

                }
            }
            catch (Exception ex)
            {
                var msg = "Date format wrong:" + strTime;
                LogHelper.ImportExportCompanyError(msg);
                return null;
            }
            return null;

        }

        public List<ProfileCompanyTemp> GetCompanies(List<ExcelWorksheet> excelWorksheets, out List<string> listError)
        {
            listError = new List<string>();
            try
            {
                var profileCompanies = _companyProfileService.GetAllProfileCompany();
                if (profileCompanies == null || !profileCompanies.Any())
                {
                    listError = null;
                    return null;
                }

                var listImport = new List<ProfileCompanyTemp>();

                var properties = GetProperties();
                foreach (var excelWorksheet in excelWorksheets)
                {
                    var iRow = 2;
                    while (true)
                    {
                        #region "Check end of file"
                        bool allColumnsAreEmpty = true;
                        for (var i = 1; i <= properties.Length; i++)
                            if (excelWorksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(excelWorksheet.Cells[iRow, i].Value.ToString()))
                            {
                                allColumnsAreEmpty = false;
                                break;
                            }
                        #endregion
                        #region "No end of file"
                        if (!allColumnsAreEmpty)
                        {
                            //get convert row to profile company object
                            string msg = string.Empty;
                            var profileCompany = ConvertRowWorkSheetToObjectProfileCompanyTemp(iRow, excelWorksheet, out msg);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                listError.Add(msg);
                                profileCompany.Status = 0;
                                profileCompany.ErrorMessage += " " + msg;
                                listImport.Add(profileCompany);
                            }
                            if (profileCompany != null)
                            {
                                //check row data ready 
                                var errMessage = CheckProfileCompanyTempCorrect(profileCompany);
                                if (!string.IsNullOrEmpty(errMessage)) //row does not ready
                                {
                                    errMessage = "Category name: " + excelWorksheet.Name + " ,row " + iRow +
                                                 ", company name:" + profileCompany.Name + " " + errMessage +
                                                 BREAK_LINE;
                                    listError.Add(errMessage);
                                    profileCompany.Status = 0;
                                    profileCompany.ErrorMessage += " " + errMessage;
                                    listImport.Add(profileCompany);
                                }
                                else //row data ready
                                {
                                    //check profile company existed
                                    if (!CheckCompanyTempExisted(profileCompanies, profileCompany))
                                    //profile company not existed
                                    {
                                        profileCompany.Status = 1;
                                        listImport.Add(profileCompany);
                                    }
                                    else //profile company existed
                                    {
                                        var error = "Category name: " + excelWorksheet.Name + " ,row " + iRow +
                                                    ", company name:" + profileCompany.Name +
                                                    " existed!" + BREAK_LINE;
                                        listError.Add(error);
                                        profileCompany.Status = 0;
                                        profileCompany.ErrorMessage += " " + "comany existed";
                                        listImport.Add(profileCompany);
                                    }
                                }
                            }
                            iRow++;
                        }
                        else
                        {
                            break;
                        }
                        #endregion

                    }
                }

                return listImport;
            }
            catch (Exception ex)
            {
                listError.Add(ex.Message);
                return null;
            }
        }
        public ProfileCompanyTemp ConvertToProfileCompanyTemp(ProfileCompany profileCompany)
        {
            var companyHours = profileCompany.CompanyHours;
            var temp = new ProfileCompanyTemp
                       {
                           Name = profileCompany.Name,
                           ContactName = profileCompany.ContactName,
                           Street1 = profileCompany.Street1,
                           Street2 = profileCompany.Street2,
                           City = profileCompany.City,
                           State = profileCompany.State,
                           Zip = profileCompany.Zip,
                           Phone = profileCompany.Phone,
                           Email = profileCompany.Email,
                           Url = profileCompany.Url,
                           Latitude = profileCompany.Latitude,
                           Longitude = profileCompany.Longitude,
                           PaymentMethod = profileCompany.PaymentMethod,
                           Created = DateTime.Now
                       };
            if (companyHours != null)
            {
                if (companyHours.Count() == 1 && companyHours.Single().IsDaily == true)
                {
                    temp.HoursMon =
                        temp.HoursTue =
                            temp.HoursWed =
                                temp.HoursThur =
                                    temp.HoursFri =
                                        temp.HoursSat =
                                            temp.HoursSun =
                                                companyHours.Single().FromHour + "-" + companyHours.Single().ToHour;
                    temp.IsDaily = true;
                    temp.MonFrom = temp.TusFrom = temp.WedFrom = temp.ThurFrom = temp.FriFrom = temp.SatFrom = temp.SunFrom = companyHours.Single().FromHour.ToString();
                    temp.MonTo = temp.TusTo = temp.WedTo = temp.ThurTo = temp.FriTo = temp.SatTo = temp.SunTo = companyHours.Single().ToHour.ToString();
                }
                else
                {
                    temp.IsDaily = false;
                    foreach (var companyHour in companyHours)
                    {
                        if (companyHour.DayOfWeek == (int)Types.Day.Monday)
                        {
                            temp.HoursMon = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.MonFrom = companyHour.FromHour.ToString();
                            temp.MonTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Tuesday)
                        {
                            temp.HoursTue = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.TusFrom = companyHour.FromHour.ToString();
                            temp.TusTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Wednesday)
                        {
                            temp.HoursWed = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.WedFrom = companyHour.FromHour.ToString();
                            temp.WedTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Thursday)
                        {
                            temp.HoursThur = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.ThurFrom = companyHour.FromHour.ToString();
                            temp.ThurTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Friday)
                        {
                            temp.HoursFri = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.FriFrom = companyHour.FromHour.ToString();
                            temp.FriTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Saturday)
                        {
                            temp.HoursSat = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.SatFrom = companyHour.FromHour.ToString();
                            temp.SatTo = companyHour.ToHour.ToString();
                        }
                        if (companyHour.DayOfWeek == (int)Types.Day.Sunday)
                        {
                            temp.HoursSun = companyHour.FromHour + "-" + companyHour.ToHour;
                            temp.SunFrom = companyHour.FromHour.ToString();
                            temp.SunTo = companyHour.ToHour.ToString();
                        }
                    }
                }

            }


            return temp;
        }
        public ProfileCompanyTemp ConvertRowWorkSheetToObjectProfileCompanyTemp(int iRow, ExcelWorksheet worksheet, out string msg)
        {
            var profileCompany = new ProfileCompany();
            msg = string.Empty;
            var companyHourErr = string.Empty;
            var msgErrorInactiveCompany = string.Empty;
            try
            {
                var properties = GetProperties();
                string companyName = string.Empty;
                try
                {
                    companyName =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_COMAPANYNAME)].Value.ToString();
                }
                catch (Exception)
                {
                    companyName =
                   worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_COMAPANYNAME)].Value as string;
                }
                string address = string.Empty;
                try
                {
                    address =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ADDRESS)].Value.ToString();
                }
                catch (Exception)
                {
                    address =
                   worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_COMAPANYNAME)].Value as string;
                }

                //string address1 =
                //    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ADDRESS1)].Value as string;
                //worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CITY)].Value.ToString();
                string city = string.Empty;
                try
                {
                    city =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CITY)].Value.ToString();
                }
                catch (Exception)
                {
                    city =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CITY)].Value as string;
                }
                string state = string.Empty;
                try
                {
                    state = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_STATE)].Value.ToString();
                }
                catch (Exception)
                {
                    state = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_STATE)].Value as string;
                }

                //string zipCode =
                //    Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value).ToString();
                string zipCode = string.Empty;
                try
                {
                    zipCode =
                        Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value)
                            .ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    zipCode = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_ZIP)].Value as string;
                }
                string location =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_LOCATION)].Value as string;
                string phoneNumber =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_PHONE)].Value as string;
                string website =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_WEBSITE)].Value as string;
                string email = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_EMAIL)].Value as string;
                //string service =
                //    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_SERVICE)].Value as string;

                var pmThod = worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_PAYMENTMETHOD)].Value;
                string paymentMethod = pmThod != null ? pmThod.ToString() : null;
                string contactPerson =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_CONTACTPERSON)].Value as string;

                string monHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSMON)].Value as string;
                string tueHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSTUE)].Value as string;
                string wedHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSWED)].Value as string;
                string thursHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSTHURS)].Value as string;
                string friHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSFRI)].Value as string;
                string satHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSSAT)].Value as string;
                string sunHours =
                    worksheet.Cells[iRow, GetColumnIndex(properties, COMPANY_PROFILE_HOURSSUN)].Value as string;


                profileCompany.Name = companyName;
                profileCompany.CompanyTypeID = (int)Types.CompanyType.NonKuyamBookIt;
                profileCompany.CompanyStatusID = (int)Types.CompanyStatus.Active;
                profileCompany.Street1 = address;
                //profileCompany.Street2 = address1;
                profileCompany.City = city;
                profileCompany.State = state;
                profileCompany.Zip = zipCode;
                profileCompany.Phone = !string.IsNullOrEmpty(phoneNumber) ? Kuyam.Domain.UtilityHelper.CleanPhone(phoneNumber) : null;
                profileCompany.Email = email;
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    try
                    {
                        profileCompany.PaymentMethod = int.Parse(paymentMethod);
                    }
                    catch (Exception)
                    {
                        profileCompany.PaymentMethod = 0;
                    }
                }
                profileCompany.ContactName = contactPerson;
                profileCompany.Created = DateTime.UtcNow;
                profileCompany.ApptDefaultPeoplePerSlot = 1;
                profileCompany.ApptDefaultSlotDuration = 60;
                profileCompany.ApptAutoConfirm = true;
                profileCompany.Url = website;
                if (!string.IsNullOrEmpty(location))
                {
                    var local = location.Split(new char[] { ',' });
                    if (local.Any() && local.Count() == 2)
                    {
                        profileCompany.Latitude = double.Parse(local[0]);
                        profileCompany.Longitude = double.Parse(local[1]);
                    }
                    else
                    {
                        var msg1 = "Category name:" + worksheet.Name + " ,row: " + iRow + " location incorrect format (lat,lng)" + BREAK_LINE;
                        LogHelper.ImportExportCompanyError(msg1);
                        msg = msg1;
                        msgErrorInactiveCompany = "location incorrect format lat,lng";
                    }

                }
                else
                {
                    profileCompany.Latitude = -1;
                    profileCompany.Longitude = -1;
                }

                profileCompany.CompanyHours = new Collection<CompanyHour>();
                if (!string.IsNullOrEmpty(monHours) &&
                    !string.IsNullOrEmpty(tueHours) &&
                    !string.IsNullOrEmpty(wedHours) &&
                    !string.IsNullOrEmpty(thursHours) &&
                    !string.IsNullOrEmpty(friHours) &&
                    !string.IsNullOrEmpty(satHours) &&
                    !string.IsNullOrEmpty(sunHours)
                    && monHours.Trim().ToUpper() == tueHours.Trim().ToUpper()
                    && tueHours.Trim().ToUpper() == wedHours.Trim().ToUpper()
                    && wedHours.Trim().ToUpper() == thursHours.Trim().ToUpper()
                    && thursHours.Trim().ToUpper() == friHours.Trim().ToUpper()
                    && friHours.Trim().ToUpper() == satHours.Trim().ToUpper()
                    && satHours.Trim().ToUpper() == sunHours.Trim().ToUpper()
                    && sunHours.Trim().ToUpper() == monHours.Trim().ToUpper())
                {
                    var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, monHours, (int)Types.Day.Isdaily, out companyHourErr);
                    if (companyHour != null)
                    {
                        companyHour.IsDaily = true;
                        profileCompany.CompanyHours.Add(companyHour);
                    }
                }
                else
                {
                    #region "insert hour"

                    if (!string.IsNullOrEmpty(monHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, monHours, (int)Types.Day.Monday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(tueHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, tueHours, (int)Types.Day.Tuesday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(wedHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, wedHours, (int)Types.Day.Wednesday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(thursHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, thursHours, (int)Types.Day.Thursday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(friHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, friHours, (int)Types.Day.Friday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(satHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, satHours, (int)Types.Day.Saturday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }
                    if (!string.IsNullOrEmpty(sunHours))
                    {
                        var companyHour = CreateCompanyHourProfileCompany(profileCompany.ProfileID, sunHours, (int)Types.Day.Sunday, out companyHourErr);
                        if (companyHour != null)
                        {
                            profileCompany.CompanyHours.Add(companyHour);
                        }
                    }

                    #endregion
                }
                //var ser = _companyProfileService.GeServiceByName(worksheet.Name);
                //var serviceCompany = new ServiceCompany();
                //serviceCompany.ServiceID = ser.ServiceID;
                //serviceCompany.Created = DateTime.UtcNow;
                //serviceCompany.Status = (int)Types.ServiceCompanyStatus.Active;
                //serviceCompany.Modified = DateTime.UtcNow;
                //serviceCompany.ProfileID = profileCompany.ProfileID;
                //profileCompany.ServiceCompanies.Add(serviceCompany);
            }
            catch (Exception ex)
            {
                var msg1 = "Category name:" + worksheet.Name + " ,row: " + iRow + " ,exception:" + ex.StackTrace.ToString() + BREAK_LINE;
                LogHelper.ImportExportCompanyError(msg1);
                msg = msg1;
                //profileCompany = null;
            }
            var profileCompanyTemp = ConvertToProfileCompanyTemp(profileCompany);
            if (!string.IsNullOrEmpty(msgErrorInactiveCompany))
            {
                profileCompanyTemp.Status = 0;
                profileCompanyTemp.ErrorMessage = msgErrorInactiveCompany;
            }
            var cate = _companyProfileService.GeServiceByName(worksheet.Name);
            profileCompanyTemp.CategoryName = worksheet.Name;
            profileCompanyTemp.CategoryId = cate.ServiceID;
            return profileCompanyTemp;
        }
        public CompanyHour CreateCompanyHourProfileCompany(int profileCompanyID, string strTime, int dayofWeek, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                var time = strTime.Split(new char[] { '-' });
                if (time.Any() && time.Count() == 2)
                {
                    var companyHour = new CompanyHour();
                    var fromDate = DateTime.Parse("01/20/2014 " + time[0]);
                    var fromHour = new TimeSpan(fromDate.Hour, fromDate.Minute, fromDate.Second);
                    var toDate = DateTime.Parse("01/20/2014 " + time[1]);
                    var toHour = new TimeSpan(toDate.Hour, toDate.Minute, toDate.Second);
                    if (fromDate > toDate)
                    {
                        var msg = "Day of week: " + dayofWeek + " have start date > end date";
                        LogHelper.ImportExportCompanyError(msg);
                        errorMsg += msg;
                        return null;
                    }
                    else
                    {
                        companyHour.ToHour = toHour;
                        companyHour.FromHour = fromHour;
                        companyHour.DayOfWeek = dayofWeek;
                        companyHour.ProfileCompanyID = profileCompanyID;
                        return companyHour;
                    }

                }
            }
            catch (Exception ex)
            {
                var msg = "Date format wrong:" + strTime;
                LogHelper.ImportExportCompanyError(msg);
                errorMsg += msg;
                return null;
            }
            return null;

        }

        public string CheckProfileCompanyTempCorrect(ProfileCompanyTemp profileCompany)
        {
            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(profileCompany.Name))
                errorMessage = "Company name can't blank";
            if (string.IsNullOrEmpty(profileCompany.Street1))
            {
                errorMessage = "Address can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.City))
            {
                errorMessage = "City can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.State))
            {
                errorMessage = "State can't blank" + BREAK_LINE;
            }
            if (string.IsNullOrEmpty(profileCompany.Zip))
            {
                errorMessage = "Zip can't blank" + BREAK_LINE;
            }
            //if (string.IsNullOrEmpty(profileCompany.Phone))
            //{
            //    errorMessage = "Phone can't blank" + BREAK_LINE;
            //}
            //if (string.IsNullOrEmpty(profileCompany.Email))
            //{
            //    errorMessage = "Email can't blank" + BREAK_LINE;
            //}
            if (profileCompany.Latitude == -1 || profileCompany.Longitude == -1)
            {
                errorMessage = "Location can't blank" + BREAK_LINE;
            }
            return errorMessage;
        }

        public bool CheckCompanyTempExisted(List<ProfileCompany> profileCompanies, ProfileCompanyTemp company)
        {
            return profileCompanies.Any(a => !string.IsNullOrEmpty(a.Name) &&
                !string.IsNullOrEmpty(a.Street1 ) &&
                 !string.IsNullOrEmpty(a.City) &&
                  !string.IsNullOrEmpty(a.State) &&
                   !string.IsNullOrEmpty(a.Zip) &&
                   UtilityHelper.DamerauLevenshteinDistance(a.Name, company.Name) < 2 &&
                   UtilityHelper.DamerauLevenshteinDistance(a.Street1, company.Street1) < 2 &&
                    UtilityHelper.DamerauLevenshteinDistance(a.City, company.City) < 2 &&
                    UtilityHelper.DamerauLevenshteinDistance(a.State, company.State) < 2 &&
                    UtilityHelper.DamerauLevenshteinDistance(a.Zip, company.Zip) < 2
                );
        }
        public void InsertCompnayTemps(List<ProfileCompanyTemp> temps)
        {
            try
            {
                //insert profile table
                //insert profile table
                //insert profile companye table
                //insert company hours table
                //insert services company table
                //_companyProfileService.InsertProfile(profile);
                var cust = DAL.xGetCust("kuyam1@kuyam.com");
                var custId = 0;
                if (cust != null)
                    custId = cust.CustID;
                var listImport = CheckExistedProfileCompanyExisted(temps);
                _adminService.InsertProfileCompanyTemp(listImport, custId);
            }
            catch (Exception exception)
            {
                //var err = "Company: " + profile.Name + "can't insert because: " + exception.StackTrace.ToString() + BREAK_LINE;
                //errorMessage = err;
            }
        }
        public string ImportCompanyTempToDatabase(string directoryPath)
        {
            var fileInfo = new FileInfo(directoryPath);

            List<string> errorExited = null;
            var sheets = GetAllWorkSheetAvailible(fileInfo, out errorExited);
            var sheetsAvailible = sheets[0];//list import
            var sheetsInAvailible = sheets[1];//list show error
            //Case categories is not existed
            List<string> errorInsert = new List<string>();
            List<string> errorsCheckFileExcel = null;
            var strResults = string.Empty;
            if (sheetsAvailible != null || sheetsAvailible.Any())
            {
                var companyImports = GetCompanies(sheetsAvailible, out errorsCheckFileExcel);
                InsertCompnayTemps(companyImports);
            }
            return strResults;
        }

        public List<ProfileCompanyTemp> CheckExistedProfileCompanyExisted(List<ProfileCompanyTemp> list)
        {
            var currentList = list.Select(a => a);
            var returnList = new List<ProfileCompanyTemp>();
            foreach (var profileCompanyTemp in list)
            {
                if (profileCompanyTemp.Status == 1)
                {
                    if (list.Count(a =>
                        !string.IsNullOrEmpty(a.Name) &&
                        a.Name.Trim().ToUpper() == profileCompanyTemp.Name.Trim().ToUpper()
                        &&
                        !string.IsNullOrEmpty(a.Street1) &&
                        a.Street1.Trim().ToUpper() == profileCompanyTemp.Street1.Trim().ToUpper()
                        &&
                        !string.IsNullOrEmpty(a.City) &&
                        a.City.Trim().ToUpper() == profileCompanyTemp.City.Trim().ToUpper()
                        &&
                        !string.IsNullOrEmpty(a.State) &&
                        a.State.Trim().ToUpper() == profileCompanyTemp.State.Trim().ToUpper()
                        &&
                        !string.IsNullOrEmpty(a.Zip) &&
                        a.Zip.Trim().ToUpper() == profileCompanyTemp.Zip.Trim().ToUpper()) > 1)
                    {
                        profileCompanyTemp.Status = 0;
                        profileCompanyTemp.ErrorMessage += "duplicated data. Please check";
                        returnList.Add(profileCompanyTemp);
                    }
                    else
                    {
                        returnList.Add(profileCompanyTemp);
                    }
                }
                else
                {
                    returnList.Add(profileCompanyTemp);
                }
            }
            return returnList;
        }

        #endregion

        #region "export data"

        public ActionResult ExportUser()
        {
            return null;
        }
        //public ActionResult ExportClientsListToExcel(List<Cust> cusList)
        //{
        //    var grid = new System.Web.UI.WebControls.GridView();

        //    grid.DataSource = 
        //                      from d in cusList
        //                      select new
        //                      {
        //                          FirstName = d.FirstName,
        //                          LastName = d.LastName,
        //                          DOB = d.Dob,
        //                          Email = d.Email

        //                      };

        //    grid.DataBind();

        //    HttpResponse Response.ClearContent();
        //    Response.AddHeader("content-disposition", "attachment; filename=Exported_Diners.xls");
        //    Response.ContentType = "application/excel";
        //    StringWriter sw = new StringWriter();
        //    HtmlTextWriter htw = new HtmlTextWriter(sw);

        //    grid.RenderControl(htw);

        //    Response.Write(sw.ToString());

        //    Response.End();

        //    return null;
        //}
        #endregion
    }

}
