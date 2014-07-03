//Script for Company setup edit page
var day = "";
var isday = "";
$(document).ready(function () {
    
    var dayclicked = false;
    $('#cbAvailable').checkBox({
        addVisualElement: true,
        'change': function (e, ui) {
            if (ui.checked == true) {
                day = "mon - sun";
                isday = "isdaily";
                $("#dayofweek a").addClass('active');
                dayclicked = false;
                if (checkhoursvalid(day)) {
                    $('#btndailyadd').css("color", "#333333");
                }
                else {
                    $('#btndailyadd').css("color", "#a6a6a6");
                }
            }
            else {
                day = "";
                isday = "";
                if (!dayclicked) {
                    $("#dayofweek a").removeClass('active');
                }
            }
        }
    });
    $('#btndailycancel').click(function () {
        $("#hourfrom").val("");
        $("#hourto").val("");
        $('#dayofweek a').removeClass('active');
        $('#cbAvailable').checkBox('changeCheckStatus', false);
        $('#addhourspopup').fadeOut(400);
        $('#btndailyadd').css("color", "#A6A6A6");
        var deviceAgent = navigator.userAgent.toLowerCase();
        var agentID = deviceAgent.match(/(iphone|ipod|android|ipad)/);
        if (agentID) {
            $('#hourfrom').scroller("destroy");
            $('#hourto').scroller("destroy");
        } else {
            $('#hourfrom').timepicker("destroy");
            $('#hourto').timepicker("destroy");
        }
    });
    $('#btndailyadd').click(function () {
        if (day.length == 0) {
            if ($("#dayofweek a.active").text().length == 0) {
                return false;
            }
        }
        if ($("#hourfrom").val().length == 0 || $("#hourto").val().length == 0) {
            return false;
        }

        if ($("#hourfrom").val() == $("#hourto").val()) {
            return false;
        }

        var StartTime = new Date("01/01/2000 " + $('#hourfrom').val());
        var EndTime = new Date("01/01/2000 " + $('#hourto').val());
        if (StartTime > EndTime) {
            return false;
        }
        var dayofweekarray = new Array();
        var totalactive = $('#dayofweek a.active').length;
        var dayStr;
        var from = $("#hourfrom").val().replace(" ", "");
        var to = $("#hourto").val().replace(" ", "");

        if (from.split(':')[0].substring(0, 1) == '0') {
            from = from.substr(1, from.length);
        }
        if (to.split(':')[0].substring(0, 1) == '0') {
            to = to.substr(1, to.length);
        }

        $(".editaddhours span").each(function (index, domEle) {
            var value = $(".editaddhours span").eq(index).attr("id");
            if (value != undefined) {
                var flag = value.split(',');
                var addedDayValue = convertDayToInt(flag[0]);
                dayofweekarray.push({
                    day: addedDayValue,
                    dayStr: flag[0],
                    from: flag[1],
                    to: flag[2]
                });
            }
        });
        var newArr = new Array();
        if ($('#cbAvailable').prop('checked')) {//day == "mon - sun") {
            newArr.push({
                day: convertDayToInt($.trim($(this).text())),
                dayStr: "isdaily",
                from: from,
                to: to
            });
        }
        else {
            var tmpAdded;
            $('#dayofweek a.active').each(function (idx) {
                dayStr = $(this).text();
                tmpAdded = {
                    day: convertDayToInt($.trim($(this).text())),
                    dayStr: dayStr,
                    from: from,
                    to: to
                };

                for (var i = 0; i < dayofweekarray.length; i++) {
                    if (dayofweekarray[i].day == 100) {
                        if (isIntersectWithNoDay(tmpAdded, dayofweekarray[i])) {
                            return false;
                        }
                    }
                }

                newArr.push(tmpAdded);
            });
        }
        var flagIntersect = false;
        for (var i = 0; i < newArr.length; i++) {

            flagIntersect = false;
            for (var j = 0; j < dayofweekarray.length; j++) {
                if (newArr[i].day != dayofweekarray[j].day) {
                    continue;
                }
                if (isIntersect(newArr[i], dayofweekarray[j])) {

                    flagIntersect = true;
                    if (gettime(newArr[i].from) < gettime(dayofweekarray[j].from)) {
                        dayofweekarray[j].from = newArr[i].from;
                    }
                    if (gettime(newArr[i].to) > gettime(dayofweekarray[j].to)) {
                        dayofweekarray[j].to = newArr[i].to;
                    }
                    break;
                }
            }
            if (!flagIntersect) {
                dayofweekarray.push(newArr[i]);
            }
        }

        dayofweekarray.sort(function (a, b) {
            var tmpA, tmpB;
            tmpA = gettime(a.from) + a.day * 24 * 60;
            tmpB = gettime(b.from) + b.day * 24 * 60;
            return tmpA > tmpB ? 1 : -1
        });

        mergerArray(dayofweekarray);

        $('#editaddhours').empty();
        $.each(dayofweekarray, function (idx, value) {
            if (value.dayStr == "isdaily") {
                $('#editaddhours').append("<span id='" + value.dayStr + "," + $.trim(value.from) + "," + $.trim(value.to) + "'  style='float:right;'>" + "mon - sun" + " " + $.trim(value.from) + "-" + $.trim(value.to) + '</span><br />');
            } else {
                $('#editaddhours').append("<span id='" + value.dayStr + "," + value.from + "," + value.to + "'  style='float:right;'>" + value.dayStr + " " + value.from + "-" + value.to + '</span><br />');
            }
        });

        $('#hourfrom').timepicker("destroy");
        $('#hourto').timepicker("destroy");

        $("#hourfrom").val("");
        $("#hourto").val("");
        $('#dayofweek a').removeClass('active');
        $('#cbAvailable').checkBox('changeCheckStatus', false);
        day = "";
        $('#hourfrom').timepicker({
            onClose: function (dateText, inst) {
                if (checkhoursvalid(day)) {
                    $('#btndailyadd').css("color", "#333333");
                }
                else {
                    $('#btndailyadd').css("color", "#a6a6a6");
                }
            }
        });
        $('#hourto').timepicker({
            onClose: function (dateText, inst) {
                if (checkhoursvalid(day)) {
                    $('#btndailyadd').css("color", "#333333");
                }
                else {
                    $('#btndailyadd').css("color", "#a6a6a6");
                }
            }
        });
    });

    function isIntersect(a, b) {
        if (a.day != b.day) {
            return false;
        }
        if (gettime(a.from) >= gettime(b.from) && gettime(a.from) <= gettime(b.to)) {
            return true;
        }
        if (gettime(b.from) >= gettime(a.from) && gettime(b.from) <= gettime(a.to)) {
            return true;
        }
        return false;
    }

    function isIntersectWithNoDay(a, b) {
        if (gettime(a.from) >= gettime(b.from) && gettime(a.from) <= gettime(b.to)) {
            return true;
        }
        if (gettime(b.from) >= gettime(a.from) && gettime(b.from) <= gettime(a.to)) {
            return true;
        }
        return false;
    }

    var indexA, indexB;
    function canMerge(arr) {

        if (arr.length < 2)
            return false;
        for (var i = 0; i < arr.length - 1; i++) {
            for (var j = i + 1; j < arr.length; j++) {
                if (isIntersect(arr[i], arr[j])) {
                    indexA = i;
                    indexB = j;
                    return true;
                }
            }
        }
    }

    function mergerArray(arr) {
        var mergeResult;
        while (canMerge(arr)) {
            mergeResult = merge(arr[indexA], arr[indexB]);
            arr[indexA] = mergeResult;
            arr.splice(indexB, 1);
        }
    }

    function merge(a, b) {
        var res = {
            day: a.day,
            dayStr: a.dayStr
        }
        if (gettime(a.from) < gettime(b.from))
            res.from = a.from;
        else
            res.from = b.from;

        if (gettime(a.to) > gettime(b.to))
            res.to = a.to;
        else
            res.to = b.to;

        return res;
    }

    function gettime(str) {

        var result = 0;
        var tmpArr = str.split(':');
        var tmpAddingHour = tmpArr[1].substring(2) == "am" ? 0 : 12;
        var tmpMinute = parseFloat(tmpArr[1].substring(0, 2));

        if ((tmpArr[1].substring(2) == "am") && (parseFloat(tmpArr[0]) == 12) && tmpMinute == 0) {
            result = 0;
        }
        else if ((tmpArr[1].substring(2) == "pm") && (parseFloat(tmpArr[0]) == 12) && tmpMinute == 0) {
            result = 12;
        } else {
            var tmpHour = parseFloat(tmpArr[0]) % 12;
            result = (tmpAddingHour * 60) + tmpHour * 60 + tmpMinute;
        }
        return result;
    }

    $('#btnreset').click(function () {
        $('#editaddhours').empty();
    });

    $('#dayofweek a').click(function () {

        if ($(this).hasClass("active")) {
            dayclicked = true;
            $('#cbAvailable').checkBox('changeCheckStatus', false);
            $(this).removeClass('active');

        }
        else {
            $(this).addClass('active');
        }
        if (checkhoursvalid(day)) {
            $('#btndailyadd').css("color", "#333333");
        }
        else {
            $('#btndailyadd').css("color", "#a6a6a6");
        }
    });
    $('#deletecategories').click(function () {
        var totalchoose = $(".catresult span.choose").length;
        for (i = 0; i < totalchoose; i++) {
            if ($("#" + $(".catresult span.choose").eq(i).attr("id") + "1").text().length == 0) {
                $(".catresult label").eq(1).remove();
            }

            $("#" + $(".catresult span.choose").eq(i).attr("id") + "1").remove();
        }
        $(".choose").remove();
    });

    //Add categories New
    $('#addcategories').click(function () {
        var textVal = $("#addcategory-button span.ui-selectmenu-status").text(); //.split(' ').join('');

        // user apply new tag
        var isExist = jQuery.inArray(textVal.split(' ').join(''), arrayTags);

        var serviceOption = document.getElementById('addcategory');
        var serviceId = serviceOption.options[serviceOption.selectedIndex].value;
        //alert(serviceId);
        if (isExist == -1) {
            // insert new tag (visible to user)
            $(insertTag(textVal, serviceId)).insertBefore("#newTagInput");

            // insert new tag to js array
            arrayTags[index] = textVal.split(' ').join('');
            index++;
        }
    });
    //End categories
    $('.catresult span').click(function () {
        if ($(this).hasClass("choose")) {
            $(this).removeClass("choose");
        }
        else {
            $(this).addClass('choose');
        }
    });
})

