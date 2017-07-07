/**
 * Monahrq Nest
 * Core Domain Module
 * Physician Data Dedupe Service
 *
 * The physician data sets contain many duplicate records, because each row contains both
 * physician and practice information. Because a physician can belong to multiple practices,
 * or the same practice can have multiple addresses, this information is duplicated.
 *
 * The purpose of the dedupe service is to merge together all of the duplicate data into a
 * more normalized format. There are two algorithms it supports:
 *
 * 1 - Take the first practice for each physician and discard any others
 * 2 - Merge duplicate practices by pac_id, and build an array of each street address it has
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('PhysicianDedupeSvc', PhysicianDedupeSvc);


  PhysicianDedupeSvc.$inject = [];
  function PhysicianDedupeSvc() {
    /**
     * Private Data
     */
    var DEDUPE_FIRST = 1, DEDUPE_MERGE = 2;


    /**
     * Service Interface
     */
    return {
      DEDUPE_FIRST: DEDUPE_FIRST,
      DEDUPE_MERGE: DEDUPE_MERGE,
      dedupe: dedupe
    };


    /**
     * Service Implementation
     */
    function dedupe(rows, method) {
      if (method == undefined) method = DEDUPE_FIRST;

      if (method == DEDUPE_FIRST) {
        return dedupe_first(rows);
      }
      else if (method == DEDUPE_MERGE) {
        return dedupe_merge(rows);
      }
    }

    function dedupe_first(rows) {
      var byNPI = _.groupBy(rows, 'npi');
      var result = [];
      _.each(byNPI, function(group) {
        result.push(group[0]);
      });
      return result;
    }

    function dedupe_merge(rows) {
      var byNPI = _.groupBy(rows, 'npi');
      var result = [];

      _.each(byNPI, function(group) {
        result.push(dedupe_merge_physician(group));
      });

      return result;
    }

    function dedupe_merge_physician(group) {
      var merged = group[0];
      var physAddresses = [];
      var practices = {};

      for (var i = 0; i < group.length; i++) {
        var row = group[i];

        if (_.has(row, 'org_pac_id') && _.isString(row['org_pac_id']) && row.org_pac_id.length > 0) {
          populate_practice(practices, row);
        }
        else {
          add_phys_address(physAddresses, row);
        }
      }

      merged.addresses = physAddresses;
      merged.practices = practices;

      return merged;
    }

    function populate_practice(practices, row) {
      if (!_.has(practices, row.org_pac_id)) {
        practices[row.org_pac_id] = {
          addresses: []
        };
      }

      _.defaults(practices[row.org_pac_id], {
        org_pac_id: row.org_pac_id,
        org_lgl_nm: row.org_lgl_nm,
        org_dba_nm: row.org_dba_nm,
        num_org_mem: row.num_org_mem,
        st: row.st
      });

      add_practice_address(practices[row.org_pac_id], row);
    }

    function add_practice_address(practice, row) {
      var hasAddr = _.any(practice.addresses, function(addr) {
        return addr.adr_ln_1 == row.adr_ln_1 && addr.adr_li_2 == row.adr_line_2
          && addr.cty == row.cty && addr.st == row.st && addr.zip.substring(0, 5) == row.zip.substring(0, 5);
      });

      if (!hasAddr) {
        practice.addresses.push({
          adr_ln_1: row.adr_ln_1,
          adr_ln_2: row.adr_ln_2,
          ln_2_sprs: row.ln_2_sprs,
          cty: row.cty,
          st: row.st,
          zip: row.zip
        });
      }
    }

    function add_phys_address(addresses, row) {
      var hasAddr = _.any(addresses, function(addr) {
        return addr.adr_ln_1 == row.adr_ln_1 && addr.adr_li_2 == row.adr_line_2
          && addr.cty == row.cty && addr.st == row.st && addr.zip.substring(0, 5) == row.zip.substring(0, 5);
      });

      if (!hasAddr) {
        addresses.push({
          adr_ln_1: row.adr_ln_1,
          adr_ln_2: row.adr_ln_2,
          ln_2_sprs: row.ln_2_sprs,
          cty: row.cty,
          st: row.st,
          zip: row.zip
        });
      }
    }


  }

})();
