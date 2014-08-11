// Detect if brownser is device
var isDevice = navigator.userAgent.match(/(iphone|ipod|android|ipad)/i) ? true : false;
$(document).ready(function () {
    //if is device go to page download app
    if (isDevice) {
        $.ajax({
            type: "GET",
            url: '/Home/CheckIsDevice',
            contentType: 'application/json; charset=utf-8',
            async: false,
            dataType:'Json',
            success: function (data, textStatus, xhr) {
                if (data==1) {
                    var returnUrl = document.URL;
                    window.location.href = "/Home/DownLoadMobileApp?ReturnUrl=" + returnUrl;
                }
            }
        });
       
    }
    
    //begin datepicker    
    $("#datepicker").datepicker();
    //end datepicker 

    var ev = (isDevice) ? 'touchstart' : 'click',
		menu_holder = '.state span';
    $(menu_holder).click(function () {
        var menu = $('ul.the_menu');
        menu.stop().slideToggle(500, function () {
            // In case user re-click on menu hoder to close menu
            // we have to remove document's click event
            if (!$(this).is(':visible')) {
                $(document).unbind(ev);
                return;
            };
            // Add click event to document to close menu
            // This event will be remove when menu is close.
            $(document).bind(ev, function (e) {
                // prevent this process when user click on menu holder again.
                var str = menu_holder + ", .the_menu li a";
                if ($(e.target).is(str)) return;
                menu.stop().slideUp(500);
                // after close the menu, also remove document's click event to improve performance
                $(document).unbind(ev);
            });
        });
    });
    dropmenu("favorite", 6);
    dropmenu("package", 3);
    dropmenu("giftcard", 2);
});

//JS Sticky Scroller
function createtopscroll(start, stop) {
    
    var scroller1 = new StickyScroller(".fixheaderposition", {
        containerid: 'header-scroll-container',
        defaultWidth: '100%',
        start: start,
        end: stop,
        interval: 300,
        range: 0,
        margin: 0
    });
}
//Dropdown for color list



function colordropdown(idcontent, idlistcolor) {
    var ev = (isDevice) ? 'touchstart' : 'click',
        coloritem = '#' + idcontent;
    $(coloritem).click(function () {
        var menu = $('#' + idlistcolor);
        menu.stop().slideToggle(500, function () {
            // In case user re-click on menu hoder to close menu
            // we have to remove document's click event
            if (!$(this).is(':visible')) {
                $(document).unbind(ev);
                return;
            };

            $("#" + idlistcolor + " li").click(function () {
                $("#" + idcontent + " .currentcolor").css("background-color", $(this).find("span.currentcolor").css("background-color"));
                $("#" + idcontent + " .colorname").text($(this).find("span.colorname").text());
                menu.stop().slideUp(500, function () {
                    $(menu).css("display", "none");
                });
                $('.' + idlistcolor).css("overflow", "auto");

            });
            // Add click event to document to close menu
            // This event will be remove when menu is close.
            $(document).bind(ev, function (n) {
                $('.' + idlistcolor).css("overflow", "auto");
                // prevent this process when user click on menu holder again.
                var str = coloritem + ", " + "#" + idlistcolor + " li" + ", " + "#" + idlistcolor;
                if ($(n.target).is(str)) return;
                menu.stop().slideUp(500);
                // after close the menu, also remove document's click event to improve performance
                $(document).unbind(ev);
            });
        });
    });
}

function createleftscroll(start, stop) {
   
    var scroller = new StickyScroller(".scroll", {
        containerid: 'left-scroll-container',
        loadfinish: 'datepicker',
        start: start,
        end: stop,
        interval: 300,
        range: 84,
        margin: 84
    });
}

function createleftscrollnonmember(start, stop) {
    
    var scroller = new StickyScroller(".notmemberscroll", {
        containerid: 'left-scroll-container',
        loadfinish: 'datepicker',
        start: start,
        end: stop,
        interval: 300,
        range: 84,
        margin: 84
    });
}

function createtopleftscroll() {
    var start = $('.header').height(),
		endleftscroll = $('.footer').position().top - $('#accordion').height() + 6,
		endtopscroll = endleftscroll - $('.header').height() + 6;
    createtopscroll(0, endtopscroll);
    createleftscroll(start, endleftscroll);
}

function createtopleftscrollnonmember() {
    var start = $('.header').height(),
			endleftscroll = $('.footer').position().top - $('.notmemberscroll').height() + 6,
			endtopscroll = endleftscroll - $('.header').height();
    createtopscroll(0, endtopscroll);
    createleftscrollnonmember(start, endleftscroll);
}

function updatetopscroll() {
    var endtopscroll = $('.footer').position().top - $('.header').height();
    createtopscroll(0, endtopscroll);
}

