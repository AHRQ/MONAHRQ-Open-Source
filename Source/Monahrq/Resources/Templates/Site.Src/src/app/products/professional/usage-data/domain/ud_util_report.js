/**
 * Professional Product
 * Usage Data Domain Module
 * Utilization Report Loader Service
 *
 * This service loads utilization data use the Data Loader. It provides loaders
 * for the following usage data:
 *
 * - Get report
 * - Get trending report
 *
 * Because of the breadth and complexity of the output directory and file structure for utilization data,
 * the two loading methods accept a query object specifying the criteria to search for. The format is the
 * same as defined by UDUtilQuerySvc.
 *
 * An example invocation:
 *
 * UDUtilQuerySvc.reset();
 * var query = UDUtilQuerySvc.query;
 * query.groupBy.viewBy = 'hospital';
 * query.groupBy.reportType = 'id';
 * query.groupBy.groupBy = 'geo';
 * query.groupBy.dimension = 'hospital';
 * query.groupBy.value = hospitalId;
 * query.viewBy = 'drg';
 * query.level.type = 'summary';
 * UDUtilReportSvc.getReport(query, reportingYear)
 *
 */
(function(ng, _) {
  'use strict';

  ng.module('monahrq.domain').service('UDUtilReportSvc', ['$q', '$rootScope', 'DataLoaderSvc', 'ReportConfigSvc', function ($q, $rootScope, DataLoaderSvc, ReportConfigSvc) {
    /**
     * Private Data
     */
    var opt = {
      reportDir: {
        'ed': 'Data/EmergencyDischarge',
        'id': 'Data/InpatientUtilization',
        'county': 'Data/County',
        'region': 'Data/Region'
      }
    };

    var isCompressed = $rootScope.config.CompressedAndOptimized === 1;

    $.monahrq = $.monahrq || {};

    /*
      Note that the report defs are being combined into a single summary & detail record
      as a convenience. You could have a separate rule for a single dimension and/or
      viewby if desired.
    */
    var reportDefinitions = [
      ///////// ED Reports
      // GEO
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'summary',
        viewBy: ['condition'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['condition'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'detail',
        viewBy: ['condition'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },
      // CLINICAL
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'clinical',
        dimension: ['condition'],
        level: 'summary',
        viewBy: ['hospital', 'region', 'hospitalType', 'county', 'zip'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'clinical',
        dimension: ['condition'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['hospital', 'region', 'hospitalType', 'county', 'zip'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'ed',
        group: 'clinical',
        dimension: ['condition'],
        level: 'detail',
        viewBy: ['hospital', 'region', 'hospitalType', 'county', 'zip'],
        dataField: 'emergencydischarge',
        tpl: '<%=reportingYear%>/CCS/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },

      ///////// IP Reports
      // GEO
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'summary',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'geo',
        dimension: ['hospital', 'county', 'region', 'hospitalType'],
        level: 'detail',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },
      // CLINICAL
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'summary',
        viewBy: ['hospital', 'region', 'hospitalType', 'county'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['hospital', 'region', 'hospitalType', 'county'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'hospital',
        reportType: 'id',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        viewBy: ['hospital', 'region', 'hospitalType', 'county'],
        dataField: 'inpatientutilization',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },

      ///////// County Reports
      // GEO
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'geo',
        dimension: ['patientcounty'],
        level: 'summary',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'geo',
        dimension: ['patientcounty'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'geo',
        dimension: ['patientcounty'],
        level: 'detail',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },
      // CLINICAL
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'summary',
        viewBy: ['patientcounty'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['patientcounty'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'population',
        reportType: 'county',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        viewBy: ['patientcounty'],
        dataField: 'County',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },

      ///////// Region Reports
      // GEO
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'geo',
        dimension: ['patientregion'],
        level: 'summary',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'geo',
        dimension: ['patientregion'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'geo',
        dimension: ['patientregion'],
        level: 'detail',
        viewBy: ['condition', 'mdc', 'drg', 'procedure'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xViewBy%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      },
      // CLINICAL
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'summary',
        viewBy: ['patientregion'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/summary.js'
      },
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        levelValue: '0',
        viewBy: ['patientregion'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details.js'
      },
      {
        gViewBy: 'population',
        reportType: 'region',
        group: 'clinical',
        dimension: ['condition', 'mdc', 'drg', 'procedure'],
        level: 'detail',
        viewBy: ['patientregion'],
        dataField: 'Region',
        tpl: '<%=reportingYear%>/<%=xDimension%>/<%=xDimension%>/<%=xDimension%>_<%=group%>/details_<%=level%>.js'
      }
    ];

    var dimensionMap = {
      hospital: 'Hospital',
      hospitalType: 'HospitalType',
      region: 'Region',
      patientregion: 'Region',
      patientcounty: 'County',
      county: 'County',
      zip: 'ZipCode',
      condition: 'CCS',
      drg: 'DRG',
      mdc: 'MDC',
      procedure: 'PRCCS'
    };


    /**
     * Service Interface
     */
    return {
      getReport: getReport,
      getTrendingReports: getTrendingReports
    };


    /**
     * Service Definition
     */
    function getReportDefinition(query) {
      var report;

      report = _.filter(reportDefinitions, function(r) {
        var levelValue = query.level.value;
        if (isCompressed) {
          levelValue = 0;
        }

        if (r.gViewBy == query.groupBy.viewBy
          && r.reportType == query.groupBy.reportType
          && r.group == query.groupBy.groupBy
          && _.contains(r.dimension, query.groupBy.dimension)
          && r.level == query.level.type
          && (_.has(r, 'levelValue') ? r.levelValue == levelValue : true)
          && _.contains(r.viewBy, query.viewBy)) {
          return true;
        }

        return false;
      });

      return report.length > 0 ? report[0] : null;
    }

    function getUrl(tpl, query, reportingYear) {
      var compiled = _.template(tpl);
      return  opt.reportDir[query.groupBy.reportType] + '/' + compiled({
        reportingYear: reportingYear,
        group: query.groupBy.value,
        group2: query.groupBy.value2,
        narrow: query.narrowBy.value,
        narrow2: query.narrowBy.value2,
        level: query.level.value,
        viewBy: query.viewBy,
        xDimension: dimensionMap[query.groupBy.dimension],
        xViewBy: dimensionMap[query.viewBy]
      });
    }

    function getReport(query, reportingYear) {
      var deferred, report, url;
      deferred = $q.defer();

      JL('services.UDUtilReportSvc.getReport').debug({msg:'query', data: query});

      report = getReportDefinition(query);
      if (report) {
        JL('services.UDUtilReportSvc.getReport').debug({msg:'report', data: report});
        url = getUrl(report.tpl, query, reportingYear);
      }
      else {
        JL('services.UDUtilReportSvc.getReport').debug('Report not found');
        deferred.reject('No report matches query parameters');
        return deferred.promise;
      }

      DataLoaderSvc.loadScript(url, function () {
        var data;

        if (query.level.type == 'detail' && isCompressed){
          var optDetail = $.monahrq[report.dataField];
          var detailObj = {};
          var levelID = parseInt(query.level.value);

          for(var item in optDetail) {
            if (item == 'TableData') {
              var tableData = optDetail[item][0];
              var detailIndex = tableData['LevelID'].indexOf(levelID);
              var tableDataObject = [];

              for (var i = 0; i < tableData['CatID'][detailIndex].length; i++) {
                var rowData = {};
                for (var column in tableData) {
                  if (column == 'LevelID') {
                    continue;
                  }
                  rowData[column] = tableData[column][detailIndex][i];
                }
                tableDataObject.push(rowData);
              }
              detailObj[item] = tableDataObject;
            }
            else {
              detailObj[item] = optDetail[item];
            }
          }
          $.monahrq[report.dataField] = detailObj;
        }

        data = $.monahrq[report.dataField];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        }
        else if (data.TableData.length == 0) {
          return deferred.reject('Empty TableData in report \"' + url + '\".');
        }
        else {
          return deferred.resolve({
            reportingYear: reportingYear,
            query: query,
            report: data
          });
        }

      },
      function(url) {
        return deferred.reject('Unable to load \"' + url + '\".');
      }, true);

      return deferred.promise;
    }

    function getTrendingReports(query, reportingYears) {
      var promises = [];

      _.each(reportingYears, function(reportingYear) {
        promises.push(getReport(query, reportingYear));
      });

      var agg = $q.all(promises).then(function(result) {
        var reports = {};

        _.each(result, function(report) {
          reports[report.reportingYear] = report.report;
        });

        return reports;
      });

      return agg;
    }

  }]);

}(angular, _));
