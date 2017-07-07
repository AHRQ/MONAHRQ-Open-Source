/**
 * Monahrq Nest
 * Services Module
 * User State Service
 *
 * Store user preferences for the duration of their session.
 *
 * Currently it is used to remember if the user prefers viewing stats at the
 * state or national level.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('UserStateSvc', UserStateSvc);


  UserStateSvc.$inject = [];
  function UserStateSvc() {
    var state = {};

    /**
     * Service Interface
     */
    return {
      get: get,
      set: set,
      clear: clear,

      props: {
        C_GEO_CONTEXT_HOSPITAL: 1,
        C_GEO_CONTEXT_NURSING: 2
      }
    };


    /**
     * Service Implementation
     */
    function init() {
    }

    function get(k) {
      if (_.has(state, k)) {
        return state[k];
      }

      return undefined;
    }

    function set(k, v) {
      state[k] = v;
    }

    function clear() {
      state = {};
    }

  }

})();

