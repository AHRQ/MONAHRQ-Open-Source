$.monahrq.Flutter.Configs.demoFlutter = {
  id: 'org.myagency.DemoFlutter',
  displayName: 'Demo Flutter',
  moduleName: 'flutters.demoFlutter',
  assets: {
    scripts: ['flutter.js', 'report-ns.js'],
    styles: ['flutter.css'],
    templates: ['views/flutter.html']
  },
  menuItems: [
    {
      menu: 'main',
      id: 'org.myagency.DemoFlutter.Menus.MainTab',
      reportId: 'org.myagency.DemoFlutter.Reports.NursingHomes',
      label: 'Demo Flutter',
      priority: 1,
      primary: true,
      classes: [],
      route: {
        name: 'top.professional.flutters.demoFlutter',
        params: []
      }
    }
  ],
  reports: [
    {
      id: 'org.myagency.DemoFlutter.Reports.NursingHomes',
      displayName: 'Demo Nursing Home Report',
      page: {
        title: "Demo Nursing Home Report",
        header: "page header",
        footer: "page footer"
      },
      custom: {
        // SimpleReportLoader config to load a single nursing home report.
        nursingHome: {
          rootObj: "NursingHomes.Report",
          reportName: 'NursingHomes',
          reportDir: 'Data/NursingHomes/NursingHomes/',
          filePrefix: 'NursingHome_'
        },

        // SimpleReportLoader config to load measure definition base data files.
        measureDefs: {
          rootObj: "NursingHomes.Base",
          reportName: 'MeasureDescriptions',
          reportDir: 'Data/Base/NursingHomeMeasures/',
          filePrefix: 'MeasureDescription_'
        },

        // SimpleReportLoader config to load a single base data file.
        nursingHomeIndex: {
          rootObj: "NursingHomes.Base",
          reportName: 'NursingHomeIndex',
          reportDir: 'Data/Base/',
          filePrefix: 'NursingHomeIndex'
        }
      }
    }
  ]
};
