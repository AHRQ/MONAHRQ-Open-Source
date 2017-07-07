/**
 * Monahrq Nest
 * Components Modals Module
 * Generic Modal
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.components.modal-topic-grid', [])
    .factory('ModalTopicGridSvc', ModalTopicGridSvc);

  ModalTopicGridSvc.$inject = ['$modal', '$rootScope'];
  function ModalTopicGridSvc($modal, $rootScope) {
    /**
     * Private Data
     */
    var modalInstance;

    /**
     * Service Interface
     */
    return {
      open: open
    };

    /**
     * Service Implementation
     */
    function open() {
      $rootScope.modalOpen = true;

      modalInstance = $modal.open({
        templateUrl: 'app/products/consumer/components/modal_topic_grid/views/modal_topic_grid.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        windowClass: 'modal--health-topics',
        controller: function ($scope, $state, topics) {
          $scope.title = 'Select a topic to compare facilities';
          $scope.topics = topics;
          $scope.pickTopic = pickTopic;

          function pickTopic(id) {
            $scope.$close();
            $state.go('top.consumer.hospitals.topic', {topicId: id, subtopicId: null});
          }
        },
        resolve: {
          topics: function (ResourceSvc) {
            return ResourceSvc.getMeasureTopicCategoriesConsumer();
          }
        }
      });

      modalInstance.result.then(onClose, onClose);
    }

    function onClose() {
      $rootScope.modalOpen = false;
    }
  }

})();
