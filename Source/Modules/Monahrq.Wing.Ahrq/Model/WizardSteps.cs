using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Collections.Generic;
using Monahrq.Wing.Ahrq.ViewModel;
using Monahrq.Wing.Ahrq.ViewModels;

namespace Monahrq.Wing.Ahrq.Model
{
    /// <summary>
    /// Class for wizard steps.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.Ahrq.Model.WizardContext}" />
    public class WizardSteps : StepCollection<WizardContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardSteps"/> class.
        /// </summary>
        /// <param name="dataTypeModel">The data type model.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <exception cref="System.ApplicationException">Unrecognized AHRQ import type</exception>
        public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
        {
            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;


            // Create a group of wizard steps giving the ViewModel a function to import each line of the file
            if (dataTypeModel.Target.Guid == Area.Constants.WingTargetGuidAsGuid)
            {
                var importer = new Area.ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else if (dataTypeModel.Target.Guid == Composite.Constants.WingTargetGuidAsGuid)
            {
                var importer = new Composite.ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else if (dataTypeModel.Target.Guid == Provider.Constants.WingTargetGuidAsGuid)
            {
                var importer = new Provider.ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else
                throw new ApplicationException("Unrecognized AHRQ import type");

        }

        /// <summary>
        /// Creates the wizard steps.
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
                new CompleteStep<WizardContext>()
                {
                     GroupName = "Select Data File(s)",
                     ViewModel = new SelectSourceViewModel(Context),
                     ViewType = typeof(Views.SelectSourceView)
                },
                new CompleteStep<WizardContext>()
                {
                     GroupName = "Import Data File(s)",
                     ViewModel = importer,
                     ViewType = typeof(Views.ProcessFileView)
                }
            };
        }
    }
}
