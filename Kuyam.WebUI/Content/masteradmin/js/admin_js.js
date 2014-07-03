//Check device 
var isDevice = navigator.userAgent.match(/(iphone|ipod|android|ipad)/i) ? true : false; 
$(function() {
	
	
	
	//Login popup
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
	$('#login').click(function(){
		showpopup("loginpopup");	
	});
	
	//Term and privacy popup
	$('#terms').click(function(){	
		$('#lightBox').css('opacity', '0.3').fadeIn(400);
		$('#lightBox').css('opacity', '0.3').fadeIn(400);
		$('#termspopup').fadeIn(400);		
		$('#termspopup').css('top', ($('#lightBox').height() - $('#termspopup').height()) / 2);
		$('#termspopup').css('left', ($('#lightBox').width() - $('#termspopup').width()) / 2);
		//Show scroller if user use device
		if (isDevice) {
			$("#termspopupbody").niceScroll({cursorborder:"",cursoropacitymin:1,cursorcolor:"#89c1f5",boxzoom:false}).resize();
			$("#termspopupbodyascrail2000")	.fadeIn(400);
			$('#termspopup .btnCloseloginPopup').click(function () {
				$("#termspopupbodyascrail2000")	.fadeOut(400);	
			});
		}
		
		$('#termspopup .btnCloseloginPopup').click(function () {
			$('#termspopup').fadeOut(400);	
			$('#lightBox').fadeOut(400);
		});				
	});
	
	$(".termspopupheader a").click(function(){
		$(".termspopupheader a").removeClass("headeractive");
		$(this).addClass("headeractive");	
		$("#divtermsofuse, #divprivacy, #divservices").hide();
		$("#div" + $(this).attr("id")).show();
		var deviceAgent = navigator.userAgent.toLowerCase();
		var agentID = deviceAgent.match(/(iphone|ipod|android|ipad)/);
		if (agentID) {
			$("#termspopupbody").niceScroll({cursorborder:"",cursoropacitymin:1,cursorcolor:"#89c1f5",boxzoom:false}).resize();
		}
	});
});

window.onresize = function () {
	centerWindow();
};
	
function centerWindow() {
	$('#comfirmationpopup').css('top', ($('#lightBox').height() - $('#comfirmationpopup').height()) / 2);
	$('#comfirmationpopup').css('left', ($('#lightBox').width() - $('#comfirmationpopup').width()) / 2);
};

function showpopup(popupid)
{
	$('#lightBox').css('opacity', '0.3').fadeIn(400);
	$('#' + popupid).fadeIn(400);		
	$('#' + popupid).css('top', ($('#lightBox').height() - $('#' + popupid).height()) / 2);
	$('#' + popupid).css('left', ($('#lightBox').width() - $('#' + popupid).width()) / 2);
    if (isDevice) {
		$('#' + popupid).css('position', 'absolute');
    }
	
	$('#' + popupid + ' .btnClose').click(function () {			
		$('#' + popupid).fadeOut(400);	
		$('#lightBox').fadeOut(400);	
	});
}