/**
 * Professional Product
 * Usage Data Report Module
 * Setup
 *
 * Configure the page states used by the utilization section of the professional website.
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
angular.module('monahrq.products.professional.usage-data', [
  'monahrq.products.professional.usage-data.domain'
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
    $stateProvider.state('top.professional.usage-data', {
      url: '/utilization',
      data: {
        pageTitle: 'Utilization'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/usage-data/views/index.html',
          controller: 'UsageDataCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgUsageData();
            }
          }
        },
        'ud-search@top.professional.usage-data': {
          templateUrl: 'app/products/professional/usage-data/views/search_begin.html',
          controller: 'UDSearchBeginCtrl'
        }
       }
    });

    $stateProvider.state('top.professional.usage-data.avoidable-stays', {
      url: '/avoidable-stays?reportType&county&topics&topic&measure&displayType',
      data: {
        pageTitle: 'Avoidable Stays',
        report: {
          county: '2aaf7fba-7102-4c66-8598-a70597e2f831',
          topic_table: '2aaf7fba-7102-4c66-8598-a70597e2f832',
          topic_map: '2aaf7fba-7102-4c66-8598-a70597e2f833'
        }
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/usage-data/views/avoidable-stays.html',
          controller: 'UDAvoidableStaysCtrl'
        },
        'ud-search@top.professional.usage-data.avoidable-stays': {
          templateUrl: 'app/products/professional/usage-data/views/search_avoidable-stays.html',
          controller: 'UDSearchAvoidableStaysCtrl',
          resolve: {
            counties: function(ResourceSvc) {
              return ResourceSvc.getHospitalCounties();
            },
            ahsTopics: function(ResourceSvc) {
              return ResourceSvc.getahsTopics();
            }
          }
        },
        'ud-report@top.professional.usage-data.avoidable-stays': {
          templateUrl: 'app/products/professional/usage-data/views/report_avoidable-stays.html',
          controller: 'UDReportAvoidableStaysCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgUDReportAHS();
            },
            ahsTopics: function(ResourceSvc) {
              return ResourceSvc.getahsTopics();
            }
          }
        },
        'ud-report-content@top.professional.usage-data.avoidable-stays': {
          templateUrl: function(stateParams) {
            if (stateParams.reportType === 'county') {
              return 'app/products/professional/usage-data/views/county_avoidable-stays.html';
            }
            else if (stateParams.reportType === 'topic') {
              if (!stateParams.displayType) return null;
              return 'app/products/professional/usage-data/views/' + stateParams.displayType + '_avoidable-stays.html';
            }
          },
          controllerProvider: function($stateParams) {
            if ($stateParams.reportType === 'county') {
              return 'UDCountyAvoidableStaysCtrl';
            }
            else if ($stateParams.reportType === 'topic') {
              if (!$stateParams.displayType) return null;
              var ctrlName = 'UD' + lcase($stateParams.displayType) + 'AvoidableStaysCtrl';
              return ctrlName;
            }
          },
          resolve: {
            counties: function(ResourceSvc) {
              return ResourceSvc.getHospitalCounties();
            },
            ahsTopics: function(ResourceSvc) {
              return ResourceSvc.getahsTopics();
            },
            ahs: function(UDAhsReportSvc){
              return UDAhsReportSvc.getReport();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.usage-data.service-use', {
      url: '/service-use?viewBy&reportType&displayType&levelType&levelValue&levelViewBy&groupBy&dimension&value&value2',
      data: {
        pageTitle: 'Service Use',
        report: {
          'ed_summary': '2aaf7fba-7102-4c66-8598-a70597e2f827',
          'ed_detail': '2aaf7fba-7102-4c66-8598-a70597e2f828',
          'ed_summary_trending': '0d497b8d-e04e-4380-815d-7d9e644dc96e',
          'ed_detail_trending': '4a2747dc-6b9d-4c77-963d-37b9b8112047',
          'id_summary': '2aaf7fba-7102-4c66-8598-a70597e2f824',
          'id_detail': '2aaf7fba-7102-4c66-8598-a70597e2f825',
          'id_summary_trending': '47426256-5f4f-4996-8f4a-3a344e39d90e',
          'id_detail_trending': '4e94e281-8e7e-4b02-8567-72b4fa239f9e',
          'county_summary': '5aaf7fba-7102-4c66-8598-a70597e2f825',
          'county_detail': '5aaf7fba-7102-4c66-8598-a70597e2f826',
          'county_summary_trending': '26094800-7af1-43ff-b476-66305c76c2f4',
          'county_detail_trending': '412a47d6-0859-47c7-a980-0a6c395c55f4',
          'region_summary': '3a40cf6b-37ad-4861-b272-930ddf2b8802',
          'region_detail': '8d25f78e-86ba-43fb-ba5f-352227187759',
          'region_summary_trending': '709f70b5-50d9-4ca9-a19d-9e3df2b38386',
          'region_detail_trending': '9d45afa3-2d1c-48e6-9b9f-77746df75c63',
          'county_map': '5aaf7fba-7102-4c66-8598-a70597e2f825',
          'region_map': '3a40cf6b-37ad-4861-b272-930ddf2b8802'
        }
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/usage-data/views/service-use.html',
          controller: 'UDServiceUseCtrl'
        },
        'ud-search@top.professional.usage-data.service-use': {
          templateUrl: 'app/products/professional/usage-data/views/search_service-use.html',
          controller: 'UDSearchServiceUseCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalRegions: function(ResourceSvc) {
              return ResourceSvc.getHospitalRegions();
            },
            hospitalCounties: function(ResourceSvc) {
              return ResourceSvc.getHospitalCounties();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            patientRegions: function(ResourceSvc) {
              return ResourceSvc.getPatientRegions();
            },
            patientCounties: function(ResourceSvc) {
              return ResourceSvc.getPatientCounties();
            },
            ccs: function(ResourceSvc) {
              return ResourceSvc.getCCS();
            },
            ccsCategories: function(ResourceSvc) {
              return ResourceSvc.getCCSCategories();
            },
            mdc: function(ResourceSvc) {
              return ResourceSvc.getMDC();
            },
            drg: function(ResourceSvc) {
              return ResourceSvc.getDRG();
            },
            drgCategories: function(ResourceSvc) {
              return ResourceSvc.getDRGCategories();
            },
            prccs: function(ResourceSvc) {
              return ResourceSvc.getPRCCS();
            },
            prccsCategories: function(ResourceSvc) {
              return ResourceSvc.getPRCCSCategories();
            }
          }
        },
        'ud-report@top.professional.usage-data.service-use': {
          templateUrl: 'app/products/professional/usage-data/views/report_util.html',
          controller: 'UDReportUtilCtrl',
          resolve: {
            content: function(ResourceSvc, $stateParams) {
              if (!$stateParams.reportType) return null;
              var rt = $stateParams.reportType.toUpperCase();
              if (rt === 'COUNTY') rt = 'County';
              else if (rt === 'REGION') rt = 'Region';
              return ResourceSvc['getpgUDReport' + rt]();
            },
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalRegions: function(ResourceSvc) {
              return ResourceSvc.getHospitalRegions();
            },
            hospitalCounties: function(ResourceSvc) {
              return ResourceSvc.getHospitalCounties();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            patientRegions: function(ResourceSvc) {
              return ResourceSvc.getPatientRegions();
            },
            patientCounties: function(ResourceSvc) {
              return ResourceSvc.getPatientCounties();
            },
            ccs: function(ResourceSvc) {
              return ResourceSvc.getCCS();
            },
            ccsCategories: function(ResourceSvc) {
              return ResourceSvc.getCCSCategories();
            },
            mdc: function(ResourceSvc) {
              return ResourceSvc.getMDC();
            },
            drg: function(ResourceSvc) {
              return ResourceSvc.getDRG();
            },
            drgCategories: function(ResourceSvc) {
              return ResourceSvc.getDRGCategories();
            },
            prccs: function(ResourceSvc) {
              return ResourceSvc.getPRCCS();
            },
            prccsCategories: function(ResourceSvc) {
              return ResourceSvc.getPRCCSCategories();
            }
          }
        },
        'ud-report-content@top.professional.usage-data.service-use': {
          templateUrl: function(stateParams) {
            if (!stateParams.displayType) return null;

            var reportType = '';
            if ((stateParams.reportType === 'county' || stateParams.reportType === 'region') && stateParams.displayType === 'map') {
              reportType = '_' + stateParams.reportType;
            }

            return 'app/products/professional/usage-data/views/' + stateParams.displayType + reportType + '_util.html';
          },
          controllerProvider: function($stateParams) {
            if (!$stateParams.displayType) return null;

            var reportType = '';
            if (($stateParams.reportType === 'county' || $stateParams.reportType === 'region') && $stateParams.displayType === 'map') {
              reportType = lcase($stateParams.reportType);
            }

            var ctrlName = 'UD' + lcase($stateParams.displayType) + reportType + 'UtilCtrl';

            return ctrlName;
          },
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalRegions: function(ResourceSvc) {
              return ResourceSvc.getHospitalRegions();
            },
            hospitalCounties: function(ResourceSvc) {
              return ResourceSvc.getHospitalCounties();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            patientRegions: function(ResourceSvc) {
              return ResourceSvc.getPatientRegions();
            },
            patientCounties: function(ResourceSvc) {
              return ResourceSvc.getPatientCounties();
            },
            ccs: function(ResourceSvc) {
              return ResourceSvc.getCCS();
            },
            ccsCategories: function(ResourceSvc) {
              return ResourceSvc.getCCSCategories();
            },
            mdc: function(ResourceSvc) {
              return ResourceSvc.getMDC();
            },
            drg: function(ResourceSvc) {
              return ResourceSvc.getDRG();
            },
            drgCategories: function(ResourceSvc) {
              return ResourceSvc.getDRGCategories();
            },
            prccs: function(ResourceSvc) {
              return ResourceSvc.getPRCCS();
            },
            prccsCategories: function(ResourceSvc) {
              return ResourceSvc.getPRCCSCategories();
            },
            stratification: function(ResourceSvc) {
              var data = ResourceSvc.getStratification();
              return data;
            },
            age: function(ResourceSvc) {
              return ResourceSvc.getAge();
            },
            sex: function(ResourceSvc) {
              return ResourceSvc.getSex();
            },
            race: function(ResourceSvc) {
              return ResourceSvc.getRace();
            },
            payer: function(ResourceSvc) {
              return ResourceSvc.getPayer();
            },
            hospitalZips: function(ResourceSvc) {
              return ResourceSvc.getHospitalZips();
            }
          }
        }
      }
    });

}

/**
 * Module run
 */
function run() {

}

function lcase(s) {
  return s.substring(0, 1).toUpperCase() + s.substring(1);
}

})();

