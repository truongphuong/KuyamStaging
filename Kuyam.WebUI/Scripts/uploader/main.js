/*
 * jQuery File Upload Plugin JS Example 6.5.1
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2010, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/*jslint nomen: true, unparam: true, regexp: true */
/*global $, window, document */

$(function () {
    //'use strict';

    // Initialize the jQuery File Upload widget:
    $('#fileuploadlogo').fileupload();
    $('#fileuploadlogo').fileupload('option', {
        maxFileSize: 1000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#logocontentfile",
        callbackStart: showloandinglogo,
        callbackSuccess: hideloandinglogo
    });

    $('#fileuploadphoto').fileupload();
    $('#fileuploadphoto').fileupload('option', {
        maxFileSize: 1000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#photocontentfile",
        downloadTemplateId: 'company-photo-template-download',
        callbackStart: showloandingphoto,
        callbackSuccess: hideloandingphoto
    });
    $('#fileuploadphoto2').fileupload();
    $('#fileuploadphoto2').fileupload('option', {
        maxFileSize: 1000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#photocontentfile2",
        downloadTemplateId: 'company-photo-template-download',
        callbackStart: showloandingphoto,
        callbackSuccess: hideloandingphoto
    });
    $('#fileuploadphoto3').fileupload();
    $('#fileuploadphoto3').fileupload('option', {
        maxFileSize: 1000000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#photocontentfile3",
        downloadTemplateId: 'company-photo-template-download',
        callbackStart: showloandingphoto,
        callbackSuccess: hideloandingphoto
    });
    $('#fileuploadvideo').fileupload();
    $('#fileuploadvideo').fileupload('option', {
        maxFileSize: 100000000,
        //            acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        autoUpload: true,
        filesContainer: "#videocontentfile",
        downloadTemplateId: 'template-download',
        callbackStart: showloandingvideo,
        callbackSuccess: hideloandingvideo
    });
});

function showloandinglogo() {    
    $("#imgLoaderlogo").show();
}

function hideloandinglogo() {
    $("#imgLoaderlogo").hide();
}

function showloandingphoto() {
    $("#imgLoaderphoto").show();
}

function hideloandingphoto() {
    $("#imgLoaderphoto").hide();
}
function showloandingvideo() {
    $("#imgLoadervideo").show();
}

function hideloandingvideo(file) {
    $("#imgLoadervideo").hide();    
    var deviceAgent = navigator.userAgent.toLowerCase();
    var agentID = deviceAgent.match(/(iphone|ipod|android|ipad)/);
    if (agentID) {        
        $("#videocontentfile").html("");
        $("#videocontentfile").append("<div id=\"formobile\" class=\"displaynone\"></div><input id=\"hdmediaid\" type=\"hidden\" value=\"" + file.mediaid + "\" name=\"hdmediaid\"><input id=\"hdmediadata\" type=\"hidden\" value=\"" + file.kalturaid + "\" name=\"hdmediadata\">");  
        $("#formobile").show();
        loadSources(kalturaPartnerId, file.kalturaid);
    } else {
        $("#videocontentfile").html(file.dataurl);
    }
    
}