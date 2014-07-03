var process = 0;
var serviceStartDate = new Date();
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

    $('.btnpreview').live('click', function () {
        $('#appointmentdetail').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() - 7);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        fillterByAll();
    });

    $('.btnnext').live('click', function () {
        $('#appointmentdetail').empty();
        serviceStartDate.setDate(serviceStartDate.getDate() + 7);
        SetDateForSearch($.datepicker.formatDate('mm/dd/yy', serviceStartDate));
        fillterByAll();
    });

    $('#allcategories').live('change', function () {
        $('#appointmentdetail').empty();
        fillterByAll();
    });

    $(".pannelappointments").click(function () {
        $(".pannelappointments").removeClass("bgactive");
        $(this).addClass("bgactive");
    });

    $('select#allcategories').selectmenu();

    $('.groupSelectName select,.selectMinute select,#category').selectmenu();


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
        //$("#employees").attr("title", objA.text()).text(objA.text());
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
});

//end  document ready

function SetDateForSearch(date) {
    $('#searchDate').val(date);
}

//function fillterBySattus(status) {
//    $.get("/appointment/fillterByStatus?status=" + status + "&nocache=" + getunixtime(), function (result) {
//        $("#appointmentdetail").html(result);
//    });
//}

function fillterByCalendar(calendarId) {
    var flag = false;
    var key = $("#hdfKey").val();
    if (key != null && typeof key != "undefined") {
        flag = true;
    }
    
    $.get("/appointment/fillterByCalendar?calendarId=" + calendarId + "&nocache=" + getunixtime() + "&flag=" + flag, function (result) {
        $("#appointmentdetail").html(result);
    });
    
}

function fillterByAll() {
    var calendarId = 0;
    var status = 0;
    var startDate = $('#searchDate').val();
    var selected = document.getElementById('allcategories');
    var serviceid = selected.options[selected.selectedIndex].value;
    $.get("/appointment/fillterByAll?nocache=" + getunixtime(), { calendarId: calendarId, status: status, startDate: startDate, serviceid: serviceid }, function (result) {
        $("#appointmentdetail").html(result);
    });
}


function cancel(apptId) {
    $("#cancelpopup").html("");
    $("#cancelpopup").load("/appointment/LoadAppointmentCancelPopup?appointmentID=" + apptId + "&nocache=" + getunixtime(), function () {
        showDialog('cancelpopup');
    });
}

function confirm(apptId, status) {

    var flag = false;
    var key = $("#hdfKey").val();
    if (key != null && typeof key != "undefined") {
        flag = true;
    }

    if (process == 1)
        return;
    process = 1;
    var param = "appointmentID=" + apptId + "&status=" + status + "&flag=" + flag;
    $('#lightBox').css('opacity', '0.1').fadeIn(200);
    $('#aptImgLoader').show();
    commonPostAjax("Appointment", "ChangeStatus", param, setlistAppoiontment, setError);
//    commonPostAjax("Appointment", "LoadMasterAgenda", "", loadMenu, setError);
}
function setlistAppoiontment(result) {
    
    $('#lightBox').hide();
    $('#aptImgLoader').hide();
    $("#appointmentdetail").html(result);
    process = 0;
    commonPostAjax("Appointment", "LoadMasterAgenda", "", loadMenu, setError);
}

function remove1(apptId, status) {
    
    var flag = false;
    var key = $("#hdfKey").val();
    if (key != null && typeof key != "undefined") {
        flag = true;
    }

    if (process == 1)
        return;
    process = 1;
    var param = "appointmentID=" + apptId + "&status=" + status + "&flag=" + flag;
    $('#lightBox').css('opacity', '0.1').fadeIn(200);
    $('#aptImgLoader').show();
    commonPostAjax("Appointment", "ChangeStatus", param, setlistAppoiontment, setError);
    
}

function loadMenu(response) {
    var arr = response.split('#');
    $('#menuPen').text(arr[0]);
    $('#menuMod').text(arr[1]);
    $('#menuCon').text(arr[2]);
    $('#menuCan').text(arr[3]);
}

function viewnote(apptId) {
    $('#viewnotespopup').html("");
    $("#viewnotespopup").load("/appointment/LoadAppointmentNotePopup?appointmentID=" + apptId + "&nocache=" + getunixtime(), function () {
        showDialog('viewnotespopup');
        //lengthofviewnote();
    });
}