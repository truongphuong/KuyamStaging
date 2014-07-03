using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace M2.Util.MVC
{
	public static class InputHelper
	{
		//public static HtmlString EditorForWithClass(HtmlHelper html, object model)
		//{
		//  return (new HtmlString(html.EditorFor(model => model.NextCall).ToString().Replace("class=\"", "class=\"date-pick ")))
		//}

		public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, List<SelectListItem> list, string ModelCollectionName, bool vertical = true)
		{
			throw new NotImplementedException();
			//return CheckBoxList(helper, helper.???)
		}

		//public static MvcHtmlString CheckBoxList(this System.Web.Mvc.HtmlHelper htmlHelper, List<SelectListItem> list, string modelCollectionName, bool vertical = true)
		//{

		//    SelectList l2 = new SelectList(list.OrderBy(x => x.Text), "Value", "Text");
		//    return CheckBoxList(htmlHelper, modelCollectionName, l2, vertical);
		//}

		//public static MvcHtmlString CheckBoxList(this System.Web.Mvc.HtmlHelper htmlHelper, string modelCollectionName, SelectList list, bool vertical = true)
		//{
		//    SelectList l2 = new SelectList(list);
		//    var sb = new StringBuilder();

		//    if (list != null)
		//    {
		//        //List<int> selVals = new List<int>();
		//        //foreach (var val in list.SelectedValues)
		//        //    selVals.Add(val.ToInt32());

		//        int i = 0;

		//        foreach (var l in list)
		//        {
		//            string collectionNameIndex = String.Format("{0}[{1}]", modelCollectionName, i);

		//            var hiddenName = new TagBuilder("input");
		//            hiddenName.Attributes.Add(new KeyValuePair<string, string>("type", "hidden"));
		//            hiddenName.Attributes.Add(new KeyValuePair<string, string>("name", String.Format("{0}.{1}", collectionNameIndex, "Txt")));
		//            hiddenName.Attributes.Add(new KeyValuePair<string, string>("value", l.Text));

		//            var hiddenValue = new TagBuilder("input");
		//            hiddenValue.Attributes.Add(new KeyValuePair<string, string>("type", "hidden"));
		//            hiddenValue.Attributes.Add(new KeyValuePair<string, string>("name", String.Format("{0}.{1}", collectionNameIndex, "Val")));
		//            hiddenValue.Attributes.Add(new KeyValuePair<string, string>("value", l.Value));

		//            var checkBoxTag = htmlHelper.CheckBox(String.Format("{0}.{1}", collectionNameIndex, "Sel"), l.Selected);

		//            var labelTag = new TagBuilder("label");
		//            labelTag.Attributes.Add(new KeyValuePair<string, string>("for", String.Format("{0}.{1}", collectionNameIndex, "Nam")));
		//            labelTag.SetInnerText(l.Text);

		//            sb.Append(hiddenName);
		//            sb.Append(hiddenValue);
		//            sb.Append(checkBoxTag);
		//            sb.Append(labelTag);
		//            if (vertical)
		//                sb.Append("<br/>\r\n");
		//            else
		//                sb.Append("&nbsp;&nbsp;\r\n");

		//            i++;
		//        }
		//    }

		//    return MvcHtmlString.Create(sb.ToString());
		//}

		public static MvcHtmlString CheckBoxList(this System.Web.Mvc.HtmlHelper htmlHelper, string controlID, SelectList list, List<int> selectedIDs, string classes = "", bool vertical = false)
		{
			var sb = new StringBuilder();

			if (selectedIDs == null)
			{
				selectedIDs = new List<int>();
				foreach (var item in list)
					if (item.Selected)
						selectedIDs.Add(item.Value.ToInt32());
			}

			if (list != null)
			{
				int i = 0;

				sb.AppendFormat("<div id=\"{0}\" class=\"{1}\">\r\n", controlID, classes);
				foreach (var l in list)
				{
					string collectionNameIndex = String.Format("{0}-{1}", controlID, l.Value);  // format used in GetCheckboxListSelections

					sb.AppendFormat("<input type=\"checkbox\" id=\"{0}\" name=\"{0}\" {1} />{2}", collectionNameIndex, selectedIDs.Contains(l.Value.ToInt32()) ? "checked=\"yes\"" : "", l.Text);
					if (vertical)
						sb.Append("<br/>\r\n");
					else
						sb.Append("\r\n");
					i++;
				}
				sb.Append("</div>");
			}

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString JQueryCheckBoxList(this System.Web.Mvc.HtmlHelper htmlHelper, string controlID, SelectList list, List<int> selectedIDs, string classes = "", bool vertical = false)
		{
			var sb = new StringBuilder();

			if (selectedIDs == null)
			{
				selectedIDs = new List<int>();
				foreach (var item in list)
					if (item.Selected)
						selectedIDs.Add(item.Value.ToInt32());
			}

			if (list != null)
			{
				int i = 0;

				sb.AppendFormat("<div id=\"{0}\" class=\"{1}\">\r\n", controlID, classes);
				foreach (var l in list)
				{
					string collectionNameIndex = String.Format("{0}-{1}", controlID, l.Value);  // format used in GetCheckboxListSelections

					sb.AppendFormat("<input type=\"checkbox\" id=\"{0}\" name=\"{0}\" {1} /><label for=\"{0}\">{2}</label>", collectionNameIndex, selectedIDs.Contains(l.Value.ToInt32()) ? "checked=\"yes\"" : "", l.Text);
					if (vertical)
						sb.Append("<br/>\r\n");
					else
						sb.Append("\r\n");
					i++;
				}
				sb.Append("</div>");
			}

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString JQueryCheckBox(this System.Web.Mvc.HtmlHelper htmlHelper, string controlID, string text, int value, bool isChecked, string classes = "")
		{
			string collectionNameIndex = String.Format("{0}-{1}", controlID, value);
			string ret = String.Format("<input type=\"checkbox\" id=\"{0}\" name=\"{0}\" {1} class=\"{3}\" /><label for=\"{0}\">{2}</label>\r\n", collectionNameIndex, isChecked ? "checked=\"yes\"" : "", text, classes);

			return MvcHtmlString.Create(ret);
		}

		public static List<int> GetCheckboxListSelections(HttpRequestBase req, string controlID)
		{
			List<int> ret = new List<int>();
			int start = controlID.Length+1;  // based on format "controlid-####"
			foreach (string r in req.Form.Keys)
			{
				if (r.StartsWith(controlID))
				{
					ret.Add(r.Substring(start).ToInt32());
				}
			}

			return ret;
		}

		public static MvcHtmlString RadioButtonForSelectList<TModel, TProperty>(
				this HtmlHelper<TModel> htmlHelper,
				Expression<Func<TModel, TProperty>> expression,
				SelectList listOfValues,
				int checkedIndex = -1,
				int lineBreakInterval = -1)
		{
			var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var sb = new StringBuilder();

			if (listOfValues != null)
			{
				// Create a radio button for each item in the list
				int total = listOfValues.Count();
				int count = 0;
				foreach (SelectListItem item in listOfValues)
				{
					// Generate an id to be given to the radio button field
					var id = string.Format("{0}_{1}", metaData.PropertyName, item.Value);

					// Create and populate a radio button using the existing html helpers
					var label = htmlHelper.Label(id, HttpUtility.HtmlEncode(item.Text));

					object o = null;
					if (checkedIndex == count)
						o = new { id = id, @checked = "checked" };
					else
						o = new { id = id };

					var radio = htmlHelper.RadioButtonFor(expression, item.Value, o).ToHtmlString();

					// Create the html string that will be returned to the client
					sb.AppendFormat("{0}{1}", radio, label);

					if (count < total - 1)
					{
						if (lineBreakInterval > 0 && (count + 1) % lineBreakInterval == 0)
						{
							sb.AppendFormat("<br />");
						}
						else
						{
							sb.AppendFormat("&nbsp;&nbsp;");
						}
					}

					count++;
				}
			}

			return MvcHtmlString.Create(sb.ToString());
		}
	}
}
