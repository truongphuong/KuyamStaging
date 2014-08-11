var twicePush = 0;
var twicePushMsg = 'processing';
var activetab = 0;
var kalturaPartnerId = '';
var redirectUrl = '';

function login(username, password) {
    var email = $('#loginform #username').val();
    var pass = $('#loginform #password').val();
    $('#loginform #loginError').hide();
    //alert(email);
    //alert(pass);
    //if (typeof (username) != 'undefined' && typeof (password) != 'undefined') {
    //    email = username;
    //    pass = password;
    //}

    if (email == '' || email == "enter e-mail address" || pass == '') {
        $('#loginform #loginError').show();
        return false;
    }

    var timezoneId = getTimezoneName();
    var loginParameters = { email: email, password: pass, timeZoneId: timezoneId };
    window.isUseDefaultAjaxHandle = true;
    $.ajax(
        {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(loginParameters),
            dataType: 'json',
            url: '/Account/LoginAjax/'
        })
        .success(function (result) {
            if (result.status == "true") {
                hideDialog("loginpopup");
                $.get('/Home/InitAppointmentReview', function (response) {
                    if (response == 'null') {
                        var param = getQueryString();
                        var url = param["returnurl"];
                        var link = "";
                        if (typeof url != 'undefined')
                            link = url;
                        else
                            link = redirectUrl;

                        if (typeof link == 'undefined' || link == null || link == "") {
                            location.reload();
                        } else {

                            window.location.href = link;
                        }
                    } else {
                        $('#popupreviewappt').html(response);
                        showpopup("popupreviewappt");
                    }
                });
            }
            else {
                $('#loginform  #loginError').html("<span style='color: Red;'>" + result.message + "</span>");
                $('#loginform  #loginError').show();
                $('#loginform  #imgLoaderContact1').hide();

            }
        })
        .error(function (error) {

        })
}

function logout() {
    $.ajax(
    {
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        url: '/Account/LogoutAjax/'
    })
    .success(function (result) {
        if (result.IsOriginalPage > 0) {
            window.location.href = result.RedirectUrl;

        } else {
            window.location.href = "/";
        }
    })

    .error(function (error) {
        window.location.reload();
    });
}

function getunixtime() {
    var d = new Date;
    var unixtime_ms = d.getTime();
    var unixtime = parseInt(unixtime_ms / 1000);
    return unixtime;
}

function getScrollBarWidth() {
    document.body.style.overflow = 'hidden';
    var width = document.body.clientWidth;
    document.body.style.overflow = 'scroll';
    width -= document.body.clientWidth;
    if (!width) width = document.body.offsetWidth - document.body.clientWidth;
    document.body.style.overflow = '';
    return width;
}

function getDocHeight() {
    return Math.max(
        $(document).height(),
        $(window).height(),
    /* For opera: */
        document.documentElement.clientHeight
    );
};


function showDialog(dialogid, btnCloseid) {
    var top = ($(window).height() - $('#' + dialogid).height()) / 2;
    if (top < 0) {
        top = 0;
        $('#' + dialogid).css('height', $(window).height());
        $('#' + dialogid).css('width', $('#' + dialogid).width() + 30);
    }
    $('#' + dialogid).css('top', top);
    $('#' + dialogid).css('left', ($(window).width() - $('#' + dialogid).width()) / 2);
    window.onresize = function () {
        var topResize = ($(window).height() - $('#' + dialogid).height()) / 2;
        if (topResize < 0) {
            topResize = 0;
            $('#' + dialogid).css('height', $(window).height());
        }
        $("#" + dialogid).css('top', topResize);
        $("#" + dialogid).css('left', ($(window).width() - $("#" + dialogid).width()) / 2);
    }
    $('#lightBox').css('opacity', '0.6').fadeIn(400);
    $('#' + dialogid).fadeIn(400);
    if (typeof btnCloseid != 'undefined') {
        $('#' + dialogid + ' .' + btnCloseid).click(function () {           
            hideDialog(dialogid);
        });
    }

    if (getDocHeight() > $(window).height()) {
        var scrollWidth = getScrollBarWidth();
        $("body").css("margin-right", scrollWidth + "px");
        if ($.browser.webkit == false || $.browser.webkit == undefined)
            $(".header").css("margin-right", scrollWidth + "px");
    }
    $("body").css("overflow", "hidden");
}

function hideDialog(dialogid) {
    $("body").css("margin-right", "0px");
    $(".header").css("margin-right", "0px");
    $("body").css("overflow", "auto");
    $("#lightBox").fadeOut(400);
    $("#" + dialogid).fadeOut(400);
}

