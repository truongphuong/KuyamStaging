using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.HipmobServices
{
    public interface IHipmobService
    {
        bool SendTextMessage(string deviceId, string text);
    }
}
