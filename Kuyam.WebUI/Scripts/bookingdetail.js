
angular.module('CompanyProfileApp', ['ui.bootstrap']);

function BookCtrl($scope, $modal, $http) {
    
    $scope.ServiceCompanies = data.ServiceCompanies;
    $scope.CompanyEmployees = data.CompanyEmployees;
    $scope.Categories = data.Categories;
    $scope.CategoryId = data.CategoryId;
    $scope.ServiceType = 0;
    $scope.ProfileId = data.ProfileID;
    $scope.CompanyType = data.CompanyTypeID;
    $scope.CalendarData = [];
    $scope.CalendarDataMobile = [];
    $scope.CurrentWeek = 0;
    $scope.employeeSelected = {};
    $scope.serviceSelected = {};
    $scope.ImageIds = data.ImageIds;

    $scope.prev = function () {
        $scope.CurrentWeek -= 1;
        var employeeId = 0;
        if ($scope.employeeSelected.EmployeeID) {
            employeeId = $scope.employeeSelected.EmployeeID;
        }
        getDataTimeslots($scope.serviceSelected.ServiceCompanyID, employeeId);
    }
      

    $scope.next = function () {
        $scope.CurrentWeek += 1;
        var employeeId = 0;
        if ($scope.employeeSelected.EmployeeID) {
            employeeId = $scope.employeeSelected.EmployeeID;
        }
        getDataTimeslots($scope.serviceSelected.ServiceCompanyID, $scope.employeeSelected.EmployeeID);
    }

    $scope.selectServiceRow = function (item, $event) {
        $(".select-who-text").html("");
        $scope.employeeSelected = {};
        $scope.serviceSelected = {};
        $scope.serviceSelected = item;
        $scope.ServiceType = item.ServiceTypeId;
        var serviceCompanyID = item.ServiceCompanyID;
        if (serviceCompanyID) {
            $http({
                method: 'POST',
                url: '/book/GetEmployeeByServiceCompanyId',
                data: JSON.stringify({ serviceComapanyId: serviceCompanyID })
            }).success(function (data, status, headers, config) {
                $scope.CompanyEmployees = data;
                getDataTimeslots(item.ServiceCompanyID, 0);
            }).error(function (data, status, headers, config) {
                $scope.message = 'Unexpected Error';
            });
        }

        var a = $($event.target).attr("class");
        if (a != "link-service ng-scope" || a != "link-class ng-scope") {
            $(".select-what-text").html(item.ServiceName);
            $('#select-who').fadeIn();
        }

    }

    $scope.selectEmployeeRow = function (item, $event) {
        $scope.employeeSelected = item;
        var a = $($event.target).attr("class");
        if (a != "link-service ng-scope" || a != "link-class ng-scope") {
            $(".select-who-text").html(item.EmployeeName);
        }
        getDataTimeslots($scope.serviceSelected.ServiceCompanyID, item.EmployeeID);
    }

    var getDataTimeslots = function (serviceCompanyID, emloyeeId) {

        var profileId = $scope.ProfileId;

       
        var url = "/CompanyProfile/GetTimslotsClassById?serviceId=" + serviceCompanyID + "&employeeId=" + emloyeeId + "&profileId=" + profileId + "&weekNumber=" + $scope.CurrentWeek;
        if ($scope.ServiceType == 0) {
            url = "/CompanyProfile/GetCalendars?serviceId=" + serviceCompanyID + "&employeeId=" + emloyeeId + "&profileId=" + profileId + "&companyType=" + $scope.CompanyType + "&weekNumber=" + $scope.CurrentWeek;
        }

        var deviceAgent = navigator.userAgent.toLowerCase();
        var agentID = deviceAgent.match(/(iphone|ipod|android|ipad)/);       
        if (agentID) {
            url += "&mobiledevice=true";
        } else {
            url += "&mobiledevice=false";
        }


        $http({
            method: 'GET',
            url: url,
        }).success(function (data, status, headers, config) {
            $scope.CalendarData = data;
            $scope.CalendarDataMobile = [];
            for(item in data) {          
                if (data[item].TimeSlot.length > 0) {
                    $scope.CalendarDataMobile = data[item].TimeSlot;
                    $scope.dayActive = data[item].DayOfWeek;
                    break;
                }
            }                   

        }).error(function (data, status, headers, config) {
            $scope.message = 'Unexpected Error';
        });

    }


    $scope.selectedCategory = function ($event, item) {
        $(".select-what-text").html("");
        $(".select-who-text").html("");
        $scope.CalendarData = [];
        $scope.CalendarDataMobile = [];
        $scope.CompanyEmployees = [];
        $scope.CurrentWeek = 0;
        $scope.CategoryId = item.ServiceID;
        var categoryId = item.ServiceID;
        var profileId = $scope.ProfileId;
        if (categoryId && profileId) {
            $http({
                method: 'POST',
                url: '/book/GetServiceCompanyByCategoryId',
                data: JSON.stringify({ profileId: profileId, categoryId: categoryId })
            }).success(function (data, status, headers, config) {
                $scope.ServiceCompanies = data;
            }).error(function (data, status, headers, config) {
                $scope.message = 'Unexpected Error';
            });

        }
    }

    $scope.dayActive = 0;
    $scope.fillterByDay = function (timeslots) {
        $scope.CalendarDataMobile = timeslots.TimeSlot;
        $scope.dayActive = timeslots.DayOfWeek;
    }

    $scope.open = function (item) {
        $scope.serviceSelected = item;
        var templateId = "serviceModalContent";
        var instanceNameCtrl = ModalServiceInstanceCtrl;
        if (item.ServiceTypeId == 1) {
            templateId = "employeeModalContent";
            instanceNameCtrl = ModalClassInstanceCtrl;
        }

        var modalInstance = $modal.open({
            templateUrl: templateId,
            controller: instanceNameCtrl,
            resolve: {
                item: function () {
                    return $scope.serviceSelected;
                }
            }
        });

    }

    $scope.toggled = function (open) {
        //$scope.items = data.ServiceCompanies;
        //console.log($scope.serviceSelected);
        //console.log($scope.employeeSelected)
        //console.log('Dropdown is now: ', open);

    };


    $scope.favorite = isFavorite;
    $scope.isAuthenticated = isAuthenticated;
    $scope.offer = {};

    $scope.gallery = function () {
        Galleria.loadTheme("/Content/gallery/galleria.classic.min.js");
        Galleria.run('#galleria');
    }
        
    $scope.addToFavorite = function () {

        var valu2 = $('#profileid').val();
        var searchParameters = { profileid: valu2 };
        $.ajax(
            {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(searchParameters),
                dataType: 'html',
                url: '/CompanyProfile/AddToFavorite/'
            })
            .success(function (result) {
                $scope.$apply(function(){
                    $scope.favorite = true;
                });
            })
            .error(function (error) {
                alert('error');
            });
    }

    $scope.removeFavorite = function(){
        
        var valu2 = $('#profileid').val();
        var searchParameters = { profileid: valu2 };
        $.ajax(
                {
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify(searchParameters),
                    dataType: 'html',
                    url: '/CompanyProfile/RemoveFromFavorite/'
                })
                .success(function (result) {
                    $scope.$apply(function(){
                        $scope.favorite = false;
                    });
                })
                .error(function (error) {
                    alert('error');
                });
    };

    $scope.getOfferInfo = function(offerId){
        //$('#modal-offer').modal({'show': true});
        
        var url = "/company/GetOffer/?companyEventId="+ offerId + "&nocache=" + getunixtime();
        $http({
            method: 'GET',
            url: url
        }).
            success(function(data, status, headers, config){
                $scope.offer = data;
                $scope.offer.Event.StartDate = new Date($scope.offer.Event.StartDate);
                $scope.offer.Event.EndDate = new Date($scope.offer.Event.EndDate);
                var offerModal = $modal.open({
                    templateUrl: "test1.html",
                    controller: "OfferCtrl",
                    resolve: {
                        items: function() {
                            return $scope.offer;
                        }
                    }
                });
            }).
            error(function(data, status, headers, config){

            });
    };

    $scope.checkAvaibility = function(serviceId, isClass){
        //alert(serviceId);
        var url = "/availability" + $scope.offer.SlugName + "?serviceId=" + serviceId;
        if(isClass){
            url = "/class" + $scope.offer.SlugName + "?serviceId=" + serviceId;
        }
        window.location.href = url;
    };

}


// Please note that $modalInstance represents a modal window (instance) dependency.
// It is not the same as the $modal service used above.

var ModalServiceInstanceCtrl = function ($scope, $modalInstance, item) {
    $scope.item = item;
    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
    $scope.bookNow = function (item) {
        $modalInstance.dismiss('cancel');
    }
};

var ModalClassInstanceCtrl = function ($scope, $modalInstance, item) {
    $scope.item = item;
    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
    $scope.bookNow = function (item) {
        $modalInstance.dismiss('cancel');
    }

};

var OfferCtrl = function($scope, $modalInstance, items) {
    $scope.offer = items;
    if (items.Offers.length > 0) {
        $scope.tabId = items.Offers[0].ID;
    }
    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    $scope.activeTab = function (tabId) {        
        $scope.tabId = tabId;
    };
}
