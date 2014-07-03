using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    /// <summary>
    /// 
    /// </summary>
	public partial class SmsMessage
	{
        public int MessageId { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MessageContent { get; set; }
        public string ClientId { get; set; }
        public string SystemId { get; set; }
	}
}
