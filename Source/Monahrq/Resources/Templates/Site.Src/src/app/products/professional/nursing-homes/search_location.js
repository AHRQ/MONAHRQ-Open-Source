/**
 * Professional Product
 * Nursing Homes Module
 * Location Search Block Controller
 *
 * This controller manages the search interface for the location page, and initiates
 * searches in response to user input.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHSearchLocationCtrl', NHSearchLocationCtrl);


  NHSearchLocationCtrl.$inject = ['$rootScope', '$scope', '$state', '$stateParams', 'NHQuerySvc',
    'nursingHomes', 'nursingHomeTypes', 'nursingHomeCounties', 'zipDistances'];
  function NHSearchLocationCtrl($rootScope, $scope, $state, $stateParams, NHQuerySvc,
    nursingHomes, nursingHomeTypes, nursingHomeCounties, zipDistances) {

    var searchTypeOptions = [
      {
        id: 'name',
        name: 'By Name'
      },
      {
        id: 'location',
        name: 'By Location'
      },
      {
        id: 'type',
        name: 'By Type'
      },
      {
        id: 'inHospital',
        name: 'By In Hospital'
      },
      {
        id: 'overallRating',
        name: 'By Overall Rating'
      }
    ];

    var inHospitalOptions = [
      {
        id: '1',
        name: 'Yes'
      },
      {
        id: '0',
        name: 'No'
      }
    ];

    var overallRatingOptions = [
      {
        id: '1',
        name: 'One Star and Up'
      },
      {
        id: '2',
        name: 'Two Stars and Up'
      },
      {
        id: '3',
        name: 'Three Stars and Up'
      },
      {
        id: '4',
        name: 'Four Stars and Up'
      },
      {
        id: '5',
        name: 'Five Stars'
      }
    ];

    var locationOptions = [
      {
        id: 'county',
        name: 'County'
      },
      {
        id: 'zip',
        name: 'Zip Code'
      }
    ];

    $scope.uiaNursingHomes = {};

    $scope.canSearch = canSearch;
    $scope.search = search;

    init();


    function init() {
      NHQuerySvc.fromStateParams($stateParams);
      $scope.query = NHQuerySvc.query;

      $scope.searchTypeOptions  = searchTypeOptions;
      $scope.inHospitalOptions = inHospitalOptions;
      $scope.overallRatingOptions = overallRatingOptions;
      $scope.locationOptions = locationOptions;

      $scope.nursingHomeOptions = optionify('ID', 'Name', nursingHomes);
      $scope.countyOptions = [{id: 999999, name: 'All'}].concat(optionify('CountyID', 'CountyName', nursingHomeCounties));
      $scope.typeOptions = _.each(optionify('TypeID', 'Name', nursingHomeTypes), function(row) {
        if (row.id == null) {
          row.id = 999;
          row.name = 'Not Assigned';
        }
      });
      $scope.zipDistances = _.map(zipDistances, function(x) {
        return {
          id: x,
          name: x
        }
      });

      var selNH = $scope.query.name ? _.where(nursingHomes, {ID: +$scope.query.name}) : null;
      $scope.uiaNursingHomes = {
        rowLabel: 'Name',
        rowId: 'ID',
        widgetId: 'uia-nursing-home',
        widgetTitle: 'Nursing Home Name',
        defaultLabel: selNH && selNH.length > 0 ? selNH[0].Name : null,
        data: nursingHomes
      };

      $scope.$watch('query.searchType', onSearchChange);
      $scope.$watch('query.location', onSearchChange);
    }

    function onSearchChange() {
      var q = NHQuerySvc.query;

      _.each(locationOptions, function(o) {
        if (q[o.id] && o.id != q.location) {
          q[o.id] = null;
        }
      });

      _.each(searchTypeOptions, function(o) {
        if (q[o.id] && o.id != q.searchType) {
          q[o.id] = null;
        }
      });
    }

    function canSearch() {
      return NHQuerySvc.isSearchable();
    }

    function search() {
      if ($scope.query.searchType === 'name') {
        $state.go('top.professional.nursing-homes.profile', {
          id: $scope.query.name
        });
      }
      else {
        var sp = NHQuerySvc.toStateParams();
        sp.displayType = 'table';
        $state.go('top.professional.nursing-homes.location', sp);
      }
    }

    function optionify(id, name, data) {
      return _.map(data, function(row) {
        return {
          id: row[id],
          name: row[name]
        };
      });
    }

  }

})();

