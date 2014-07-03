using Kuyam.Database;

namespace Kuyam.Domain.Admin
{
    public class NewsletterInfoDetail
    {
        public string CompanyName { get; set; }
        public string CompanyDescripton { get; set; }
        public string CompanyImage { get; set; }
        public string CompanyId { get; set; }

        public Types.FeatureCompanyType FeatureCompanyType { get; set; }

        public string DescriptionTruncate
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyDescripton))
                    return string.Empty;
                int maxlength = 150;
                if (CompanyDescripton.Length <= maxlength)
                    return CompanyDescripton;
                int index = maxlength-1;
                while (CompanyDescripton[index] != ' '&& index>0)
                {
                    index = index - 1;
                }
                return CompanyDescripton.Substring(0, index)+"...";
            }
        }


    }
}