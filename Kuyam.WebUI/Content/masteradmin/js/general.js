$(document).ready(function () {  
	//begin datepicker    
	$( "#datepicker" ).datepicker({
		showOtherMonths: true,
		selectOtherMonths: true
	});
	//end datepicker 
	
	// Detect if brownser is device
	var isDevice = navigator.userAgent.match(/(iphone|ipod|android|ipad)/i) ? true : false;
	
    if (isDevice) {
		$("input[type=text], textarea, input[type=password]").mouseover(zoomDisable).mousedown(zoomEnable);
    }
	
	
	//select to favorite
    var listcount = $('#favorite li').length;
    var cli = 1;
    function btndownreset() {
        $('#favoritearrowdown').css('border-top-width', '1px');
        $('#favoritearrowdown').css('border-top-color', '#999999');
    }

	var ev = (isDevice) ? 'touchstart' : 'click',
		menu_holder = '.state span';
	$(menu_holder).click(function(){
		var menu = $('ul.the_menu');
		menu.stop().slideToggle(500, function(){
			// In case user re-click on menu hoder to close menu
			// we have to remove document's click event
			if(!$(this).is(':visible')) {
				$(document).unbind(ev);
				return;
			};
			// Add click event to document to close menu
			// This event will be remove when menu is close.
			$(document).bind(ev, function(e) {
				// prevent this process when user click on menu holder again.
				if($(e.target).is(menu_holder)) return;
				menu.stop().slideUp(500);
				// after close the menu, also remove document's click event to improve performance
				$(document).unbind(ev);
			});
		});
	});
	// Add Event for #schedulefavorite
	
	$('#favoritearrowdown').click(function() {
		if (cli < listcount) {
			$('#favoritearrowup').css('display','block');
			$('#favoritearrowdown').css('border-top-color','transparent');
			$('#favoritearrowdown').css('border-top-width','0px');
			if((cli + 6) <= listcount)
			{
				$('#favorite li:nth-child(' + cli + ')').slideToggle();
				cli++;
				$('#favoritearrowup').show();
			}
		}
		if(cli + 6 > listcount)
		{
			$('#favoritearrowdown').hide();
			$('#favoritearrowup').removeClass('schedulefavoritearrowup');
			$('#favoritearrowup').addClass('schedulefavoritearrowup1');
		}
		
	});
	$('#favoritearrowup').click(function() {
		if (cli > 1) {
			if((cli + 6) >= 1)
			{
				cli--;
				$('#favorite li:nth-child(' + cli + ')').slideToggle();
				$('#favoritearrowdown').show();
				$('#favoritearrowup').removeClass('schedulefavoritearrowup1');
				$('#favoritearrowup').addClass('schedulefavoritearrowup');
			}
		}
		if(cli == 1)
		{
			$('#favoritearrowup').hide();
			btndownreset();
		}
		
	});
	
	$("#favorite li").click(function(){
			var index = $("#favorite li a").index($('a[class="active"]'));
			if(index != -1)
			{
				$("#favorite li a").eq(index).removeClass("active");	
			}
			var objA = $("#favorite li a").eq($("#favorite li").index(this));
			objA.addClass("active");	
			$("#schedulefavorite").attr("title",objA.text());	
			$("#schedulefavorite").text(objA.text());	
	});
	//end select to favorite
	
	//script select employee
	var employeelistcount = $('#employeeslist li').length;
	var employeelistli = 1;
	
	$('#employees').click(function(){
		$('#employeeslist').slideToggle('normal',function(){
			if ($('#employeeslist').is(':hidden')) {
				$('#employees').removeClass('schedulefavoriteactive');
				$('#employees').addClass('schedulefavorite');
				$('#employeeslistarrowdown').css('display','none');
				$('#employeeslistarrowup').css('display','none');
				$('#employeeslistarrowdown').css('border-top-color','#999999');
			} else {
				
				$('#employees').removeClass('schedulefavorite');
				$('#employees').addClass('schedulefavoriteactive');
				
				if(employeelistcount >= 3)
				{	
					$('#employeeslistarrowdown').css('display','block');	
					btnemployeesdownreset();
				}
				else
				{
					$('#employeeslistarrowdown').css('display','none');
					$('#employeeslist').css('border-bottom','1px solid #999999');
				}
				
			}
		});	
	});
	
	$("#employeeslist li").click(function(){
			var index = $("#employeeslist li a").index($('a[class="active"]'));
			if(index != -1)
			{
				$("#employeeslist li a").eq(index).removeClass("active");	
			}
			var objA = $("#employeeslist li a").eq($("#employeeslist li").index(this));
			objA.addClass("active");	
			$("#employees").attr("title",objA.text()).text(objA.text());	
	});
	
	$('#employeeslistarrowdown').click(function() {
		if (employeelistli < employeelistcount) {
			$('#employeeslistarrowup').css('display','block');
			$('#employeeslistarrowdown').css({'border-top-color':'transparent', 'border-top-width':'0px'});
			if((employeelistli + 3) <= employeelistcount)
			{
				$('#employeeslist li:nth-child(' + employeelistli + ')').slideToggle();
				employeelistli++;
				$('#employeeslistarrowup').show();
			}
		}
		if(employeelistli + 3 > employeelistcount)
		{
			$('#employeeslistarrowdown').hide();
			$('#employeeslistarrowup').removeClass('schedulefavoritearrowup').addClass('schedulefavoritearrowup1');
		}
		
	});
	$('#employeeslistarrowup').click(function() {
		if (employeelistli > 1) {
			if((employeelistli + 3) >= 1)
			{
				employeelistli--;
				$('#employeeslist li:nth-child(' + employeelistli + ')').slideToggle();
				$('#employeeslistarrowdown').show();
				$('#employeeslistarrowup').removeClass('schedulefavoritearrowup1');
				$('#employeeslistarrowup').addClass('schedulefavoritearrowup');
			}
		}
		if(employeelistli == 1)
		{
			$('#employeeslistarrowup').hide();
			btnemployeesdownreset();
		}
		
	});
	
	//end script select employee
	
	//User setting popup save change
	$("#ussavechange").click(function(){
		showpopup("savechangepopup");
	});
	//End user setting popup save change
	
	//List and Calendar button in Employee Based Appts List
	$("#idlist").click(function(){
		$(this).removeClass("optionlist").addClass("activeoptionlist");
		$("#idcalendar").removeClass("activeoptioncalendar").addClass("optioncalendar");
	});
	
	$("#idcalendar").click(function(){
		$("#idlist").removeClass("activeoptionlist").addClass("optionlist");
		$(this).removeClass("optioncalendar").addClass("activeoptioncalendar");
	});
	//End List and Calendar button in Employee Based Appts List

	//Reset function for appointment_review.html page	
	$(".apreset a").click(function(){
		$(".apservicecontent .timeselected").text('');
		$(".apservicecontent .timeselected").append("&nbsp;<br />&nbsp;");
	});
})

