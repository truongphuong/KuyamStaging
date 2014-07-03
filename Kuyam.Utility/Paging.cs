using System;
using System.Web;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Lớp phân trang.
/// </summary>
public class Paging
{
    private int totalCount = 0;         // tổng số record trả về từ CSDL.
    private int maxPage = 0;            // số trang lớn nhất.
    private int lowerBoundary;          // vùng số thấp.
    private int upperBoundary;          // vùng số cao.        

    public Paging(int totalCount)
    {
        this.queryString = "";
        this.param = "page";

        this.perPage = 10;
        this.currentPage = 1;

        this.linksBefore = 2;
        this.linksAfter = 2;

        this.firstText = "1";
        this.lastText = "Cuối";
        this.nextText = "Tiếp »";
        this.prevText = "« Trước";

        this.showFirst = true;
        this.showPrev = true;
        this.showNext = true;
        this.showLast = true;
        this.showMore = true;
        this.showPageNumber = true;

        this.currentMode = Mode.None; // tùy chọn: [none | combobox | editbox].
        this.inputInValid = "Số trang không tồn tại.";

        this.totalCount = totalCount;
    }

    /// <summary>
    /// Hàm tính số trang lớn nhất, tính tròn tăng từ totalCount.
    /// </summary>
    private void MaxPage()
    {
        if (this.totalCount != 0)
        {
            this.maxPage = Convert.ToInt32(Math.Floor((double)(this.totalCount - 1) / this.PerPage)) + 1;

        }
    }

    /// <summary>
    /// Hàm lấy địa chỉ url của trang đặt paging.
    /// </summary>
    /// <param name="pageNumber">số trang sẽ xem.</param>
    /// <returns>string</returns>
    private string TargetUrl(double pageNumber)
    {
        string url = string.Empty;
        url = string.Format(this.QueryString, "=" + pageNumber.ToString());
        return url;
    }

