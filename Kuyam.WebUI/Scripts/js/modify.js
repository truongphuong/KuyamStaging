function get_param(param) {
    var search = window.location.search.substring(1);
    var compareKeyValuePair = function (pair) {
        var key_value = pair.split('=');
        var decodedKey = decodeURIComponent(key_value[0]);
        var decodedValue = decodeURIComponent(key_value[1]);
        if (decodedKey == param) return decodedValue;
        return null;
    };

    var comparisonResult = null;

    if (search.indexOf('&') > -1) {
        var params = search.split('&');
        for (var i = 0; i < params.length; i++) {
            comparisonResult = compareKeyValuePair(params[i]);
            if (comparisonResult !== null) {
                break;
            }
        }
    } else {
        comparisonResult = compareKeyValuePair(search);
    }

    return comparisonResult;
}

$(document).ready(function () {

    $("#modifypouperror").html("&nbsp;");
    $('#inputhour').timepicker({
        ampm: true,
        stepMinute: 15
    });

    $("#inputmonth").datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        dateFormat: "M dd",
        dayNamesMin: $.datepicker._defaults.dayNamesShort
    });

    $("#appointmentId").val(get_param('appointmentId'));

    $("#accordion").accordion("option", "active", 1);

    $("#datepicker").datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        dayNamesMin: $.datepicker._defaults.dayNamesShort
    });

    $(".pannelappointments").click(function () {
        $(".pannelappointments").removeClass("bgactive");
        $(this).addClass("bgactive");
    });
    $('.groupSelectName select,.selectMinute select,#category').selectmenu();

    $('.backtosearch').click(function () {
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#loginpopup').fadeIn(400);
        $('#loginpopup').css('top', ($('#lightBox').height() - $('#loginpopup').height()) / 2);
        $('#loginpopup').css('left', ($('#lightBox').width() - $('#loginpopup').width()) / 2);
        $('#loginpopup .btnYes').click(function () {
            $('#loginpopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
        $('#loginpopup .btnNo').click(function () {
            $('#loginpopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
    });

    var date = new Date();
    var d = date.getDate();
    var m = date.getMonth();
    var y = date.getFullYear();

    var calendar = $('#calendar').fullCalendar({
        defaultView: 'agendaWeek',
        header: {
            left: '',
            center: 'title',
            right: ''
        },
        selectable: false,
        selectHelper: true,
        allDaySlot: false,
        height: 800,
        firstDay: date.getDay(),
        minTime: 0,
        maxTime: 24,
        slotMinutes: 60,
        disableDragging: true,
        disableResizing: true,
        columnFormat: {
            week: 'ddd d'
        },
        select: function (start, end, allDay) {
            var title = prompt('Event Title:');
            if (title) {
                calendar.fullCalendar('renderEvent',
						{
						    title: title,
						    start: start,
						    end: end,
						    allDay: allDay
						},
						true // make the event "stick"
					);
            }
            calendar.fullCalendar('unselect');
        },
        editable: true,
        dayNamesShort: ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'],
        monthNamesShort: ['jan', 'feb', 'mar', 'apr', 'may', 'jun', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec'],
        eventClick: function (calEvent, jsEvent, view) {
            //alert('Event: ' + calEvent.title);
            //alert('Coordinates: ' + jsEvent.pageX + ',' + jsEvent.pageY);
            //alert('View: ' + view.name);    
            //$(".fc-event-skin").removeClass("fc-event-skin-active");
            //$(this).addClass("fc-event-skin-active");
            //alert(calEvent.currentAppointment);
            if (calEvent.currentAppointment == null) {
                $(".fc-event-skin").removeClass("fc-event-skin-active-modify");
                $(this).addClass("fc-event-skin-active-modify");
                var startDate = $.fullCalendar.formatDate(calEvent.start, "MMM dd");
                var startTime = $.fullCalendar.formatDate(calEvent.start, "h:mm tt");
                //alert(calEvent.duration);                
                $("#inputmonth").val(startDate.toLowerCase());
                $("#inputhour").val(startTime.toLowerCase());
                //alert(startTime);
            }
            if (calEvent.currentAppointment > 0) {
                $(".fc-event-skin").removeClass("fc-event-skin-active");
                $(this).addClass("fc-event-skin-active");
                $("#appointmentId").val(calEvent.currentAppointment);
                GetAppointmentInfo(calEvent.currentAppointment);
            }
        },
        readyState: function () {
        },
        events: "/Appointment/GetCalendars/?calendarId=" + get_param('calendarId') + "&appointmentId=" + get_param('appointmentId')
    });

    $('#calendarOption').live('change', function () {
        refeshcalendar();
        var calendarname = $("#calendarOption option:selected").text() + "’s calendar";       
        $("#availabilitycalendar").html(calendarname);
    });

    $('#employeeOption').live('change', function () {
        refeshcalendar();
        var employeename = $("#employeeOption option:selected").text() + "’s 7-day availability";
        $("#employeenameavailability").html(employeename);
    });

    $('.btnRequest').live('click', function () {
        requestModification();
    });

});


function GetAppointmentInfo(appointmentid) {
    $.get("/appointment/GetAppointmentInfo?appointmentId=" + appointmentid + "&nocache=" + getunixtime(), function (result) {
        //$("#appointmentdetail").html(result);
        var txtdate = result.appointment.startdate + "<span >&nbsp;at " + result.appointment.hour + "</span>";
        $("#currentdate").html(txtdate);
        var calendarname = "for " + result.appointment.calendarname;
        var employeename = "with " + result.appointment.employeename;
        $("#calendarname").html(calendarname);
        $("#employeename").html(employeename);
        $("#serveicename").html(result.appointment.servicename);
        $("#serviecdescription").html(result.appointment.sevicedescripton);
        $("#inputmonth").val(result.appointment.startdate);
        $("#inputhour").val(result.appointment.hour);

        $("#calendarOption").html(result.calendar);
        $("#employeeOption").html(result.employee);
        $("#servcieOption").html(result.service);
        $('select#calendarOption').selectmenu();
        $('select#employeeOption').selectmenu();
        $('select#servcieOption').selectmenu();
    });

}

function refeshcalendar() {
    $('#calendar').fullCalendar('removeEvents');
    $('#calendar').fullCalendar('addEventSource', "/Appointment/GetCalendars/?calendarId=" + $("#calendarOption option:selected").val() + "&employeeId=" + $("#employeeOption option:selected").val() + "&appointmentId=" + $("#appointmentId").val());
}


function requestModification() {

    var appointmentId = $("#appointmentId").val();
    var calendar = document.getElementById('calendarOption');
    if (calendar.options.length > 0) {
        var calendarId = calendar.options[calendar.selectedIndex].value;
    } else {
        return;
    }

    var employeeservice = document.getElementById('employeeOption');
    var employeeid = null;
    var employeeName = "";
    if (employeeservice.options.length > 0) {
        var employeeidSelect = employeeservice.options[employeeservice.selectedIndex].value;
        if (isNaN(employeeidSelect) == false) {
            employeeid = employeeidSelect;
        }
        employeeName = employeeservice.options[employeeservice.selectedIndex].text;
    } else {
        return;
    }

    var servicetype = document.getElementById('servcieOption');
    if (servicetype.options.length > 0) {
        var serviceId = servicetype.options[servicetype.selectedIndex].value;
    } else {
        //$("#modifypouperror").html("please choose one service");
        return;

    }
    var monthday = $(".inputMonth").val();
    if (monthday == '') {
        return;
    }
    var hour = $(".inputTime").val();
    if (hour == '') {
        return;
    }

    var now = new Date();
    var unixnow = Date.parse(now);
    var date = $.datepicker.parseDate('M dd', monthday);
    var tmp = date.toDateString() + " " + hour;
    var unixdate = Date.parse(tmp);
    if (unixnow >= unixdate) {
        $("#modifypouperror").html("invalid time");
//        alert("aa");
        return;
    }
    date = monthday + " " + hour;    
    var param = "appointmentId=" + appointmentId + "&serviceId=" + serviceId + "&employeeid=" + employeeid + "&employeeName=" + employeeName + "&calendarId=" + calendarId + "&date=" + date;
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    commonPostAjax("Appointment", "RequestModification", param, callbacksucess, setError);

}

function callbacksucess(result) {
    $('#aptImgLoader').hide();
    $('#lightBox').css('opacity', '0.3').fadeOut(400);   
    window.location = "/appointment/";
} 