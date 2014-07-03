using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;


// Add to pages/namespaces section: <add namespace="M2.Util.MVC" />

namespace M2.Util.MVC
/*{
    public static class LinkHelper
    {
        public static MvcHtmlString ActionLinkImage(this HtmlHelper html, string imagePath, string url, object htmlAttributes)
        {
            return MvcHtmlString.Create(String.Format("<a href=\"{1}\" style=\"text-decoration:none\"><img src=\"{0}\"></a>", imagePath, url));
        }

        public static string ActionLinkImage(this HtmlHelper html, string imagePath, string url, int width, int height)
        {
            return String.Format("<a href=\"{1}\" style=\"text-decoration:none\"><img src=\"{0}\" width={2} height={3}></a>", imagePath, url, width, height);
        }
    }
}
*/

//namespace Helpers
{
    public static class ImageLinkExtensions
    {
        //protected string RenderPartialViewToString(string viewName, object model)
        //{
        //    if (string.IsNullOrEmpty(viewName))
        //        viewName = ControllerContext.RouteData.GetRequiredString("action");

        //    ViewData.Model = model;

        //    using (StringWriter sw = new StringWriter())
        //    {
        //        ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
        //        ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
        //        viewResult.View.Render(viewContext, sw);

        //        return sw.GetStringBuilder().ToString();
        //    }
        //}

        public static MvcHtmlString ImageLink(this System.Web.Mvc.HtmlHelper helper, string actionName, string controllerName, string imgUrl, string alt, int width = -1, int height = -1, object routeValues = null, object linkHtmlAttributes = null, object imageHtmlAttributes = null)
        {
            return GetImageLink(helper, actionName, controllerName, null, imgUrl, null, alt, width, height, routeValues, linkHtmlAttributes, imageHtmlAttributes);
        }

        public static MvcHtmlString ImageRolloverLink(this System.Web.Mvc.HtmlHelper helper, string actionName, string controllerName, string imgUrl, string imgUrlRollover, string alt, int width = -1, int height = -1, object routeValues = null, object linkHtmlAttributes = null, object imageHtmlAttributes = null)
        {
            return GetImageLink(helper, actionName, controllerName, null, imgUrl, imgUrlRollover, alt, width, height, routeValues, linkHtmlAttributes, imageHtmlAttributes);
        }

        public static MvcHtmlString ImageAndTextLink(this System.Web.Mvc.HtmlHelper helper, string actionName, string controllerName, string text, string imgUrl, string alt, int width = -1, int height = -1, object routeValues = null, object linkHtmlAttributes = null, object imageHtmlAttributes = null)
        {
            return GetImageLink(helper, actionName, controllerName, text, imgUrl, null, alt, width, height, routeValues, linkHtmlAttributes, imageHtmlAttributes);
        }

        /// <summary>
        /// A Simple ActionLink Image
        /// </summary>
        /// <param name="actioNName">name of the action in controller</param>
        /// <param name="imgUrl">url of the image</param>
        /// <param name="alt">alt text of the image</param>
        /// <param name="linkHtmlAttributes">attributes for the link</param>
        /// <param name="imageHtmlAttributes">attributes for the image</param>
        /// <returns></returns>
        private static MvcHtmlString GetImageLink(this System.Web.Mvc.HtmlHelper helper, string actionName, string controllerName, string text, string imgUrl, string imgUrlRollover, string alt, int width = -1, int height = -1, object routeValues = null, object linkHtmlAttributes = null, object imageHtmlAttributes = null)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            var url = urlHelper.Action(actionName, controllerName, routeValues);

            //Create the link
            var linkTagBuilder = new TagBuilder("a");
            linkTagBuilder.MergeAttribute("href", url);
            linkTagBuilder.MergeAttribute("style", "text-decoration:none;vertical-align:middle;display:inline;");
			linkTagBuilder.MergeAttributes(new RouteValueDictionary(linkHtmlAttributes));

            //Create image
            var imageTagBuilder = new TagBuilder("img");
            imageTagBuilder.MergeAttribute("src", urlHelper.Content(imgUrl));
            imageTagBuilder.MergeAttribute("alt", urlHelper.Content(alt));
			imageTagBuilder.MergeAttribute("style", "vertical-align:middle;display:inline;");
			if (width > 0)
                imageTagBuilder.MergeAttribute("width", width.ToString());
            if (height > 0)
                imageTagBuilder.MergeAttribute("height", height.ToString());
            imageTagBuilder.MergeAttributes(new RouteValueDictionary(imageHtmlAttributes));

            // TODO: Rollover image
            if (imgUrlRollover != null && imgUrlRollover != "")
            {
                

            }

            //Add image to link
            linkTagBuilder.InnerHtml = imageTagBuilder.ToString(TagRenderMode.SelfClosing) + " " + text;

            return MvcHtmlString.Create(linkTagBuilder.ToString());
        }

        //<a href="" onclick="openDialog('Link Networks', '', 'linkNetworksDiv'); return false;" style="text-decoration:none"><img alt="Link" height="14" src="@Url.Content("~/images/add.jpg")" width="14" />Link</a>
        //@Html.Partial(\"_PitchLinkPartial\")
        //@ Need a div:
        //@ <div id="_PitchLinkPartialDiv" class="dialog">
        //@  @Html.Partial("_PitchLinkPartial")
        //@ </div>
        public static MvcHtmlString ImageAndTextLinkPartialDivPopup(this System.Web.Mvc.HtmlHelper helper, string title, string partialView, string imgUrl, int width = -1, int height = -1, object linkHtmlAttributes = null, object imageHtmlAttributes = null)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            //Create the link
            var linkTagBuilder = new TagBuilder("a");
            linkTagBuilder.MergeAttribute("href", "");
            linkTagBuilder.MergeAttribute("style", "text-decoration:none");
            linkTagBuilder.MergeAttribute("onclick", "openDialog('" + title + "', '', '" + partialView + "Div');return false;");
            linkTagBuilder.MergeAttributes(new RouteValueDictionary(linkHtmlAttributes));

            //Create image
            var imageTagBuilder = new TagBuilder("img");
            imageTagBuilder.MergeAttribute("src", urlHelper.Content(imgUrl));
            imageTagBuilder.MergeAttribute("alt", urlHelper.Content(title));
            if (width > 0)
                imageTagBuilder.MergeAttribute("width", width.ToString());
            if (height > 0)
                imageTagBuilder.MergeAttribute("height", height.ToString());
            imageTagBuilder.MergeAttributes(new RouteValueDictionary(imageHtmlAttributes));

            //// TODO: Rollover image
            //if (imgUrlRollover != null && imgUrlRollover != "")
            //{
                

            //}

            //helper.RenderPartial(partialView);

            //Add image to link
            linkTagBuilder.InnerHtml = imageTagBuilder.ToString(TagRenderMode.SelfClosing) + " " + title;

            return MvcHtmlString.Create(linkTagBuilder.ToString());
        }

        public static MvcHtmlString GoBackLink(this System.Web.Mvc.HtmlHelper helper, string text="Back", int stepCount=1, bool refreshPage=false)
        {
			string reload = "";
			if (refreshPage)
				reload = "window.location.reload();";
			return MvcHtmlString.Create("<a href=\"javascript:history.go(-" + stepCount + ");" + reload + "\">" + text + "</a>");
			//return MvcHtmlString.Create("<a href=\""+ HttpContext.Current.Request.UrlReferrer.OriginalString + "\">" + text + "</a>");
		}

		public static MvcHtmlString SaveCancel(this System.Web.Mvc.HtmlHelper helper, string saveText = "Save", string savingText = "Saving", string cancelText = "Cancel", bool addSpace = true, string idname = "submit", string script="")
        {
            //string html = String.Format("<p><br /><input type=\"button\" id=\"submitButton2\" style=\"display:none;\" value=\"{2}...\"><input id=\"submitButton\" type=\"submit\" value=\"{0}\" onclick=\"style.display='none';$('#submitButton2').show();\" />&nbsp;{1}</p><script>jQuery('#submitButton').show(); jQuery('#submitButton2').hide();\"</script>", saveText, GoBackLink(helper, cancelText), savingText);
			string html = String.Format("<input type=\"submit\" id=\"{3}\" name=\"{3}\" value=\"{0}\" onclick=\"value='{1}...';{4}\"> or {2}", saveText, savingText, GoBackLink(helper, cancelText), idname, script);
			if (addSpace)
				html = String.Format("<p><br />{0}</p>", html);

            return MvcHtmlString.Create(html);
        }

		public static MvcHtmlString SaveCancelIf(this System.Web.Mvc.HtmlHelper helper, bool allowSave, string saveText = "save", string savingText = "saving", string cancelText = "cancel", bool addSpace = true, string idname = "submit", string script = "")
		{
			if (allowSave)
				return SaveCancel(helper, saveText, savingText, cancelText, addSpace, idname, script);
			else
				return GoBackLink(helper);
		}

		public static MvcHtmlString SaveCancelLC(this System.Web.Mvc.HtmlHelper helper, string saveText = "save", string savingText = "saving", string cancelText = "cancel", bool addSpace = true, string idname = "submit", string script = "")
		{
			return SaveCancel(helper, saveText.ToLower(), savingText.ToLower(), cancelText.ToLower(), addSpace, idname, script);
		}

		public static MvcHtmlString ApplyCancel(this System.Web.Mvc.HtmlHelper helper, string saveText = "Apply", string savingText = "Applying", string cancelText = "Cancel", bool addSpace = true, string idname = "submit", string script = "")
		{
			//string html = String.Format("<p><br /><input type=\"button\" id=\"submitButton2\" style=\"display:none;\" value=\"{2}...\"><input id=\"submitButton\" type=\"submit\" value=\"{0}\" onclick=\"style.display='none';$('#submitButton2').show();\" />&nbsp;{1}</p><script>jQuery('#submitButton').show(); jQuery('#submitButton2').hide();\"</script>", saveText, GoBackLink(helper, cancelText), savingText);
			string html = String.Format("<input type=\"submit\" id=\"{3}\" name=\"{3}\" value=\"{0}\" onclick=\"value='{1}...';{4}\"> or {2}", saveText, savingText, GoBackLink(helper, cancelText, 0, true), idname, script);
			if (addSpace)
				html = String.Format("<p><br />{0}</p>", html);

			return MvcHtmlString.Create(html);
		}

		public static MvcHtmlString SaveCustom(this System.Web.Mvc.HtmlHelper helper, string saveText = "Save", string savingText = "Saving", string customText = "Cancel", string customLinkID = "customLink", string customUrl = "#", bool addSpace = true, string idname = "submit", string script = "")
		{
			//string html = String.Format("<p><br /><input type=\"button\" id=\"submitButton2\" style=\"display:none;\" value=\"{2}...\"><input id=\"submitButton\" type=\"submit\" value=\"{0}\" onclick=\"style.display='none';$('#submitButton2').show();\" />&nbsp;{1}</p><script>jQuery('#submitButton').show(); jQuery('#submitButton2').hide();\"</script>", saveText, GoBackLink(helper, cancelText), savingText);
			string html = String.Format("<input type=\"submit\" id=\"{4}\" name=\"{4}\" value=\"{0}\" onclick=\"value='{1}...';{5}\"> or <a href=\"{6}\" id=\"{3}\">{2}</a>", saveText, savingText, customText, customLinkID, idname, script, customUrl);
			if (addSpace)
				html = String.Format("<p><br />{0}</p>", html);

			return MvcHtmlString.Create(html);
		}

		/*        private static string RenderViewToString(this HtmlHelper helper, string viewName, object viewData)
				{
					ControllerContext context = helper.ViewContext.Controller.ControllerContext;

					//Create memory writer 
					var sb = new StringBuilder();
					var memWriter = new StringWriter(sb);

					//Create fake http context to render the view 
					var fakeResponse = new HttpResponse(memWriter);
					var fakeContext = new HttpContext(HttpContext.Current.Request, fakeResponse);
					var fakeControllerContext = new ControllerContext(
						new HttpContextWrapper(fakeContext),
						context.RouteData, context.Controller);

					var oldContext = HttpContext.Current;
					HttpContext.Current = fakeContext;

					//Use HtmlHelper to render partial view to fake context 
					var html = new HtmlHelper(new ViewContext(fakeControllerContext,
						new FakeView(), new ViewDataDictionary(), new TempDataDictionary()),
						new ViewPage());
					html.RenderPartial(viewName, viewData);

					//Restore context 
					HttpContext.Current = oldContext;

					//Flush memory and return output 
					memWriter.Flush();
					return sb.ToString();
				}

				protected string RenderPartialViewToString(string viewName, object model)
				{
					if (string.IsNullOrEmpty(viewName))
						viewName = ControllerContext.RouteData.GetRequiredString("action");

					ViewData.Model = model;

					using (StringWriter sw = new StringWriter())
					{
						ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
						ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
						viewResult.View.Render(viewContext, sw);

						return sw.GetStringBuilder().ToString();
					}
				}
		*/
    }
}