function btndownreset()
{
	$('#favoritearrowdown').css('border-top-width','1px');
	$('#favoritearrowdown').css('border-top-color','#999999');
}

function btnemployeesdownreset()
{
	$('#employeeslistarrowdown').css('border-top-width','1px');
	$('#employeeslistarrowdown').css('border-top-color','#999999');
}

//JS Sticky Scroller
function createtopscroll(start, stopscroll)
{
	if($("#header-scroll-container").length > 0)
	{
		$("#header-scroll-container").replaceWith($("#header-scroll-container").html());
	}
	var scroller1 = new StickyScroller(".fixheaderposition", {
		containerid: 'header-scroll-container',
		defaultWidth: '100%',
		start: start,
		end: stopscroll,
		interval: 300,
		range: 0,
		margin: 0
	});	
}

function createleftscroll(start, stopscroll)
{
	if($("#left-scroll-container").length > 0)
	{
		$("#left-scroll-container").replaceWith($("#left-scroll-container").html());
	}
	var scroller = new StickyScroller(".scroll",{
		containerid: 'left-scroll-container',
		loadfinish: 'datepicker',
		start: start,
		end: stopscroll,
		interval: 300,
		range: 84,
		margin: 84
	});	
}

function createleftscrollnonmember(start, stopscroll)
{
	if($("#left-scroll-container").length > 0)
	{
		$("#left-scroll-container").replaceWith($("#left-scroll-container").html());
	}
	var scroller = new StickyScroller(".notmemberscroll",{
		containerid: 'left-scroll-container',
		loadfinish: 'datepicker',
		start: start,
		end: stopscroll,
		interval: 300,
		range: 84,
		margin: 84
	});	
}

