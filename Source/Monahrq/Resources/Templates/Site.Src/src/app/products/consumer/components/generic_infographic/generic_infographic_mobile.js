/**
 * Consumer Products
 * Components Module
 * Generic Infographic Directive
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.components.generic-infographic', [])
    .directive('mhcGenericInfographic', genericInfographic);

  function genericInfographic() {
    controller.$inject = ['$rootScope', '$scope', 'ResourceSvc', 'InfographicReportSvc'];

    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      replace: true,
      scope: {
        topicCategoryId: '=',
        preview: '=',
        showSurgicalLink: '=',
        showSurgicalSafetyLink: '=',
        onDataLoad: '&'
      },
      templateUrl: 'app/products/consumer/components/generic_infographic/views/generic_infographic_mobile.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($rootScope, $scope, ResourceSvc, InfographicReportSvc) {
      var topics, report, measures = [];

      $scope.isPreview = isPreview;
      $scope.getMeasureTitle = getMeasureTitle;
      $scope.showDesc = false;
      $scope.shareURL = encodeURIComponent(window.location);
      $scope.isDataLoaded = isDataLoaded;
      $scope.share = share;
      $scope.feedbackModal = feedbackModal;

      init();

      function init() {
        loadData();

        $rootScope.$on('$locationChangeSuccess', function(event, url) {
          $scope.shareURL = escape(url);
        });
      }

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

      function isDataLoaded() {
        if (report) {
          return true;
        }

        return false;
      }

      function loadData() {
        ResourceSvc.getMeasureTopicCategoriesConsumer()
          .then(function(data) {
            topics = data;
            return InfographicReportSvc.getGenericReport();
          })
          .then(function(data) {
            report = data;

            if ($scope.onDataLoad) {
              $scope.onDataLoad({hasData: $scope.isDataLoaded()});
            }
          })
          .then(update);
      }

      function update() {
        var topicCategoryID = $scope.topicCategoryId;
        $scope.topic = _.findWhere(topics, {'TopicCategoryID': topicCategoryID});
        $scope.report = _.findWhere(report, {'TopicCategoryID': topicCategoryID});

        if ($scope.topic.Name === 'Summary Scores') {
          $scope.showDesc = true;
        }

        ResourceSvc.getMeasureTopicsConsumer()
          .then(function(mts) {
            var topics = _.where(mts, {TopicCategoryID: topicCategoryID});
            var measureIds = _.flatten(_.pluck(topics, 'Measures'));
            return ResourceSvc.getMeasureDefs(measureIds);
          })
          .then(function(result) {
            measures = result;
          });
      }

      function getMeasureTitle(name) {
        if (measures && measures.length > 0) {
          var m = _.findWhere(measures, {'MeasuresName': name});
          if (m) {
            return m.PlainTitleConsumer;
          }
        }

        return null;
      }

      function isPreview() {
        return $scope.preview;
      }

    }
  }

})();

