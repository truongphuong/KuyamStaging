using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class ReceiptIP
    {        
        public string ReceiptNumber { get; set; }
        public string PurchasedOn { get; set; }      
        public int AppointmentID { get; set; }
        public int PaymentMethod { get; set; }
        public bool isPaid { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal PercentPaymentFee { get; set; }
        public decimal TransactionAdditionalFee { get; set; }
        public decimal PaymentFeeTotal { get; set; }
        public decimal PercentKuyamFee { get; set; }
        public decimal AppointmentAdditionalFee { get; set; }
        public decimal KuyamFeeTotal { get; set; }
        public string DiscountCodeNumber { get; set; }
        public decimal OrderSubtotal { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
