using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Database;
using Kuyam.Domain.BlogServices;
using Kuyam.Database.BlogModels;
using Kuyam.Domain;
using System.Configuration;
using Kuyam.Domain.GiftCardServices;
using Kuyam.Domain.KuyamServices;
using Kuyam.Utility;
using Kuyam.WebUI.InfoConnServiceReference;
using Kuyam.WebUI.Models;
using Kuyam.WebUI.WebMapper;
using Kuyam.WebUI.Extension;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Helpers;
using M2.Util;
using Kuyam.Domain.CompanyProfileServices;

namespace Kuyam.WebUI.Controllers
{
    public class GiftCardController : KuyamBaseController
    {
        private readonly IGiftCardServices _giftCardServices;
        private readonly IAppointmentService _appointmentService;
        private readonly CompanyProfileService _companyProfileService;
        public GiftCardController(IGiftCardServices giftCardServices,
            IAppointmentService appointmentService,
            CompanyProfileService companyProfileService)
        {
            _giftCardServices = giftCardServices;
            _appointmentService = appointmentService;
            _companyProfileService = companyProfileService;
        }

        [NonAction]
        public ActionResult BuyGiftCardStyle()
        {
            return View();
        }

        [NonAction]
        public ActionResult GiftCardBalanceStyle()
        {
            return View();
        }

        [Authorize]
        public ActionResult BuyGiftCard()
        {
            //List<AppointmentParticipant> result = _appointmentService.GetListAppointmentParticipantCustID(MySession.CustID, null, DateTime.Now, DateTime.Now.AddDays(7), false);
            //ViewBag.CalendarList = _companyProfileService.GetCalendarByCustId(MySession.CustID);
            //ViewBag.Category = _appointmentService.GetListService();
            //ViewBag.HtmlData = Generation(result);
            //SendEmailToUser(null, "GiftCardReceipt", "Kuyam giftcard");
            return View();
        }

        public ActionResult BuyGiftCard_Email()
        {
            return PartialView("GiftCardPartial/_ByGiftCardEMailForm");
        }

        public ActionResult BuyGiftCard_Mail()
        {

            return PartialView("GiftCardPartial/_ByGiftCardMailForm");
        }

        [Authorize]
        public ActionResult GiftCardBalance()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GiftCardBalance(string giftCode)
        {
            var giftCard = _giftCardServices.GetGiftCardByGiftCardCode(giftCode, MySession.CustID);
            var usedAmount = string.Empty;
            if (giftCard.IsFailed)
            {
                return Json(new { giftAmount = 0, used = "$0.00", isLock = false, IsFailed = true }, JsonRequestBehavior.AllowGet);
            }
            else if (giftCard.IsLocked)
            {
                return Json(new { giftAmount = 0, used = "$0.00", isLock = true, IsFailed = true }, JsonRequestBehavior.AllowGet);
            }
            decimal giftBlance = 0;
            if (giftCard.UsedValue == null)
            {
                var giftUsed = giftCard.GiftCardHistories.Where(a => a.GiftCardId == giftCard.Id);
                
                if (giftUsed.Any())
                {
                    var used = giftUsed.Sum(a => a.UsedValue);
                    giftBlance = (giftCard.Amount - used);
                    usedAmount = giftBlance.ToString("c");
                }
                else
                {
                    giftBlance = giftCard.Amount;
                    usedAmount = giftCard.Amount.ToString("c");
                }
            }
            else
            {
                usedAmount = giftCard.UsedValue.Value.ToString("c");
            }
            
            return Json(new { giftAmount = giftBlance, used = usedAmount, isLock = false, IsFailed = false }, JsonRequestBehavior.AllowGet);//View(model);
        }

