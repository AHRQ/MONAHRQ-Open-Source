/**
 * Consumer Product
 * Hospital Reports Module
 * Profile Page Controller
 *
 * This controller generates the page that provides an overview of a particular hospital.
 * There are separate child views and controllers which a responsible for the tabbed reports.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
    angular.module('monahrq.products.consumer.hospitals')
    .controller('CHProfileCtrl', CHProfileCtrl);


    CHProfileCtrl.$inject = ['$scope', '$state', '$stateParams', 'HospitalRepositorySvc', 'CHReportSvc', 'ScrollToElSvc', 'ModalLegendSvc', 'ConsumerReportConfigSvc', 'ZipDistanceSvc', 'hospitalZips', 'zipDistances', 'ModalMeasureSvc',];
    function CHProfileCtrl($scope, $state, $stateParams, HospitalRepositorySvc, CHReportSvc, ScrollToElSvc, ModalLegendSvc, ConsumerReportConfigSvc, ZipDistanceSvc, hospitalZips, zipDistances, ModalMeasureSvc) {
    var id;
    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.reportSettings.tabAPI = {};
    $scope.profile = {};
    $scope.overallRec = null;
    $scope.hasSearch = false;
    $scope.hasResult = false;
    $scope.query = {
      name: null
    };
    $scope.zipDistances = zipDistances;
    $scope.query = {};
    $scope.showValidationErrors = false;

    $scope.search = search;
    $scope.nearbyHospitals = nearbyHospitals;
    $scope.getHospitalType = getHospitalType;
    $scope.modalMeasure = modalMeasure;
    $scope.modalLegend = modalLegend;
    $scope.showOverallRating = showOverallRating;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;
    $scope.toggleFullTabTitle = toggleFullTabTitle;
    $scope.doctorsTabTitle = "Doctors that can...";

    init();

    function init() {
      id = $stateParams.id;
      if (id == null || id == '') {
        $scope.hasSearch = false;
        return;
      }

      $scope.hasSearch = true;
      loadData(id);

      $scope.$watch(function() {
        if (_.isFunction($scope.reportSettings.tabAPI.getActiveTab)) {
          return $scope.reportSettings.tabAPI.getActiveTab();
        }

        return null;
      }, function() {
          setupReportHeaderFooter();
      });
      setupReportHeaderFooter();
    }

    function toggleFullTabTitle(tabName) {
        if (tabName == "doctor") {
            $scope.doctorsTabTitle = "Doctors that can admit you to this hospital";
        }
        else {
            $scope.doctorsTabTitle = "Doctors that can...";
        }
    }

    function share() {
        window.location = buildShareUrl();
    }

    function feedbackModal() {
        ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
        var url = escape(window.location);
        return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    function getActiveTab() {
      return _.isFunction($scope.reportSettings.tabAPI.getActiveTab) ? $scope.reportSettings.tabAPI.getActiveTab() : null;
    }

    function getChildConfig(report, activeTab) {
      var config;

      if (_.isObject(report.ChildConfig) && activeTab) {
        if (activeTab === 'physicians' && report.ChildConfig.AdmittingDoctors.HelpOverride) {
          config = report.ChildConfig.AdmittingDoctors;
        }
      }

      return config;
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      var activeTab = getActiveTab();
      var childConfig = getChildConfig(report, activeTab);
      var header = report.ReportHeader;
      var footer = report.ReportFooter;

      if (!report) return;

      if (childConfig) {
        header = childConfig.Header;
        footer = childConfig.Footer;
      }

      $scope.reportSettings.header = header;
      $scope.reportSettings.footer = footer;
    }

    function loadData(id) {
      HospitalRepositorySvc.getProfile(id)
        .then(function(data) {
          $scope.hasResult = true;
          $scope.profile = data;
          $scope.gmapUrl = makeGmapUrl();

          if (data.LatLng && data.LatLng.length === 2) {
            var zipCoord = _.findWhere(hospitalZips, {Zip: data.zip});
            if (zipCoord) {
              $scope.zipDist = ZipDistanceSvc.calcDist(zipCoord.Latitude, zipCoord.Longitude, data.LatLng[0], data.LatLng[1]);
            }
          }
          ScrollToElSvc.scrollToEl('.profile .report');
        });

      CHReportSvc.getHospitalOverviewReport([id], $scope.config.HOSPITAL_OVERALL_ID)
        .then(function(_report) {
          if (_report.length == 1) {
            $scope.overallRec = _report[0].peerRating;
          }
        });
    }

    function getHospitalType() {
      var types = _.pluck($scope.profile.types, 'type_Name');

      if (types.length > 0) {
        return types.join(', ');
      }

      return null;
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
      return h.address + ', ' + h.city + ', ' + h.state + ' ' + h.zip;
    }

    function canSearch() {
      return $scope.query.name;
    }

    function search() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }
      $state.go('top.consumer.profile-search', {type: 'hospital', term: $scope.query.name});
    }

    function nearbyHospitals() {
      $state.go('top.consumer.hospitals.location', {location: getAddress(), distance: $scope.query.distance});
    }

    function modalLegend(){
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      var activeTab = getActiveTab();
      var childConfig = getChildConfig(report, activeTab);
      ModalLegendSvc.open(id, null, childConfig);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.open(id);
    }

    function showOverallRating() {
      return $scope.config.USE_CMS_OVERALL_ID === 1;
    }

  }

})();


