using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using System.Data;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The default dataset import wizard import results view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultWizardImportResultsViewModel : WizardStepViewModelBase<DatasetContext>
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
        /// Gets or sets the import totals.
        /// </summary>
        /// <value>
        /// The import totals.
        /// </value>
        public DataTable ImportTotals { get; set; }
        /// <summary>
        /// Gets or sets the record warnings.
        /// </summary>
        /// <value>
        /// The record warnings.
        /// </value>
        public DataTable RecordWarnings { get; set; }
        /// <summary>
        /// Gets or sets the value stats.
        /// </summary>
        /// <value>
        /// The value stats.
        /// </value>
        public DataTable ValueStats { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardImportResultsViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public DefaultWizardImportResultsViewModel(DatasetContext c)
            : base(c)
        {
            #region Mock Values
#if MOCK
            DataSetName = "Inpatient Hospital Data";
            DataSetDate = "Quarter 2, 2012";

            DataTable dt = new DataTable("ImportTotals");

            dt.Columns.Add(new DataColumn("Description", typeof(string)));
            dt.Columns.Add(new DataColumn("Count", typeof(int)));

            DataRow dr = dt.NewRow();
            dr["Description"] = "Rows Loaded:";
            dr["Count"] = 631393;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Description"] = "Rows Excluded:";
            dr["Count"] = 128015;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Description"] = "Data Elements Per Record:";
            dr["Count"] = 43;
            dt.Rows.Add(dr);

            ImportTotals = dt;
            
            dt = new DataTable("RecordWarnings");
            dt.Columns.Add(new DataColumn("Column", typeof(string)));
            dt.Columns.Add(new DataColumn("Records_Affected", typeof(int)));
            dt.Columns.Add(new DataColumn("Message", typeof(string)));

            dr = dt.NewRow();
            dr["Column"] = "Age";
            dr["Records_Affected"] = 128009;
            dr["Message"] = "Required field empty - Rows not loaded";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Race";
            dr["Records_Affected"] = 5407;
            dr["Message"] = "Value mapped to null based on crosswalk (info)";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Sex";
            dr["Records_Affected"] = 5;
            dr["Message"] = "Value mapped to null based on crosswalk (info)";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Race";
            dr["Records_Affected"] = 5407;
            dr["Message"] = "Value mapped to null based on crosswalk (info)";
            dt.Rows.Add(dr);

            RecordWarnings = dt;

            dt = new DataTable("ValueStats");
            dt.Columns.Add(new DataColumn("Column", typeof(string)));
            dt.Columns.Add(new DataColumn("Number_Missing", typeof(int)));
            dt.Columns.Add(new DataColumn("Minimum", typeof(string)));
            dt.Columns.Add(new DataColumn("Maximum", typeof(string)));

            dr = dt.NewRow();
            dr["Column"] = "Age";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "0";
            dr["Maximum"] = "194";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Age in Days";
            dr["Number_Missing"] = 518634;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Race";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Sex";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Primary Payer";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Patient State / County Code";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Hospital ID";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Discharge Disposition";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Column"] = "Admission Type";
            dr["Number_Missing"] = 0;
            dr["Minimum"] = "-";
            dr["Maximum"] = "-";
            dt.Rows.Add(dr);

            ValueStats = dt;
#endif
            #endregion Mock Values
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Import results"; }
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
