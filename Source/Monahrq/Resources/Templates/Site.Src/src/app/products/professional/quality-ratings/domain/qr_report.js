/**
 * Professional Product
 * Quality Ratings Domain Module
 * Quality Reports Report Loader Service
 *
 * This service loads hospital data use the Data Loader. It provides single
 * and bulk loaders for the following quality data:
 *
 * - Hospital Quality, all hospitals for a given measure
 * - Hospital Quality, all measures for a given hospital
 *
 * It predates the SimpleReportLoader.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings.domain')
    .factory('QRReportSvc', QRReportSvc);


  QRReportSvc.$inject = ['$q', 'DataLoaderSvc'];
  function QRReportSvc($q, DataLoaderSvc) {
    /**
     * Private Data
     */
    var opt = {
      reportDir: 'Data/QualityRatings'
    };

    $.monahrq = $.monahrq || {};
    $.monahrq.qualitydata = $.monahrq.qualitydata || {};


    /**
     * Service Interface
     */
    return {
      getReportByHospital: getReportByHospital,
      getReportByMeasure: getReportByMeasure,
      getReportsByMeasures: getReportsByMeasures,
      getReportsByHospitals: getReportsByHospitals
    };


    /**
     * Service Implementation
     */
    function getReportByHospital(id) {
      var deferred, url;
      deferred = $q.defer();

      url = opt.reportDir + '/Hospital/hospital_' + id + '.js';

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq.qualitydata['hospital_' + id];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        } else {
          data = _.map(data, function (r) {
            r.HospitalType = (r.HospitalType ? r.HospitalType.split(',') : r.HospitalType);
            return r;
          });
          return deferred.resolve(data);
        }

      }, function () {
      }, true);

      return deferred.promise;
    }

    function getReportByMeasure(id) {
      var deferred, url;
      deferred = $q.defer();

      url = opt.reportDir + '/Measure/measure_' + id + '.js';

      DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.qualitydata['measure_' + id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            data = _.map(data, function (r) {
              r.HospitalType = (r.HospitalType ? r.HospitalType.split(',') : []);
              r.HospitalType = _.map(r.HospitalType, function (typeId) {
                return +typeId;
              });
              return r;
            });
            return deferred.resolve(data);
          }

        },
        function (url) {
          return deferred.reject('Unable to load \"' + url + '\".');
        }, true);

      return deferred.promise;
    }

    function getReportsByMeasures(measures) {
      var deferred, promises;
      deferred = $q.defer();

      promises = _.map(measures, function (m) {
        return getReportByMeasure(m);
      });

      $q.all(promises).then(function (reports) {
          deferred.resolve(reports);
        },
        function (reason) {
          deferred.reject(reason);
        });

      return deferred.promise;
    }

    function getReportsByHospitals(hospitals) {
      var deferred, promises;
      deferred = $q.defer();

      promises = _.map(hospitals, function (h) {
        return getReportByHospital(h);
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
