/**
 * Consumer Product
 * Hospital Reports Module
 * Health Topics Page Controller
 *
 * This controller generates a grid of available health topics, serving as a
 * menu to navigate to the by-topic report.
 *
 * The original iteration of this page supported expanded details for each topic; the
 * logic below handles the interleaving needed for that, though the details are no
 * longer rendered in the template.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHHealthTopicsCtrl', CHHealthTopicsCtrl);

  CHHealthTopicsCtrl.$inject = ['$scope', 'topics'];
  function CHHealthTopicsCtrl($scope, topics) {
    $scope.topicClick = topicClick;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;
    init();

    function init() {
      $scope.visibleTopicDesc = null;

      var model = [];
      var summary = _.map(topics, function(t) {
        return {
          'type': 'summary',
          'topic': t
        };
      });
      var details = _.map(topics, function(t) {
        return {
          'type': 'detail',
          'topic': t
        };
      });

      _.each(summary, function(s, i) {
        if (i % 3 == 0 && i > 0) {
          for (var j = i - 3; j < i; j++) {
            model.push(details[j]);
          }
        }
        model.push(summary[i]);
      });

      if (summary.length % 3 > 0) {
        for (var i = details.length - details.length % 3; i < details.length; i++) {
          model.push(details[i]);
        }
      }

      if ((summary.length * 2) - model.length === 3) {
        for (var i = details.length - 3; i < details.length; i++) {
          model.push(details[i]);
        }
      }

      $scope.model = model;
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

    function topicClick(topic) {
      if ($scope.visibleTopicDesc == topic.TopicCategoryID) {
        $scope.visibleTopicDesc = null;
      }
      else {
        $scope.visibleTopicDesc = topic.TopicCategoryID;
      }
    }

  }
})();
