// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;

namespace M2.Util
{
    public static class SMSClient
    {
        public static string From { get; set; }
        public static string Number { get; set; }
        public static CarrierIDs CarrierID { get; set; }
        public static string Subject { get; set; }

		public static bool IsActive
		{
			get
			{
				return !Number.IsNullOrEmpty();
			}
		}

        public class Carrier
        {
            public string Name { get; set; }
            public string Domain { get; set; }

            public Carrier(string name, string domain)
            {
                Name = name;
                Domain = domain;
            }
        }

        // Must match Carriers enum
        public static List<Carrier> CarrierList = new List<Carrier>()
        {
            new Carrier("AT&T", "txt.att.net"),
            new Carrier("AT&T MMS", "MMS.att.net"),
            new Carrier("Alltel", "message.alltel.com"),
            new Carrier("Cingular", "cingularme.com"),
            new Carrier("Metro PCS", "MyMetroPcs.com"),
            new Carrier("Nextel", "messaging.nextel.com"),
            new Carrier("Powertel", "ptel.net"),
            new Carrier("Sprint", "messaging.sprintpcs.com"),
            new Carrier("SunCom", "tms.suncom.com"),
            new Carrier("T-Mobile", "tmomail.net"),
            new Carrier("US Cellular", "email.uscc.net"),
            new Carrier("Verizon", "vtext.com"),
            new Carrier("Virgin Mobile", "vmobl.com")
        };

        // Must match CarrierAddresses dictionary
        public enum CarrierIDs
        {
            ATT,
            ATTMMS,
            Alltel,
            Cingular,
            MetroPCS,
            Nextel,
            Powertel,
            Sprint,
            SunCom,
            TMobile,
            USCellular,
            Verizon,
            VirginMobile
        }

        /// <summary>
        /// In seconds.  Set to 0 for no timeout.
        /// </summary>
        public static int DupeIntervalTimeout { get; set; }

        private static DateTime _lastSendDT = DateTime.Now;
        private static string _lastSend = null;

        static SMSClient()
        {
            DupeIntervalTimeout = 60;
        }

        public static void Send(string body)
        {
            Send(Number, CarrierID, From, Subject, body);
        }

        public static void Send(string number, string subject, string body)
        {
            Send(number, CarrierID, From, subject, body);
        }

        public static void Send(string number, CarrierIDs carrier, string subject, string body)
        {
            Send(number, carrier, From, subject, body);
        }

        public static void Send(string number, CarrierIDs carrier, string from, string subject, string body)
        {
			if (number.IsNullOrEmpty())
				return;

            string encoded = GetEncoded();
            if (encoded != _lastSend || (DateTime.Now - _lastSendDT).TotalSeconds >= DupeIntervalTimeout)
            {
                SMTPClient.SendMessage(from, number + "@" + CarrierList[(int)carrier].Domain, subject, body);
                _lastSendDT = DateTime.Now;
                _lastSend = encoded;
            }
        }

        private static string GetEncoded()
        {
            return String.Format("{0}|{1}|{2}|{3}", From, Number, CarrierID.ToInt32().ToString(), Subject);
        }
    }
}
