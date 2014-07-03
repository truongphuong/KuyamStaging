using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using System.Data.Entity;

namespace Kuyam.Domain.PromoCodeServices
{
    public class PromoCodeServices : IPromoCodeServices
    {
        #region Private Fields
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<UserDiscount> _userDiscountRepository;

        #endregion

        public PromoCodeServices(IRepository<Discount> discountRepository,
            IRepository<UserDiscount> userDiscountRepository)
        {
            _discountRepository = discountRepository;
            _userDiscountRepository = userDiscountRepository;
        }
        public List<Discount> GetAllDiscountByAdmin(int discountType)
        {
            var discounts = _discountRepository.Table.Where(a => a.DiscountType == discountType);
            return discounts.ToList();
        }
        public List<Discount> GetAllDiscountByAdminAndStatus(int status, int discountType)
        {
            var discounts = _discountRepository.Table.Where(a => a.Status == status && a.DiscountType == discountType);
            return discounts.ToList();
        }


        public bool CheckExistedPromoCode(string promocode, int discountType)
        {
            return _discountRepository.Table.Any(a => a.Code == promocode.Trim() && a.DiscountType == discountType);
        }


        public Discount CreateDiscount(Discount discount)
        {
            _discountRepository.Insert(discount);
            return discount;
        }


        public List<Discount> AdminGetDiscounts(string key, int pageIndex, int pageSize, out int totalRecord, int status, DateTime? startDateTime, DateTime? endDateTime, int discountType)
        {
            totalRecord = 0;
            var query =
                    _discountRepository.Table.Where(
                        u => ((key == null || key == string.Empty ||
                              u.Name.ToUpper().Contains(key.ToUpper()))
                              || u.Code.ToUpper().Contains(key.ToUpper()))
                              && u.DiscountType == discountType);
            if (status != -1)
            {
                query = query.Where(u => u.Status == status);
            }
            if (startDateTime.HasValue && endDateTime.HasValue)
            {
                var sdate = new DateTime(startDateTime.Value.Year, startDateTime.Value.Month, startDateTime.Value.Day);
                var pstsDate = DateTimeUltility.ConvertToPstTime(sdate);
                var edate = new DateTime(endDateTime.Value.Year, endDateTime.Value.Month, endDateTime.Value.Day);
                var psteDate = DateTimeUltility.ConvertToPstTime(edate);
                query = query.Where(u => DbFunctions.TruncateTime(u.StartDate) >= DbFunctions.TruncateTime(pstsDate) && DbFunctions.TruncateTime(u.EndDate) <= DbFunctions.TruncateTime(psteDate));
            }
            else
            {
                if (startDateTime.HasValue)
                {
                    var date = new DateTime(startDateTime.Value.Year, startDateTime.Value.Month, startDateTime.Value.Day);
                    var psteDate = date;
                    query = query.Where(u => DbFunctions.TruncateTime(u.StartDate) == DbFunctions.TruncateTime(psteDate));
                }
                else if (endDateTime.HasValue)
                {
                    var date = new DateTime(endDateTime.Value.Year, endDateTime.Value.Month, endDateTime.Value.Day);
                    var psteDate = date;
                    query = query.Where(u => DbFunctions.TruncateTime(u.EndDate) == DbFunctions.TruncateTime(psteDate));
                }
            }
            totalRecord = query.Count();
            return query.OrderByDescending(u => u.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        }


        public Discount GetDiscountById(int id)
        {
            var discounts = _discountRepository.Table.Where(u => u.DiscountId == id);
            return discounts.SingleOrDefault();
        }


        public Discount UpdateDiscount(Discount discount)
        {
            var dis = _discountRepository.Table.SingleOrDefault(u => u.DiscountId == discount.DiscountId);
            if (dis == null)
                return null;

            dis.Name = discount.Name;
            dis.Status = discount.Status;
            dis.StartDate = discount.StartDate;
            dis.EndDate = discount.EndDate;
            dis.Quantity = discount.Quantity;
            dis.ModifiedDate = DateTime.UtcNow;
            dis.Code = discount.Code;
            dis.Amount = discount.Amount;
            _discountRepository.Update(dis);
            return dis;
        }


        public List<Discount> AdminGetUserDiscounts(string key, int pageIndex, int pageSize, out int totalRecord, int status, DateTime? startDateTime, DateTime? endDateTime, int discountType)
        {
            totalRecord = 0;
            var query =
                    _discountRepository.Table.Where(
                        u => ((key == null || key == string.Empty ||
                              u.Name.ToUpper().Contains(key.ToUpper()))
                              || u.Code.ToUpper().Contains(key.ToUpper()))
                              && u.DiscountType == discountType);

            if (status != -1)
            {
                query = query.Where(a => a.Status == status);
            }
            return query.OrderByDescending(u => u.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
