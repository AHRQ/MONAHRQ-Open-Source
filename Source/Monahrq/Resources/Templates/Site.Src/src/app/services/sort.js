/**
 * Monahrq Nest
 * Services Module
 * Sort Service
 *
 * Provides utility functions for sorting arrays of javascript objects by key. objSort
 * is used to sort by a field on the object in the array. objSort2 is used to sort
 * by a field on a child object of that object.
 *
 * The numeric variants handle the special cases where the values will either be a number or
 * the '-' symbol, which has special meaning in monahrq Wings.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('SortSvc', SortSvc);


  function SortSvc() {
    /**
     * Service Interface
     */
    return {
      objSort: objSort,
      objSort2: objSort2,
      objSortNumeric: objSortNumeric,
      objSort2Numeric: objSort2Numeric
    };


    /**
     * Service Implementation
     */
    function objSortNumeric(list, field, dir) {
      if (list.length === 0) return;

      list.sort(function(a, b) {
        // force 'not enough data' values to sort to end of list
        if (a[field] == '-') {
          return 1;
        }
        if (b[field] == '-') {
          return -1;
        }

        if (dir === 'asc') {
          return a[field] - b[field];
        }
        else {
          return b[field] - a[field];
        }
      });
    }

    function objSort(list, field, dir) {
      if (list.length === 0) return;

      if (typeof list[0][field] === 'number') {
        list.sort(function(a, b) {
          if (dir === 'asc') {
            return a[field] - b[field];
          }
          else {
            return b[field] - a[field];
          }
        });
      }
      else {
        list.sort(function(a, b) {
          var av = a[field],
            bv = b[field],
            x = (dir === 'asc' ? 1 : -1);

          if (av > bv) return x;
          else if (av < bv) return -x;
          return 0;
        });
      }
    }

    function objSort2(list, field, field2, dir) {
      if (list.length === 0) return;

      if (typeof list[0][field][field2] === 'number') {
        list.sort(function(a, b) {
          if (dir === 'asc') {
            return a[field][field2] - b[field][field2];
          }
          else {
            return b[field][field2] - a[field][field2];
          }
        });
      }
      else {
        list.sort(function(a, b) {
          var av = a[field][field2],
            bv = b[field][field2],
            x = (dir === 'asc' ? 1 : -1);

          if (av > bv) return x;
          else if (av < bv) return -x;
          return 0;
        });
      }
    }

    function objSort2Numeric(list, field, field2, dir) {
      if (list.length === 0) return;

      list.sort(function(a, b) {
        // force 'not enough data' values to sort to end of list
        if (a[field][field2] == '-') {
          return 1;
        }
        if (b[field][field2] == '-') {
          return -1;
        }

        if (dir === 'asc') {
          return a[field][field2] - b[field][field2];
        }
        else {
          return b[field][field2] - a[field][field2];
        }
      });
    }
  }

})();

