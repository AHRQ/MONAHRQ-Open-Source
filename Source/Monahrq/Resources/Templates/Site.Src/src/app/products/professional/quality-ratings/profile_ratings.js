/**
 * Professional Product
 * Quality Ratings Module
 * Profile Page Quality Tab Ratings Table Controller
 *
 * This controller generates a report of a given hospital's quality ratings, grouped by
 * topic, subtopic, and measure.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRProfileRatingsCtrl', QRProfileRatingsCtrl);

  QRProfileRatingsCtrl.$inject = ['$scope', '$q', 'QRReportSvc', 'ResourceSvc', 'ReportConfigSvc', 'ModalTopicCategorySvc', 'ModalTopicSvc', 'SortSvc'];
  function QRProfileRatingsCtrl($scope, $q, QRReportSvc, ResourceSvc, ReportConfigSvc, ModalTopicCategorySvc, ModalTopicSvc, SortSvc) {

    var hospitalId = $scope.hospitalProfile.id,
        visibleTopics = {},
        measureTopicCategories = [],
        measureTopics = [];

    $scope.model = {};
    $scope.topicCats = [];
    $scope.query = {
      comparedTo: 'peer'
    };

    $scope.$watch('query.comparedTo', function() {
      if ($scope.query.comparedTo === 'nat') {
        $scope.ratingField = 'NatRating';
      }
      else {
        $scope.ratingField = 'PeerRating';
      }
    });

    loadData();

    function loadData() {
      var promises1 = [];

      if(ReportConfigSvc.webElementAvailable('Quality_ConditionTopicExplore_Button')){
        promises1.push(ResourceSvc.getMeasureTopicCategories());
        promises1.push(ResourceSvc.getMeasureTopics());
      }
      $q.all(promises1)
      .then(function(result){

        measureTopicCategories = result[0];
        measureTopics = result[1];
        $scope.topicCats = buildTopicTree(measureTopicCategories, 'TopicCategoryID', measureTopics, 'TopicCategoryID', 'topics');
        $scope.topicCats = _.sortBy($scope.topicCats, function(topic){ return topic.Name});

        var promises2 = [];
        promises2.push(ResourceSvc.getMeasureDefs(getAllMeasureIds()));
        promises2.push(QRReportSvc.getReportByHospital(hospitalId));

        $q.all(promises2)
          .then(function(result) {
            var measureDefs = {}, reports, measureDefData;
            measureDefData = result[0];
            reports = result[1];


            _.each(measureDefData, function(def) {
              measureDefs[def.MeasureID] = def;
            });

            updateSearch(measureDefs, reports);
        });
      });
    }

    function updateSearch(measureDefs, report) {
      _.each(measureTopics, function(topic) {
        var curMeasureDefs = _.pick(measureDefs, topic.Measures);

        $scope.model[topic.TopicID] = [];
        _.each(curMeasureDefs, function(measureDef) {
          $scope.model[topic.TopicID].push(buildRow(measureDef, report));
        });
      });
    }

    function buildRow(measureDef, data) {
      var query, row, h, homeasures, m, measures;

      row = {
        id: measureDef.MeasureID,
        name: measureDef.MeasuresName,
        title: measureDef.SelectedTitle
      };

      row.data = _.findWhere(data, {MeasureID:measureDef.MeasureID});

      return row;
    }

    function getAllMeasureIds() {
      var ids = [];
      _.each(measureTopics, function(m) {
        if (m.Measures) {
          ids = _.union(ids, m.Measures);
        }
      });

      return ids;
    }

    function buildTopicTree(parent, parentKey, child, childKey, collectionName) {
      var tree = _.clone(parent);
      _.each(parent, function(p) {
        p[collectionName] = _.filter(child, function(c) {
          return p[parentKey] == c[childKey];
        });
      });

      return tree;
    }


    $scope.showTopic = function(id) {
      return _.has(visibleTopics, id);
    };

    $scope.toggleTopic = function (id) {
      if ($scope.showTopic(id)) {
        delete visibleTopics[id];
      }
      else {
        visibleTopics[id] = true;
      }
    };

    $scope.modalTopicCategory = function(id) {
      ModalTopicCategorySvc.open(id);
    };

    $scope.modalTopic = function(id){
      ModalTopicSvc.open(id)
    };

    $scope.getTopicCategoryName = function(id) {
      var tc = _.findWhere(measureTopicCategories, {TopicCategoryID: id});
      return tc ? tc.Name : '';
    };

    $scope.showSymbol = function(measure) {
      return measure && measure.PeerRating != -1;
    };

  }

})();

