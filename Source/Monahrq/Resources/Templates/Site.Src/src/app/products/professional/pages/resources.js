/**
 * Professional Product
 * Pages Module
 * Resources Page
 *
 * This controller manages the top-level page for the Monahrq help section. It dynamically
 * builds the help sections available based on what categories of reports were generated.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('ResourcesCtrl', ResourcesCtrl);


  ResourcesCtrl.$inject = ['$sce', '$scope', '$state', '$stateParams', '$q', 'ResourceSvc',
    'ReportConfigSvc', 'content', 'measureTopicCategories', 'measureTopics', 'ahsTopics'];
  function ResourcesCtrl($sce, $scope, $state, $stateParams, $q, ResourceSvc,
    ReportConfigSvc, content, measureTopicCategories, measureTopics, ahsTopics) {

    var topicVisibility = {};
    var measureDefs = {};
    var activePage = 'AboutHealthCareQuality';
    if ($stateParams.page) activePage = $stateParams.page;

    $scope.content = content;
    $scope.measureTopicCategories = measureTopicCategories;
    $scope.measureDefs = measureDefs;
    $scope.ahsTopics = ahsTopics;

    $scope.skipSubnav = function($event) {
      document.getElementById('resources-content').focus();
      $event.preventDefault();
    };

    $scope.toggleTopic = function(topicId) {
      if (!_.has(topicVisibility, topicId)) topicVisibility[topicId] = true;
      else topicVisibility[topicId] = !topicVisibility[topicId];
    };

    $scope.isTopicVisible = function(topicId) {
      return _.has(topicVisibility, topicId) && topicVisibility[topicId];
    };

    $scope.getTopicsFor = function(catId) {
      return _.where(measureTopics, {TopicCategoryID: +catId});
    };

    $scope.getMeasuresFor = function(topicId) {
      if (_.has(measureData, topicId)) {
        return measureData[topicId];
      }

      return [];

    };

    function loadData() {
      ResourceSvc.getMeasureDefs(getAllMeasureIds())
      .then(function(result) {
        _.each(result, function(def) {
          measureDefs[def.MeasureID] = def;
        });
      });
    }

    function getAllMeasureIds() {
      var ids = [];

      if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
        _.each(measureTopics, function(m) {
          if (m.Measures) {
            ids = _.union(ids, m.Measures);
          }
        });
      }

      if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_AHS')) {
        _.each(ahsTopics, function(t) {
          ids = _.union(ids, _.pluck(t.measures, 'MeasureId'));
        });
      }

      ids = _.reject(ids, function(x) { return x == undefined; });

      return ids;
    }

    $scope.setPage = function(pg) {
      $state.go('top.professional.resources', {page:pg});
    };

    $scope.activePage = function() {
      return activePage;
    };

    $scope.hasMeasure = function(key) {
      return _.contains($scope.config.SURGICALSAFETY_MEASURES, key);
    }

    loadData();
  }
})();