function updatetopleftscroll() {
    var start = $('.header').height(),
		endleftscroll = $('.footer').position().top - $('#accordion').height() + 6,
		endtopscroll = endleftscroll - $('.header').height();
    createtopscroll(0, endtopscroll);
    createleftscroll(start, endleftscroll);
}

function updateleftscrollnonmember() {
    var start = $('.header').height(),
		endleftscroll = $('.footer').position().top - $('.notmemberscroll').height() + 6,
		endtopscroll = endleftscroll - $('.header').height();
    createtopscroll(0, endtopscroll);
    createleftscrollnonmember(start, endleftscroll);
}

//End JS Sticky Scroller

//begin popup
function showPopup(idForm, idItem) {
    var id = '#' + idForm;
    var rowItem = '#' + idItem;
    $(id).appendTo(rowItem).show();
    $(id).fadeIn(200);

    $('#btnClose').click(function () {        
        $(id).fadeOut(200);
    });
}
function showpopup(popupid) {
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    $('#' + popupid).fadeIn(400);
    //document.documentElement.style.overflow = 'hidden';  // firefox, chrome
    //document.body.scroll = "no"; // ie only
    $('#' + popupid).css('top', ($('#lightBox').height() - $('#' + popupid).height()) / 2);
    $('#' + popupid).css('left', ($('#lightBox').width() - $('#' + popupid).width()) / 2);
    var deviceAgent = navigator.userAgent.toLowerCase();
    var agentID = deviceAgent.match(/(iphone|ipod|android)/);
    if (agentID) {
        $('#' + popupid).css('position', 'absolute');
    }


    $('#' + popupid + ' .btnClose').click(function () {       
        $('body').css("overflow", "");
        $('#' + popupid).fadeOut(400);
        $('#lightBox').fadeOut(400);
        //document.documentElement.style.overflow = 'auto';  // firefox, chrome
        //document.body.scroll = "yes"; // ie only	
    });
}
//end popup

function loadhomepageitems() {
    var rheight = 58;
    for (i = 0; i < $('.activityitem .text').length; i++) {
        if ($('.activityitem .text').eq(i).height() > rheight) {
            rheight = $('.activityitem .text').eq(i).height();
            if (rheight > 72) {
                rheight = 72;
            }
        }
        var goposition = (rheight - 20) / 4
        if (i == 2) {

            for (n = 0; n < 3; n++) {
                $('.activityitem .text').eq(n).css('height', rheight);

                $('.activityitem .text .iconnext').eq(n).css('margin-top', goposition);
            }
        }
        else if (i == $('.activityitem .text').length - 1) {
            for (m = 3; m < $('.activityitem .text').length; m++) {
                $('.activityitem .text').eq(m).css('height', rheight);
                $('.activityitem .text .iconnext').eq(m).css('margin-top', goposition);
            }
        }

    }
}

function resetlefttop(htotal) {
    if (htotal > $(window).height()) {
        $('.colLeft .fixposition').css({ 'position': 'absolute', 'top': $('.colLeft .fixposition').position().top });
        $('.fixheaderposition').css({ 'position': 'absolute', 'top': $('.fixheaderposition').position().top });
    }
    else {
        $('.colLeft .fixposition').css('position', 'fixed');
        $('.fixheaderposition').css('position', 'fixed');
    }
}

function employee_edit_add_active(checkboxid) {
    $('#li' + checkboxid).addClass('liactive');
}

function employee_edit_remove_active(checkboxid) {
    $('#li' + checkboxid).removeClass('liactive');
}

function lengthofviewnote() {
    if ($(".boxappointmentnotes").prop('scrollHeight') <= 280) {
        $(".boxappointmentnotes .noteitem").css("background-image", "url(/images/line1.png)");
    }
    else {
        $(".boxappointmentnotes .noteitem").css("background-image", "url(/images/line2.png)");
    }

}

function btndownreset() {
    $('#favoritearrowdown').css('border-top-width', '1px');
    $('#favoritearrowdown').css('border-top-color', '#999999');
}

function btnemployeesdownreset() {
    $('#employeeslistarrowdown').css('border-top-width', '1px');
    $('#employeeslistarrowdown').css('border-top-color', '#999999');
}

