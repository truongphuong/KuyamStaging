using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using M2.Util;
using System.Web.Mvc;


namespace M2.Util.MVC
{
    public static class Converter
    {
        public static SelectListItem ToSelectListItem(this SelectItem t)
        {
            SelectListItem si = new SelectListItem();
            si.Text = t.Text;
            si.Value = t.Value;
            si.Selected = t.Selected;
            return si;
        }

        public static SelectItem ToSelectItem(this SelectListItem si)
        {
            SelectItem t = new SelectItem();
            t.Text = si.Text;
            t.Value = si.Value;
            t.Selected = si.Selected;
            return t;
        }

        public static List<SelectItem> ToSelectItemList(this List<SelectListItem> ls)
        {
            List<SelectItem> list = new List<SelectItem>();
            foreach (SelectListItem sli in ls)
                list.Add(sli.ToSelectItem());
            return list;
        }

        public static List<SelectListItem> ToSelectListItemList(this List<SelectItem> ls)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (SelectItem si in ls)
                list.Add(si.ToSelectListItem());
            return list;
        }

        public static IEnumerable<SelectListItem> ToSelectListEnum(this Dictionary<int, string> dictlist, List<int> selectedValues = null)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            bool selected = false;
            foreach (var s in dictlist)
            {
                if (selectedValues != null)
                    selected = selectedValues.Contains(s.Key);

                selectList.Add(new SelectListItem() { Text = s.Value, Value = s.Key.ToString(), Selected = selected });
            }
            return selectList.AsEnumerable();
        }
    }
}
