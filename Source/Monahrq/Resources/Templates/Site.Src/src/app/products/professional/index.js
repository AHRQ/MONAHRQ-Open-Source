/**
 * Professional Product
 * Main Module
 * Setup
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional', [
    'angularCharts',
    'monahrq.products.professional.pages',
    'monahrq.products.professional.quality-ratings',
    'monahrq.products.professional.usage-data',
    'monahrq.products.professional.nursing-homes',
    'monahrq.products.professional.physicians'
  ]);

})();
