using System;
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

namespace Monahrq.Wing.Physician.CGCAHPS.Model
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
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardContext"/> class.
		/// </summary>
		public WizardContext()
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));
            
            InitCurrentImport();
        }

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
		/// The record key
		/// </summary>
		const string RECORD_KEY = "{E79C02F4-8ADB-421D-B28C-176ED2723114}";          // TODO: per content type
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
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>
		/// The name of the file.
		/// </value>
		public string FileName { get; set; }

		/// <summary>
		/// Initializes the current import.
		/// </summary>
		private void InitCurrentImport()
        {
            if(DatasetItem == null) DatasetItem = new Dataset();
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
                           .FirstOrDefault(c => c.ContentType.Name.ToUpper() == "CG-CAHPS SURVEY RESULTS DATA");

                if (contentItemRecord == null)
                    return;

                using (var transaction = session.BeginTransaction())
                {
                    var transactionDelete = string.Format("delete from {0} where Dataset_Id = {1}",
                                                          typeof(DatasetTransactionRecord).EntityTableName(),
                                                          contentItemRecord.Id);
                    session.CreateSQLQuery(transactionDelete)
                           .SetTimeout(300)
                           .ExecuteUpdate();

                    //if (contentItemRecord.Summary != null)
                    //{
                    //    string summaryDelete = string.Format("delete from {0} where Id ={1}",
                    //                                         typeof (ContentItemSummaryRecord).EntityTableName(),
                    //                                         contentItemRecord.Summary.Id);
                    //    session.CreateSQLQuery(summaryDelete)
                    //           .SetTimeout(300)
                    //           .ExecuteUpdate();
                    //}

                    session.Delete(contentItemRecord);

                    session.CreateSQLQuery(string.Format("TRUNCATE TABLE {0}", typeof(CGCAHPSSurveyTarget).EntityTableName()))
                           .SetTimeout(300)
                           .ExecuteUpdate();

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
    }
}
