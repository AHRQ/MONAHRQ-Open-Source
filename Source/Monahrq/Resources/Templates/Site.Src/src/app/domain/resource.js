/**
 * Monahrq Nest
 * Core Domain Module
 * Resource Loader
 *
 * The ResourceSvc provides methods for loading base data files. Where possible,
 * loader functions are dynamically added to the service interface via the 'resources'
 * array. However, there are a number of hand-coded loaders due to each having particular
 * complexity or requirements that precludes their inclusion.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('ResourceSvc', ResourceSvc);


  ResourceSvc.$inject = ['$q', 'DataLoaderSvc', 'SortSvc'];
  function ResourceSvc($q, DataLoaderSvc, SortSvc) {
      var opt = {
          baseDir: 'Data/Base',
          countyGeo: 'countyGeo_',
          regionGeo: 'Geo_'
        },
        API = {},
        resources;

      $.monahrq = $.monahrq || {};
      $.monahrq.cms = $.monahrq.cms || {};
      $.monahrq.qualitydata = $.monahrq.qualitydata || {};
      $.monahrq.costdata = $.monahrq.costdata || {};
      $.monahrq.utilization = $.monahrq.utilization || {};
      $.monahrq.NursingHomes = $.monahrq.NursingHomes || {};
      $.monahrq.NursingHomes.Base = $.monahrq.NursingHomes.Base || {};
      $.monahrq.NursingHomes.Base.Profiles = $.monahrq.NursingHomes.Base.Profiles || {};
      $.monahrq.NursingHomes.Base.MeasureDescriptions= $.monahrq.NursingHomes.Base.MeasureDescriptions || {};
      $.monahrq.Physicians = $.monahrq.Physicians || {};
      $.monahrq.Physicians.Base = $.monahrq.Physicians.Base || {};
      $.monahrq.Physicians.Base.MeasureDescriptions = $.monahrq.Physicians.Base.MeasureDescriptions || {};
      $.monahrq.MedicalPractices = $.monahrq.MedicalPractices || {};
      $.monahrq.MedicalPractices.Base = $.monahrq.MedicalPractices.Base || {};
      $.monahrq.MedicalPractices.Base.MeasureDescriptions = $.monahrq.MedicalPractices.Base.MeasureDescriptions || {};
      $.monahrq.MedicalPractices = $.monahrq.MedicalPractices || {};
      $.monahrq.MedicalPractices.Report = $.monahrq.MedicalPractices.Report || {};
      $.monahrq.MedicalPractices.Report.CGCAHPS = $.monahrq.MedicalPractices.Report.CGCAHPS || {};
      $.monahrq.MedicalPractices.Report.CGCAHPS.MedicalPractice = $.monahrq.MedicalPractices.Report.CGCAHPS.MedicalPractice || {};
      $.monahrq.Flutter = $.monahrq.Flutter || {};
      $.monahrq.Flutter.Configs = $.monahrq.Flutter.Configs || {};
      $.monahrq.Flutter.Base = $.monahrq.Flutter.Base || {};

      resources = ['Age', 'Sex', 'Payer', 'Race', 'Stratification',
        'HospitalTypes', 'HospitalZips',
        'ctrlProcedures', 'ctrlDiagnosisRelatedGroups', 'ctrlDiagnosisCategories',
        'ahsTopics', 'udDimensions', 'modalQR',
        'layout', 'pgHome', 'pgAboutUs', 'pgResources', 'pgQualityRatings',
        'pgQRProfile', 'pgQRLocation', 'pgQRCondition', 'pgQRCompare',
        'pgUsageData', 'pgUDReportED', 'pgUDReportID', 'pgUDReportCounty', 'pgUDReportAHS',
        'pgUDReportRegion',
        'ReportConfig', 'ConsumerReportConfig', 'Menu'
      ];

      _(resources).each(function (resource) {
        var methodName = 'get' + resource; //resource.substring(0, 1).toUpperCase() + resource.substring(1);
        API[methodName] = function () {
          return getResource({resource: resource});
        }
      });


      function endpoint() {
        return opt.baseDir + '/' + Array.prototype.slice.call(arguments).join('/') + '.js';
      }

      function getResource(params) {
        var deferred, url;
        deferred = $q.defer();
        url = params.url ? params.url : endpoint(params.resource);

        DataLoaderSvc.loadScript(url, function () {
          var data = params.ns ? $.monahrq[params.ns][params.resource] : $.monahrq[params.resource];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            var data2;

            if (params.callback) {
              data2 = params.callback(data);
            }
            else {
              data2 = data;
            }

            return deferred.resolve(data2);
          }

        }, function () {
        }, true, params.cacheBust);

        return deferred.promise;
      }

      API.getFlutterRegistry = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('FlutterRegistry');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Flutter.Base.FlutterRegistry;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getHospitals = function () {
        return getResource({
          resource: 'Hospitals',
          url: 'Data/Base/Hospitals.js',
          callback: function (data) {
            var hospitals = _.map(data, function (r) {
              r.HospitalTypes = (typeof r.HospitalTypes == 'string' ? r.HospitalTypes.split(',') : []);
              r.HospitalTypes = _.map(r.HospitalTypes, function (typeId) {
                return +typeId;
              });
              return r;
            });

            SortSvc.objSort(hospitals, 'Name', 'asc');

            return hospitals;
          }
        });
      };

      API.getPatientRegions = function () {
        return getResource({
          resource: 'PatientRegions',
          url: 'Data/Base/PatientRegions.js',
          callback: function (data) {
            SortSvc.objSort(data, 'Name', 'asc');
            return data;
          }
        });
      };

      API.getHospitalRegions = function () {
        return getResource({
          resource: 'HospitalRegions',
          url: 'Data/Base/HospitalRegions.js',
          callback: function (data) {
            SortSvc.objSort(data, 'Name', 'asc');
            return data;
          }
        });
      };

     API.getPatientCounties = function() {
       return getResource({
         resource: 'PatientCounties',
         url: 'Data/Base/PatientCounties.js',
         callback: function(data) {
           SortSvc.objSort(data, 'CountyName', 'asc');
           return data;
         }
       });
     };

      API.getHospitalCounties = function () {
        return getResource({
          resource: 'HospitalCounties',
          url: 'Data/Base/HospitalCounties.js',
          callback: function (data) {
            SortSvc.objSort(data, 'CountyName', 'asc');
            return data;
          }
        });
      };

      API.getNursingHomes = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeIndex');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.NursingHomeIndex;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            SortSvc.objSort(data, 'Name', 'asc');
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;

      };

      API.getNursingHomeTypes = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeTypes');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.NursingHomeTypes;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;

      };

      API.getNursingHomeCounties = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeCounties');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.NursingHomeCounties;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            SortSvc.objSort(data, 'CountyName', 'asc');
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getNursingHomeProfile = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeProfiles', 'Profile_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.Profiles[id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getNursingHomeMeasureTopics = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeMeasures', 'MeasureTopics');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.MeasureTopics;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getNursingHomeMeasureTopicsConsumer = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeMeasures', 'MeasureTopicsConsumer');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.MeasureTopicsConsumer;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getNursingHomeMeasure = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('NursingHomeMeasures', 'MeasureDescription_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.NursingHomes.Base.MeasureDescriptions[id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
          return deferred.reject('Unable to load \"' + url + '\".');
        }, true);

        return deferred.promise;
      };

      API.getNursingHomeMeasures = function (measureIds, useAny) {
        var deferred, promises;
        deferred = $q.defer();

        promises = _.map(measureIds, function (id) {
          return API.getNursingHomeMeasure(id);
        });

        var p;
        if (useAny) {
          p = promiseAny(promises);
        }
        else {
          p = $q.all(promises);
        }

        p.then(function (defs) {
          deferred.resolve(_.flatten(defs));
        },
        function (reason) {
          deferred.reject(reason);
        });

        return deferred.promise;
      };

      API.getCostQualityTopics = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('CostQualityMeasures', 'CostQualityTopic');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.costqualityTopics;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getCostQualityMeasure = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('CostQualityMeasures', 'Measure_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.costdata['measuredescription_' + id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getPhysicians = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysiciansIndex');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.PhysicianIndex;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            SortSvc.objSort(data, 'Name', 'asc');
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;

      };

      API.getPhysicianCity = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysicianCityIndex');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.PhysicianCityIndex;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getPhysicianPratices = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('MedicalPractices');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.MedicalPractices;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            SortSvc.objSort(data, 'Name', 'asc');
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;

      };

      API.getPhysicianSpecialties = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysicianSpecialty');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.PhysicianSpecialty;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getPhysicianHospitalAffiliation = function() {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysicianHospitalAffiliations');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.PhysicianHospitalAffiliation;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getConfiguration = function () {
        return getResource({
          resource: 'configuration',
          url: 'Data/website_config.js'
        });
      };

      API.getCountyGeoFor = function (state) {
        var resource = opt.countyGeo + state;
        return getResource({resource: resource});
      };

      API.getRegionGeoFor = function (state, regionalContext) {
        var rc;
        if (regionalContext == 'HospitalServiceArea') {
          rc = 'hsa';
        }
        else if (regionalContext == 'HealthReferralRegion') {
          rc = 'hrr';
        }

        if (rc) {
          var resource = rc + opt.regionGeo + state;
          return getResource({resource: resource});
        }
        else {
          // we don't have data for custom regions
          var d = $q.defer();
          d.resolve({});
          return d.promise;
        }
      };

      API.getHospitalProfile = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('hospitals', 'Hospital_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq['hospitalProfile'];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getMeasureTopicCategoriesConsumer = function () {
        return getResource({
          resource: 'measuretopiccategories_consumer',
          ns: 'qualitydata',
          url: endpoint('QualityMeasures', 'TopicCategoriesConsumer')
        });
      };

      API.getMeasureTopicsConsumer = function () {
        return getResource({
          resource: 'measuretopics_consumer',
          ns: 'qualitydata',
          url: endpoint('QualityMeasures', 'TopicsConsumer'),
          callback: function (data) {
            return _.map(data, function (row) {
              row.Measures = _.map(row.MeasuresID.split(','), function (id) {
                return +id;
              });
              return row;
            });
          }
        });
      };

      API.getMeasureTopicCategories = function () {
        return getResource({
          resource: 'measuretopiccategories',
          ns: 'qualitydata',
          url: endpoint('QualityMeasures', 'TopicCategories')
        });
      };

      API.getMeasureTopics = function () {
        return getResource({
          resource: 'measuretopics',
          ns: 'qualitydata',
          url: endpoint('QualityMeasures', 'Topics'),
          callback: function (data) {
            return _.map(data, function (row) {
              row.Measures = _.map(row.MeasuresID.split(','), function (id) {
                return +id;
              });
              return row;
            });
          }
        });
      };

      API.getMeasureDef = function (id) {
        return getResource({
          resource: 'measuredescription_' + id,
          ns: 'qualitydata',
          url: endpoint('QualityMeasures', 'Measure_' + id)
        });
      };

      API.getMeasureDefs = function (measureIds) {
        var deferred, promises;
        deferred = $q.defer();

        promises = _.map(measureIds, function (id) {
          return API.getMeasureDef(id);
        });

        $q.all(promises).then(function (defs) {
            deferred.resolve(_.flatten(defs));
          },
          function (reason) {
            deferred.reject(reason);
          });

        return deferred.promise;
      };

      API.getZipDistances = function () {
        return getResource({
          resource: 'ZipDistances',
          callback: function (data) {
            return _.map(data, function (row) {
              return +row.Distance;
            });
          }
        });
      };

      API.getCCS = function () {
        return getResource({
          resource: 'dxccs',
          url: endpoint('DXCCS')
        });
      };

      API.getCCSCategories = function () {
        return getResource({
          resource: 'dxccscategories',
          url: endpoint('DXCCSCategories')
        });
      };

      API.getMDC = function () {
        return getResource({
          resource: 'mdc',
          url: endpoint('MDC')
        });
      };

      API.getDRG = function () {
        return getResource({
          resource: 'drg',
          url: endpoint('DRG')
        });
      };

      API.getDRGCategories = function () {
        return getResource({
          resource: 'drgcategories',
          url: endpoint('DRGCategories')
        });
      };

      API.getPRCCS = function () {
        return getResource({
          resource: 'prccs',
          url: endpoint('PRCCS')
        });
      };

      API.getPRCCSCategories = function () {
        return getResource({
          resource: 'prccscategories',
          url: endpoint('PRCCSCategories')
        });
      };

      API.getCmsEntities = function () {
        return getResource({
          resource: 'entities',
          ns: 'cms',
          url: endpoint('CmsEntities'),
          cacheBust: true
        });
      };

      API.getMedicalPracticeMeasureTopicCategories = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('MedicalPracticeMeasures', 'MeasureTopicCategories');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.MedicalPractices.Base.MeasureTopicCategories;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getMedicalPracticeMeasureTopics = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('MedicalPracticeMeasures', 'MeasureTopics');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.MedicalPractices.Base.MeasureTopics;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getMedicalPracticeMeasure = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('MedicalPracticeMeasures', 'MeasureDescription_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.MedicalPractices.Base.MeasureDescriptions[id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
          return deferred.reject('Unable to load \"' + url + '\".');
        }, true);

        return deferred.promise;
      };

      API.getMedicalPracticeMeasures = function (measureIds, useAny) {
        var deferred, promises;
        deferred = $q.defer();

        promises = _.map(measureIds, function (id) {
          return API.getMedicalPracticeMeasure(id);
        });

        var p;
        if (useAny) {
          p = promiseAny(promises);
        }
        else {
          p = $q.all(promises);
        }

        p.then(function (defs) {
          deferred.resolve(_.flatten(defs));
        },
        function (reason) {
          deferred.reject(reason);
        });

        return deferred.promise;
      };

      API.getPhysicianMeasureTopics = function () {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysicianMeasures', 'MeasureTopics');

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.MeasureTopics;

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
        }, true);

        return deferred.promise;
      };

      API.getPhysicianMeasure = function (id) {
        var deferred, url;
        deferred = $q.defer();
        url = endpoint('PhysicianMeasures', 'MeasureDescription_' + id);

        DataLoaderSvc.loadScript(url, function () {
          var data = $.monahrq.Physicians.Base.MeasureDescriptions[id];

          if (_(data).isUndefined()) {
            return deferred.reject('Unable to load \"' + url + '\".');
          } else {
            return deferred.resolve(data);
          }

        }, function () {
          return deferred.reject('Unable to load \"' + url + '\".');
        }, true);

        return deferred.promise;
      };

      API.getPhysicianMeasures = function (measureIds, useAny) {
        var deferred, promises;
        deferred = $q.defer();

        promises = _.map(measureIds, function (id) {
          return API.getPhysicianMeasure(id);
        });

        var p;
        if (useAny) {
          p = promiseAny(promises);
        }
        else {
          p = $q.all(promises);
        }

        p.then(function (defs) {
          deferred.resolve(_.flatten(defs));
        },
        function (reason) {
          deferred.reject(reason);
        });

        return deferred.promise;
      };

    function promiseAny(arrayOfPromises) {
      if(!arrayOfPromises || !(arrayOfPromises instanceof Array)) {
        throw new Error('Must pass Promise.any an array');
      }

      if(arrayOfPromises.length === 0) {
        return $q.resolve([]);
      }


      // For each promise that resolves or rejects,
      // make them all resolve.
      // Record which ones did resolve or reject
      var resolvingPromises = arrayOfPromises.map(function(promise) {
        return promise.then(function(result) {
          return {
            resolve: true,
            result: result
          };
        }, function(error) {
          return {
            resolve: false,
            result: error
          };
        });
      });

      return $q.all(resolvingPromises).then(function(results) {
        // Count how many passed/failed
        var passed = [], failed = [], allFailed = true;
        results.forEach(function(result) {
          if(result.resolve) {
            allFailed = false;
          }
          passed.push(result.resolve ? result.result : null);
          failed.push(result.resolve ? null : result.result);
        });

        if(allFailed) {
          throw failed;
        } else {
          return passed;
        }
      });
    }

      return API;
    }

})();
