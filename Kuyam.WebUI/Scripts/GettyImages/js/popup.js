// JavaScript Document
var kalturaId = "";
var modeUpload = 0;
var day = "";
//Add categories
var arrayTags = [""];
var index = 0;
var step = 5;
var current = 0;
var visible = 4;
var speed = 300;
var liSize = 96;
var carousel_height = 80;
var divSize = liSize * visible;
var maximum1 = 0;
var ulSize1 = 0;

var maximum = 0;
var ulSize = 0;
var itemStartNumber = 1;
$(document).ready(function () {
    //search from getty 
    $('.carousel_btnnext1').live("click", function () {
        if (current + step < 0 || current + step > maximum1) { return true; }
        else {
            current = current + step;
            $('#my_carousel1 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        // alert(maximum1);
        if (current == maximum1) {
            var startnumber = itemStartNumber + 50;
            LoadGettyImages(startnumber);
        }
        return false;
    });

    $('.carousel_btnprev1').live("click", function () {
        if (current - step < 0 || current - step > maximum1) {
            if (itemStartNumber > 50) {
                var startnumber = itemStartNumber - 50;
                LoadGettyImages(startnumber);
            }
            return true;
        }
        else {
            current = current - step;
            $('#my_carousel1 ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }

        return false;
    });

    $('.carousel-wrapper-thumbs a').live("click", function (e) {
        e.preventDefault();
        var img = $(this).find('img');
        $('.carousel-wrapper-thumbs .carousel-thumbs li').removeClass('ad-active');
        $(this).parents('li').addClass('ad-active');
        $('#carousel_viewimage1').html($('<img>').attr({ 'src': this.href, 'title': img.attr('title') }).fadeIn(1000));
        $('#carousel_viewimage1').append('<div class="caption"><a class="btnuploadimage1" onclick="UploadImageToKaltura();"></a><span class="imgtitle">' + img.attr('title') + '</span><span class="imgalt">' + img.attr('alt') + '</span></div>');
    });
    //end

    //load in database
    $('.carousel_btnnext').live("click", function () {
        if (current + step < 0 || current + step > maximum) { return true; }
        else {
            current = current + step;
            $('#my_carousel ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    $('.carousel_btnprev').live("click", function () {
        if (current - step < 0 || current - step > maximum) { return true; }
        else {
            current = current - step;
            $('#my_carousel ul').animate({ left: -((liSize + 12) * current) }, speed, null);
        }
        return false;
    });

    $('#my_carousel .carousel-thumbs a').live("click", function (e) {
        e.preventDefault();
        // Active highlight thumbnail
        $('#my_carousel .carousel-thumbs li').removeClass('ad-active');
        $(this).parents('li').addClass('ad-active');
        kalturaId = $(this).attr("kalturaId");
        var id = $(this).attr("mediaId");
        $("#hdmedia").val(id);
        $('#carousel_viewimage').html($('<span></span><img>').attr({ src: this.href }).fadeIn(1000));
        $('#carousel_viewimage').append('<div class="caption"></div>');
    });


    //end

    $("#search").bind("keypress", function(event) {
        if (event.which == 13) {
            $("#btnSearch").click();
        }
    });
});


//Load all getty images by API
function LoadGettyImages(pageIndex) {
   
    var key = $("#search").val();
    if (key == '' || key == null || key == 'search gettyimages for more...') {
        key = "";
        return false;
    }

    current = 0;
    ShowLoanding();
    
    var data = { key: key, pageIndex: pageIndex };

    $.ajax({
        url: "/Media/LoadGetttyImages",
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        //beforeSend: onAjaxBeforeSend,
        success: function (msg) {

            itemStartNumber = msg.ItemStartNumber;
            $('#GettyImagesSearch').setTemplateURL('/Media/GetTemplate/gettyimagessearch.htm', null, { filter_data: false });
            $('#GettyImagesSearch').processTemplate(msg);

            /************* CAROUSEL 2 ****************/
            maximum1 = $('#my_carousel1 ul li').size();
            ulSize1 = (liSize + 15) * maximum1;

            $('#my_carousel1 ul').css("width", ulSize1 + "px").css("left", -(current * liSize)).css("position", "absolute");

            $('#my_carousel1').css("width", divSize + "px").css("height", carousel_height + "px").css("visibility", "visible").css("overflow", "hidden").css("position", "relative");

            //Auto load the first images

            $('#my_carousel1 .carousel-thumbs li:first').addClass('ad-active');
            var imglink = $('#my_carousel1 .carousel-thumbs li:first').find('a');
            var imgattr = $('#my_carousel1 .carousel-thumbs li:first').find('img');
            $('#carousel_viewimage1').html($('<span></span><img src = "' + imglink.attr('href') + '" title="' + imgattr.attr('title') + '">'));
            $('#carousel_viewimage1').append('<div class="caption"><a class="btnuploadimage1" onclick="UploadImageToKaltura();"></a><span class="imgtitle">' + imgattr.attr('title') + '</span><span class="imgalt">' + imgattr.attr('alt') + '</span></div>');

            /************* END CAROUSEL 2 ****************/
            HideLoanding();
        }
    });
    return false;
}


//Load all getty images client
function LoadGettyImagesClient(isBackgroundRequest) {
    current = 0;
    if (isBackgroundRequest == undefined || isBackgroundRequest == false)
        ShowLoanding();
    var custId = $("#search").val();
    if (custId == '' || custId == null || custId == 'search gettyimages for more...') {
        custId = "1";
    }
    var data = { custId: custId };

    $.ajax({
        url: "/Media/LoadGetttyImagesClient",
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        //beforeSend: onAjaxBeforeSend,
        success: function (msg) {

            $('#GettyImagesClient').setTemplateURL('/Media/GetTemplate/gettyimagesclient.htm', null, { filter_data: false });
            $('#GettyImagesClient').processTemplate(msg);

            /************* CAROUSEL 1 ****************/
            maximum = $('#my_carousel ul li').size();
            ulSize = (liSize + 15) * maximum;

            $('#my_carousel ul').css("width", ulSize + "px").css("left", -(current * liSize)).css("position", "absolute");
            $('#my_carousel').css("width", divSize + "px").css("height", carousel_height + "px").css("visibility", "visible").css("overflow", "hidden").css("position", "relative");

            //Auto load the first images
            $('#my_carousel .carousel-thumbs li:first').addClass('ad-active');
            var imglink = $('#my_carousel .carousel-thumbs li:first').find('a');
            $('#carousel_viewimage').html($('<span></span><img src = "' + imglink.attr('href') + '">'));
            kalturaId = $(imglink).attr("kalturaId");
            var id = $(imglink).attr("mediaId");
            $("#hdmedia").val(id);
            /************* END CAROUSEL 1 ****************/

            if (isBackgroundRequest == undefined || isBackgroundRequest == false)
            HideLoanding();
        }

    });

    return false;
}

function UploadImageToKaltura() {

    var flag = true;
    var urlPreview = $('#my_carousel1 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('src');
    var gettyImageId = $('#my_carousel1 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('class');
    if (gettyImageId == "image0") {
        return false;
    }
    var title = $('#my_carousel1 .carousel-nav .carousel-thumbs li.ad-active .center-thumb img').attr('title');

    $("#my_carousel .carousel-nav .carousel-thumbs li .center-thumb img").each(function (index, domEle) {
        var gettyId = $('#my_carousel .carousel-nav .carousel-thumbs li .center-thumb img').eq(index).attr('class');
        if (gettyId != undefined && gettyId == gettyImageId) {
            alert("This image is existing in photo gallery.");
            flag = false;
            return false;
        }
    });

    if (flag) {
        ShowLoanding();
        var data = { urlPreview: urlPreview, gettyImageId: gettyImageId , title:title};
        $.ajax({
            url: "/Media/UploadImageToKaltura",
            data: JSON.stringify(data),
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            //beforeSend: onAjaxBeforeSend,
            success: function () {
                LoadGettyImagesClient();
                HideLoanding();
            }
        });
    }

    return false;
}

function ShowLoanding() {
    if ($('#lightBox').is(':visible')) {
        $('#lightBox').css('z-index', '10001');
        $('#ajaxBusy').show();
    }
    else {
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#lightBox').css('z-index', '9998');
        hideLoading();
    }
}

function HideLoanding() {
    if ($('#lightBox').css('z-index')== '10001') {
        $('#lightBox').css('z-index', '9998');
    } else {
        $('#lightBox').css('opacity', '0.3').fadeOut(400);
    };
    hideLoading();
}

function hideLoading() {
    $('#ajaxBusy').hide();
}

