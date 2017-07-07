using System;
using System.Collections.Generic;
using System.Data;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System.ComponentModel.Composition;

namespace Monahrq.Wing.Ahrq.Area
{
    //[Export(MigrationContractNames.Target, typeof(ITargetMigration))]
    /// <summary>
    /// Class of Migrations.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Extensibility.Data.Migration.DataMigrationImpl" />
    /// <seealso cref="Monahrq.Sdk.Extensibility.Data.Migration.ITargetMigration" />
    public class Migrations : DataMigrationImpl, ITargetMigration
    {
        /// <summary>
        /// Gets the target names.
        /// </summary>
        /// <value>
        /// The target names.
        /// </value>
        public IEnumerable<string> TargetNames
        {
            get
            {
                return new[]
                {
                    "AreaTarget"
                    , "CompositeTarget"
                    , "ProviderTarget"
                };
            }
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public override int Create()
        {
            //// Creating table Monahrq_Wing_Ahrq_Area_AreaTarget
            //SchemaBuilder.CreateTable("Monahrq_Wing_Ahrq_Area_AreaTarget", table => table
            //    .ContentPartVersionRecord()
            //    .Column("MeasureCode", DbType.String, column => column.WithLength(30))
            //    .Column("StratID", DbType.String, column => column.WithLength(12))
            //    .Column("ProviderID", DbType.String, column => column.WithLength(12))
            //    .Column("ObservedNumerator", DbType.Int32)
            //    .Column("ObservedDenominator", DbType.Int32)
            //    .Column("ObservedRate", DbType.Single)
            //    .Column("ObservedCIHigh", DbType.Single)
            //    .Column("ObservedCILow", DbType.Single)
            //    .Column("RiskAdjustedRate", DbType.Single)
            //    .Column("RiskAdjustedCIHigh", DbType.Single)
            //    .Column("RiskAdjustedCILow", DbType.Single)
            //    .Column("ExpectedRate", DbType.Single)
            //    .Column("StandardErr", DbType.Single)
            //    .Column("Threshold", DbType.Int32)
            //    .Column("NatBenchmarkRate", DbType.Single)
            //    .Column("NatRating", DbType.String, column => column.WithLength(30))
            //    .Column("PeerBenchmarkRate", DbType.Single)
            //    .Column("PeerRating", DbType.String, column => column.WithLength(20))
            //    .Column("TotalCost", DbType.Double)
            //);

            //// Creating table Monahrq_Wing_Ahrq_Composite_CompositeTarget
            //SchemaBuilder.CreateTable("Monahrq_Wing_Ahrq_Composite_CompositeTarget", table => table
            //    .ContentPartVersionRecord()
            //    .Column("MeasureCode", DbType.String, column => column.WithLength(30))
            //    .Column("StratID", DbType.String, column => column.WithLength(12))
            //    .Column("ProviderID", DbType.String, column => column.WithLength(12))
            //    .Column("ObservedNumerator", DbType.Int32)
            //    .Column("ObservedDenominator", DbType.Int32)
            //    .Column("ObservedRate", DbType.Single)
            //    .Column("ObservedCIHigh", DbType.Single)
            //    .Column("ObservedCILow", DbType.Single)
            //    .Column("RiskAdjustedRate", DbType.Single)
            //    .Column("RiskAdjustedCIHigh", DbType.Single)
            //    .Column("RiskAdjustedCILow", DbType.Single)
            //    .Column("ExpectedRate", DbType.Single)
            //    .Column("StandardErr", DbType.Single)
            //    .Column("Threshold", DbType.Int32)
            //    .Column("NatBenchmarkRate", DbType.Single)
            //    .Column("NatRating", DbType.String, column => column.WithLength(30))
            //    .Column("PeerBenchmarkRate", DbType.Single)
            //    .Column("PeerRating", DbType.String, column => column.WithLength(20))
            //    .Column("TotalCost", DbType.Double)
            //);

            //// Creating table Monahrq_Wing_Ahrq_Provider_ProviderTarget
            //SchemaBuilder.CreateTable("Monahrq_Wing_Ahrq_Provider_ProviderTarget", table => table
            //    .ContentPartVersionRecord()
            //    .Column("MeasureCode", DbType.String, column => column.WithLength(30))
            //    .Column("StratID", DbType.String, column => column.WithLength(12))
            //    .Column("ProviderID", DbType.String, column => column.WithLength(12))
            //    .Column("ObservedNumerator", DbType.Int32)
            //    .Column("ObservedDenominator", DbType.Int32)
            //    .Column("ObservedRate", DbType.Single)
            //    .Column("ObservedCIHigh", DbType.Single)
            //    .Column("ObservedCILow", DbType.Single)
            //    .Column("RiskAdjustedRate", DbType.Single)
            //    .Column("RiskAdjustedCIHigh", DbType.Single)
            //    .Column("RiskAdjustedCILow", DbType.Single)
            //    .Column("ExpectedRate", DbType.Single)
            //    .Column("StandardErr", DbType.Single)
            //    .Column("Threshold", DbType.Int32)
            //    .Column("NatBenchmarkRate", DbType.Single)
            //    .Column("NatRating", DbType.String, column => column.WithLength(30))
            //    .Column("PeerBenchmarkRate", DbType.Single)
            //    .Column("PeerRating", DbType.String, column => column.WithLength(20))
            //    .Column("TotalCost", DbType.Double)
            //);

            return 1;
        }
    }
}