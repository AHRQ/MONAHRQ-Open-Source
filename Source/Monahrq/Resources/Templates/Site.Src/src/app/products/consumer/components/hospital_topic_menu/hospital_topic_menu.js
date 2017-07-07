/**
 * Consumer Products
 * Components Module
 * Hospital Topic Menu Directive
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.components.hospital-topic-menu', [])
    .directive('mhcHospitalTopicMenu', hospitalTopicMenu);

  function hospitalTopicMenu() {
    controller.$inject = ['$scope'];

    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
      },
      templateUrl: 'app/products/consumer/components/hospital_topic_menu/views/hospital_topic_menu.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope) {
      init();

      function init() {
      }
    }
  }

})();

