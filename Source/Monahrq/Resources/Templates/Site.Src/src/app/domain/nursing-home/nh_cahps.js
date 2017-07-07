/**
 * Monahrq Nest
 * Core Domain Module
 * Nursing Home CAHPS Service
 *
 * The CAHPS service provides functions for loading and preparing the report
 * for the NH-CAHPS data set.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('NHCAHPSSvc', NHCAHPSSvc);


  NHCAHPSSvc.$inject = ['$q', '$rootScope', 'ResourceSvc', 'NHReportLoaderSvc'];
  function NHCAHPSSvc($q, $rootScope, ResourceSvc, NHReportLoaderSvc) {
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
        return _.indexOf($rootScope.config.NURSING_OVERALL_CAHPS_MEASURES, t.OverallMeasure) >= 0;
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

    function loadCAHPSReport(measureTopics) {
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

        getTopics: getTopics,
        getMeasureDefs: getMeasureDefs,
        getGroupedMeasureDefs: getGroupedMeasureDefs,
        getMeasure: getMeasure,
        getOverallMeasure: getOverallMeasure,
        getOverallMeasureDef: getOverallMeasureDef,
        getQuestionLabels: getQuestionLabels,
        isEmpty: isEmpty
      };

      function removeEmptyTopics() {
        CAHPSReport.topics = _.filter(CAHPSReport.topics, function(t) {
          return _.findWhere(CAHPSReport.measures, {'MeasureID': t.OverallMeasure}) != null;
        });
      }

      return ResourceSvc.getNursingHomeMeasures(measureIds, true)
        .then(function (measureDefs) {
          CAHPSReport.measureDefs = _.compact(measureDefs);
          return NHReportLoaderSvc.getMeasureReports(_.compact(_.pluck(measureDefs, 'MeasureID')));
        }, function(error) {

        })
        .then(function (measures) {
          CAHPSReport.measures = _.flatten(_.pluck(measures, 'data'));

          removeEmptyTopics();

          var overallTopic = _.findWhere(topics, {OverallMeasure: $rootScope.config.NURSING_OVERALL_FMLYRATE_ID});
          var subtopics = _.filter(topics, function (t) {
            return t.OverallMeasure != $rootScope.config.NURSING_OVERALL_FMLYRATE_ID;
          });


          CAHPSReport.isLoaded = true;
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

    function getMeasure(CAHPSReport, id, nhid) {
      var m = _.findWhere(CAHPSReport.measures, {MeasureID: id, NursingHomeID: +nhid});
      if (!m) {
        m = {CAHPSResponseValues: []};
      }
      return m;
    }

    function getOverallMeasure(CAHPSReport, topic, nhid) {
      return getMeasure(CAHPSReport, topic.OverallMeasure, nhid);
    }

    function getOverallMeasureDef(CAHPSReport, topic) {
      return _.findWhere(CAHPSReport.measureDefs, {MeasureID: topic.OverallMeasure});
    }

    function getQuestionLabels(measureDef) {
      return $rootScope.config.NURSING_CAHPS_QUESTION_TYPES[measureDef.CAHPSQuestionType];
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

        var topics = CAHPSReport.topics;

        //  Find one topic category that is not empty.
        for (var index = 0; index < topics.length; index++) {
            var measures = CAHPSReport.getMeasureDefs(CAHPSReport, topics[index]);
            if (measures.length != 0) { return false; }
        }
        return true;
    }

  }

})();
