/**
 * Monahrq Nest
 * Components Module
 * Internal Link Directive
 *
 * This is a legacy directive to scroll to a section of a page. Superseded by the scrollto directive.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module("monahrq.components.internal-link",[])
    .directive("mhInternalLink", internalLink);


  scrollTo.$inject = ["$window"];
  function internalLink($window) {
    /**
     * Directive Definition
     */
    return {
      restrict: "AC",
      compile: compile
    };

    /**
     * Directive Compile
     */
    function compile() {
      var document = $window.document;

      function scrollInto(idOrName) {//find element with the given id or name and scroll to the first element it finds
        if (!idOrName) //move to top if idOrName is not provided
          $window.scrollTo(0, 0);

        //check if an element can be found with id attribute
        var el = document.getElementById(idOrName);
        if (!el) {//check if an element can be found with name attribute if there is no such id
          el = document.getElementsByName(idOrName);

          if (el && el.length)
            el = el[0];
          else
            el = null;
        }

        if (el) {//if an element is found, scroll to the element
          el.scrollIntoView();
          el.focus();
        }
        //otherwise, ignore
      }

      return function (scope, element, attr) {
        element.bind("click", function (event) {
          event.preventDefault();
          scrollInto(attr.mhInternalLink);
        });
      };
    }
  }

})();

