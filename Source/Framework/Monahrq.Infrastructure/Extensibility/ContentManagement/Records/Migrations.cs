using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Sdk.Utilities;


namespace Monahrq.Sdk.Extensibility.ContentManagement.Records
{

    [Export(MigrationContractNames.Target, typeof(ITargetMigration))]
    public class Migrations : DataMigrationImpl, ITargetMigration
    {
        public IEnumerable<string> TargetNames
        {
            get
            {

                 return new string[]
                {
                    Inflector.Pluralize(typeof(Dataset).Name)
					//, Inflector.Pluralize(typeof(ContentItemSummaryRecord).Name)
					, Inflector.Pluralize(typeof(DatasetVersionRecord).Name)
					, Inflector.Pluralize(typeof(DatasetTransactionRecord).Name)
					//, Inflector.Pluralize(typeof(ContentTypeRecord).Name)
                };
            }
        }

        public override int Create()
        {
            //// Creating table Monahrq_Sdk_Extensibility_Dataset_Records_ContentItemRecord
            //#region  SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentItemRecord).Name))
            //SchemaBuilder.CreateTable(typeof(Dataset).FullName.Replace(".", "_"), table => table
            //    .ContentPartRecord()
            //    .Column("Data", DbType.String, column => column.Unlimited())
            //    .Column("File", DbType.String)
            //    .Column("Description", DbType.String)
            //    .Column("IsFinished", DbType.Boolean)
            //    .Column("ContentType_id", DbType.Int32)
            //    .Column("Summary_id", DbType.Int32)
            //);
            //#endregion  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentItemRecord")

            //// Creating table Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentItemSummaryRecord
            //#region  SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentItemSummaryRecord).Name))
            //SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentItemSummaryRecord).Name), table => table
            //    .ContentPartVersionRecord()
            //    .Column("Data", DbType.String, column => column.Unlimited())
            //);
            //#endregion  SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentItemSummaryRecord).Name)

            //// Creating table Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentItemVersionRecord
            //#region  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentItemVersionRecord")
            //SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentItemVersionRecord).Name), table => table
            //    .ContentPartVersionRecord()
            //    .Column("Number", DbType.Int32)
            //    .Column("Published", DbType.Boolean)
            //    .Column("Latest", DbType.Boolean)
            //    .Column("Data", DbType.String, column => column.Unlimited())
            //);
            //#endregion  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentItemVersionRecord")

            //// Creating table Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentPartTransactionRecord
            //#region  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentPartTransactionRecord")
            //SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentPartTransactionRecord).Name), table => table
            //    .ContentPartVersionRecord()
            //    .Column("Code", DbType.Int32)
            //    .Column("Message", DbType.String)
            //    .Column("Extension", DbType.Int32)
            //    .Column("Data", DbType.String, column => column.Unlimited())
            //);
            //#endregion  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentPartTransactionRecord")

            //// Creating table Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentTypeRecord
            //#region  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentTypeRecord")
            //SchemaBuilder.CreateTable(Inflector.Pluralize(typeof(ContentTypeRecord).Name), table => table
            //    .ContentPartRecord()
            //    .Column("Name", DbType.String)
            //);
            //#endregion  SchemaBuilder.CreateTable("Monahrq_Sdk_Extensibility_ContentManagement_Records_ContentTypeRecord")



            return 1;
        }
    }
}