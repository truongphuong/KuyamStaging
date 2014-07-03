using System;
using System.Collections.Generic;
using Kuyam.Database;

namespace Kuyam.Domain.PromoCodeServices
{
    public interface IPromoCodeServices
    {
        List<Discount> GetAllDiscountByAdmin(int discountType);
        List<Discount> GetAllDiscountByAdminAndStatus(int status, int discountType);
        bool CheckExistedPromoCode(string promocode, int discountType);
        Discount CreateDiscount(Discount discount);
        List<Discount> AdminGetDiscounts(string key, int pageIndex, int pageSize, 
            out int totalRecord, 
            int status, 
            DateTime? startDateTime,
            DateTime? endDateTime,
            int discountType);
        List<Discount> AdminGetUserDiscounts(string key, int pageIndex, int pageSize,
            out int totalRecord,
            int status,
            DateTime? startDateTime,
            DateTime? endDateTime,
            int discountType);
        Discount GetDiscountById(int id);
        Discount UpdateDiscount(Discount discount);
    }
}