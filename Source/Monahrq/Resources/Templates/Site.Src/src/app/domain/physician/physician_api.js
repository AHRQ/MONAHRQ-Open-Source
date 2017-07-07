/**
 * Monahrq Nest
 * Core Domain Module
 * Physician API Service
 *
 * This provides the real-time api equivalent to the physician report loader. Rather than
 * load report data from local data files, it is able to interface with the Medicare API
 * to perform the same searches. The host user is able to toggle between which loader to
 * use via the website_config file.
 *
 * Endpoint: http://data.medicare.gov/resource/mj5m-pzi6.json
 * Old Endpoint: http://data.medicare.gov/resource/s63f-csi6.json
 * API Docs: http://dev.socrata.com/docs/endpoints.html
 * API Browser: https://data.medicare.gov/Physician-Compare/National-Downloadable-File/mj5m-pzi6
 * Old API Browser: https://data.medicare.gov/Physician-Compare/National-Downloadable-File/s63f-csi6
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('PhysicianAPISvc', PhysicianAPISvc);


  PhysicianAPISvc.$inject = ['$q', '$http', 'ResourceSvc'];
  function PhysicianAPISvc($q, $http, ResourceSvc) {
    /**
     * Private Data
     */

    var config = {
        //endpoint: 'http://data.medicare.gov/resource/s63f-csi6.json',
        endpoint: 'http://data.medicare.gov/resource/mj5m-pzi6.json',
      pageSize: 50000,
      sortField: 'npi'
    };

    var _states = [], _token;

    /**
     * Service Interface
     */
    return {
      init: init,
      getById: getById,
      findByName: findByName,
      findByPracticeName: findByPracticeName,
      findBySpecialty: findBySpecialty,
      findByCondition: findByCondition,
      findByCity: findByCity,
      findByZip: findByZip,
      findByHospAfl: findByHospAfl
    };


    /**
     * Service Implementation
     */
    function init(websiteConfig) {
      _states = websiteConfig.website_States;
      _token = websiteConfig.PHYSICIAN_API_TOKEN;
    }

    function getById(id) {
      var query = makeQuery([{param: 'npi', value: id}]);
      return runQuery(query);
    }

    function findByName(first_name, last_name) {
      var params = [];

      if (first_name.length > 0) {
        params.push({param: 'frst_nm', value: first_name});
      }

      if (last_name.length > 0) {
        params.push({param: 'lst_nm', value: last_name, combine: 'AND'});
      }

      var query = makeQuery(params);
      return runQuery(query);
    }

    function findByPracticeName(practiceName) {
      var query = makeQuery([
        {param: 'org_lgl_nm', value: practiceName} //,
        //{param: 'org_dba_nm', value: practiceName, combine: 'OR'}
      ]);
      return runQuery(query);
    }

    function findBySpecialty(specialty) {
      var query = makeQuery([{param: 'pri_spec', value: specialty}]);
      return runQuery(query);
    }

    function findByCondition(condition) {
    }

    function findByCity(city) {
      var query = makeQuery([
        {param: 'cty', value: city}
      ]);
      return runQuery(query);
    }

    function findByZip(zip) {
      var query = makeQuery([{param: 'zip', value: zip}]);
      return runQuery(query);
    }

    function findByHospAfl(providerId) {
      var params = [];
      for (var x = 1; x <= 5; x++) {
        params.push({param: 'hosp_afl_' + x, value: providerId, combine: 'OR'})
        x++;
      }

      var query = makeQuery(params);
      return runQuery(query);
    }

    function makeQuery(params) {
      var query = config.endpoint + '?';

      // Build WHERE clause
      //////
      query += '$where=';

      // Limit search to states this monahrq site reports on
      var st = _.reduce(_states, function(acc, s) {
        if (acc.length > 0) {
          acc = acc + 'OR ';
        }
        return acc + "st='" + s + "' ";
      }, "");

      query += '(' + st + ')';

      // add any addition parameters the user specified
      var p = _.reduce(params, function(acc, s) {
        var combine = '';

        if (acc.length > 0) {
          if (s.combine) {
            combine = s.combine;
          }
          else {
            combine = 'AND';
          }
        }

        return acc + ' ' + combine + ' ' + s.param + "='" + s.value + "'";
      }, "");

      query += ' AND (' + p + ')';

      // Build ORDER and LIMIT clauses
      //////
      query += "&$order=" + config.sortField + "&$limit=" + config.pageSize;

      return query;
    }

    function runQuery(query) {
      var d = $q.defer();

      var config = {
        headers:  {
          'X-App-Token': _token
        }
      };

      $http.get(query, config)
        .success(function(data, status, headers, config) {
          postProcess(data);
          d.resolve(data);
        })
        .error(function(data, status, headers, config) {
          d.reject();
        });

      return d.promise;
    }

    function postProcess(data) {
      _.each(data, function(row) {
        row.frst_nm = toTitleCase(row.frst_nm);
        row.lst_nm = toTitleCase(row.lst_nm);
      });
    }

    function toTitleCase(str)
    {
      return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
    }

  }

})();
