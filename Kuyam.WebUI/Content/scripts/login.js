$(function () {
    $('select#category').selectmenu();
    $('#password').hide();
    $('#passwordtext').focus(function () {
        $('#passwordtext').hide();
        $('#password').show();
        $('#password').css('color', '#333333');
        $('#password').focus();
    });
    $('#password').blur(function () {
        if ($('#password').val() == '') {
            $('#passwordtext').show();
            $('#password').hide();
        }
    });

    $(".howboxcontent li").click(function (e) {

        var className = $(e.target).attr('class');
        if (className == "arrow1") {
            $(".tab1").animate({ width: "74px" }, 500).removeClass("active");
            $(".tab2").animate({ width: "540px" }, 500).addClass("active");
            return false;
        }
        else if (className == "arrow3") {
            $(".tab2").animate({ width: "74px" }, 500).removeClass("active");
            $(".tab3").animate({ width: "540px" }, 500).addClass("active");
            return false;
        }

        if (!$(this).hasClass("active")) {
            $(this).animate({ width: "540px" }, 500);
            for (i = 0; i < 3; i++) {
                if ($(".howboxcontent li").eq(i).hasClass("active")) {
                    $(".howboxcontent li").eq(i).animate({ width: "74px" }, 500).removeClass("active");
                }
            }
            $(this).addClass("active");
        }
    });

    createtopscroll();
});

function signuppopup() {
    $('#lightBox').css('opacity', '0.3').fadeIn(400);
    $('#signuppopup').fadeIn(400);
    $('#signuppopup').css('top', ($('#lightBox').height() - $('#signuppopup').height()) / 2);
    $('#signuppopup').css('left', ($('#lightBox').width() - $('#signuppopup').width()) / 2);
    $('#signuppopup .btnCloseloginPopup').click(function () {
        $('#signuppopup').fadeOut(400);
        $('#lightBox').fadeOut(400);
    });
};