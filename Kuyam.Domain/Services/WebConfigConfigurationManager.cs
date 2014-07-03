using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Kuyam.Domain.Services
{
    public class WebConfigConfigurationManager : IConfigurationManager
    {
        public string GetAppSetting(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }

        public ConnectionStringSettings GetConnectionString(string key)
        {
            return WebConfigurationManager.ConnectionStrings[key];
        }
    }
}
