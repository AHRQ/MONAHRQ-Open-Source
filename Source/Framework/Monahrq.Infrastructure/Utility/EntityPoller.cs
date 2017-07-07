using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.Utility
{
    /// <summary>
    /// The EnityPoller class that polles the database of a particular entity at a set intervals, retry attempts.
    /// </summary>
    /// <typeparam name="T">A class that implements the IEntity Interface.</typeparam>
    /// <example>
    /// var poller = new EntityPoller&lt;Dataset&gt;(o =&gt; o.Id == Id);
    /// poller.ActiveCycle = 200; //milliseconds
    /// poller.IdleCycle = 10000; //10 seconds
    /// poller.RetryAttempts = 5;
    /// poller.RetryInitialInterval = 10000; //10 seconds
    /// poller.EntityReceived += OnNotification;
    /// poller.Error += OnError;
    /// poller.Start();
    ///   </example>
    public class EntityPoller<T>
        where T : class, IEntity
    {
        #region Fields
        private readonly Func<T, bool> _query;
        private readonly object _sync = new object();
        private bool _polling;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPoller{T}"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public EntityPoller(Func<T, bool> query)
        {
            _query = query;

            IdleCycle = 10000;
            ActiveCycle = 1000;
            RetryAttempts = 5;
            RetryInitialInterval = 10000;

            DataProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            Logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
        }

        #region Injected Services
        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IDomainSessionFactoryProvider DataProvider { get; set; }

        //[Import(LogNames.Session)]
        protected ILoggerFacade Logger { get; set; }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the idle cycle.
        /// </summary>
        /// <value>
        /// The idle cycle.
        /// </value>
        public int IdleCycle { get; set; }
        /// <summary>
        /// Gets or sets the active cycle.
        /// </summary>
        /// <value>
        /// The active cycle.
        /// </value>
        public int ActiveCycle { get; set; }
        /// <summary>
        /// Gets or sets the retry attempts.
        /// </summary>
        /// <value>
        /// The retry attempts.
        /// </value>
        public int RetryAttempts { get; set; }
        /// <summary>
        /// Gets or sets the retry initial interval.
        /// </summary>
        /// <value>
        /// The retry initial interval.
        /// </value>
        public int RetryInitialInterval { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Polls this EnityPoller.
        /// </summary>
        /// <returns></returns>
        private T Poll()
        {
            T entity = null;

            try
            {
                using (var session = DataProvider.SessionFactory.OpenSession())
                {
                    while (true)
                    {
                        entity = Retry(() => session.Query<T>().FirstOrDefault(_query), RetryAttempts, RetryInitialInterval);
                        if (entity != null) break;

                        Thread.Sleep(IdleCycle);
                    }
                }

                Thread.Sleep(ActiveCycle);

            }
            catch (Exception ex)
            {
                Stop();

                if (Error != null)
                    Error.Invoke(ex);
            }

            return entity;
        }

        /// <summary>
        /// Starts this EnityPoller.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Already polling</exception>
        public void Start()
        {
            var worker = new AsyncWorker(Poll);
            var completed = new AsyncCallback(PollComplete);
            var onCompleted = new AsyncComplete(OnEntity);

            lock (_sync)
            {
                if (_polling)
                    throw new InvalidOperationException("Already polling");

                var operation = AsyncOperationManager.CreateOperation(onCompleted);
                worker.BeginInvoke(completed, operation);
                _polling = true;
            }
        }

        /// <summary>
        /// Stops this EnityPoller.
        /// </summary>
        public void Stop()
        {
            lock (_sync)
            {
                _polling = false;
            }
        }

        /// <summary>
        /// Retries the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="attempts">The attempts.</param>
        /// <param name="initialInterval">The initial interval.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        private static T Retry(Func<T> action, int attempts = 5, int initialInterval = 10000)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            for (var i = 1; i <= attempts; i++)
            {
                try
                {
                    var result = action.Invoke();
                    return result;
                }
                catch (Exception)
                {
                    if (i >= attempts) throw;
                    Thread.Sleep(initialInterval);
                }

                initialInterval *= 5;
            }


            return null;
        }
        #endregion

        #region Events & Delegates
        /// <summary>
        /// Polls the complete.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result.</param>
        private void PollComplete(IAsyncResult asyncResult)
        {
            var worker = (AsyncWorker) ((AsyncResult) asyncResult).AsyncDelegate;
            var entity = worker.EndInvoke(asyncResult);
            var operation = (AsyncOperation) asyncResult.AsyncState;

            lock (_sync)
            {
                _polling = false;
            }

            if (entity != null)
                ((AsyncComplete)operation.UserSuppliedState).Invoke(entity);
        }

        private delegate T AsyncWorker();
        private delegate void AsyncComplete(T entity);

        public delegate void EntityReceivedArgs(T entity);
        public event EntityReceivedArgs EntityReceived;

        public delegate void PollerErrorArgs(Exception ex);
        public event PollerErrorArgs Error;

        private void OnEntity(T entity)
        {
            if (EntityReceived != null)
                EntityReceived(entity);

            Start();
        }
        #endregion


    }
}