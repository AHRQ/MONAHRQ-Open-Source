using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.DataSets.ViewModels
{
    public class DefaultWizardCheckForDataErrorsViewModel : WizardStepViewModelBase<DatasetContext>
    {
        public string DataSetDate { get; set; }
        public string DataSetName { get; set; }
        public string FileName { get; set; }
        //public int FileSize { get; set; }
        public int FirstRowRecords { get; set; }
        public int InvalidValues { get; set; }
        public int PercentageComplete { get; set; }
        public int Records { get; set; }
        public int RecordsNotExported { get; set; }
        //public int SevereErrors { get; set; }
        public int TotalFileSize { get; set; }
        //public int Warnings { get; set; }
        public string FeedbackText { get; set; }

        public DefaultWizardCheckForDataErrorsViewModel(DatasetContext c)
            : base(c)
        {
            #region Mock Values
            DataSetName = "Inpatient Hospital Data";
            DataSetDate = "Quarter 2, 2012";
            PercentageComplete = 50;
            //FileSize = 1234567;
            TotalFileSize = 4357987;
            Records = 123456;
            RecordsNotExported = 24;
            FileName = "FAUX_Data_NBH_4758.csv";
            FirstRowRecords = 24;
            //SevereErrors = 1;
            InvalidValues = 0;
            //Warnings = 26;
            FeedbackText = "sample feedback text";
            #endregion Mock Values
        }

        public override string DisplayName
        {
            get { return "Validation"; }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
