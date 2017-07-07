using System;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Utility;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.MedicareProviderCharge.Models
{
   
    [Export, 
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardContext"/> class,it also initializes the defaults.
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
        const string RECORD_KEY = "{1DDA3E11-D5DB-4DF0-9BFB-625591C73167}";          // TODO: per content type
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

            //DeletePreviousImport();
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
                           .FirstOrDefault(c => c.ContentType.Name.ToUpper() == "MEDICARE PROVIDER CHARGE DATA");

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

                    session.CreateSQLQuery(string.Format("TRUNCATE TABLE {0}", typeof(MedicareProviderChargeTarget).EntityTableName()))
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
