/**
 * Monahrq Nest
 * Components Modals Module
 * Generic Modal
 *
 * Open a modal with caller-provided content:
 *   svc.open(title, content)
 *
 *  'content' may be HTML markup.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalGenericSvc', ModalGenericSvc);

  ModalGenericSvc.$inject = ['$modal', '$rootScope'];
  function ModalGenericSvc($modal, $rootScope) {
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
    function open(title, content) {
      $rootScope.modalOpen = true;

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_generic.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function ($scope) {
          $scope.title = title;
          $scope.content = content;
        }
      });

      modalInstance.result.then(onClose, onClose);
    }

    function onClose() {
      $rootScope.modalOpen = false;
    }
  }

})();
