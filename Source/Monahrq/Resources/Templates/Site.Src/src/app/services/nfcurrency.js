/**
 * Monahrq Nest
 * Services Module
 * Currency Filter
 *
 * This extends the functionality of Angular's built-in currency filter.
 * It removes cents from a currency value, so only whole dollars are rendered.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .filter('nfcurrency', nfcurrency);


  nfcurrency.$inject = ['$filter', '$locale'];
  function nfcurrency($filter, $locale) {
    var currency = $filter('currency'), formats = $locale.NUMBER_FORMATS;

    return function (amount, symbol) {
      var value = currency(amount, symbol) || '';
      return value.replace(new RegExp('\\' + formats.DECIMAL_SEP + '\\d{2}'), '')
    }
  }

})();


