/**
* Override javascript alert() and wrap it into a jQuery-UI Dialog box
*
* depends $.getOrCreateDialog
*
* param { String } the alert message
* param { Object } jQuery Dialog box options
*/
function alert(message, ptitle, options) {
    var defaults = {
        modal: true,
        resizable: false,
        buttons: {
            Ok: function () {
                $(this).dialog('close');
            }
        },
        show: 'fade',
        title: ptitle,
        hide: 'fade',
        minHeight: 50,
        dialogClass: 'modal-shadow'
    }

    $alert = $.getOrCreateDialog('alert');
    // set message
    $("p", $alert).html(message);
    // init dialog
    $alert.dialog($.extend({}, defaults, options));
}

/**
* Override javascript confirm() and wrap it into a jQuery-UI Dialog box
*
* @depends $.getOrCreateDialog
*
* @param { String } the alert message
* @param { String/Object } the confirm callback
* @param { Object } jQuery Dialog box options
*/
function confirm(message, callback, options) {

    var defaults = {
        modal: true,
        resizable: false,
        buttons: {
            Ok: function () {
                $(this).dialog('close');
                return (typeof callback == 'string') ?
    window.location.href = callback :
    callback();
            },
            Cancel: function () {
                $(this).dialog('close');
                return false;
            },
            'open': function(event, ui) {
                // find all the buttons - note that the 'ui' argument is an empty object
                var buttons = $(event.target).parent().find('.ui-dialog-buttonset').children();
        
                // enable all buttons
                buttons.removeClass('ui-state-disabled').attr('disabled',false);
        
                // add the icons
                buttons.removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary');
                $(buttons[0]).append("<span class='ui-icon ui-icon-check'></span>");
                $(buttons[1]).append("<span class='ui-icon ui-icon-close'></span>");
        
                // push the first button to the left side
                $(buttons[0]).css('position','absolute').css('left','25px');
            }
        },
        show: 'fade',
        hide: 'fade',
        minHeight: 50,
        dialogClass: 'modal-shadow'
    }

    $confirm = $.getOrCreateDialog('confirm');
    // set message
    $("p", $confirm).html(message);
    // init dialog
    $confirm.dialog($.extend({}, defaults, options));
}

$.extend({
    /**
    * Create DialogBox by ID
    *
    * param { String } elementID
    */
    getOrCreateDialog: function (id) {

        $box = $('#' + id);
        if (!$box.length) {
            $box = $('<div id="' + id + '"><p></p></div>').hide().appendTo('body');
        }
        return $box;
    }
});