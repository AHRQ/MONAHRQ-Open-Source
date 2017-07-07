using Microsoft.Practices.Prism.Commands;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.Views;
using Monahrq.Theme.Controls.Wizard.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// the default dataset import wizard data error report view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultWizardDataErrorReportViewModel 
            : WizardStepViewModelBase<DatasetContext>
    {
        /// <summary>
        /// Gets or sets the data set date.
        /// </summary>
        /// <value>
        /// The data set date.
        /// </value>
        public string DataSetDate { get; set; }
        /// <summary>
        /// Gets or sets the name of the data set.
        /// </summary>
        /// <value>
        /// The name of the data set.
        /// </value>
        public string DataSetName { get; set; }
        /// <summary>
        /// Gets or sets the severe errors label.
        /// </summary>
        /// <value>
        /// The severe errors label.
        /// </value>
        public string SevereErrorsLabel { get; set; }
        /// <summary>
        /// Gets or sets the severe errors.
        /// </summary>
        /// <value>
        /// The severe errors.
        /// </value>
        public DataTable SevereErrors { get; set; }
        /// <summary>
        /// Gets or sets the invalid values label.
        /// </summary>
        /// <value>
        /// The invalid values label.
        /// </value>
        public string InvalidValuesLabel { get; set; }
        /// <summary>
        /// Gets or sets the invalid values.
        /// </summary>
        /// <value>
        /// The invalid values.
        /// </value>
        public DataTable InvalidValues { get; set; }
        /// <summary>
        /// Gets or sets the warnings label.
        /// </summary>
        /// <value>
        /// The warnings label.
        /// </value>
        public string WarningsLabel { get; set; }
        /// <summary>
        /// Gets or sets the warnings.
        /// </summary>
        /// <value>
        /// The warnings.
        /// </value>
        public DataTable Warnings { get; set; }
        /// <summary>
        /// Gets the navigate mappings.
        /// </summary>
        /// <value>
        /// The navigate mappings.
        /// </value>
        public ICommand NavigateMappings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardDataErrorReportViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public DefaultWizardDataErrorReportViewModel(DatasetContext c)
            : base(c)
        {
            #region Mock Values
            DataSetName = "Inpatient Hospital Data";
            DataSetDate = "Quarter 2, 2012";
            SevereErrorsLabel = "Severe Errors (5):";
            InvalidValuesLabel = "Invalid Values (3):";
            WarningsLabel = "Warnings (2):";

            DataTable dt = new DataTable("SevereErrors");
            dt.Columns.Add(new DataColumn("Name", typeof(string)));
            dt.Columns.Add(new DataColumn("Description", typeof(string)));

            DataRow dr = dt.NewRow();
            dr["Name"] = "Age";
            dr["Description"] = "Required Field Empty - Record will not be loaded";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Sex";
            dr["Description"] = "Required Field Empty - Record will not be loaded";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 4";
            dr["Description"] = "Required Field Empty - Record will not be loaded";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 5";
            dr["Description"] = "Required Field Empty - Record will not be loaded";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 7";
            dr["Description"] = "Required Field Empty - Record will not be loaded";
            dt.Rows.Add(dr);

            SevereErrors = dt;
            
            dt = new DataTable("InvalidValues");
            dt.Columns.Add(new DataColumn("Name"));
            dt.Columns.Add(new DataColumn("Description"));

            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 8";
            dr["Description"] = "Invalid value. Valid codes must be at least 3 characters.";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 9";
            dr["Description"] = "Invalid value. Valid codes must be at least 3 characters.";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 12";
            dr["Description"] = "Invalid value. Valid codes must be at least 3 characters.";
            dt.Rows.Add(dr);
            
            InvalidValues = dt;
            
            dt = new DataTable("Warnings");
            dt.Columns.Add(new DataColumn("Name"));
            dt.Columns.Add(new DataColumn("Description"));

            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 13";
            dr["Description"] = "Column of ICD-9-CM codes does not have any leading zeros.";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Name"] = "Diagnosis Code 14";
            dr["Description"] = "Column of ICD-9-CM codes does not have any leading zeros.";
            dt.Rows.Add(dr);

            Warnings = dt;

            #endregion Mock Values

            WireNavigators();
        }

        /// <summary>
        /// Wires the navigators.
        /// </summary>
        private void WireNavigators()
        {
            NavigateMappings = new DelegateCommand(() => ExecuteNavigateMappings());
        }

        /// <summary>
        /// Executes the navigate mappings.
        /// </summary>
        private void ExecuteNavigateMappings()
        {
            var steps = this.DataContextObject.Steps.Collection as Dictionary<StepGroup, List<CompleteStep<DatasetContext>>>;
            var stepDefinitions = steps.SelectMany(st => st.Value).ToList();
            var stepContext = stepDefinitions.FirstOrDefault(st => st.ViewType == typeof(DefaultWizardColumnMappingSummaryView));
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<WizardRequestNavigateEvent<DatasetContext>>()
                .Publish(new WizardRequestNavigateEventArgs<DatasetContext>(stepContext));
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Validation Report"; }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }
    }
}
