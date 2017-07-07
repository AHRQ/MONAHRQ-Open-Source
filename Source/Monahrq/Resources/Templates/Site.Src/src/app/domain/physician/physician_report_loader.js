/**
 * Monahrq Nest
 * Core Domain Module
 * Physician Report Loader Service
 *
 * This service loads physician data use the Simple Report Loader. It provides single
 * and bulk loaders for the following data:
 *
 * - Find physicians by id, name, practice, specialty, zip or city
 * - Load CG-CAHPS reports by practice
 * - Load the HEDIS report by physician id
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('PhysicianReportLoaderSvc', PhysicianReportLoaderSvc);


  PhysicianReportLoaderSvc.$inject = ['$q', 'SimpleReportLoaderSvc', 'ResourceSvc'];
  function PhysicianReportLoaderSvc($q, SimpleReportLoaderSvc, ResourceSvc) {
    /**
     * Private Data
     */
    $.monahrq = $.monahrq || {};
    $.monahrq.Physicians = $.monahrq.Physicians || {};
    $.monahrq.Physicians.Report = $.monahrq.Physicians.Report || {};
    $.monahrq.Physicians.Report.Specialty = $.monahrq.Physicians.Report.Specialty || {};
    $.monahrq.Physicians.Report.Zip = $.monahrq.Physicians.Report.Zip || {};
    $.monahrq.Physicians.Report.Practice = $.monahrq.Physicians.Report.Practice || {};
    $.monahrq.Physicians = $.monahrq.Physicians || {};
    $.monahrq.Physicians.Base = $.monahrq.Physicians.Base || {};
    $.monahrq.Physicians.Base.Profiles = $.monahrq.Physicians.Base.Profiles || {};

    var config = {
      physician: {
        rootObj: $.monahrq.Physicians.Base,
        reportName: 'Profiles',
        reportDir: 'Data/Base/PhysicianProfiles/',
        filePrefix: 'Profile_'
      },
      specialty: {
        rootObj: $.monahrq.Physicians.Report,
        reportName: 'Specialty',
        reportDir: 'Data/Physicians/Specialty/',
        filePrefix: 'Specialty_'
      },
      zip: {
        rootObj: $.monahrq.Physicians.Report,
        reportName: 'Zip',
        reportDir: 'Data/Physicians/Zip/',
        filePrefix: 'Zip_'
      },
      practice: {
        rootObj: $.monahrq.Physicians.Report,
        reportName: 'Practice',
        reportDir: 'Data/Physicians/Practice/',
        filePrefix: 'Practice_'
      },
      cgcahpsPractices: {
        rootObj: $.monahrq.MedicalPractices.Report.CGCAHPS,
        reportName: 'MedicalPractice',
        reportDir: 'Data/MedicalPractices/',
        filePrefix: 'MedicalPractice_'
      }
    };

    /**
     * Service Interface
     */
    return {
      init: init,
      getById: getById,
      findByNPIs: findByNPIs,
      findByName: findByName,
      findByPracticeName: findByPracticeName,
      findByPracticeId: findByPracticeId,
      findBySpecialty: findBySpecialty,
      findByZip: findByZip,
      findByCity: findByCity,
      findByHospAfl: findByHospAfl,
      getDemoPhysicians: getDemoPhysicians,
      getCGCAHPSPractices: getCGCAHPSPractices,
      getCGCAHPSPractice: getCGCAHPSPractice,
      getHedisReport: getHedisReport
    };


    /**
     * Service Implementation
     */
    function init(websiteConfig) {
    }

    function getById(id) {
      return SimpleReportLoaderSvc.load(config.physician, id)
        .then(function(result) {
          postProcess(result.data);
          return result.data;
        });
    }

    function findByNPIs(npis) {
      return SimpleReportLoaderSvc.bulkLoad(config.physician, npis)
    }

    function getDemoPhysicians() {
      return ResourceSvc.getPhysicians()
        .then(function(physicianIndex) {
          return findByNPIs(_.pluck(_.take(physicianIndex, 10), 'npi'));
        });
    }

    function findByName(first_name, last_name) {
      return ResourceSvc.getPhysicians()
        .then(function(physicianIndex) {
          var matches = [];
          _.each(physicianIndex, function(physician) {
            var reportFirstName = physician.frst_nm.toLowerCase(),
              reportLastName = physician.lst_nm.toLowerCase();

            if (first_name) {
              first_name = first_name.toLowerCase();
            } else {
              first_name = null;
            }

            if (last_name) {
              last_name = last_name.toLowerCase();
            } else {
              last_name = null;
            }

            var fnMatch = reportFirstName.indexOf(first_name) > -1,
              lnMatch = reportLastName.indexOf(last_name) > -1;

            if (first_name && first_name.length > 0 && last_name && last_name.length > 0 && fnMatch && lnMatch) {
              matches.push(physician.npi);
            }
            else if (first_name && first_name.length > 0 && last_name === null && fnMatch) {
              matches.push(physician.npi);
            }
            else if (last_name && last_name.length > 0 && first_name === null && lnMatch) {
              matches.push(physician.npi);
            }
          });

          if (matches.length > 0) {
            return SimpleReportLoaderSvc.bulkLoad(config.physician, matches)
              .then(function(data) {
                  _.each(data, function(data) { postProcess(data.data)});
                  return data;
                });
          }
          else {
            return [{id: null, data: []}];
          }
        });
    }

    function findByPracticeName(practiceName) {
      return ResourceSvc.getPhysicianPratices()
        .then(function(physicianIndex) {
          var practiceArr = [],
            practiceIds = [],
            practiceNameResults = [],
            reportPracticeNames = [],
            reportPractices = [];

          if (practiceName) {
            practiceName = practiceName.toLowerCase();
          }

          _.each(physicianIndex, function(name) {
            name.PracticeName = name.PracticeName.toLowerCase();
            reportPracticeNames.push(name.PracticeName);
          });

          _.each(reportPracticeNames, function(reportPracticeName) {
            if (reportPracticeName.indexOf(practiceName) > -1) {
              practiceNameResults.push(reportPracticeName);
            }
          });

          _.each(practiceNameResults, function(practiceNameResult) {
            practiceArr.push(_.findWhere(physicianIndex, {PracticeName: practiceNameResult}));
          });

          _.each(practiceArr, function(practiceResult) {
            practiceIds.push(practiceResult.GroupPracticePacId);
          });

          if (practiceIds) {
            return SimpleReportLoaderSvc.bulkLoad(config.practice, practiceIds)
              .then(function (data) {
                _.each(data, function (data) {
                  postProcess(data.data);
                });
                return data;
              });
          }

          return [{id: null, data:[]}];
        });
    }

    function findByPracticeId(practiceId) {
      return SimpleReportLoaderSvc.load(config.practice, practiceId)
        .then(function(data) {
            postProcess(data.data);
            return data;
          });

    }

    function findBySpecialty(id) {
      return SimpleReportLoaderSvc.load(config.specialty, id)
        .then(function(data) {
          postProcess(data.data);
          return data;
        });
    }

    function findByZip(zip) {
      return SimpleReportLoaderSvc.load(config.zip, zip)
        .then(function(data) {
          postProcess(data.data);
          return data;
        });
    }

    function findByCity(city) {
      return ResourceSvc.getPhysicianCity()
        .then(function(data) {
          var npis = _.filter(data, function(row) {
            return row.cty.toLowerCase() == city.toLowerCase();
          });

          if (npis.length == 0) {
            return [{id: null, data: []}];
          }

          return findByNPIs(_.pluck(npis, 'npi'))
            .then(function(data) {
              _.each(data, function(data) { postProcess(data.data)});
              return data;
            });
        });
    }

    function findByHospAfl(providerId) {
      return[{id: null, data: []}];
    }

    function postProcess(data) {
      _.each(data, function(row) {
        row.frst_nm = toTitleCase(row.frst_nm);
        row.lst_nm = toTitleCase(row.lst_nm);
      });
    }

    function toTitleCase(str)
    {
      return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
    }

    function getCGCAHPSPractices(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.cgcahpsPractices, ids)
        .then(function(reports) {
          var result = {};
          for (var i = 0; i < reports.length; i++) {
            result[reports[i].id] = reports[i].data;
          }
          return result;
        });
    }

    function getCGCAHPSPractice(id) {
      return SimpleReportLoaderSvc.load(config.cgcahpsPractices, id);
    }

    function getHedisReport(id) {
      var report = {
        isLoaded: false
      };

      return ResourceSvc.getPhysicianMeasureTopics()
        .then(function(topics) {
          report.topics = topics;
          var measureIds = _.flatten([].concat(_.pluck(topics, 'MeasureIDs')));
          if (measureIds && measureIds.length > 0) {
            return ResourceSvc.getPhysicianMeasures(measureIds, true);
          }

          return null;
        })
        .then(function(measureDefs) {
          report.measureDefs = measureDefs;
          if (measureDefs) {
            return getById(id);
          }

          return null;
        })
        .then(function(data) {
          if (data == null) {
            return report;
          }

          var ratesObj = {};
          _.each(data, function(row) {
            ratesObj[row.org_pac_id] = row;
          });

          var rates = _.flatten(_.pluck(_.values(ratesObj), 'HEDISRates'));
          rates = _.reject(rates, function (x) { return x == undefined; });

          report.rows = _.compact(_.map(rates, function(m) {
            var md = _.findWhere(report.measureDefs, {MeasureID: m.MeasureID});
            var row = {
              practiceId: m.PracticeId,
              measureId: m.MeasureID,
              name: md ? md.SelectedTitleConsumer : '',
              physicianRate: m.PhysicianRate,
              peerRate: m.PeerRate
            };

            if (row.physicianRate) {
              return row;
            }

            return null;
          }));

          report.hasPeer = _.any(report.rows, function(r) {
            return r.peerRate != null;
          });

          report.isLoaded = true;
          return report;
        })
    }

  }

})();
