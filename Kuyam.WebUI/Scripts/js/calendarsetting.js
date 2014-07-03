$(document).ready(function () {
    $('#btnfacebook').click(function () {
        location.href = "/CalendarSetting/FacebookInfoConnService";
    });
    $('#btngoogle').click(function () {
        location.href = "/CalendarSetting/GoogleInfoConnService";
    });

    $.fn.fileName = function () {
        var $this = $(this),
        $val = $this.val(),
        valArray = $val.split('\\'),
        newVal = valArray[valArray.length - 1];
        if (newVal !== '') {
            $("#txtFileName").val(newVal);
        }
    };

    $(function () {
        $('input[type=file]').bind('change focus click', function () { $(this).fileName() });
    });
    createtopscroll(0, $('.footer').position().top - $('.header').height());
    $('.chkcsyncalendar input[type="radio"]').checkBox();
    $("#ulcalendarlistcontent").niceScroll("#ulcalendarlistcontent1", { cursorborder: "", cursoropacitymin: 1, cursorcolor: "#4d4d4d", boxzoom: false, cursorwidth: 14, cursorborderradius: 0 }).resize();

    $("#btnAdd").click(function () {
        $('#txtCalendarName').val("");
        $('#saveCalendar').text("add calendar");
        $('#saveCalendar').attr('title', "add calendar");
        $('#spanTitle').html("<strong>add calendar</strong>");
        showpopup("addcalendarpopup");
    });

    $("#btnSubmitDelete").click(function () {
        var id = $('.chkcsyncalendar').find(':radio').filter(':checked').attr('value');
        var pass = $('#txtpassword').val();
        var isError = true;
        if (isFacebookAccount == false && pass == '') {
            $("#passerror").html("please input password.");
            isError = false;
        } else {
            $("#passerror").html("");
        }
        if (isError) {
            var eparameter = { id: id, pass: pass };
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(eparameter),
                dataType: 'html',
                url: '/CalendarSetting/DeleteCalendar/'
            })
            .success(function (result) {
                if (result == 'true') {
                    $('#loginpopup').fadeOut(400);
                    $('#lightBox').fadeOut(400);
                    window.location.href = '/CalendarSetting';
                } else if (result == 2) {
                    $("#passerror").html("wrong password. please try again");
                } else {
                    alert('can not delete this calendar');
                }
            })
            .error(function (error) {
                alert('connact data error');
            });
        }
    });
    $("#btnEdit").click(function () {
        $('#saveCalendar').attr('title', "save changes");
        if ($('.chkcsyncalendar').find(':radio').filter(':checked').length == 0) {
            alert('select a calendar to edit');
            return false;
        } else {
            var id = $('.chkcsyncalendar').find(':radio').filter(':checked').attr('value');
            $.get("/CalendarSetting/GetCalendar", { id: id }, function (response) {

                $('#saveCalendar').text("save changes");
                $('#txtCalendarName').val(response.calName);
                $("#currentcolor").css("background-color", "#" + response.calBackColor);
                $("#colorname").html(response.calBackColorName);

                $('#spanTitle').html("<strong>edit</strong>" + " &#8220;" + response.calName + "&#8221; " + "<strong>calendar</strong>");
                showpopup("addcalendarpopup");
            });
        }
    });
    $('#btnDelete').click(function (e) {
        $('#txtpassword').val("");
        $("#passerror").html("");

        if ($('.chkcsyncalendar').find(':radio').filter(':checked').length == 0) {
            alert('select a calendar to delete');
            return false;
        } else {
            var id = $('.chkcsyncalendar').find(':radio').filter(':checked').attr('value');
            $.get("/CalendarSetting/GetCalendar", { id: id }, function (response) {

                $('#spanDeleteTitle').html("&#8220;" + response.calName + "&#8221; ");
                if (isFacebookAccount)
                    $("#divPassword").hide();
                else
                    $("#divPassword").show();
                
                showpopup("deletecalendarpopup");
            });
        }
        $('#btnDeleteCalendarCancel').click(function (n) {
            $('#deletecalendarpopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
            n.preventDefault();
        })
        e.preventDefault();
    });
    $("#saveCalendar").click(function () {
        $('#divError').empty();
        var name = $.trim($('#txtCalendarName').val());
        var backColor = $.trim($('#colorname').text());

        if (typeof name == 'undefined' || name == '') {
            alert("calendar name is required field");
            return false;
        }

        if ($('#saveCalendar').text() != 'save changes') {
            var parameters = { name: name, backColor: backColor };
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(parameters),
                dataType: 'json',
                url: '/CalendarSetting/AddCalendar/'
            })
                .success(function (result) {
                    if (result == true) {
                        window.location.href = '/CalendarSetting';
                    } else {
                        alert(result.message);
                    }
                })
                .error(function (error) {
                    alert("error");
                });
        }
        else {

            var id = $('.chkcsyncalendar').find(':radio').filter(':checked').attr('value');
            var eparameter = { id: id, name: name, backColor: backColor };
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(eparameter),
                dataType: 'json',
                url: '/CalendarSetting/UpdateCalendar/'
            })
            .success(function (result) {
                if (result == true) {
                    $('#loginpopup').fadeOut(400);
                    $('#lightBox').fadeOut(400);
                    window.location.href = '/CalendarSetting';
                } else {
                    alert(result.message);
                }
            })
            .error(function (error) {
                alert('get data error');
            });
        }
    });
});
function saveiCalendar() {
    var selectedFile = $('#UploadUrl').val();
    var matches = selectedFile.match(/\.(ics?)$/i);
    if (matches == null) {
        alert('please select an iCal file');
        return false;
    }
    else {
        $("#file_upload").submit();
        return true;
    }
}

function saveiCal(uploadUrl) {
    alert(uploadUrl);

    var eparameter = { uploadUrl: uploadUrl};
    $.ajax({
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(eparameter),
        dataType: 'html',
        url: '/CalendarSetting/ICalInfoConnService/'
    })
    .success(function (result) {
        if (result == 'true') {
            $('#loginpopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
            window.location.href = '/CalendarSetting';
        } else {
            alert('get data error');
        }
    })
    .error(function (error) {
        alert('get data error');
    });
};