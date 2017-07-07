/**
 * Monahrq Nest
 * Services Module
 * Flutter Loader Service
 *
 * This module loads and initializes any flutters that have been specified in the flutter registry.
 * Normally Angular expects all dependencies to be specified before it bootstraps; this uses the
 * ocLazyLoad library to work around that to load controllers, services, etc after bootstrap.
 *
 * The flutter registry is a data file listing all flutters the host user wishes to use. Each
 * flutter then has a config file which specifies the code, templates, and styles it provides.
 * Further details can be found in the Flutter documentation.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('FlutterLoaderSvc', FlutterLoaderSvc);


  FlutterLoaderSvc.$inject = ['$q', '$ocLazyLoad', 'ResourceSvc', 'DataLoaderSvc', 'FlutterConfigSvc', 'MenuSvc'];
  function FlutterLoaderSvc($q, $ocLazyLoad, ResourceSvc, DataLoaderSvc, FlutterConfigSvc, MenuSvc) {
    /**
     * Service Interface
     */
    return {
      loadAll: loadAll
    };


    /**
     * Service Implementation
     */
    function loadAll() {
      var deferred;
      deferred = $q.defer();

      ResourceSvc.getFlutterRegistry()
        .then(function(registry) {
          var promises = [];

          _.each(registry.flutters, function(flutter) {
            promises.push(loadFlutterConfig(flutter));
          });

          $q.all(promises)
            .then(function(configs) {
              deferred.resolve(configs);
            });
        });

      return deferred.promise;
    }

    function loadFlutterConfig(flutter) {
      var deferred;
      deferred = $q.defer();

      DataLoaderSvc.loadScript(flutter.modulePath + '/' + flutter.configFilename, function() {
        var config;
        var flutterKeys = _.keys($.monahrq.Flutter.Configs);
        for (var i = 0; i < flutterKeys.length; i++) {
          var x = $.monahrq.Flutter.Configs[flutterKeys[i]];
          if (x.id == flutter.configId) {
            config = x;
            break;
          }
        }

        if (config) {
          FlutterConfigSvc.add(config);
          loadFlutter(flutter, config)
            .then(function() {
              MenuSvc.addFlutter(config);
              deferred.resolve(config);
            });
        }
        else {
          JL("services.FlutterLoaderSvc").error("Flutter " + flutter.configId + " was not found");
        }
      });

      return deferred.promise;
    }

    function loadFlutter(flutter, config) {
      var files = [];

      _.each(config.assets.scripts, function(path) {
        files.push(flutter.modulePath + '/' + path);
      });

      _.each(config.assets.templates, function(path) {
        files.push(flutter.modulePath + '/' + path);
      });

      _.each(config.assets.styles, function(path) {
        files.push(flutter.modulePath + '/' + path);
      });

      return $ocLazyLoad.load({
        name: config.moduleName,
        files: files
      });
    }

  }

})();
