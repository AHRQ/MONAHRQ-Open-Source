using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Utility.Extensions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Wing.ReportingEntities
{
    /// <summary>
    /// Model class for Reporting for the website.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.WingModule" />
    public abstract class ReportingEntityModule  : WingModule
    { 
        //[Import]
        //protected IEventAggregator Events {get; private set;}
        //IEnumerable<IDataReaderDictionary> DataProviders { get; set; }
        IDomainSessionFactoryProvider FactoryProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingEntityModule"/> class and initializes other private members.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="factoryProvider">The factory provider.</param>
        /// <param name="dataLoaders">The data loaders.</param>
        protected ReportingEntityModule(
                ILogWriter logger,
                IDomainSessionFactoryProvider factoryProvider,
                IEnumerable<IDataLoader> dataLoaders)
        {
            FactoryProvider = factoryProvider;
            DataLoaders = dataLoaders;
            Logger = logger;
        }

        ILogWriter Logger { get; set; }
    
        IEnumerable<IDataLoader> DataLoaders { get; set; }

        /// <summary>
        /// To run the data loader and get the data.
        /// </summary>
        private void RunDataLoaders()
        {
            //DateTime start = DateTime.Now;
            foreach (var loader in DataLoaders)
            {
                if (loader.IsBackground)
                {
                    loader.OnFeedback += (o, e) =>
                          Events.GetEvent<UiMessageUpdateEventForeGround>()
                                  .Publish(
                                      new UiMessageUpdateEventForeGround
                                      {
                                          Message = e.Data
                                      });
                }
                else
                {
                    loader.OnFeedback += (o, e) =>
                            Events.GetEvent<MessageUpdateEvent>()
                                    .Publish(
                                        new MessageUpdateEvent
                                            {
                                                Message = e.Data
                                            });
                }
                Application.Current.DoEvents();
                loader.LoadData();
            }
            Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Empty });
            Application.Current.DoEvents();
            //DateTime end = DateTime.Now;
            //MessageBox.Show("Start: " + start.ToString() + " End: " + end.ToString());
        }

        //private void OnPublishStatus(string statusMessage, bool isBackground = false)
        //{}

        /// <summary>
        /// Over rides the parent class method and installs the database with required based data.
        /// </summary>
        protected override void OnWingAdded()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            base.OnWingAdded();

            InstallDb();
            //RunDataLoaders();
            //DataLoaders.ForEach(dl => dl.DataProvider.Programmabilities.ForEach(p => p.Apply()));
            Logger.Information("Data loaded");
        }

        /// <summary>
        /// Called when [content type added].
        /// </summary>
        protected override void OnContentTypeAdded(/*Infrastructure.Data.Extensibility.ContentManagement.Records.ContentTypeRecord contentType*/)
        {
            //base.OnContentTypeAdded(contentType);
        }


        /// <summary>
        /// Determines whether the reporting is installed or not.
        /// </summary>
        /// <returns></returns>
        public override bool Install()
        {
            return true;
        }

        /// <summary>
        /// Call the RunDataLoaders.
        /// </summary>
        /// <returns></returns>
        public override bool InstallDb()
        {
            try
            {
                RunDataLoaders();
                DataLoaders.ForEach(dl => dl.DataProvider.Programmabilities.ForEach(p => p.Apply()));

                return true;
            }
            catch (Exception ex)
            {
                SessionLogger.Log(ex.Message, Category.Exception, Priority.High);
                OperationsLogger.Log(ex.Message, Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Determines whether Update has occurred or not.
        /// </summary>
        /// <returns></returns>
        public override bool Update()
        {
            return true;
        }

        /// <summary>
        /// Determines whether database has been updated.
        /// </summary>
        /// <returns></returns>
        public override bool UpdateDb()
        {
            return true;
        }
    }

    /// <summary>
    /// Class for reporting loader export attribute.
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.ExportAttribute" />
    public abstract class ReportingEntityLoaderExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingEntityLoaderExportAttribute"/> class and initializes other private members.
        /// </summary>
        /// <param name="contract">The contract.</param>
        protected ReportingEntityLoaderExportAttribute(string contract)
            : base(contract, typeof(IDataLoader))
        {

        }
    }

}
