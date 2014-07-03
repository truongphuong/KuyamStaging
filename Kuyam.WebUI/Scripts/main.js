//function SetGhostText(controlID, ghostText) {
//    var ghostColor = "#aaa";

//    // Reference our element
//    var txtContent = document.getElementById(controlID);

//    // Set default state of input
//    txtContent.value = ghostText;
//    jQuery("#" + controlID).addclass("placeholderText");

//    // Apply onfocus logic
//    txtContent.onfocus = function () {
//        // If the current value is our default value
//        if (this.value == ghostText) {
//            this.value = "";
//            jQuery("#" + controlID).removeclass("placeholderText");
//        }
//    }

//    // Apply onblur logic
//    txtContent.onblur = function () {
//        // If the current value is empty
//        if (this.value == "") {
//            this.value = ghostText;
//            jQuery("#" + controlID).addclass("placeholderText");
//        }
//    }
//}


// Create class "placeholderText" in CSS to format the placeholder, if desired
// Requires Modernizer HTML5-input suport
$(function () {
    SetupPage();
});

$(document).ready(function () {
    $('.DateTime').datepicker({ showOn: 'both' });
});


function SetupPage()
{
    var supported = Modernizr.input.placeholder;
    if (!supported) {
        jQuery("input").each(function () {
            if (jQuery(this).val() == "" && jQuery(this).attr("placeholder") != null && jQuery(this).attr("placeholder") != "") {

                jQuery(this).val(jQuery(this).attr("placeholder"));
                jQuery(this).addClass("placeholderText");

                jQuery(this).focus(function () {
                    if (jQuery(this).val() == jQuery(this).attr("placeholder")) {
                        jQuery(this).val("");
                        jQuery(this).removeClass("placeholderText");
                    }
                });

                jQuery(this).blur(function () {
                    if (jQuery(this).val() == "") {
                        jQuery(this).val(jQuery(this).attr("placeholder"));
                        jQuery(this).addClass("placeholderText");
                    }
                });
            }
        });
    }

    {
        var buttons = $("input:submit, button");
        buttons.button();
        buttons.addClass("abutton");

        buttons = $("a.button");
        buttons.button();
        buttons.addClass("ui-button").addClass("abutton").addClass("ui-button-text");
    }
}

/*
http://blog.stevenlevithan.com/archives/date-time-format
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
} ();

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

///////////// SPINNER ///////////////////
/* Need:
     <div id="spinner">
        Loading...
    </div>
   And:
      spinner.gif
*/
var spinner2Visible = false;
function showSpinner2() {
    if (!spinnerVisible) {
        $("div#spinner").fadeIn("fast");
        spinnerVisible = true;
    }
};
function hideSpinner2() {
    if (spinnerVisible) {
        var spinner = $("div#spinner");
        spinner.stop();
        spinner.fadeOut("fast");
        spinnerVisible = false;
    }
};

function daysBetween(date1, date2) {
    return Math.abs(subtractDate(date1, date2));
}

function subtractDate(date1, date2) {

    // The number of milliseconds in one day
    var ONE_DAY = 1000 * 60 * 60 * 24

    // Convert both dates to milliseconds
    var date1_ms = date1.getTime()
    var date2_ms = date2.getTime()

    // Calculate the difference in milliseconds
    var difference_ms = date1_ms - date2_ms

    // Convert back to days and return
    return Math.round(difference_ms / ONE_DAY)

}

// Setup a link as follows:
//    <a href="javascript:showDialog('@Url.Content("~/home/nda")', 'Non-Disclosure Agreement')" class="dialoglink">Non-Disclosure Agreement</a>
function showDialog(url, ptitle) {
    var loadingHtml = '<img src="@Url.Content("~/images/spinner.gif")" class="spinner"/>';
    $('#dialog').dialog({
        autoOpen: true,
        width: 650,
        height: 620,
        position: 'center',
        resizable: false,
        title: ptitle,
        modal: true,
        open: function (event, ui) {
            $(this).html(loadingHtml);
            $(this).load(url);

            //$(this).find('.ui-dialog-titlebar').hide();

            // find all the buttons - note that the 'ui' argument is an empty object
            //var buttons = $(event.target).parent().find('.ui-dialog-buttonset').children();
            //buttons.css({ "font-size": "0.85em", "padding": "2" }).addClass("dialog-button");

            // enable all buttons
            //buttons.removeClass('ui-state-disabled').attr('disabled',false);

            // add the icons
            //buttons.removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary');
            //$(buttons[0]).append("<span class='ui-icon ui-icon-check'></span>");
            //$(buttons[1]).append("<span class='ui-icon ui-icon-close'></span>");

            // push the first button to the left side
            //$(buttons[0]).css('position','absolute').css('left','25px');
        }
    });
};