        [HttpPost]
        public ActionResult PurchaseWithPayment(
                string amount,
                string type,
                string recipientName,
                string recipientEmail,
                string ownerName,
                string message,
                string city,
                string state,
                string zipcode,
                string address1,
                string address2,
                string typeShipping,
                string costShipping,
                string estimateDate)
        {

            var giftCardInfo = new GiftCard
                               {
                                   Amount = decimal.Parse(amount),
                                   Created = DateTime.UtcNow,
                                   GiftCardType = int.Parse(type),
                                   RecipientName = recipientName,
                                   RecipientEmail = recipientEmail,
                                   Message = message,
                                   SenderName = ownerName,
                                   IsActivated = false,
                                   ShippingMethod = string.IsNullOrEmpty(typeShipping) ? 0 : int.Parse(typeShipping),
                                   ShippingCost = !string.IsNullOrEmpty(costShipping) ? decimal.Parse(costShipping) : 0,
                                   City = city,
                                   State = state,
                                   Address1 = address1,
                                   Address2 = address2,
                                   ZipCode = zipcode,
                                   ReceiveNumber = UtilityHelper.GenerateRandomDigitCode(8),
                                   GiftCardCode = UtilityHelper.GenerateRandomDigitCode(16),
                                   EstimateDate = estimateDate,
                                   SenderEmail = MySession.Cust.Email,
                                   CustId = MySession.CustID
                               };
            var result = _giftCardServices.BuyGiftCard(giftCardInfo);
            if (result != null)
            {
                var redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"];
                var totalAmount = result.Amount + result.ShippingCost;
                return RedirectToAction("BuyGiftCard", "PayPal", new { id = result.Id, amount = totalAmount.ToString() });
                //return Json(new { url = "url"}, JsonRequestBehavior.AllowGet);

            }
            return Json(new { url = string.Empty }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GiftCardPurchased(string reval)
        {
            var idDecrypt = string.Empty;
            try
            {
                idDecrypt = SecurityHelper.DecryptStringFromBytesAes(reval);
            }
            catch (Exception)
            {
                return RedirectToAction("Error404", "Error");
            }
            var model = new GiftCardModels();

            var giftCard = _giftCardServices.GetGiftCardById(int.Parse(idDecrypt));
            if (giftCard != null)
            {
                model.Id = giftCard.Id;
                model.Amount = giftCard.Amount.ToString("c");
                model.Address1 = giftCard.Address1;
                model.Address2 = giftCard.Address1;
                model.City = giftCard.City;
                model.EstimateDate = giftCard.EstimateDate;
                model.GiftCardCode = giftCard.GiftCardCode;
                model.ReceiveNumber = giftCard.ReceiveNumber;
                model.ShippingCost = giftCard.ShippingCost.Value.ToString("c");
                model.Created = giftCard.Created.ToString("MMMM dd, yyyy");
                model.GiftCardType = giftCard.GiftCardType;
                model.ShippingMethod = giftCard.ShippingMethod.Value;
                model.RecipientName = UppercaseWords(giftCard.RecipientName);
                model.TransactionId = giftCard.TransactionID;
                var inAmount = (int)giftCard.Amount;
                model.IntAmount = "$" + inAmount.ToString();
                var totalPayment = giftCard.Amount + giftCard.ShippingCost;
                model.PayAmount = totalPayment.Value.ToString("c");
                var sess = Session["isActive"];
                if (sess != null && (bool)sess)
                {
                    //Session["isActive"] = null;
                    //Send email to owner
                    SendEmailToUser(giftCard, "GiftCardReceipt", "kuyam gift card", true);

                    //Send email to receipt
                    if (giftCard.GiftCardType == (int)Types.GiftCardType.email)
                    {
                        //mark via email
                        MarkSendEmail(giftCard);
                        SendEmailToUser(giftCard, "GiftCard", "kuyam gift card receipt", false);
                    }

                    //Create ticket

                    CreateZenTicket(giftCard, "gift card purchase");
                }


            }
            return View(model);
        }
        public ActionResult GetPaymentSuccess(int id)
        {
            return RedirectToAction("GetTransactionId", "PayPal", new { idGiftCard = id });
        }
        public ActionResult GetPaymentFail()
        {
            return RedirectToAction("BuyGiftCard");
        }

        [NonAction]
        public ActionResult GiftCardReceiptStyle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetEstimateDate()
        {
            DateTime time = DateTimeUltility.ConvertToPstTime(DateTime.Now);
            DateTime compare = DateTime.Parse(time.ToShortDateString() + " 2:00PM");
            bool greater = (time > compare);
            var currentDate = time;
            var estimateDateFree1 = string.Empty;
            var estimateDatePremium1 = string.Empty;
            var formatDate = "MMM d";
            var formatDate2 = "MMM d yyyy";
            var formatMonth = "MMM";
            if (greater)
            {
                var date4 = currentDate.AddDays(4);
                var date6 = currentDate.AddDays(7);
                if (date4.Month == date6.Month)
                {
                    var month = date4.ToString(formatMonth, CultureInfo.InvariantCulture);
                    estimateDateFree1 = month + " " + date4.Day + "-" + date6.Day;
                }
                else
                {
                    estimateDateFree1 = date4.ToString(formatDate) + "-" + date6.ToString(formatDate2);
                }

                estimateDatePremium1 = currentDate.AddDays(3).ToString(formatDate);
            }
            else
            {
                var date3 = currentDate.AddDays(3);
                var date5 = currentDate.AddDays(6);
                if (date3.Month == date5.Month)
                {
                    var month = date3.ToString(formatMonth, CultureInfo.InvariantCulture);
                    estimateDateFree1 = month + " " + date3.Day + "-" + date5.Day;
                }
                else
                {
                    estimateDateFree1 = date3.ToString(formatDate) + "-" + date5.ToString(formatDate2);
                }
                estimateDatePremium1 = currentDate.AddDays(2).ToString(formatDate);
            }
            return Json(new { estimateDateFree = estimateDateFree1, estimateDatePremium = estimateDatePremium1 },
                JsonRequestBehavior.AllowGet);
        }

        public void SendEmailToUser(GiftCard gift, string templateName, string subject, bool isOwner)
        {
            //Send email to user
            try
            {
                string templateEmail;
                var nameUsers = gift.RecipientName.Split(new char[] { ' ' });
                var nameUsers1 = gift.SenderName.Split(new char[] { ' ' });
                string typeShipping = string.Empty;
                if (gift.ShippingMethod == 1)
                {
                    typeShipping = "FREE standard shipping";
                }
                if (gift.ShippingMethod == 2)
                {
                    typeShipping = "premium shipping";
                }
                var totalCost = gift.Amount + gift.ShippingCost;
                dynamic firstalertObject = new
                {
                    RecipentNameFirsName = UppercaseWords(nameUsers[0]),
                    SenderNameFirstName = UppercaseWords(nameUsers1[0]),
                    DateSendEmail = DateTime.Now.ToString("D"),
                    FullNameRecipentName = UppercaseWords(gift.RecipientName),
                    FullNameSender = UppercaseWords(gift.SenderName),
                    Memo = gift.Message,
                    GiftCardNumber = gift.GiftCardCode,
                    TransactionId = gift.TransactionID,
                    ReceiveNumber = gift.ReceiveNumber,
                    ShoppingCost = gift.ShippingCost.Value.ToString("c"),
                    AmountGift = gift.Amount.ToString("c"),
                    DateOfPurchase = gift.Created.ToString("MMMM dd, yyyy"),
                    TotalCost = totalCost.Value.ToString("c"),
                    TypeShipping = typeShipping,
                    GiftEstimateDate = gift.EstimateDate,
                    AmoutNumber = (int)gift.Amount,
                    CardType = gift.GiftCardType
                }.ToExpando();
                templateEmail = this.RenderPartialViewToString(templateName, (object)firstalertObject);
                string sender = string.Empty;
                string sender1 = string.Empty;
                if (gift.GiftCardType == 2)
                {
                    sender = gift.SenderEmail;
                    sender1 = gift.SenderEmail;
                }
                else
                {
                    sender = gift.SenderEmail;
                    sender1 = gift.RecipientEmail;
                }
                //sender = sender1 = "phongvo@vinasource.com";
                EmailHelper.SendMail(sender, isOwner ? sender : sender1, subject, templateEmail);
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("{0} send email err {1} ", subject, ex.StackTrace.ToString()));
            }

        }
        public string UppercaseWords(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
        public void CreateZenTicket(GiftCard giftCard, string subject)
        {
            if (ConfigurationManager.AppSettings["KuyamVersion"] != null &&
                       int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
                subject = "[QA] " + subject;
            else if (ConfigurationManager.AppSettings["KuyamVersion"] != null &&
                     int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
                subject = "[DEV] " + subject;
            var status = (TicketStatus.Pending);
            var type = (TicketType.Incident);
            var priority = (TicketPriority.High);
            string description = string.Empty;
            if (giftCard.GiftCardType == 1)
            {
                description = "gift card purchase: \n"
                              + "date: " + giftCard.Created.ToString("d") + "\n"
                              + "bought by: " + UppercaseWords(giftCard.SenderName) + "," + giftCard.SenderEmail + "\n"
                              + "delivery: email" + "\n"
                              + "sent to:" + UppercaseWords(giftCard.RecipientName) + "," + giftCard.RecipientEmail;
            }
            else
            {
                var typeShipping = giftCard.ShippingMethod == 1 ? "FREE" : "premium";
                description = "gift card purchase: \n"
                              + "date: " + giftCard.Created.ToString("d") + "\n"
                              + "bought by: " + UppercaseWords(giftCard.SenderName) + "," + giftCard.SenderEmail + "\n"
                              + "delivery: postal mail" + "\n"
                              + "shipping:" + typeShipping + "\n"
                              + "sent to:" + UppercaseWords(giftCard.Address1) + " ," + UppercaseWords(giftCard.City) + " ," + UppercaseWords(giftCard.State) + " ," + giftCard.ZipCode;
            }
            ZenAPI.CreateTicket(subject, status, type, priority, description);
        }

        public void MarkSendEmail(GiftCard giftCard)
        {
            try
            {
                giftCard.GiftStatus = (int)Types.GiftCardStatus.Send;
                _giftCardServices.UpdateGiftCardInfo(giftCard);
            }
            catch (Exception ex)
            {

                LogHelper.Error(string.Format("Mark send gift card {0} error {1}", giftCard.Id, ex.StackTrace.ToString()));
            }
        }
    }
}
