using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kuyam.WebUI.Areas.API.Models
{
    public class BeatifyInputParamModel
    {
        public int EventId { get; set; }
        public int CateGoryId { get; set; }
        public string CityName { get; set; }
    }
}