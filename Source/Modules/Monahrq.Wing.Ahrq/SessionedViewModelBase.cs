using System.Linq;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Ahrq.Common;
using NHibernate;
using System;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Microsoft.Practices.ServiceLocation;
using System.Threading;
using Microsoft.Practices.Prism.Events;
using NHibernate.Linq;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// View model base.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{TContext}" />
    public abstract class SessionedViewModelBase<TContext> : WizardStepViewModelBase<TContext>
        where TContext : DataSets.Model.DatasetContextBase
    {
        /// <summary>
        /// The session
        /// </summary>
        protected ISession Session;

        /// <summary>
        /// Gets or sets the CTS.
        /// </summary>
        /// <value>
        /// The CTS.
        /// </value>
        protected CancellationTokenSource Cts { get; set; }
        CountdownEvent _importCompletedCountdownEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionedViewModelBase{TContext}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected SessionedViewModelBase(TContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Starts the import.
        /// </summary>
        public virtual void StartImport()
        {
            //_importCompletedCountdownEvent = new CountdownEvent(1);
            Cts = new CancellationTokenSource();
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));

            //Session = DataContextObject.ShellContext.SessionFactoryHolder.GetSessionFactory().OpenSession();
            Session = OpenSession();
        }

        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession()
        {
            return DataContextObject.Provider.SessionFactory.OpenSession();
        }

        /// <summary>
        /// Cancelleds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Cancelled(object sender, EventArgs e)
        {
            // don't use cancellation token if user cancelled too late
            //if (_importCompletedCountdownEvent.CurrentCount > 0)

            if (Cts.IsCancellationRequested)
            {
                if (DataContextObject.DatasetItem != null && DataContextObject.DatasetItem.IsFinished)
                {
                    DataContextObject.DatasetItem.IsFinished = false;
                    DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);
                }

                Cts.Cancel();
            }
        }

        // overloads to handle each type of target, and the ImportType, and ImportTypeRecord...
        /// <summary>
        /// Save or update the dataset record in the session.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void Insert(DatasetRecord entity)
        {
            if (!Session.IsOpen) Session = OpenSession();
            Session.SaveOrUpdate(entity);
        }

        /// <summary>
        /// Save or update the dataset record in the session.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void SaveImportEntry(Dataset entity)
        {
            if (!Session.IsOpen) Session = OpenSession();
            Session.SaveOrUpdate(entity);
        }

        /// <summary>
        /// To get the counry matching the measure code , id and datasetRecord id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="measureCode">The measure code.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="datasetRecord">The dataset record.</param>
        /// <returns></returns>
        protected bool AnyCounty<T>(string measureCode, string id, int datasetRecord) where T : AhrqTarget
        {
            if (!Session.IsOpen) Session = OpenSession();
            return Session.Query<T>().Any(a => a.MeasureCode.ToLower() == measureCode.ToLower() &&
                                                a.CountyFIPS.ToLower() == id && a.Dataset.Id == datasetRecord);
        }


        /// <summary>
        /// To get the counry matching the measure code , id and datasetRecord id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="measureCode">The measure code.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="datasetRecord">The dataset record.</param>
        /// <returns></returns>
        protected bool AnyHospital<T>(string measureCode, string id, int datasetRecord) where T : AhrqTarget
        {
            if (!Session.IsOpen) Session = OpenSession();
            return Session.Query<T>().Any(a => a.MeasureCode == measureCode &&
                                                a.LocalHospitalID == id &&
                                                a.Dataset.Id == datasetRecord);
        }
        //protected void SaveImportTypeRecord(ContentTypeRecord entity)
        //{
        //    if (!_session.IsOpen) _session = OpenSession();
        //    _session.SaveOrUpdate(entity);
        //}

        /// <summary>
        /// Action to perform after import is completed.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        protected virtual void ImportCompleted(bool success)
        {
            // signal import complete so cancellation token won't be used if user cancelled too late
            // _importCompletedCountdownEvent.Signal();

            try
            {
                if (success)
                {
                    DataContextObject.Finish();
                }
                if (Session.IsOpen)
                {
                    Session.Close();
                }
            }
            finally
            {
                Session.Dispose();
            }
        }
    }
}
