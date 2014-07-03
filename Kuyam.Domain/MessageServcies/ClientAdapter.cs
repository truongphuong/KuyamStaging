using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.MessageServcies
{
    /// <summary>
    /// This class is used to send events/messages to multiple clients.
    /// </summary>
    public class ClientAdapter
    {
        /// <summary>
        /// The recipient list.
        /// </summary>
        private Dictionary<string, Client> recipients = new Dictionary<string, Client>();

        /// <summary>
        /// Send a message to a particular recipient.
        /// </summary>
        public void SendMessage(SMSMessage message)
        {
            if (recipients.ContainsKey(message.CustId.ToString()))
            {
                Client client = recipients[message.CustId.ToString()];

                client.EnqueueMessage(message);
            }
        }

        public void ReciveMessage(SMSMessage message)
        {
            if (recipients.ContainsKey(message.CustId.ToString()))
            {
                Client client = recipients[message.CustId.ToString()];

                client.EnqueueMessage(message);
            }
        }

        /// <summary>
        /// Called by a individual recipient to wait and receive a message.
        /// </summary>
        /// <returns>The message content</returns>
        public string GetMessage(string userName)
        {
            string messageContent = string.Empty;

            if (recipients.ContainsKey(userName))
            {
                Client client = recipients[userName];

                messageContent = client.DequeueMessage().Message;
            }

            return messageContent;
        }

        /// <summary>
        /// Join a user to the recipient list.
        /// </summary>
        public void Join(string userName)
        {
            recipients[userName] = new Client();
        }

        /// <summary>
        /// Singleton pattern.
        /// This pattern will ensure there is only one instance of this class in the system.
        /// </summary>
        public static ClientAdapter Instance = new ClientAdapter();
        private ClientAdapter() { }
    }
}
