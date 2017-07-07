/**
 * Consumer Product
 * Pages Module
 * Header Section Controller
 *
 * This controller corresponds to the site-wide page header on the consumer site.
 * It handles requests for sharing, printing, and feedback.
 *
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.pages')
    .controller('CHeaderCtrl', CHeaderCtrl);


  CHeaderCtrl.$inject = ['$scope', 'ModalFeedbackSvc'];
  function CHeaderCtrl($scope, ModalFeedbackSvc) {
    $scope.share = share;
    $scope.print = print;
    $scope.feedbackModal = feedbackModal;

    init();


    function init() {
      var nbViz = true;

      if (!_.contains($scope.config.active_products, 'professional')) {
        nbViz = false;
      }

      $scope.notificationBar = {
        visible: nbViz
      };
    }

    function print() {
      //START: TICKET:MONNGBD-10 [Date is added at the top of print page]
      var d = new Date();
      $('#content').prepend('<div id="print-timestamp" class="text-right">'+d.toLocaleDateString()+'</div>');
      //END: TICKET:MONNGBD-10 [Date is added at the top of print page]
      window.print();
      //START: TICKET:MONNGBD-10 => Date is added at the top of print page
      $('#print-timestamp').remove();
      //END: TICKET:MONNGBD-10 [Date is added at the top of print page]
    }

    function share() {
      window.location = buildShareUrl();
    }

    function feedbackModal(){
      ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
      var url = escape(window.location);
      return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

  }

})();
