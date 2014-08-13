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

