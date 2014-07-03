using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Interface;
using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.Utility;
using System.Data.Linq.SqlClient;

namespace Kuyam.Domain
{
    public class OrderService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<ProfileCompany> _profileCompanyRepository;
        private readonly IRepository<ServiceCompany> _serviceCompanyRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<CompanyEmployee> _companyEmployeeRepository;
        private readonly IRepository<UserPackagePurchase> _userPackagePurchaseRepository;
        private readonly IRepository<RegularClient> _regularClientRepository;

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountRegularClient> _discountRegularClientRepository;
        private readonly IRepository<DiscountService> _discountServiceRepository;
        private readonly IRepository<UserDiscount> _userDiscountRepository;
        private readonly IRepository<DiscountInvite> _discountInviteRepository;
        private readonly IRepository<CompanyPackageService> _companyPackageServiceRepository;
        private readonly IRepository<CompanyPackage> _companyPackageRepository;
        private readonly IRepository<OrderGettyImageDetail> _orderGettyImageDetailRepository;

        #endregion

        #region Ctor

        public OrderService(IRepository<Order> orderRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<Appointment> appointmentRepository,
            IRepository<UserPackagePurchase> userPackagePurchaseRepository,
            IRepository<ProfileCompany> profileCompanyRepository,
            IRepository<ServiceCompany> serviceCompanyRepository,
            IRepository<CompanyEmployee> companyEmployeeRepository,
            IRepository<Service> serviceRepository,
            IRepository<RegularClient> regularClientRepository,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRegularClient> discountRegularClientRepository,
            IRepository<DiscountService> discountServiceRepository,
            IRepository<UserDiscount> userDiscountRepository,
            IRepository<CompanyPackageService> companyPackageServiceRepository,
            IRepository<CompanyPackage> companyPackageRepository,
            IRepository<OrderGettyImageDetail> orderGettyImageDetailRepository,
            IRepository<DiscountInvite> discountInviteRepository)
        {
            this._orderRepository = orderRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._appointmentRepository = appointmentRepository;
            this._userPackagePurchaseRepository = userPackagePurchaseRepository;
            this._profileCompanyRepository = profileCompanyRepository;
            this._serviceCompanyRepository = serviceCompanyRepository;
            this._companyEmployeeRepository = companyEmployeeRepository;
            this._serviceRepository = serviceRepository;
            this._regularClientRepository = regularClientRepository;
            this._discountRepository = discountRepository;
            this._discountServiceRepository = discountServiceRepository;
            this._discountRegularClientRepository = discountRegularClientRepository;
            this._userDiscountRepository = userDiscountRepository;
            this._companyPackageServiceRepository = companyPackageServiceRepository;
            this._companyPackageRepository = companyPackageRepository;
            this._orderGettyImageDetailRepository = orderGettyImageDetailRepository;
            this._discountInviteRepository = discountInviteRepository;
        }

        #endregion

        public void InsertOrder(Order order)
        {
            _orderRepository.Insert(order);
        }

        public void UpdateOrder(Order order)
        {
            _orderRepository.Update(order);
        }

        public void InsertOrderDetail(OrderDetail orderDetail)
        {
            _orderDetailRepository.Insert(orderDetail);
        }

        public void InsertUserDiscount(UserDiscount userDiscount)
        {
            _userDiscountRepository.Insert(userDiscount);
        }

        public void UpdateUserDiscount(UserDiscount userDiscount)
        {
            _userDiscountRepository.Update(userDiscount);
        }

        public RegularClient GetRegularClientByEmail(string email, int profileID)
        {
            var query = from rgl in _regularClientRepository.Table
                        where rgl.Email == email && rgl.Status == true
                              && rgl.CompanyProfileId == profileID
                        select rgl;
            return query.FirstOrDefault();
        }

