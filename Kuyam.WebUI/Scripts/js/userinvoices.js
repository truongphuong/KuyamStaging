var serviceStartDate = new Date();
$(document).ready(function () {
    $("#accordion").accordion("option", "active", 1);
    $('#searchDate').datepicker({ dateFormat: 'mm/dd/yy',
        onSelect: function (date, instance) {
            serviceStartDate = new Date(date);
            GetuserInvoicesInfo(1);
        }
    });
    var defaultdate = $.datepicker.formatDate('mm/dd/yy', serviceStartDate);
    SetDateForSearch(defaultdate);

    $('.groupSelectName select,.selectMinute select,#category, #allcategories, #paymentmethod').selectmenu();
    clapinvoices("invoceitem", "invoceitemactive");

    $('.btnpreview').live('click', function () {
        $('.invocelist').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() - 1);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        GetuserInvoicesInfo(1);
    });

    $('.btnnext').live('click', function () {
        $('.invocelist').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() + 1);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        GetuserInvoicesInfo(1);
    });

    $('#allcategories').live('change', function () {
        $('.invocelist').empty();
        GetuserInvoicesInfo(1);
    });

    $('#paymentmethod').live('change', function () {
        $('.invocelist').empty();
        GetuserInvoicesInfo(1);
    });

    $('#txtseachname').live('keyup', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') { //Enter is pressed.
            $('.invocelist').empty();
            GetuserInvoicesInfo(1);
        }

    });

    $(".lualist li").removeClass("active");
    $(".lualist #li8").addClass("active");
    setTimeout(addToolTip, 400);
});

//end  document

function pagingEvent(pageIndex) {
    GetuserInvoicesInfo(pageIndex);
}

function printInvoiceById(invoiceId) {
    printContent(invoiceId);
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
}

function printInvoicesList() {
    var str = "";
    $('.invoceitem').each(function () {
        var currentId = $(this).attr('id');       
        var content = document.getElementById(currentId).innerHTML + '<hr/>';
        str += content + '<br/>';
    });

    $('<div id="invoiceItemList"/>').append(str).appendTo('body').css('display', 'none');
    printContent('invoiceItemList');
}
//Tooltip for categories
function addToolTip() {
    var options = $('#allcategories option').toArray();
    var links = $('#allcategories-menu li[role="presentation"] a');
    for (var i = 0; i < options.length; i++) {
        $(links[i]).attr('title', $(options[i]).attr('title'));
    }
}

function downloadInvoicesList() {
    var serviceStartDates = serviceStartDate.format();
    var serviceIds = $('#allcategories option:selected').val();    
    var empNames = $('#txtseachname').val();
    var payMethods = $('#paymentMethod option:selected').val();

    var url = '/appointment/DownloadInvoicesList' + '?serviceStartDate=' + serviceStartDates + '&serviceId=' + serviceIds + '&empName=' + empNames
              + '&paymentMethod=' + payMethods;
    window.location = url ;
    

//    var parameters = { serviceStartDate: serviceStartDates, serviceId: serviceIds};
//    var option = {
//        type: 'POST',
//        contentType: 'application/json; charset=utf-8',
//        data: JSON.stringify(parameters),
//        dataType: 'html',
//        url: '/appointment/DownloadInvoicesList',
//        success: function (result) {           
//            if (result.length == 0) {                
//            }
//            else {                
//            }            
//        },
//        error: function (error) {
//            alert(error);
//        }
//    };
//    $.ajax(option);
}


function GetuserInvoicesInfo(pageIndex) {
    //Prepare parameters for ajax callback to server.
    var serviceStartDates = serviceStartDate;
    var serviceIds = $('#allcategories option:selected').val();
    var empNames = $('#txtseachname').val();
    var payMethods = $('#paymentmethod option:selected').val();    
    var parameters = { serviceStartDate: serviceStartDates, serviceId: serviceIds, empName: empNames, paymentMethod: payMethods, page: pageIndex };
    var option = {
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(parameters),
        dataType: 'html',
        url: '/appointment/Receipt',
        success: function (result) {
            $('.invocelist').empty();
            if (result.length == 0) {
                $('.invocelist').append('<div>No Data found!</div>');
            }
            else {
                $('.invocelist').html(result);
            }          

            $('.groupSelectName select,.selectMinute select,#category, #allcategories, #anypayment').selectmenu();
            clapinvoices("invoceitem", "invoceitemactive");
            createtopleftscroll();
        },
        error: function (error) {
            alert(error);
        }
    };

    $.ajax(option);
}

function SetDateForSearch(date) {
    $('#searchDate').val(date);
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