using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
	public class SmsModels
	{
        public int Id { get; set; }
        public string MessageId { get; set; }
        public int CustId { get; set; }
        public string Message { get; set; }
        public string SenderAddress { get; set; }
        public string DestinationAddress { get; set; }
        public System.DateTime DateTime { get; set; }
        public int? DeliveryStatus { get; set; }
        public string ErrorCode { get; set; }
        public string DecriptionStatus { get; set; }
        public int ReceivedStatus { get; set; }
	}
}