/**
 * Monahrq Nest
 * Core Domain Module
 * Flutter Config Service
 *
 * A simple service for storing and making each flutter's config available to other
 * modules for use.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('FlutterConfigSvc', FlutterConfigSvc);


  FlutterConfigSvc.$inject = [];
  function FlutterConfigSvc() {
    var configs = {};

    return {
      get: get,
      getAll: getAll,
      getIds: getIds,

      add: add
    };


    function get(id) {
      if (_.has(configs, id)) {
        return configs[id];
      }

      return null;
    }

    function getAll() {
      return _.values(configs);
    }

    function getIds() {
      return _.keys(configs);
    }

    function add(config) {
      configs[config.id] = config;
    }

  }

})();
