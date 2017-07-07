using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FluentNHibernate.Cfg.Db;
using System;
using FluentNHibernate.Cfg;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility.Extensions;
using cfg = NHibernate.Cfg.Configuration;
using System.Data.SqlClient;
using MessageBox = System.Windows.MessageBox;

namespace Monahrq.Infrastructure.Data
{
    [Serializable]
    public abstract class AbstractDataServicesProvider<T> : IDataServicesProvider
    {
        [Import]
        public IConfigurationService ConfigService { get; set; }

        public abstract IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);

        public cfg BuildConfiguration()
        {
            if (MonahrqNHibernateProvider.Configuration == null)
            {
                MonahrqNHibernateProvider.BuildConfiguration();
            }

            return MonahrqNHibernateProvider.Configuration;
        }

        protected virtual void MappingStrategy(MappingConfiguration config)
        {

        }

        #region Old Code
        //[Serializable]
        //class OrchardLoadEventListener : DefaultLoadEventListener, ILoadEventListener
        //{

        //    public new void OnLoad(LoadEvent @event, LoadType loadType)
        //    {
        //        var source = (ISessionImplementor)@event.Session;
        //        IEntityPersister entityPersister;
        //        if (@event.InstanceToLoad != null)
        //        {
        //            entityPersister = source.GetEntityPersister(null, @event.InstanceToLoad);
        //            @event.EntityClassName = @event.InstanceToLoad.GetType().FullName;
        //        }
        //        else
        //            entityPersister = source.Factory.GetEntityPersister(@event.EntityClassName);
        //        if (entityPersister == null)
        //            throw new HibernateException("Unable to locate persister: " + @event.EntityClassName);

        //        var keyToLoad = new EntityKey(@event.EntityId, entityPersister, source.EntityMode);

        //        if (loadType.IsNakedEntityReturned)
        //        {
        //            @event.Result = Load(@event, entityPersister, keyToLoad, loadType);
        //        }
        //        else if (@event.LockMode == LockMode.None)
        //        {
        //            @event.Result = ProxyOrLoad(@event, entityPersister, keyToLoad, loadType);
        //        }
        //        else
        //        {
        //            @event.Result = LockAndLoad(@event, entityPersister, keyToLoad, loadType, source);
        //        }
        //    }
        //}
        #endregion

        public string GetDBSchemaVersion(SqlConnection con)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            const string query = @"
           DECLARE @sql VARCHAR(100);
           IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'SchemaVersions')
           BEGIN
                CREATE TABLE SchemaVersions( [Id] [int] IDENTITY(1,1) NOT NULL, [Name] NVARCHAR(255) NOT NULL, [Version] NVARCHAR(50) NOT NULL, ActiveDate DATETIME NOT NULL DEFAULT (GETDATE()) );
            
                set @sql='INSERT INTO SchemaVersions([Name],[Version]) VALUES (''Database Schema'', ''{0}'')';
                EXEC (@sql)
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='NAME' AND c.[object_id]=OBJECT_ID('SchemaVersions'))
            BEGIN
            ALTER TABLE SchemaVersions add NAME VARCHAR(100)
            END;";
            const string querySelect = @"IF EXISTS(SELECT 1 FROM SchemaVersions WHERE Version LIKE '5.0%' AND NAME IS NULL)          
            UPDATE SchemaVersions SET NAME = 'Database Schema' WHERE Version LIKE '5.0%' AND NAME IS NULL;    
            SELECT TOP 1 [Version] FROM SchemaVersions where [Name] = 'Database Schema' ORDER BY ActiveDate DESC; ";

            using (var sqlcommand = new SqlCommand(string.Format(query, configService.CurrentSchemaVersion), con))
            {
                sqlcommand.ExecuteNonQuery();
            }
            using (var sqlcommand = new SqlCommand(string.Format(querySelect, configService.CurrentSchemaVersion), con))
            {
                var schemaVersion = (string)sqlcommand.ExecuteScalar();
                return schemaVersion;
            }
        }


        public bool UpgradeDatabase(SqlConnection con)
        {
            if (con == null)
                throw new ArgumentNullException("con");
            using (con)
            {
                con.Open();
                var schemaVersion = GetDBSchemaVersion(con);
                var actual = new Version(schemaVersion);
                var expected = new Version(ConfigService.CurrentSchemaVersion);

                if (expected > actual)
                {
                    var msg =
                        string.Format(
                            "Your current database “{0}” needs to be upgraded. Please click “Yes” to upgrade now or “No” to create a new database via the database connection manager.",
                            con.Database);

                    var msgResult = MessageBox.Show(msg, "MONAHRQ Database Update Required!",
                        MessageBoxButton.YesNo);

                    if (msgResult == MessageBoxResult.Yes)
                    {
                        MonahrqContext.ForceDbUpGrade = true;
                        MonahrqContext.DbNameToUpGrade = con.Database;
                        MonahrqContext.IsMajorDbUpgrade = expected.CheckForUpdate(actual) == UpgradeTypeEnum.Major;
                    }
                    else
                    {
                        MonahrqContext.ForceDbRecreate = true;
                    }

                    return true;
                }

                if (expected < actual)
                {
                    var msg =
                        string.Format(
                            "The Database version of {0} is higher than the expected version {1} for this release. Please make sure you have the correct version of MONAHRQ installed.",
                            schemaVersion, ConfigService.CurrentSchemaVersion);
                    throw new Exception(msg);
                }
            }
            return false;
        }

    }
}
