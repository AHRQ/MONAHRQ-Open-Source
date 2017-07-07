using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;

namespace Monahrq.Wing.NursingHomeCompare.NHC.Model
{
	/// <summary>
	/// Model used for progressing through the NH Defieciency Import screens.
	/// </summary>
	/// <seealso cref="Monahrq.DataSets.Model.DatasetContextBase" />
	[Export,
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContextBase
    {
		/// <summary>
		/// The record key
		/// </summary>
		const string RECORD_KEY = "{051D3B42-4578-4324-9498-DCFA11626113}";

		/// <summary>
		/// Initializes a new instance of the <see cref="WizardContext"/> class.
		/// </summary>
		public WizardContext()
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe((e) => Cancelled(this, EventArgs.Empty));
            InitCurrentImport();
        }

        public int Month { get; set; }

		/// <summary>
		/// Occurs when [cancelled].
		/// </summary>
		public event EventHandler Cancelled = delegate { };
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>
		/// The name of the file.
		/// </value>
		public string FileName { get; set; }
		/// <summary>
		/// Gets or sets the summary.
		/// </summary>
		/// <value>
		/// The summary.
		/// </value>
		public string Summary { get; set; }
		/// <summary>
		/// Gets or sets the current import.
		/// </summary>
		/// <value>
		/// The current import.
		/// </value>
		internal Lazy<Dataset> CurrentImport { get; set; }

		/// <summary>
		/// Initializes the current import.
		/// </summary>
		private void InitCurrentImport()
        {
            CurrentImport = new Lazy<Dataset>(() =>
            {
                if (DatasetItem == null)
                {
                    InitCurrentImport();
                    return null;
                }
                var temp = this[RECORD_KEY] as Dataset;
                if (temp == null)
                {
                    using (var session = Provider.SessionFactory.GetCurrentSession())
                    {
                        temp = session.Get<Dataset>(DatasetItem.Id);
                    }
                    if (temp == null)
                    {
                        InitCurrentImport();
                        return null;
                    }

                    this[RECORD_KEY] = temp;
                }
                return temp;
            }, true);
        }

		/// <summary>
		/// Applies the summary record.
		/// </summary>
		/// <param name="mappingOnly">if set to <c>true</c> [mapping only].</param>
		protected override void ApplySummaryRecord(bool mappingOnly = false)
        {
            var currentContentItem = DatasetItem ?? new Dataset();
            //var summary = new ContentItemSummaryRecord();
            currentContentItem.SummaryData = Summary;
            //currentContentItem.Summary = summary;
        }
    }
}