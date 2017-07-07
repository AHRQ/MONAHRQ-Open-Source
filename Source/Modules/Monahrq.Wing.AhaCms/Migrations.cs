using System;
using System.Collections.Generic;
using System.Data;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System.ComponentModel.Composition;

namespace Monahrq.Wing.AhaCms
{
    [Export(MigrationContractNames.Target, typeof(ITargetMigration))]
    public partial class Migrations : DataMigrationImpl, ITargetMigration
    {
        public IEnumerable<string> TargetNames
        {
            get
            {
                return new string[] 
                {
                   "Monahrq_Wing_AhaCms_Target"
                };
            }
        }

        public override int Create()
        {
            //// Creating table Monahrq_Wing_AhaCms_Target
            //SchemaBuilder.CreateTable("Monahrq_Wing_AhaCms_Target", table => table
            //    .ContentPartVersionRecord()
            //    .Column("Name", DbType.String)
            //    .Column("Address", DbType.String)
            //    .Column("Telephone", DbType.String)
            //    .Column("HospitalWebsite", DbType.String)
            //    .Column("CMSCertificationNumber", DbType.String)
            //    .Column("TypeOfFacility", DbType.String)
            //    .Column("TypeOfControl", DbType.String)
            //    .Column("TotalStaffedBeds", DbType.Int32)
            //    .Column("TotalPatientRevenue", DbType.Int32)
            //    .Column("TotalDischarges", DbType.Int32)
            //    .Column("TotalPatientDays", DbType.Int32)
            //);

            return 1;
        }
    }
}