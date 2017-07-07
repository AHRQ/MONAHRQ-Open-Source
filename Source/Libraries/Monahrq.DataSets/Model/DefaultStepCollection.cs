using Monahrq.DataSets.ViewModels;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.DataSets.ViewModels.MappingSummary;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.DataSets.Views;
using Monahrq.Theme.Controls.Wizard.Models;
using System.Collections.Generic;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset full wizard default wizard step collection.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultStepCollection : StepCollection<DatasetContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStepCollection"/> class.
        /// </summary>
        /// <param name="dataTypeModel">The data type model.</param>
        /// <param name="exsitingTypeId">The exsiting type identifier.</param>
        public DefaultStepCollection(DataTypeModel dataTypeModel, int? exsitingTypeId)
            : base()
        {
            Context.ExistingDatasetId = exsitingTypeId;
            Context.SelectedDataType = dataTypeModel;
            Context.Steps = this;
            ApplySteps();
        }

        /// <summary>
        /// Applies the steps.
        /// </summary>
        protected virtual void ApplySteps()
        {

            var list = new List<CompleteStep<DatasetContext>>
                {
                    new CompleteStep<DatasetContext>
                        {
                            ViewModel = new DefaultWizardSelectFileViewModel(Context),
                            ViewType = typeof (DefaultWizardSelectFileView),        // select file
                            Visited = true
                        },
                    new CompleteStep<DatasetContext>
                        {
                            ViewModel = new DefaultWizardFileReadabilityViewModel(Context),
                            ViewType = typeof (DefaultWizardFileReadabilityView)    // check file
                        },
                };

            var list2 = new List<CompleteStep<DatasetContext>>
                {
                    new CompleteStep<DatasetContext>
                        {
                            ViewModel =  new DefaultWizardColumnMappingViewModel(Context),
                            ViewType = typeof (DefaultWizardColumnMappingView)      // required
                        },
                      //new CompleteStep<DatasetContext>()
                      //  {
                      //      ViewModel = new DefaultWizardOptionalColumnMappingViewModel(Context),
                      //      ViewType = typeof (DefaultWizardColumnMappingView)      // optional
                      //  },

                    new CompleteStep<DatasetContext>
                        {
                            ViewModel = new FieldsViewModel(Context),
                            ViewType = typeof (FieldsViewCorrect)          // crosswalk
                        },

                 
                };

            var list3 = new List<CompleteStep<DatasetContext>>
                {
                            new CompleteStep<DatasetContext>
                        {
                            ViewModel = new Report(Context),
                            ViewType = typeof (DefaultWizardColumnMappingSummaryView)
                        },

                        
                    new CompleteStep<DatasetContext>
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