    /// <summary>
    /// Hàm xây dựng phân trang
    /// </summary>
    /// <returns>trả về chuỗi html với cấu trúc phân trang.</returns>
    public string BuildPaging()
    {
        StringBuilder outputPage = new StringBuilder();

        this.MaxPage();

        if (this.totalCount > this.PerPage)
        {
            /**
             * thêm hàm javascript xử lý khi CurrentMode là Combobox hay Editbox.
             */
            if (this.showPageNumber)
            {
                if (this.CurrentMode == Mode.Editbox)
                {
                    outputPage.Append("<script type=\"text/javascript\">");
                    outputPage.Append("$(document).ready(function() { ");
                    outputPage.Append("\n\tpagingeditbox(\"" + this.QueryString + "\", " + this.maxPage + ", \"" + this.InputInValid + "\");\n");
                    outputPage.Append("});</script>");
                    outputPage.AppendLine();
                }
                if (this.CurrentMode == Mode.Combobox)
                {
                    outputPage.Append("<script type=\"text/javascript\">");
                    outputPage.Append("$(document).ready(function() { ");
                    outputPage.Append("\n\tpagingcombobox(\"" + this.QueryString + "\");\n");
                    outputPage.Append("});</script>");
                    outputPage.AppendLine();
                }
            }

            /**
             * xét giá trị lại biến "currentPage" trang hiện tại đang xem.
             */
            if (HttpContext.Current.Request.QueryString[this.Param] != null)
            {
                this.currentPage = int.Parse(HttpContext.Current.Request.QueryString[this.Param]);
            }

            /**
		     * tính vùng số (thấp) thoát ra khỏi liên kết trước trang hiện tại.
		     * vd: 1 2 [3].
		     */
            if ((this.CurrentPage - this.LinksBefore) < 1)
            {
                this.lowerBoundary = 1;
            }
            else
            {
                this.lowerBoundary = this.CurrentPage - this.LinksBefore;
            }

            /**
		     * tính vùng số (cao) thoát ra khỏi liên kết sau trang hiện tại.
		     * vd: [3] 4 5.
		     */
            if ((this.CurrentPage + this.LinksAfter) > this.maxPage)
            {
                this.upperBoundary = this.maxPage;
            }
            else
            {
                this.upperBoundary = this.CurrentPage + this.LinksAfter;
            }

            /**
		     * tạo link từ miền giá trị lấy được trong khoảng giới hạn.
		     * khởi tạo mảng và truyền giá trị tương ứng vào mảng đó.
		     * vd: ... 1 2 [3] 4 5 ...
		     */
            ArrayList pageLinks = new ArrayList();

            if (this.ShowFirst && this.ShowMore && (this.CurrentPage - this.LinksBefore) > 1)
            {
                pageLinks.Add("<span class=\"borderNone p0 ml4 dotmore\">...</span>");
            }

            for (int i = this.lowerBoundary; i <= this.upperBoundary; i++)
            {
                if (this.CurrentPage == i)
                {
                    switch (this.CurrentMode)
                    {
                        case Mode.None:
                            pageLinks.Add("<span class=\"current-none\">" + i + "</span>");
                            break;
                        case Mode.Combobox:
                            StringBuilder combobox = new StringBuilder();
                            combobox.Append("<select id=\"paging-mode-combobox\" class=\"current-combobox\">");
                            for (double j = 1; j <= this.maxPage; j++)
                            {
                                combobox.Append("<option value=\"" + TargetUrl(j) + "\"");
                                if (this.CurrentPage == j)
                                {
                                    combobox.Append(" selected=\"selected\"");
                                }
                                combobox.Append(">" + j + "</option>");
                                combobox.AppendLine();
                            }
                            combobox.Append("</select>");
                            combobox.AppendLine();
                            pageLinks.Add(combobox.ToString());
                            break;
                        case Mode.Editbox:
                            string editbox = "<input type=\"text\" id=\"paging-mode-editbox\" class=\"current-editbox\" value=\"" + i + "\">";
                            pageLinks.Add(editbox);
                            break;
                        default:
                            pageLinks.Add("<span class=\"current-none\">" + i + "</span>");
                            break;
                    }
                }
                else
                {
                    pageLinks.Add("<a href=\"" + this.TargetUrl(i) + "\">" + i + "</a>");
                }
            }

            if (this.ShowMore && (this.CurrentPage + this.LinksAfter) < this.maxPage)
            {
                pageLinks.Add("<span class=\"borderNone p0 ml4 dotmore\">...</span>");
            }

            /**
	         * tạo link: first | prev | next | last.
	         */
            string firstLink = null;
            string prevLink = null;
            string nextLink = null;
            string lastLink = null;

            if (this.CurrentPage != 1)
            {
                prevLink = "<a href=\"" + this.TargetUrl(this.CurrentPage - 1) + "\" class=\"fpnl-page\">" + this.prevText + "</a>";
            }

            if (this.CurrentPage != this.maxPage)
            {
                nextLink = "<a href=\"" + this.TargetUrl(this.CurrentPage + 1) + "\" class=\"fpnl-page\">" + this.nextText + "</a>";
            }

            if (this.lowerBoundary != 1)
            {
                firstLink = "<a href=\"" + this.TargetUrl(1) + "\" class=\"fpnl-page\">" + this.FirstText + "</a>";
            }
            if (this.upperBoundary != this.maxPage)
            {
                lastLink = "<a href=\"" + this.TargetUrl(this.maxPage) + "\" class=\"fpnl-page\">" + this.maxPage.ToString() + "</a>";
            }

            /**
             * kiểm tra điều kiện và truyền phần tử vào thẻ chứa paging. 
             * một số dạng paging sau:
             * Ký hiệu: X = [none | combobox | editbox]
             * 1: F P ... 1 2 [X] 4 5 ... N L
             * 2: ... 1 2 [X] 4 5 ...
             * 3: 1 2 [X] 4 5
             * 4: F P N L
             * 5: [X] N L
             * 6: N L
             * 7: P N
             * 8: ..vv..
             */

            if (this.ShowPrev && prevLink != null)
            {
                outputPage.Append(prevLink);
                outputPage.AppendLine();
            }

            if (this.ShowFirst && firstLink != null)
            {
                outputPage.Append(firstLink);
                outputPage.AppendLine();
            }

            if (this.ShowPageNumber && pageLinks.Count > 0)
            {
                foreach (string link in pageLinks)
                {
                    outputPage.Append(link);
                    outputPage.AppendLine();
                }
            }

            if (this.ShowLast && lastLink != null)
            {
                outputPage.Append(lastLink);
                outputPage.AppendLine();
            }

            if (this.ShowNext && nextLink != null)
            {
                outputPage.Append(nextLink);
                outputPage.AppendLine();
            }
        }

        return outputPage.ToString();
    }

