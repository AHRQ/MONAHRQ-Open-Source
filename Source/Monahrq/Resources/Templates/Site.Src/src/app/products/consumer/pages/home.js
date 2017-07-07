/**
 * Consumer Product
 * Pages Module
 * Home Page Controller
 *
 * This controller corresponds to the front page of the consumer site. It is primarily
 * responsible for building the search controls and running user searches.
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.pages')
    .controller('CHomeCtrl', CHomeCtrl);


  CHomeCtrl.$inject = ['$scope', '$sce', '$state', '$stateParams', 'ScrollToElSvc'];
  function CHomeCtrl($scope, $sce, $state, $stateParams, ScrollToElSvc) {
    $scope.search = search;
    $scope.query = {
      searchType: null,
      location: null
    };
    $scope.showValidationErrors = false;

    $scope.searchTypes = [];
    if ($scope.ConsumerReportConfigSvc.hasEntity($scope.ConsumerReportConfigSvc.entities.HOSPITAL)) {
      $scope.searchTypes.push({id: 'hospitals', label: 'Hospitals'})
    }
    if ($scope.ConsumerReportConfigSvc.hasEntity($scope.ConsumerReportConfigSvc.entities.NURSINGHOME)) {
      $scope.searchTypes.push({id: 'nursing-homes', label: 'Nursing Homes'});
    }
    if ($scope.ConsumerReportConfigSvc.hasEntity($scope.ConsumerReportConfigSvc.entities.PHYSICIAN)) {
      $scope.searchTypes.push({id: 'physicians', label: 'Doctors'});
    }

    $scope.onDownButton = onDownButton;
    $scope.getSearchTitle = getSearchTitle;
    $scope.getEntityGroupTitle = getEntityGroupTitle;

    init();

    if ($scope.config.products.consumer.HOMEPAGE_VIDEO == 1) {
      $scope.videoUrl = $sce.trustAsResourceUrl($scope.config.products.consumer.HOMEPAGE_VIDEO_URL);
    }

    function init() {
    }

    function search() {
      if ($scope.query.searchType && $scope.query.location) {
        if ($scope.query.searchType == 'physicians') {
          physicianSearch();
          return;
        }

        $state.go('top.consumer.' + $scope.query.searchType + '.location', {
          location: $scope.query.location,
          distance: 25
        });
      }
      else {
        $scope.showValidationErrors = true;
      }
    }

    function physicianSearch() {
      var params = {};
      params.searchType = 'location';
      params.location = $scope.query.location;
      $state.go('top.consumer.physicians.search', params);
    }

    function onDownButton() {
      ScrollToElSvc.scrollToEl('#home-search');
    }

    function getSearchTitle() {
      var entities = $scope.ConsumerReportConfigSvc.getEntities();
      var entityNames = {};
      entityNames[$scope.ConsumerReportConfigSvc.entities.HOSPITAL] =  'hospitals';
      entityNames[$scope.ConsumerReportConfigSvc.entities.NURSINGHOME] =  'nursing homes';
      entityNames[$scope.ConsumerReportConfigSvc.entities.PHYSICIAN] =  'doctors';
      var title = 'Find ';


      if (entities.length === 1) {
        title = title + entityNames[entities[0]] + ':';
      }
      else if (entities.length === 2) {
        title = title + entityNames[entities[0]] + ' or ' + entityNames[entities[1]] + ':';
      }
      else if (entities.length === 3) {
        title = title + entityNames[entities[0]] + ', ' + entityNames[entities[1]] + ', or ' + entityNames[entities[2]] + ':';
      }

      return title;
    }

    function getEntityGroupTitle() {
      var rcs = $scope.ConsumerReportConfigSvc;
      var entityNames = {};
      if (rcs.hasEntity(rcs.entities.HOSPITAL)) {
        entityNames[rcs.entities.HOSPITAL] = 'hospital';
      }
      if (rcs.hasEntity(rcs.entities.NURSINGHOME)) {
        entityNames[rcs.entities.NURSINGHOME] = 'nursing home';
      }
      var entities = _.without(rcs.getEntities(), 'PHYSICIAN');
      var title;

      if (entities.length === 1) {
        title = entityNames[entities[0]];
      }
      else if (entities.length === 2) {
        title = entityNames[entities[0]] + ' or ' + entityNames[entities[1]];
      }

      return title;
    }

  }

})();
