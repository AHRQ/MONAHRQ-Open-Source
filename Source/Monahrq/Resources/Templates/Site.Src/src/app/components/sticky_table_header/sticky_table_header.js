/**
 * Monahrq Nest
 * Components Module
 * Sticky Table Header Directive
 *
 * Cause the header of a table to remain visible on-screen when the user scrolls the browser
 * past it.
 *
 * <table mh-sticky-table-header="modelToWatch">
 *   <thead><tr><th>Col</th></tr></thead>
 * </table>
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module("monahrq.components.sticky-table-header",[])
    .directive("mhStickyTableHeader", stickyTableHeader);


  stickyTableHeader.$inject = ["$window", "$compile"];
  function stickyTableHeader($window, $compile) {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      link: link
    };

    /**
     * Directive Link
     */
    function link(scope, element, attrs) {
      var hasRun = false;

      scope.$watch(attrs.mhStickyTableHeader, function(nv, ov) {
        if ((nv === ov && hasRun) || nv === undefined ) {
          return;
        }

        hasRun = true;
        makeSticky.call(jQuery(element), scope);
      });
    }

    function makeSticky(scope) {
      if($(this).find('thead').length > 0 && $(this).find('th').length > 0) {
        // Clone <thead>
        var $w	   = $(window),
          $t	   = $(this);

        // if we're reapplying the sticky, remove the old wrapper and fixed header
        if ($t.parent().hasClass('sticky-wrap')) {
          $t.parent().find('.sticky-thead').remove();
          $t.unwrap();
        }

        var $thead = $t.find('thead').clone(),
          $col   = $t.find('thead, tbody').clone();

        // if we're reapplying, we need to clean out any old ng-repeats to prevent recursing
        $thead.find('[data-ng-repeat]').not(':first').remove();
        $thead.find('tr').contents().filter(function() {
          return this.nodeType == 8; // we're remove comments
        }).remove();

        for (var i = 0; i < $col.length; i++) {
          jQuery($col[i]).find('[data-ng-repeat]').not(':first').remove();
          jQuery($col[i]).find('tr').contents().filter(function() {
            return this.nodeType == 8; // we're remove comments
          }).remove();
        }

        // Add class, remove margins, reset width and wrap table
        $t
          .addClass('sticky-enabled')
          .css({
            margin: 0,
            width: '100%',
            position: 'relative',
            'z-index': '50'
          }).wrap('<div class="sticky-wrap" />');

        if($t.hasClass('overflow-y')) $t.removeClass('overflow-y').parent().addClass('overflow-y');

        // Create new sticky table head (basic)
        $t.after('<table class="sticky-thead" />');

        // If <tbody> contains <th>, then we create sticky column and intersect (advanced)
        /*if($t.find('tbody th').length > 0) {
          $t.after('<table class="sticky-col" /><table class="sticky-intersect" />');
        }*/

        // Create shorthand for things
        var $stickyHead  = $(this).siblings('.sticky-thead'),
          //$stickyCol   = $(this).siblings('.sticky-col'),
          //$stickyInsct = $(this).siblings('.sticky-intersect'),
          $stickyWrap  = $(this).parent('.sticky-wrap');

        $stickyHead.append($compile($thead)(scope));
        //[MONNGBD-20] left:0 and right: 0 is removed to fixed sticky header alignment issue in IE
        $stickyHead.css({
          position: 'fixed',
          top: 0,
          margin: '0 auto'
        });

        /*$stickyCol
          .append($compile($col)(scope))
          .find('thead th:gt(0)').remove()
          .end()
          .find('tbody td').remove();*/

        //$stickyInsct.html($compile('<thead><tr><th>'+$t.find('thead th:first-child').html()+'</th></tr></thead>')(scope));

        // Set widths
        var setWidths = function () {
            $t.find('thead th').each(function (i) {
                $stickyHead.find('th').eq(i).width($(this).width());
            });

            // Set width of sticky table head
            $stickyHead.width($t.width());

              // disabling fixed column due to repaint perf issues
              /*.end()
              .find('tr').each(function (i) {
                $stickyCol.find('tr').eq(i).height($(this).height());
              });*/


            // Set width of sticky table col
            //$stickyCol.find('th').add($stickyInsct.find('th')).width($t.find('thead th').width())
          },
          repositionStickyHead = function () {
            // Return value of calculated allowance
            var allowance = calcAllowance();

            if($w.scrollTop() > $t.offset().top && $w.scrollTop() < $t.offset().top + $t.outerHeight() - allowance) {
              //START:[MONNGBD-20]: alignment issue is solved for fixed header on page scroll
              var top_distance = $(document).scrollTop() - $('table.sticky-enabled').offset().top;
              $stickyHead.css({
                opacity: 1,
                'z-index': 100,
                position: 'absolute',
                top: top_distance+'px',
                'table-layout': 'fixed',
              });
              //END:[MONNGBD-20]: alignment issue is solved for fixed header on page scroll
            }
            else {
              $stickyHead.css({
                opacity: 0,
                'z-index': 0,
                position: 'absolute',
                top: '-999px'
              });
            }

            /*
            // Check if wrapper parent is overflowing along the y-axis
            if($t.height() > $stickyWrap.height()) {
              // If it is overflowing (advanced layout)
              // Position sticky header based on wrapper scrollTop()
              if($stickyWrap.scrollTop() > 0) {
                // When top of wrapping parent is out of view
                $stickyHead.add($stickyInsct).css({
                  opacity: 1,
                  'z-index': 100,
                  top: $stickyWrap.scrollTop()
                });
              } else {
                // When top of wrapping parent is in view
                $stickyHead.add($stickyInsct).css({
                  opacity: 0,
                  'z-index': 0,
                  top: 0
                });
              }
            } else {
              // If it is not overflowing (basic layout)
              // Position sticky header based on viewport scrollTop
              if($w.scrollTop() > $t.offset().top && $w.scrollTop() < $t.offset().top + $t.outerHeight() - allowance) {
                // When top of viewport is in the table itself
                $stickyHead.add($stickyInsct).css({
                  opacity: 1,
                  'z-index': 100,
                  top: $w.scrollTop() - $t.offset().top
                });
              } else {
                // When top of viewport is above or below table
                $stickyHead.add($stickyInsct).css({
                  opacity: 0,
                  'z-index': -999,
                  top: 0
                });
              }
            }
            */
          },
          /*repositionStickyCol = function () {
            if($stickyWrap.scrollLeft() > 0) {
              // When left of wrapping parent is out of view
              $stickyCol.add($stickyInsct).css({
                opacity: 1,
                'z-index': 100,
                left: $stickyWrap.scrollLeft()
              });
            } else {
              // When left of wrapping parent is in view
              $stickyCol
                .css({
                  opacity: 0,
                  'z-index': 0
                })
                .add($stickyInsct).css({ left: 0 });
            }
          },*/
          calcAllowance = function () {
            var a = 0;
            // Calculate allowance
            $t.find('tbody tr:lt(0)').each(function () {
              a += $(this).height();
            });

            // Set fail safe limit (last three row might be too tall)
            // Set arbitrary limit at 0.25 of viewport height, or you can use an arbitrary pixel value
            if(a > $w.height()*0.25) {
              a = $w.height()*0.25;
            }

            // Add the height of sticky header
            a += $stickyHead.height();
            return a;
          };

        setWidths();

        setTimeout(function() {
          setWidths();
          repositionStickyHead();
          //repositionStickyCol();
        }, 0);

        $t.parent('.sticky-wrap').scroll(/*$.throttle(250,*/ function() {
          repositionStickyHead();
          //repositionStickyCol();
        });//);

        $w
          .load(setWidths)
          .resize(/*$.debounce(250,*/ function () {
            setWidths();
            repositionStickyHead();
            //repositionStickyCol();
          })//)
          .scroll(repositionStickyHead); //$.throttle(250, repositionStickyHead));
      }

    }

  }
})();

