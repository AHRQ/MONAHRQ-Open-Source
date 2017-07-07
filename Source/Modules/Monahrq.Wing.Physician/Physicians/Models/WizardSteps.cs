using System;
using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.Physicians.ViewModels;
using Monahrq.Wing.Physician.Physicians.Views;

namespace Monahrq.Wing.Physician.Physicians.Models
{
	/// <summary>
	/// Model used to create the Model steps used for progressing through the Physician Import screens.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.Physician.Physicians.Models.WizardContext}" />
	public class WizardSteps : StepCollection<WizardContext>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardSteps"/> class.
		/// </summary>
		/// <param name="dataTypeModel">The data type model.</param>
		/// <param name="datasetId">The dataset identifier.</param>
		/// <exception cref="ApplicationException">Unrecognized Medicare Provider Charge import type</exception>
		public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
        {
            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;

            // Create a group of wizard steps giving the ViewModel a function to import each line of the file
            if (dataTypeModel.Target.Guid == PhysicianConstants.WingTargetGuidAsGuid)
            {
                var importer = new ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else
                throw new ApplicationException("Unrecognized Medicare Provider Charge import type");

        }

		/// <summary>
		/// Creates the steps.
		/// </summary>
		/// <param name="importer">The importer.</param>
		/// <param name="headerLine">The header line.</param>
		/// <param name="lineFunction">The line function.</param>
		/// <returns></returns>
		private List<CompleteStep<WizardContext>> CreateSteps(ProcessFileViewModel importer, string headerLine, Func<string,bool> lineFunction)
        {
            importer.Initialize(headerLine, lineFunction);

            return new List<CompleteStep<WizardContext>>
            {
                new CompleteStep<WizardContext>
                {
                     GroupName = "Select Data File(s)",
                     ViewModel = new SelectSourceViewModel(Context),
                     ViewType = typeof(SelectSourceView)
                },
                new CompleteStep<WizardContext>
                {
                     GroupName = "Import Data File(s)",
                     ViewModel = importer,
                     ViewType = typeof(ProcessFileView)
                }
            };
        }
    }
}
