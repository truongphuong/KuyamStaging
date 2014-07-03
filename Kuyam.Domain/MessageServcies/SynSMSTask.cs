using Kuyam.Database;
using Kuyam.Domain.SmsServices;
using Kuyam.Domain.Tasks;
using Kuyam.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kuyam.Domain.MessageServcies
{
    public partial class SynSMSTask : ITask 
    {

        private readonly ISMSProvider _smsProvider;
        private readonly ISmsServices _smsServices;

        public SynSMSTask(ISMSProvider smsProvider, ISmsServices smsServices)
        {
            this._smsProvider = smsProvider;
            this._smsServices = smsServices;
        }

        private static int _total = 0;

        public void Execute()
        {
            int hCount = 10;

            string phoneinApp = ConfigurationManager.AppSettings["PhoneInapp"];
            var messageHeadersList = _smsProvider.GetMessageList(hCount.ToString(), "0");
            if (messageHeadersList == null || (_total > 0 && messageHeadersList.total == _total))
            {
                _smsProvider.CreateMessageIndex();
                messageHeadersList = null;                
                return;
            }


            if (_total == 0)
            {
                var query = _smsServices.GetAll(int.MaxValue);
                _total = query.Count();
            }

            int take = messageHeadersList.total - _total;

            List<Message> newMessage = null;
            if (take > hCount)
            {
                messageHeadersList = _smsProvider.GetMessageList(take.ToString(), "0");
                newMessage = messageHeadersList.messages.Skip(0).Take(take).ToList();
            }
            else
            {
                newMessage = messageHeadersList.messages.Skip(0).Take(take).ToList();
            }

            int? custId = null;
            if (newMessage != null && newMessage.Count() > 0)
            {
                _total = messageHeadersList.total;
                var custQuery = _smsServices.GetAllCust();
                newMessage.Reverse();
                _smsServices.DeleteSmsByStatus((int)Types.DeliveryStatus.Temp);
                foreach (Message item in newMessage)
                {
                    char[] separators = new char[] { '+', '1' };
                    string from = item.from.value.TrimStart(separators);
                    string to = item.recipients[0].value.ToString().TrimStart(separators);
                    string formatPhonefrom = UtilityHelper.FormatPhone(from);
                    string formatPhoneTo = UtilityHelper.FormatPhone(to);

                    var custs = custQuery.Where(m => (m.MobilePhone == from || m.MobilePhone == formatPhonefrom) || (m.MobilePhone == to || m.MobilePhone == formatPhoneTo)).ToList();
                    if (custs != null && custs.Count() > 0)
                    {
                        foreach (var cust in custs)
                        {
                            custId = cust.CustID;
                            int receivedStatus = (int)Types.ReceiveStatus.Sender;
                            if (item.messageId.StartsWith("r"))
                                receivedStatus = (int)Types.ReceiveStatus.Receive;
                            var message = new SMSMessage
                            {
                                MessageId = item.messageId,
                                Message = item.text,
                                SenderAddress = from,
                                DestinationAddress = to,
                                DateTime = DateTime.Parse(item.timeStamp),
                                CustId = custId,
                                DeliveryStatus = (int)Types.DeliveryStatus.Send,
                                ReceivedStatus = receivedStatus

                            };
                            _smsServices.AddMessage(message);
                            if (item.isIncoming=="True")
                            {
                                NotificationZendeskTicket(cust, "new SMS message received.", item.text, message.SenderAddress);
                            }
                        }

                    }
                    else
                    {
                        var message = new SMSMessage
                        {
                            MessageId = item.messageId,
                            Message = item.text,
                            SenderAddress = from,
                            DestinationAddress = to,
                            DateTime = DateTime.Parse(item.timeStamp),
                            CustId = null,
                            DeliveryStatus = (int)Types.DeliveryStatus.Send,
                            ReceivedStatus = (int)Types.ReceiveStatus.Receive

                        };
                        _smsServices.AddMessage(message);
                        if (item.isIncoming=="True")
                            NotificationZendeskTicket(null, "unrecognized messages", item.text, message.SenderAddress);
                    }

                }

            }
        }


        public void NotificationZendeskTicket(Cust cust, string subject, string message, string phoneNumber = "")
        {
            string url = ConfigurationManager.AppSettings["WebHost"];
            string groupId = string.Empty;
            if (phoneNumber == "5719697135" || ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
            {
                subject = "[QA] " + subject;
                groupId = "20654376";

            }
            else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
            {
                subject = "[DEV] " + subject;

            }


            var status = (TicketStatus.Pending);
            var type = (TicketType.Incident);
            var priority = (TicketPriority.High);

            StringBuilder description = new StringBuilder();

            if (cust != null)
            {
                description.AppendFormat("name: {0} {1} \n", cust.FirstName, cust.LastName);
                description.AppendFormat("message: {0} \n", message);
                description.AppendFormat("link to user page: {0}admin/AdminUserDetail/{1}?userListPageIndex=1\n", url, cust.CustID);
            }
            else
            {
                description.AppendFormat("phone: {0} \n", phoneNumber);
                description.AppendFormat("message: {0} \n", message);
            }

            ZenAPI.CreateTicket(subject, status, type, priority, description.ToString(), groupId);
        }

    }
}
