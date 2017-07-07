using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Utility;
using NHibernate.Criterion;

namespace Monahrq.Infrastructure.Services.SysTray
{
    /// <summary>
    /// The dataset systray application processor. Handles execution of MS-DRG systray application.
    /// </summary>
    public static class DatasetSysTrayProcessor
    {
        /// <summary>
        /// Processes the dataset.
        /// </summary>
        /// <param name="dataset">The dataset.</param>
        public static void ProcessDataset(Dataset dataset)
        {
            var sessionProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();

            var logger = ServiceLocator.Current.GetInstance<ILoggerFacade>(LogNames.Session);

            bool shouldExecute;

            string connectionStringToUse;
            using (var session = sessionProvider.SessionFactory.OpenStatelessSession())
            {
                var targetType = session.CreateCriteria<Target>()
                                        .Add(Restrictions.Eq("Name", dataset.ContentType.Name))
                                        .SetProjection(Projections.Property("ClrType"))
                                        .UniqueResult<string>();

                var query = string.Format("select Count([Id]) from {0} with(nolock) where [Dataset_Id] = {1} and [DRG] is null or [MDC] is null;",
                                          Type.GetType(targetType).EntityTableName(), dataset.Id);

                connectionStringToUse = session.Connection.ConnectionString;
                
                shouldExecute = session.CreateSQLQuery(query).UniqueResult<int>() > 0;
            }

            if (shouldExecute)
            {
                logger.Log("Starting Grouper SysTray App.", Category.Info, Priority.High);

                var workerThread = new Thread(DoWork) {IsBackground = true};
                workerThread.Start(new WorkerParms
                    {
                        Dataset = dataset,
                        ConfigurationString = connectionStringToUse,
                        Logger = logger
                    });

            }
            else
            {
                dataset.DRGMDCMappingStatus = DrgMdcMappingStatusEnum.Completed;

                using (var session = sessionProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(dataset);
                        //session.Flush();

                        trans.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Does the work.
        /// </summary>
        /// <param name="o">The o.</param>
        public static async void DoWork(object o)
        {
            WorkerParms objParams = o as WorkerParms;

            if (objParams == null)
                return;

            //while (!_shouldStop)
            //{
            var logger = objParams.Logger;
            var logFilePath = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Logs");

            try
            {
            if (!Directory.Exists(logFilePath))
                Directory.CreateDirectory(logFilePath);

            var connectionString = objParams.ConfigurationString;

            if (!string.IsNullOrEmpty(connectionString))
            {
                var variableStrings = string.Format("{0}|{1}|{2}", objParams.Dataset.Id, connectionString, logFilePath);

                logger.Log(string.Format("Grouper SysTray App Command Line Varables: {0}", variableStrings), Category.Info, Priority.High);

                //await Task.Run(() =>
                //{
                using (var sysTrayProc = new Process())
                {
                    sysTrayProc.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    sysTrayProc.StartInfo.FileName = "Monahrq.SysTray.exe";
                    sysTrayProc.StartInfo.Arguments = "\"" + variableStrings + "\"";
                    sysTrayProc.StartInfo.UseShellExecute = true;
                    sysTrayProc.StartInfo.CreateNoWindow = false;
                    sysTrayProc.Start();

                    sysTrayProc.WaitForExit(int.MaxValue);

                    #region Don't remove as if you want to capture responses from sys try application, then uncomment out the code.
                    //do
                    //{
                    //    var message = sysTrayProc.StandardOutput.ReadLineAsync().Result; 
                    //    logger.Log(message, Category.Info, Priority.High);
                    //} 
                    //while (!sysTrayProc.HasExited);
                    #endregion

                    sysTrayProc.Close();
                }
                //});
            }
            else
            {
                logger.Log(string.Format("Grouper SysTray App could not be ran for Dataset, {0} (id:{1}), due to connectionstring being null.", objParams.Dataset.File, objParams.Dataset.Id), Category.Exception, Priority.High);
            }

                //logger.Log(string.Format("Grouper SysTray App Finished Processing for Dataset: {0} (id:{1})", objParams.Dataset.File, objParams.Dataset.Id), Category.Info, Priority.High);
            }
            catch (Exception exc)
            {
                var excToUse = exc.GetBaseException();

                if (exc is AggregateException)
                {
                    excToUse = ((AggregateException) exc).Flatten().GetBaseException();
                }

                if (logger != null)
                {
                    logger.Log(excToUse.GetBaseException().Message, Category.Exception, Priority.High);
                }
            }
            //    RequestStop();
            //}
        }
        /// <summary>
        /// Requests the stop.
        /// </summary>
        public static void RequestStop()
        {
            _shouldStop = true;
        }
        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        /// <summary>
        /// The should stop
        /// </summary>
        private static volatile bool _shouldStop;
    }

    /// <summary>
    /// The background worker parameters.
    /// </summary>
    internal class WorkerParms
    {
        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public Dataset Dataset { get; set; }
        /// <summary>
        /// Gets or sets the configuration string.
        /// </summary>
        /// <value>
        /// The configuration string.
        /// </value>
        public string ConfigurationString { get; set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILoggerFacade Logger { get; set; }
    }
}
