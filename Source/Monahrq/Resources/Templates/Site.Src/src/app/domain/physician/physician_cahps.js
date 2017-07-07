/**
 * Monahrq Nest
 * Core Domain Module
 * Physician CG-CAHPS Service
 *
 * The CAHPS service provides functions for loading and preparing the report
 * for the CG-CAHPS data set.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('PhysicianCAHPSSvc', PhysicianCAHPSSvc);


  PhysicianCAHPSSvc.$inject = ['$q', '$rootScope', 'ResourceSvc', 'PhysicianReportLoaderSvc'];
  function PhysicianCAHPSSvc($q, $rootScope, ResourceSvc, PhysicianReportLoaderSvc) {
    return {
      loadCAHPSReport: loadCAHPSReport,
      getCAHPSTopicIds: getCAHPSTopicIds,
      onlyCAHPSTopics: onlyCAHPSTopics,
      withoutCAHPSTopics: withoutCAHPSTopics,
      hasOverallMeasure: hasOverallMeasure,
      hasAnyCompositeMeasures: hasAnyCompositeMeasures
    };


    function getCAHPSTopicIds(topics) {
      return _.pluck(_.filter(topics, function(t) {
        return _.indexOf($rootScope.config.MEDICALPRACTICE_OVERALL_CAHPS_MEASURES, t.OverallMeasure) >= 0;
      }), 'TopicID');
    }

    function onlyCAHPSTopics(topics) {
      var tids = getCAHPSTopicIds(topics);

      return _.filter(topics, function(t) {
        return _.indexOf(tids, t.TopicID) >= 0;
      });
    }

    function withoutCAHPSTopics(topics) {
      var tids = getCAHPSTopicIds(topics);

      return _.filter(topics, function(t) {
        return _.indexOf(tids, t.TopicID) < 0;
      });
    }

    function loadCAHPSReport(measureTopics, practiceId) {
      var topics = onlyCAHPSTopics(measureTopics);
      var measureIds = _.flatten([].concat(_.pluck(topics, 'MeasureIDs'), _.pluck(topics, 'OverallMeasure')));
      var CAHPSReport = {
        isLoaded: false,
        overallTopic: null,
        subtopics: null,
        topics: topics,
        measureDefs: null,
        measures: null,
        _groupDefCache: {},

        getMeasure: getMeasure,
        getMeasureDefs: getMeasureDefs,
        getGroupedMeasureDefs: getGroupedMeasureDefs,
        getOverallMeasure: getOverallMeasure,
        getOverallMeasureDef: getOverallMeasureDef,
        getQuestionLabels: getQuestionLabels,
        getTopics: getTopics,
        isEmpty: isEmpty
      };

      return ResourceSvc.getMedicalPracticeMeasures(measureIds, true)
        .then(function (measureDefs) {
          CAHPSReport.measureDefs = _.compact(measureDefs);
          return PhysicianReportLoaderSvc.getCGCAHPSPractice(practiceId);
        }, function(error) {

        })
        .then(function(report) {
          CAHPSReport.report = report.data;

          var overallTopic = _.findWhere(topics, {OverallMeasure: $rootScope.config.MEDICALPRACTICE_OVERALL_QUALITY_ID});
          var subtopics = _.filter(topics, function (t) {
            return t.OverallMeasure != $rootScope.config.MEDICALPRACTICE_OVERALL_QUALITY_ID;
          });

          if (CAHPSReport.report) {
            CAHPSReport.isLoaded = true;
          }
          return CAHPSReport;
        });
    }

    function getMeasureDefs(CAHPSReport, topic) {
      return _.filter(CAHPSReport.measureDefs, function (md) {
        return topic && md && md.TopicID == topic.TopicID && md.MeasureID != topic.OverallMeasure;
      });
    }

    function getGroupedMeasureDefs(CAHPSReport, topic) {
      if (_.has(CAHPSReport._groupDefCache, ""+topic.TopicID)) {
        return CAHPSReport._groupDefCache[topic.TopicID];
      }

      var defs = getMeasureDefs(CAHPSReport, topic);
      CAHPSReport._groupDefCache[""+topic.TopicID] = _.groupBy(defs, function(d) { return d.CAHPSQuestionType; });
      return CAHPSReport._groupDefCache[topic.TopicID];
    }

    function getMeasure(CAHPSReport, measureID) {
      return _.findWhere(CAHPSReport.report, {MeasureID: measureID});
    }

    function getOverallMeasure(CAHPSReport, topic) {
      return _.findWhere(CAHPSReport.report, {MeasureID: topic.OverallMeasure});
    }

    function getOverallMeasureDef(CAHPSReport, topic) {
      return _.findWhere(CAHPSReport.measureDefs, {MeasureID: topic.OverallMeasure});
    }

    function getQuestionLabels(measureDef) {
      return $rootScope.config.MEDICALPRACTICE_CAHPS_QUESTION_TYPES[measureDef.CAHPSQuestionType];
    }

    function hasOverallMeasure(CAHPSReport) {
      if (CAHPSReport && CAHPSReport.measureDefs) {
        var m = _.findWhere(CAHPSReport.measureDefs, CAHPSReport.overallTopic.OverallMeasure);

        if (m) {
          return true;
        }
      }

      return false;
    }

    function hasAnyCompositeMeasures(CAHPSReport) {
      if (CAHPSReport && CAHPSReport.measureDefs) {
        var m = _.findWhere(CAHPSReport.measureDefs, CAHPSReport.overallTopic.OverallMeasure);

        if (m) {
          return true;
        }
      }

      return false;
    }

    function getTopics(CAHPSReport, topicCat) {
      return _.where(CAHPSReport.topics, {TopicCategoryID: topicCat.TopicCategoryID});
    }

    function isEmpty(CAHPSReport) {

        var topicCats = CAHPSReport.topicCategories;

        //  Find one topic category that is not empty.
        for (var index = 0; index < topicCats.length; index++) {
            var topics = CAHPSReport.getTopics(CAHPSReport, topicCats[index]);
            if (topics.length != 0) { return false; }
        }
        return true;
    }

  }

})();
