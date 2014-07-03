using System.Collections.Generic;

namespace Kuyam.Domain.Admin
{
    public class NewsletterInfoItemGroup
    {
        public string Name { get; set; }
        public List<NewsletterInfoDetail> InfoDetails { get; set; }

        public NewsletterInfoItemGroup()
        {
            InfoDetails = new List<NewsletterInfoDetail>();
        }
    }
}