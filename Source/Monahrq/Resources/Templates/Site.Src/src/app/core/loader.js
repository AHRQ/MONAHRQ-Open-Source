/**
 * Monahrq Nest
 * Core Module
 * Loader Controller
 *
 * This controller processes the core data loaded by the app's top-level state.
 * The data is made available on the root scope. Also the CMS loads template
 * overrides.
 */
(function() {
'use strict';

angular.module('monahrq.core')
  .controller('LoaderCtrl', LoaderCtrl);


LoaderCtrl.$inject = ['$rootScope', '$state', '$window', '$location', '$templateCache', 'ReportConfigSvc',
  'ConsumerReportConfigSvc', 'MenuSvc', 'websiteConfig', 'reportConfig', 'consumerReportConfig', 'CmsEntitySvc', 'menu'];
function LoaderCtrl($rootScope, $state, $window, $location, $templateCache, ReportConfigSvc,
                    ConsumerReportConfigSvc, MenuSvc, websiteConfig, reportConfig, consumerReportConfig, CmsEntitySvc, menu) {
  $rootScope.config = websiteConfig;

  ReportConfigSvc.init(reportConfig);
  $rootScope.ReportConfigSvc = ReportConfigSvc;

  ConsumerReportConfigSvc.init(consumerReportConfig);
  $rootScope.ConsumerReportConfigSvc = ConsumerReportConfigSvc;

  MenuSvc.set(menu);

  if (websiteConfig.WEBSITE_VERSION) {
    $rootScope.VERSION = websiteConfig.WEBSITE_VERSION;
  }
  else {
    $rootScope.VERSION = '7.0';
  }

  updateTemplates();
  setupGoogleAnalytics(websiteConfig.website_GoogleAnalyticsKey);

  function setupGoogleAnalytics(gaKey) {
    (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
      (i[r].q = i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
      m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
    })(window,document,'script','http://www.google-analytics.com/analytics.js','ga');

    $window.ga('create', gaKey, 'auto');
    $window.ga('send', 'pageview', $location.path());
  }

  function updateTemplates() {
    CmsEntitySvc.init()
      .then(function() {
        _.each(CmsEntitySvc.getPageTemplates(), function(item) {
          $templateCache.put(item.value.path, item.value.template);
        });
      });
  }

}

})();
