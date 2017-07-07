/**
 * Professional Product
 * Physicians Module
 * Physician Table Controller
 *
 * This controller handles the physician search report. It will find all physicians
 * matching the user's query. Reports are then loaded and shown for any matching physicians.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.physicians')
    .controller('PhysicianTableCtrl', PhysicianTableCtrl);


  PhysicianTableCtrl.$inject = ['$scope', '$state', '$q', 'PhysicianQuerySvc', 'ReportConfigSvc', 'ResourceSvc', 'PhysicianAPISvc', 'PhysicianReportLoaderSvc', 'SortSvc', 'PhysicianDedupeSvc', 'ModalLegendSvc', 'physicianSpecialty'];
  function PhysicianTableCtrl($scope, $state, $q, PhysicianQuerySvc, ReportConfigSvc, ResourceSvc, PhysicianAPISvc, PhysicianReportLoaderSvc, SortSvc, PhysicianDedupeSvc, ModalLegendSvc, physicianSpecialty){
    var config, cahpsReport = {};
    $scope.query = PhysicianQuerySvc.query;
    $scope.tableModel = [];
    $scope.columnModel = [];
    $scope.reportSettings = {};
    $scope.haveSearched = false;
    $scope.sortByOptions = [];

    $scope.getPracticeName = getPracticeName;
    $scope.getPracticeRating = getPracticeRating;
    $scope.modalLegend = modalLegend;

    init();


    function init() {
      $scope.searchStatus = 'NOT_STARTED';
      setupReportHeaderFooter();
      findPhysicians();

      buildSortOptions();
      PhysicianQuerySvc.query.sortBy = 'lst_nm.asc';
      $scope.$watch('query.sortBy', function() { sortTableModel($scope.tableModel); });

    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function findPhysicians() {
      if (!PhysicianQuerySvc.isSearchable()) return;
      $scope.searchStatus = 'SEARCHING';

      ResourceSvc.getConfiguration()
        .then(function(_config) {
          var searchFn;
          config = _config;

          if (config.USED_REAL_TIME == 1) {
            searchFn = buildSearchRemote();
          }
          else {
            searchFn = buildSearchLocal();
          }

          searchFn(config).then(function (physicians) {
            var pd = PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);
            $scope.tableModel = pd;
            sortTableModel($scope.tableModel);

            return ResourceSvc.getMedicalPracticeMeasure(config.MEDICALPRACTICE_OVERALL_QUALITY_ID);
          })
          .then(function(measureDef) {
            $scope.cahpsMeasureDef = measureDef;
            var practiceIds = _.pluck($scope.tableModel, 'org_pac_id');
            return PhysicianReportLoaderSvc.getCGCAHPSPractices(practiceIds);
          }, function() {
            completeLoad();
          })
          .then(function(result) {
            cahpsReport = result;

            $scope.hasCAHPS = false;
            for (var row in $scope.tableModel)
                if (getPracticeRating($scope.tableModel[row]) !== undefined)
                { $scope.hasCAHPS = true; break; }

            completeLoad();
          }, function() {
            completeLoad();
          });
        });

      function completeLoad() {
        $scope.haveSearched = true;
        if ($scope.tableModel.length > 0) {
          $scope.searchStatus = 'COMPLETED';
        }
        else {
          $scope.searchStatus = 'NO_RESULTS';
        }
      }
    }

    function buildSearchLocal() {
      var q = PhysicianQuerySvc.query;
      var fn;

      if ($scope.config.DE_IDENTIFICATION) {
        fn = function (config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.getDemoPhysicians()
            .then(function (result) {
              var physicians = _.flatten(_.map(result, function (row) {
                return row.data || [];
              }));
              return physicians;
            });
        }
      }
      else if (q.searchType === 'name') {
        var first = q.firstName ? q.firstName : '',
            last = q.lastName ? q.lastName : '';

        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findByName(first, last)
            .then(function(result) {
              var physicians = _.flatten(_.map(result, function(row) {
                return row.data || [];
              }));
              return physicians;
            });
        }
      }
      else if (q.searchType === 'practiceName') {
        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findByPracticeName(q.practiceName)
            .then(function(result) {
              var physicians = _.flatten(_.map(result, function(row) {
                return row.data || [];
              }));
              return physicians;
            });
        }
      }
      else if (q.searchType === 'city') {
        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findByCity(q.city)
            .then(function(result) {
              var physicians = _.flatten(_.map(result, function(row) {
                return row.data || [];
              }));
              return physicians;
            });
        }
      }
      else if (q.searchType === 'zip') {
        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findByZip(q.zip)
            .then(function(result) {
              return result.data;
            });
        }
      }
      else if (q.searchType === 'specialty') {
        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findBySpecialty(q['specialty'])
            .then(function(result) {
              return result.data;
            });
        }
      }
      else if (q.searchType === 'condition') {
        var specialtyId = (q['condition'].split('|'))[0];
        var specialties = _.pluck(_.filter(physicianSpecialty, function(row) {
          return row.Id == specialtyId;
        }), 'Id');

        fn = function(config) {
          var promises = [];
          PhysicianReportLoaderSvc.init(config);
          _.each(specialties, function(id) {
            promises.push(PhysicianReportLoaderSvc.findBySpecialty(id));
          });

          return $q.all(promises)
            .then(function(results) {
              var physicians = _.flatten(_.map(results, function(row) {
                return row.data;
              }));
              return physicians;
            });
        }
      }

      return fn;
    }

    function buildSearchRemote() {
      var q = PhysicianQuerySvc.query;
      var fn;

      if (q.searchType === 'name') {
        var first = q.firstName ? q.firstName : '',
            last = q.lastName ? q.lastName : '';

        fn = function(config) {
          PhysicianAPISvc.init(config);
          return PhysicianAPISvc.findByName(first, last);
        }
      }
      else if (q.searchType === 'practiceName') {
        fn = function(config) {
          PhysicianAPISvc.init(config);
          return PhysicianAPISvc.findByPracticeName(q.practiceName);
        }
      }
      else if (q.searchType === 'city') {
        fn = function(config) {
          PhysicianAPISvc.init(config);
          return PhysicianAPISvc.findByCity(q.city);
        }
      }
      else if (q.searchType === 'zip') {
        fn = function(config) {
          PhysicianAPISvc.init(config);
          return PhysicianAPISvc.findByZip(q.zip);
        }
      }
      else if (q.searchType === 'specialty') {
        var row = _.findWhere(physicianSpecialty, {Id: +q.specialty});
        fn = function(config) {
          PhysicianAPISvc.init(config);
          return PhysicianAPISvc.findBySpecialty(row.Name);
        }
      }
      else if (q.searchType === 'condition') {
        var specialtyId = (q['condition'].split('|'))[0];
        var specialties = _.pluck(_.filter(physicianSpecialty, function(row) {
          return row.Id == specialtyId;
        }), 'Name');

        fn = function(config) {
          var promises = [];
          PhysicianAPISvc.init(config);
          _.each(specialties, function(id) {
            promises.push(PhysicianAPISvc.findBySpecialty(id));
          });

          return $q.all(promises)
            .then(function(results) {
              var physicians = _.flatten(results);
              return physicians;
            });
        }
      }

      return fn;
    }

    function sortTableModel(tm) {
      if (!PhysicianQuerySvc.query.sortBy) return;
      var sortParams = PhysicianQuerySvc.query.sortBy.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      if (sortField != 'practiceRating') {
        SortSvc.objSort(tm, sortField, sortDir);
      }
      else {
        tm.sort(function(a, b) {
          var aRating = +getPracticeRating(a),
              bRating = +getPracticeRating(b);
          if (sortDir === 'asc') {
            return aRating - bRating;
          }
          else {
            return bRating - aRating;
          }
        });
      }
    }

    function buildSortOptions() {
      var options = [
        {
          id: 'lst_nm',
          name: "Physician Name"
        },
        {
          id: 'org_lgl_nm',
          name: "Practice Name"
        },
        {
          id: 'practiceRating',
          name: "Overall Practice Rating"
        },
        {
          id: 'cty',
          name: "City"
        },
        {
          id: 'zip',
          name: "ZIP Code"
        },
        {
          id: 'st',
          name: "State"
        },
        {
          id: 'pri_spec',
          name: "Primary Specialty"
        }
      ];

      _.each(options, function(option) {
        $scope.sortByOptions.push({
          id: option.id + ".asc",
          name: option.name + " (Ascending)"
        });
        $scope.sortByOptions.push({
          id: option.id + ".desc",
          name: option.name + " (Descending)"
        });
      });

    }

    function getPracticeName(row) {
      if (_.size(row.practices) > 0) {
        var prac = row.practices[_.first(_.keys(row.practices))];
        return prac.org_lgl_nm;
      }
      else if (_.has(row, 'org_lgl_nm')) {
        return row.org_lgl_nm;
      }

      return null;
    }

    function getPracticeRating(row) {
      var practice = cahpsReport[row.org_pac_id];
      var row = _.findWhere(practice, {MeasureID: config.MEDICALPRACTICE_OVERALL_QUALITY_ID});
      return row && row.PeerRating;
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }
  }

})();

