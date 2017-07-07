/**
 * Professional Product
 * Nursing Homes Module
 * Location Table Controller
 *
 * This controller handles the by-location search report. It will find all nursing homes
 * matching the user's query. Rating reports are then loaded and shown for any matching homes.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHTableLocationCtrl', NHTableLocationCtrl);


  NHTableLocationCtrl.$inject = ['$scope', '$state', 'NHQuerySvc', 'NHReportLoaderSvc', 'NHRepositorySvc', 'ResourceSvc',
    'SortSvc', 'ReportConfigSvc', 'ModalMeasureSvc', 'ModalLegendSvc', 'ModalGenericSvc'];
  function NHTableLocationCtrl($scope, $state, NHQuerySvc, NHReportLoaderSvc, NHRepositorySvc, ResourceSvc,
                               SortSvc, ReportConfigSvc, ModalMeasureSvc, ModalLegendSvc, ModalGenericSvc){
    var OVERALL_ID;
    var reports = [];
    var homesToCompare = {};
    var measureDefs = [];
    var listObj = [];

    $scope.query = NHQuerySvc.query;
    $scope.tableModel = [];
    $scope.columnModel = [];
    $scope.sortByOptions = [];
    $scope.reportSettings = {};
    $scope.haveSearched = false;

    $scope.showCompare = showCompare;
    $scope.canCompare = canCompare;
    $scope.toggleCompare = toggleCompare;
    $scope.compareHomes = compareHomes;
    $scope.modalMeasure = modalMeasure;
    $scope.modalLegend = modalLegend;
    $scope.showCompareHelpModal = showCompareHelpModal;
    $scope.updateColumn = updateColumn;

    init();


    function init() {
      console.log('NHTableLocationCtrl:init');
      $scope.searchStatus = 'NOT_STARTED';
      if (NHQuerySvc.query.searchType === 'overallRating') {
        NHQuerySvc.query.comparedTo = 'nat';
      }
      else {
        NHQuerySvc.query.comparedTo = 'peer';
      }
      NHQuerySvc.query.sortBy = 'name.asc';
      $scope.$watch('query.sortBy', function() { sortTableModel($scope.tableModel); });

      NHRepositorySvc.init().then(loadReport);
      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ReportConfigSvc.configForReport(id);
      $scope.reportSettings.header = report.ReportHeader;
      $scope.reportSettings.footer = report.ReportFooter;
    }

    function loadReport() {
      console.log('NHTableLocationCtrl:loadReport');
      if (!NHQuerySvc.isSearchable()) return;
      $scope.searchStatus = 'SEARCHING';

      ResourceSvc.getConfiguration()
        .then(function(config) {
          var overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID, config.NURSING_OVERALL_FMLYRATE_ID]);
          OVERALL_ID = config.NURSING_OVERALL_ID;

          ResourceSvc.getNursingHomeMeasures(overallIDs)
            .then(function(result) {
              measureDefs = result;

              NHReportLoaderSvc.getMeasureReports(overallIDs)
                .then(function(result) {
                  reports = result;

                  searchHomes()
                    .then(updateTable);

                  buildColumnModel(measureDefs);
                  buildSortOptions();
                });
            });
        });
    }

    function buildColumnModel(measureDefs) {
      var anyMissing = false;
      var cols = [];

      _.each(measureDefs, function (def) {
          var report = _.findWhere(reports, { id: def.MeasureID });
          if (report.data == null) {
              anyMissing = true;
              return;
          }

          var col = {
              id: def.MeasureID,
              name: def.SelectedTitle,
              showForNational: true,
              showForState: true,
              showForCounty: true,
              show: function (col) {
                  if ($scope.query.comparedTo == 'nat') {
                      return col.showForNational;
                  }
                  if ($scope.query.comparedTo == 'peer') {
                      return col.showForState;
                  }
                  if ($scope.query.comparedTo == 'county') {
                      return col.showForCounty;
                  }
                  return true;
              },
          };

          //  Calculate the sort options that show/hide a column.
          if (def.MeasuresName == "NH-OA-01") {
              col.showForState = false;
              for (var row in $scope.tableModel) {
                  if ($scope.tableModel[row][col.id].PeerRating != "-") {
                      col.showForState = true;
                      break;
                  }
              }
              col.showForCounty = false;
              for (var row in $scope.tableModel) {
                  if ($scope.tableModel[row][col.id].CountyRating != "-") {
                      col.showForCounty = true;
                  }
              }
          }
          else if (def.MeasuresName == "NH_COMP_OVERALL") {
              col.showForNational = false;
              for (var row in $scope.tableModel) {
                  if ($scope.tableModel[row][col.id].NatRating != "-") {
                      col.showForNational = true;
                  }
              }
          }

          //  Add column to final column list.
          cols.push(col);
      });

      if (anyMissing) {
        cols = _.filter(cols, function(col) {
          return col.id != OVERALL_ID;
        })
      }

      listObj = cols;
      $scope.columnModel = _.filter(listObj, function (o) {
          return o.show(o);
      });
    }

    function updateTable(homes) {
      var ids = _.pluck(homes, 'ID');

      NHRepositorySvc.getProfiles(ids)
        .then(function(profiles) {
          var finalReport = _.object(ids, _.map(profiles, function(profile) {
            return {
              id: profile.ID,
              name: profile.Name,
              address: profile.City + ', ' + profile.State
            };
          }));

          _.each(reports, function(report) {
            _.each(report.data, function(row) {
              if (_.has(finalReport, row.NursingHomeID)) {
                finalReport[row.NursingHomeID][""+report.id] = row;
              }
            });
          });

          finalReport = _.values(finalReport);

          sortTableModel(finalReport);

          $scope.tableModel = finalReport;
          $scope.haveSearched = true;
          $scope.searchStatus = 'COMPLETED';

        });
    }

    function searchHomes() {
      var query = NHQuerySvc.query;

      if (query.searchType === 'type') {
        return NHRepositorySvc.findByType(query.type);
      }
      else if (query.searchType === 'inHospital') {
        return NHRepositorySvc.findByInHospital(query.inHospital == 0 ? false : true);
      }
      else if (query.searchType === 'overallRating') {
        return NHRepositorySvc.findByMeasureRating(OVERALL_ID, query.overallRating);
      }
      else if (query.searchType === 'location' && query.location === 'county') {
        if (query.county === 999999) {
          return NHRepositorySvc.all();
        }
        else {
          return NHRepositorySvc.findByCounty(query.county);
        }
      }
      else if (query.searchType === 'location' && query.location === 'zip') {
        return NHRepositorySvc.findByZip(query.zip, +query.zipDistance);
      }
    }

    function sortTableModel(tm) {
      if (!NHQuerySvc.query.sortBy) return;
      var sortParams = NHQuerySvc.query.sortBy.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      if (sortField == 'name') {
        SortSvc.objSort(tm, sortField, sortDir);
      }
      else {
        var f;
        if (NHQuerySvc.query.comparedTo == 'nat') {
          f = 'NatRating';
        }
        else if (NHQuerySvc.query.comparedTo == 'peer') {
          f = 'PeerRating';
        }
        else if (NHQuerySvc.query.comparedTo == 'county') {
          f = 'CountyRating';
        }

        SortSvc.objSort2Numeric(tm, sortField, f, sortDir);
      }
    }

    function buildSortOptions() {
      var options = [];
      var descName = ' (High to Low)';
      var ascName = ' (Low to High)';

      options.push({
        id: 'name.asc',
        name: 'Nursing Home (A to Z)'
      });
      options.push({
        id: 'name.desc',
        name: 'Nursing Home (Z to A)'
      });

      _.each($scope.columnModel, function (c) {
        options.push({
          id: c.id + '.asc',
          name: c.name + ascName
        });
        options.push({
          id: c.id + '.desc',
          name: c.name + descName
        });
      });

      $scope.sortByOptions = options;
    }

    function showCompare() {
      return $scope.ReportConfigSvc.webElementAvailable('NH_Compare_Column');
    }

    function canCompare() {
      var size = _.size(homesToCompare);
      return (size >= 2 && size <= 5);
    }

    function toggleCompare(id) {
      if (_.has(homesToCompare, id)) {
        delete homesToCompare[id];
      }
      else {
        homesToCompare[id] = true;
      }
    }

    function compareHomes() {
      if (!canCompare()) return;

      $state.go('top.professional.nursing-homes.compare', {
        ids: _.keys(homesToCompare)
      });
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openNursingMeasure(id);
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function showCompareHelpModal() {
        if (!canCompare())
            ModalGenericSvc.open('Help', 'Please select at least two nursing homes to view this report.  No more than five nursing homes may be selected at a time.');
    }

    function updateColumn() {
        $scope.columnModel = _.filter(listObj, function (o) {
            return o.show(o);
        });
    }
  }

})();

