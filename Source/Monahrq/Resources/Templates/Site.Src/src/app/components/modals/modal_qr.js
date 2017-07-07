/**
 * Monahrq Nest
 * Components Modals Module
 * Measure Modal
 *
 * The professional site's quality ratings reports have some special help content provided in
 * the ModalQR base data file. This modal is used to display it.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalQRSvc', ModalQRSvc);


  ModalQRSvc.$inject = ['$modal', '$rootScope'];
  function ModalQRSvc($modal, $rootScope) {
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
    function open(key) {
      $rootScope.modalOpen = true;

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_qr.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function($scope, modalData) {
          $scope.content = modalData[key];
        },
        resolve: {
          modalData: function(ResourceSvc) {
            return ResourceSvc.getmodalQR();
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
