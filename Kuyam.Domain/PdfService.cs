using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Web;
using Kuyam.Database.Extensions;
using Kuyam.Utility;
using Kuyam.Database;

namespace Kuyam.Domain
{
    public class PdfService
    {

        #region Utilities

        protected virtual Font GetFont()
        {
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            string fontPath = Path.Combine(HostingEnvironment.MapPath("~/App_Data/Pdf/"), "FreeSerif.ttf");
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        public static string StripTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = Regex.Replace(text, @"(>)(\r|\n)*(<)", "><");
            text = Regex.Replace(text, "(<[^>]*>)([^<]*)", "$2");
            text = Regex.Replace(text, "(&#x?[0-9]{2,4};|&quot;|&amp;|&nbsp;|&lt;|&gt;|&euro;|&copy;|&reg;|&permil;|&Dagger;|&dagger;|&lsaquo;|&rsaquo;|&bdquo;|&rdquo;|&ldquo;|&sbquo;|&rsquo;|&lsquo;|&mdash;|&ndash;|&rlm;|&lrm;|&zwj;|&zwnj;|&thinsp;|&emsp;|&ensp;|&tilde;|&circ;|&Yuml;|&scaron;|&Scaron;)", "@");

            return text;
        }

        public static string ConvertHtmlToPlainText(string text, bool decode = false)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            if (decode)
                text = HttpUtility.HtmlDecode(text);

            text = text.Replace("<br>", "\n");
            text = text.Replace("<br >", "\n");
            text = text.Replace("<br />", "\n");
            text = text.Replace("&nbsp;&nbsp;", "\t");
            text = text.Replace("&nbsp;&nbsp;", "  ");

            return text;
        }

        #endregion

