using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace M2.Util.MVC
{
    public static class ListExt
    {
        public static IEnumerable<SelectListItem> ToSelectListEnum(this StandardTypeList stl)
        {
            List<SelectListItem> ret = new List<SelectListItem>();
            foreach (var s in stl)
                ret.Add(new SelectListItem() { Text = s.Key, Value = s.Value.ToString() });
            return ret;
        }

        public static SelectList ToSelectList(this StandardTypeList stl, object selectedValue = null)
        {
            List<SelectListItem> ret = new List<SelectListItem>();
            foreach (var s in stl)
                ret.Add(new SelectListItem() { Text = s.Key, Value = s.Value.ToString() });

            if (selectedValue == null)
                return new SelectList(ret, "Value", "Text");
            else
                return new SelectList(ret, "Value", "Text", selectedValue);
        }
    }
}
