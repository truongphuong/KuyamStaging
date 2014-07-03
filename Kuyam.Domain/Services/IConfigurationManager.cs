using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.Services
{
   public interface IConfigurationManager
    {
        string GetAppSetting(string key);
        ConnectionStringSettings GetConnectionString(string key);
    }
}
