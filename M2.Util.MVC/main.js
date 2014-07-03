//function SetGhostText(controlID, ghostText) {
//    var ghostColor = "#aaa";

//    // Reference our element
//    var txtContent = document.getElementById(controlID);

//    // Set default state of input
//    txtContent.value = ghostText;
//    jQuery("#" + controlID).addclass("placeholderText");

//    // Apply onfocus logic
//    txtContent.onfocus = function () {
//        // If the current value is our default value
//        if (this.value == ghostText) {
//            this.value = "";
//            jQuery("#" + controlID).removeclass("placeholderText");
//        }
//    }

//    // Apply onblur logic
//    txtContent.onblur = function () {
//        // If the current value is empty
//        if (this.value == "") {
//            this.value = ghostText;
//            jQuery("#" + controlID).addclass("placeholderText");
//        }
//    }
//}


// Create class "placeholderText" in CSS to format the placeholder, if desired
// Requires Modernizer HTML5-input suport
jQuery(document).ready(function () {
    var supported = Modernizr.input.placeholder;
    if (!supported) {
        jQuery("input").each(function () {
            if (jQuery(this).val() == "" && jQuery(this).attr("placeholder") != "") {

                jQuery(this).val(jQuery(this).attr("placeholder"));
                jQuery(this).addClass("placeholderText");

                jQuery(this).focus(function () {
                    if (jQuery(this).val() == jQuery(this).attr("placeholder")) {
                        jQuery(this).val("");
                        jQuery(this).removeClass("placeholderText");
                    }
                });

                jQuery(this).blur(function () {
                    if (jQuery(this).val() == "") {
                        jQuery(this).val(jQuery(this).attr("placeholder"));
                        jQuery(this).addClass("placeholderText");
                    }
                });
            }
        });
    }
});