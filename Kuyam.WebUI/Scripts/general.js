$(function() {
	//disable keyboad navigation in tabs
	$.widget( "ui.tabs", $.ui.tabs, {
		options: {
		  keyboard: true
		},
		_tabKeydown: function(e) {
		  if(this.options.keyboard) {
			this._super( '_tabKeydown' );
		  } else {
			return false;
		  }
		}
	});
    //$( "#tabs" ).tabs({
    //  collapsible: true
    //});
	
	////style item odd
	//$('#tabs').find('.ui-tabs-panel').each(function(){
	//	var itemsLength = $(this).find('.eight_fix').length;
	//	for(var i = 0; i < itemsLength; i++){
	//		if (i % 2 == 0) {
	//			var itemOdd = $(this).find('.eight_fix').eq(i).css({'margin-right': '18px'});
	//		}
	//	}
	//});
	//slide images
	$('#demo1').skdslider({delay:5000, animationSpeed: 2000,showNextPrev:false,showPlayButton:false,autoSlide:true,animationType:'fading',});
});

function getunixtime() {
    var d = new Date;
    var unixtime_ms = d.getTime();
    var unixtime = parseInt(unixtime_ms / 1000);
    return unixtime;
}

function searchProfileCompanyBykey(key) {
    var param = "key=" + key;
    var url = "/company/companysearch?" + param;
    if (key == 'search for a business' || key == '') {
        url = "/company/companysearch";
    }
    self.location.href = url;
    return false;
}

