using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Configuration;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using Kuyam.Utility;

namespace Kuyam.Domain.GiftCardServices
{
    public class GiftCardServices : IGiftCardServices
    {
        #region Private Fields
        private readonly IRepository<GiftCard> _gifCardRepository;
        private readonly IRepository<GiftCardHistory> _giftCardHistoryRepository;
        private readonly IRepository<GiftCardLockCust> _giftCardLockCustRepository;
        #endregion
        public GiftCardServices(IRepository<GiftCard> gifCardRepository,
            IRepository<GiftCardHistory> giftCardHistoryRepository,
            IRepository<GiftCardLockCust> giftCardLockCustRepository)
        {
            _gifCardRepository = gifCardRepository;
            _giftCardHistoryRepository = giftCardHistoryRepository;
            _giftCardLockCustRepository = giftCardLockCustRepository;
        }

        public GiftCard BuyGiftCard(GiftCard gift)
        {
            try
            {
                _gifCardRepository.Insert(gift);
                return gift;
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public GiftCard GetGiftCardById(int id)
        {
            var giftCards = _gifCardRepository.Table.Where(a => a.Id == id);
            return giftCards.FirstOrDefault();
        }


        public GiftCard UpdateGiftCardInfo(GiftCard gift)
        {
            _gifCardRepository.Update(gift);
            return gift;
        }

        public bool DeleteGiftCard(GiftCard gift)
        {
            try
            {
                _gifCardRepository.Delete(gift);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public GiftCard GetGiftCardByGiftCardCode(string giftCardCode)
        {
            var giftCards = _gifCardRepository.Table.Where(a => a.IsActivated && a.GiftCardCode != null && a.GiftCardCode.Trim() == giftCardCode.Trim());
            return giftCards.FirstOrDefault();
        }

        public GiftCard GetGiftCardByGiftCardCode(string giftCardCode, int custId)
        {
            if (string.IsNullOrWhiteSpace(giftCardCode))
                return new GiftCard { IsLocked = false };
            var giftCards = _gifCardRepository.Table.Where(a => a.IsActivated && a.GiftCardCode != null && a.GiftCardCode.Trim() == giftCardCode.Trim()).FirstOrDefault();
            bool isLocked = LockCustWhenWrongCode(custId);
            if (isLocked)
            {
                return new GiftCard { IsLocked = true };
            }
            if (giftCards == null)
            {
                return new GiftCard { IsLocked = false, IsFailed = true };
            }
            UnLockAllWhenCheckSucces(custId);
            return giftCards;
        }

        private bool LockCustWhenWrongCode(int custId)
        {
            var giftCustlock = _giftCardLockCustRepository.Table.Where(m => m.CustId == custId).FirstOrDefault();
            if (giftCustlock == null)
            {
                var item = new GiftCardLockCust
                {
                    CustId = custId,
                    IsLocked = false,
                    LockedTime = DateTime.UtcNow,
                    FailedGiftCardAttemptCount = 1,
                    FailedGiftCardAttemptStart = DateTime.UtcNow
                };
                _giftCardLockCustRepository.Insert(item);
                return false;
            }

            giftCustlock.FailedGiftCardAttemptCount += 1;

            int failedCount = ConfigManager.FailedCount;
            int lockedTime = ConfigManager.LockedTime;
            if (giftCustlock.FailedGiftCardAttemptCount.HasValue && giftCustlock.FailedGiftCardAttemptCount.Value <= failedCount
                || giftCustlock.FailedGiftCardAttemptStart.HasValue && giftCustlock.FailedGiftCardAttemptStart.Value.AddMinutes(lockedTime) <= DateTime.UtcNow)
            {
                giftCustlock.IsLocked = false;
                giftCustlock.FailedGiftCardAttemptStart = DateTime.UtcNow;
                _giftCardLockCustRepository.Update(giftCustlock);
                return false;
            }

            giftCustlock.IsLocked = true;
            giftCustlock.FailedGiftCardAttemptStart = DateTime.UtcNow;
            _giftCardLockCustRepository.Update(giftCustlock);

            return true;
        }

        private bool UnLockAllWhenCheckSucces(int custId)
        {
            var giftCustlock = _giftCardLockCustRepository.Table.Where(m => m.CustId == custId).FirstOrDefault();
            if (giftCustlock != null)
            {
                giftCustlock.IsLocked = false;
                giftCustlock.FailedGiftCardAttemptCount = 0;
                giftCustlock.FailedGiftCardAttemptStart = DateTime.UtcNow;
                _giftCardLockCustRepository.Update(giftCustlock);
                return true;
            }

            return false;

        }

        /// <summary>
        ///  insert gift history
        /// </summary>
        /// <param name="giftItem"></param>
        /// <returns></returns>
        public GiftCardHistory GiftRedeem(GiftCardHistory giftItem)
        {
            _giftCardHistoryRepository.Insert(giftItem);
            return giftItem;
        }

        public List<GiftCard> AdminGetListGiftCardByMostRecent(string key, int pageIndex, int pageSize, out int totalRecord, int typeSearch, int typePurchased, int statusPurchased, string shippingMethod)
        {
            totalRecord = 0;
            var query =
                    _gifCardRepository.Table.Where(
                        u => ((key == null || key == string.Empty ||
                              u.RecipientName.ToUpper().Contains(key.ToUpper())
                              || u.Address1.ToUpper().Contains(key.ToUpper()))
                             || (!string.IsNullOrEmpty(u.Address2) && u.Address2.ToUpper().Contains(key.ToUpper()))
                             || u.SenderName.ToUpper().Contains(key.ToUpper())
                             || u.ZipCode.ToUpper().Contains(key.ToUpper())
                             || u.City.ToUpper().Contains(key.ToUpper())
                             || u.State.ToUpper().Contains(key.ToUpper())

                             || (!string.IsNullOrEmpty(u.RecipientEmail) && u.RecipientEmail.Contains(key.ToUpper()))) && u.IsActivated == true);
            if (typeSearch == 0 || (typePurchased == -1 && statusPurchased == -1)) //case init         
            {
                //totalRecord = query.Count();
                query = query.Where(u => (u.GiftCardType == (int)Types.GiftCardType.email && u.GiftStatus == (int)Types.GiftCardStatus.Send) || (u.GiftCardType == (int)Types.GiftCardType.mail));
            }
            else//case select type Purchase or status Purchased
            {
                if (typePurchased == -1)//Type does not select
                {
                    switch (statusPurchased)
                    {
                        case 1: //case gift card type is email and gift card status is 'send via email'
                            query =
                                query.Where(
                                    u =>
                                        u.GiftCardType == (int)Types.GiftCardType.email &&
                                        u.GiftStatus == (int)Types.GiftCardStatus.Send);
                            break;
                        case 2: //case gift card type is mail and gift card status is 'ordered for standard shipping'
                            query =
                                query.Where(
                                    u =>
                                        u.GiftCardType == (int)Types.GiftCardType.mail &&
                                        u.GiftStatus == (int)Types.GiftCardStatus.NoSend &&
                                        u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaFreeStandardShipping);
                            break;
                        case 3: //case gift card  type is mail and gift card stauts is 'ordered for premium shipping'
                            query =
                                query.Where(
                                    u =>
                                        u.GiftCardType == (int)Types.GiftCardType.mail &&
                                        u.GiftStatus == (int)Types.GiftCardStatus.NoSend &&
                                        u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaPreminumShipping);
                            break;
                        case 4: //case gift card  type is mail and gift card stauts is 'sent via standard shipping' 
                            query =
                                query.Where(
                                    u =>
                                        u.GiftCardType == (int)Types.GiftCardType.mail &&
                                        u.GiftStatus == (int)Types.GiftCardStatus.Send &&
                                        u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaFreeStandardShipping);
                            break;
                        case 5: //case gift card  type is mail and gift card stauts is 'sent via premium shipping' 
                            query =
                                query.Where(
                                    u =>
                                        u.GiftCardType == (int)Types.GiftCardType.mail &&
                                        u.GiftStatus == (int)Types.GiftCardStatus.Send &&
                                        u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaPreminumShipping);
                            break;
                    }
                }
                else// Type select
                {
                    if (typePurchased == 1)//type is email
                    {
                        switch (statusPurchased)
                        {
                            case -1: //case gift card type is email and gift card status is not selected
                                query =
                                    query.Where(
                                        u =>
                                            u.GiftCardType == (int)Types.GiftCardType.email &&
                                            u.GiftStatus == (int)Types.GiftCardStatus.Send);
                                break;
                            case 1: //case gift card type is email and gift card status is 'send via email'
                                query =
                                    query.Where(
                                        u =>
                                            u.GiftCardType == (int)Types.GiftCardType.email &&
                                            u.GiftStatus == (int)Types.GiftCardStatus.Send);
                                break;
                        }
                    }
                    else// type is mail
                    {
                        if (typePurchased == 2)//ordered for mail 
                        {
                            switch (statusPurchased)
                            {
                                case -1: //case gift card type is mail and gift card status is not selected
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.NoSend);
                                    break;
                                case 2: //case gift card type is mail and gift card status is 'ordered for standard shipping'
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.NoSend && u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaFreeStandardShipping);
                                    break;
                                case 3: //case gift card type is mail and gift card status is 'ordered for standard shipping'
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.NoSend && u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaPreminumShipping);
                                    break;
                            }
                        }
                        else//case sent via mail 
                        {
                            switch (statusPurchased)
                            {
                                case -1: //case gift card type is mail and gift card status is not selected
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.Send);
                                    break;
                                case 4: //case gift card type is mail and gift card status is 'ordered for standard shipping'
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.Send && u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaFreeStandardShipping);
                                    break;
                                case 5: //case gift card type is mail and gift card status is 'ordered for standard shipping'
                                    query =
                                        query.Where(
                                            u =>
                                                u.GiftCardType == (int)Types.GiftCardType.mail && u.GiftStatus == (int)Types.GiftCardStatus.Send && u.ShippingMethod == (int)Types.GiftCardShippingMethod.ViaPreminumShipping);
                                    break;
                            }
                        }
                    }
                }
            }
            totalRecord = query.Count();
            return query.OrderByDescending(u => u.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
