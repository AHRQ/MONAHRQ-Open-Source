/**
 * Monahrq Nest
 * Services Module
 * Scroll To El Service
 *
 * Given a jQuery selector, this will smoothly scroll the browser window from
 * its current vertical position to that of the specified element.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('ScrollToElSvc', ScrollToElSvc);


  function ScrollToElSvc() {
    /**
     * Service Interface
     */
    return {
      scrollToEl: scrollToEl
    };


    /**
     * Service Implementation
     */
    function scrollToEl(sel) {
      var $el = $(sel);
      jQuery('html, body').animate({
        scrollTop: $el.offset().top
      }, 300);
      $el.focus();
    }

  }

})();

