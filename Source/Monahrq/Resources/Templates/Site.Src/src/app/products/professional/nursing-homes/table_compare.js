/**
 * Professional Product
 * Nursing Homes Module
 * Compare Table Controller
 *
 * The compare page allows the user to compare how a set of nursing homes performed
 * for the four primary topics homes are scored by. If NH-CAHPS reports are available,
 * they will also be included.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHTableCompareCtrl', NHTableCompareCtrl);

  NHTableCompareCtrl.$inject = ['$scope', '$state', '$q', '_', '$stateParams',
    'NHQuerySvc', 'NHReportLoaderSvc', 'ResourceSvc', 'SortSvc',
    'ReportConfigSvc', 'nursingHomes', 'measureTopics', 'ModalMeasureSvc', 'ModalLegendSvc', 'ModalTopicSvc', 'ModalGenericSvc', 'NHCAHPSSvc'];
  function NHTableCompareCtrl($scope, $state, $q, _, $stateParams,
    NHQuerySvc, NHReportLoaderSvc, ResourceSvc, SortSvc,
    ReportConfigSvc, nursingHomes, measureTopics, ModalMeasureSvc, ModalLegendSvc, ModalTopicSvc, ModalGenericSvc, NHCAHPSSvc) {

    var visibleTopics = {}, visibleInScore = {}, nhIds = [], overallMeasures = [];

    $scope.columns = [];
    $scope.topics = [];
    $scope.model = {};
    $scope.query = {
      comparedTo: 'nat'
    };
    $scope.reportSettings = {};
    $scope.overallMeasure = {};

    $scope.showTopic = showTopic;
    $scope.toggleTopic = toggleTopic;
    $scope.showInScore = showInScore;
    $scope.toggleInScore = toggleInScore;
    $scope.backToReport = backToReport;
    $scope.canBackToReport = canBackToReport;
    $scope.selectText = selectText;
    $scope.modalTopic = modalTopic;
    $scope.modalMeasure = modalMeasure;
    $scope.modalLegend = modalLegend;
    $scope.modalGroup = modalGroup;
    $scope.modalInSummary = modalInSummary;
    $scope.getMeasureTitle = getMeasureTitle;
    $scope.hasOverallMeasure = hasOverallMeasure;
      $scope.showModalMeasureHeader = showModalMeasureHeader;

    init();

    function init() {
      nhIds = $stateParams.ids;// ? $stateParams.ids.split(/,/) : [];
      nhIds = _.map(nhIds, function(id) {
        return +id;
      });

      loadData();
      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ReportConfigSvc.configForReport(id);
      $scope.reportSettings.header = report.ReportHeader;
      $scope.reportSettings.footer = report.ReportFooter;
    }

    function loadData() {
      $scope.columns = getColumns();

      loadOverallMeasures();

      $scope.topics = measureTopics;
      _.each($scope.topics, loadTopicMeasures);

      NHReportLoaderSvc.getNursingHomeReports(nhIds)
        .then(processReports);

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
        toggleInScore(key + '-inSummary');
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
        return _.contains(nhIds, nh.ID);
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
          title = measure.SelectedTitle;
        }
      });

      return title;
    }

    function hasOverallMeasure(id) {
      return _.findWhere(overallMeasures, {MeasureID: id});
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

    function showInScore(id) {
      return _.has(visibleInScore, id);
    }

    function toggleInScore(id) {
      if ($scope.showInScore(id)) {
        delete visibleInScore[id];
      }
      else {
        visibleInScore[id] = true;
      }
    }

    function canBackToReport() {
      if ($state.previous.name && !$state.previous.name['abstract'])
        return true;
      return false;
    }

    function backToReport() {
      if ($state.previous.name) {
        var stateName = $state.previous.name;
        var stateParams = $state.previous.params;
      }

      if (stateName)
        $state.go(stateName, stateParams);
    }

    function selectText(element) {
      var doc = document, text = doc.getElementById(element), range, selection;
      if (doc.body.createTextRange) { //ms
        range = doc.body.createTextRange();
        range.moveToElementText(text);
        range.select();
      } else if (window.getSelection) { //all others
        selection = window.getSelection();
        range = doc.createRange();
        range.selectNodeContents(text);
        selection.removeAllRanges();
        selection.addRange(range);
      }
    }

    function modalTopic(id) {
      ModalTopicSvc.openNursingTopic(id);
    }

    function modalMeasure(id) {
        ModalMeasureSvc.openNursingMeasure(id);
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function modalGroup(topicId, groupName) {
      var topic = _.findWhere($scope.topics, {TopicID: topicId});

      if (/long/i.test(groupName)) {
        ModalGenericSvc.open(groupName, topic.GroupingModalsContent.LongStay);
      }
      else if (/short/i.test(groupName)) {
        ModalGenericSvc.open(groupName, topic.GroupingModalsContent.ShortStay);
      }
    }

    function modalInSummary(topicId, name) {
      var topic = _.findWhere($scope.topics, {TopicID: topicId});

      if (/^in/i.test(name)) {
        ModalGenericSvc.open('Help', topic.GroupingModalsContent.InScore);
      }
      else if (/^notIn/i.test(name)) {
        ModalGenericSvc.open('Help', topic.GroupingModalsContent.NotInScore);
      }
    }

    function showModalMeasureHeader() {
        var modalContent = "The ratings below are based on information from the  Nursing Home CAHPS - Family Member Survey. The survey is filled out by adult family members of nursing home residents who have stayed in the nursing home for longer than 100 days. The survey asks the family members about their experiences with care and services at the nursing home. To learn more information about how the rates are calculated, review the About the Ratings.";
        ModalGenericSvc.open('Survey Summary � Overall Rating of Care', modalContent);
    }
}



})();

