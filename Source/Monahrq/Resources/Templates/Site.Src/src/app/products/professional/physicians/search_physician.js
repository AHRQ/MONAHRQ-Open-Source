/**
 * Professional Product
 * Physicians Module
 * Physicians Page Controller
 *
 * This controller manages the search interface for the physician page, and initiates
 * searches in response to user input.
 */

(function() {
  'use strict';

  /**
   * Angular wiring
   */

  angular.module('monahrq.products.professional.physicians')
    .controller('PhysiciansSearchCtrl', PhysiciansSearchCtrl);

  PhysiciansSearchCtrl.$inject = ['$scope', '$state', '$stateParams', 'SortSvc', 'PhysicianQuerySvc', 'physicianSpecialty'];

  function PhysiciansSearchCtrl($scope, $state, $stateParams, SortSvc, PhysicianQuerySvc, physicianSpecialty) {
    $scope.query = {};
    var searchTypeOptions = [
      {
        id: 'name',
        name: 'By Physician Name'
      },
      {
        id: 'practiceName',
        name: 'By Practice or Medical Group Name'
      },
      {
        id: 'city',
        name: 'By City'
      },
      {
        id: 'zip',
        name: 'By Zip'
      },
      {
        id: 'specialty',
        name: 'By Specialty'
      },
      {
        id: 'condition',
        name: 'By Medical Condition'
      }
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


    //$scope.specialtyOptions = [];
   // $scope.conditionOptions = [];

    $scope.canSearch = canSearch;
    $scope.search = search;

    init();


    function init() {
      PhysicianQuerySvc.fromStateParams($stateParams);
      $scope.query = PhysicianQuerySvc.query;

      $scope.matchTypeName= "";
      $scope.matchTypeCity = "";
      $scope.matchTypeZip = "Exact Match";
      $scope.matchTypeFacility = "";
      if ($scope.config.USED_REAL_TIME == 1) {
        $scope.matchTypeName= "(Exact Match)";
        $scope.matchTypeFacility = "Exact Match";
        $scope.matchTypeCity = "Exact Match";
      }

      buildSpecialtyOptions();

      $scope.searchTypeOptions = searchTypeOptions;
      $scope.$watch('query.searchType', onSearchChange);
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

    function onSearchChange() {
      var q = PhysicianQuerySvc.query;

      _.each(searchTypeOptions, function(o) {
        if (q[o.id] && o.id != q.searchType) {
          q[o.id] = null;
        }
      });
    }

    function canSearch() {
      return PhysicianQuerySvc.isSearchable();
    }

    function search() {
      var sp = PhysicianQuerySvc.toStateParams();
      $state.go('top.professional.physicians.find-physician', sp);
    }

  }

})();