function commonGetAjax(applicationName, action, param, callbackSuccess, callbackError, dataType) {
    if (typeof dataType == 'undefined')
        dataType = 'json';
    var url = '/' + applicationName + '/' + action;
    param += "&nocache=" + getunixtime();
    if (param != "")
        url += '?' + param;
    $.ajax({
        url: url,
        dataType: dataType,
        type: 'GET',
        success: function (response) {
            if (callbackSuccess != null && typeof callbackSuccess != 'undefined')
                callbackSuccess(response);
        },
        error: function (error) {
            if (callbackError != null && typeof callbackError != 'undefined')
                callbackError(error);
        }
    });
}

function commonPostAjax(applicationName, action, param, callbackSuccess, callbackError, dataType) {
    if (typeof dataType == 'undefined')
        dataType = 'json';
    var url = '/' + applicationName + '/' + action;
    $.ajax({
        url: url,
        dataType: dataType,
        type: 'POST',
        data: param,
        success: function (response) {
            if (callbackSuccess != null && typeof callbackSuccess != 'undefined')
                callbackSuccess(response);
        },
        error: function (error) {
            if (callbackError != null && typeof callbackError != 'undefined')
                callbackError(error);
        }
    });
}

$('#alreadymember').click(function () {
    $('#signuppopup').fadeOut(400);
    $('#loginpopup').fadeIn(400);
    $('#loginpopup').css('top', ($('#lightBox').height() - $('#loginpopup').height()) / 2);
    $('#loginpopup').css('left', ($('#lightBox').width() - $('#loginpopup').width()) / 2);
    $('#loginpopup .btnCloseloginPopup').click(function () {
        $('#loginpopup').fadeOut(400);
        $('#lightBox').fadeOut(400);
    });
});

$('#notmember').click(function () {
    $('#loginpopup').fadeOut(400);
    $('#signuppopup').fadeIn(400);
    $('#signuppopup').css('top', ($('#lightBox').height() - $('#signuppopup').height()) / 2);
    $('#signuppopup').css('left', ($('#lightBox').width() - $('#signuppopup').width()) / 2);
    $('#signuppopup .btnCloseloginPopup').click(function () {
        $('#signuppopup').fadeOut(400);
        $('#lightBox').fadeOut(400);
    });
});


function setError() {
    pageActive();
}

function validateEmail(email) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function generateSlug(value) {
    return value.toLowerCase().replace(/-+/g, ' ').replace(/[^a-z0-9- ]/g, ' ').replace(/\s+/g, '-');
};

/*
* Date Format 1.2.3
* (c) 2007-2009 Steven Levithan <stevenlevithan.com>
* MIT license
*
* Includes enhancements by Scott Trenda <scott.trenda.net>
* and Kris Kowal <cixar.com/~kris.kowal/>
*
* Accepts a date, a mask, or a date and a mask.
* Returns a formatted version of the given date.
* The date defaults to the current date/time.
* The mask defaults to dateFormat.masks.default.
*/

var dateFormat = function () {
    var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
		timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
		timezoneClip = /[^-+\dA-Z]/g,
		pad = function (val, len) {
		    val = String(val);
		    len = len || 2;
		    while (val.length < len) val = "0" + val;
		    return val;
		};

    // Regexes and supporting functions are cached through closure
    return function (date, mask, utc) {
        var dF = dateFormat;

        // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
        if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
            mask = date;
            date = undefined;
        }

        // Passing date through Date applies Date.parse, if necessary
        date = date ? new Date(date) : new Date;
        if (isNaN(date)) throw SyntaxError("invalid date");

        mask = String(dF.masks[mask] || mask || dF.masks["default"]);

        // Allow setting the utc argument via the mask
        if (mask.slice(0, 4) == "UTC:") {
            mask = mask.slice(4);
            utc = true;
        }

        var _ = utc ? "getUTC" : "get",
			d = date[_ + "Date"](),
			D = date[_ + "Day"](),
			m = date[_ + "Month"](),
			y = date[_ + "FullYear"](),
			H = date[_ + "Hours"](),
			M = date[_ + "Minutes"](),
			s = date[_ + "Seconds"](),
			L = date[_ + "Milliseconds"](),
			o = utc ? 0 : date.getTimezoneOffset(),
			flags = {
			    d: d,
			    dd: pad(d),
			    ddd: dF.i18n.dayNames[D],
			    dddd: dF.i18n.dayNames[D + 7],
			    m: m + 1,
			    mm: pad(m + 1),
			    mmm: dF.i18n.monthNames[m],
			    mmmm: dF.i18n.monthNames[m + 12],
			    yy: String(y).slice(2),
			    yyyy: y,
			    h: H % 12 || 12,
			    hh: pad(H % 12 || 12),
			    H: H,
			    HH: pad(H),
			    M: M,
			    MM: pad(M),
			    s: s,
			    ss: pad(s),
			    l: pad(L, 3),
			    L: pad(L > 99 ? Math.round(L / 10) : L),
			    t: H < 12 ? "a" : "p",
			    tt: H < 12 ? "am" : "pm",
			    T: H < 12 ? "A" : "P",
			    TT: H < 12 ? "AM" : "PM",
			    Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
			    o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
			    S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
			};

        return mask.replace(token, function ($0) {
            return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
        });
    };
}();

