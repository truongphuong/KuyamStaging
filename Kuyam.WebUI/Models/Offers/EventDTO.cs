using Kuyam.Database;
using Kuyam.Database.Extensions;
using Kuyam.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models.Offers
{
    public class EventDTO
    {
        public EventDTO(Kuyam.Database.Event ev)
        {
            this.EventID = ev.EventID;
            this.Name = ev.Name;
            this.StartDate = ev.StartDate;
            this.EndDate = ev.EndDate;
            this.Description = ev.Description;
            this.Created = ev.Created;
            this.Modified = ev.Modified;
        }
        public int EventID { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string Description { get; set; }
    }
}