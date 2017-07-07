using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Services.Contracts;
using Monahrq.Theme.Controls.Wizard.Models;


[assembly: WingAssembly("EA37972D-4C6C-4C97-8DAD-1A8844D6763E", "AHA Annual Survey Database", "AHA Annual Survey Database")]


namespace Monahrq.Wing.AhaCms
{

    static class Constants
    {
        public const string WingGuid = "{106EC787-36AB-4D9C-9D85-5157FBD185E2}";
    }

    [DatasetWingExport]
    public class DatasetWing : DatasetWing<AhaCmsTarget>
    {

    }


    [Monahrq.Sdk.Attributes.Wings.WingModule(typeof(AhaCmsTarget)
        , Constants.WingGuid
        , "AHA Annual Survey Database"
        , "AHA Annual Survey Database"
        , DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable)
    ]
    public partial class Module : WingModule
    {
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
        }

       

        private void Subscribe()
        {
            Events.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid>>()
                .Subscribe((args) =>
                {
                    if (args.Id == WingGUID)
                    {
                        args.WizardSteps = new WizardSteps(args.Data);
                    }
                });
        }
        
    }
}
