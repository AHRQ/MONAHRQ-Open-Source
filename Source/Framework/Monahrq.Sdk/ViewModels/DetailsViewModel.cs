using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Model;
using Monahrq.Sdk.Utilities;
using NHibernate;
using NHibernate.Criterion;
using PropertyChanged;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Generic;
using System.Collections;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The abstract\base details view model for all details related details related view models.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Monahrq.Sdk.Model.BaseNotify" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="System.IDisposable" />
    [ImplementPropertyChanged]
    [RegionMemberLifetime(KeepAlive = false)]
    public abstract class DetailsViewModel<TEntity> : BaseNotify, INavigationAware, IPartImportsSatisfiedNotification, IDisposable
        where TEntity : class, IEntity, new()
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsViewModel{TEntity}"/> class.
        /// </summary>
        protected DetailsViewModel()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsViewModel{TEntity}"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        protected DetailsViewModel(TEntity model)
        {
            Model = model;
        }

        #endregion

        #region Commands

        /// <summary>
        /// The save command
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public DelegateCommand SaveCommand { get; set; }

        /// <summary>
        /// The cancel command
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public DelegateCommand DeleteCommand { get; set; }

        /// <summary>
        /// Gets or sets the create new command.
        /// </summary>
        /// <value>
        /// The create new command.
        /// </value>
        public DelegateCommand CreateNewCommand { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        protected IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import]
        protected IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the data service provider.
        /// </summary>
        /// <value>
        /// The data service provider.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IDomainSessionFactoryProvider DataServiceProvider { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        private TEntity _model;

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public TEntity Model
        {
            get { return _model; }
            set { _model = value; }
        }

        /// <summary>
        /// Gets or sets the additional entities.
        /// </summary>
        /// <value>
        /// The additional entities.
        /// </value>
        public Dictionary<string, IEnumerable<IEntity>> AdditionalEntities { get; set; }

        /// <summary>
        /// Gets or sets the original model hash code.
        /// </summary>
        /// <value>
        /// The original model hash code.
        /// </value>
        public string OriginalModelHashCode { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes the current model.
        /// </summary>
        protected virtual void RefreshCurrentModel()
        {
            if (Model == null) return;
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                session.Refresh(Model);
            }
        }

        /// <summary>
        /// Loads the Model entity by id during the onLoad and/or OnRefresh method is called.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public virtual void LoadModel(object id)
        {
            if (id == null) return; // THROW AN USER READABLE ERROR AND LOG.
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            using (var trans = session.BeginTransaction())
            {
                ExecLoad(session, id);

                trans.Commit();
            }
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        protected virtual void ExecLoad(ISession session, object id)
        {
            Model = session.CreateCriteria<TEntity>()
                           .Add(Restrictions.Eq("Id", id))
                           .FutureValue<TEntity>()
                           .Value;
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="completeCallback">The complete callback.</param>
        public virtual void OnSaveAsync(TEntity model, Action<OperationResult<TEntity>> completeCallback)
        {
            model.CleanBeforeSave();
            model.Validate();

            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        OnSaveInternal(session, model);

                        trans.Commit();
                        //session.Flush();
                        //
                        completeCallback(new OperationResult<TEntity> {Model = model, Status = true});

                        Notify(string.Format("{0} {1} has been successfully saved.", model.Name,
                                                              typeof(TEntity).Name));
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        completeCallback(new OperationResult<TEntity> { Exception = exc.GetBaseException(), Status = false });
                        NotifyError(exc, typeof(TEntity), model.Name, "Error saving data for entity of type {0}", typeof(TEntity).Name);
                        //throw;
                    }
                }

            }
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void OnSave(TEntity model)
        {
            model.CleanBeforeSave();
            model.Validate();

            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        OnSaveInternal(session, model);

                        trans.Commit();
                        //session.Flush();
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        throw exc;
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="enableNotificantions">if set to <c>true</c> [enable notificantions].</param>
        public virtual void OnSave(bool enableNotificantions = true)
        {
            Model.CleanBeforeSave();
            Model.Validate();

            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        OnSaveInternal(session, Model);

                        trans.Commit();
                        //session.Flush();
                        //
                        if(enableNotificantions)
                            Notify(string.Format("{0} {1} has been successfully saved.", Model.Name,
                                                              typeof(TEntity).Name));
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        NotifyError(exc, typeof(TEntity), Model.Name);

                        //throw;
                    }
                }

            }
        }

        /// <summary>
        /// Called when [save internal].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="model">The model.</param>
        protected virtual void OnSaveInternal(ISession session, TEntity model)
        {
            if (!Model.IsPersisted)
                session.SaveOrUpdate(model);
            else
                try
                {
                    Model = session.Merge(model);
                    //Model = model;
                }
                catch
                {
                    Model = session.Merge(model);
                }
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public virtual void OnCancel() { }

        /// <summary>
        /// Called when [delete].
        /// </summary>
        public virtual void OnDelete()
        {
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    if (Model is ISoftDeletableOnly)
                    {
                        Model.IsDeleted = true;
                        Model = session.Merge(Model);
                    }
                    else
                    {
                        session.Delete(Model);
                    }
                    session.Evict(Model);

                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// Called when [create new].
        /// </summary>
        public virtual void OnCreateNew() { }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            InitCommands();
            InitProperties();
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected virtual void InitCommands()
        {
            SaveCommand = new DelegateCommand(() => this.OnSave(), () => !_saveInitiated);
            CancelCommand = new DelegateCommand(OnCancel);
            DeleteCommand = new DelegateCommand(OnDelete);
            CreateNewCommand = new DelegateCommand(OnCreateNew);
        }

        protected bool _saveInitiated = false;

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected virtual void InitProperties()
        {}

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        /// <param name="queries">The queries.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> InitProperties(QueryPacket[] queries)
        {
            var results = new Dictionary<string, object>();
            IList queryResults;
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                var multiQuery = session.CreateMultiQuery();

                foreach (var q in queries)
                {
                    multiQuery.Add(q.EnityType.Name, q.Hql);
                    multiQuery.SetCacheable(true);
                    multiQuery.SetCacheRegion(Inflector.Pluralize(q.EnityType.Name));
                    multiQuery.SetForceCacheRefresh(true);
                }

                queryResults = multiQuery.List();
            }

            return results;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// Gets the model hash code.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public string GetModelHashCode(TEntity entity)
        {
            if (entity == null) return null;
            var crypto = new SHA1CryptoServiceProvider();
            try
            {
                var serializer = JsonHelper.Serialize(entity);

                using (var memory = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memory))
                    {
                        writer.Write(serializer);
                    }
                    var hashValue = crypto.ComputeHash(memory.ToArray());
                    return Convert.ToBase64String(hashValue);
                }
            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error computing hash code for model");
                return null;
            }
            finally
            {
                crypto.Dispose();
            }
        }

        /// <summary>
        /// Determines whether the specified entity is dirty.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity is dirty; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsDirty(TEntity entity)
        {
            if (string.IsNullOrEmpty(OriginalModelHashCode) || entity == null) return false;

            var currentHashCode = GetModelHashCode(entity);

            return currentHashCode != OriginalModelHashCode;
        }

        #endregion

        #region INavigateAware

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            //CurrentSession = DataServiceProvider.SessionFactory.OpenSession();
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public abstract bool IsNavigationTarget(NavigationContext navigationContext);

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Model = null;
        }

        #endregion

        #region Struct

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Monahrq.Sdk.Model.BaseNotify" />
        /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
        /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
        /// <seealso cref="System.IDisposable" />
        public struct QueryPacket
        {
            /// <summary>
            /// Gets or sets the type of the enity.
            /// </summary>
            /// <value>
            /// The type of the enity.
            /// </value>
            public Type EnityType { get; set; }
            /// <summary>
            /// Gets or sets the HQL.
            /// </summary>
            /// <value>
            /// The HQL.
            /// </value>
            public string Hql { get; set; }
        }

        #endregion

        #region UI Notification
        /// <summary>
        /// Notifies the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void Notify(string message)
        {
            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(message);
        }

        /// <summary>
        /// Notifies the error.
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityName">Name of the entity.</param>
        protected void NotifyError(Exception exc, Type entityType, string entityName, string message = null, params object[] args)
        {
            EventAggregator.GetEvent<ServiceErrorEvent>()
                           .Publish(new ServiceErrorEventArgs((exc.InnerException ?? exc), typeof(TEntity).Name, entityName));
        }

        #endregion

    }

}