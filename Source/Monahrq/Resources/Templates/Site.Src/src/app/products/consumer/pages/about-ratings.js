/**
 * Professional Product
 * Pages Module
 * About Ratings Page
 *
 * This controller manages the top-level page for the Monahrq help section. It dynamically
 * builds the help sections available based on what categories of reports were generated.
 */
(function () {
    // debugger;
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.pages')
    .controller('AboutRatingsCtrl', AboutRatingsCtrl);


  AboutRatingsCtrl.$inject = ['$sce', '$scope', '$state', '$stateParams', '$q', 'ResourceSvc',
    'ConsumerReportConfigSvc', 'content', 'measureTopicCategories', 'measureTopics', 'ahsTopics'];
  function AboutRatingsCtrl($sce, $scope, $state, $stateParams, $q, ResourceSvc,
    ConsumerReportConfigSvc, content, measureTopicCategories, measureTopics, ahsTopics) {

    var topicVisibility = {};
    var measureDefs = {};
    var activePage = 'resourceWhatIsHealthCareQuality';
    if ($stateParams.page) activePage = $stateParams.page;

    $scope.content = content;
    $scope.measureTopicCategories = measureTopicCategories;
    $scope.measureDefs = measureDefs;
    $scope.ahsTopics = ahsTopics;
    $scope.getEntityGroupTitleMobile = getEntityGroupTitleMobile;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;
    jQuery('.resource__content-panel').focus();

    $scope.skipSubnav = function($event) {
      document.getElementById('how-to-use--content').focus();
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

    function share() {
        window.location = buildShareUrl();
    }

    function feedbackModal() {
        ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
        var url = escape(window.location);
        return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    function getEntityGroupTitleMobile() {
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

      if (ConsumerReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
        _.each(measureTopics, function(m) {
          if (m.Measures) {
            ids = _.union(ids, m.Measures);
          }
        });
      }

      if (ConsumerReportConfigSvc.webElementAvailable('Resource_AboutQR_AHS')) {
        _.each(ahsTopics, function(t) {
          ids = _.union(ids, _.pluck(t.measures, 'MeasureId'));
        });
      }

      ids = _.reject(ids, function(x) { return x == undefined; });

      return ids;
    }

    $scope.setPage = function(pg) {
      $state.go('top.consumer.about-ratings', {page:pg});
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
