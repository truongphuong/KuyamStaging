using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database.Extensions
{
    public class CompanyInvoices
    {
        public DateTime ServiceStartDate { get; set; }
        public string ServiceDescription { get; set; }
        public DateTime? PurchasedOn { get; set; }
        public string EmployeeName { get; set; }
        public string ClientName { get; set; }
        public bool IsRegular { get; set; }
        public decimal? ServiceAmmount { get; set; }
        public string CompanyName { get; set; }
        public int AppointmentStatus { get; set; }
        public int PaymentMethod { get; set; }
        public int? Duration { get; set; }
        public string ServiceName { get; set; }
        public string ReceiptNumber { get; set; }
        public decimal? OrderSubTotalDiscount { get; set; }
        public decimal? OrderDiscount { get; set; }
        public decimal? PercentPaymentFee { get; set; }
        public decimal? TransactionAdditionalFee { get; set; }
        public decimal? PaymentFeeTotal { get; set; }
        public decimal? PercentKuyamFee { get; set; }
        public decimal? AppointmentAdditionalFee { get; set; }
        public decimal? KuyamFeeTotal { get; set; }
        public string DiscountCodeNumber { get; set; }
        public string GiftCardCodeNumber { get; set; }
        public decimal? GiftCardAmount { get; set; }
        public decimal? OrderSubtotal { get; set; }
        public decimal? OrderTotal { get; set; }
        public int DiscountType { get; set; }
    }
}
