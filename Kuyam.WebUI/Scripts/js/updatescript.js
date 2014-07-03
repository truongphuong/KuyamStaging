$(document).ready(function () {

    updatetopleftscroll();
    $('#ui-accordion-accordion-panel-1 .pannelappointments').click(function (e) {
        if (typeof slider != "undefined") {
            slider.sliderTo(this.id);
        } else {
            self.location.href = "/Appointment/";
        }
    });

    $('#signup').click(function () {
        redirectUrl = "";
        $('#loginError').html('');
        showDialog('signuppopup', 'btnCloseloginPopup');
    });


    $("#signup2").keypress(function (e) {
        kCode = e.keyCode || e.charCode;
        if (kCode == 13) {
            singUpNow();
            return false;
        }
    });

    $("#signup1").keypress(function (e) {
        kCode = e.keyCode || e.charCode;
        if (kCode == 13) {
            singUpNow1();
            return false;
        }
    });

    $("#searchByName").keypress(function (e) {
        kCode = e.keyCode || e.charCode;
        if (kCode == 13) {
            searchProfileCompanyWithName();
            return false;
        }
    });


    $('.btndiscover').click(function (e) {
        var index = $("#category-menu li").index($('li[class*="ui-selectmenu-item-selected"]'));
        var id = $("#category option").eq(index).val();
        var url = "/company/companysearch/" + id;
        window.location = url;
        return false;
    });

    $('.homePageLogin').click(function () {
        redirectUrl = "";
        ShowLoginPopup();
    });

    $('.linksignout').click(function (e) {
        logout();
    });

    $('.btnLogin').click(function () {
        login();
    });

    $('#signup').click(function () {
        $('#signuppopup .loginError').hide();
        showDialog('signuppopup', 'btnCloseloginPopup');
    });

   
    
    //$("#ui-accordion-accordion-panel-1").height(326);

    //$("#accordion").accordion("option", "active", 0);


    $("#accordion").accordion({
        collapsible: false,
        autoHeight: false,
        active: activetab,
        changestart: function (event, ui) {
            if (event.currentTarget != undefined) {
                if (event.currentTarget.id == "ui-accordion-accordion-header-2") {
                    window.location = "/calendarview/";
                    return false;
                }
            }
        }
    });



    $("#datepicker").datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        dayNamesMin: $.datepicker._defaults.dayNamesShort
    });

    $(".pannelappointments").click(function (e) {
        e.stopPropagation();
        $(".pannelappointments").removeClass("bgactive");
        $(this).addClass("bgactive");
    });


});	

