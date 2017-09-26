/**
 * Consumer Product
 * Nursing Home Reports Module
 * Profile Page Controller
 *
 * This controller generates the page that provides an overview of a particular nursing home.
 * There is a separate child view and controller which is responsible for the tabbed report.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.nursing-homes')
    .controller('CNHProfileCtrl', CNHProfileCtrl);


  CNHProfileCtrl.$inject = ['$scope', '$state', '$stateParams', 'NHRepositorySvc', 'ScrollToElSvc', 'ModalLegendSvc', 'ConsumerReportConfigSvc', 'ZipDistanceSvc', 'hospitalZips', 'zipDistances', 'deviceDetector'];
  function CNHProfileCtrl($scope, $state, $stateParams, NHRepositorySvc, ScrollToElSvc, ModalLegendSvc, ConsumerReportConfigSvc, ZipDistanceSvc, hospitalZips, zipDistances, deviceDetector) {
    var id;

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.profile = {};
    $scope.hasSearch = false;
    $scope.hasResult = false;
    $scope.query = {
      name: null
    };
    $scope.zipDistances = zipDistances;
    $scope.showValidationErrors = false;

    $scope.search = search;
    $scope.nearbyNursingHomes = nearbyNursingHomes;
    $scope.asBool = asBool;
    $scope.getFormattedDate = getFormattedDate;
    $scope.modalLegend = modalLegend;
    $scope.deviceData = deviceDetector;
    $scope.allData = JSON.stringify($scope.deviceData, null, 2);

    init();

    function init() {
      id = $stateParams.id;
      if (id == null || id == '') {
        $scope.hasSearch = false;
        return;
      }

      $scope.hasSearch = true;
      loadData(id);
      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function loadData(id) {
      NHRepositorySvc.getProfile(id)
        .then(function(data) {
          $scope.hasResult = true;
          ScrollToElSvc.scrollToEl('.profile .report');

          data.bInHospital = asBool(data.InHospital);
          data.bInRetirementCommunity = asBool(data.InRetirementCommunity);
          data.bSpecialFocus = asBool(data.SpecialFocus);
          data.bLastYearOwnershipChange = asBool(data.LastYearOwnershipChange);
          data.bHasSprinkler = asBool(data.HasSprinkler);

          $scope.profile = data;
          $scope.gmapUrl = makeGmapUrl();

          if (data.LatLng && data.LatLng.length === 2) {
            var zipCoord = _.findWhere(hospitalZips, {Zip: data.Zip});
            if (zipCoord) {
              $scope.zipDist = ZipDistanceSvc.calcDist(zipCoord.Latitude, zipCoord.Longitude, data.LatLng[0], data.LatLng[1]);
            }
          }
        });
    }

    function asBool(val) {
      if (val == 'Y' || val == 'Yes' || val == 'True') {
        return true;
      }

      return false;
    }

    function getFormattedDate(val) {
      var d = new Date(val);

      if (d) {
        var day = d.getDate();
        var monthIndex = d.getMonth();
        var year = d.getFullYear();
        return day + '/' + monthIndex + 1 + '/' + year;
      }
      else {
        return "";
      }
    }

    function makeGmapUrl() {
      var h = $scope.profile;
      var start = "http://maps.googleapis.com/maps/api/staticmap?center=",
        end = "&maptype=roadmap&size=340x350&sensor=true", addr, zoom, marker="";

      if (_.has($scope.config, 'DE_IDENTIFICATION') && $scope.config.DE_IDENTIFICATION === 1) {
        zoom = "&zoom=3";
        addr = "USA";
      }
      else {
        zoom = "&zoom=13";
        addr = escape(getAddress());
        marker = "&markers=" + addr;
      }

      return start + addr + zoom + end + marker;
    }

    function getAddress() {
      var h = $scope.profile;
      return h.Address + ', ' + h.City + ', ' + h.State + ' ' + h.Zip;
    }

    function canSearch() {
      return $scope.query.name;
    }

    function search() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }
      $state.go('top.consumer.profile-search', {type: 'nursing', term: $scope.query.name});
    }

    function nearbyNursingHomes() {
      $state.go('top.consumer.nursing-homes.location', {location: getAddress(), distance: $scope.query.distance});
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

  }

})();


