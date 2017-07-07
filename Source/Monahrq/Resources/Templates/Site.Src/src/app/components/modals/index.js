/**
 * Monahrq Nest
 * Components Modals Module
 * Setup
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals', [])

  .directive('modalFocus', function() {
    var directive = {
      restrict: 'EA',
      scope: {
      },
      link: link
    };
    return directive;

    function link(scope, elem, attrs) {
      jQuery(elem).keydown(checkFocus);

      function checkFocus($event) {
        if ($event.keyCode === 9) {
          var tabs = jQuery('.modal-dialog :tabbable');
          var idx = tabs.index($event.target);
          if (idx === 0 && $event.shiftKey) {
            tabs.eq(tabs.length - 1).focus();
            $event.preventDefault();
          }
          else if (idx === tabs.length - 1 && !$event.shiftKey) {
            tabs.eq(0).focus();
            $event.preventDefault();
          }
        }
      }
    }
  });

})();
