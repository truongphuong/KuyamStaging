var process = 0;
var serviceStartDate = new Date();
$(function () {
    $('select#allcategories').selectmenu();
    $(".lualist li").removeClass("active");
    $(".lualist #li7").addClass("active");
});
$(document).ready(function () {
    $("#accordion").accordion("option", "active", 1);

    $('#searchDate').datepicker({ dateFormat: 'mm/dd/yy',
        onSelect: function (date, instance) {
            serviceStartDate = new Date(date);
            fillterByAll();
        }
    });

    var defaultdate = $.datepicker.formatDate('mm/dd/yy', serviceStartDate);
    SetDateForSearch(defaultdate);

    $('.lnksubmitreview').live('click', function () {
        $("#formaddreview").load("/appointment/LoadAppointmentReviewPopup?appointmentID=" + $(this).attr('appid') + "&nocache=" + getunixtime(), function () {
            lengthofviewnote();
            showDialog('formaddreview');
        });

    });

    $('.btnpreview').live('click', function () {
        $('#appointmentdetail').empty();
        $('#divViewhistory').show();
        serviceStartDate.setDate(serviceStartDate.getDate() - 7);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        fillterByAll();
    });

    $('.btnnext').live('click', function () {
        $('#appointmentdetail').empty();
        $('#divViewhistory').show();
        serviceStartDate.setDate(serviceStartDate.getDate() + 7);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        fillterByAll();
    });

    //script select employee

    var employeelistcount = $('#employeeslist li').length;
    var employeelistli = 1;

    $('#employees').click(function () {
        $('#employeeslist').slideToggle('normal', function () {
            if ($('#employeeslist').is(':hidden')) {
                $('#employees').removeClass('schedulefavoriteactive');
                $('#employees').addClass('schedulefavorite');
                $('#employeeslistarrowdown').css('display', 'none');
                $('#employeeslistarrowup').css('display', 'none');
                $('#employeeslistarrowdown').css('border-top-color', '#999999');
            } else {

                $('#employees').removeClass('schedulefavorite');
                $('#employees').addClass('schedulefavoriteactive');

                if (employeelistcount >= 3) {
                    $('#employeeslistarrowdown').css('display', 'block');
                    btnemployeesdownreset();
                }
                else {
                    $('#employeeslistarrowdown').css('display', 'none');
                    $('#employeeslist').css('border-bottom', '1px solid #999999');
                }

            }
        });
    });

    $("#employeeslist li").click(function () {
        var index = $("#employeeslist li a").index($('a[class="active"]'));
        if (index != -1) {
            $("#employeeslist li a").eq(index).removeClass("active");
        }
        var objA = $("#employeeslist li a").eq($("#employeeslist li").index(this));
        objA.addClass("active");
        $("#employees").attr("title", objA.text()).text(objA.text());
    });

    $('#employeeslistarrowdown').click(function () {
        if (employeelistli < employeelistcount) {
            $('#employeeslistarrowup').css('display', 'block');
            $('#employeeslistarrowdown').css({ 'border-top-color': 'transparent', 'border-top-width': '0px' });
            if ((employeelistli + 3) <= employeelistcount) {
                $('#employeeslist li:nth-child(' + employeelistli + ')').slideToggle();
                employeelistli++;
                $('#employeeslistarrowup').show();
            }
        }
        if (employeelistli + 3 > employeelistcount) {
            $('#employeeslistarrowdown').hide();
            $('#employeeslistarrowup').removeClass('schedulefavoritearrowup').addClass('schedulefavoritearrowup1');
        }

    });
    $('#employeeslistarrowup').click(function () {
        if (employeelistli > 1) {
            if ((employeelistli + 3) >= 1) {
                employeelistli--;
                $('#employeeslist li:nth-child(' + employeelistli + ')').slideToggle();
                $('#employeeslistarrowdown').show();
                $('#employeeslistarrowup').removeClass('schedulefavoritearrowup1');
                $('#employeeslistarrowup').addClass('schedulefavoritearrowup');
            }
        }
        if (employeelistli == 1) {
            $('#employeeslistarrowup').hide();
            btnemployeesdownreset();
        }

    });

    //end script select employee

    $('#allcategories').live('change', function () {
        $('#appointmentdetail').empty();
        fillterByAll();
    });
});

function SetDateForSearch(date) {
    $('#searchDate').val(date);
}

function fillterByAll() {    
    var calendarId = 0;
    var acal = $('#employeeslist').find('.active');
    if (acal.length > 0)
        calendarId = acal.attr('calendarid');
    var status = 0;
    var startDate = $('#searchDate').val();
    var selected = document.getElementById('allcategories');
    var serviceid = selected.options[selected.selectedIndex].value;
    $("#divViewhistory").hide();
    $.get("/appointment/filterHistoryByAll?nocache=" + getunixtime(), { calendarId: calendarId, status: status, startDate: startDate, serviceid: serviceid }, function (result) {
        $("#appointmentdetail").html(result);      
        
        $('.lnksubmitreview').click(function () {
            $("#formaddreview").load("/appointment/LoadAppointmentReviewPopup?appointmentID=" + $(this).attr('appid') + "&nocache=" + getunixtime(), function () {
                lengthofviewnote();
                showDialog('formaddreview');
                return false;
            });

        });
    });
}

function viewnote(apptId) {
    $('#viewnotespopup').html("");
    $("#viewnotespopup").load("/appointment/LoadAppointmentNotePopup?appointmentID=" + apptId + "&nocache=" + getunixtime(), function () {
        showDialog('viewnotespopup');
        lengthofviewnote();
    });
}


function fillterByCalendar(calendarId) {
    if (calendarId != 0) {
        var startDate = $('#searchDate').val();
        var selected = document.getElementById('allcategories');
        var serviceid = selected.options[selected.selectedIndex].value;
        $.get("/appointment/fillterHistoryByCalendar?calendarId=" + calendarId + "&startDate=" + startDate + "&serviceid=" + serviceid + "&nocache=" + getunixtime(), function (result) {
            $("#appointmentdetail").html(result);
        });
    }
}

function filterAllHistory() {
    var calendarId = 0;
    var acal = $('#employeeslist').find('.active');
    if (acal.length > 0)
        calendarId = acal.attr('calendarid');
    var status = 0;
    var startDate = $('#searchDate').val();
    var selected = document.getElementById('allcategories');
    var serviceid = selected.options[selected.selectedIndex].value;
    $.get("/appointment/filterAllHistory?nocache=" + getunixtime(), { calendarId: calendarId, status: status, startDate: startDate, serviceid: serviceid }, function (result) {
        $("#appointmentdetail").html(result);
        $('#divViewhistory').hide();
    });
}

