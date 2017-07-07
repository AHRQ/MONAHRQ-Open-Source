/**
 * Monahrq Nest
 * Components Modals Module
 * Feedback Modal
 *
 * Open the feedback modal. 'config' is the website_config object.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalFeedbackSvc', ModalFeedbackSvc);

  ModalFeedbackSvc.$inject = ['$modal', '$state', '$rootScope'];
  function ModalFeedbackSvc($modal, $state, $rootScope) {
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
    function isProfessional() {
      return $state.includes('top.professional');
    }

    function open(config) {
      $rootScope.modalOpen = true;

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_feedback.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function($scope){
          var pc = config.products[isProfessional() ? 'professional' : 'consumer'];
          var targetEmail = pc.website_FeedBackEmail;
          $scope.feedbackTopics = pc.website_FeedbackTopics;
          $scope.feedbackUrl = pc.website_FeebackUrl;
          $scope.model;
          $scope.linkurl;

          $scope.setURL = function(topic){
            $scope.linkurl = "mailto:" + targetEmail + "?subject=" + topic;
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
