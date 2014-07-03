using System.Collections.Generic;
using Kuyam.Database;

namespace Kuyam.Domain.GiftCardServices
{
    public interface IGiftCardServices
    {
        GiftCard GetGiftCardById(int id);
        GiftCard BuyGiftCard(GiftCard gift);
        GiftCard UpdateGiftCardInfo(GiftCard gift);
        bool DeleteGiftCard(GiftCard gift);
        GiftCard GetGiftCardByGiftCardCode(string giftCardCode);
        GiftCard GetGiftCardByGiftCardCode(string giftCardCode,int custId);
        GiftCardHistory GiftRedeem(GiftCardHistory giftItem);
        List<GiftCard> AdminGetListGiftCardByMostRecent(string key, int pageIndex, int pageSize, out int totalRecord,
            int typeSearch, int typePurchased, int statusPurchased, string shippingMethod);
    }
}