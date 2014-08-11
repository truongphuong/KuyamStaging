using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models.Offers
{
    public class OfferModel
    {
        public OfferModel()
        {
            ServiceOffers = new List<CompanyServiceEventDTO>();
            ClassOffers = new List<CompanyServiceEventDTO>();
        }
        public string SlugName { get; set; }
        public EventDTO Event { get; set; }
        public List<CompanyServiceEventDTO> ServiceOffers { get; set; }
        public List<CompanyServiceEventDTO> ClassOffers { get; set; }
    }
}