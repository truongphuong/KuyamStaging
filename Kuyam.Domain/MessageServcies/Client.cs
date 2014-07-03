using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kuyam.Domain.MessageServcies
{/// <summary>
    /// This class represents a web client which can receive messages.
    /// </summary>
    public class Client
    {
        private ManualResetEvent messageEvent = new ManualResetEvent(false);
        private Queue<SMSMessage> messageQueue = new Queue<SMSMessage>();

        /// <summary>
        /// This method is called by a sender to send a message to this client.
        /// </summary>
        /// <param name="message">the new message</param>
        public void EnqueueMessage(SMSMessage message)
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(message);

                // Set a new message event.
                messageEvent.Set();
            }
        }

        /// <summary>
        /// This method is called by the client to receive messages from the message queue.
        /// If no message, it will wait until a new message is inserted.
        /// </summary>
        /// <returns>the unread message</returns>
        public SMSMessage DequeueMessage()
        {
            // Wait until a new message.
            messageEvent.WaitOne();

            lock (messageQueue)
            {
                if (messageQueue.Count == 1)
                {
                    messageEvent.Reset();
                }
                return messageQueue.Dequeue();
            }
        }
    }
}
