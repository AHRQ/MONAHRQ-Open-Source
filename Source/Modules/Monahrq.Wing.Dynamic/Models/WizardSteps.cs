using System;
using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.ViewModels;
using Monahrq.Wing.Dynamic.Views;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Class to define the wizard steps
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.StepCollection{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    public class WizardSteps : StepCollection<WizardContext>
    {
        /// <summary>
        /// The dynamic type model
        /// </summary>
        private DataTypeModel _dynamicTypeModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardSteps"/> class.
        /// </summary>
        /// <param name="dataTypeModel">The data type model.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <exception cref="ApplicationException"></exception>
        public WizardSteps(DataTypeModel dataTypeModel, int? datasetId)
        {
            _dynamicTypeModel = dataTypeModel;
            var dynamicTargetGuid = dataTypeModel.Target.Guid;

            //var sessionProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            //using (var session = sessionProvider.SessionFactory.OpenStatelessSession())
            //{
            //    var  = session.CreateCriteria<Target>()
            //                               .Add(Restrictions.InsensitiveLike("Name", dataTypeModel.DataTypeName))
            //                               .SetMaxResults(1)
            //                               //.SetProjection(Property.ForName("Guid"))
            //                               .UniqueResult();
            //}                              

            Context.SelectedDataType = dataTypeModel;
            Context.ExistingDatasetId = datasetId;
            Context.Steps = this;
            Context.CustomTargetName = dataTypeModel.DataTypeName;
            if (Context.CustomTarget == null)
                Context.CustomTarget = dataTypeModel.Target;

            Context.InitContext();

            // Create a group of wizard steps giving the ViewModel a function to import each line of the file
            if (base.Context.CustomTarget.Guid == dynamicTargetGuid)
            {
                //var importer = new SimpleImportProcessor(Context);

                //importer.Initialize(importer.HeaderLine, importer.LineFunction);

                var list = new List<CompleteStep<WizardContext>>();
                var list2 = new List<CompleteStep<WizardContext>>();
                var list3 = new List<CompleteStep<WizardContext>>();
                if (Context.CustomTarget.ImportType == DynamicStepTypeEnum.Simple)
                {
                    var importer = new SimpleImportProcessor(Context);
                    importer.Initialize(importer.HeaderLine, importer.LineFunction);

                    list = new List<CompleteStep<WizardContext>>
                    {
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Select Data File(s)",
                            ViewModel = new SimpleSelectSourceViewModel(Context),
                            ViewType = typeof (SelectSourceView)
                        },
                     
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Import Data File(s)",
                            ViewModel = importer,
                            ViewType = typeof (ProcessFileView)
                        }
                    };

                    AddGroupOfSteps(new StepGroup("Import Data"), list);
                    //AddGroupOfSteps(new StepGroup("Data Mapping"), list2);

                    //AddGroupOfSteps(new StepGroup(dataTypeModel.DataTypeName) {IsCurrent = true}, list);
                }
                else
                {
                    list = new List<CompleteStep<WizardContext>>
                    {
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Import Data",
                            ViewModel = new FullWizardSelectFileViewModel(Context),
                            ViewType = typeof (FullWizardSelectFileView), // select file
                            Visited = true
                        },
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Import Data",
                            ViewModel = new FullWizardFileReadabilityViewModel(Context),
                            ViewType = typeof (FullWizardFileReadabilityView) // check file
                        }
                    };

                    list2 = new List<CompleteStep<WizardContext>>
                    {
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Data Mapping",
                            ViewModel = new FullWizardColumnMappingViewModel(Context),
                            ViewType = typeof (FullWizardColumnMappingView) // required
                        },
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Data Mapping",
                            ViewModel = new FullWizardFieldsViewModel(Context),
                            ViewType = typeof (FullWizardFieldsViewCorrect) // crosswalk
                        }
                    };

                    list3 = new List<CompleteStep<WizardContext>>
                    {
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Complete",
                            ViewModel = new FullWizrdReportViewModel(Context),
                            ViewType = typeof (FullWizardColumnMappingSummaryView)
                        },
                        new CompleteStep<WizardContext>
                        {
                            //GroupName = "Complete",
                            ViewModel = new FullWizardValidationViewModel(Context),
                            ViewType = typeof (FullWizardValidationView)
                        }
                    };

                    AddGroupOfSteps(new StepGroup("Import Data"), list);
                    AddGroupOfSteps(new StepGroup("Data Mapping"), list2);
                    AddGroupOfSteps(new StepGroup("Complete"), list3);
                }
            }
            else
                throw new ApplicationException(string.Format("Unrecognized {0} import type", dataTypeModel.DataTypeName));
        }

        /// <summary>
        /// Creates the wizard steps.
        /// </summary>
        /// <param name="importer">The importer.</param>
        /// <param name="headerLine">The header line.</param>
        /// <param name="lineFunction">The line function.</param>
        /// <returns></returns>
        private List<CompleteStep<WizardContext>> CreateSteps(SimpleProcessFileViewModel importer, string headerLine,
                                                              Func<string, bool> lineFunction)
        {
            //importer.Initialize(headerLine, lineFunction);

            if (Context.CustomTarget.ImportType == DynamicStepTypeEnum.Simple)
            {
                return new List<CompleteStep<WizardContext>>
                    {

                        new CompleteStep<WizardContext>
                            {
                                GroupName = "Select Data File(s)",
                                ViewModel = new SimpleSelectSourceViewModel(Context),
                                ViewType = typeof (SelectSourceView)
                            },
                        new CompleteStep<WizardContext>
                            {
                                GroupName = "Import Data File(s)",
                                ViewModel = importer,
                                ViewType = typeof (ProcessFileView)
                            }

                    };
            }

            return new List<CompleteStep<WizardContext>>
                {
                    new CompleteStep<WizardContext>
                        {
                            GroupName = "Import Data",
                            ViewModel = new FullWizardSelectFileViewModel(Context),
                            ViewType = typeof (FullWizardSelectFileView),        // select file
                            Visited = true
                        },
                    new CompleteStep<WizardContext>
                        {
                            GroupName = "Import Data",
                            ViewModel = new FullWizardFileReadabilityViewModel(Context),
                            ViewType = typeof (FullWizardFileReadabilityView)    // check file
                        },
                    new CompleteStep<WizardContext>
                        {
                            GroupName = "Data Mapping",
                            ViewModel =  new FullWizardColumnMappingViewModel(Context),
                            ViewType = typeof (FullWizardColumnMappingView)      // required
                        },
                    new CompleteStep<WizardContext>
                        {
                            GroupName = "Data Mapping",
                            ViewModel = new FullWizardFieldsViewModel(Context),
                            ViewType = typeof (FullWizardFieldsViewCorrect)          // crosswalk
                        },
                        new CompleteStep<WizardContext>
                        {
                            GroupName = "Complete",
                            ViewModel = new FullWizrdReportViewModel(Context ),
                            ViewType = typeof (FullWizardColumnMappingSummaryView)
                        },

                        
                    new CompleteStep<WizardContext>
                        {
                            GroupName = "Complete",
                            ViewModel = new FullWizardValidationViewModel(Context),
                            ViewType = typeof (FullWizardValidationView)
                        },
                  
                };

            //AddGroupOfSteps(new StepGroup("Import Data"), list);
            //AddGroupOfSteps(new StepGroup("Data Mapping"), list2);
            //AddGroupOfSteps(new StepGroup("Complete"), list3);

            //return new DefaultStepCollection(_dynamicTypeModel).Collection.Values.ToList();

            //return new List<CompleteStep<WizardContext>>
            //    {

            //        new CompleteStep<WizardContext>
            //            {
            //                GroupName = "Select Data File(s)",
            //                ViewModel = new SelectSourceViewModel(Context),
            //                ViewType = typeof (SelectSourceView)
            //            },
            //        new CompleteStep<WizardContext>
            //            {
            //                GroupName = "Import Data File(s)",
            //                ViewModel = importer,
            //                ViewType = typeof (ProcessFileView)
            //            }

            //    };
        }

       // public void AddGroupOfSteps(StepGroup group, List<CompleteStep<TValue>> steps)
    //        where TValue: new()
    //    {
    //        foreach (var step in steps)
    //        {

    //            step.GroupName = group.DisplayName;
    //        }

    //        Collection.Add(group, steps);
    //    }
    }
}
