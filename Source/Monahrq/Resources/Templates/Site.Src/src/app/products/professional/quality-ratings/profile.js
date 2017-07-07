
/**
 * Professional Product
 * Quality Ratings Module
 * Profile Page Controller
 *
 * This controller generates the profile page for a given hospital. Most of the details are
 * delegated to five child views. This report shows basic hospital profile information,
 * quality ratings, top 25 DRGs, patient experience ratings, and affiliated physicians.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRProfileCtrl', QRProfileCtrl);


  QRProfileCtrl.$inject = ['$scope', '$state', '$stateParams', '$sce', 'ReportConfigSvc',
    'content', 'hospitals', 'hospitalProfile', 'ModalMeasureSvc'];
  function QRProfileCtrl($scope, $state, $stateParams, $sce, ReportConfigSvc,
    content, hospitals, hospitalProfile, ModalMeasureSvc) {

    $scope.reportId = $state.current.data.report.profile;
    $scope.reportSettings = {};
    $scope.content = content;
    $scope.hospitals = hospitals;
    $scope.hospitalProfile = hospitalProfile;
    $scope.showOverallRating = showOverallRating;
    $scope.modalMeasure = modalMeasure;

    var activeTab = 'utilization';
    if (ReportConfigSvc.webElementAvailable('Quality_ConditionTopicExplore_Button')) {
      activeTab = 'quality-ratings';
    }

    $scope.gmapUrl = makeGmapUrl(hospitalProfile);

    setupReportHeaderFooter();

    function getChildConfig(report, activeTab) {
      var config;

      if (report && _.isObject(report.ChildConfig) && activeTab) {
        if (activeTab === 'physicians' && report.ChildConfig.AdmittingDoctors.HelpOverride) {
          config = report.ChildConfig.AdmittingDoctors;
        }
        else if (activeTab === 'utilization' && report.ChildConfig.Top25DRG.HelpOverride) {
          config = report.ChildConfig.Top25DRG;
        }
      }

      return config;
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report['profile'];
      var report = ReportConfigSvc.configForReport(id);
      var childConfig = getChildConfig(report, activeTab);
      if (!report) return;
      var header = report.ReportHeader;
      var footer = report.ReportFooter;


      if (childConfig) {
        header = childConfig.Header;
        footer = childConfig.Footer;
      }

      $scope.reportSettings.header = header;
      $scope.reportSettings.footer = footer;
    }

    $scope.getFTypes = function () {
      return (_.pluck($scope.hospitalProfile.types, 'type_Name')).join(', ');
    };

    $scope.getYN = function (val) {
      var fval = null;

      if (val === true) {
        fval = 'Yes';
      }
      else if (val === false) {
        fval = 'No';
      }

      return fval;
    };

    $scope.trustedUrl = function (url) {
      return $sce.trustAsResourceUrl(url)
    };

    $scope.setActiveTab = function (tab) {
      activeTab = tab;
      setupReportHeaderFooter();
    };

    $scope.isActiveTab = function (tab) {
      return activeTab == tab;
    };

    function makeGmapUrl(h) {
      var start = "http://maps.googleapis.com/maps/api/staticmap?center=",
        end = "&maptype=roadmap&size=340x350&sensor=true", addr, zoom, marker="";

      if (_.has($scope.config, 'DE_IDENTIFICATION') && $scope.config.DE_IDENTIFICATION === 1) {
        zoom = "&zoom=3";
        addr = "USA";
      }
      else {
        zoom = "&zoom=13";
        addr = escape(h.address + ',' + h.city + ',' + h.state + ',' + h.zip);
        marker = "&markers=" + addr;
      }

      return start + addr + zoom + end + marker;
    }

    function showOverallRating() {
      return $scope.config.USE_CMS_OVERALL_ID === 1;
    }

    function modalMeasure(id) {
      ModalMeasureSvc.open(id);
    }
  }

})();



