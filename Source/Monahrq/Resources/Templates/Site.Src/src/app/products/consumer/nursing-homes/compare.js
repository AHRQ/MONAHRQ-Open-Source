/**
 * Consumer Product
 * Nursing Home Reports Module
 * Compare Page Controller
 *
 * The compare page allows the user to compare how a set of nursing homes performed
 * for the four primary topics homes are scored by. If NH-CAHPS reports are available,
 * they will also be included.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.nursing-homes')
    .controller('CNHCompareCtrl', CNHCompareCtrl);


  CNHCompareCtrl.$inject = ['$scope', '$state', '$stateParams', 'NHRepositorySvc', 'CNHReportSvc', 'NHReportLoaderSvc', 'ResourceSvc',
    'ModalLegendSvc', 'ModalMeasureSvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc', 'UserStateSvc', 'nursingHomes', 'measureTopics',
  'NHCAHPSSvc', 'ModalGenericSvc', 'ModalTopicSvc'];
  function CNHCompareCtrl($scope, $state, $stateParams, NHRepositorySvc, CNHReportSvc, NHReportLoaderSvc, ResourceSvc,
    ModalLegendSvc, ModalMeasureSvc, ScrollToElSvc, ConsumerReportConfigSvc, UserStateSvc, nursingHomes, measureTopics,
    NHCAHPSSvc, ModalGenericSvc, ModalTopicSvc) {
    var report, model;
    var visibleTopics = {}, visibleInScore = {}, overallMeasures = [], sectionState = {};

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.columns = [];
    $scope.topics = [];
    $scope.model = {};
    $scope.overallMeasure = {};
    $scope.query = {};
    $scope.hasResults = false;
    $scope.hasSearch = false;

    $scope.modalTopic = modalTopic;
    $scope.updateSearch = updateSearch;
    $scope.getMeasureTitle = getMeasureTitle;
    $scope.hasOverallMeasure = hasOverallMeasure;
    $scope.toggleSection = toggleSection;
    $scope.showSection = showSection;
    $scope.getSectionStateName = getSectionStateName;
    $scope.backToReport = backToReport;
    $scope.modalLegend = modalLegend;
    $scope.modalMeasure = modalMeasure;
    $scope.showModalMeasureHeader = showModalMeasureHeader;


    init();

    function init() {
      var gcNursing = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_NURSING);
      $scope.query.compareTo = gcNursing ? gcNursing : 'national';
      $scope.$watch('query.compareTo', function(nv, ov) {
        if (nv === ov) return;
        UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_NURSING, nv);
      });

      $scope.model = [];
      $scope.query.ids = _.map($stateParams.ids.split(','), function(id) { return +id });
      loadData();
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

    function loadData() {
      $scope.columns = getColumns();

      loadOverallMeasures();

      $scope.topics = NHCAHPSSvc.withoutCAHPSTopics(measureTopics);
      _.each($scope.topics, loadTopicMeasures);

      NHReportLoaderSvc.getNursingHomeReports($scope.query.ids)
        .then(function(result) {
          ScrollToElSvc.scrollToEl('.compare .report');
          processReports(result);
        });

      loadCAHPSData();
    }

    function loadCAHPSData() {
      NHCAHPSSvc.loadCAHPSReport(measureTopics)
        .then(function(report) {
          $scope.CAHPSReport = report;
        });
    }

    function loadOverallMeasures() {
      ResourceSvc.getConfiguration()
        .then(function(config) {
          var overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID]);
          ResourceSvc.getNursingHomeMeasures(overallIDs)
            .then(function(measures) {
              overallMeasures = measures;
              $scope.overallMeasure = _.findWhere(measures, {MeasureID: config.NURSING_OVERALL_ID});
            });
        });
    }

    function loadTopicMeasures(topic) {
      ResourceSvc.getNursingHomeMeasures(topic.MeasureIDs)
        .then(function(measures) {
          topic.measures = measures;

          if (topic.SubsetInScore) {
            groupMeasures(topic);
          }
        });
    }

    function groupMeasures(topic) {
      var groupByType = _.groupBy(topic.measures, function(m) { return m.MeasureType; });
      _.each(_.keys(groupByType), function(key) {
        groupByType[key] = _.groupBy(groupByType[key], function(m) { return m.InScore; });
        //toggleInScore(key + '-inSummary');
      });

      topic.groupByType = groupByType;
    }

    function processReports(reports) {
      var model = {};


      _.each(reports, function(report) {
        model[report.id] = {};

        _.each(report.data, function(row) {
          model[report.id][row.MeasureID] = row;
        });
      });

      $scope.model = model;
    }

    function getColumns() {
      var nhs = _.filter(nursingHomes, function (nh) {
        return _.contains($scope.query.ids, nh.ID);
      });

      var columns = _.map(nhs, function (nh) {
        return {
          id: nh.ID,
          name: nh.Name
        };
      });

      return columns;
    }

    function getMeasureTitle(id) {
      var title;

      _.each(overallMeasures, function(measure) {
        if (measure.MeasureID === id) {
          title = measure.SelectedTitleConsumer;
        }
      });

      return title;
    }

    function hasOverallMeasure(id) {
      return _.findWhere(overallMeasures, {MeasureID: id});
    }

    function handleNoResults() {
      $scope.model = [];
      $scope.hasResults = false;

      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function updateSearch() {
      $state.go('^.compare', {
        ids: $scope.query.ids.join(',')
      });
    }

    function getSectionId(type, section) {
      var id;
      if (type == 'topics') {
        id = type;
      }
      else if (type == 'topic') {
        id = type + section.TopicID;
      }
      else if (type == 'topicGroups') {
        id = type;
      }
      else if (type == 'subtopic') {
        id = type + section;
      }
      else if (type == 'inScore' || type == 'notInScore') {
        id = type + 'Score' + section;
      }
      else if (type == 'CAHPSTopic') {
          id = type + section.TopicID;
      }

      return id;
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
          sectionState['topic'+s.TopicID] = sectionState[id];

          if (s.groupByType) {
            _.each(_.keys(s.groupByType), function(g) {
              sectionState['inScore' + 'Score' + g] = true;
              sectionState['subtopic' + g] = sectionState[id];
            });
          }
        });
        sectionState['topicGroups'] = true;

        //toggle CAHPSReport ratings
        if (!$scope.CAHPSReport.isEmpty($scope.CAHPSReport)) {
            _.each($scope.CAHPSReport.topics, function (t) {
                sectionState['CAHPSTopic' + t.TopicID] = sectionState[id];
            });
        }
      }
      else if (type == 'topicGroups') {
        if (section.groupByType) {
          _.each(_.keys(section.groupByType), function (g) {
            sectionState['inScore' + 'Score' + g] = sectionState[id];
            sectionState['subtopic' + g] = sectionState[id];
          });
        }
      }
      else if (type == 'subtopic') {
        sectionState['inScoreScore' + section] = sectionState['subtopic' + section];
        sectionState['notInScoreScore' + section] = false;
      }
    }

    function showSection(type, section) {
      var id = getSectionId(type, section);
      return sectionState[id];
    }

    function getSectionStateName(type, section) {
      var id = getSectionId(type, section);
      var names = {
        topics: {
          true: 'Hide All Ratings',
          false: 'Show All Ratings'
        },
        topic: {
          true: 'Hide Ratings',
          false: 'Show Ratings'
        },
        CAHPSTopic: {
            true: 'Hide Ratings',
            false: 'Show Ratings'
        },
        topicGroups: {
          true: 'Collapse All',
          false: 'Expand All'
        },
        inScore: {
          true: 'Hide',
          false: 'Show'
        },
        notInScore: {
          true: 'Hide',
          false: 'Show'
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
        $state.go('top.consumer.nursing-homes.location');
      }
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openNursingMeasure(id);
    }

    function modalTopic(id) {
      ModalTopicSvc.openNursingTopic(id);
    }


    function showModalMeasureHeader() {
        var modalContent = "The ratings below are based on information from the  Nursing Home CAHPS - Family Member Survey. The survey is filled out by adult family members of nursing home residents who have stayed in the nursing home for longer than 100 days. The survey asks the family members about their experiences with care and services at the nursing home. To learn more information about how the rates are calculated, review the About the Ratings.";
        ModalGenericSvc.open('Survey Summary – Overall Rating of Care', modalContent);
    }
}

})();


