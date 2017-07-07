using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels;
using Monahrq.DataSets.ViewModels.MappingSummary;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.DataSets.Views;
using Monahrq.Theme.Controls.Wizard.Models;

namespace Monahrq.Wing.AhaCms
{
    public class WizardSteps : DefaultStepCollection
    {
          public WizardSteps(DataTypeModel dataTypeModel): base(dataTypeModel)
          {
          }

          protected override void ApplySteps()
          {
              var list = new List<CompleteStep<DatasetContext>>()
                {
                    new CompleteStep<DatasetContext>()
                        {
                            ViewModel = new DefaultWizardSelectFileViewModel(Context),
                            ViewType = typeof (DefaultWizardSelectFileView),        // select file
                            Visited = true
                        },
                    new CompleteStep<DatasetContext>()
                        {
                            ViewModel = new DefaultWizardFileReadabilityViewModel(Context),
                            ViewType = typeof (DefaultWizardFileReadabilityView)    // check file
                        },
                };

              var list2 = new List<CompleteStep<DatasetContext>>
                {
                    new CompleteStep<DatasetContext>()
                        {
                            ViewModel =  new DefaultWizardColumnMappingViewModel(Context),
                            ViewType = typeof (DefaultWizardColumnMappingView)      // required
                        },
                      new CompleteStep<DatasetContext>()
                        {
                            ViewModel = new DefaultWizardOptionalColumnMappingViewModel(Context),
                            ViewType = typeof (DefaultWizardColumnMappingView)      // optional
                        },
                 
                };

              var list3 = new List<CompleteStep<DatasetContext>>
                {
                            new CompleteStep<DatasetContext>()
                        {
                            ViewModel = new Report(Context),
                            ViewType = typeof (DefaultWizardColumnMappingSummaryView)
                        },

                        
                    new CompleteStep<DatasetContext>()
                        {
                            ViewModel = new ValidationViewModel(Context),
                            ViewType = typeof (ValidationView)
                        },
                };

              AddGroupOfSteps(new StepGroup("Import Data"), list);
              AddGroupOfSteps(new StepGroup("Data Mapping"), list2);
              AddGroupOfSteps(new StepGroup("Complete"), list3);
          }
           
     
    }
}
