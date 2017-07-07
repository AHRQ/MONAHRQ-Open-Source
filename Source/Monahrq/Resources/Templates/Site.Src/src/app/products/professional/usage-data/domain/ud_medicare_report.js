/**
 * Professional Product
 * Usage Data Domain Module
 * Medicare Report Loader Service
 *
 * This service loads medicare data use the Data Loader. It provides single
 * and bulk loaders for the following usage data:
 *
 * - get Medicare report for hospital
 *
 */
(function(ng, _) {
  'use strict';

  ng.module('monahrq.domain').service('UDMedicareReportSvc', ['$q', 'DataLoaderSvc', function ($q, DataLoaderSvc) {
    var opt = {
      reportDir: 'Data/medicare',
    };

    $.monahrq = $.monahrq || {};

    function getUrl(id) {
      return  opt.reportDir + '/drg/hospital/Hospital_' + id + '/summary.js';
    }

    function getReport(id) {
      var deferred, report, url;
      deferred = $q.defer();

      url = getUrl(id);

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq['medicareprovidercharge'];

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
