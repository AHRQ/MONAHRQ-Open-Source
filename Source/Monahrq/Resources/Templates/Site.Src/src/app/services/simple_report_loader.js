/**
 * Monahrq Nest
 * Services Module
 * Simple Report Loader Service
 *
 * This service allows reports to be loaded in a configuration-driven manner. It supports
 * structures where a single directory contains a set of one or more data files,
 * which is typical of Wings. Its usage is described in detail in the documentation.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('SimpleReportLoaderSvc', SimpleReportLoaderSvc);


  SimpleReportLoaderSvc.$inject = ['$q', '$parse', 'DataLoaderSvc'];
  function SimpleReportLoaderSvc($q, $parse, DataLoaderSvc) {
    /**
     * Service Interface
     */
    return {
      load: load,
      bulkLoad: bulkLoad
    };


    /**
     * Service Implementation
     */


    /*
      'cfg' describes report as follows:
      {
        rootObj: $.monahrq.NursingHomes.Report,
        reportName: 'Measures',
        reportDir: 'Data/NursingHomes/Measures/',
        filePrefix: 'Measure_'
      };

      rootObj also supports an angular expression, eg "NursingHomes.Report". The context is set to $.monahrq.

      With a report file structured as follows:
      $.monahrq.NursingHomes.Report.Measures['1']=[
        {}, {}, {}
      ]
    */
    function load(cfg, id, processorFn) {
      var deferred, url, ns, sid;

      sid = id ? "" + id : '';
      deferred = $q.defer();

      //TODO: caching
      /*if (_.has(opt.reportRoot, cfg.reportName) && _.has(sid, opt.reportRoot[cfg.reportName])) {
        deferred.resolve(opt.reportRoot[cfg.reportName][sid]);
        return deferred.promise;
      }*/

      url = cfg.reportDir + cfg.filePrefix + sid + '.js';
      DataLoaderSvc.loadScript(url, function () {
          var root;
          if (_.isString(cfg.rootObj)) {
            root = $parse(cfg.rootObj)($.monahrq);
          }
          else {
            root = cfg.rootObj;
          }

          var data;
          if (id && cfg.idInKey == true) {
            var rn = cfg.reportName;
            if (cfg.reportPrefix) {
              rn = cfg.reportPrefix + sid;
            }
            data = root[rn];
          }
          else if (id) {
            data = root[cfg.reportName][sid]  ;
          }
          else {
            data = root[cfg.reportName];
          }

          if (_(data).isUndefined() || _(root).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            if (_.isFunction(processorFn)) {
              var data = processorFn(data);
            }
            return deferred.resolve({id: id, data: data});
          }
        },
        function (url) {
          // give caller a chance to handle missing files gracefully
          return deferred.resolve({id: id, data: null});
          //return deferred.reject('Unable to load \"' + url + '\".');
        }, true);

      return deferred.promise;
    }

    function bulkLoad(cfg, ids, processorFn) {
      var deferred, promises;
      deferred = $q.defer();

      promises = _.map(ids, function (id) {
        return load(cfg, id, processorFn);
      });

      $q.all(promises).then(function (reports) {
          deferred.resolve(reports);
        },
        function (reason) {
          deferred.reject(reason);
        });

      return deferred.promise;
    }

  }

})();
