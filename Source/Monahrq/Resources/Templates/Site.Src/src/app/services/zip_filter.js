/**
 * Monahrq Nest
 * Services Module
 * Zip Code Filter
 *
 * Format 5+4 zip codes with a dash.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .filter('zip', zip);


  zip.$inject = ['$filter'];
  function zip($filter) {
    return function (code) {
      if (code && code.length == 5) {
        return code;
      }
      else if (code && code.length == 9) {
        return code.substring(0, 5) + '-' + code.substring(5);
      }
    }
  }

})();


