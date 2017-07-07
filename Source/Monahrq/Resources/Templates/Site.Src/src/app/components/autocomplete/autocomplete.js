/**
 * Monahrq Nest
 * Components Module
 * Autocomplete Directive
 *
 * The Autocomplete directive wraps UI-Autocomplete to add accessbility and browser
 * compatibility enhancements.
 *
 * <div data-mh-autocomplete="config" selected-id="selection"></div>
 *
 * $scope.selection = null;
 * $scope.config = {
 *  rowLabel: 'label',
 *  rowId: 'id',
 *  widgetId: 'uia-things',
 *  widgetTitle: 'Select a Thing',
 *  defaultLabel: null,
 *  hasAll: false,
 *  excludeZero: true,
 *  data: [{id: 1, label: 'name'}]
 * };
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.autocomplete', [])
    .directive('mhAutocomplete', autocomplete);


  function autocomplete() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        config: '=mhAutocomplete',
        selectedId: '='
      },
      templateUrl: 'app/components/autocomplete/views/autocomplete.html',
      link: link,
      controller: controller
    };

    /**
     * Directive Link
     */
    function link(scope, elem, attrs) {
      // this is a workaround for ie 10+ where the placeholder text triggers the input event
      var $textbox = elem.find('.mh-autocomplete-text');
      $textbox.on('focus', function() {
        $textbox.autocomplete('option', 'minLength', 0);
        $textbox.autocomplete('search', '');
      });
    }

    /**
     * Directive Controller
     */
    function controller($scope, $element) {
      var focusAllowed = false, lastClosed = null;

      function source(request, response) {
        var data = _.filter($scope.config.data, function(row) {
          var filter;

          if ($scope.config.excludeZero && +row[$scope.config.rowId] === 0) {
            filter = false;
          }
          else {
            var val = _.has(row, $scope.config.rowLabel) && row[$scope.config.rowLabel] != null ? row[$scope.config.rowLabel] : "";
            var term = request.term ? request.term : "";
            filter = term === "" || val.toLowerCase().indexOf(term.toLowerCase()) >= 0;
          }

          return filter;
        });

        var rdata = _.map(data, function(row) {
          return {
            value: row[$scope.config.rowLabel],
            id: row[$scope.config.rowId],
            category: row.category
          };
        });

        if (!rdata.length) {
          rdata.push({
              value: 'Not Found',
              id: null
          });
        }

        if ($scope.config.hasAll) {
          rdata.unshift({
            value: 'All',
            id: $scope.config.allId ? $scope.config.allId : '0'
          });
        }

        response(rdata);
      }

      function onSelect(event, ui) {
        event.preventDefault();

        $scope.uia.selectedLabel = ui.item.value;
        $scope.selectedId = ui.item.id;

        if ($scope.config.onChange) {
          $scope.config.onChange($scope.selectedId);
        }

        $scope.$apply();
      }

      function onFocus(event, ui) {
        event.preventDefault();

        if (focusAllowed) {
          console.log(ui.item.value);
          $scope.uia.selectedLabel = ui.item.value;
        }

        $scope.$apply();
      }

      function onOpen(event, ui) {
        // IE 10 will fire a second set of open/focus/close events after a selection is made. debouncing to ignore it.
        if (lastClosed === null || event.timeStamp - lastClosed > 300) {
          focusAllowed = true;
        }

        setTimeout(function() {
          $(event.target).parent().find('.ui-autocomplete').css({
            "width": ($(event.target).outerWidth() + "px")
          });
        }, 20);
      }

      function onClose(event) {
        focusAllowed = false;
        lastClosed = event.timeStamp;
        $element.find('.hiddenFocus').focus();
      }

      function _create() {
        this._super();
        this.widget().menu( "option", "items", "> :not(.ui-autocomplete-category)" );
      }

      function _renderMenu( ul, items ) {
        var that = this,
          currentCategory = "";

        $.each( items, function( index, item ) {
          var li;

          if (item.category !== undefined && item.category !== currentCategory ) {
            ul.append( "<li class='ui-autocomplete-category'>" + item.category + "</li>" );
            currentCategory = item.category;

            if (!ul.hasClass('ui-autocomplete-categories')) {
              ul.addClass('ui-autocomplete-categories');
            }
          }

          li = that._renderItemData( ul, item );

          if ( item.category !== undefined ) {
            li.attr( "aria-label", item.category + " : " + item.label );
          }
        });

        ul.removeAttr('tabindex');
      }

      $.widget( "ui.autocomplete", $.ui.autocomplete, {
        _create: _create,
        _renderMenu: _renderMenu
      });

      $scope.$watch('selectedId', function(nv, ov) {
        if (nv === null) {
          $scope.uia.selectedLabel = null;
        }
      });

      $scope.uia = {};
      $scope.uia.selectedLabel = $scope.config.defaultLabel;

      if ($scope.config.hasAll && $scope.config.defaultLabel === null && $scope.selectedId == ($scope.config.allId ? $scope.config.allId : '0')) {
        $scope.uia.selectedLabel = 'All';
      }

      $scope.uia.config = {
        options: {
          source: source,
          onlySelect: true,
          scroll: true,
          focusOpen: false,
          minLength: 999
        },
        events: {
          select: onSelect,
          focus: onFocus,
          open: onOpen,
          close: onClose
        }
      };
    }
  }
})();
