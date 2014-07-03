using System;
using System.Collections.Generic;
using Kuyam.Database;
using System.Linq;

namespace Kuyam.Domain.SmsServices
{
    public interface ISmsServices
    {
        List<SMSMessage> GetAllMessage(int cusId);

        List<string> GetAll(int headerCount);

        bool AddMessage(SMSMessage smsMessage);

        SMSMessage GetMessageByMessageIdAndCustId(string messageId, int cusId);

        bool UpdateMessage(SMSMessage smsMessage);

        List<Cust> GetAllCust();

        int DeleteSmsByStatus(int status = (int)Types.DeliveryStatus.Temp);
    }

}