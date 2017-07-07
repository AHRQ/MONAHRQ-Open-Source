using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Types;
using Monahrq.Sdk.Utilities;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate.Linq;
using PropertyChanged;
using MappingType = Monahrq.DataSets.Model.MappingType;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Context class for Wizards
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Model.DatasetContext" />
    [Export,
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContext //,IProvideViewType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardContext"/> class.
        /// </summary>
        public WizardContext()
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe((e) => Cancelled(this, EventArgs.Empty));

            if (this.DatasetItem == null) DatasetItem = new Dataset();
            this[RECORD_KEY] = DatasetItem;

            // InitCurrentImport();
        }

        /// <summary>
        /// Gets or sets the file progress.
        /// </summary>
        /// <value>
        /// The file progress.
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
        const string RECORD_KEY = "{8054374C-DC8F-45DE-AC5F-5084ED0CF0B9}";          // TODO: per content type
        /// <summary>
        /// Gets or sets the type of the current import.
        /// </summary>
        /// <value>
        /// The type of the current import.
        /// </value>
        internal Target CurrentImportType { get; set; }

        /// <summary>
        /// Event handler for cacelled action event.
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
        /// Gets or sets the name of the custom target.
        /// </summary>
        /// <value>
        /// The name of the custom target.
        /// </value>
        public string CustomTargetName { get; set; }
        /// <summary>
        /// Gets or sets the custom target.
        /// </summary>
        /// <value>
        /// The custom target.
        /// </value>
        public Target CustomTarget { get; set; }

        /// <summary>
        /// Gets the target columns.
        /// </summary>
        /// <value>
        /// The target columns.
        /// </value>
        public IList<DataColumn> TargetColumns { get; private set; }

        /// <summary>
        /// Gets or sets the type of the selected mapping.
        /// </summary>
        /// <value>
        /// The type of the selected mapping.
        /// </value>
        public MappingType SelectedMappingType { get; set; }
        //public int SelectedMappingDatasetId { get; set; }
        //public string SelectDataMappingFile { get; set; }
        public ObservableCollection<SelectListItem> ExistingsDatasets { get; set; }

        /// <summary>
        /// Initializes the context.
        /// </summary>
        public void InitContext()
        {
                CustomTarget = GetTargetByName(CustomTargetName.ToUpper());
                    TargetElements = CustomTarget.Elements.ToList();


            using (var session = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory.OpenSession())
            {
                if (CustomTarget != null)
                {
                    DataTable DynamicWingTargetDataTable = new DataTable(CustomTarget.DbSchemaName);
                    string sqlStatement = string.Format("SELECT TOP 0 * FROM [dbo].[{0}] ", CustomTarget.DbSchemaName);
                    using (SqlDataAdapter sqlDa = new SqlDataAdapter(sqlStatement, session.Connection as SqlConnection))
                    {
                        sqlDa.Fill(DynamicWingTargetDataTable);
                    }

                    if (DynamicWingTargetDataTable.Columns.Count > 0)
                    {
                        TargetColumns = DynamicWingTargetDataTable.Columns.OfType<DataColumn>().ToList();
                    }
                }

            }

            if (!CustomTarget.AllowMultipleImports)
                DeletePreviousImport();
        }

        /// <summary>
        /// Applies the summary record.
        /// </summary>
        protected override void ApplySummaryRecord(bool mappingOnly = false)
        {
            //var currentContentItem = DatasetItem ?? new Dataset();

            if (DatasetItem == null) return;
            
            if (!DatasetItem.Infoset.Element.IsEmpty)
                DatasetItem.Infoset.Element.RemoveNodes();

            //if (!string.IsNullOrEmpty(currentContentItem.SummaryData))
            //    currentContentItem.SummaryData = null;

            ProgressState progress = null;
            if (!mappingOnly && ValidationSummary != null)
            {
                progress = new ProgressState(ValidationSummary.CountValid, ValidationSummary.Total);
            }

            var element = new DatasetSummaryHelper(this, progress, RequiredMappings, OptionalMappings).AsElement;
            DatasetItem.Infoset.Element.Add(element);
        }

        /// <summary>
        /// Deletes the previous import.
        /// </summary>
        private void DeletePreviousImport()
        {
            if (CurrentImportType == null) return;

            using (var session = Provider.SessionFactory.OpenSession())
            {
                var contentItemRecord = session.Query<Dataset>()
                                               .FirstOrDefault(c => c.ContentType.Name.ToUpper() == CurrentImportType.Name.ToUpper());

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

                    //if (contentItemRecord.SummaryData != null)
                    //{
                    //    var summaryDelete = string.Format("delete from {0} where Id ={1}",
                    //                                         typeof (ContentItemSummaryRecord).EntityTableName(),
                    //                                         contentItemRecord.Summary.Id);
                    //    session.CreateSQLQuery(summaryDelete)
                    //           .SetTimeout(300)
                    //           .ExecuteUpdate();
                    //}

                    session.Delete(contentItemRecord);

                    session.CreateSQLQuery(string.Format("TRUNCATE TABLE {0}", this.CustomTarget.DbSchemaName))
                           .SetTimeout(300)
                           .ExecuteUpdate();

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current crosswalk.
        /// </summary>
        /// <value>
        /// The current crosswalk.
        /// </value>
        public new IEnumerable<ViewModels.MappedFieldEntryViewModel> CurrentCrosswalk
        {
            get
            {
                return this[CROSSWALK_KEY] as IEnumerable<ViewModels.MappedFieldEntryViewModel>;
            }
            set
            {
                this[CROSSWALK_KEY] = value;
                CrosswalkCache = ElementCrossWalkMap.CreateCrosswalkCache(value);
            }
        }

        /// <summary>
        /// Lazy initialization.
        /// </summary>
        protected override void InitLazyType()
        {
            // base.InitLazyType();
            var targetName = !string.IsNullOrEmpty(CustomTarget.DbSchemaName)
                            ? Inflector.Singularize(CustomTarget.DbSchemaName.Replace("Targets_", null))
                            : Inflector.Singularize(CustomTarget.Name.Replace(" ", null));

            var properties = TargetColumns.Where(col => !col.ColumnName.In(new[] { "Id", "Dataset_Id" })).Select(CreateField).ToList();

            var dynTargetBuilder = new DynamicTargetBuilder(targetName, typeof(DynamicDatasetRecord));
            var dynObject = dynTargetBuilder.CreateNewObject(properties);

            _lazyTargetType = dynTargetBuilder.ObjType;
        }

        /// <summary>
        /// Creates the field.
        /// </summary>
        /// <param name="col">The data column.</param>
        /// <returns></returns>
        private Field CreateField(DataColumn col)
        {
            var element = CustomTarget.Elements.FirstOrDefault(e => e.Name.EqualsIgnoreCase(col.ColumnName));

            var dataType = col.DataType;
            if (element != null && element.Scope != null)
            {
                if (!string.IsNullOrEmpty(element.Scope.ClrType))
                {
                    var colType = Type.GetType(element.Scope.ClrType);
                    dataType = col.AllowDBNull 
                                    ? typeof (Nullable<>).MakeGenericType(colType) 
                                    : colType;
                }
            }

            return new Field
                        {
                            FieldName = col.ColumnName,
                            FieldType = dataType,
                            IsRequired = !col.AllowDBNull
                        };
        }
    }
}
