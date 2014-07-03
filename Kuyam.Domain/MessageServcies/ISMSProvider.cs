using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.MessageServcies
{
    public partial interface ISMSProvider
    {
        SendSMSResult SendSms(string message, string[] PhoneNumber, bool notifyDeliveryStatus = false);

        SendSMSResult SendSms(string[] PhoneNumber, string subject, string message, bool groupflag, ArrayList attachments = null);

        MessageHeaderList GetMessageHeaders(string hCount, string iCursor);

        MessageList GetMessageList(string limit, string offset);

        void CreateMessageIndex();
    }
}
