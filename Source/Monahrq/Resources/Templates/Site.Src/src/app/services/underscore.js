/**
 * Monahrq Nest
 * Services Module
 * Underscore wrapper
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('underscore', [])
    .factory('_', function () {
      return window._;
    });

})();

