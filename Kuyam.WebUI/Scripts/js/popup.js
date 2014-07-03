$(function () {
    /*
    $('div.item').click(function () {
    if ($(this).attr("class") == "itemyellow") {
    $('div.item').removeClass("itemyellowactive");
    $(this).addClass("itemyellowactive");
    }
    else {
    $('div.item').removeClass("itemnormalactive");
    $(this).addClass("itemnormalactive");
    }
    });

    $('label.border').click(function (e) {

    var className = $(e.target).attr('class');
    if (className == "title" || className == "btnView" || className == "btnSchedule" || className == "company") {
    return false;
    }
    else {
    var id = "#" + $(this).attr("for");
    $('#formPopup').appendTo(id);
    $('#lightBox').css('opacity', '0.6').fadeIn(200);
    $('#formPopup').fadeIn(200);

    $('#btnClose').click(function (e) {
    e.stopPropagation();
    $('#formPopup').fadeOut(200);
    $('#lightBox').fadeOut(200);

    $('.listSearch div.item').removeClass("itemnormalactive");
    });
    }
    });
    */
//    $('.btnnext').click(function () {
//        $('#lightBox').css('opacity', '0.6').fadeIn(200);
//        $('#comfirmationpopup').fadeIn(200);
//        centerWindow();
//        $('#comfirmationpopup .btnClose').click(function () {
//            $('#comfirmationpopup').fadeOut(200);
//            $('#lightBox').fadeOut(200);
//        });
//    });

    //    $('.boxPrice .company').click(function () {
    //        $('#lightBox').css('opacity', '0.6').fadeIn(200);
    //        $('#popupthanks').fadeIn(200);
    //        $('#popupthanks').css('top', ($('#lightBox').height() - $('#popupthanks').height()) / 2);
    //        $('#popupthanks').css('left', ($('#lightBox').width() - $('#popupthanks').width()) / 2);
    //        $('#popupthanks .btnClose').click(function () {
    //            $('#popupthanks').fadeOut(200);
    //            $('#lightBox').fadeOut(200);
    //        });
    //    });

    $('#terms').click(function () {
       
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#termspopup').fadeIn(400);
        $('#termspopup').css('top', ($('#lightBox').height() - $('#termspopup').height()) / 2);
        $('#termspopup').css('left', ($('#lightBox').width() - $('#termspopup').width()) / 2);
        $('#termspopup .btnCloseloginPopup').click(function () {
            $('#termspopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
    });

    $('#termprivacy').click(function () {
        hideandshowprivacy("termsofuse");
   
        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#termspopup').fadeIn(400);
        $('#termspopup').css('top', ($('#lightBox').height() - $('#termspopup').height()) / 2);
        $('#termspopup').css('left', ($('#lightBox').width() - $('#termspopup').width()) / 2);
        $('#termspopup .btnCloseloginPopup').click(function () {
            $('#termspopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
    });

    $('#privacy').click(function () {
        hideandshowprivacy("privacy");

        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#termspopup').fadeIn(400);
        $('#termspopup').css('top', ($('#lightBox').height() - $('#termspopup').height()) / 2);
        $('#termspopup').css('left', ($('#lightBox').width() - $('#termspopup').width()) / 2);
        $('#termspopup .btnCloseloginPopup').click(function () {
            $('#termspopup').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
    });

    $('#disclosurepopup').click(function () {

        $('#lightBox').css('opacity', '0.3').fadeIn(400);
        $('#termspopupDisclosure').fadeIn(400);
        $('#termspopupDisclosure').css('top', ($('#lightBox').height() - $('#termspopupDisclosure').height()) / 2);
        $('#termspopupDisclosure').css('left', ($('#lightBox').width() - $('#termspopupDisclosure').width()) / 2);
        $('#termspopupDisclosure .btnCloseloginPopup').click(function () {
            $('#termspopupDisclosure').fadeOut(400);
            $('#lightBox').fadeOut(400);
        });
    });
    window.onresize = function () {
        centerWindow();
    };

    function centerWindow() {
        $('#comfirmationpopup').css('top', ($('#lightBox').height() - $('#comfirmationpopup').height()) / 2);
        $('#comfirmationpopup').css('left', ($('#lightBox').width() - $('#comfirmationpopup').width()) / 2);
    };
});