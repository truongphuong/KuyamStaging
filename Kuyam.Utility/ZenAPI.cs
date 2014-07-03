using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Utility
{
    public class ZenAPI
    {
        static string requestUri = ConfigurationManager.AppSettings["zendeskURI"];//https://kuyam.zendesk.com/
        static string username = ConfigurationManager.AppSettings["username"];//huyphan@vinasource.com
        static string password = ConfigurationManager.AppSettings["password"];//huyphanvns        
        static string _groupid = ConfigurationManager.AppSettings["groupid"];//Test:20648228;QA1:20654376            

        public static void CreateTicket(string name, TicketStatus status, TicketType type, TicketPriority priority, string comment, string groupid = "")
        {
            var client = new RestSharp.RestClient(requestUri);
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            var request = new RestRequest("/api/v2/tickets.json", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            NewTicket createTicket = new NewTicket();
            if (string.IsNullOrEmpty(groupid))
            {
                groupid = _groupid;
            }
            createTicket.ticket = new Ticket
            {
                subject = name,
                status = TicketStatus.New.ToString(),
                type = ((TicketType)type).ToString(),
                priority = ((TicketPriority)priority).ToString(),
                comment = new Comment { value = comment },
                group_id = Int32.Parse(groupid),
                assignee_id = 0

            };

            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(createTicket);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            Console.WriteLine(content);
        }
    }


    /// <summary>
    /// Defineed by Zendesk API
    /// </summary>
    public class FullTicket
    {
        public int id { get; set; }
        public string url { get; set; }
        public string external_id { get; set; }
        public string type { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public string priority { get; set; }
        public string status { get; set; }
        public string recipient { get; set; }
        public int requester_id { get; set; }
        public int submitter_id { get; set; }
        public int assignee_id { get; set; }
        public int organization_id { get; set; }
        public int group_id { get; set; }
    }

    public class NewTicket
    {
        public Ticket ticket { get; set; }
    }

    public class Ticket
    {
        public string subject { get; set; }
        public Comment comment { get; set; }
        public string priority { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string[] Tags { get; set; }
        public int group_id { get; set; }
        public int requester_id { get; set; }
        public int assignee_id { get; set; }

    }

    public class Comment
    {
        public string value { get; set; }
    }

    public enum TicketStatus
    {
        New,
        Open,
        Pending,
        Solved,
        Closed
    }

    public enum TicketType
    {
        Problem,
        Incident,
        Question,
        Task
    }

    public enum TicketPriority
    {
        Urgent,
        High,
        Normal,
        Low
    }
}
