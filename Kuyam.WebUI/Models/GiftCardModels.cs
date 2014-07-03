using System.Collections.Generic;
using Kuyam.Database;
using System;
using System.Linq;
using Kuyam.Database.BlogModels;

namespace Kuyam.WebUI.Models
{
    public class GiftCardModels
    {
        public int Id { get; set; }
        public int GiftCardType { get; set; }
        public string Amount { get; set; }
        public bool IsActivated { get; set; }
        public string GiftCardCode { get; set; }
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Message { get; set; }
        public bool IsRecipientNotified { get; set; }
        public string Created { get; set; }
        public decimal UsedValue { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int ShippingMethod { get; set; }
        public string ShippingCost { get; set; }
        public string EstimateDate { get; set; }
        public string ReceiveNumber { get; set; }
        public string PayAmount { get; set; }
        public string TransactionId { get; set; }
        public string IntAmount { get; set; }
    }
}
