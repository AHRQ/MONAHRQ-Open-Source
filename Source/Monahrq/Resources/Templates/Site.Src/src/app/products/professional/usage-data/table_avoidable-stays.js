/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays Condition Report Table Controller
 *
 * This controller generates the avoidable hospital stays by condition report, and displays the
 * results in a table.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDTableAvoidableStaysCtrl', UDTableAvoidableStaysCtrl);

  UDTableAvoidableStaysCtrl.$inject = ['$scope', '$state', '$stateParams', '$filter', 'UDAHSQuerySvc', 'counties', 'ahs', 'SortSvc', 'ModalLegendSvc', 'ReportConfigSvc'];
  function UDTableAvoidableStaysCtrl($scope, $state, $stateParams, $filter, UDAHSQuerySvc, counties, ahs, SortSvc, ModalLegendSvc, ReportConfigSvc) {

    $scope.query = UDAHSQuerySvc.query;
    $scope.model = {topicUserPcts: null};
    $scope.query.sortBy = 'name.asc';
    $scope.reportSettings = {};

    UDAHSQuerySvc.notifyReportChange($state.current.data.report.topic_table);
    setupReportHeaderFooter();

    function setupReportHeaderFooter() {
      var id = $state.current.data.report.topic_table;
      var report = ReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    $scope.modalLegend = function() {
      var id = $state.current.data.report['topic_table'];
      ModalLegendSvc.open(id, '');
    };


    $scope.showTable = function() {
      return !_.isNull(UDAHSQuerySvc.query.topic.measure);
    };

    $scope.$watch('query.sortBy', function(n, o) {
      if (n == o || !n) return;
      sortRows();
    });

    $scope.updatePct = function() {
      if($scope.model.topicUserPcts){
        var pct = $scope.model.topicUserPcts;

        _.each($scope.selectedAhsCounties, function (c) {
          if (c.pct10 == '-1' || c.pct10 == '-2') {
            c.pctUser = c.pct10;
          }
          else {
            c.pctUser = (c.pct10 / 0.1) * ($scope.model.topicUserPcts / 100);
          }
        });

        if ($scope.totalRow) {
          if ($scope.totalRow.pct10 == '-1' || $scope.totalRow.pct10 == '-2') {
            $scope.totalRow.pctUser = $scope.totalRow.pct10;
          }
          else {
            $scope.totalRow.pctUser = ($scope.totalRow.pct10 / 0.1) * ($scope.model.topicUserPcts / 100);
          }
        }
      }
    };

    $scope.selectText = function(element) {
        var doc = document
            , text = doc.getElementById(element)
            , range, selection
        ;
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
    $scope.selectTextMultiple = function (array_of_elements) {
            var doc = document,
                    selection;
            var range = [];
            var text = [];
            selection = window.getSelection();
            selection.removeAllRanges();

            for (var i = 0, len = array_of_elements.length; i < len; i++) {
                if (doc.body.createTextRange) { //ms
                    text[array_of_elements[i]] = doc.getElementById(array_of_elements[i]);
                    range[array_of_elements[i]] = doc.body.createTextRange();
                    range[array_of_elements[i]].moveToElementText(text[array_of_elements[i]]);
                    range[array_of_elements[i]].select();

                }
                else if (window.getSelection) { //all others
                    text[array_of_elements[i]] = doc.getElementById(array_of_elements[i]);
                    range[array_of_elements[i]] = doc.createRange();
                    range[array_of_elements[i]].selectNodeContents(text[array_of_elements[i]]);

                    selection.addRange(range[array_of_elements[i]]);
                }

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

    getRows();

    function getRows(){
      var topic = +$scope.query.topic.topic,
          measure = $scope.query.topic.measure;

      $scope.totalRow = null;

      var selectedRows = _.filter(ahs, function(row){
        if (row.topicId == topic && row.measureId == measure) {
          if (row.countyId == -1) {
            $scope.totalRow = row;
            return false;
          }

          return true;
        }

        return false;
      });

      var selectedRowsWithName = _.map(selectedRows, function(row){
        var result = counties.filter(function( obj ) {
          return obj.CountyID == row.countyId;
        });
        row.name = result.length > 0 ? result[0].CountyName : '';
        return row;
      });

      $scope.selectedAhsCounties = selectedRowsWithName;
      sortRows();
    }

    function sortRows() {
      if ($scope.query.sortBy) {
        var sortParams = $scope.query.sortBy.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];
        if (sortField === 'topicUserPcts') {
          sortField = 'pctUser';
        }
      }

      if (sortField != 'name') {
        // force a numeric sort -- some of the data fields are output as strings.
        SortSvc.objSortNumeric($scope.selectedAhsCounties, sortField, sortDir);
      }
      else {
        SortSvc.objSort($scope.selectedAhsCounties, sortField, sortDir);
      }
    }

  }

})();