// Setup a link as follows:
//    <a href="javascript:showDialog('@Url.Content("~/home/nda")', 'Non-Disclosure Agreement')" class="dialoglink">Non-Disclosure Agreement</a>
function showSignUpDialog(url, ptitle) {
    //var loadingHtml = '<img src="../images/spinner.gif" class="spinner"/>';
    var loadingHtml = '<img src="@Url.Content("~/images/spinner.gif")" class="spinner"/>';
    $('#SignUpDialog').dialog({
        autoOpen: true,
        dialogClass: 'SignUpDialog',
        position: 'center',
        resizable: false,
        title: ptitle,
        width: 313,
        height: 485,
        modal: true,
        open: function (event, ui) {
            $(this).html(loadingHtml);
            $(this).load(url);

            //$(this).find('.ui-dialog-titlebar').hide();

            // find all the buttons - note that the 'ui' argument is an empty object
            //var buttons = $(event.target).parent().find('.ui-dialog-buttonset').children();
            //buttons.css({ "font-size": "0.85em", "padding": "2" }).addClass("dialog-button");

            // enable all buttons
            //buttons.removeClass('ui-state-disabled').attr('disabled',false);

            // add the icons
            //buttons.removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary');
            //$(buttons[0]).append("<span class='ui-icon ui-icon-check'></span>");
            //$(buttons[1]).append("<span class='ui-icon ui-icon-close'></span>");

            // push the first button to the left side
            //$(buttons[0]).css('position','absolute').css('left','25px');
        }
    });
};

// Setup a link as follows:
//    <a href="javascript:showPopUpDialog('@Url.Content("~/home/nda")', 'Non-Disclosure Agreement')" class="dialoglink">Non-Disclosure Agreement</a>
function showPopUpDialog(url, ptitle) {
    //var loadingHtml = '<img src="../images/spinner.gif" class="spinner"/>';
    var loadingHtml = '<img src="@Url.Content("~/images/spinner.gif")" class="spinner"/>';
    $('#PopUpDialog').dialog({
        autoOpen: true,
        dialogClass: 'PopUpDialog',
        position: 'center',
        resizable: false,
        title: ptitle,
        width: 677,
        height: 596,
        modal: true,
        open: function (event, ui) {
            $(this).html(loadingHtml);
            $(this).load(url);

        }
    });
};

function showCompanySearchDialog(url, ptitle, onClose) {
    //var loadingHtml = '<img src="../images/spinner.gif" class="spinner"/>';
    var loadingHtml = '<img src="/images/spinner.gif" class="spinner"/>';
    $('#CompanySearchDialog').dialog({
        autoOpen: true,
        dialogClass: 'CompanySearchDialog',
        position: 'center',
        resizable: false,
        title: ptitle,
        width: 730,
        height: 491,
        modal: true,
        close: onClose,
        open: function (event, ui) {
            $(this).html(loadingHtml);
            $(this).load(url);

        }
    });
};


function showMyApptDialog(url, ptitle, onClose) {
    //var loadingHtml = '<img src="../images/spinner.gif" class="spinner"/>';
    var loadingHtml = '<img src="/images/spinner.gif" class="spinner"/>';
    $('#MyApptDialog').dialog({
        autoOpen: true,
        dialogClass: 'MyApptDialog',
        position: 'center',
        resizable: false,
        title: ptitle,
        width: 730,
        height: 491,
        modal: true,
        close: onClose,
        open: function (event, ui) {
            $(this).html(loadingHtml);
            $(this).load(url);

        }
    });
};

function SetFormLinkSubmit(formid, controlid) {
    var f = '#' + formid;
    var c = '#' + controlid;
    $(c).click(function () {
        $(f).submit();
    });
}

$(document).ready(function () {

    $('#password-clear').show();
    $('#password-password').hide();

    $('#password-clear').focus(function () {
        $('#password-clear').hide();
        $('#password-password').show();
        $('#password-password').focus();
    });

    $('#password-password').blur(function () {
        if ($('#password-password').val() == '') {
            $('#password-clear').show();
            $('#password-password').hide();
        }
    });

    $('.default-value').each(function () {
        var default_value = this.value;

        $(this).focus(function () {
            if (this.value == default_value) {
                this.value = '';
                this.style.fontStyle = 'normal';
            }
        });
        $(this).blur(function () {
            if (this.value == '') {
                this.value = default_value;
                this.style.fontStyle = 'italic';
            }
        });
    });

});
