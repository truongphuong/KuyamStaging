/// <reference path="jquery-1.7.2.js" />
/// <reference path="jquery-ui-1.8.23.custom.min.js" />
/// <reference path="jquery.printElement.min.js" />
/// <reference path="jspdf.js" />

var serviceStartDate = new Date();
$(document).ready(function () {
    accordionforadmin(0);
    $('#searchDate').datepicker({
        onSelect: function (date, instance) {
            serviceStartDate = new Date(date);
            GetCompanyInvoicesInfo(1);
        }
    });

    $('#CompanyInvoicesMenu').attr("class", "leftitem choose");
    var defaultdate = $.datepicker.formatDate('mm/dd/yy', serviceStartDate);
    SetDateForSearch(defaultdate);
    // GetCompanyInvoicesInfo();
    $('.btnpreview').live('click', function () {
        
        $('.invocelist').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() - 1);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        GetCompanyInvoicesInfo(1);
    });

    $('.btnnext').live('click', function () {
        $('.invocelist').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() + 1);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        GetCompanyInvoicesInfo(1);
    });

    $('#allcategories').live('change', function () {
        $('.invocelist').empty();
        GetCompanyInvoicesInfo(1);
    });

    $('#paymentMethod').live('change', function () {
        $('.invocelist').empty();
        GetCompanyInvoicesInfo(1);
    });

    $('#txtseachname').live('keyup', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') { //Enter is pressed.
            $('.invocelist').empty();
            GetCompanyInvoicesInfo(1);
        }

    });
    setTimeout(addToolTip, 400);
});

function SetDateForSearch(date) {
    $('#searchDate').val(date);
}
//Tooltip for categories
function addToolTip() {
    var options = $('#allcategories option').toArray();
    var links = $('#allcategories-menu li[role="presentation"] a');
    for (var i = 0; i < options.length; i++) {
        $(links[i]).attr('title', $(options[i]).attr('title'));
    }
}

function OnSucess(result) {
    if (result.length == 0) // No data returned from the server
    {
        $('.invocelist').empty().append('<div>No data found!</div>');
    }

    accordionforadmin(0);

    $('.groupSelectName select,.selectMinute select, #allcategories, #paymentMethod').selectmenu();
    clapinvoices("invoceitem", "companyinvoiceactive");
    createtopleftscroll();


}

function OnError(error) {
    alert('Error');
}

function formatJsonDate(jsonDate) {
    return (new Date(parseInt(jsonDate.substr(6)))).format("mm/dd/yy");
};

function formatJsonHours(jsonDate) {
    var hours = new Date(parseInt(jsonDate.substr(6))).getHours();
    var minutes = new Date(parseInt(jsonDate.substr(6))).getMinutes();

    if (hours >= 0 && hours <= 12) {
        hours += ":" + minutes + "am";
    }
    else {
        hours += ":" + minutes + "pm";
    }

    return hours;
};


function GetCompanyInvoicesInfo(pageIndex) {
    //Prepare parameters for ajax callback to server.
    var startDates = serviceStartDate;
    //alert(serviceStartDate);
    var serviceIds = $('#allcategories option:selected').val();
    var empNames = $('#txtseachname').val();
    var payMethods = $('#paymentMethod option:selected').val();
    var parameters = { serviceStartDate: startDates, serviceId: serviceIds, empName: empNames, paymentMethod: payMethods, page: pageIndex, companyId: companyId };
    var option = {
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(parameters),
        dataType: 'html',
        url: '/companyappointment/CompanyInvoices',
        success: function (result) {
            $('.invocelist').empty();
            if (result.length == 0) {
                $('.invocelist').append('<div>No Data found!</div>');
            }
            else {
                $('.invocelist').html(result);
            }

            accordionforadmin(0);

            $('.groupSelectName select,.selectMinute select,#category, #allcategories, #anypayment').selectmenu();
            clapinvoices("invoceitem", "companyinvoiceactive");
            createtopleftscroll();
        },
        error: function (error) {
            alert(error);
        }
    };

    $.ajax(option);
}

function accordionforadmin(activetab) {
    $("scroll").hide();
    $("#accordion").accordion({
        active: activetab,
        autoHeight: false,
        navigation: true
    });
    $("scroll").show();
}

