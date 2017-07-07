/**
 * Professional Product
 * Nursing Homes Module
 * Profile Page Ratings Table Controller
 *
 * This controller shows the star ratings a given nursing home received in the four
 * primary topics, and the NH-CAHPS report if available.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHProfileRatingsCtrl', NHProfileRatingsCtrl);

  NHProfileRatingsCtrl.$inject = ['$scope', '$q', 'ResourceSvc', 'NHReportLoaderSvc', 'ReportConfigSvc', 'SortSvc',
    'ModalMeasureSvc', 'ModalTopicSvc', 'ModalGenericSvc', 'NHCAHPSSvc', 'measureTopics'];
  function NHProfileRatingsCtrl($scope, $q, ResourceSvc, NHReportLoaderSvc, ReportConfigSvc, SortSvc,
                                ModalMeasureSvc, ModalTopicSvc, ModalGenericSvc, NHCAHPSSvc, measureTopics) {
    var visibleTopics = {}, visibleInScore = {};
    var nursingHomeId = $scope.profile.ID;
    var overallMeasures = [];

    $scope.topics = [];
    $scope.ratings = {};
    $scope.CAHPSReport = {};
    $scope.nhid = nursingHomeId;

    $scope.showTopic = showTopic;
    $scope.toggleTopic = toggleTopic;
    $scope.showInScore = showInScore;
    $scope.toggleInScore = toggleInScore;
    $scope.modalTopic = modalTopic;
    $scope.modalGroup = modalGroup;
    $scope.modalInSummary = modalInSummary;
    $scope.modalMeasure = modalMeasure;
    $scope.getMeasureTitle = getMeasureTitle;
    $scope.getRating = getRating;
    $scope.hasOverallMeasure = hasOverallMeasure;
    $scope.coalesce = coalesce;
    $scope.showModalMeasureHeader = showModalMeasureHeader;
    $scope.showLoadedCAHPSTopics = showLoadedCAHPSTopics;

    init();


    function init() {
      loadData();
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

    function showLoadedCAHPSTopics() {
      _.each($scope.loadedCAHPStopics, function(topic) {
        toggleTopic(topic.TopicID);
      });
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

    function loadData() {
      loadOverallMeasures();

      ResourceSvc.getNursingHomeMeasureTopics()
        .then(function(data) {
          $scope.topics = NHCAHPSSvc.withoutCAHPSTopics(measureTopics);
          _.each($scope.topics, loadTopicMeasures);
        });

      NHReportLoaderSvc.getNursingHomeReport(nursingHomeId)
        .then(processReport);

      loadCAHPSData();
    }

    function loadCAHPSData() {
      NHCAHPSSvc.loadCAHPSReport(measureTopics)
        .then(function(report) {
          $scope.CAHPSReport = report;
          $scope.CAHPSReport.showCAHPS = false;
          _.each(report.topics, function(topic) {
            $scope.loadedCAHPStopics = topic;
            visibleTopics[topic.TopicID] = true;
          });
        });
    }

    function loadOverallMeasures() {
      ResourceSvc.getConfiguration()
        .then(function(config) {
          var overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID]);
          ResourceSvc.getNursingHomeMeasures(overallIDs)
            .then(function(measures) {
              overallMeasures = measures;
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

    function processReport(report) {
      _.each(report.data, function(row) {
        $scope.ratings[row.MeasureID] = row;
      });
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

    function getRating(row) {
      if (row == undefined) {
        return null;
      }

      if ($scope.query.comparedTo === 'nat') {
        return row.NatRating || 0;
      }
      else if ($scope.query.comparedTo === 'peer') {
        return row.PeerRating || 0;
      }
      else if ($scope.query.comparedTo === 'county') {
        return row.CountyRating || 0;
      }
    }

    function modalTopic(id) {
      ModalTopicSvc.openNursingTopic(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openNursingMeasure(id);
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

    function coalesce() {
        var result = null;
        for (var index = 0; index < arguments.length; index++) {
            result = arguments[index];
            if (result != undefined && result != null)
                return result;
        }
        return null;
    }

    function showModalMeasureHeader() {
        var modalContent = "The ratings below are based on information from the  Nursing Home CAHPS - Family Member Survey. The survey is filled out by adult family members of nursing home residents who have stayed in the nursing home for longer than 100 days. The survey asks the family members about their experiences with care and services at the nursing home. To learn more information about how the rates are calculated, review the About the Ratings.";
        ModalGenericSvc.open('Survey Summary � Overall Rating of Care', modalContent);
    }
  }

})();

