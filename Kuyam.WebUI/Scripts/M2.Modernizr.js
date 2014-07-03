Modernizr.addTest('ipad', function () {
    return !!navigator.userAgent.match(/iPad/i);
});

Modernizr.addTest('iphone', function () {
    return !!navigator.userAgent.match(/iPhone/i);
});

Modernizr.addTest('ipod', function () {
    return !!navigator.userAgent.match(/iPod/i);
});

Modernizr.addTest('appleios', function () {
    return (Modernizr.ipad || Modernizr.ipod || Modernizr.iphone);
});