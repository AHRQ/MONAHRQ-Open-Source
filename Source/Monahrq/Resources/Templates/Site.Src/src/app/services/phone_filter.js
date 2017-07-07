/**
 * Monahrq Nest
 * Services Module
 * Phone Number Filter
 *
 * This formats 7 and 10 digit phone numbers. Given a number such as
 * 5555551212 it will return 555-555-1212.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .filter('phone', phone);


  phone.$inject = ['$filter'];
  function phone($filter) {
    return function (num) {
      if (num && num.length == 7) {
        return num.substring(0, 3) + '-' + num.substring(3);
      }
      else if (num && num.length == 10) {
        return num.substring(0, 3) + '-' + num.substring(3, 6) + '-' + num.substring(6);
      }
    }
  }

})();


