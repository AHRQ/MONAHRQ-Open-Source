using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Wing.BaseData.TopicBaseData;
using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using System.Windows;

namespace Monahrq.Wing.BaseData
{
    static class Constants
    {
        public const string WingGuid = "47622C20-88AB-4761-963F-AD033979B4E9";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "Base Data", "Loads the base data", DependsOnModuleNames = new string[] { "Configuration Management" })]
    public partial class Module : WingModule
    {
        [Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        [ImportMany]
        IEnumerable<IDataLoader> DataLoaders { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            RunDataLoaders();
        }

        private void RunDataLoaders()
        {
            foreach (var loader in DataLoaders)
            {
                Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent() { Message = "Loading: " + loader.Reader.Description });
                Application.Current.DoEvents();
                loader.LoadData();
            }
            Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent() { Message = string.Empty });
            Application.Current.DoEvents();
        }

        protected override void OnWingAdded()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            base.OnWingAdded();

            // Setup sprocs.
            SetupSprocs();

            // Import the base topics.
            TopicImport topicsBase = new TopicImport();
            topicsBase.Import();
            Logger.Information("Topics Base Data loaded.");
        }

        private void SetupSprocs()
        {
            AddSprocForAge();
            AddSprocForStratification();
        }

        private void AddSprocForAge()
        {
            try
            {
                var settings = ServiceLocator.Current.GetInstance<IConfigurationService>().ConnectionSettings;
                settings.ExecuteNonQuery(
                    new string[]
                        {
                            "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetAge]') AND type in (N'P', N'PC')) "
                            ,"DROP PROCEDURE [dbo].[spGetAge]"
                        });

                settings.ExecuteNonQuery(
                    new string[]
                        {
                            "CREATE PROCEDURE [dbo].[spGetAge] AS"
                            ,"BEGIN"
                            ,"	SELECT 1 AS ID, '<18' AS Name"
                            ,"	UNION"
                            ,"	SELECT 2, '18-44'"
                            ,"	UNION"
                            ,"	SELECT 3, '45-64'"
                            ,"	UNION"
                            ,"	SELECT 4, '65+'"
                            ,"END"
                        });
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private void AddSprocForStratification()
        {
            try
            {
                var settings = ServiceLocator.Current.GetInstance<IConfigurationService>().ConnectionSettings;
                settings.ExecuteNonQuery(
                    new string[]
                        {
                            "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetStratification]') AND type in (N'P', N'PC')) "
                            ,"DROP PROCEDURE [dbo].[spGetStratification]"
                        });

                settings.ExecuteNonQuery(
                    new string[]
                        {
                            "CREATE PROCEDURE [dbo].[spGetStratification] AS"
                            ,"BEGIN"
                            ,"	SELECT 0 AS ID, 'Total' AS Name, 'Total' AS Caption"
                            ,"	UNION"
                            ,"	SELECT 1, 'Age', 'Age Group'"
                            ,"	UNION"
                            ,"	SELECT 2, 'Sex', 'Gender'"
                            ,"	UNION"
                            ,"	SELECT 3, 'Payer', 'Payer'"
                            ,"	UNION"
                            ,"	SELECT 4, 'Race', 'Race/Ethnicity'"
                            ,"END"
                        });
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
