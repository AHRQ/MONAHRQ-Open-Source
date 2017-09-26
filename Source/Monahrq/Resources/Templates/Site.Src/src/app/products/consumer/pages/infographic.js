/**
 * Consumer Product
 * Pages Module
 * Infographic Page
 *
 * This controller builds the consumer surgical safety infographic page. It handles
 * user interaction with citations, and also dynamically displays page sections and data
 * based on the contents of the report file.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.pages')
    .controller('ConsumerInfographicCtrl', InfographicCtrl);


  InfographicCtrl.$inject = ['$rootScope', '$scope', '$window', '$state', 'report', 'ModalGenericSvc', 'ScrollToElSvc', 'topics'];
  function InfographicCtrl($rootScope, $scope, $window, $state, report, ModalGenericSvc, ScrollToElSvc, topics) {
    var homeUrl = $state.href('top.consumer.home', {}, {absolute: true});
    var siteLinkTag = '<a href="' + homeUrl + '">' + report.siteName + '</a>',
        citationData = [
        {
            name: "citation_1",
            citationTitle: "Citation 1",
            citationBody: "<a href=\"http://www.ahrq.gov/professionals/quality-patient-safety/pfp/index.html\">http://www.ahrq.gov/professionals/quality-patient-safety/pfp/index.html</a>"
        },
        {
            name: "citation_2",
            citationTitle: "Citation 2",
            citationBody: "<a href=\"http://www.ahrq.gov/professionals/quality-patient-safety/pfp/index.html\">http://www.ahrq.gov/professionals/quality-patient-safety/pfp/index.html</a>"
        },
        {
          name: "citation_3",
          citationTitle: "Citation 3",
          citationBody: "AHRQ Quality Indicators results as reported in " + siteLinkTag
        },
        {
          name: "citation_4",
          citationTitle: "Citation 4",
          citationBody: "These evidence-based practices are the subject of measures used in Hospital Compare, developed by the Centers for Medicare and Medicaid Services (CMS) see: <a href=\"http://www.medicare.gov/hospitalcompare/About/What-Is-HOS.html\">http://www.medicare.gov/hospitalcompare/About/What-Is-HOS.html</a>"
        },
        {
          name: "citation_5",
          citationTitle: "Citation 5",
          citationBody: "CMS Hospital Compare measure results as reported in "  + siteLinkTag
        },
        {
          name: "citation_6",
          citationTitle: "Citation 6",
          citationBody: "<a href=\"http://www.safesurg.org/uploads/1/0/9/0/1090835/surgical_safety_checklist_production.pdf\">http://www.safesurg.org/uploads/1/0/9/0/1090835/surgical_safety_checklist_production.pdf</a>"
        },
        {
          name: "citation_7",
          citationTitle: "Citation 7",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_8",
          citationTitle: "Citation 8",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_9",
          citationTitle: "Citation 9",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_10",
          citationTitle: "Citation 10",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_11",
          citationTitle: "Citation 11",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_12",
          citationTitle: "Citation 12",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_13",
          citationTitle: "Citation 13",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_14",
          citationTitle: "Citation 14",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_15",
          citationTitle: "Citation 15",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_16",
          citationTitle: "Citation 16",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        },
        {
          name: "citation_17",
          citationTitle: "Citation 17",
          citationBody: "<a href=\"http://www.nejm.org/doi/full/10.1056/NEJMsa0810119\">http://www.nejm.org/doi/full/10.1056/NEJMsa0810119</a>"
        }
    ];
    $scope.gotoFootnote = gotoFootnote;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;
    $scope.footnoteAccordionAPI = {};
    var surg = _.findWhere(topics, {Name: 'Surgical patient safety'});
    $scope.surgicalTopicId = surg ? surg.TopicCategoryID : null;

    init();

    function init() {
      $scope.shareURL = encodeURIComponent(window.location);
      $scope.report = report;
      $scope.report.citationData = citationData;
      $scope.getMeasureValue = getMeasureValue;
      $scope.modalCitations = modalCitations;

      $rootScope.$on('$locationChangeSuccess', function(event, url) {
        $scope.shareURL = encodeURIComponent(url);
      });
    }

    function share() {
        window.location = buildShareUrl();
    }

    function feedbackModal() {
        ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
        var url = escape(window.location);
        return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    $scope.wrapperClass = function() {
      return 'page--infographic';
    };

    $scope.activeSection = function(activeSection) {
      var data = report,
          val;

      val = _.contains(data.activeSections, activeSection);

      return val;
    };

    $scope.gotoHome = function() {
      if (report.siteURL == null || report.siteURL.length == 0) {
        $state.go('top.home');
      }
      else {
        $window.location.href = report.siteURL;
      }
    };

    function getLookupObj(key){
      var data = report[key],
          arr = {};

      for (var i in data) {
        arr[data[i]['name']] = i;
      }

      return arr;
    }

    function getReportValue(key, name, val, objKeyValue) {
      var data = report[key],
          measureObj = getLookupObj(key),
          measureVal;

      if (objKeyValue === undefined) {
        objKeyValue = 'values'
      }

      measureObj = measureObj[name];

      measureObj = data[measureObj];

      if (val === 'noVal') {
        measureVal = measureObj[objKeyValue];
      } else {
        measureVal = measureObj[objKeyValue][val];
      }

      return measureVal;
    }

    function getMeasureValue(name, val) {
      return getReportValue('measures', name, val);
    }

    function modalCitations(name) {
      var title = getReportValue('citationData', name, 'noVal', 'citationTitle'),
          content = getReportValue('citationData', name, 'noVal', 'citationBody');

      ModalGenericSvc.open(title, content);
    }

    function gotoFootnote($event, id) {
      $event.preventDefault();
      $scope.footnoteAccordionAPI.open('footnotesPanel');
      window.setTimeout(function() {
        ScrollToElSvc.scrollToEl(id);
      }, 0);
    }
  }

})();
