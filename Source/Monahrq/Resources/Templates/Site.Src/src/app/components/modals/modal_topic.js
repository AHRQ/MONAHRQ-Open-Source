/**
 * Monahrq Nest
 * Components Modals Module
 * Topic Modal
 *
 * Show help content for the specified Topic.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalTopicSvc', ModalTopicSvc);

  ModalTopicSvc.$inject = ['$modal', '$rootScope'];
  function ModalTopicSvc($modal, $rootScope) {
    /**
     * Private Data
     */
    var modalInstance;
    var activeEl;

    /**
     * Service Interface
     */
    return {
      open: openHospitalTopic,
      openHospitalTopic: openHospitalTopic,
      openNursingTopic: openNursingTopic,
      openPhysicianTopic: openPhysicianTopic
    };

    /**
     * Service Implementation
     */
    function openNursingTopic(id) {
      open('nursing', id);
    }

    function openHospitalTopic(id) {
      open('hospital', id);
    }

    function openPhysicianTopic(id) {
      open('physician', id);
    }

    function open(source, id) {
      $rootScope.modalOpen = true;
      saveFocus();

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_topic.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function ($scope, measureTopics) {
          $scope.topic = _.findWhere(measureTopics, {TopicID: +id});
        },
        resolve: {
          measureTopics: function ($state, ResourceSvc) {
            if ($state.includes('top.consumer')) {
              if (source === 'nursing') {
                return ResourceSvc.getNursingHomeMeasureTopicsConsumer();
              }
              else if (source === 'physician') {
                return ResourceSvc.getMedicalPracticeMeasureTopics();
              }
              else {
                return ResourceSvc.getMeasureTopicsConsumer();
              }
            }
            else if ($state.includes('top.professional')) {
              if (source === 'hospital') {
                return ResourceSvc.getMeasureTopics();
              }
              else if (source === 'nursing') {
                return ResourceSvc.getNursingHomeMeasureTopics();
              }
              else if (source === 'physician') {
                return ResourceSvc.getMedicalPracticeMeasureTopics();
              }
            }
          }
        }
      });

      modalInstance.result.then(restoreFocus, restoreFocus);
    }

    function saveFocus() {
      activeEl = document.activeElement;
    }

    function restoreFocus() {
      $rootScope.modalOpen = false;
      if (activeEl) {
        activeEl.focus();
      }
    }
  }

})();