//This function using for slideUp and slideDown of User Invoices page
function clapinvoices(classitem, activeclass) {

    $("." + classitem).stop().toggle(
        function () {

            var currentId = $(this).attr("id");
            $("." + classitem).each(function (i, e) {
                $(".activeitem").removeClass(activeclass);
                $(".invitemcontent").hide("slow");
                $(".invicon a").attr("class", "plus");
            });

            $("#" + currentId + " .activeitem").addClass(activeclass);
            $("#" + currentId + " .invitemcontent").show("slow");
            $("#" + currentId + " .invicon a").attr("class", "minus");
            $("#" + currentId + " #invamount").hide();
            $("#" + currentId + " #ordertotal").show();
        },
		function () {
		    var currentId = $(this).attr("id");
		    $("#" + currentId + " .activeitem").removeClass(activeclass);
		    $("#" + currentId + " .invitemcontent").hide("slow");
		    $("#" + currentId + " .invicon a").attr("class", "plus");
		    $("#" + currentId + " #ordertotal").hide();
		    $("#" + currentId + " #invamount").show();
		});
}


function pagingEvent(pageIndex) {
    GetCompanyInvoicesInfo(pageIndex);
}


function printInvoiceById(invoiceId) {
    printContent(invoiceId);
}

function printInvoicesList() {
    var str = "";
    $('.invoceitem').each(function () {
        var currentId = $(this).attr('id');
        var content = document.getElementById(currentId).innerHTML + '\n' + document.getElementById("invoiceDetail" + currentId.substr(currentId.length - 2)).innerHTML + '<hr/>';
        str += content + '<br/>';
    });

    $('<div id="invoiceItemList"/>').append(str).appendTo('body').css('display', 'none');
    printContent('invoiceItemList');
}

function downloadCompanyInvoicesList() {
    var serviceStartDates = serviceStartDate.format();
    var serviceIds = $('#allcategories option:selected').val();
    var empNames = $('#txtseachname').val();
    var payMethods = $('#paymentMethod option:selected').val();
    var url = '/CompanyAppointment/DownloadInvoicesAsPdf' + '?serviceStartDate=' + serviceStartDates + '&serviceId=' + serviceIds + '&empName=' + empNames
              + '&paymentMethod=' + paymentMethod + "&profileId=" + companyId;
    window.location = url;
}

function printContent(id) {
    if (id == 'invoiceItemList') {
        $('#invoiceItemList').css('display', 'block');
    }
    $('.printinvoce').removeAttr('display');
    $('.printinvoce').css('display', 'none');
    $('#' + id).printElement(
                    {
                        printMode: 'popup'
                    }
                 );
    $('.printinvoce').css('display', 'block');
    if (id == 'invoiceItemList') {
        $('#invoiceItemList').css('display', 'none');
    }

    //        var str = document.getElementById(ids).innerHTML;
    //        newwin = window.open('', 'printwin', 'left=100,top=100,width=700,height=500')
    //        newwin.document.write('<HTML>\n<HEAD>\n')
    //        newwin.document.write('<TITLE>Company Invoices</TITLE>\n')
    //        newwin.document.write('<script>\n')
    //        newwin.document.write('function chkstate(){\n')
    //        newwin.document.write('if(document.readyState=="complete"){\n')
    //        newwin.document.write('window.close()\n')
    //        newwin.document.write('}\n')
    //        newwin.document.write('else{\n')
    //        newwin.document.write('setTimeout("chkstate()",2000)\n')
    //        newwin.document.write('}\n')
    //        newwin.document.write('}\n')
    //        newwin.document.write('function print_win(){\n')
    //        newwin.document.write('window.print();\n')
    //        newwin.document.write('chkstate();\n')
    //        newwin.document.write('}\n')
    //        newwin.document.write('<\/script>\n')
    //        newwin.document.write('<link rel="stylesheet" type="text/css" href="../../css/stylePrintable.css"/>');
    //        newwin.document.write('</HEAD>\n')
    //        newwin.document.write('<BODY onload="print_win()">\n')
    //        newwin.document.write(str)
    //        newwin.document.write('</BODY>\n')
    //        newwin.document.write('</HTML>\n')
    //        newwin.document.close()
}