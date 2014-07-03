using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public partial class Hotel
    {
        /// <summary>
        /// Gets the media list.
        /// </summary>
        /// <returns></returns>
        public List<Medium> GetMediaList()
        {
            return HotelMedias.Select(m => m.Medium).ToList();
        }
    }
}
