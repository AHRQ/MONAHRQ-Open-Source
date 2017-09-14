/**
 * Consumer Product
 * Main Module
 * Setup
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer', [
    'monahrq.products.consumer.components',
    'monahrq.products.consumer.pages',
    'monahrq.products.consumer.hospitals',
    'monahrq.products.consumer.nursing-homes',
    'monahrq.products.consumer.physicians',
    'ng.deviceDetector'
  ]);

})();