//End script for comapany setup edit page

function checkhoursvalid(day) {
    //check length of day
    if (day.length == 0) {
        if ($("#dayofweek a.active").text().length == 0) {
            return false;
        }
    }
    //check from and to hours have empty!
    if ($("#hourfrom").val().length == 0 || $("#hourto").val().length == 0) {
        return false;
    }
    //check from and to hours have same value
    if ($("#hourfrom").val() == $("#hourto").val()) {
        return false;
    }
    //check from and to hours . If to hours greater than from hours
    var StartTime = new Date("01/01/2000 " + $('#hourfrom').val());
    var EndTime = new Date("01/01/2000 " + $('#hourto').val());
    if (StartTime > EndTime) {
        return false;
    }
    return true;
}
function removeByValue(arr, val) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == val) {
            arr.splice(i, 1);
            break;
        }
    }
    index--;
}

function removeTag(el) {
    tag = $(el).prev().html().split(' ').join('').trim();
    $("#tag-" + tag).remove();
    //console.log(tag.split(' ').join(''));
    removeByValue(arrayTags, tag);
}

function insertTag(tag, serviceId) {
    var liEl = '<li  id="tag-' + tag.split(' ').join('') + '" class="li_tags" ' + ' serviceId="' + serviceId + '" >' +
					'<span href="javascript://" class="a_tag">' + tag + '</span>&nbsp;' +
					'<a href="" onclick="removeTag(this); return false;"' +
					' class="del" id="del_' + tag.split(' ').join('') + '">x</strong></a>' +
					'</li>';
    return liEl;
}

function convertDayToInt(str) {
    switch (str.substring(0, 3)) {
        case "sun":
            return 7;
            break;
        case "mon":
            return 1;
            break;
        case "tue":
            return 2;
            break;
        case "wed":
            return 3;
            break;
        case "thu":
            return 4;
            break;
        case "fri":
            return 5;
            break;
        case "sat":
            return 6;
            break;
        default:
            return 100;
            break;
    }
}
