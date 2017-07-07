/**
 * Monahrq Nest
 * Components Module
 * CMS Zone Directive
 *
 * CMS Zone outputs the content of the specified zone.
 *
 * <div data-mh-cms-zone="'zone-name'" data-mh-cms-zone-report-id="reportId" data-mh-cms-zone-product="'Professional'"></div>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module("monahrq.components.cms",[])
    .directive("mhCmsZone", cmsZone);


  cmsZone.$inject = ['CmsEntitySvc'];
  function cmsZone(CmsEntitySvc) {
    /**
     * Directive Definition
     */
    return {
      restrict: "EA",
      replace: true,
      scope: {
        zone: '=mhCmsZone',
        reportId: '=mhCmsZoneReportId',
        product: '=mhCmsZoneProduct'
      },
      template: '<div class="cms-zone"><div class="contain" data-ng-bind-html="zoneContent"></div></div>',
      link: link
    };

    function link(scope, element) {
      element.addClass('cms-zone--' + scope.zone);

      CmsEntitySvc.init()
        .then(updateZones);
      scope.$watch('reportId', updateZones);

      function updateZones() {
        var zones = CmsEntitySvc.getReportZones(scope.product, scope.reportId);
        scope.zoneContent = null;
        _.each(zones, function(zone) {
          if (zone.value.zone === scope.zone) {
            scope.zoneContent = zone.value.template;
          }
        });
      }

    }
  }

})();
