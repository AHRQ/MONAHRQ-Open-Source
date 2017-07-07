/**
 * Monahrq Nest
 * Services Module
 * Data Loading Service
 *
 * In addition to being hosted by a web server, it is also necessary for the application to run when
 * the index.html file is loaded by the web browser from the user’s local filesystem. So while an
 * application would typically load data via XHR requests, this is not possible in the local scenario
 * due to the same-origin policy imposed by the browser security model.
 *
 * The solution this service provides is to inject <script> tags into the browser DOM, with those
 * tags referencing syntactically-valid JavaScript files containing a particular unit of data to be
 * loaded. The files contain an array or object assigned to a namespaced location within the global
 * window object. All modern browsers support this method of loading data without running afoul of
 * security restrictions.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('DataLoaderSvc', DataLoaderSvc);


  DataLoaderSvc.$inject = ['$window', '$q'];
  function DataLoaderSvc($window, $q) {
    /**
     * Private Data
     */
    var loadedScripts = [];

    /**
     * ServiceInterface
     */
    return {
      loadScript: loadScript
    };

    /**
     * Service Implementation
     */
    function loadScript(url, callback, errorcallback, forceRefresh, cacheBust) {
      var tag;
      if (forceRefresh === undefined) {
        forceRefresh = true;
      }
      if (cacheBust === undefined) {
        cacheBust = false;
      }


      JL("services.DataLoaderSvc").debug('Loading ' + url);

      if (_(loadedScripts).contains(url) && !forceRefresh) {
        JL("services.DataLoaderSvc").debug('Found in cache');
        _(function() {
          callback();
        }).defer();

        return;
      }

      if (cacheBust) {
        url = url + '?nocache=' + (new Date()).getTime();
      }

      tag = $window.document.createElement('script');
      tag.setAttribute('src', url);

      if(tag.addEventListener) {
        tag.addEventListener("load", doCallback, false);
        tag.addEventListener("error", doErrorCallback, false);
      }
      else if(tag.readyState) { //IE8
        tag.onreadystatechange = doCallback;
      }

      function doCallback() {
	      if (this.readyState && this.readyState != 'complete' && this.readyState != 'loaded') return;
        JL("services.DataLoaderSvc").info('Loaded ' + url);
        callback();
        loadedScripts.push(url);
      }

      function doErrorCallback() {
        JL("services.DataLoaderSvc").error('Failed to load ' + url);
        errorcallback(url);
      }

      $window.document.getElementsByTagName('head')[0].appendChild(tag);
    }

  }

})();

