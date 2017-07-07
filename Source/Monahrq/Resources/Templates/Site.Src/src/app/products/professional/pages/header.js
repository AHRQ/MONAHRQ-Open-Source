/**
 * Professional Product
 * Pages Module
 * Site Header Block
 *
 * This controller corresponds to the site-wide page header on the professional site.
 * It handles requests for sharing, printing, and feedback.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('HeaderCtrl', HeaderCtrl);

  HeaderCtrl.$inject = ['$scope', 'ModalFeedbackSvc'];
  function HeaderCtrl($scope, ModalFeedbackSvc) {
    $scope.shareUrl = buildShareUrl();
    init();

    function init() {
      var nbViz = true;

      if (!_.contains($scope.config.active_products, 'consumer')) {
        nbViz = false;
      }

      $scope.notificationBar = {
        visible: nbViz
      };
    }

    $scope.$on('LocationChange', function() {
      $scope.shareUrl = buildShareUrl();
    });

    $scope.print = function() {
      //START: TICKET:MONNGBD-10 [Date is added at the top of print page]
      var d = new Date();
      $('#content').prepend('<div id="print-timestamp" class="text-right" style="margin:10px 5px 0 0;">'+d.toLocaleDateString()+'</div>');
      //END: TICKET:MONNGBD-10 [Date is added at the top of print page]
      window.print();
      //START: TICKET:MONNGBD-10 [Date is added at the top of print page]
      $('#print-timestamp').remove();
      //END: TICKET:MONNGBD-10 [Date is added at the top of print page]
    };

    function buildShareUrl() {
      var url = escape(window.location);
      return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    $scope.share = function() {
      window.location = buildShareUrl();
    };

    $scope.feedbackModal = function(){
      ModalFeedbackSvc.open($scope.config);
    };
  }
})();

