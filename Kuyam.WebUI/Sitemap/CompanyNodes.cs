using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kuyam.Database;
using Kuyam.Domain.KuyamServices;
using Kuyam.Domain.LandingePageServices;
using MvcSiteMapProvider;
using Kuyam.Domain.Seo;

namespace Kuyam.WebUI.Sitemap
{
    public abstract class CompanyBaseNodes : DynamicNodeProviderBase
    {
        const string ItemName = "_sitemapCompanies";
        protected IEnumerable<ProfileCompany> Companies
        {
            get
            {
                if (HttpContext.Current.Items[ItemName] == null)
                {
                    var profileCompanyService = (IProfileCompanyService)DependencyResolver.Current.GetService(typeof(IProfileCompanyService));
                    var companies = profileCompanyService.GetAll().Where(c => c.CompanyStatusID == (int)Types.CompanyStatus.Active).ToList();
                    HttpContext.Current.Items[ItemName] = companies;
                }
                return (IEnumerable<ProfileCompany>)HttpContext.Current.Items[ItemName];
            }
        }

        protected string ParentNode(ProfileCompany company)
        {
            return "Company_" + company.ProfileID;
        }
    }

    public class CompanyNodes : CompanyBaseNodes
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            foreach (var company in Companies)
            {
                var dynamicNode = new DynamicNode(ParentNode(company), company.Name);
                dynamicNode.Title = company.Name;
                dynamicNode.Controller = "CompanyProfile";
                dynamicNode.Action = "Availability";
                dynamicNode.RouteValues.Add("seName", company.GetSeName(company.ProfileID));
                dynamicNode.LastModifiedDate = company.Modified;
                yield return dynamicNode;
            }
        }
    }

}