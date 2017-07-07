/**
 * Consumer Product
 * Physicians Report Module
 * Physician Profile Page Controller
 *
 * This controller generates the profile page for a given physician. Most of the details are
 * delegated to two child views that share this controller. This report shows basic physician
 * profile information, the HEDIS and CG-CAHPS reports if available, and a list of hospitals
 * the physician is affiliated with.
 */

(function() {
  'use strict';

  /**
   * Angular wiring
   */

  angular.module('monahrq.products.consumer.physicians')
    .controller('PProfileCtrl', PProfileCtrl);

  PProfileCtrl.$inject = ['$scope', '$state', '$stateParams', '$q', 'ResourceSvc', 'PhysicianAPISvc', 'PhysicianReportLoaderSvc',
    'PhysicianDedupeSvc', 'ScrollToElSvc', 'ModalLegendSvc', 'ConsumerReportConfigSvc', 'hospitals', 'PhysicianCAHPSSvc',
    'ModalTopicCategorySvc', 'ModalTopicSvc', 'ModalMeasureSvc', 'ModalGenericSvc', 'ReportConfigSvc'];
  function PProfileCtrl($scope, $state, $stateParams, $q, ResourceSvc, PhysicianAPISvc, PhysicianReportLoaderSvc,
                        PhysicianDedupeSvc, ScrollToElSvc, ModalLegendSvc, ConsumerReportConfigSvc, hospitals, PhysicianCAHPSSvc,
                        ModalTopicCategorySvc, ModalTopicSvc, ModalMeasureSvc, ModalGenericSvc, ReportConfigSvc) {
    var _config;
    var visibleTopics = {}, visibleTopicCats = {};
    var reportConfig;
    var practices;

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.query = {
      name: null
    };
    $scope.showValidationErrors = false;
    $scope.allTopics = [];

    $scope.search = search;
    $scope.hasPractices = hasPractices;
    $scope.modalLegend = modalLegend;
    $scope.modalTopicCategory = modalTopicCategory;
    $scope.modalTopic = modalTopic;
    $scope.modalMeasure = modalMeasure;
    $scope.modalMeasureHedis = modalMeasureHedis;
    $scope.modalReport = modalReport;
    $scope.showTopic = showTopic;
    $scope.toggleTopic = toggleTopic;
    $scope.showTopicCat = showTopicCat;
    $scope.toggleTopicCat = toggleTopicCat;
    $scope.getPractice = getPractice;
    $scope.numPractices = numPractices;
    $scope.loadCAHPSData = loadCAHPSData;
    $scope.selectPractice = selectPractice;
    $scope.coalesce = coalesce;
    $scope.reportSettings.tabAPI = {};
    $scope.reportSettings.showHedis = true;
    $scope.toggleAllTopics = toggleAllTopics;

    init();


    function init() {
      reportConfig = ConsumerReportConfigSvc.configForReport($scope.reportId);

      loadDataById();
      $scope.searchStatus = 'NOT_STARTED';
      $scope.tab = 'gen-info';
      $scope.goToTab = goToTab;

      $scope.wrapperClass = function() {
        return 'page--physicians';
      };

      $scope.content = {
        title: ''
      };
      setupReportHeaderFooter();

      $scope.$watch(function() {
        if (_.isFunction($scope.reportSettings.tabAPI.getActiveTab)) {
          return $scope.reportSettings.tabAPI.getActiveTab();
        }

        return null;
      }, function() {
          setupReportHeaderFooter();
      });
    }

    function getActiveTab() {
      return _.isFunction($scope.reportSettings.tabAPI.getActiveTab) ? $scope.reportSettings.tabAPI.getActiveTab() : null;
    }

    function getChildConfig(report, activeTab) {
      var config;

      if (_.isObject(report.ChildConfig) && activeTab) {
        if (activeTab === 'affiliations' && report.ChildConfig.HospitalAffiliation.HelpOverride) {
          config = report.ChildConfig.HospitalAffiliation;
        }
        else if (activeTab === 'practiceRatings' && report.ChildConfig.MedicalPractice.HelpOverride) {
          config = report.ChildConfig.MedicalPractice;
        }
      }

      return config;
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      var activeTab = getActiveTab();
      var childConfig = getChildConfig(report, activeTab);
      var header = report.ReportHeader;
      var footer = report.ReportFooter;

      if (!report) return;

      if (childConfig) {
        header = childConfig.Header;
        footer = childConfig.Footer;
      }

      $scope.reportSettings.header = header;
      $scope.reportSettings.footer = footer;
    }

    function goToTab(tab) {
      $scope.tab = tab;
    }

    function hasHedis() {
      return _.isArray(reportConfig.Display) && _.contains(reportConfig.Display, 'HEDIS Measures');
    }

    function hasCAHPS() {
      return _.isArray(reportConfig.Display) && _.contains(reportConfig.Display, 'Medical Practice');
    }

    function hasPractices() {
      return $scope.physicianData && _.size($scope.physicianData.practices) > 0;
    }

    function numPractices() {
      return _.size($scope.physicianData.practices);
    }

    function makeGmapUrl(physician) {
      var start = "http://maps.googleapis.com/maps/api/staticmap?center=",
        end = "&maptype=roadmap&size=340x350&sensor=true", addr, zoom, marker="";

      if (_.has($scope.config, 'DE_IDENTIFICATION') && $scope.config.DE_IDENTIFICATION === 1) {
        zoom = "&zoom=3";
        addr = "USA";
      }
      else {
        zoom = "&zoom=13";
        addr = escape(physician.adr_ln_1 + ',' + physician.cty + ',' + physician.st + ',' + physician.zip);
        marker = "&markers=" + addr;
      }

      return start + addr + zoom + end + marker;
    }

    function loadDataById() {
      if (!$stateParams.id) {
        return;
      }
      $scope.physicianId = $stateParams.id;

      $scope.searchStatus = 'SEARCHING';

      ResourceSvc.getConfiguration()
        .then(function(config) {
          _config = config;

          if (config.USED_REAL_TIME == 1) {
            PhysicianAPISvc.init(config);
            PhysicianAPISvc.getById($stateParams.id)
              .then(processData);
          }
          else {
            PhysicianReportLoaderSvc.init(config);
            PhysicianReportLoaderSvc.getById($stateParams.id)
              .then(processData);
          }


          if (hasHedis()) {
            loadHedisData();
          }

          if (hasCAHPS()) {
            loadCAHPSData();
          }
        });
    }

    function loadHedisData() {
      PhysicianReportLoaderSvc.getHedisReport($stateParams.id)
        .then(function(report) {
          report.groupedRows = _.groupBy(report.rows, 'practiceId');
          $scope.hedisReport = report;
        })
    }

    function loadCAHPSData() {
      var cahpsTopicCategories, cahpsTopics;

      ResourceSvc.getPhysicianPratices()
      .then(function(result) {
        practices = result;
        return ResourceSvc.getMedicalPracticeMeasureTopicCategories()
      })
      .then(function(result) {
        cahpsTopicCategories = result;
        return ResourceSvc.getMedicalPracticeMeasureTopics();
      })
      .then(function(result) {
        cahpsTopics = result;
        $scope.allTopics = cahpsTopics;
        return $scope.reportSettings.activePractice ? PhysicianCAHPSSvc.loadCAHPSReport(cahpsTopics, $scope.reportSettings.activePractice.id) : {};
      })
      .then(function(result) {
        $scope.CAHPSReport = result;
        $scope.CAHPSReport.topicCategories = cahpsTopicCategories;
      });

    }

    function processData(physicians) {
      var dedupe = PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);

      if (dedupe.length >= 1) {
        $scope.searchStatus = 'COMPLETED';
        ScrollToElSvc.scrollToEl('.profile .report');
      }
      else {
        $scope.searchStatus = 'NO_RESULTS';
        return;
      }

      var firstPhysiciansObj = _.first(dedupe),
        genInfo = {
          npi: firstPhysiciansObj.npi,
          pacID: firstPhysiciansObj.ind_pac_id,
          enrollID: firstPhysiciansObj.ind_enrl_id,
          cred: firstPhysiciansObj.cred,
          gradYear: firstPhysiciansObj.grd_yr,
          school: firstPhysiciansObj.med_sch,
          primarySpecialty: firstPhysiciansObj.pri_spec,
          secondarySpecialty: firstPhysiciansObj.sec_spec_all,

          assgn: firstPhysiciansObj.assgn,
          erx: firstPhysiciansObj.erx,
          pqrs: firstPhysiciansObj.pqrs,
          ehr: firstPhysiciansObj.ehr
        },
        gender = firstPhysiciansObj.gndr,
        hosAfls = [];

      $scope.gMap = makeGmapUrl(firstPhysiciansObj);
      $scope.physicianData = firstPhysiciansObj;
      $scope.genInfo = genInfo;

      if (firstPhysiciansObj.cred === undefined ||  firstPhysiciansObj.cred === null) {
        firstPhysiciansObj.cred = '';
      }
      if (firstPhysiciansObj.mid_nm === undefined || firstPhysiciansObj.mid_nm === null) {
        firstPhysiciansObj.mid_nm = '';
      }

      $scope.physicianName = firstPhysiciansObj.frst_nm + ' '
      + firstPhysiciansObj.mid_nm + ' ' + firstPhysiciansObj.lst_nm
      + ' ' + firstPhysiciansObj.cred;

      if (gender === 'Male' || gender === 'M') {
        genInfo.gender = 'Male';
      } else if (gender === 'Female' || gender === 'F'){
        genInfo.gender = 'Female';
      }


      genInfo.erx = (genInfo.erx == 'Y' ? true : false);
      genInfo.pqrs = (genInfo.pqrs == 'Y' ? true: false);
      genInfo.ehr = (genInfo.ehr == 'Y' ? true : false);

      $scope.practices = [];
      if (firstPhysiciansObj.practices) {
        $scope.practices = _.map(firstPhysiciansObj.practices, function(p) {
          var p2 = _.findWhere(practices, {GroupPracticePacId: p.org_pac_id});
          return {
            id: p.org_pac_id,
            name: p.org_lgl_nm,
            numMembers: p2 ? p2.NumberofGroupPracticeMembers : null
          };
        });

        $scope.reportSettings.activePractice = _.first($scope.practices);
      }

      relatedHospitals(firstPhysiciansObj);
    }

    function relatedHospitals(physician) {
      var hospPromises = [];
      $scope.genInfo.hospitalProfiles = [];

      if (_config.USED_REAL_TIME == 1) {
        var afl_fields = ['hosp_afl_1', 'hosp_afl_2', 'hosp_afl_3', 'hosp_afl_4', 'hosp_afl_5'];
        _.each(afl_fields, function(field) {
          if (!_.has(physician, field)) return;
          var providerId = physician[field];
          var hosp = _.findWhere(hospitals, {"HospitalProviderId": providerId});
          if (hosp == null) return;
          hospPromises.push(ResourceSvc.getHospitalProfile(hosp.Id));
        });
        $q.all(hospPromises)
          .then(function(profiles) {
            $scope.genInfo.hospitalProfiles = profiles;
          });
      }
      else {
        ResourceSvc.getPhysicianHospitalAffiliation()
          .then(function(affiliations) {
            var providerIds = _.pluck(_.where(affiliations, {Npi: +physician.npi}), 'HospitalProviderID');
            _.each(hospitals, function(h) {
              if(_.contains(providerIds, h.HospitalProviderId)) {
                hospPromises.push(ResourceSvc.getHospitalProfile(h.Id));
              }
            });
            $q.all(hospPromises)
              .then(function(profiles) {
                $scope.genInfo.hospitalProfiles = profiles;
              });
          });
      }
    }

    function canSearch() {
      return $scope.query.name;
    }

    function search() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }
      $state.go('top.consumer.profile-search', {type: 'physician', term: $scope.query.name});
    }

    function modalLegend(){
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      var activeTab = getActiveTab();
      var childConfig = getChildConfig(report, activeTab);
      ModalLegendSvc.open(id, null, childConfig);
    }

    function modalTopicCategory(id) {
      ModalTopicCategorySvc.openPhysicianTopicCategory(id);
    }

    function modalTopic(id) {
      ModalTopicSvc.openPhysicianTopic(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openPhysicianMeasure(id);
    }

    function modalMeasureHedis(id) {
      ModalMeasureSvc.openPhysicianHedisMeasure(id);
    }

    function modalReport(report) {
      var help = {
        'HEDIS': {
          title: 'Medical Group Ratings of Medical Care for Diabetes, Asthma, High Blood Pressure, and COPD',
          content: 'The ratings below are based on information from the Healthcare Effectiveness Data and Information Set (HEDIS) which reports the percentage of patients receiving the appropriate care from their medical group for the following conditions: diabetes, asthma, high blood pressure, and chronic obstructive pulmonary disease (COPD). To learn more information about how the rates are calculated, review the About the Ratings page.'
        },
        'CAHPS': {
          title: 'Medical Group Patient Survey Results',
          content: 'The ratings below are based on information from the Clinician & Group CAHPS 2.0 Visit Survey (CG-CAHPS) which ask adults and children about their experience with care at their most recent visit to a provider’s office. The ratings shown are for the entire medical group, combining results for each provider. To learn more information about how the rates are calculated, review the About the Ratings page.'
        }
      };

      ModalGenericSvc.open(help[report].title, help[report].content);
    }

    function showTopic(id) {
      return _.has(visibleTopics, id);
    }

    function toggleTopic(id) {
      if ($scope.showTopic(id)) {
        delete visibleTopics[id];
      }
      else {
        visibleTopics[id] = true;
      }
    }

    function toggleAllTopics(topicCatId) {
      $scope.topicCatId = topicCatId;
      _.each($scope.allTopics, function(topic) {
        if ( topic.TopicCategoryID == $scope.topicCatId )
          toggleTopic(topic.TopicID);
      });
    }

    function showTopicCat(id) {
      return _.has(visibleTopicCats, id);
    }

    function toggleTopicCat(id) {
      if ($scope.showTopicCat(id)) {
        delete visibleTopicCats[id];
      }
      else {
        visibleTopicCats[id] = true;
      }
    }

    function getPractice(rec) {
      return _.findWhere($scope.practices, {id: rec.id});
    }

    function selectPractice(id) {
      $scope.reportSettings.activePractice = _.findWhere($scope.practices, {id: id});

      if ($scope.reportSettings.tabAPI) {
        $scope.reportSettings.tabAPI.setActiveTab('practiceRatings');
        ScrollToElSvc.scrollToEl('.profile .report');
      }
    }

    function coalesce() {
        var result = null;
        for (var index = 0; index < arguments.length; index++) {
            result = arguments[index];
            if (result != undefined && result != null)
                return result;
        }
        return null;
    }
  }

})();
