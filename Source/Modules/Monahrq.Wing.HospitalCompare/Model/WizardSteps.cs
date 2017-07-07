using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Wing.HospitalCompare.Model
{
	/// <summary>
	/// Model used for setting up the Modes for progressing through the HospitalCompare Import screens.
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.HospitalCompare.Model.WizardContext}" />
	public class WizardSteps : StepCollection<WizardContext>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardSteps"/> class.
		/// </summary>
		/// <param name="dataTypeModel">The data type model.</param>
		/// <param name="datasetId">The dataset identifier.</param>
		public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
            : base()
        {
            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;
            this.AddGroupOfSteps(new StepGroup("Hospital Compare Data") { IsCurrent = true }, CreateSteps());
        }

		/// <summary>
		/// Creates the steps.
		/// </summary>
		/// <returns></returns>
		private List<CompleteStep<WizardContext>> CreateSteps()
        {
            return new List<CompleteStep<WizardContext>>()
            {
                new CompleteStep<WizardContext>()
                {
                     GroupName = "Select Data File",
                      ViewModel = new ViewModel.SelectSourceViewModel(Context),
                      ViewType = typeof(Views.SelectSourceView)
                },
                new CompleteStep<WizardContext>()
                {
                     GroupName = "Import Data File",
                      ViewModel = new ViewModel.ProcessFileViewModel(Context),
                      ViewType = typeof(Views.ProcessFileView)
                }
            };
        }
    }
}