// Some common format strings
dateFormat.masks = {
    "default": "ddd mmm dd yyyy HH:MM:ss",
    shortDate: "m/d/yy",
    mediumDate: "mmm d, yyyy",
    longDate: "mmmm d, yyyy",
    fullDate: "dddd, mmmm d, yyyy",
    shortTime: "h:MM TT",
    mediumTime: "h:MM:ss TT",
    longTime: "h:MM:ss TT Z",
    isoDate: "yyyy-mm-dd",
    isoTime: "HH:MM:ss",
    isoDateTime: "yyyy-mm-dd'T'HH:MM:ss",
    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
};

// Internationalization strings
dateFormat.i18n = {
    dayNames: [
		"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
		"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ],
    monthNames: [
		"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
		"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
    ]
};

// For convenience...
Date.prototype.format = function (mask, utc) {
    return dateFormat(this, mask, utc);
};

function domScript() {
    var scrpt = document.createElement('script');
    scrpt.src = '/Scripts/js/updatescript.js';
    $('body').append(scrpt);
}

function getTimezoneName() {
    timezone = jstz.determine();
    return timezone.name();
}

// Add html element to show "page is loading" when post ajax
$(document).ready(function () {
    if ($("#lightBox").length == 0)
        $('body').append('<div id="lightBox" class="lightBox z499"> </div>');
    if ($("#ajaxBusy").length == 0)
        $('body').append('<img id="ajaxBusy" src="/Images/progress.gif" class="waiting" style="z-index:9999" alt="loading..." />');

});

// Listen action post of ajax to show "page is loading". To use this, you must declare: window.isUseDefaultAjaxHandle = true;
$(document).ajaxStart(function () {
    if (window.isUseDefaultAjaxHandle)
        pageBusy();
    else if (window.isShowIconLoadingAjax)
        showIconLoading();

}).ajaxStop(function () {
    if (window.isUseDefaultAjaxHandle)
        pageActive();
    else if (window.isShowIconLoadingAjax)
        hideIconLoading();
    window.isUseDefaultAjaxHandle = false;
    window.isShowIconLoadingAjax = false;
});

// Pause page
function pageBusy() {
    $('#lightBox').css({ 'opacity': '0.15', 'z-index': '9998' }).fadeIn(200);
    $('#ajaxBusy').css({ 'z-index': '100000' }).show();
}

// Active page
function pageActive() {
    $('#ajaxBusy').hide();
    $('#lightBox').fadeOut(200);
    $('#aptImgLoader').hide();
}

function showIconLoading() {
    $('#ajaxBusy').show();
}

function hideIconLoading() {
    $('#ajaxBusy').hide();
}

var nextFunctionAfterAlert;
function showAlertMessage(message, nextfunction) {
    $("#alertMessage").html(message);
    showDialog("popupAlert");
    nextFunctionAfterAlert = nextfunction;
}

function closeAlertMessage() {
    hideDialog("popupAlert");
    if (typeof nextFunctionAfterAlert === 'function')
        nextFunctionAfterAlert();
}
function closeGuestMessage() {
    hideDialog("popupGuest");
    if (typeof nextFunctionAfterAlert === 'function')
        nextFunctionAfterAlert();
}
function reloadPage() {
    window.location.href = window.location.pathname;
}

function encodeUrl(str) {
    str = str.trim();
    str = str.replace(/ /g, '_');
    return encodeURIComponent(str);
}

function getQueryString() {
    var result = {}, queryString = location.search.substring(1), re = /([^&=]+)=([^&]*)/g, m;
    while (m = re.exec(queryString)) {
        result[decodeURIComponent(m[1]).toLowerCase()] = decodeURIComponent(m[2]);
    }
    return result;
}
function showConfirmGuestMessage(message, nextfunction) {
    $("#alertGuestMessage").html(message);
    showDialog("popupGuest");
    nextFunctionAfterAlert = nextfunction;
}

$(function () {
    var url = $.url(window.location.href);
    var returnUrl = url.param('ReturnUrl');
    if (typeof returnUrl != 'undefined' && returnUrl != '')
        redirectUrl = returnUrl;

    $("a.connectFacebook, a.btnFBLogin").on("click", function (event) {
        event.preventDefault();
        var link = $(this).attr("href");
        if (redirectUrl != "") {
            if (link.indexOf('?') > 0)
                link = link + "&redirectTo=" + redirectUrl;
            else
                link = link + "?redirectTo=" + redirectUrl;
        }
        window.location = link;
        return false;
    });
});
