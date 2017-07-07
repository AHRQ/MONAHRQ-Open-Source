using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Utility;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Physician.Physicians.Models
{
	/// <summary>	
	/// Model used for progressing through the Physician CGCAHPS Import screens.
	/// </summary>
	/// <seealso cref="Monahrq.DataSets.Model.DatasetContext" />
	[Export,
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContext
    {
		#region Fields and Constants

		/// <summary>
		/// The record key
		/// </summary>
		const string RECORD_KEY = "{1DDA3E11-D5DB-4DF0-9BFB-625591C73167}";          // TODO: per content type

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the selected states.
		/// </summary>
		/// <value>
		/// The selected states.
		/// </value>
		public List<string> SelectedStates { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is physician managed in monahrq.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is physician managed in monahrq; otherwise, <c>false</c>.
		/// </value>
		public bool IsPhysicianManagedInMONAHRQ { get; set; }

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>
		/// The name of the file.
		/// </value>
		public string FileName { get; set; }

		/// <summary>
		/// Gets or sets the file.
		/// </summary>
		/// <value>
		/// The file.
		/// </value>
		public FileProgress File { get; set; }

		/// <summary>
		/// Gets or sets the summary.
		/// </summary>
		/// <value>
		/// The summary.
		/// </value>
		public string Summary { get; set; }

		/// <summary>
		/// Gets or sets the type of the current import.
		/// </summary>
		/// <value>
		/// The type of the current import.
		/// </value>
		internal Target CurrentImportType { get; set; }

		/// <summary>
		/// Occurs when [cancelled].
		/// </summary>
		public event EventHandler Cancelled = delegate { };

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WizardContext"/> class.
		/// </summary>
		public WizardContext()
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));

            InitCurrentImport();
        }

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the current import.
		/// </summary>
		private void InitCurrentImport()
        {
            if (DatasetItem == null) DatasetItem = new Dataset();
            this[RECORD_KEY] = DatasetItem;

            DeletePreviousImport();
        }

		/// <summary>
		/// Applies the summary record.
		/// </summary>
		/// <param name="mappingOnly">if set to <c>true</c> [mapping only].</param>
		protected override void ApplySummaryRecord(bool mappingOnly = false)
        {
            //    var currentContentItem = CurrentContentItem ?? new Dataset();
            //    var summary = new ContentItemSummaryRecord {Data = Summary};
            //    currentContentItem.Summary = summary;
        }

		/// <summary>
		/// Deletes the previous import.
		/// </summary>
		private void DeletePreviousImport()
        {
            using (
                var session =
                    ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory.OpenSession())
            {
                var contentItemRecord =
                    session.Query<Dataset>()
                           .FirstOrDefault(c => c.ContentType.Name.ToUpper() == "PHYSICIAN DATA");

                if (contentItemRecord == null)
                    return;

                using (var transaction = session.BeginTransaction())
                {
                    //var transactionDelete = string.Format("delete from {0} where Dataset_Id = {1}",
                    //                                      typeof(DatasetTransactionRecord).EntityTableName(),
                    //                                      contentItemRecord.Id);

                    //session.CreateSQLQuery(transactionDelete)
                    //       .SetTimeout(5000)
                    //       .ExecuteUpdate();

                    session.CreateSQLQuery(string.Format("TRUNCATE TABLE {0}", typeof(PhysicianTarget).EntityTableName()))
                           .SetTimeout(5000)
                           .ExecuteUpdate();

                    //session.Delete(contentItemRecord);

                    transaction.Commit();
                }

                //using (var truncateCommand = new SqlCommand())
                //{
                //    truncateCommand.Connection = sqlConnection;
                //    truncateCommand.CommandType = CommandType.Text;
                //    truncateCommand.CommandText =;
                //    truncateCommand.ExecuteNonQuery();
                //}
            }
        }

        #endregion
    }
}
