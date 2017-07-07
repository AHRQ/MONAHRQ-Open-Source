/**
 * Professional Product
 * Usage Data Domain Module
 * AHS Report Loader Service
 *
 * This service loads avoidable hospital stays data use the Data Loader. It provides single
 * and bulk loaders for the following usage data:
 *
 * - get AHS report
 *
 */
(function(ng, _) {
  'use strict';

  ng.module('monahrq.domain').service('UDAhsReportSvc', ['$q', 'DataLoaderSvc', function ($q, DataLoaderSvc) {
    var opt = {
      reportDir: 'Data/Reports',
    };

    $.monahrq = $.monahrq || {};

    function getUrl() {
      return  opt.reportDir + '/ahs.js';
    }

    function getReport() {
      var deferred, report, url;
      deferred = $q.defer();

      url = getUrl();

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq['ahs'];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        } else {
          return deferred.resolve(data);
        }

      }, function() {}, true);

      return deferred.promise;
    }

    return {
      getReport: getReport
    }

  }]);

}(angular, _));
