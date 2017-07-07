/**
 * Monahrq Nest
 * Core Domain Module
 * ReportConfig Loader
 *
 * The ReportConfig service loads the consumer and professional ReportConfig data files,
 * and provides a variety of methods for working with the data. ReportConfig provides
 * configuration information for each report, such as help text, visibility and naming
 * of columns and reporting elements, and what years/quarters are included.
 *
 * A major use of the service is the webElementAvailable method. Various aspects of the
 * application UI are shown or hidden depending on what reports are included in the build.
 * The webElementDeps are the controlled elements, each specifying a list of report aliases
 * whose inclusion in the build signifies that the element should be displayed.
 *
 * Visibility is sometimes also determined by whether an entire category of reports is available,
 * such as hospitals or physicians. This is mostly used by the menu system.
 *
 * Finally, the Display field in ReportConfig and the corresponding methods below control
 * whether the host user wishes for certain UI elements to be made available, such as the
 * map on the hospital profile. This is a finer granularity than the webElementAvailable checks,
 * which are dependant on the presence of a report, rather than a flag within that report's
 * configuration.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('ReportConfigSvc', ReportConfigImpl)
    .factory('ConsumerReportConfigSvc', ReportConfigImpl);

  function ReportConfigImpl() {
    var API = {},
      activeReportAliases = [],
      config = [];

    var reportAliases = {
      '7af51434-5745-4538-b972-193f58e737d7': 'QR-9',
      '2aaf7fba-7102-4c66-8598-a70597e2f823': 'QR-8',
      '7aac8244-0f39-424a-85be-943b465ed61a': 'QR-3',
      '2aaf7fba-7102-4c66-8598-a70597e2f82b': 'QR-1',
      '12546e5a-ca82-4d7e-aab5-c1db88a4fd33': 'QR-4',
      '2aaf7fba-7102-4c66-8598-a70597e2f821': 'QR-6',
      '1bd85413-734b-4c7a-9aaf-c442d8c2face': 'QR-2',
      '7aac8244-0f39-424a-85be-943b465ed61b': 'QR-5',
      '2aaf7fba-7102-4c66-8598-a70597e2f820': 'QR-7',

      '2aaf7fba-7102-4c66-8598-a70597e2f833': 'UR-8',
      '5aaf7fba-7102-4c66-8598-a70597e2f826': 'UR-12',
      '2aaf7fba-7102-4c66-8598-a70597e2f827': 'UR-5',
      '2aaf7fba-7102-4c66-8598-a70597e2f832': 'UR-9',
      '2aaf7fba-7102-4c66-8598-a70597e2f824': 'UR-1',
      '5caf7fba-7102-4c66-8598-a70597e2f826': 'UR-4',
      '2aaf7fba-7102-4c66-8598-a70597e2f826': 'UR-3',
      '2aaf7fba-7102-4c66-8598-a70597e2f825': 'UR-2',
      '5aaf7fba-7102-4c66-8598-a70597e2f825': 'UR-11',
      '2aaf7fba-7102-4c66-8598-a70597e2f831': 'UR-10',
      '2aaf7fba-7102-4c66-8598-a70597e2f829': 'UR-7',
      '2aaf7fba-7102-4c66-8598-a70597e2f828': 'UR-6',

      '3a40cf6b-37ad-4861-b272-930ddf2b8802': 'UR-13',
      '8d25f78e-86ba-43fb-ba5f-352227187759': 'UR-14',

      '87e04110-46b0-4cae-9592-022c3111fac7': 'NH-1',
      'ba52b7b2-f4c8-4831-b910-1d036b94ae75': 'NH-2',
      'f2f2b7fe-8653-488b-8ed8-fd4417cd0f9e': 'NH-3',

      '4c5727b4-0e85-4f80-ade9-418b49a1373e': 'PH-1',
      'e007bb9c-e539-41d6-9d06-ff52f8a15bf6': 'PH-2',

      '64b545bf-d183-4f0d-8abc-6643ff39f5dd': 'IG-1'
    };

    var webElementDeps = {
      'Utilization_Tab': ['UR-1', 'UR-2', 'UR-3', 'UR-4', 'UR-5', 'UR-6', 'UR-7', 'UR-8', 'UR-9', 'UR-10', 'UR-11', 'UR-12', 'UR-13', 'UR-14'],
      'Quality_Tab': ['QR-1', 'QR-2', 'QR-3', 'QR-4', 'QR-5', 'QR-6', 'QR-7', 'QR-8'/*,'QR-9'*/],
      'Nursing_Tab': ['NH-1', 'NH-2', 'NH-3'],
      'Physician_Tab': ['PH-1', 'PH-2'],

      'Quality_ConditionTopicExplore_Button': ['QR-2', 'QR-3', 'QR-4', 'QR-5', 'QR-6', 'QR-7', 'QR-8'],
      'Quality_HospitalExplore_Button': ['QR-1', 'QR-9'],
      'Utilization_AHSFind_Button': ['UR-8', 'UR-9', 'UR-10'],
      'Utilization_ServiceUseFind_Button': ['UR-1', 'UR-2', 'UR-3', 'UR-4', 'UR-5', 'UR-6', 'UR-7', 'UR-11', 'UR-12', 'UR-13', 'UR-14'],

      'Quality_ConditionTopic_Tab': ['QR-2', 'QR-3', 'QR-4', 'QR-5', 'QR-6', 'QR-7', 'QR-8'],
      'Quality_Hospital_Tab': ['QR-1', 'QR-9'],
      'Utilization_AHS_Tab': ['UR-8', 'UR-9', 'UR-10'],
      'Utilization_ServiceUse_Tab': ['UR-1', 'UR-2', 'UR-3', 'UR-4', 'UR-5', 'UR-6', 'UR-7', 'UR-11', 'UR-12', 'UR-13', 'UR-14'],

      'ServiceUse_ViewBy_Hospital_Radio': ['UR-1', 'UR-5'],
      'ServiceUse_ViewBy_Population_Radio': ['UR-11', 'UR-13'],
      'ServiceUse_Hosp_ReportType_Inpatient_Radio': ['UR-1'],
      'ServiceUse_Hosp_ReportType_Emergency_Radio': ['UR-5'],

      'AHS_ConditionTopic_Radio': ['UR-8', 'UR-9'],
      'AHS_SpecificCounty_Radio': ['UR-10'],

      'AHS_ViewRates_Tab': ['UR-9'],
      'AHS_MapView_Tab': ['UR-8'],
      'IP_ViewRates_Tab': ['UR-1'],
      'IP_MapView_Tab': ['UR-3'],
      'ED_ViewRates_Tab': ['UR-5'],
      'ED_MapView_Tab': ['UR-7'],
      'County_ViewRates_Tab': ['UR-11'],
      'County_MapView_Tab': ['UR-11'],
      'Region_ViewRates_Tab': ['UR-13'],
      'Region_MapView_Tab': ['UR-13'],

      'Quality_ViewRates_Tab': ['QR-1'],
      'Quality_MapView_Tab': ['QR-1'],

      'Quality_Cond_Display_Symbols_Dropdown': ['QR-2'],
      'Quality_Cond_Display_SymbolsAndRAR_Dropdown': ['QR-3'],
      'Quality_Cond_Display_BarChart_Dropdown': ['QR-4'],
      'Quality_Cond_Display_RawData_Dropdown': ['QR-5'],

      'Quality_Compare_Column': ['QR-6', 'QR-7', 'QR-8'],
      'Quality_Compare_Display_Symbols_Dropdown': ['QR-6'],
      'Quality_Compare_Display_SymbolsAndRAR_Dropdown': ['QR-7'],
      'Quality_Compare_Display_BarChart_Dropdown': ['QR-8'],

      'Nursing_Explore_Button': ['NH-2'],
      'NH_ViewRates_Tab': ['NH-2'],
      'NH_MapView_Tab': ['NH-2'],
      'NH_Compare_Column': ['NH-3'],

      'Physician_Explore_Button': ['PH-1', 'PH-2'],

      'Resource_AboutQR_Hospital': ['QR-1', 'QR-2', 'QR-3', 'QR-4', 'QR-5', 'QR-6', 'QR-7', 'QR-8'],
      'Resource_AboutQR_AHS': ['UR-8', 'UR-9', 'UR-10'],

      'Infographic_Surgical': ['IG-1'],

      'Nursing_Content': ['NH-2']
    };

    var udDimensions = {
      'patientcounty': 'Patient County',
      'county': 'County',
      'region': 'Region',
      'patientregion': 'Patient Region',
      'zip': 'Zip Code',
      'hospital': 'Hospital Name',
      'hospitalType': 'Hospital Type',
      'condition': 'Health Condition or Topic',
      'mdc': 'Major Diagnosis Category',
      'drg': 'Diagnosis Related Group',
      'procedure': 'Procedure'
    };
    var udDimensionsInv = _.invert(udDimensions);

    var QRProfileReportId = "7af51434-5745-4538-b972-193f58e737d7";
    var NHProfileReportId = "87e04110-46b0-4cae-9592-022c3111fac7";

    var nhMeasureDisplays = ["Quality Measures", "Health Inspection", "Staffing"];
    var nhDisplays = ["Basic Descriptive Data", "Map", "Overall Score"].concat(nhMeasureDisplays);

    API.init = function (c) {
      config = c;
      activeReportAliases = _.filter(reportAliases, function (v, k) {
        return _.findWhere(config, {ID: k});
      });
    };

    API.webElementAvailable = function (elId) {
      return _.size(_.intersection(activeReportAliases, webElementDeps[elId])) > 0;
    };

    API.configForReport = function (reportId) {
      return _.findWhere(config, {ID: reportId});
    };

    API.getDimensionIdFrom = function (name) {
      return udDimensionsInv[name];
    };

    API.getDimensionNameFrom = function (id) {
      return udDimensions[id];
    };

    API.nhProfileHasDisplay = function(name) {
      var report = API.configForReport(NHProfileReportId);
      if (report) {
        return _.contains(report.Display, name);
      }

      return false;
    };

    API.nhProfileHasMeasures = function() {
      return _.any(nhMeasureDisplays, function(d) {
        return API.nhProfileHasDisplay(d);
      });
    };

    _.each(nhDisplays, function(name) {
      var funName = name.replace(/ /g, '');
      API['nhProfileHas' + funName] = function() {
        return API.nhProfileHasDisplay(name);
      };
    });


    API.qrProfileHasDescription = function () {
      var report = API.configForReport(QRProfileReportId);
      if (report) {
        return _.contains(report.Display, "Basic Descriptive Data");
      }

      return false;
    };

    API.qrProfileHasCCData = function () {
      return API.qrProfileHasCCDataMedicare() || API.qrProfileHasCCDataIP();
    };

    API.qrProfileHasCCDataIP = function () {
      var report = API.configForReport(QRProfileReportId);
      if (report) {
        return _.contains(report.Display, "Cost and Charge Data (All Patients)");
      }

      return false;
    };

    API.qrProfileHasCCDataMedicare = function () {
      var report = API.configForReport(QRProfileReportId);
      if (report) {
        return _.contains(report.Display, "Cost and Charge Data (Medicare)");
      }

      return false;
    };

    API.qrProfileHasMap = function (reportID) {
        var report = API.configForReport(reportID);
      if (report) {
        return _.contains(report.Display, "Map");
      }

      return false;
    };

    API.qrProfileHasPE = function () {
      var report = API.configForReport(QRProfileReportId);
      if (report) {
        return _.contains(report.Display, "Overall Patient Experience Rating");
      }

      return false;
    };

    API.hasCostQuality = function() {
      var r = API.configForReport('7d841284-5179-44e5-a00e-bdd042b0a7bd');
      return r != undefined;
    };

    API.entities = {
      HOSPITAL: 'HOSPITAL',
      NURSINGHOME: 'NURSINGHOME',
      PHYSICIAN: 'PHYSICIAN'
    };

    API.getEntities = function() {
      var entities = [];

      if (API.hasEntity(API.entities.HOSPITAL)) {
        entities.push(API.entities.HOSPITAL);
      }

      if (API.hasEntity(API.entities.NURSINGHOME)) {
        entities.push(API.entities.NURSINGHOME);
      }

      if (API.hasEntity(API.entities.PHYSICIAN)) {
        entities.push(API.entities.PHYSICIAN);
      }

      return entities;
    };

    API.hasEntity = function(entity) {
      if (entity === API.entities.HOSPITAL) {
        return API.webElementAvailable('Utilization_Tab') || API.webElementAvailable('Quality_Tab');
      }
      else if (entity === API.entities.NURSINGHOME) {
        return API.webElementAvailable('Nursing_Tab');
      }
      else if (entity === API.entities.PHYSICIAN) {
        return API.webElementAvailable('Physician_Tab');
      }
    };

    API.hasEntities = function(entities) {
      return _.all(entities, API.hasEntity);
    };

    return API;
  }

})();
