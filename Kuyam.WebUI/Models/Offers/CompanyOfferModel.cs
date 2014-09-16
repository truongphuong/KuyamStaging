using Kuyam.WebUI.Models.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Models
{
    public class CompanyOfferModel
    {
        public string SlugName { get; set; }
        public EventDTO Event { get; set; }
        public List<CompanyServiceEventDTO> Offers { get; set; }
    }
}