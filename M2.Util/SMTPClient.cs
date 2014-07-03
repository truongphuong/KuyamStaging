// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;

namespace M2.Util
{
	public static class SMTPClient
	{
		public static string Host { get; set; }
		public static int Port { get; set; }
		public static bool EnableSSL { get; set; }
		public static string From { get; set; }
		public static string Username { get; set; }
		public static string Password { get; set; }
		public static bool GoAsync { get; set; }

		private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			// Get the unique identifier for this asynchronous operation.
			String token = (string)e.UserState;

			if (e.Cancelled)
			{
				Logger.Info("SMTPClient", String.Format("{0}: Send cancelled.", token));
			}
			else if (e.Error != null)  // TODO: Sometimes getting errors but message goes through
			{
				throw new Exception(e.Error.ToString());
				//Logger.Error("SMTPClient.SCC", String.Format("{0}: {1}", token, e.Error.ToString()));
			}
		}

		public static void SendMessage(string to, string subject, string body, bool isBodyHtml = true, string tag = null)
		{
			SendMessage(null, to, subject, body, isBodyHtml, tag);
		}

		public static void SendMessage(string from, string to, string subject, string body, bool isBodyHtml = true, string tag = null)
		{
			SmtpClient client = new SmtpClient();
			if (from.IsNullOrEmpty())
				from = From;
			client.Host = Host;
			client.Port = Port;
			client.EnableSsl = EnableSSL;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(Username, Password);
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			MailMessage message = new MailMessage(from, to, subject, body);
			message.IsBodyHtml = isBodyHtml;

			//client.Send(message);
			try
			{
				if (GoAsync)
				{
					client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
					client.SendAsync(message, tag);
				}
				else
				{
					client.Timeout = 10000;
					client.Send(message);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
				//Logger.Error("SMTPClient.SendMessage", "Error: " + ex.Message);
			}

		}
	}

}
