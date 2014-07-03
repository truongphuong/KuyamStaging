using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kuyam.WebUI.Helpers
{
    public static class HtmlHelper
    {
        public static MvcHtmlString CustomValidationSummary(this System.Web.Mvc.HtmlHelper helper)
        {
            return CustomValidationSummary(helper, true);
        }

        public static MvcHtmlString CustomValidationSummary(this System.Web.Mvc.HtmlHelper helper, bool excludePropertyErrors)
        {
            return CustomValidationSummary(helper, excludePropertyErrors, String.Empty);
        }

        public static MvcHtmlString CustomValidationSummary(this System.Web.Mvc.HtmlHelper helper, string validationMessage)
        {
            return CustomValidationSummary(helper, validationMessage);
        }

        public static MvcHtmlString CustomValidationSummary(this System.Web.Mvc.HtmlHelper helper, bool excludePropertyErrors, string validationMessage)
        {
            if (helper.ViewData.ModelState.IsValid)
                return MvcHtmlString.Create("");

            string error = "";
            error = "<div class='ui-state-error ui-corner-all' >";    //style='padding: 10px; ' 
            //error += "<span class='ui-icon ui-icon-alert' style='float: left; margin-top:-1px; margin-right: .3em;'></span> The following errors occurred:";
            error += "<div style='position:relative;margin-left: -30px;margin-right:6px;'>";// 

            if (!String.IsNullOrEmpty(validationMessage))
                error += validationMessage;

            error += "<ul style='list-style-type: none;'>";
            foreach (var key in helper.ViewData.ModelState.Keys)
            {
                bool showError = true;
                if (excludePropertyErrors && !String.IsNullOrEmpty(key))
                    showError = false;

                if (showError)
                    foreach (var err in helper.ViewData.ModelState[key].Errors)
                        error += "<li>" + (!String.IsNullOrEmpty(key) ? key + ": " : "") + err.ErrorMessage + "</li>";
            }
            error += "</ul>";

            error += "</div>";
            error += "</div>";
            return MvcHtmlString.Create(error);
        }
    }
}