function createtopleftscroll()
{
	var start = $('.header').height(),
		endleftscroll = $('.footer').position().top - $('#accordion').height() + 6,
		endtopscroll = endleftscroll - $('.header').height() + 6;
	createtopscroll(0, endtopscroll);
	createleftscroll(start, endleftscroll);
}

function createtopleftscrollnonmember()
{
	var start = $('.header').height(),
			endleftscroll = $('.footer').position().top - $('.notmemberscroll').height() + 6,
			endtopscroll = endleftscroll - $('.header').height();
		createtopscroll(0, endtopscroll);
		createleftscrollnonmember(start, endleftscroll);
}

function updatetopscroll()
{
	var endtopscroll = $('.footer').position().top - $('.header').height();
	createtopscroll(0, endtopscroll);
}

function updatetopleftscroll()
{
	var start = $('.header').height(),
		endleftscroll = $('.footer').position().top - $('#accordion').height() + 6,
		endtopscroll = endleftscroll - $('.header').height();
	createtopscroll(0, endtopscroll);
	createleftscroll(start, endleftscroll);
}

function updateleftscrollnonmember()
{
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
	var rowItem='#' + idItem;
	$(id).appendTo(rowItem).show();
	$(id).fadeIn(200);		
	
	$('#btnClose').click(function () {			
		$(id).fadeOut(200);			
	});
}
//end popup

function loadhomepageitems()
{
	var rheight = 58;
	for(i =0; i < $('.activityitem .text').length ; i++)
	{
		if($('.activityitem .text').eq(i).height() > rheight)
		{
			rheight = $('.activityitem .text').eq(i).height();		
			if(rheight  > 72)
			{
				rheight = 72;
			}
		}
		var goposition = (rheight - 20)/4
		if (i == 2)
		{
			
			for(n =0; n < 3 ; n++)
			{
				$('.activityitem .text').eq(n).css('height',rheight);
				
				$('.activityitem .text .iconnext').eq(n).css('margin-top', goposition);
			}
		}
		else if(i == $('.activityitem .text').length - 1)
		{
			for(m = 3; m < $('.activityitem .text').length ; m++)
			{
				$('.activityitem .text').eq(m).css('height',rheight);
				$('.activityitem .text .iconnext').eq(m).css('margin-top', goposition);
			}
		}
		
	}	
}

function resetlefttop(htotal)
{
	if (htotal > $(window).height())
	{
		$('.colLeft .fixposition').css({'position':'absolute','top':$('.colLeft .fixposition').position().top});
		$('.fixheaderposition').css({'position':'absolute','top':$('.fixheaderposition').position().top});
	}
	else
	{
		$('.colLeft .fixposition').css('position','fixed');
		$('.fixheaderposition').css('position','fixed');
	}	
}

function employee_edit_add_active(checkboxid)
{
	$('#li' + checkboxid).addClass('liactive');
}

function employee_edit_remove_active(checkboxid)
{
	$('#li' + checkboxid).removeClass('liactive');
}

function lengthofviewnote()
{
	if($(".boxappointmentnotes").prop('scrollHeight') <= 280)
	{
		$(".boxappointmentnotes .noteitem").css("background-image","url(images/line1.png)");
	}
	else
	{
		$(".boxappointmentnotes .noteitem").css("background-image","url(images/line2.png)");
	}
	
}

function showpopup(popupid)
{
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
		$('#' + popupid).fadeOut(400);	
		$('#lightBox').fadeOut(400);	
		//document.documentElement.style.overflow = 'auto';  // firefox, chrome
    	//document.body.scroll = "yes"; // ie only	
	});
}

function accordionforadmin(activetab)
{
	$("scroll").hide();
	$("#accordion").accordion({
		active: activetab,
		autoHeight: false,
		navigation: true
	});
	$("scroll").show();
}


function zoomDisable(){
  $('head meta[name=viewport]').remove();
  $('head').prepend('<meta name="viewport" content="user-scalable=0" />');
}
function zoomEnable(){
  $('head meta[name=viewport]').remove();
  $('head').prepend('<meta name="viewport" content="user-scalable=1" />');
}



		