        public virtual void PrintInvoicesToPdf(IList<CompanyInvoices> invoices, string filePath)
        {
            if (invoices == null)
                throw new ArgumentNullException("invoices");

            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            var pageSize = PageSize.A4;

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var cell = new PdfPCell();
            #region Invoices

            var invoicesTable = new PdfPTable(6);
            invoicesTable.WidthPercentage = 100f;
            invoicesTable.SetWidths(new[] { 20, 20, 20, 20, 10, 10 });

            //date
            cell = new PdfPCell(new Phrase("date", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //description
            cell = new PdfPCell(new Phrase("service description", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //employee
            cell = new PdfPCell(new Phrase("employee", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //client
            cell = new PdfPCell(new Phrase("client", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //regular
            cell = new PdfPCell(new Phrase("regular?", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //amount
            cell = new PdfPCell(new Phrase("amount ($)", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);
            int invoiceNumber = 0;
            for (int i = 0; i < invoices.Count; i++)
            {
                var item = invoices[i];
                invoiceNumber++;
                //startDate name
                string name = string.Format("{0:MM/dd/yy h tt}", DateTimeUltility.ConvertToUserTime(item.ServiceStartDate, DateTimeKind.Utc));
                cell = new PdfPCell();
                cell.AddElement(new Paragraph(name, font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //description
                string description = !string.IsNullOrEmpty(item.ServiceDescription) ? item.ServiceDescription : string.Empty;
                cell = new PdfPCell(new Phrase(description, font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //EmployeeName
                string employeeName = !string.IsNullOrEmpty(item.EmployeeName) ? item.EmployeeName : string.Empty;
                cell = new PdfPCell(new Phrase(employeeName, font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //client
                string clientName = !string.IsNullOrEmpty(item.ClientName) ? item.ClientName : string.Empty;
                cell = new PdfPCell(new Phrase(clientName, font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //regular
                string isRegular = string.Empty;
                if (item.IsRegular)
                {
                    isRegular = "Is regular";
                }
                else
                {
                    isRegular = "Not regular";
                }

                cell = new PdfPCell(new Phrase(isRegular, font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //amount
                decimal orderSubtotal = item.OrderSubtotal.HasValue ? item.OrderSubtotal.Value : 0;
                cell = new PdfPCell(new Phrase(orderSubtotal.ToString("0.00"), font));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                if (invoiceNumber % 20 == 0)
                {
                    doc.Add(invoicesTable);
                    invoicesTable = new PdfPTable(5);
                    invoicesTable.WidthPercentage = 100f;
                    invoicesTable.SetWidths(new[] { 20, 20, 20, 20, 20 });
                    doc.NewPage();
                }
            }

            if (invoiceNumber < 20)
            {
                doc.Add(invoicesTable);
            }

            #endregion
            doc.Close();
        }


        public virtual void PrintUserInvoicesToPdf(IList<CompanyInvoices> invoices, string filePath)
        {
            if (invoices == null)
                throw new ArgumentNullException("invoices");

            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            var pageSize = PageSize.A4;

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var cell = new PdfPCell();
            #region Invoices

            var invoicesTable = new PdfPTable(5);
            invoicesTable.WidthPercentage = 100f;
            invoicesTable.SetWidths(new[] { 35, 20, 20, 10, 15 });

            //date
            cell = new PdfPCell(new Phrase("date", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //description
            cell = new PdfPCell(new Phrase("service description", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //employee
            cell = new PdfPCell(new Phrase("employee", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //client
            cell = new PdfPCell(new Phrase("calendar", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);

            //amount
            cell = new PdfPCell(new Phrase("amount ($)", font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            invoicesTable.AddCell(cell);
            int invoiceNumber = 0;
            int invoiceCount = invoices.Count;

            for (int i = 0; i < invoices.Count; i++)
            {                
                invoiceNumber++;
                var item = invoices[i];                                 
                string paymentMethod = "";
                 if (item.PaymentMethod == (int)Kuyam.Database.Types.PaymentMethod.Paypal)
                 {
                   paymentMethod = "Paypal";
                 }
                 else
                 {
                     paymentMethod = "pay in person";
                 }
                //startDate name
                 string name = string.Format("{0:MM/dd/yy h tt}", DateTimeUltility.ConvertToUserTime(item.ServiceStartDate, DateTimeKind.Utc));
                name += "\n         receipt #: " + item.ReceiptNumber +
                    "\n  purchased on: " + string.Format("{0}", DateTimeUltility.ConvertToUserTime(item.PurchasedOn.Value, DateTimeKind.Utc)) +
                    "\n              status: " + ((Kuyam.Database.Types.AppointmentStatus)item.AppointmentStatus).ToString().ToLower() +
                    "\n         payment: " + paymentMethod +
                    "\n        company: " + item.CompanyName;
                cell = new PdfPCell();
                cell.Border = PdfPCell.BOTTOM_BORDER;
                cell.PaddingLeft = 2;
                cell.PaddingBottom = 20;
                cell.AddElement(new Paragraph(name, font));                                 
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //description
                string description = !string.IsNullOrEmpty(item.ServiceDescription) ? item.ServiceDescription : string.Empty;
                cell = new PdfPCell(new Phrase(description, font));
                cell.Border = PdfPCell.BOTTOM_BORDER;
                cell.PaddingBottom = 20;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //EmployeeName
                string employeeName = !string.IsNullOrEmpty(item.EmployeeName) ? item.EmployeeName : string.Empty;
                cell = new PdfPCell(new Phrase(employeeName + "\n \n company services" + "\n\n Total amount paid", font));
                cell.Border = PdfPCell.BOTTOM_BORDER;
                cell.PaddingBottom = 20;
                //cell.PaddingLeft = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                ////client
                string clientName = !string.IsNullOrEmpty(item.ClientName) ? item.ClientName : string.Empty;
                cell = new PdfPCell(new Phrase(clientName, font));
                cell.Border = PdfPCell.BOTTOM_BORDER;
                cell.PaddingBottom = 20;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoicesTable.AddCell(cell);

                //amount
                decimal orderSubtotal = item.OrderSubtotal.HasValue ? item.OrderSubtotal.Value : 0;
                cell = new PdfPCell(new Phrase(orderSubtotal.ToString("0.00") + "\n\n" + item.OrderSubtotal.Value.ToString("0.00") + "\n\n" + item.OrderTotal.Value.ToString("0.00"), font));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = PdfPCell.BOTTOM_BORDER;
                cell.PaddingBottom = 20;
                cell.PaddingRight = 0;
                invoicesTable.AddCell(cell);

                if (invoiceNumber % 20 == 0)
                {
                    doc.Add(invoicesTable);
                    invoicesTable = new PdfPTable(5);
                    invoicesTable.WidthPercentage = 100f;
                    invoicesTable.SetWidths(new[] { 20, 20, 20, 20, 20 });
                    doc.NewPage();
                }                
            }

            if (invoiceNumber < 20)
            {
                doc.Add(invoicesTable);
            }

            #endregion
            doc.Close();
        }
    }
}
