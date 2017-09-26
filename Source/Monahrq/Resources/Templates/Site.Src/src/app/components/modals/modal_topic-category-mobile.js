/**
 * Monahrq Nest
 * Components Modals Module
 * Topic Category Modal
 *
 * Show help content for the specified Topic Category.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalTopicCategorySvc', ModalTopicCategorySvc);

  ModalTopicCategorySvc.$inject = ['$modal', '$rootScope'];
  function ModalTopicCategorySvc($modal, $rootScope) {
    /**
     * Private Data
     */
    var modalInstance;
    var activeEl;

    /**
     * Service Interface
     */
    return {
      open: openHospitalTopicCategory,
      openHospitalTopicCategory: openHospitalTopicCategory,
      openPhysicianTopicCategory: openPhysicianTopicCategory
    };

    /**
     * Service Implementation
     */
    function openHospitalTopicCategory(id) {
      open('hospital', id);
    }

    function openPhysicianTopicCategory(id) {
      open('physician', id);
    }

    function open(source, id) {
      $rootScope.modalOpen = true;
      saveFocus();

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_topic-category-mobile.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function($state, $scope, measureTopicCategories) {
          $scope.topic = _.findWhere(measureTopicCategories, {TopicCategoryID: +id});

          if ($scope.topic.Name === "Adult Surveys" || $scope.topic.Name === "Child Surveys") {
              $scope.topic.LongTitle += "  To learn more information about how the rates are calculated, review the About the Ratings page."
          }

          $scope.isConsumer = function() {
            return $state.includes('top.consumer') && source != 'physician';
          };

          $scope.isProfessional = function() {
            return $state.includes('top.professional') || source == 'physician';
          };
        },
        resolve: {
          measureTopicCategories: function($state, ResourceSvc) {
            if (source === 'physician') {
              return ResourceSvc.getMedicalPracticeMeasureTopicCategories();
            }

            if ($state.includes('top.consumer')) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            }
            else if ($state.includes('top.professional')) {
              return ResourceSvc.getMeasureTopicCategories();
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
