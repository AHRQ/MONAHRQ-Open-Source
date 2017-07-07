/**
 * Professional Product
 * Quality Ratings Module
 * Compare Table Controller
 *
 * The compare page allows the user to compare how a set of hospitals performed
 * for the available topics.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRTableCompareCtrl', QRTableCompareCtrl);

  QRTableCompareCtrl.$inject = ['$scope', '$state', '$q', '_',
    'QRQuerySvc', 'QRReportSvc', 'ResourceSvc', 'SortSvc', 'ModalQRSvc', 'ModalMeasureSvc',
    'ReportConfigSvc', 'ModalLegendSvc', 'hospitals', 'measureTopicCategories', 'measureTopics', '$location', '$anchorScroll'];
  function QRTableCompareCtrl($scope, $state, $q, _,
    QRQuerySvc, QRReportSvc, ResourceSvc, SortSvc, ModalQRSvc, ModalMeasureSvc,
    ReportConfigSvc, ModalLegendSvc, hospitals, measureTopicCategories, measureTopics, $location, $anchorScroll) {

    $scope.showHeaderMore = false;
    $scope.reportSettings = {};

    console.log(QRQuerySvc.query);

    $scope.query = QRQuerySvc.query;

    if (!$scope.query.comparedTo) {
      $scope.query.comparedTo = 'nat';
    }

    if (!$scope.query.displayType || !_.contains(['symbols', 'symbols_rar', 'bar_chart'], $scope.query.displayType)){
      $scope.query.displayType = getDefaultDisplayType();
    }
    QRQuerySvc.notifyReportChange($state.current.data.report[$scope.query.displayType]);

    $scope.model = {};
    $scope.columns = [];

    var visibleTopics = {};

    $scope.topicCats = buildTopicTree(measureTopicCategories, 'TopicCategoryID', measureTopics, 'TopicCategoryID', 'topics');
    $scope.topicCats = _.sortBy($scope.topicCats, function (topic) {
      return topic.Name
    });

    if ($scope.query.topicSel) {
      visibleTopics[$scope.query.topicSel.TopicCategoryID] = true;
      $location.hash('compare-' + $scope.query.topicSel.TopicCategoryID);
      $anchorScroll();
    } else{
      visibleTopics[$scope.topicCats[0].TopicCategoryID] = true;
    }

    $scope.$watch('query.displayType', function(n, o) {
      if (n == o) return;
      QRQuerySvc.notifyReportChange($state.current.data.report[$scope.query.displayType]);
      setupReportHeaderFooter();
    });

    setupReportHeaderFooter();
    if (QRQuerySvc.query.hospitals.length > 0) {
      loadData();
    }

    $scope.selectText = function (element) {
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
    };


    function setupReportHeaderFooter() {
      var dt = $scope.query.displayType;
      if (dt) {
        var id = $state.current.data.report[dt];
        var report = ReportConfigSvc.configForReport(id);
        if (report) {
          $scope.reportSettings.header = report.ReportHeader;
          $scope.reportSettings.footer = report.ReportFooter;
        }
      }
    }

    function getAllMeasureIds() {
      var ids = [];
      _.each(measureTopics, function (m) {
        if (m.Measures) {
          ids = _.union(ids, m.Measures);
        }
      });

      return ids;
    }

    function loadData() {
      var promises = [], hids;

      hids = QRQuerySvc.query.hospitals;
      if (hids.length == 0) return;

      promises.push(ResourceSvc.getMeasureDefs(getAllMeasureIds()));
      promises.push(QRReportSvc.getReportsByHospitals(hids));

      $q.all(promises)
        .then(function (result) {
          var measureDefs = {}, reports = {}, measureDefData, reportData;
          measureDefData = result[0];
          reportData = result[1];

          _.each(measureDefData, function (def) {
            measureDefs[def.MeasureID] = def;
          });

          _.each(reportData, function (report) {
            reports[report[0].HospitalID] = report;
          });

          updateSearch(measureDefs, reports);
        });
    }

    function updateSearch(measureDefs, data) {
      var topicKeys, hids, measureIds;

      hids = _.map(QRQuerySvc.query.hospitals, function (id) {
        return +id;
      });

      $scope.columns = getColumns(hids);
      SortSvc.objSort($scope.columns, 'name', 'asc');

      _.each(measureTopics, function (topic) {
        var curMeasureDefs = _.pick(measureDefs, topic.Measures);

        $scope.model[topic.TopicID] = [];
        _.each(curMeasureDefs, function (measureDef) {
          $scope.model[topic.TopicID].push(buildRow(measureDef, hids, data));
        });
      });
    }

    function getColumns(hids) {
      var hs = _.filter(hospitals, function (h) {
        return _.contains(hids, h.Id);
      });

      var columns = _.map(hs, function (h) {
        var row = {
          id: h.Id,
          name: h.Name
        };
        return row;
      });

      return columns;
    }

    function buildRow(measureDef, hospitals, data) {
      var query, row, h, homeasures, m, measures;


      row = {
        id: measureDef.MeasureID,
        name: measureDef.MeasuresName,
        title: measureDef.SelectedTitle,
        HigherScoresAreBetter: measureDef.HigherScoresAreBetter,
        NatRateAndCI: measureDef.NatRateAndCI,
        PeerRateAndCI: measureDef.PeerRateAndCI
      };

      _.each(hospitals, function (hid) {
        var hdata = [];
        _.each(data, function (d) {
          if (d.length > 0 && d[0].HospitalID == hid) {
            hdata = d;
          }
        });
        row[hid] = _.findWhere(hdata, {MeasureID: measureDef.MeasureID})
      });

      return row;
    }

    function getDefaultDisplayType() {
      if (ReportConfigSvc.webElementAvailable('Quality_Compare_Display_Symbols_Dropdown')) {
        return 'symbols';
      }
      else if (ReportConfigSvc.webElementAvailable('Quality_Compare_Display_SymbolsAndRAR_Dropdown')) {
        return 'symbols_rar';
      }
      else if (ReportConfigSvc.webElementAvailable('Quality_Compare_Display_BarChart_Dropdown')) {
        return 'bar_charts';
      }
    }

    $scope.showSymbol = function (measure) {
      var show = false,
        dt = $scope.query.displayType;

      if (dt === 'symbols' || dt === 'symbols_rar') {
        show = true;
      }

      if (measure && (($scope.showPeer() && measure.PeerRating == '-1')
        || (!$scope.showPeer() && measure.NatRating == '-1'))) {
        show = false;
      }

      return show;
    };

    $scope.showChart = function () {
      var dt = $scope.query.displayType;
      return dt === 'bar_charts';
    };

    $scope.showRAR = function (measure) {
      var show = false,
        dt = $scope.query.displayType;

      if (dt === 'symbols_rar') {
        show = true;
      }

      if (measure
        && (dt === 'symbols' || dt === 'symbols_rar')
        && (($scope.showPeer() && measure.PeerRating == '-1')
        || (!$scope.showPeer() && measure.NatRating == '-1'))) {
        show = true;
      }

      return show;
    };

    $scope.showPeer = function () {
      return $scope.query.comparedTo === 'peer';
    };

    $scope.showAverage = function (avg) {
      return $scope.showChart() && avg != -1;
    };

    $scope.showTopic = function (id) {
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

    $scope.toggleHeaderMore = function () {
      $scope.showHeaderMore = !$scope.showHeaderMore;
    };

    $scope.modalLegend = function (displayType) {
      var id = $state.current.data.report[displayType];
      ModalLegendSvc.open(id, displayType);
    };

    $scope.modalMeasure = function (id) {
      ModalMeasureSvc.open(id);
    };

    $scope.modalQR = function (id) {
      ModalQRSvc.open(id);
    };

    $scope.updateCompare = function () {
      if ($scope.query.comparedTo == 'nat') {
        $scope.compareLabelField = 'NatLabel';
        $scope.compareRatingField = 'NatRating';
        $scope.compareFilledField = 'NatFilled';
        $scope.compareAvgField = 'NatRateAndCI';
      }
      else if ($scope.query.comparedTo == 'peer') {
        $scope.compareLabelField = 'PeerLabel';
        $scope.compareRatingField = 'PeerRating';
        $scope.compareFilledField = 'PeerFilled';
        $scope.compareAvgField = 'PeerRateAndCI';
      }
    };

    $scope.canBackToReport = function () {
      if ($state.previous.name && !$state.previous.name['abstract'])
        return true;
      return false;
    };

    $scope.backToReport = function () {
      if ($state.previous.name) {
        var stateName = $state.previous.name;
        var stateParams = $state.previous.params;
      }

      if (stateName)
        $state.go(stateName, stateParams);
    };

    $scope.updateCompare();

    function buildTopicTree(parent, parentKey, child, childKey, collectionName) {
      var tree = _.clone(parent);
      _.each(parent, function (p) {
        p[collectionName] = _.filter(child, function (c) {
          return p[parentKey] == c[childKey];
        });
      });

      return tree;
    }
  }

})();

