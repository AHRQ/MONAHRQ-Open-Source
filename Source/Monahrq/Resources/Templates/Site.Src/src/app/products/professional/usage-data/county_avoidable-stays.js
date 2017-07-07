/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays County Report Controller
 *
 * This controller loads the report for avoidable hospital stays, grouped by county.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDCountyAvoidableStaysCtrl', UDCountyAvoidableStaysCtrl);


  UDCountyAvoidableStaysCtrl.$inject = ['$scope', '$state', '$stateParams', '$filter', 'UDAHSQuerySvc', 'ModalLegendSvc',
    'ReportConfigSvc', 'counties', 'ahsTopics', 'ahs'];
  function UDCountyAvoidableStaysCtrl($scope, $state, $stateParams, $filter, UDAHSQuerySvc, ModalLegendSvc,
    ReportConfigSvc, counties, ahsTopics, ahs) {

    $scope.query = UDAHSQuerySvc.query;
    $scope.topicUserPcts = {};
    $scope.reportSettings = {};

    UDAHSQuerySvc.notifyReportChange($state.current.data.report.county);
    setupReportHeaderFooter();

    function setupReportHeaderFooter() {
      var id = $state.current.data.report.county;
      var report = ReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    $scope.modalLegend = function () {
      var id = $state.current.data.report['county'];
      ModalLegendSvc.open(id, '');
    };

    $scope.updateUserPct = function () {
      updateTable();
    };

    $scope.showTable = function () {
      var t = UDAHSQuerySvc.query.county.topics;
      return t && _.size(t) > 0;
    };

    $scope.selectText = function (element) {
      var doc = document,
        text = doc.getElementById(element),
        range,
        selection;
      if (doc.body.createTextRange) { //ms
        range = doc.body.createTextRange();
        range.moveToElementText(text);
        range.select();
      } else if (window.getSelection) { //all others
        selection = window.getSelection();
        range = doc.createRange();
        range.selectNodeContents(text);
        selection.removeAllRanges();
        selection.addRange(range);
      }
    };

    $scope.filterValue = function (filter, param, value) {
      if (value == "-1") return "-";
      else if (value == "-2") return "c";

      return $filter(filter)(value, param);
    };

    $scope.filterMoneyValue = function (filter, param, value) {
      if (value == "-1") return "-";
      else if (value == "-2") return "c";

      return '$' + $filter(filter)(value, param);
    };

    updateTable();

    function updateTable() {
      var topics = [],
        query = $scope.query.county;

      $scope.county = _.findWhere(counties, {CountyID: query.county}).CountyName;

      var rows = _.filter(ahs, function (r) {
        return r.countyId == query.county && _.has(query.topics, r.topicId);
      });


      rows = _.map(rows, function (r) {
        var measures = _.findWhere(ahsTopics, {id: +r.topicId}).measures;
        r.measure = _.findWhere(measures, {id: r.measureId}).name;
        return r;
      });

      var groupedRows = _.groupBy(rows, function (r) {
        return r.topicId;
      });

      $scope.topics = _.map(groupedRows, function (rg) {
        var topic = _.findWhere(ahsTopics, {id: +rg[0].topicId});
        var row = {
          id: rg[0].topicId,
          name: topic.name,
          measures: rg
        };

        if (_.has($scope.topicUserPcts, row.id)) {
          var pct = $scope.topicUserPcts[row.id];
          _.each(row.measures, function (m) {
            if (pct) {
              if (m.pct10 == '-1' || m.pct10 == '-2') {
                m.pctUser = m.pct10;
              }
              else {
                m.pctUser = (m.pct10 / 0.1) * ($scope.topicUserPcts[row.id] / 100);
              }
            }
            else {
              m.pctUser = null;
            }
          });
        }

        return row;
      });
    }
  }

})();

