using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Core;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Extensibility.Data;
using Monahrq.Sdk.Modules.Wings;
using NHibernate;
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Entities.Core;

namespace Monahrq.Wing.BaseData
{
    public abstract class BaseDataModule<T> : WingModule, IDataLoader
        where T : ContentPartRecord
    {

        protected ILogWriter Logger { get; private set; }
        protected bool FirstImport { get; private set; }

        public BaseDataModule()
        {
            Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            FirstImport = false;
        }

        public void LoadData()
        {
            if (FirstImport)
            {
                ImportData();
                AfterImport();
            }
        }

        protected virtual void AfterImport()
        {
        }

        private DataTable CreateSourceDataTable()
        {
            DataTable result = new DataTable();
            try
            {
                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
                builder.Provider = "Microsoft.ACE.OLEDB.12.0";
                builder.DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BaseData\\");
                builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";
                string query = "SELECT * FROM [" + BaseDataFileName + "]";
                //create an OleDbDataAdapter to execute the query
                using (OleDbDataAdapter dAdapter = new OleDbDataAdapter(query, builder.ConnectionString))
                {
                    dAdapter.Fill(result);
                    dAdapter.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            return result;
        }

        private IStatelessSession CreateSession()
        {
            return ServiceLocator.Current
                    .GetInstance<IMonahrqShell>()
                    .SessionFactoryHolder
                    .GetSessionFactory()
                    .OpenStatelessSession();
        }

        private void ImportData()
        {
            try
            {
                using (var dt = CreateSourceDataTable())
                {

                    using (var session = CreateSession())
                    {
                        using (var bulkInsert = new BulkInsert<T, int>(session.Connection))
                        {
                            foreach (DataRow dr in dt.Rows.OfType<DataRow>())
                            {
                                var target = DataRowToTarget(dr);
                                bulkInsert.Insert(target);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        protected abstract T DataRowToTarget(DataRow dr);

        protected abstract string BaseDataFileName { get; }

        protected override void OnContentTypeAdded(ContentTypeRecord contentType)
        {
            base.OnContentTypeAdded(contentType);
            FirstImport = true;
        }

        public abstract ReaderDefinition Reader  { get; } 
        
    }
}