        public List<CompanyInvoices> GetCompanyInvoicesInfo(DateTime? createdDate, int? serviceId, string empName, int? paymentMethod, int profileCompanyId, int pageIndex, int pageSize, out int totalRecord)
        {
            DateTime? dtstart = null;
            DateTime? dtEnd = null;
            if (createdDate.HasValue)
            {
                //dtstart = DateTimeUltility.ConvertToUtcTime(createdDate.Value.Date);
                dtstart = createdDate.Value.Date;
                dtEnd = dtstart.Value.AddDays(1);
            }
            totalRecord = 0;
            var query1 = from regu in _regularClientRepository.Table
                         where regu.CompanyProfileId == profileCompanyId
                          && regu.Status == true
                         select regu.Email;

            var query2 = from prf in _profileCompanyRepository.Table
                         join o in _orderRepository.Table on prf.ProfileID equals o.ProfileID
                         join ord in _orderDetailRepository.Table on o.OrderID equals ord.OrderID
                         join apt in _appointmentRepository.Table on ord.AppointmentID equals apt.AppointmentID
                         join svc in _serviceCompanyRepository.Table on apt.ServiceCompanyID equals svc.ServiceCompanyID
                         join empl in _companyEmployeeRepository.Table on apt.EmployeeID equals empl.EmployeeID
                         where o.ProfileID == profileCompanyId
                               && (serviceId == -1 || serviceId == 0 || serviceId == null || (svc.ServiceID == serviceId))
                               && (createdDate == null || apt.Start >= dtstart.Value && apt.Start < dtEnd.Value)
                               && (empName == "" || empName == string.Empty || (empl.EmployeeName.Contains(empName)))
                               && (paymentMethod == null || paymentMethod == -1 || o.PaymentMethodID == paymentMethod)
                         select new CompanyInvoices
                         {
                             ClientName = o.Cust.FirstName + " " + o.Cust.LastName,
                             CompanyName = prf.Name,
                             EmployeeName = empl.EmployeeName,
                             IsRegular = query1.Contains(o.Cust.aspnet_Users.UserName),
                             PaymentMethod = o.PaymentMethodID,
                             ReceiptNumber = o.PurchaseOrderNumber,
                             ServiceDescription = svc.Service.Desc,
                             PurchasedOn = o.CreatedOnUtc,
                             ServiceStartDate = apt.Start,
                             ServiceAmmount = apt.Price,
                             AppointmentStatus = apt.AppointmentStatusID,
                             DiscountCodeNumber = o.DiscountCodeNumber,
                             GiftCardCodeNumber = o.GiftCardCodeNumber,
                             GiftCardAmount = o.GiftCardAmount,
                             PercentKuyamFee = o.PercentKuyamFee,
                             AppointmentAdditionalFee = o.AppointmentAdditionalFee,
                             KuyamFeeTotal = o.KuyamFeeTotal,
                             PercentPaymentFee = o.PercentPaymentFee,
                             PaymentFeeTotal = o.PaymentFeeTotal,
                             TransactionAdditionalFee = o.TransactionAdditionalFee,
                             OrderDiscount = o.OrderDiscount,
                             OrderSubtotal = o.OrderSubtotal,
                             OrderTotal = o.OrderTotal,
                             DiscountType = o.UserDiscounts.Any() ? o.UserDiscounts.FirstOrDefault().Discount.DiscountType : (int)Types.DiscountType.Admin
                         };

            totalRecord = query2.Count();
            return query2.OrderBy(m => m.ServiceStartDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<CompanyInvoices> GetUserInvoicesInfo(DateTime? createdDate, int? serviceId, string empName, int? paymentMethod, int custId, int pageIndex, int pageSize, out int totalRecord)
        {
            DateTime? dtstart = null;
            DateTime? dtEnd = null;
            if (createdDate.HasValue)
            {
                //dtstart = DateTimeUltility.ConvertToUtcTime(createdDate.Value.Date);
                dtstart = createdDate.Value.Date;
                dtEnd = dtstart.Value.AddDays(1);
            }

            var query = from prf in _profileCompanyRepository.Table
                        join o in _orderRepository.Table on prf.ProfileID equals o.ProfileID
                        join ord in _orderDetailRepository.Table on o.OrderID equals ord.OrderID
                        join apt in _appointmentRepository.Table on ord.AppointmentID equals apt.AppointmentID
                        join svc in _serviceCompanyRepository.Table on apt.ServiceCompanyID equals svc.ServiceCompanyID
                        join empl in _companyEmployeeRepository.Table on apt.EmployeeID equals empl.EmployeeID
                        where o.CustID == custId
                              && (serviceId == 0 || serviceId == -1 || serviceId == null || (svc.ServiceID == serviceId))
                              && (createdDate == null || apt.Start >= dtstart.Value && apt.Start < dtEnd.Value)
                              && (empName == string.Empty || empName == "" || (empl.EmployeeName.Contains(empName)))
                              && (paymentMethod == -1 || paymentMethod == null || o.PaymentMethodID == paymentMethod)
                        select new CompanyInvoices
                        {
                            ClientName = o.Cust.FirstName + " " + o.Cust.LastName,
                            CompanyName = prf.Name,
                            EmployeeName = empl.EmployeeName,
                            IsRegular = false,
                            PaymentMethod = o.PaymentMethodID,
                            ReceiptNumber = o.PurchaseOrderNumber,
                            ServiceDescription = svc.Service.Desc,
                            ServiceStartDate = apt.Start,
                            PurchasedOn = o.CreatedOnUtc,
                            ServiceAmmount = apt.Price,
                            AppointmentStatus = apt.AppointmentStatusID,
                            DiscountCodeNumber = o.DiscountCodeNumber,
                            GiftCardCodeNumber = o.GiftCardCodeNumber,
                            GiftCardAmount = o.GiftCardAmount,
                            PercentKuyamFee = o.PercentKuyamFee,
                            AppointmentAdditionalFee = o.AppointmentAdditionalFee,
                            KuyamFeeTotal = o.KuyamFeeTotal,
                            PercentPaymentFee = o.PercentPaymentFee,
                            TransactionAdditionalFee = o.TransactionAdditionalFee,
                            OrderDiscount = o.OrderDiscount,
                            OrderSubtotal = o.OrderSubtotal,
                            OrderTotal = o.OrderTotal
                        };

            totalRecord = query.Count();
            return query.OrderBy(m => m.ServiceStartDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public DiscountExt GetDiscoutByCode(string code, int serviceId, int profileId, int custId)
        {
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);

            var query = from dcs in _discountServiceRepository.Table
                        where dcs.ServiceCompanyId == serviceId
                        select dcs.DiscountId;


            var query2 = _discountRepository.Table.Where(d => d.Code == code
                && d.Status == (int)Types.DiscountStatus.Active
                && d.DiscountType == (int)Types.DiscountType.Admin
                && (d.Quantity == -1 || d.Quantity > d.UserDiscounts.Count())
                && d.StartDate <= dtNow && d.EndDate >= dtNow);



            if (query2.Any())
            {
                var bookedappointment = _appointmentRepository.Table.Where(
                    m => m.CustID == custId
                    && ((m.AppointmentStatusID == (int)Types.AppointmentStatus.Pending && m.Start > dtNow) || m.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed));

                if (bookedappointment.Any())
                    return null;
                string timezone = TimeZone.CurrentTimeZone.StandardName;
                string servertime = DateTime.Now.ToString();
                return query2.Select(dc => new DiscountExt
                {
                    discountid = dc.DiscountId,
                    name = dc.Name,
                    code = dc.Code,
                    amount = dc.Amount,
                    percent = dc.Percent,
                    quantity = dc.Quantity,
                    applytoallservices = dc.ApplyToAllServices,
                    profilecompanyid = dc.ProfileCompanyId ?? 0,
                    status = dc.Status,
                    ServerTimezone = timezone,
                    ServerDate = servertime
                }).FirstOrDefault();

            }
            var query3 = from dc in _discountRepository.Table
                         join idc in _discountInviteRepository.Table on dc.DiscountId equals idc.DiscountId
                         where dc.Code == code
                         && (serviceId == 0 || dc.ApplyToAllServices || query.Contains(dc.DiscountId))
                         && dc.ProfileCompanyId == profileId
                         && idc.CustId == custId
                         && (dc.Quantity == -1 || dc.Quantity > dc.UserDiscounts.Count())
                         && dc.StartDate <= dtNow && dc.EndDate >= dtNow
                         && dc.Status == (int)Types.DiscountStatus.Active
                         select new DiscountExt
                         {
                             discountid = dc.DiscountId,
                             name = dc.Name,
                             code = dc.Code,
                             amount = dc.Amount,
                             percent = dc.Percent,
                             quantity = dc.Quantity,
                             applytoallservices = dc.ApplyToAllServices,
                             profilecompanyid = dc.ProfileCompanyId ?? 0,
                             status = dc.Status

                         };

            return query3.FirstOrDefault();
        }

        public Discount GetDiscoutByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            var query = from dc in _discountRepository.Table
                        where dc.Code == code.Trim()
                        select dc;
            return query.FirstOrDefault();
        }

        public DiscountExt GetDiscoutCodeForPackage(string code, int packageId, int profileId, int custId)
        {
            try
            {
                DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);

                Discount discount = DAL.GetDiscountByDiscountCode(code, profileId);

                // get list servicecompanyID from discount code
                List<int> scIDs = new List<int>();
                if (discount != null && !discount.ApplyToAllServices)
                {
                    scIDs = (from dcs in _discountServiceRepository.Table
                             join dc in _discountRepository.Table on dcs.DiscountId equals dc.DiscountId
                             where dc.Code == code
                             select dcs.ServiceCompanyId).ToList();
                }
                else
                {
                    scIDs = (from sc in _serviceCompanyRepository.Table
                             where sc.ProfileID == profileId
                             select sc.ServiceCompanyID).Distinct().ToList();
                }

                //get list servicecompanyID from package
                List<int> scIDs1 = (from ps in _companyPackageServiceRepository.Table
                                    where ps.CompanyPackageId == packageId
                                    select ps.ServiceCompanyId).ToList();
                if (scIDs1 == null || scIDs == null || scIDs1.Count > scIDs.Count)
                    return null;

                List<int> sctemp = scIDs1.Where(x => scIDs.Contains(x)).Distinct().ToList();

                bool flag = false;

                if (sctemp.Count == scIDs1.Count)
                    flag = true;

                var query2 = _discountRepository.Table.Where(d => d.Code == code
                && d.Status == (int)Types.DiscountStatus.Active
                && d.DiscountType == (int)Types.DiscountType.Admin
                && (d.Quantity == -1 || d.Quantity > d.UserDiscounts.Count())
                && d.StartDate <= dtNow && d.EndDate >= dtNow);

                if (query2.Any())
                {
                    var bookedappointment = _appointmentRepository.Table.Where(
                    m => m.CustID == custId
                    && ((m.AppointmentStatusID == (int)Types.AppointmentStatus.Pending && m.Start > dtNow) || m.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed));


                    if (bookedappointment.Any())
                        return null;
                    return query2.Select(dc => new DiscountExt
                    {
                        discountid = dc.DiscountId,
                        name = dc.Name,
                        code = dc.Code,
                        amount = dc.Amount,
                        percent = dc.Percent,
                        quantity = dc.Quantity,
                        applytoallservices = dc.ApplyToAllServices,
                        profilecompanyid = dc.ProfileCompanyId ?? 0,
                        status = dc.Status
                    }).FirstOrDefault();
                }

                var query3 = from dc in _discountRepository.Table
                             join idc in _discountInviteRepository.Table on dc.DiscountId equals idc.DiscountId
                             where dc.Code == code
                             && ((flag || dc.ApplyToAllServices)
                             && dc.ProfileCompanyId == profileId
                             && idc.CustId == custId)
                             && (dc.Quantity == -1 || dc.Quantity > dc.UserDiscounts.Count())
                             && dc.StartDate <= dtNow && dc.EndDate >= dtNow
                             && dc.Status == (int)Types.DiscountStatus.Active
                             select new DiscountExt
                             {
                                 discountid = dc.DiscountId,
                                 name = dc.Name,
                                 code = dc.Code,
                                 amount = dc.Amount,
                                 percent = dc.Percent,
                                 quantity = dc.Quantity,
                                 applytoallservices = dc.ApplyToAllServices,
                                 profilecompanyid = dc.ProfileCompanyId ?? 0,
                                 status = dc.Status
                             };

                return query3.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public DiscountExt GetDiscoutPackageByCode(string code, int companyPackageId, int custId)
        {
            DateTime dtNow = DateTimeUltility.ConvertToUserTime(DateTime.UtcNow);

            // select company which has package code satisfy the input data
            var profileCompany =
                _profileCompanyRepository.Table.FirstOrDefault(
                    c => c.CompanyPackages.Any(p => p.PackageId == companyPackageId));

            if (profileCompany != null)
            {
                // get service identities of package
                var servicesOfPackage = _companyPackageRepository.Table.Where(cp => cp.PackageId == companyPackageId)
                                                                 .SelectMany(
                                                                     c =>
                                                                     c.CompanyPackageServices.Select(
                                                                         s => s.ServiceCompany.ServiceCompanyID));

                // get discount
                var discount = _discountRepository.Table.FirstOrDefault(d =>
                    // check discount is exist && active
                                                               d.ProfileCompanyId == profileCompany.ProfileID &&
                                                               d.Code == code &&
                                                               d.Status == (int)Types.DiscountStatus.Active &&
                                                               d.StartDate <= dtNow && d.EndDate >= dtNow &&

                                                               // make sure discount is apply to all services of package
                                                               (
                                                                   d.ApplyToAllServices ||
                                                                   d.DiscountServices.All(
                                                                       s =>
                                                                       servicesOfPackage.Contains(
                                                                           s.ServiceCompanyId))
                                                               )
                                                                   // make sure customer is allow to use discount code
                                                               && d.UserDiscounts.Any(u =>
                                                                                     u.CustId == custId
                                                                                     &&
                                                                                     (
                                                                                         d.Quantity == -1 ||
                                                                                         u.NumberofUsage < d.Quantity
                                                                                     )
                                                                     )
                    );


                var query2 = _discountRepository.Table.Where(d => d.Code == code
                  && d.Status == (int)Types.DiscountStatus.Active
                  && d.DiscountType == (int)Types.DiscountType.Admin
                  && (d.Quantity == -1 || d.Quantity > d.UserDiscounts.Count())
                  && d.StartDate <= dtNow && d.EndDate >= dtNow);

                if (query2.Any())
                {
                    var bookedappointment = _appointmentRepository.Table.Where(
                    m => m.CustID == custId
                    && ((m.AppointmentStatusID == (int)Types.AppointmentStatus.Pending && m.Start > dtNow) || m.AppointmentStatusID == (int)Types.AppointmentStatus.Confirmed));


                    if (bookedappointment.Any())
                        return null;
                    return query2.Select(dc => new DiscountExt
                    {
                        discountid = dc.DiscountId,
                        name = dc.Name,
                        code = dc.Code,
                        amount = dc.Amount,
                        percent = dc.Percent,
                        quantity = dc.Quantity,
                        applytoallservices = dc.ApplyToAllServices,
                        profilecompanyid = dc.ProfileCompanyId ?? 0,
                        status = dc.Status
                    }).FirstOrDefault();
                }


                var query3 = from dc in _discountRepository.Table
                             join idc in _discountInviteRepository.Table on dc.DiscountId equals idc.DiscountId
                             where dc.Code == code
                             && dc.ProfileCompanyId == profileCompany.ProfileID
                             && idc.CustId == custId
                             && (dc.ApplyToAllServices || dc.DiscountServices.All(s => servicesOfPackage.Contains(s.ServiceCompanyId)))
                             && (dc.Quantity == -1 || dc.Quantity > dc.UserDiscounts.Count())
                             && dc.StartDate <= dtNow && dc.EndDate >= dtNow
                             && dc.Status == (int)Types.DiscountStatus.Active
                             select new DiscountExt
                             {
                                 discountid = dc.DiscountId,
                                 name = dc.Name,
                                 code = dc.Code,
                                 amount = dc.Amount,
                                 percent = dc.Percent,
                                 quantity = dc.Quantity,
                                 applytoallservices = dc.ApplyToAllServices,
                                 profilecompanyid = dc.ProfileCompanyId ?? 0,
                                 status = dc.Status
                             };

                return query3.FirstOrDefault();


            }
            return null;
        }

        public Discount GetDiscoutByDiscountId(int discountId)
        {
            var query = from dc in _discountRepository.Table
                        where dc.DiscountId == discountId
                        select dc;
            return query.FirstOrDefault();
        }

        public UserDiscount GetUserDiscoutByDiscountId(int discountId)
        {
            var query = from dc in _userDiscountRepository.Table
                        where dc.DiscountId == discountId
                        select dc;
            return query.FirstOrDefault();
        }

        public Order GetOrderByAppointmentId(int appId)
        {
            var query = from or in _orderRepository.Table
                        from ord in or.OrderDetails.DefaultIfEmpty()
                        where ord.AppointmentID == appId
                        select or;
            return query.FirstOrDefault();
        }

        public ReceiptIP GetReceiptIP(int appointmentID)
        {
            var query = (from o in _orderRepository.Table
                         join ord in _orderDetailRepository.Table on o.OrderID equals ord.OrderID
                         join apt in _appointmentRepository.Table on ord.AppointmentID equals apt.AppointmentID
                         where apt.AppointmentID == appointmentID
                         select o).FirstOrDefault();

            ReceiptIP result = new ReceiptIP()
                       {
                           PaymentMethod = query.PaymentMethodID,
                           ReceiptNumber = query.PurchaseOrderNumber ?? string.Empty,
                           AppointmentID = appointmentID,
                           isPaid = DAL.IsAppointmentPaid(appointmentID, query.PaymentMethodID),
                           PurchasedOn = query.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                           DiscountCodeNumber = query.DiscountCodeNumber ?? string.Empty,
                           PercentKuyamFee = query.PercentKuyamFee.HasValue ? query.PercentKuyamFee.Value : 0,
                           AppointmentAdditionalFee = query.AppointmentAdditionalFee.HasValue ? query.AppointmentAdditionalFee.Value : 0,
                           KuyamFeeTotal = query.KuyamFeeTotal.HasValue ? query.KuyamFeeTotal.Value : 0,
                           PercentPaymentFee = query.PercentPaymentFee.HasValue ? query.PercentPaymentFee.Value : 0,
                           TransactionAdditionalFee = query.TransactionAdditionalFee.HasValue ? query.TransactionAdditionalFee.Value : 0,
                           OrderDiscount = query.OrderDiscount.HasValue ? query.OrderDiscount.Value : 0,
                           OrderSubtotal = query.OrderSubtotal.HasValue ? query.OrderSubtotal.Value : 0,
                           OrderTotal = query.OrderTotal.HasValue ? query.OrderTotal.Value : 0
                       };

            return result;
        }

        #region Gettyimage

        public void InsertOrderGettyimage(OrderGettyImageDetail ord)
        {
            _orderGettyImageDetailRepository.Insert(ord);
        }

        #endregion


    }
}
