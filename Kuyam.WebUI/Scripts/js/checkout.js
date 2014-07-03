    var inervalId;
    var countsecond = 30; // 30 second
    var extend5MinuteMore = 300; // 5 minute
    var countDownFrom = 600; // 10 minute
    var isTimeExtendMode = false;
    var lastCheckTimeSlot = new Date().getTime();
    function startCountDown(selector) {
        inervalId = window.setInterval('updateClock("' + selector + '");', 1000);
        //        $.post("/AppointmentPreview/SetLockedTimeSlot", { key: '@ViewBag.LockKey', minute: 10 });
    }

    function formatNumber(number) {
        if (number < 10) {
            return "0" + number;
        }
        return "" + number;
    }

    function updateClock(selector) {
        if (countDownFrom <= 0 && isTimeExtendMode === false) {
            showDelayPopup();
            return;
        } else if (countDownFrom <= 0) {
            hideDialog('newpayment');
            hideDialog('delayclock');
            window.clearInterval(inervalId);
            return;
        }
        var minute = parseInt(countDownFrom / 60);
        var second = countDownFrom % 60;
        $(selector).html(formatNumber(minute) + ":" + formatNumber(second));

        if (countDownFrom <= 0) {
            //            $.post('/AppointmentPreview/RemoveLockedTimeSlot', { key: '@ViewBag.LockKey' }, function () { });
            window.clearInterval(inervalId);

        }
        countDownFrom -= 1;
    }

    function showDelayPopup() {
        showDialog('delayclock');
        countDownFrom = countsecond;
        isTimeExtendMode = true;
    }

    function changeTime() {
        $('#delayclock').hide();
        countDownFrom = extend5MinuteMore;
        isTimeExtendMode = true;       
        $('.ptime').html("please complete your request in 05:00 minutes. otherwise this time will be released to others.");
    }

    function cancelbooking() {
        hideDialog('delayclock');
        hideDialog('newpayment');
        commonGetAjax("CompanyProfile", "ClearTimeSlot", "", null, setError);
    }

    function dateDiff(start, end) {
        var days = (end.getTime() - start.getTime()) / 1000 / 60;
        return days;
    }



    function continueCheckout(seviceId) {
        var policy = $("#hdfPolicy").val();
        var cncelHour = $("#hdfCancelHour").val();
        var percent = $("#hdfCancelPercent").val();
        hideDialog('selectservice');
        if (policy > 0) {
            var dateNow = new Date();
            var endDate = new Date($("#hdfCurrentDate").val());
            var total = dateDiff(dateNow, endDate);
            var canceltime = cncelHour * 60;
            if (total <= canceltime) {
                $(".cancellationbox").css({ 'background-color': '#FF7F7F' })
            } else {
                $(".cancellationbox").css({ 'background-color': '#FEFBC7' })
            }

            $("#cancelhour").html(cncelHour);
            if (policy == 1) {
                $('#policyType').html(cncelHour);
            } else if (policy == 2) {
                $('#policyType').html(cncelHour);
            } else if (policy == 3) {
                $('#policyType').html(cncelHour);
                $("#returnpercent").html(percent);
                if (cncelHour == '0') {
                    $('#policyType').html("anytime");
                    $("#cancelhour").html("anytime");
                }
                if (percent == '0') {
                    percent = "100"
                } else if (percent == "25") {
                    percent = "75";
                } else if (percent == "50") {
                    percent = "50";
                } else if (percent == "75") {
                    percent = "25";
                }
                $("#returnpercent").html(percent + "%");
            }
            setTimeout("showDialog('cancellationpopup')", 700);

        } else { checkout(); }
    }

    function checkout() {
        checkTimeSlot(checkoutHandle);
    }

    function checkoutHandle() {
        loaddataCheckout();
        hideDialog('cancellationpopup');
        setTimeout("showDialog('newpayment')", 500);
        isTimeExtendMode = false;
        countDownFrom = 600; //10 minute
        window.clearInterval(inervalId);
        startCountDown('.countdownclock');
    }

    function loaddataCheckout() {
        $("#applycodeError").html("");
        var selected = document.getElementById('servicetype');
        var seviceId = selected.options[selected.selectedIndex].value;   
        var calendar = document.getElementById('username');
        var calendarId = calendar.options[calendar.selectedIndex].value;
        var startDate = new Date($("#hdfCurrentDate").val());
        var expectedDate = $.fullCalendar.formatDate(startDate, "yy/MM/dd hh:mm tt");
        var packageId = $("#activepackageId").val()
        var employeeId = $("#servicetype option:selected").attr('employeeid');
        var param = "serviceId=" + seviceId + "&employeeId=" + employeeId + "&calendarId=" + calendarId + "&startDate=" + expectedDate + "&packageId=" + packageId;
        commonGetAjax("CompanyProfile", "GetDataCheckoutByServiceId", param, setDataCheckout, setError);
    }

    function setDataCheckout(result) {
        $("#newpayment").html("");
        $("#newpayment").html(result.content);
        $('#cbemail').checkBox({ addVisualElement: false });
        $('#cbsms').checkBox({ addVisualElement: false });
        var descriptionpkg = $("#packagename").val();
        var packageremain = $("#packageremain").val();
        if (packageremain != -1) {
            packageremain -= 1;
            descriptionpkg += "<br><span>" + packageremain + " remaining after booking</span>";
        } else {
            descriptionpkg += "<br><span>unlimited booking(s)</span>";
        }
        $("#descpkginfo").html(descriptionpkg);
    }

    function checkTimeSlot(nextFunction) {
        lastCheckTimeSlot = new Date().getTime();
        var selected = document.getElementById('servicetype');
        var seviceId =  selected.options[selected.selectedIndex].value;        
        var employeeId = $("#servicetype option:selected").attr('employeeid');
        var calendar = document.getElementById('username');
        var calendarId = calendar.options[calendar.selectedIndex].value;
        var startDate = new Date($("#hdfCurrentDate").val());
        var expectedDate = $.fullCalendar.formatDate(startDate, "yy/MM/dd hh:mm tt");
        var packageId = $("#activepackageId").val();
        var param = "serviceId=" + seviceId + "&employeeId=" + employeeId + "&calendarId=" + calendarId + "&startDate=" + expectedDate + "&packageId=" + packageId;
        commonGetAjax("CompanyProfile", "CheckTimeSlot",
            param,
            function (result) {
               if(result==false)
                {
                   showAlertMessage("it looks like this time has already been booked. please refresh the page and select a different employee or time.", reloadPage);

                } else {
                    nextFunction();
                }

            },
            setError);
    }


    function selectCheckout(employeeId, startDate, startTime) {
        startTime = "starts: " + startTime;
        $('#divystart').text(startDate);
        $('#divystarttime').text(startTime);
        getServicebyEmployeeId(employeeId);
        
    }

    function getServicebyEmployeeId(id) {
        var packageId = $("#activepackageId").val();
        var startDate = new Date($("#hdfCurrentDate").val());
        var expectedDate = $.fullCalendar.formatDate(startDate, "yy/MM/dd hh:mm tt");
        var param ="profileId=" +$("#profileid").val()+"&employeeId=" + id + "&packageId=" + packageId + "&startDate=" + expectedDate;
        commonGetAjax("CompanyProfile", "GetServiceByEmployeeId", param, sethtmloption, setError);
    }
    

    function sethtmloption(result) {
        
        if(typeof result.invalidtime != 'undefined')
        {
           showAlertMessage("please select a different time because this slot is not enough duration.");
            return;
        }

        $("#servicetype").html("");
        $("#servicetype").html(result.sevice);
        $('#servicetype option:selected').next('option').attr('selected', 'selected');
        var eployeeName= $("#servicetype option:selected").attr('employeename');       
        var employeeNameTruncate=eployeeName;
        if (typeof employeeNameTruncate!= 'undefined' && employeeNameTruncate.length > 15) {
            employeeNameTruncate = employeeNameTruncate.substring(0, 15) + "...";
        }
        $('#employeeName').text(eployeeName);
        var companyName = $('#hdfcompanyname').val();

        $("#companyName").text(companyName);       
        var note = "to book, select from " + employeeNameTruncate + "’s services:";
        $("#hnote").text(note);
        $('#companyName').attr('title', companyName);
        $('#employeeName').attr('title', eployeeName);
                
        $("#username").html(result.calendar);
        var packageId = $("#activepackageId").val();
        var packageremain = $("#packageremain").val();
        if (packageId != '' && packageremain != 0) {
            var descriptionpkg = $("#packagename").val() + " - ";

            if (packageremain != -1) {
                packageremain += " remaining";
            } else {
                packageremain = "unlimited booking(s)";
            }
            descriptionpkg += "<span id=\"maxusespkg\" class=\"colorred\">" + packageremain + "</span>";
            $("#txtpackage").html(descriptionpkg);
            $("#remainingPkg").show();
            $("#massageexperience").show();

        } else {
            $("#remainingPkg").hide();
            $("#massageexperience").hide();
        }
        $('select#servicetype').selectmenu();
        $('select#username').selectmenu();

        $("#yendtime").html("");
        showDialog("selectservice");
    }

    function populateServiceHour(employeeId, serviceId, flag) {
        if (typeof serviceId == 'undefined' || typeof employeeId == 'undefined') {
            return;
        }
        $('#calendar').fullCalendar('removeEvents');
        var calendarId = $(".personcalendar .activeperson").attr("id");
        $('#calendar').fullCalendar('addEventSource', "/CompanyProfile/GetCalendars/?employeeId=" + $("#message option:selected").val() + "&calendarId=" + calendarId);
        //$('#calendar').fullCalendar('refetchEvents');       
    }

    function bookAppointmentNow() {
        var mess = $('#txtCustomerScheduleLog').val();
        if (mess.length > 1000) {
            alert("note must be less than 1000 characters.");
            return false;
        }
        var selected = document.getElementById('servicetype');
        var seviceId = selected.options[selected.selectedIndex].value;
        var employee = document.getElementById('message');
        var employeeId = employee.options[employee.selectedIndex].value;
        var startDate = new Date($("#hdfCurrentDate").val());
        var expectedDate = $.fullCalendar.formatDate(startDate, "yy/MM/dd hh:mm tt");
        var packageId = 0;
        var note = $('#txtCustomerScheduleLog').val();
        if (note == 'e.g. i have lower back pain')
            note = '';
        var param = "email=" + $('#cbemail').attr('checked') + "&sms=" + $('#cbsms').attr('checked') + "&mess=" + note + "&price=" + $("#price").val();
        var discountamount = $("#discoutamountapply").val();
        if (discountamount > 0) {
            param += "&discountId=" + $("#discoutidapply").val();
        }

        $('#lightBox').css({ 'opacity': '0.6' }).fadeIn(200);
        commonPostAjax("CompanyProfile", "BookAppointment", param, bookAppointmentSuccess, bookAppointmentError);

    }


    function cashPayment() {
        var mess = $('#txtCustomerScheduleLog').val();
        if (mess.length > 1000) {
            alert("note must be less than 1000 characters.");
            return false;
        }
        var note = $('#txtCustomerScheduleLog').val();
        if (note == 'e.g. i have lower back pain')
            note = '';

        var param = "email=" + $('#cbemail').attr('checked') + "&sms=" + $('#cbsms').attr('checked') + "&mess=" + note + "&price=" + $("#price").val();

        var discountamount = $("#discoutamountapply").val();
        if (discountamount > 0) {
            param += "&discountId=" + $("#discoutidapply").val();
        }
        $("#newpayment").hide();
        $('#lightBox').css({ 'opacity': '0.15', 'z-index': '10000' }).fadeIn(200);
        $('#imgLoadermain').show();
        commonPostAjax("CompanyProfile", "BookAppointmentCash", param, cashAppointmentSuccess, bookAppointmentError);

    }

    function cashAppointmentSuccess(result) {
        $('#lightBox').fadeOut(200);
        $('#imgLoadermain').hide();
        if (result == 'true') {
            window.location.href = "/Appointment";
        }
        else {
            window.location.href = "/CompanyProfile/availability/" + $("#profileid").val();
        }

    }

    function bookByPackage(packageId) {
        var note = $('#txtCustomerScheduleLog').val();
        if (note == 'e.g. i have lower back pain')
            note = '';
        var param = "email=" + $('#cbemail').attr('checked') + "&sms=" + $('#cbsms').attr('checked') + "&mess=" + note + "&price=" + $("#price").val() + "&packageId=" + packageId;
        commonPostAjax("CompanyProfile", "BookAppointmentByPackage", param, bookAppointmentPackageSuccess, bookAppointmentError);
    }


    function bookAppointmentSuccess(result) {
        $('#newpayment').hide();
        $('#comfirmationpopup').hide();
        window.location.href = "/Appointment/Preapprove";
    }

    function bookAppointmentPackageSuccess(result) {
        $('#newpayment').hide();
        $('#comfirmationpopup').hide();
        if (result == 'true') {
            window.location.href = "/Appointment";
        } else {
            window.location.href = "/CompanyProfile/availability/" + $("#profileid").val();
        }
    }

    function bookAppointmentError() {
        $('#newpayment').hide();
        $('#comfirmationpopup').hide();
    }

    var calendarColor = "FBB03B";
    function fillterByCalendar(calendarId) {       
        
        $('#calendar').fullCalendar('removeEvents');
        var classname = $('.personcalendar #'+calendarId).attr("class");
        var corlor =$('.personcalendar #'+calendarId).attr("color");       
        if (classname != "activeperson") {          
            $('.personcalendar a').removeClass("activeperson");
            $('.personcalendar #' + calendarId).addClass("activeperson");
            $('.personcalendar a').css({ "background-color": "#FFFFFF" });
            $('.personcalendar #' + calendarId).css({'background-color':"#"+corlor});
            var url = "/CompanyProfile/GetCalendars/?employeeId=" + $("#message option:selected").val() + "&calendarId=" + calendarId;
            calendarColor = corlor;
        } else {
            $('.personcalendar a').removeClass("activeperson");
            $('.personcalendar a').css({ "background-color": "#FFFFFF" });
            var url = "/CompanyProfile/GetCalendars/?employeeId=" + $("#message option:selected").val() + "&calendarId=" + 0;
            calendarColor = "FBB03B";
        }
        $('#calendar').fullCalendar('addEventSource', url);
        
    }

    function applyDiscount() {

        var profileId = $("#profileid").val();
        var code = $.trim($("#txtdiscount").val());
        var selected = document.getElementById('servicetype');
        var seviceId = selected.options[selected.selectedIndex].value;
        $("#applycodeError").html("&nbsp;");
        if (code == "")
            return;
        $.get("/CompanyProfile/GetDiscountCode?code=" + code + "&serviceId=" + seviceId + "&profileId=" + profileId + "&nocache=" + getunixtime(), function (result) {
            var totaldue = $("#price").val();
            var discountamount = 0;
            if (result != null) {
                var price = $("#price").val();
                if (result.amount > 0) {
                    discountamount = result.amount;

                } else {
                    discountamount = price * result.percent / 100;
                }
                totaldue = price - discountamount;
                if (totaldue < 0)
                    totaldue = 0;
                var totalduetext = "<span>total due:</span><br> $" + parseFloat(totaldue).toFixed(2);
                $(".totaldue").html(totalduetext);
                $("#discoutamountapply").val(discountamount);
                $("#totaldue").val(totaldue);
                $("#discoutidapply").val(result.discountid);

            } else {
                var totalduetext = "<span>total due:</span><br> $" + parseFloat(totaldue).toFixed(2);
                $("#discoutamountapply").val(0);
                $(".totaldue").html(totalduetext);
                $("#totaldue").val(totaldue);
                $("#discoutidapply").val(0);
                $("#applycodeError").html("invalid discount code or expired");
            }

        });
       
    }