function dropmenu(selectid, totalitem) {
    var listcount = $('#' + selectid + ' li').length,
		cli = 1,
		ev = (isDevice) ? 'touchstart' : 'click',
		arrowup = $('#' + selectid + 'arrowup'),
		arrowdown = $('#' + selectid + 'arrowdown'),
		lnkselect = $('#schedule' + selectid),
		ulcontent = $('#' + selectid),
		linode = $('#' + selectid + ' li'); ;

    lnkselect.click(function (e) {
        $('#' + selectid).stop().slideToggle('normal', function () {
            if (ulcontent.is(':hidden')) {
                lnkselect.removeClass('schedule' + selectid + 'active');
                lnkselect.addClass('schedule' + selectid);
                arrowdown.css('display', 'none');
                arrowup.css('display', 'none');
                arrowdown.css('border-top-color', '#999999');
            } else {

                lnkselect.removeClass('schedule' + selectid);
                lnkselect.addClass('schedule' + selectid + 'active');

                if (listcount >= totalitem) {
                    arrowdown.css({ 'display': 'block', 'border-top-width': '1px', 'border-top-color': '#999999' });
                }
                else {
                    arrowdown.css({ 'display': 'none' });
                    ulcontent.css('border-bottom', '1px solid #999999');
                }
            }

            if (!$(this).is(':visible')) {
                $(document).unbind(ev);
                return;
            };
            // Add click event to document to close menu
            // This event will be remove when menu is close.
            $(document).bind(ev, function (e) {

                // prevent this process when user click on menu holder again.
                //                if ($(e.target).is("#schedule" + selectid + " ,#" + selectid + "arrowup ,#" + selectid + "arrowdown")) return;
                if ($(e.target).is("#schedule" + selectid + " ,#" + selectid + "arrowup ,#" + selectid + "arrowdown, #" + selectid + " li a")) return;

                ulcontent.stop().slideUp(500, function () {
                    ulcontent.find('li').each(function (i, el) {
                        $(el).show();
                    });
                });
                lnkselect.removeClass('schedule' + selectid + 'active').addClass('schedule' + selectid);
                arrowup.css('display', 'none');
                arrowdown.css({ 'border-top-color': '#999999', 'display': 'none' });
                cli = 1;
                // after close the menu, also remove document's click event to improve performance
                $(document).unbind(ev);

            });
        });
        e.preventDefault();
        //return false;
    });

    arrowdown.click(function () {

        if (cli < listcount) {
            arrowup.css('display', 'block');
            arrowdown.css({ 'border-top-color': 'transparent', 'border-top-width': '0px' });
            if ((cli + totalitem) <= listcount) {
                $('#' + selectid + ' li:nth-child(' + cli + ')').stop().hide('slow');
                cli++;
                arrowup.css('display', 'block');
            }
        }
        if (cli + totalitem > listcount) {
            arrowdown.css('display', 'none');
            arrowup.removeClass('schedule' + selectid + 'arrowup').addClass('schedule' + selectid + 'arrowup1');
        }
        return false;
    });
    arrowup.click(function () {
        if (cli > 1) {
            if ((cli + totalitem) >= 1) {
                cli--;
                $('#' + selectid + ' li:nth-child(' + cli + ')').stop().show('slow');
                arrowdown.css('display', 'block');
                arrowup.removeClass('schedule' + selectid + 'arrowup1').addClass('schedule' + selectid + 'arrowup');
            }
        }
        if (cli == 1) {
            arrowup.css('display', 'none');
            arrowdown.css({ 'border-top-width': '1px', 'border-top-color': '#999999' });
        }
        return false;
    });

    linode.click(function () {       
        var index = $("#" + selectid + " li a").index($('a[class="active"]'));
        if (index != -1) {
            $("#" + selectid + " li a").eq(index).removeClass("active");
        }
        var objA = $("#" + selectid + " li a").eq(linode.index(this));
        objA.addClass("active");
        lnkselect.attr("title", objA.text());        
        //return false;
        //$("#schedulefavorite").text(objA.text());	
    });

    function accordionforadmin(activetab) {
        $("scroll").hide();
        $("#accordion").accordion({
            active: activetab,
            autoHeight: false,
            navigation: true
        });
        $("scroll").show();
    }

    //This function using for slideUp and slideDown of User Invoices page
    function clapinvoices(classitem, activeclass) {

        $('#printInvoice').live('click', function () {
            alert('dfdfdfdfd');
        });
        $("." + classitem).stop().toggle(
		function () {
		    var currentId = $(this).attr("id");
		    $("." + classitem).each(function (i, e) {
		        $(".activeitem").removeClass(activeclass);
		        $(".invitemcontent").hide("slow");
		        $(".invicon a").attr("class", "plus");
		    });

		    $("#" + currentId + " .activeitem").addClass(activeclass);
		    $("#" + currentId + " .invitemcontent").show("slow");
		    $("#" + currentId + " .invicon a").attr("class", "minus");
		},
		function () {
		    var currentId = $(this).attr("id");
		    $("#" + currentId + " .activeitem").removeClass(activeclass);
		    $("#" + currentId + " .invitemcontent").hide("slow");
		    $("#" + currentId + " .invicon a").attr("class", "plus");
		});
    }
}