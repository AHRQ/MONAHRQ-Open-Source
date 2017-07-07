/**
 * Professional Product
 * Nursing Homes Module
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
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHProfileCtrl', NHProfileCtrl);


  NHProfileCtrl.$inject = ['$scope', '$state', '$stateParams', 'content', 'profile', 'nursingHomeCounties', 'NHReportLoaderSvc', 'ResourceSvc', 'ReportConfigSvc', 'ModalMeasureSvc', 'ModalLegendSvc'];
  function NHProfileCtrl($scope, $state, $stateParams, content, profile, nursingHomeCounties, NHReportLoaderSvc, ResourceSvc, ReportConfigSvc, ModalMeasureSvc, ModalLegendSvc) {
    var overallRatingRow;
    $scope.getYN = getYN;
    $scope.getFormattedDate = getFormattedDate;
    $scope.getOverallRating = getOverallRating;
    $scope.modalMeasure = modalMeasure;
    $scope.modalLegend = modalLegend;
    $scope.coalesce = coalesce;

    $scope.content = content;
    $scope.profile = profile;
    $scope.query = {
      comparedTo: 'peer'
    };
    $scope.reportSettings = {};
    $scope.reportId = $state.current.data.report;

    init();


    function init() {
      setupReportHeaderFooter();
      $scope.gmapUrl = makeGmapUrl(profile);
      $scope.countyName = getCountyName(profile);
      loadOverallRating();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ReportConfigSvc.configForReport(id);
      $scope.reportSettings.header = report.ReportHeader;
      $scope.reportSettings.footer = report.ReportFooter;
    }

    function loadOverallRating() {
      ResourceSvc.getConfiguration()
        .then(function(config) {
          NHReportLoaderSvc.getNursingHomeReport(profile.ID)
            .then(function (report) {
              overallRatingRow = _.findWhere(report.data, {MeasureID: config.NURSING_OVERALL_ID});
            });
        });
    }

    function getOverallRating() {
      if (overallRatingRow == undefined) return null;
      if ($scope.query.comparedTo === 'nat') {
        return overallRatingRow.NatRating;
      }
      else if ($scope.query.comparedTo === 'peer') {
        return overallRatingRow.PeerRating;
      }
      else if ($scope.query.comparedTo === 'county') {
        return overallRatingRow.CountyRating;
      }
    }

    function getCountyName(profile) {
      var c = _.findWhere(nursingHomeCounties, {CountyID: profile.CountyID});
      return c ? c.CountyName : null;
    }

    function getYN(val) {
      var fval = null;

      if (val === true || val === '1' || val === 1) {
        fval = 'Yes';
      }
      else if (val === false || val === '0' || val === 0) {
        fval = 'No';
      }
      else {
        fval = val;
      }

      return fval;
    }

    function getFormattedDate(val) {
      var d = new Date(val);

      if (d) {
        return d.toDateString();
      }
      else {
        return "";
      }
    }

    function makeGmapUrl(profile) {
      var start = "http://maps.googleapis.com/maps/api/staticmap?center=",
        end = "&maptype=roadmap&size=340x350&sensor=true", addr, zoom, marker="";

      if (_.has($scope.config, 'DE_IDENTIFICATION') && $scope.config.DE_IDENTIFICATION === 1) {
        zoom = "&zoom=3";
        addr = "USA";
      }
      else {
        zoom = "&zoom=13";
        addr = escape(profile.Address + ',' + profile.City + ',' + profile.State + ',' + profile.Zip);
        marker = "&markers=" + addr;
      }

      return start + addr + zoom + end + marker;
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openNursingMeasure(id);
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function coalesce() {
        var result = null;
        for (var index = 0; index < arguments.length; index++) {
            result = arguments[index];
            if (result != undefined && result != null)
                return result;
        }
        return null;
    }
  }

})();



