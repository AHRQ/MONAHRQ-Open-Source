/*
 * Basic Table Flutter Config
 */
$.monahrq.Flutter.Configs.BasicTableFlutter = {
  // Uniquely identify this flutter config. This id appears in the flutter registry.
  id: 'gov.ahrq.BasicTableFlutter',
  // User-friendly name for the flutter.
  displayName: 'Basic Table Flutter',
  // The angular module to which the flutter adds its components.
  moduleName: 'flutters.BasicTableFlutter',

  // What files are loaded as part of the flutter. Relative to the modulePath from the registry.
  assets: {
    scripts: ['basic-table.js', 'vendor/angular-smart-table/dist/smart-table.min.js'],
    styles: ['basic-table.css'],
    templates: ['views/basic-table.html']
  },


  // What menu links the flutter adds to the monahrq navigation.
  menuItems: [
    {
      // Whether the flutter should add to the consumer or professional product.
      product: 'professional',
      // What menu this item should be added to. Currently only 'main' is supported, for the top nav.
      menu: 'main',
      // The type of menu item. This should always be 'flutter'.
      type: 'flutter',
      // An identifier for this menu item.
      id: 'gov.ahrq.BasicTableFlutter.Menus.MainTab',
      // The id of the parent menu item to attach to (as defined in flutter\flutter-main-menu.js). Use null to place this
      // at the top level of the menu.
      parent: 'p5',
      // What reporting entity this flutter consumes -- HOSPITAL, NURSINGHOME, PHYSICIAN. Normally null.
      entity: null,

      // What report this menu item points to. Should match the 'id' field in reports.
      reportId: 'gov.ahrq.BasicTableFlutter.Reports.HCUP',

      // Label for menu item.
      label: 'HCUP Report',

      // Sort order.
      priority: 1,

      // If we can only display one menu item, which should it be.
      primary: true,

      // CSS classes to add to menu item.
      classes: [],

      // The ui-router state this menu item activates.
      route: {
        // State name.
        name: 'top.professional.flutters.BasicTableFlutter',
        // What state name (or portion of a name) should be checked against the current state to determine 'active'
        // status of the menu item.
        activeName: 'top.professional.flutters.BasicTableFlutter',
        // Any extra state parameters (eg, if you want to set a default search).
        params:  {
          // The basic table flutter uses a reportId parameter. The documentation of other flutters should be consulted
          // to determine what parameters they define.
          reportId: 'gov.ahrq.BasicTableFlutter.Reports.HCUP'
        }
      }
    }
  ],

  // What reports the flutter provides. A report is usually a combination of a flutter and a wing.
  reports: [
    {
      // Unique identifier for the report
      id: 'gov.ahrq.BasicTableFlutter.Reports.HCUP',
      // User-friendly name for the report
      displayName: 'HCUP County Hospital Stays',

      // Every report must allow displaying basic information on its page.
      page: {
        title: "HCUP County Hospital Stays",
        header: "page header",
        footer: "page footer"
      },

      // Because each flutter may visualize data differently, it may define its own configuration for how data is
      // loaded and displayed.
      custom: {
        table: {
          hasGlobalSearch: true,
          hasPager: true,
          columns: [
            {name: 'county_name', label: 'County Name', format: 'html'},
            {name: 'total_number_of_discharges', label: 'Total Number of Discharges', format: 'number'},
            {name: 'mean_los', label: 'Mean Length of Stay', format: 'number', formatOptions: [2]},
            {name: 'mean_cost_stay', label: 'Mean Cost of Stay', format: 'nfcurrency'}
          ]
        },
        report: {
          rootObj: "flutters.hcupcountyhospitalstaysdata.report.summary",
          reportName: 'Data',
          reportDir: 'Data/Wings/hcupcountyhospitalstaysdata/',
          filePrefix: 'summary'
        }
      }
    }
  ]
};
