using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.NursingHomeCompare.NHC.ViewModel;
using Monahrq.Wing.NursingHomeCompare.NHC.Views;

namespace Monahrq.Wing.NursingHomeCompare.NHC.Model
{
	/// <summary>
	/// Model used to create the Model steps used for progressing through the NHCompare Import screens.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.NursingHomeCompare.NHC.Model.WizardContext}" />
	public class WizardSteps : StepCollection<WizardContext>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardSteps"/> class.
		/// </summary>
		/// <param name="dataTypeModel">The data type model.</param>
		/// <param name="datasetId">The dataset identifier.</param>
		public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
        {
            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;
            AddGroupOfSteps(new StepGroup("Nursing Home Compare Data") { IsCurrent = true }, CreateSteps());
        }

		/// <summary>
		/// Creates the steps.
		/// </summary>
		/// <returns></returns>
		private List<CompleteStep<WizardContext>> CreateSteps()
        {
            return new List<CompleteStep<WizardContext>>
            {
                new CompleteStep<WizardContext>
                {
                     GroupName = "Select Data File",
                      ViewModel = new SelectSourceViewModel(Context),
                      ViewType = typeof(NursingHomeCompare.Views.SelectSourceView)
                },
                new CompleteStep<WizardContext>
                {
                     GroupName = "Import Data File",
                      ViewModel = new ProcessFileViewModel(Context),
                      ViewType = typeof(ProcessFileView)
                }
            };
        }
    }
}
