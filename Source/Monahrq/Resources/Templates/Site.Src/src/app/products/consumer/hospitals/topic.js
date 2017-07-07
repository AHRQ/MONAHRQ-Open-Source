/**
 * Consumer Product
 * Hospital Reports Module
 * Topic Page Controller
 *
 * This controller shows each measure's quality ratings for all hospitals, grouped by the
 * topic and subtopic the user specified.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHTopicCtrl', CHTopicCtrl);


  CHTopicCtrl.$inject = ['$scope', '$state', '$stateParams', '$location', '$timeout', 'ResourceSvc', 'HospitalRepositorySvc', 'CHReportSvc', 'SortSvc',
    'ModalLegendSvc', 'ModalTopicCategorySvc', 'ModalTopicSvc', 'ModalMeasureSvc', 'ModalGenericSvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc', 'UserStateSvc', 'zipDistances', 'topics', 'subtopics', 'nga11yAnnounce', 'ModalTopicGridSvc'];
  function CHTopicCtrl($scope, $state, $stateParams, $location, $timeout, ResourceSvc, HospitalRepositorySvc, CHReportSvc, SortSvc,
                       ModalLegendSvc, ModalTopicCategorySvc, ModalTopicSvc, ModalMeasureSvc, ModalGenericSvc, ScrollToElSvc, ConsumerReportConfigSvc, UserStateSvc, zipDistances, topics, subtopics, nga11yAnnounce, ModalTopicGridSvc) {
    var report, measureDefs = {};
    var compareIds = {};
    var scrollFocus;

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.query = {};
    $scope.hasResults = false;
    $scope.hasSearch = false;
    $scope.zipDistances = zipDistances;
    $scope.topics = [];
    $scope.subTopics = [];
    $scope.topicName = null;
    $scope.sortOptions = [];
    $scope.preview = true;
    $scope.openTopicPicker = ModalTopicGridSvc.open;
    $scope.infographicHasData = false;
    /*$scope.sortOptions = {
      hospital: [
        {label: 'Name (a-z)', value: 'name.asc'},
        {label: 'Name (z-a)', value: 'name.desc'}
      ],
      measure: [
        {label: 'Rating (top-bottom)', value: '.asc'},
        {label: 'Rating (bottom-top)', value: '.desc'}
      ]
    };*/
    $scope.infographicHelp = 'Click here to look at an “infographic” that tells you, quickly and clearly, why you '
    + 'cannot assume your surgery will be error-free.  We also let you know how our community does overall when it '
    + 'comes to keeping patients safe before, during and after surgery. SHARE THE INFOGRAPHIC WITH OTHERS!';
    $scope.informedHelp = 'Click here to read about surgical safety problems: how often they happen; how well local '
    + 'hospitals prevent them; and what to do to keep yourself safe';

    $scope.toggleCompare = toggleCompare;
    $scope.canCompare = canCompare;
    $scope.getCompareTabIndex = getCompareTabIndex;
    $scope.updateSearch = updateSearch;
    $scope.isSubtopicActive = isSubtopicActive;
    $scope.getActiveSubtopicIndex = getActiveSubtopicIndex;
    $scope.gotoMap = gotoMap;
    $scope.gotoCompare = gotoCompare;
    $scope.modalLegend = modalLegend;
    $scope.modalTopic  = modalTopic;
    $scope.modalSubtopic = modalSubtopic;
    $scope.modalMeasure = modalMeasure;
    $scope.gotoCostQuality = gotoCostQuality;
    $scope.showSupportsCost = showSupportsCost;
    $scope.jumpToReport = jumpToReport;
    $scope.showSymbol = showSymbol;
    $scope.onInfographicLoad = onInfographicLoad;
    $scope.showCompareHelpModal = showCompareHelpModal;

    init();

    function init() {
      $scope.model = [];

      var gcHospital = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL);
      $scope.query.compareTo = gcHospital ? gcHospital : 'state';

      $scope.query.sort = 'name.asc';
      $scope.query.topicId = $stateParams.topicId ? +$stateParams.topicId : null;
      $scope.query.subtopicId = $stateParams.subtopicId ? +$stateParams.subtopicId : null;
      $scope.query.zip = $stateParams.zip;
      $scope.query.distance = +$stateParams.distance;

      if ($stateParams.focus === 'infographic' || $stateParams.focus === 'report') {
        scrollFocus = $stateParams.focus
      }
      else {
        scrollFocus = 'report';
      }


      buildTopics();
      if ($scope.query.topicId) {
        var t = _.findWhere(topics, {TopicCategoryID: $scope.query.topicId});
        if (t) {
          $scope.topicName = t.Name;
        }
        buildSubTopics($scope.query.topicId);
        if (_.isArray($scope.subtopics) && $scope.query.subtopicId === null) {
          $scope.query.subtopicId = $scope.subtopics[0].id;
        }
      }

      $scope.$watch('query.compareTo', function(nv, ov) {
        if (nv === ov) return;
        UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL, nv);
      });

      $scope.$watch('query.sort', updateSort);

      HospitalRepositorySvc.init()
        .then(loadReport);

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

    function buildTopics() {
      $scope.topics = _.map(topics, function(topic) {
        return {
          id: topic.TopicCategoryID,
          name: topic.Name
        };
      });
    }

    function buildSubTopics(topicId) {
      $scope.subtopics = _.map(_.filter(subtopics, function(subtopic) {
        return subtopic.TopicCategoryID == topicId;
      }),
      function(subtopic) {
        return {
          id: subtopic.TopicID,
          name: subtopic.Name,
          measures: subtopic.Measures,
          description: subtopic.Description || subtopic.LongTitle
        }
      });
    }

    function jumpToReport() {
      ScrollToElSvc.scrollToEl('.topic .report');
    }

    function loadReport() {
      var query, measureIds;

      $scope.hasSearch = true;
      $scope.hasResults = true;

      query = $scope.query;
      if (!query.topicId || !query.subtopicId) {
        if (query.topicId) {
          ScrollToElSvc.scrollToEl('.topic .' + scrollFocus);
        }
        return;
      }

      var subtopic = _.findWhere($scope.subtopics, {id: +query.subtopicId});
      measureIds = subtopic.measures;

      ResourceSvc.getMeasureDefs(measureIds)
        .then(function(_measureDefs) {
           _.each(_measureDefs, function(def) {
            measureDefs[def.MeasureID] = def;
          });

          $scope.measureDefs = measureDefs;
          $scope.measureKeys = _.keys(measureDefs).sort();

          return CHReportSvc.getHospitalMeasuresReport(measureIds, $scope.query.zip, $scope.query.distance);
        })
        .then(function(_report) {
          ScrollToElSvc.scrollToEl('.topic .' + scrollFocus);
          report = _report;
          buildSortOptions();
          updateTable();
        })
    }

    function handleNoResults() {
      $scope.model = [];
      $scope.hasResults = false;

      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function buildSortOptions() {
      $scope.sortOptions = [];
      $scope.sortOptions.push({label: 'Name, a to z', value: 'name.asc'});
      $scope.sortOptions.push({label: 'Name, z to a', value: 'name.desc'});

      _.each($scope.measureKeys, function(key) {
        var col = $scope.measureDefs[key];
        $scope.sortOptions.push({
          label: col.SelectedTitleConsumer + ', top to bottom',
          value: col.MeasureID + '.asc'
        });
        $scope.sortOptions.push({
          label: col.SelectedTitleConsumer + ', bottom to top',
          value: col.MeasureID + '.desc'
        });
      });
    }

    function updateTable() {
      $scope.hasResults = true;
      $scope.model = report;
      updateSort(0, 1);
    }

    function updateSort(ov, nv) {
      if (ov == nv) return;
      if (!$scope.query.sort) return;

      var sortParams = $scope.query.sort.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      var level = ($scope.query.compareTo === 'state' ? 'PeerRating' : 'NatRating');

      if (sortField === 'name') {
        SortSvc.objSort($scope.model, sortField, sortDir);
      }
      // per-measure ratings
      else {
        SortSvc.objSort2Numeric($scope.model, sortField, level, sortDir);
      }
    }

    function updateSearch(updatedField) {
      if (updatedField == 'topic') {
        $scope.query.subtopicId = null;

        $state.go('^.topic', {
          topicId: $scope.query.topicId,
          subtopicId: $scope.query.subtopicId,
          zip: $scope.query.zip,
          distance: $scope.query.distance
        });
      }
      else {
        if (updatedField == 'subtopic') {
          $state.current.reloadOnSearch = false;
          $location.search({
            topicId: $scope.query.topicId,
            subtopicId: $scope.query.subtopicId,
            zip: $scope.query.zip,
            distance: $scope.query.distance
          });
          $stateParams.subtopicId = $scope.query.subtopicId;
          $timeout(function () {
            $state.current.reloadOnSearch = undefined;
          });
        }
        report = null;
        measureDefs = {};
        $scope.model = [];
        $scope.measureDefs = {};
        loadReport();
      }
    }

    function isSubtopicActive(subtopic) {
      return $scope.query.subtopicId === subtopic.id;
    }

    function getActiveSubtopicIndex() {
      return _.findIndex($scope.subtopics, function(x) {
        return x.id === $scope.query.subtopicId;
      });
    }

    function toggleCompare(id) {
      var prevSize = _.size(compareIds);

      if (_.has(compareIds, id)) {
        delete compareIds[id];
      }
      else {
        compareIds[id] = true;
      }

      updateCompareStatusMsg(prevSize);
    }

    function updateCompareStatusMsg(prevSize) {
      if (canCompare() && _.size(compareIds) === 2 && prevSize === 1) {
        nga11yAnnounce.assertiveAnnounce("Compare button enabled");
      }
      else if (!canCompare() && _.size(compareIds) === 1 && prevSize === 2) {
        nga11yAnnounce.assertiveAnnounce("Compare button disabled");
      }
    }

    function canCompare() {
      var size = _.size(compareIds);
      return (size >= 2 && size <= 5);
    }

    function getCompareTabIndex() {
      return canCompare() ? '0' : '-1';
    }

    function gotoMap() {
      var params = {
        topicId: $scope.query.topicId,
        subtopicId: $scope.query.subtopicId
      };

      if ($scope.query.zip && $scope.query.distance) {
        params.zip =  $scope.query.zip;
        params.distance = $scope.query.distance;
      }

      $state.go('^.topic-map', params);
    }

    function gotoCompare() {
      if (!canCompare()) return;

      $state.go('^.compare', {
        ids: _.keys(compareIds).join(','),
        topicId: $scope.query.topicId
      });
    }

    function showSupportsCost() {
      /*if ($scope.measureDefs && _.size($scope.measureDefs) === 1) {
        var k = _.first(_.keys($scope.measureDefs));*/
        if (_.findWhere($scope.measureDefs, {SupportsCost: true}) && ConsumerReportConfigSvc.hasCostQuality()) {
          return true;
        }
      //}

      return false;
    }

    function showSymbol(measure) {
      var field = $scope.query.compareTo == 'national' ? 'NatRating' : 'PeerRating';

      if (measure[field] == -1 && measure.RateAndCI != '-') {
        return false;
      }

      return true;
    }

    function gotoCostQuality() {
      if (canCompare()) {
        $state.go("^.cost-quality", {
          subtopicId: $scope.query.subtopicId,
          hospitalIds: _.keys(compareIds).join(',')
        });
      }
      else {
        ModalGenericSvc.open('Help', 'Please select at least two hospitals to view this report.  No more than five hospitals may be selected at a time.');
      }
    }

    function modalLegend(){
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

    function onInfographicLoad(hasData) {
      $scope.infographicHasData = hasData;
    }

    function showCompareHelpModal() {
      if (!canCompare())
        ModalGenericSvc.open('Help', 'Please select at least two hospitals to view this report.  No more than five hospitals may be selected at a time.');
    }

  }

})();