    /// <summary>
    /// Hàm xây dựng phân trang ajax
    /// </summary>
    /// <returns>trả về chuỗi html với cấu trúc phân trang.</returns>
    public string BuildPagingAjax(int pageIndex)
    {
        StringBuilder outputPage = new StringBuilder();

        this.MaxPage();

        if (this.totalCount > this.PerPage)
        {
            /**
             * xét giá trị lại biến "currentPage" trang hiện tại đang xem.
             */

            this.currentPage = pageIndex;

            /**
             * tính vùng số (thấp) thoát ra khỏi liên kết trước trang hiện tại.
             * vd: 1 2 [3].
             */
            if ((this.CurrentPage - this.LinksBefore) < 1)
            {
                this.lowerBoundary = 1;
            }
            else
            {
                this.lowerBoundary = this.CurrentPage - this.LinksBefore;
            }

            /**
		     * tính vùng số (cao) thoát ra khỏi liên kết sau trang hiện tại.
		     * vd: [3] 4 5.
		     */
            if ((this.CurrentPage + this.LinksAfter) > this.maxPage)
            {
                this.upperBoundary = this.maxPage;
            }
            else
            {
                this.upperBoundary = this.CurrentPage + this.LinksAfter;
            }

            /**
		     * tạo link từ miền giá trị lấy được trong khoảng giới hạn.
		     * khởi tạo mảng và truyền giá trị tương ứng vào mảng đó.
		     * vd: ... 1 2 [3] 4 5 ...
		     */
            ArrayList pageLinks = new ArrayList();

            if (this.ShowFirst && this.ShowMore && (this.CurrentPage - this.LinksBefore) > 1)
            {
                pageLinks.Add("<span class=\"borderNone p0 ml4 dotmore\">...</span>");
            }

            for (int i = this.lowerBoundary; i <= this.upperBoundary; i++)
            {
                if (this.CurrentPage == i)
                {
                    switch (this.CurrentMode)
                    {
                        case Mode.None:
                            pageLinks.Add("<span class=\"current-none\">" + i + "</span>");
                            break;                        
                        default:
                            pageLinks.Add("<span class=\"current-none\">" + i + "</span>");
                            break;
                    }
                }
                else
                {
                    pageLinks.Add("<a onclick=\"pagingEvent(" + i + ")\">" + i + "</a>");
                }
            }

            if (this.ShowMore && (this.CurrentPage + this.LinksAfter) < this.maxPage)
            {
                pageLinks.Add("<span class=\"borderNone p0 ml4 dotmore\">...</span>");
            }

            /**
	         * tạo link: first | prev | next | last.
	         */
            string firstLink = null;
            string prevLink = null;
            string nextLink = null;
            string lastLink = null;

            if (this.CurrentPage != 1)
            {
                prevLink = "<a title=\"previous\" onclick=\"pagingEvent(" + (this.CurrentPage - 1) + ")\" class=\"fpnl-page\">" + this.prevText + "</a>";
            }

            if (this.CurrentPage != this.maxPage)
            {
                nextLink = "<a title=\"next\" onclick=\"pagingEvent(" + (this.CurrentPage + 1) + ")\" >" + this.nextText + "</a>";
            }

            if (this.lowerBoundary != 1)
            {
                firstLink = "<a title=\"1\" onclick=\"pagingEvent(1)\" >" + this.FirstText + "</a>";
            }
            if (this.upperBoundary != this.maxPage)
            {
                lastLink = "<a  title=\"" + this.maxPage.ToString() + "\" onclick=\"pagingEvent(" + this.maxPage + ")\" >" + this.maxPage.ToString() + "</a>";
            }

            /**
             * kiểm tra điều kiện và truyền phần tử vào thẻ chứa paging. 
             * một số dạng paging sau:
             * Ký hiệu: X = [none | combobox | editbox]
             * 1: F P ... 1 2 [X] 4 5 ... N L
             * 2: ... 1 2 [X] 4 5 ...
             * 3: 1 2 [X] 4 5
             * 4: F P N L
             * 5: [X] N L
             * 6: N L
             * 7: P N
             * 8: ..vv..
             */

            if (this.ShowPrev && prevLink != null)
            {
                outputPage.Append(prevLink);
                outputPage.AppendLine();
            }

            if (this.ShowFirst && firstLink != null)
            {
                outputPage.Append(firstLink);
                outputPage.AppendLine();
            }

            if (this.ShowPageNumber && pageLinks.Count > 0)
            {
                foreach (string link in pageLinks)
                {
                    outputPage.Append(link);
                    outputPage.AppendLine();
                }
            }

            if (this.ShowLast && lastLink != null)
            {
                outputPage.Append(lastLink);
                outputPage.AppendLine();
            }

            if (this.ShowNext && nextLink != null)
            {
                outputPage.Append(nextLink);
                outputPage.AppendLine();
            }
        }

        return outputPage.ToString();
    }
    /// <summary>
    /// Thuộc tính cho trang.
    /// </summary>
    #region Paging Properties
    private string id;
    private string queryString;
    private string param;
    private int perPage;
    private int currentPage;
    private int linksBefore;
    private int linksAfter;
    private string firstText;
    private string prevText;
    private string nextText;
    private string lastText;
    private bool showFirst;
    private bool showPrev;
    private bool showNext;
    private bool showLast;
    private bool showMore;
    private bool showPageNumber;
    private Mode currentMode;
    private string inputInValid;
    /// <summary>
    /// ID của thẻ div chứa paging.
    /// </summary>
    public string ID
    {
        get { return id; }
        set { id = value; }
    }
    /// <summary>
    /// Trang liên kết. Chấp nhận 2 dạng sau:
    /// 1: "".
    /// 2: filename.aspx?query.
    /// </summary>
    public string QueryString
    {
        get { return queryString; }
        set
        {
            queryString = value;
            queryString = Regex.Replace(queryString, @"(.\d+)", "{0}", RegexOptions.Compiled);
        }
    }

