$(function () {
    $(".companyTimeSlot").on("click", function (e) {
        e.preventDefault();
        var isLogin = isAuthenticated;
        if (isLogin) {
            window.location = $(this).attr("href");
        } else {
            redirectUrl = $(this).attr("href");
            $('#loginpopup').hide();
            showDialog('signuppopup', 'btnCloseloginPopup');
            return false;
        }
    });

    $(".companyLink").on("click", function (e) {
        e.preventDefault();       
        var isLogin = isAuthenticated;
        if (isLogin) {
            window.location = $(this).attr("href");
        } else {
            redirectUrl = $(this).attr("href");
            $('#loginpopup').hide();
            showDialog('signuppopup', 'btnCloseloginPopup');
            return false;
        }
    });

    $('.boxPrice .company').on("click", function () {
        console.log("company click");
        var isLogin = isAuthenticated;
        if (!isLogin) {
            redirectUrl = $(this).attr("href");
            $('#loginpopup').hide();
            showDialog('signuppopup', 'btnCloseloginPopup');
            return false;
        }

        pageBusy();

        var companyName = $(this).attr("companyName");
        var parameters = { companyName: companyName };
        $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(parameters),
                dataType: 'html',
                url: '/company/RequireSendEmailListCompanyHours/'
            })
            .success(function (result) {
                pageActive();
                showPopupThankYou();
                $('#popupthanks .btnClose').click(function() {
                    $('#popupthanks').fadeOut(200);
                    $('#lightBox').fadeOut(200);
                });

            })
            .error(function(error) {
                pageActive();

            });
    });
    
    
});

function addFavorite(favoriteItem) {
    var isLogin = isAuthenticated;
    if (!isLogin) {
        $('#loginpopup').hide();
        showDialog('signuppopup', 'btnCloseloginPopup');
        return false;
    }

    window.isUseDefaultAjaxHandle = true;
    var valu2 = $(favoriteItem).attr("profileID");
    var parameters = { profileID: valu2 };
    $.ajax(
        {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(parameters),
            dataType: 'html',
            url: '/company/AddToFavorite/'
        })
        .success(function(result) {
            if (result == "true") {
                $(favoriteItem).parent().html('<span class="pinkHeart" profileid="' + valu2 + '" onclick="removeFavorite(this);"></span>');
            }
        })
        .error(function(error) {
            pageActive();
        });
}

function removeFavorite(favoriteItem) {
    var isLogin = isAuthenticated;
    if (!isLogin) {
        $('#loginpopup').hide();
        showDialog('signuppopup', 'btnCloseloginPopup');
        return false;
    }

    var thisItem = $(favoriteItem);
    window.isUseDefaultAjaxHandle = true;
    var valu2 = $(favoriteItem).attr("profileID");
    var parameters = { profileID: valu2 };
    $.ajax(
        {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(parameters),
            dataType: 'html',
            url: '/company/RemoveFavorite/'
        })
        .success(function(result) {
            if (result == "true") {
                $(favoriteItem).parent().html('<span class="brownHeart" profileid="' + valu2 + '" onclick="addFavorite(this);"></span>');
            }
        })
        .error(function(error) {
            pageActive();
        });
}

function showPopupThankYou() {
    showDialog('popupthankcompanylisthours');
    
    setTimeout(function () {
        hideDialog('popupthankcompanylisthours');
    }, 3000);
}

