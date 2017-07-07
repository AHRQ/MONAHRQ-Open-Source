using System;
using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.HEDIS.ViewModel;
using Monahrq.Wing.Physician.HEDIS.Views;

namespace Monahrq.Wing.Physician.HEDIS.Model
{
    public class WizardSteps : StepCollection<WizardContext>
    {
        public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
        {
            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;


            // Create a group of wizard steps giving the ViewModel a function to import each line of the file
            if (dataTypeModel.Target.Guid == HEDISConstants.WingGuidAsGuid)
            {
                var importer = new ImportProcessor(Context);

                AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) { IsCurrent = true },
                    CreateSteps(importer, importer.HeaderLine, importer.LineFunction));
            }
            else
                throw new ApplicationException("Unrecognized NH-CAHPS Survey Results import type");

        }

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