    /// <summary>
    /// Biến QueryString.
    /// </summary>
    public string Param
    {
        get { return param; }
        set { param = value; }
    }
    /// <summary>
    /// Số dòng hiển thị trên một trang .
    /// </summary>
    public int PerPage
    {
        get { return perPage; }
        set { perPage = value; }
    }
    /// <summary>
    /// Số của trang hiện tại.
    /// </summary>
    public int CurrentPage
    {
        get { return currentPage; }
        set { currentPage = value; }
    }
    /// <summary>
    /// Số link trước trang hiện tại.
    /// </summary>
    public int LinksBefore
    {
        get { return linksBefore; }
        set { linksBefore = value; }
    }
    /// <summary>
    /// Số link sau trang hiện tai.
    /// </summary>
    public int LinksAfter
    {
        get { return linksAfter; }
        set { linksAfter = value; }
    }
    /// <summary>
    /// Text hiển thị cho link về trang đầu tiên.
    /// </summary>
    public string FirstText
    {
        get { return firstText; }
        set { firstText = value; }
    }
    /// <summary>
    /// Text hiển thị cho link tới trang trước.
    /// </summary>
    public string PrevText
    {
        get { return prevText; }
        set { prevText = value; }
    }
    /// <summary>
    /// Text hiển thị cho link tới trang kế tiếp.
    /// </summary>
    public string NextText
    {
        get { return nextText; }
        set { nextText = value; }
    }
    /// <summary>
    /// Text hiển thị cho link tới trang cuối.
    /// </summary>
    public string LastText
    {
        get { return lastText; }
        set { lastText = value; }
    }
    /// <summary>
    /// Cho phép hiển thị link về trang đầu tiên.
    /// </summary>
    public bool ShowFirst
    {
        get { return showFirst; }
        set { showFirst = value; }
    }
    /// <summary>
    /// Cho phép hiển thị link về trang trước.
    /// </summary>
    public bool ShowPrev
    {
        get { return showPrev; }
        set { showPrev = value; }
    }
    /// <summary>
    /// Cho phép hiển thị link tới trang kế tiếp.
    /// </summary>
    public bool ShowNext
    {
        get { return showNext; }
        set { showNext = value; }
    }
    /// <summary>
    /// Cho phép hiển thị link tới trang cuối.
    /// </summary>
    public bool ShowLast
    {
        get { return showLast; }
        set { showLast = value; }
    }
    /// <summary>
    /// Cho phép hiển thị thông báo tồn tại trang ngoài vùng [...].
    /// </summary>
    public bool ShowMore
    {
        get { return showMore; }
        set { showMore = value; }
    }
    /// <summary>
    /// Cho phép hiển thị link số trang trong vùng.
    /// </summary>
    public bool ShowPageNumber
    {
        get { return showPageNumber; }
        set { showPageNumber = value; }
    }
    /// <summary>
    /// Chế độ tùy chọn cho trang hiện tại.
    /// </summary>
    public Mode CurrentMode
    {
        get { return currentMode; }
        set { currentMode = value; }
    }
    /// <summary>
    /// Chuỗi thông báo khi chọn chế độ Mode.Editbox không hợp lệ.
    /// </summary>
    public string InputInValid
    {
        get { return inputInValid; }
        set { inputInValid = value; }
    }
    #endregion
}

/// <summary>
/// Xét cho CurrenMode.
/// </summary>
public enum Mode
{
    None,
    Combobox,
    Editbox   
}

/**
 * Phuongt<Phuongt@vinasoure.com.vn>: 29/04/2008
 *  - Ra v1.0.
 */