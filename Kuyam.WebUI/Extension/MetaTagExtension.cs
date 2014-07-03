using Kuyam.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Kuyam.WebUI.Extension
{
    public class MetaTagExtension
    {
        private readonly string _description;
        private readonly string _keywords;      
        public MetaTagExtension()
        {

        }
        public MetaTagExtension(string description)
        {            
            var Keywords = MyApp.Settings.TagSetting.Keywords;
            this._description = description;
            this._keywords = Keywords;          
        }
        public MvcHtmlString MetaTag()
        {
            StringBuilder stbdBuilder = new StringBuilder();
            stbdBuilder.AppendFormat("\n  <meta name=\"description\" content=\"{0}\" />", _description);
            stbdBuilder.AppendFormat("\n  <meta name=\"keywords\" content=\"{0}\" />", _keywords);          
            stbdBuilder.AppendFormat("\n  <meta name=\"robots\" content=\"index, follow\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"author\" content=\"kuyam.com\" />");
            stbdBuilder.AppendFormat("\n  <meta name=\"copyright\" content=\"kuyam.Com\" />");
            return MvcHtmlString.Create(stbdBuilder.ToString());
        }
    }

}
