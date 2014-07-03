//Script for Company setup edit page
var day = "";
var isday = "";
var step = 5;
var current = 0;
var visible = 4;
var speed = 300;
var liSize = 96;
var carousel_height = 80;
var divSize = liSize * visible;
var maximum = 0;
var ulSize = 0;
var maximum1 = 0;
var ulSize1 = 0;
var maximum2 = 0;
var ulSize2 = 0;
var startNumber = 1;
$(document).ready(function () {
    ///New
    $('#fileuploadlogo').fileupload();
    $('#fileuploadlogo').fileupload('option', {
        maxFileSize: 3000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#logocontentfile",
        callbackStart: showloandinglogoerror,
        send: showloandinglogo,
        callbackSuccess: hideloandinglogo
    });

    $('#fileuploadphoto').fileupload();
    $('#fileuploadphoto').fileupload('option', {
        maxFileSize: 3000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#photocontentfile",
        downloadTemplateId: 'company-photo-template-download',
        callbackStart: showloandingerror,
        send: showloandingphoto,
        callbackSuccess: hideloandingphoto
    });
    //search getty image
    $("#search").keypress(function (e) {
        kCode = e.keyCode || e.charCode;
        if (kCode == 13) {
            getGettyImages(1);
            return false;
        }
    });

    $('#defaultInput').click(function () {
        var id = $('#my_carousel .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');
        var chkisdefault = $('#chkisdefault_' + id).val();
        if (chkisdefault == 'True') {
            $('#chkisdefault_' + id).val("False");
        } else {
            $("#my_carousel .carousel-thumbs ul li").each(function (index, domEle) {
                var imgactive = $(this).find('img');
                id = imgactive.attr('id');
                $('#chkisdefault_' + id).val("False");
            });
            $('#chkisdefault_' + id).val("True");
        }
    });

    $('#hiddenInput').click(function () {
        var id = $('#my_carousel .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');
        var chkishidden = $('#chkishidden_' + id).val();
        if (chkishidden == 'True') {
            $('#chkishidden_' + id).val("False");
        } else {
            $('#chkishidden_' + id).val("True");
        }
    });
    $('#btndelete').click(function () {
        hideDialog("selectservice");
        var imageId = $('#my_carousel .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');
        mediaDelete(imageId);
    });
    $('#bookme').click(function () {
        hideDialog("selectservice");
    });
    $('#btnok').click(function () {
        hideDialog("savechangepopup");
    });
    $('#text').checkBox({
        addVisualElement: false,
        'change': function (e, ui) {
            if (ui.checked == true) {
                $(".companynumber").show();
            } else {
                $(".companynumber").hide();
            }
        }
    });

    $('#call').checkBox({ addVisualElement: false });
    $('#mail').checkBox({ addVisualElement: false });
    $('.photoimg a').lightBox({
        fixedNavigation: true,
        captionText: false
    });

    $('#btnsavechange').click(function () {
        location.href = "#";
    });
    ///End New

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
                } else {
                    $('#btndailyadd').css("color", "#a6a6a6");
                }
            } else {
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
        if ($('#cbAvailable').prop('checked')) { //day == "mon - sun") {
            newArr.push({
                day: convertDayToInt($.trim($(this).text())),
                dayStr: "isdaily",
                from: from,
                to: to
            });
        } else {
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
                } else {
                    $('#btndailyadd').css("color", "#a6a6a6");
                }
            }
        });
        $('#hourto').timepicker({
            onClose: function (dateText, inst) {
                if (checkhoursvalid(day)) {
                    $('#btndailyadd').css("color", "#333333");
                } else {
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
        if (gettime(a.from) >= gettime(b.from) && gettime(a.from) < gettime(b.to)) {
            return true;
        }
        if (gettime(b.from) >= gettime(a.from) && gettime(b.from) < gettime(a.to)) {
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
        } else if ((tmpArr[1].substring(2) == "pm") && (parseFloat(tmpArr[0]) == 12) && tmpMinute == 0) {
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

        } else {
            $(this).addClass('active');
        }
        if (checkhoursvalid(day)) {
            $('#btndailyadd').css("color", "#333333");
        } else {
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
        } else {
            $(this).addClass('choose');
        }
    });


    /************* Gallery Slider ****************/

    $('.carousel_btnnext').click(function () {

        if (current + step < 0 || current + step >= maximum) {
            return;
        } else {
            current = current + step;
            $('#my_carousel ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    $('.carousel_btnprev').click(function () {
        if (current - step < 0 || current - step > maximum - visible) {
            return;
        } else {
            current = current - step;
            $('#my_carousel ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    //Auto load the first images
    var imglink = $('#my_carousel .carousel-thumbs li:first').find('a');
    $('#carousel_viewimage').html($('<span></span><img src = "' + imglink.attr('href') + '">'));
    imglink.parents('li').addClass('ad-active');

    var imgactive = $('#my_carousel .carousel-thumbs li:first a').find('img');
    var imgactiveid = imgactive.attr('id');
    setcheckbox(imgactiveid);

    $('#my_carousel .carousel-thumbs a').click(function (e) {
        e.preventDefault();
        var img = $(this).find('img');
        //            $('#my_carousel .carousel-thumbs a').removeClass('ad-active');
        //            $(this).addClass('ad-active');
        $('#my_carousel .carousel-thumbs li').removeClass('ad-active');
        $(this).parents('li').addClass('ad-active');

        $('#carousel_viewimage').html($('<span></span><img>').attr({ src: this.href }).fadeIn(1000));
        $('#carousel_viewimage').append('<div class="caption"></div>');

        //Set active checkbox: Default and Hidden
        var id = img.attr('id');
        setcheckbox(id);
    });

    $('.carousel_btnnext1').click(function () {
        if (current + step < 0 || current + step > maximum1) {
            return;
        } else {
            current = current + step;
            $('#my_carousel1 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
       var count = Math.floor(maximum1 / 5)*5;
        if (current == maximum1 || count == current) {
            var index = startNumber + 50;
            getGettyImages(index);
        }
        return false;
    });

    $('.carousel_btnprev1').click(function () {
        if (current - step < 0 || current - step > maximum1) {
            if (startNumber > 50) {
                var index = startNumber - 50;
                getGettyImages(index);
            }
            return;
        } else {
            current = current - step;
            $('#my_carousel1 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });


    $('.carousel_btnnext2').click(function () {
        if (current + step < 0 || current + step > maximum2) {
            return;
        } else {
            current = current + step;
            $('#my_carousel2 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    $('.carousel_btnprev2').click(function () {
        if (current - step < 0 || current - step > maximum2) {
            return;
        } else {
            current = current - step;
            $('#my_carousel2 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    /************* End Gallery Slider ****************/

    /************* BUTTON SCROLL TO ****************/
    $('.btnpurchasemore a').click(function (ev) {
        ev.preventDefault();
        $('html, body').stop().animate({ scrollTop: 900 }, 1000);
    });
    /************* END BUTTON SCROLL TO ****************/

    /************* SHOW POPUP ****************/
    $('.deletepopup').click(function () {
        showpopup("selectservice");
    });
    /************* END SHOW POPUP ****************/

});

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

//---------------------------New------------------------------------

//function insertGettyImage() {
//    debugger;
//    var imageId = $('#my_carousel1 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');
//    var companyId = $('#companyID').val();
//    var searchParameter = { imageId: imageId, companyId: companyId };
//    var total = $("#hdftolal").val();
//    if (total >= 10) {
//        alert("max is 10 images !");
//        return;
//    } else {
//        $('#aptImgLoader').show();
//        $('#lightBox').css('opacity', '0.3').fadeIn(400);
//        $.ajax(
//            {
//                type: 'POST',
//                contentType: 'application/json; charset=utf-8',
//                data: JSON.stringify(searchParameter),
//                dataType: 'json',
//                url: '/CompanySetup/InsertGettyImage/'
//            })
//            .success(function (result) {
//                if (result == true || result == "true") {
//                    location.reload();
//                } else {
//                    alert('insert image error');
//                    return false;
//                }
//            })
//            .error(function (error) {
//                alert('insert image error');
//            });
//        }
//    }
function insertGettyImage() {

    var imageId = $('#my_carousel1 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');

    var companyId = $('#companyID').val();
    var tags = $('#hdftags_' + imageId).val();
    var heigth = $('#hdfheight_' + imageId).val();
    var width = $('#hdfwidth_' + imageId).val();
    var title = $('#hdftitle_' + imageId).val();
    var preview = $('#hdfpreview_' + imageId).val();
    var thumb = $('#hdfthumb_' + imageId).val();

    var image = {
        GettyImageId: imageId,
        ProfileId: companyId,
        Title: title,
        UrlThumb: thumb,
        UrlPreview: preview,
        PixelHeight: heigth,
        PixelWidth: width,
        Tags: tags
    };

    var searchParameter = { image: image };
    var total = $("#hdftolal").val();
    if (total >= 10) {
        alert("max is 10 images !");
        return;
    } else {
        $('#aptImgLoader').show();
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $.ajax(
            {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(searchParameter),
                dataType: 'html',
                url: '/CompanySetup/InsertGettyImage/'
            })
            .success(function (result) {
                $('#divgettyimagecart').html(result);
                //                $('#aptImgLoader').hide();
                //                $('#lightBox').css('opacity', '0.3').fadeOut(400);
                //                $('#lightBox').hide();
                //                $('#colorblack').hide();
                location.reload();
                return false;
            })
            .error(function (error) {
                alert('insert image error');
            });
    }
}


function deleteGettyImage(imageId) {
    var searchParameter = { imageId: imageId };
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    $.ajax(
            {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(searchParameter),
                dataType: 'json',
                url: '/CompanySetup/DeleteGettyImage/'
            })
            .success(function (result) {
                if (result == true || result == "true") {
                    location.reload();
                } else {
                    alert('delete error');
                }

            })
            .error(function (error) {
                alert('delete error');
                console.log(error);
            });
}

function getGettyImages(index) {
    startNumber = index;
    var key = $('#search').val();
    var searchParameter = { key: key, itemStartNumber: index };
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    current = 0;
    $.ajax({
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(searchParameter),
        dataType: 'html',
        url: '/CompanySetup/GetGettyImagesByKey/'
    })
            .success(function (result) {
                $('#aptImgLoader').hide();
                $('#lightBox').css('opacity', '0.3').fadeOut(400);

                $('#divgettyimage').html(result);

                maximum1 = $('#my_carousel1 ul li').size();
                ulSize = (liSize + 15) * maximum1;

                $('#my_carousel1 ul').css("width", ulSize + "px").css("left", -(current * liSize)).css("position", "absolute");

                $('#my_carousel1').css("width", divSize + "px").css("height", carousel_height + "px").css("visibility", "visible").css("overflow", "hidden").css("position", "relative");

                //Auto load the first images
                var imglink = $('#my_carousel1 .carousel-thumbs li:first').find('a');
                var imgattr = $('#my_carousel1 .carousel-thumbs li:first').find('img');
                $('#carousel_viewimage1').html($('<span></span><img src = "' + imglink.attr('href') + '">'));

                if (true) {
                    $('#carousel_viewimage1').append('<div class="caption"><a class="btnuploadimagenocheck" href="javascript:void(0);" onclick="insertGettyImage();"></a><span class="imgtitle">' + imgattr.attr('title') + '</span><span class="imgalt">' + imgattr.attr('alt') + '</span></div>');
                }
                else {
                    $("#colorblack").addClass('displaynone');
                    $('#carousel_viewimage1').append('<div class="caption displaynone"><a class="btnuploadimagenocheck" href="javascript:void(0);" onclick="insertGettyImage();"></a><span class="imgtitle">' + imgattr.attr('title') + '</span><span class="imgalt">' + imgattr.attr('alt') + '</span></div>');
                }
                imglink.parents('li').addClass('ad-active');

                $('#colorblack').hide();
            })
            .error(function (error) {
            });
};

function deletecartitem() {
    var imageId = $('#my_carousel2 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('id');
    deleteGettyImage(imageId);
}

// Delete media info
function mediaDelete(id) {
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    var searchParameters = { id: id };
    $.ajax(
            {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(searchParameters),
                dataType: 'json',
                url: '/Company/DeleteMediaById'
            })
            .success(function (result) {
                if (result == true || result == 'true') {
                    location.reload();
                    //                    $('#aptImgLoader').hide();
                    //                    $('#lightBox').css('opacity', '0.3').fadeOut(400);
                    //                    window.location.href = '/CompanySetup/Image';
                } else {
                    alert("Delete is error");
                }
            })
            .error(function (error) {

            });
}

function showloandingerror() {
    $(".companysetup .error").remove();
    //    $('#aptImgLoader').show();
    //    $('#lightBox').css('opacity', '0.3').fadeIn(400);

}

function showloandinglogoerror() {
    $(".companysetup .error").remove();

}
function showloandinglogo() {
    $(".companysetup .error").remove();
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);

}

function hideloandinglogo() {
    location.reload();
    $('#aptImgLoader').hide();
    $('#lightBox').css('opacity', '0.3').fadeOut(400);
}

function showloandingphoto() {
    $(".companysetup .error").remove();
    var total = $("#hdftolal").val();
    if (total >= 10) {
        alert("max is 10 images !");
        return false;
    } else {
        $('#aptImgLoader').show();
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        //    $("#imgLoaderphoto").show();
    }
}
function hideloandingphoto() {
    location.reload();
    $("#imgLoaderphoto").hide();
}

function savechange() {

    var images = [];
    $("#my_carousel .carousel-thumbs ul li").each(function (index, domEle) {

        var imgactive = $(this).find('img');
        var id = imgactive.attr('id');

        var chkisdefault = $('#chkisdefault_' + id).val();
        var chkishidden = $('#chkishidden_' + id).val();

        var image = {
            Id: id,
            Default: chkisdefault,
            Hidden: chkishidden
        }
        images.push(image);
    });
    var searchParameter = { images: images };
    $('#aptImgLoader').show();
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    $.ajax(
            {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(searchParameter),
                dataType: 'json',
                url: '/CompanySetup/SaveCompanyImages/'
            })
            .success(function (result) {
                if (result == true || result == 'true') {
                    $('#aptImgLoader').hide();
                    showpopup("savechangepopup");

                    //                    $('#lightBox').css('opacity', '0.3').fadeOut(400);
                    $('#divgettyimage').html(result);
                    $('#colorblack').hide();
                } else {
                    alert("error");
                }

            })
            .error(function (error) {
            });
};
//------------------------------------------------------------------

function setcheckbox(id) {
    var chkisdefault = $('#chkisdefault_' + id).val();
    var chkishidden = $('#chkishidden_' + id).val();
    if (chkisdefault == 'True') {
        //            $('#lbldefault').addClass('ui-checkbox-state-checked ui-checkbox-checked');
        $('#defaultInput').attr({ checked: "checked" });
    }
    else {
        //            $('#lbldefault').removeClass('ui-checkbox-state-checked ui-checkbox-checked');
        $('#defaultInput').removeAttr('checked');
    }
    if (chkishidden == 'True') {
        $('#hiddenInput').attr({ checked: "checked" });
        //            $('#lblhide').addClass('ui-checkbox-state-checked ui-checkbox-checked');
    } else {
        //            $('#lblhide').removeClass('ui-checkbox-state-checked ui-checkbox-checked');
        $('#hiddenInput').removeAttr('checked');
    }
}

