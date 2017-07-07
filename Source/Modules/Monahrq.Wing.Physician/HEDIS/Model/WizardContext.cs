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

namespace Monahrq.Wing.Physician.HEDIS.Model
{
    [Export, 
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContext
    {
        public WizardContext()
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));
            
            InitCurrentImport();
        }

        public FileProgress File { get; set; }
        public string Summary { get; set; }
        const string RECORD_KEY = "{E79C02F4-8ADB-421D-B28C-176ED2723114}";          // TODO: per content type
        internal Target CurrentImportType { get; set; }

        public event EventHandler Cancelled = delegate { };
        public string FileName { get; set; }

        private void InitCurrentImport()
        {
            if(DatasetItem == null) DatasetItem = new Dataset();
            this[RECORD_KEY] = DatasetItem;

            //DeletePreviousImport();
        }

        protected override void ApplySummaryRecord(bool mappingOnly = false)
        {
            //    var currentContentItem = CurrentContentItem ?? new Dataset();
            //    var summary = new ContentItemSummaryRecord {Data = Summary};
            //    currentContentItem.Summary = summary;
        }


        private void DeletePreviousImport()
        {
            using (
                var session =
                    ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory.OpenSession())
            {
                var contentItemRecord =
                    session.Query<Dataset>()
                           .FirstOrDefault(c => c.ContentType.Name.ToUpper() == "Medical Practice HEDIS Measures Data".ToUpper());

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

                    session.CreateSQLQuery(string.Format("TRUNCATE TABLE {0}", typeof(HEDISTarget).EntityTableName()))
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
