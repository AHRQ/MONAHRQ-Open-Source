using System;
using System.Collections.Generic;
using System.Data;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System.ComponentModel.Composition;

namespace Monahrq.Wing.NursingHomeCompare
{
    //[Export(MigrationContractNames.Target, typeof(ITargetMigration))]
    public partial class Migrations : DataMigrationImpl, ITargetMigration
    {
        public IEnumerable<string> TargetNames
        {
            get
            {
                return new string[] 
                {
                    "NursingHomeTarget"
                };
            }
        }

        public override int Create()
        {
            //// Creating table Monahrq_Wing_HospitalCompare_HospitalCompareTarget
            //SchemaBuilder.CreateTable("Targets_HospitalCompareTargets", table => table
            //    .ContentPartVersionRecord()
            //    .Column("CMSProviderID", DbType.String, column => column.WithLength(12))
            //    .Column("ConditionCode", DbType.Int32)
            //    .Column("MeasureCode", DbType.String, column => column.WithLength(25))
            //    .Column("CategoryCode", DbType.Int32)
            //    .Column("Rate", DbType.Single)
            //    .Column("Sample", DbType.Int32)
            //    .Column("Lower", DbType.Single)
            //    .Column("Upper", DbType.Single)
            //    .Column("Note", DbType.String, column => column.WithLength(5))
            //    .Column("BenchmarkID", DbType.String, column => column.WithLength(15))
            //    //.Column("Provider_id", DbType.Int32)
            //    .Column("Footnote_id", DbType.Int32)
            //);

            //// Creating table Monahrq_Wing_HospitalCompare_HospitalCompareHospital
            //SchemaBuilder.CreateTable("Monahrq_Wing_HospitalCompare_HospitalCompareHospital", table => table
            //    .ContentPartVersionRecord()
            //    .Column("ProviderNumber", DbType.String, column => column.WithLength(12))
            //    .Column("Name", DbType.String, column => column.WithLength(80))
            //    .Column("City", DbType.String, column => column.WithLength(35))
            //    .Column("State", DbType.String, column => column.WithLength(2))
            //    .Column("Zip", DbType.String, column => column.WithLength(5))
            //    .Column("County", DbType.String, column => column.WithLength(35))
            //    .Column("FIPS", DbType.String, column => column.WithLength(5))
            //);

            //// Creating table Monahrq_Wing_HospitalCompare_HospitalCompareFootnote
            //SchemaBuilder.CreateTable("Monahrq_Wing_HospitalCompare_HospitalCompareFootnote", table => table
            //    .ContentPartVersionRecord()
            //    .Column("Name", DbType.String, column => column.WithLength(12))
            //    .Column("Description", DbType.String, column => column.Unlimited())
            //);

            return 1;
        }
    }
}