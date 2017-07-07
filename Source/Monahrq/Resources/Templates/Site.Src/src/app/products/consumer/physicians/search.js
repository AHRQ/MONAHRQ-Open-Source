/**
 * Consumer Product
 * Physicians Report Module
 * Physician Search Page Controller
 *
 * This controller handles user searches of physicians by various criteria such as
 * name, location, and specialty. It will generate a model of matching physicians,
 * including the CG-CAHPS rating of their practice if available.
 */

(function() {
  'use strict';

  /**
   * Angular wiring
   */

  angular.module('monahrq.products.consumer.physicians')
    .controller('PSearchCtrl', PSearchCtrl);

  PSearchCtrl.$inject = ['$scope', '$state', '$stateParams', '$q', 'SortSvc', 'ResourceSvc', 'PhysicianAPISvc',
    'PhysicianReportLoaderSvc', 'PhysicianDedupeSvc', 'MapQuestSvc', 'ScrollToElSvc', 'ModalLegendSvc',
    'ConsumerReportConfigSvc', 'physicianSpecialty', 'WalkthroughSvc','$rootScope'];

  function PSearchCtrl($scope, $state, $stateParams, $q, SortSvc, ResourceSvc, PhysicianAPISvc,
    PhysicianReportLoaderSvc, PhysicianDedupeSvc, MapQuestSvc, ScrollToElSvc, ModalLegendSvc,
    ConsumerReportConfigSvc, physicianSpecialty, WalkthroughSvc, $rootScope) {

    var reportConfig;

    var config, cahpsReport = {};
    var isResultsWalkthrough = false;
    var pageId = 'physSearch';
    var hasGuideTool = !_.isUndefined($rootScope.hasGuideTool) ? $rootScope.hasGuideTool : false;

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.query = {};
    var searchTypeOptions = [
      {
        id: 'name',
        name: 'By Doctor Name'
      },
      {
        id: 'location',
        name: 'By Location'
      },
      {
        id: 'specialty',
        name: 'By Specialty'
      },
      {
        id: 'condition',
        name: 'By Medical condition'
      }
    ];
    $scope.sortOptions = [
      {label: 'Last Name (a-z)', value: 'lst_nm.asc'},
      {label: 'Last Name (z-a)', value: 'lst_nm.desc'},
      {label: 'Practice Name (a-z)', value: 'primaryPracticeName.asc'},
      {label: 'Practice Name (z-a)', value: 'primaryPracticeName.desc'},
      {label: 'Overall Practice Rating (low-high)', value: 'practiceRating.asc'},
      {label: 'Overall Practice Rating (high-low)', value: 'practiceRating.desc'},
      {label: 'City (a-z)', value: 'cty.asc'},
      {label: 'City (z-a)', value: 'cty.desc'},
      {label: 'Zip (1-9)', value: 'zip.asc'},
      {label: 'Zip (9-1)', value: 'zip.desc'},
      {label: 'State (a-z)', value: 'st.asc'},
      {label: 'State (z-a)', value: 'st.desc'},
      {label: 'Primary Specialty (a-z)', value: 'pri_spec.asc'},
      {label: 'Primary Specialty (z-a)', value: 'pri_spec.desc'}
    ];

    var conditions1 = _.map(physicianSpecialty, function(s) {
      return {
        id: s.Id,
        conditions: s.ProviderTaxonomy ? s.ProviderTaxonomy.split('|') : []
      }
    });
    var conditions2 = [];
    _.each(conditions1, function(c) {
      _.each(c.conditions, function(c2, idx) {
        conditions2.push({
          id: c.id + '|' + idx,
          name: c2
        });
      });
    });
    var selCond = $stateParams.condition ? _.where(conditions2, {id: $stateParams.condition}) : null;
    $scope.uiaConditions = {
      rowLabel: 'name',
      rowId: 'id',
      widgetId: 'uia-condition',
      widgetTitle: 'Condition',
      defaultLabel: selCond && selCond.length > 0 ? selCond[0].name : null,
      data: conditions2
    };

    $scope.canSearch = canSearch;
    $scope.search = search;
    $scope.getPracticeName = getPracticeName;
    $scope.getPracticeRating = getPracticeRating;
    $scope.modalLegend = modalLegend;
    $scope.IntroOptions = WalkthroughSvc.IntroOptionsPhysiciansSearch();
    $scope.startWalkthrough = startWalkthrough;
    $scope.beforeChangeEvent = beforeChangeEvent;
    $scope.onExit = onExit;

    init();


    function init() {
      $scope.searchStatus = 'NOT_STARTED';
      MapQuestSvc.init($scope.config.website_MapquestApiKey, $scope.config.website_States);
      $scope.matchTypeLocation = "Enter address, city or zip code";
      $scope.matchTypeName = "";
      $scope.matchTypeFacility = "";
      if ($scope.config.USED_REAL_TIME == 1) {
        $scope.matchTypeName= "(Exact Match)";
        $scope.matchTypeFacility = "Exact Match";
      }

      buildSpecialtyOptions();

      $scope.searchTypeOptions = searchTypeOptions;
      $scope.$watch('query.searchType', onSearchChange);

      $scope.$watch('query.sort', function() { sortTableModel($scope.tableModel); });

      $scope.query.sort = 'lst_nm.asc';
      $scope.query.searchType = $stateParams.searchType;
      $scope.query.firstName = $stateParams.firstName;
      $scope.query.lastName = $stateParams.lastName;
      $scope.query.practiceName = $stateParams.practiceName;
      $scope.query.location = $stateParams.location;
      $scope.query.specialty = $stateParams.specialty;
      $scope.query.condition = $stateParams.condition;

      findPhysicians();
      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      reportConfig = ConsumerReportConfigSvc.configForReport(id);
      if (reportConfig) {
          $scope.reportSettings.header = reportConfig.ReportHeader;
          $scope.reportSettings.footer = reportConfig.ReportFooter;
      }
    }

    function buildSpecialtyOptions() {
      var options = _.map(physicianSpecialty, function(row) {
        return {
          id: ""+row.Id,
          name: row.Name
        };
      });
      SortSvc.objSort(options, 'name', 'asc');
      $scope.specialtyOptions = options;
    }

    function onExit() {
        WalkthroughSvc.setIntroIsRunning(false);
        if (isResultsWalkthrough) {
            WalkthroughSvc.OnExit(pageId);
        }
    }

    function beforeChangeEvent(el, scope) {
        if (this._currentStep == 0) {
            WalkthroughSvc.screenRead($scope.IntroOptions.steps);
        }
    }

    function startWalkthrough(step) {
        if (hasGuideTool) {
            if (WalkthroughSvc.getIntroIsRunning() || !WalkthroughSvc.hasClosedWalkthrough(pageId)) {
                isResultsWalkthrough = true;
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsPhysiciansSearchResults();
                setTimeout(function () { $scope.walkthroughStart(step); }, 0);
            }
        }
    }

    function findPhysicians() {
      if (!canSearch()) {
        return;
      }

      $scope.searchStatus = 'SEARCHING';

      if ($scope.query.searchType === 'location' && !$scope.config.DE_IDENTIFICATION) {
        MapQuestSvc.geocode($scope.query.location)
          .then(function(data) {
            if (data.length == 0) {
              $scope.searchStatus = 'NO_RESULTS';
              return;
            }
            var geo = _.first(data);
            var params = {};

            if (_.has(geo, 'zip') && geo.zip != null && geo.zip.length > 0) {
              params.searchType = 'zip';
              params.zip = geo.zip;
            }
            else if (_.has(geo, 'city')) {
              params.searchType = 'city';
              params.city = geo.city;
            }
            else {
              return;
            }

            doFind(params);
          });
      }
      else {
        doFind();
      }
    }

    function doFind(params) {
      ResourceSvc.getConfiguration()
        .then(function(_config) {
          var searchFn;
          config = _config;

          if (config.USED_REAL_TIME == 1) {
            searchFn = buildSearchRemote(params);
          }
          else {
            searchFn = buildSearchLocal(params);
          }

          searchFn(config).then(function (physicians) {
            var pd = PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);
            $scope.tableModel = pd;
            if ($scope.tableModel.length > 0) {
              updateTableModel($scope.tableModel);
              sortTableModel($scope.tableModel);
            }

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
        if ($scope.tableModel.length > 0) {
          $scope.searchStatus = 'COMPLETED';
          ScrollToElSvc.scrollToEl('.physician-search .report');
        }
        else {
          $scope.searchStatus = 'NO_RESULTS';
        }
      }
    }

    function updateTableModel(model) {
      _.each(model, function(row) {
        row.primaryPracticeName = getPracticeName(row);
      });
    }

    function buildSearchLocal(params) {
      var q = params ? params : $scope.query;
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
      else if (q.searchType === 'zip') {
        fn = function(config) {
          PhysicianReportLoaderSvc.init(config);
          return PhysicianReportLoaderSvc.findByZip(q.zip)
            .then(function(result) {
              return result.data;
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

    function buildSearchRemote(params) {
      var q = params ? params : $scope.query;
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
      if (!$scope.query.sort || !tm) return;
      var sortParams = $scope.query.sort.split("."),
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

    function onSearchChange() {
      var q = $scope.query;

      _.each(searchTypeOptions, function(o) {
        if (q[o.id] && o.id != q.searchType) {
          q[o.id] = null;
        }
      });
    }

    function canSearch() {
      return ($scope.query.searchType == 'name' && ($scope.query.firstName || $scope.query.lastName))
        || ($scope.query.searchType == 'location' && $scope.query.location)
        || ($scope.query.searchType == 'condition' && $scope.query.condition)
        || ($scope.query.searchType == 'specialty' && $scope.query.specialty);
    }

    function search() {
      var sp = {
        searchType: $scope.query.searchType,
        firstName: $scope.query.firstName,
        lastName: $scope.query.lastName,
        practiceName: $scope.query.practiceName,
        location: $scope.query.location,
        specialty: $scope.query.specialty,
        condition: $scope.query.condition
      };

      if (sp.searchType == $stateParams.searchType
       && sp.firstName == $stateParams.firstName
       && sp.lastName == $stateParams.lastName
       && sp.practiceName == $stateParams.practiceName
       && sp.location == $stateParams.location
       && sp.specialty == $stateParams.specialty) {
          if (WalkthroughSvc.getSearchComplete('ph')) {
              $scope.startWalkthrough();
          }
          else {
              $scope.startWalkthrough(3);
          }
      }

      $state.go('top.consumer.physicians.search', sp);
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }
  }

})();
