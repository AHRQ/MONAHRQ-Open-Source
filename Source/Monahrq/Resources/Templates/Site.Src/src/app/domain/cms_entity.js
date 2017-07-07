/**
 * Monahrq Nest
 * Core Domain Module
 * CMS Entity Service
 *
 * Load and query data from the CmsEntity base data file. The CMS allows for
 * both entire page templates to be replaced, as well as for header/footer zones
 * on pages to be populated with content. The service is initialized during Angular
 * bootstrap.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('CmsEntitySvc', CmsEntitySvc);


  CmsEntitySvc.$inject = ['$q', 'ResourceSvc'];
  function CmsEntitySvc($q, ResourceSvc) {
    var entities = [],
      loadD = $q.defer();

    return {
      PRODUCT_CONSUMER: 'Consumer',
      PRODUCT_PROFESSIONAL: 'Professional',

      init: init,
      getPageTemplates: getPageTemplates,
      getReportZones: getReportZones
    };

    function init() {
      if (loadD.promise.$$state.status === 0) {
        ResourceSvc.getCmsEntities()
          .then(function(_entities) {
            entities = _entities;
            loadD.resolve();
          }, function() {
            loadD.reject();
          });
      }

      return loadD.promise;
    }

    function getPageTemplates() {
      return _.where(entities, {type: 'page_template'});
    }

    function getReportZones(product, reportId) {
      return _.filter(entities, function(entity) {
          return entity.type === 'page_zone'
            && entity.value.product === product
            &&  entity.value.report_id === reportId;
        });
    }

  }

})();
