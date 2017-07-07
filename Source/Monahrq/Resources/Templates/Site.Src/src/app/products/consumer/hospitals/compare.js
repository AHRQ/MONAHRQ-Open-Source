/**
 * Consumer Product
 * Hospital Reports Module
 * Compare Page Controller
 *
 * The compare page allows the user to compare how a set of hospitals performed
 * for the topics the user selects.
 *
 * The controller loads hospitals, measure definitions, and quality reports based
 * on the user selections. A model is then build from the data in a format that
 * corresponds to the page's grouped tabular structure.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHCompareCtrl', CHCompareCtrl);


  CHCompareCtrl.$inject = ['$scope', '$state', '$stateParams', 'ResourceSvc', 'HospitalRepositorySvc', 'CHReportSvc', 'HospitalReportLoaderSvc',
  'ModalLegendSvc', 'ModalTopicCategorySvc', 'ModalTopicSvc', 'ModalMeasureSvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc', 'UserStateSvc', 'topics', 'subtopics'];
  function CHCompareCtrl($scope, $state, $stateParams, ResourceSvc, HospitalRepositorySvc, CHReportSvc, HospitalReportLoaderSvc,
  ModalLegendSvc, ModalTopicCategorySvc, ModalTopicSvc, ModalMeasureSvc, ScrollToElSvc, ConsumerReportConfigSvc, UserStateSvc, topics, subtopics) {
    var hospitalIds, reports, model;
    var sectionState = {}, measureDefs = {};

    $scope.reportId = $state.current.data.report;
    $scope.srMessages = [];
    $scope.reportSettings = {};
    $scope.query = {};
    $scope.hasResults = false;
    $scope.conditions = [];
    $scope.concerns = [];
    $scope.topics = topics;
    $scope.subtopics = subtopics;

    $scope.updateSearch = updateSearch;
    $scope.toggleSection = toggleSection;
    $scope.showSection = showSection;
    $scope.getSectionStateName = getSectionStateName;
    $scope.backToReport = backToReport;
    $scope.gotoCostQuality = gotoCostQuality;
    $scope.modalLegend = modalLegend;
    $scope.modalTopic  = modalTopic;
    $scope.modalSubtopic  = modalSubtopic;
    $scope.modalMeasure = modalMeasure;
    $scope.showSupportsCost = showSupportsCost;
    $scope.addSrMsg = addSrMsg;
    $scope.getVisibilityName = getVisibilityName;
    $scope.showSymbol = showSymbol;

    init();

    function init() {
      $scope.model = [];

      var gcHospital = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL);
      $scope.query.compareTo = gcHospital ? gcHospital : 'state';

      $scope.query.ids = _.map($stateParams.ids.split(','), function(id) { return +id });

      $scope.query.topicIds = {};
      $scope.$watchCollection('query.topicIds', onTopicFilterChange);
      if ($stateParams.topicId) {
        $scope.query.topicIds[$stateParams.topicId] = true;
      }

      $scope.$watch('query.compareTo', function(nv, ov) {
        if (nv === ov) return;
        UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL, nv);
      });

      resetSectionState();
      buildFilterTopics();
      buildReportTopics();
      loadReport();
      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function loadReport() {
      HospitalRepositorySvc.init()
        .then(function() {
          $scope.hospitals = _.map($scope.query.ids, function(id) {
            return HospitalRepositorySvc.getIndexRecord(id);
          });

          return ResourceSvc.getMeasureDefs(getAllMeasureIds());
        })
        .then(function(_measureDefs) {
          _.each(_measureDefs, function (def) {
            measureDefs[def.MeasureID] = def;
          });
          $scope.measureDefs = measureDefs;

          if (_.size($scope.query.topicIds) > 0) {
            onTopicFilterChange(1, 2);
          }

          return HospitalReportLoaderSvc.getQualityByHospitalReports($scope.query.ids);
        })
        .then(function(_reports) {
          reports = _reports;
          updateReport();
          ScrollToElSvc.scrollToEl('.compare .report');
        });
    }

    function getAllMeasureIds() {
      var ids = [];
      _.each(subtopics, function (m) {
        if (m.Measures) {
          ids = _.union(ids, m.Measures);
        }
      });

      return ids;
    }

    function resetSectionState() {
      sectionState = {
        topic_filter: true
      };
    }

    function onTopicFilterChange(nv, ov) {
      if (nv == ov) return;

      $scope.hasResults = _.any($scope.query.topicIds);
      resetSectionState();
      buildReportTopics();
    }

    function buildFilterTopics() {
      $scope.topicsList = _.map(topics, function(topic) {
        return {
          id: topic.TopicCategoryID,
          label: topic.Name
        };
      });
    }

    function buildReportTopics() {
      var reportTopics = [];
      _.each(topics, function(t) {
        if (!_.has($scope.query.topicIds, t.TopicCategoryID) || $scope.query.topicIds[t.TopicCategoryID] == false) return;
        reportTopics.push({
          id: t.TopicCategoryID,
          name: t.Name,
          description: t.Description,
          subtopics: subtopicsFor(t.TopicCategoryID)
        });
      });

      function subtopicsFor(id) {
        return _.map(_.where(subtopics, {TopicCategoryID: id}), function(t) {
          return {
            id: t.TopicID,
            name: t.Name,
            measures: measuresFor(t.Measures)
          };
        });
      }

      function measuresFor(measures) {
        return _.map(measures, function(m) {
          var name = _.has(measureDefs, m) ? measureDefs[m].SelectedTitleConsumer : null;
          var supportsCost = _.has(measureDefs, m) ? measureDefs[m].SupportsCost : null;
          return {
            id: m,
            name: name,
            supportsCost: supportsCost
          }
        });
      }

      $scope.reportTopics = reportTopics;

      //if ($stateParams.topicId) {
        //toggleSection('topics', reportTopics);
        initSections();
      //}
    }

    function handleNoResults() {
      $scope.model = [];
      $scope.hasResults = false;

      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function updateReport() {
      var model = {};

      _.each(reports, function(reportWrapper) {
        var report = reportWrapper.data;

        _.each(report, function(row) {
          var mrow;
          if (_.has(model, row.MeasureID)) {
            mrow = model[row.MeasureID];
          }
          else {
            mrow = {
              id: row.MeasureID,
              hospitalId: reportWrapper.id
            };
         }

          mrow[reportWrapper.id] = {
            natRating: row.NatRating,
            peerRating: row.PeerRating,
            RateAndCI: row.RateAndCI
          };

          model[row.MeasureID] = mrow;
        })
      });

      $scope.model = model;
    }

    function getMeasureName(id) {
      return 'measure ' + id;
    }

    function updateSearch() {
      $state.go('^.compare', {
        ids: $scope.query.ids.join(',')
      });
    }

    function getSectionId(type, section) {
      var id;
      if (type == 'topics' || type == 'topic_filter') {
        id = type;
      }
      else {
        id = type + section.id;
      }

      return id;
    }

    function initSections() {
      toggleSection('topics', $scope.reportTopics);
      _.each($scope.reportTopics, function(topic) {
        toggleSection('subtopics', topic);
      });
    }

    function toggleSection(type, section) {
      var id = getSectionId(type, section);
      if (!_.has(sectionState, id)) {
        sectionState[id] = true;
      }
      else {
        sectionState[id] = !sectionState[id]
      }

      if (type == 'topics') {
        _.each(section, function(s) {
          sectionState['topic'+s.id] = sectionState[id];
        });
      }
      else if (type == 'subtopics') {
        _.each(section.subtopics, function(s) {
          sectionState['subtopic'+s.id] = sectionState[id];
        });
      }
    }

    function showSection(type, section) {
      var id = getSectionId(type, section);
      return sectionState[id];
    }

    function getSectionStateName(type, section) {
      var id = getSectionId(type, section);
      var names = {
        topic_filter: {
          true: 'Hide Topics',
          false: 'Show Topics'
        },
        topics: {
          true: 'Hide All Ratings',
          false: 'Show All Ratings'
        },
        topic: {
          true: 'Hide Ratings',
          false: 'Show Ratings'
        },
        subtopics: {
          true: 'Collapse All',
          false: 'Expand All'
        },
        subtopic: {
          true: 'Hide',
          false: 'Show'
        }
      };
      var state = sectionState[id] || false;

      return names[type][state];
    }

    function backToReport($event) {
      $event.preventDefault();

      if ($state.previous.name) {
        var stateName = $state.previous.name.name;
        var stateParams = $state.previous.params;
      }

      if (stateName && _.isString(stateName) && stateName.length > 0) {
        $state.go(stateName, stateParams);
      }
      else {
        $state.go('top.consumer.hospitals.location');
      }
    }

    function showSupportsCost(measure) {
      if (measure.supportsCost && ConsumerReportConfigSvc.hasCostQuality()) {
        return true;
      }

      return false;
    }

    function gotoCostQuality(subtopic) {
      $state.go("^.cost-quality", {
        subtopicId: subtopic.id,
        hospitalIds: $scope.query.ids.join(',')
      });
    }

    function modalLegend() {
      var id = $state.current.data.report;
      ModalLegendSvc.open(id, 'symbols');
    }

    function modalTopic(id) {
      ModalTopicCategorySvc.open(id);
    }

    function modalSubtopic(id) {
      ModalTopicSvc.open(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.open(id);
    }

    function addSrMsg(msg) {
      $scope.srMessages.push(msg);
    }

    function getVisibilityName(topicId) {
      if ($scope.query.topicIds[topicId]) {
        return 'Removed ';
      }
      else {
        return 'Added ';
      }
    }

    function showSymbol(measure) {
      if (measure == undefined) {
        return;
      }

      var field = $scope.query.compareTo == 'national' ? 'natRating' : 'peerRating';

      if (measure[field] == -1 && measure.RateAndCI != '-') {
        return false;
      }

      return true;
    }

  }

})();


