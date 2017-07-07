using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.DataSets.ViewModels
{
    public class DefaultWizardImportDataViewModel : WizardStepViewModelBase<DatasetContext>
    {
        public string DataSetDate { get; set; }
        public string DataSetName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int DatabaseErrorMessages { get; set; }
        public int PercentageComplete { get; set; }
        public int Records { get; set; }
        public int RecordsLoaded { get; set; }
        public int RecordsNotExported { get; set; }
        public int RecordsNotLoaded { get; set; }
        public int TotalFileSize { get; set; }

        public DefaultWizardImportDataViewModel(DatasetContext c) : base(c)
        {
        }

        public override string DisplayName
        {
            get { return "Import data"; }
        }

        public override bool IsValid()
        {
            return true;
        }


        void InitModel()
        {
            DataSetName = DataContextObject.SelectedDataType.DataTypeName;
            DataSetDate = DataContextObject.Entry.TimePeriod;
            PercentageComplete = 0;
            var fi = new FileInfo(DataContextObject.DatasourceDefinition.CurrentFile);
            FileSize = fi.Length;
            Records = DataContextObject.TargetsToImport.Count();
            RecordsNotExported = 0;
            FileName = DataContextObject.DatasourceDefinition.CurrentFile;
            RecordsLoaded = 0;
            RecordsNotLoaded = 0;
            DatabaseErrorMessages = 0;
        }
    }
}
