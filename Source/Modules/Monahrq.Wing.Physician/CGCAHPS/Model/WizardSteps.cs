using System;
using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.CGCAHPS.ViewModel;
using Monahrq.Wing.Physician.CGCAHPS.Views;

namespace Monahrq.Wing.Physician.CGCAHPS.Model
{
	/// <summary>
	/// Model used to create the Model steps used for progressing through the Physician CGCAHPS Import screens.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.Physician.CGCAHPS.Model.WizardContext}" />
	public class WizardSteps : StepCollection<WizardContext>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardSteps"/> class.
		/// </summary>
		/// <param name="dataTypeModel">The data type model.</param>
		/// <exception cref="ApplicationException">Unrecognized NH-CAHPS Survey Results import type</exception>
		public WizardSteps(DataTypeModel dataTypeModel)
        {
            Context.SelectedDataType = dataTypeModel;
            Context.Steps = this;


            // Create a group of wizard steps giving the ViewModel a function to import each line of the file
            if (dataTypeModel.Target.Guid == CGCAHPSConstants.WingGuidAsGuid)
            {
                var importer = new ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else
                throw new ApplicationException("Unrecognized NH-CAHPS Survey Results import type");

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
