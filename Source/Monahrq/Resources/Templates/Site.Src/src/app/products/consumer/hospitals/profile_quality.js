/**
 * Consumer Product
 * Hospital Reports Module
 * Profile Quality Panel Controller
 *
 * This controller shows the hospital's quality ratings for the available
 * topics and measures.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHProfileQualityCtrl', CHProfileQualityCtrl);


  CHProfileQualityCtrl.$inject = ['$scope', '$stateParams', '$q', 'HospitalReportLoaderSvc', 'ResourceSvc', 'ModalTopicCategorySvc', 'ModalTopicSvc', 'ModalMeasureSvc', 'UserStateSvc'];
  function CHProfileQualityCtrl($scope, $stateParams, $q, HospitalReportLoaderSvc, ResourceSvc, ModalTopicCategorySvc, ModalTopicSvc, ModalMeasureSvc, UserStateSvc) {
    var hospitalId = $stateParams.id,
      visibleTopics = {},
      measureTopicCategories = [],
      measureTopics = [];

    $scope.model = {};
    $scope.topics = [];
    $scope.query = {};

    $scope.showTopic = showTopic;
    $scope.toggleTopic = toggleTopic;
    $scope.modalTopicCategory = modalTopicCategory;
    $scope.modalTopic = modalTopic;
    $scope.modalMeasure = modalMeasure;
    $scope.getTopicCategoryName = getTopicCategoryName;
    $scope.showAll = showAll;
    $scope.isShowAll = false;
    $scope.showSymbol = showSymbol;

    init();


    function init() {
      var gcHospital = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL);
      $scope.query.compareTo = gcHospital ? gcHospital : 'national';
      $scope.$watch('query.compareTo', function(nv, ov) {
        if (nv === ov) return;
        UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL, nv);
      });

      loadData();
    }


    function loadData() {
      var promises1 = [];

      promises1.push(ResourceSvc.getMeasureTopicCategories());
      promises1.push(ResourceSvc.getMeasureTopics());

      $q.all(promises1)
      .then(function(result){

        measureTopicCategories = result[0];
        measureTopics = result[1];
        $scope.topics = buildTopicTree(measureTopicCategories, 'TopicCategoryID', measureTopics, 'TopicCategoryID', 'topics');
        $scope.topics = _.sortBy($scope.topics, function(topic){ return topic.Name});

        var promises2 = [];
        promises2.push(ResourceSvc.getMeasureDefs(getAllMeasureIds()));
        promises2.push(HospitalReportLoaderSvc.getQualityByHospitalReport(hospitalId));

        $q.all(promises2)
          .then(function(result) {
            var measureDefs = {}, reports, measureDefData;
            measureDefData = result[0];
            reports = result[1];

            _.each(measureDefData, function(def) {
              measureDefs[def.MeasureID] = def;
            });

            updateSearch(measureDefs, reports.data);
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
      var row = {
        id: measureDef.MeasureID,
        name: measureDef.MeasuresName,
        title: measureDef.SelectedTitleConsumer
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

    function showAll() {
      $scope.isShowAll = !$scope.isShowAll;
      _.each(measureTopicCategories, function(tc) {
        toggleTopic(tc.TopicCategoryID);
      });
    }

    function modalTopicCategory(id) {
      ModalTopicCategorySvc.open(id);
    }

    function modalTopic(id) {
      ModalTopicSvc.open(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.open(id);
    }

    function getTopicCategoryName(id) {
      var tc = _.findWhere(measureTopicCategories, {TopicCategoryID: id});
      return tc ? tc.Name : '';
    }

    function showSymbol(measure) {
        if (measure == undefined || measure.data == undefined) {
            return;
        }

        var field = $scope.query.compareTo == 'national' ? 'NatRating' : 'PeerRating';

        if (measure.data[field] == -1 && measure.data.RateAndCI != '-') {
            return false;
        }

        return true;
    }

  }

})();


