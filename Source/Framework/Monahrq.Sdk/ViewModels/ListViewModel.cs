using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Model;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Windows;
using System.IO;
using System.Diagnostics;
using NHibernate.Exceptions;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The abstract / base class for all List based view models.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Monahrq.Sdk.Model.BaseNotify" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.IPaging" />
    [ImplementPropertyChanged]
    public abstract class ListViewModel<TEntity> : BaseNotify, IPartImportsSatisfiedNotification, INavigationAware, IPaging
        where TEntity : class, IEntity, new()
    {
        #region Private Fields

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
        /// Gets or sets the edit selected item command.
        /// </summary>
        /// <value>
        /// The edit selected item command.
        /// </value>
        public DelegateCommand<TEntity> EditSelectedItemCommand { get; set; }

        /// <summary>
        /// Gets or sets the add new item command.
        /// </summary>
        /// <value>
        /// The add new item command.
        /// </value>
        public DelegateCommand<TEntity> AddNewItemCommand { get; set; }

        /// <summary>
        /// Gets or sets the delete selected item command.
        /// </summary>
        /// <value>
        /// The delete selected item command.
        /// </value>
        public DelegateCommand<TEntity> DeleteSelectedItemCommand { get; set; }

        /// <summary>
        /// Gets or sets the save selected item command.
        /// </summary>
        /// <value>
        /// The save selected item command.
        /// </value>
        public DelegateCommand<TEntity> SaveSelectedItemCommand { get; set; }
        /// <summary>
        /// Gets or sets the cancel selected item command.
        /// </summary>
        /// <value>
        /// The cancel selected item command.
        /// </value>
        public DelegateCommand<TEntity> CancelSelectedItemCommand { get; set; }

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
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        protected IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the dataservice provider.
        /// </summary>
        /// <value>
        /// The dataservice provider.
        /// </value>
        [Import]
        protected IDomainSessionFactoryProvider DataserviceProvider { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initial load.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial load; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialLoad { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        private ListCollectionView _collectionItems;

        /// <summary>
        /// Gets or sets the collection items.
        /// </summary>
        /// <value>
        /// The collection items.
        /// </value>
        public ListCollectionView CollectionItems
        {
            get
            {
                return _collectionItems ?? (_collectionItems = new ListCollectionView(Enumerable.Empty<TEntity>().ToList()));
            }
            set { _collectionItems = value; }
        }

        /// <summary>
        /// Gets the items. Should onlny be utilized to retrieve items from the CollectionItems. i.e. Saving
        /// </summary>
        /// <value>
        /// The items from the CollectionItems.
        /// </value>
        protected ObservableCollection<TEntity> Items { get; set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return CollectionItems != null ? CollectionItems.Count : 0; } }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the current selected item.
        /// </summary>
        /// <value>
        /// The current selected item.
        /// </value>
        public TEntity CurrentSelectedItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loading.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading { get; set; }

        #endregion

        #region Methods & Events

        /// <summary>
        /// Called when [load].
        /// </summary>
        protected virtual void OnLoad()
        {
            using (var busyCursor = ApplicationCursor.SetCursor(System.Windows.Input.Cursors.Wait))
            {
                IsLoading = true;
                if (!IsInitialLoad)
                {
                    using (var session = DataserviceProvider.SessionFactory.OpenSession())
                    {
                        using (var trans = session.BeginTransaction())
                        {
                            ExecLoad(session);
                            trans.Commit();
                        }
                    }
                    IsLoaded = true;
                }
                IsInitialLoad = false;
                IsLoading = false;
                busyCursor.Pop();
            }

        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected virtual void ExecLoad(ISession session)
        {
            CollectionItems = new ListCollectionView(session.Query<TEntity>().ToObservableCollection());
        }

        /// <summary>
        /// Called when [un load].
        /// </summary>
        protected virtual void OnUnLoad() { }

        /// <summary>
        /// Called when [save].
        /// </summary>
        protected virtual void OnSave()
        {
            Validate();
            if (HasErrors) return;
            IsLoaded = false;
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        protected virtual void OnCancel() { }

        /// <summary>
        /// Called when [OnEdit].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void OnEdit(TEntity entity) { }

        /// <summary>
        /// Called when [add new item].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void OnAddNewItem(TEntity entity)
        {
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        if (!entity.IsPersisted)
                            session.SaveOrUpdate(entity);
                        else
                            entity = session.Merge(entity);

                        trans.Commit();

                        Notify(string.Format("{0} {1} has been successfully saved.", entity.Name,
                                                              typeof(TEntity).Name));
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        LogEntityError(exc, typeof(TEntity), entity.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Called when [delete].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void OnDelete(TEntity entity)
        {
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                string entityName = entity.Name;
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        //session.Evict(entity);

                        if (entity is ISoftDeletableOnly)
                        {
                            entity.IsDeleted = true;
                            entity = session.Merge(entity);
                            session.Evict(entity);
                        }
                        else
                        {
                            session.Evict(entity);
                            session.Delete(entity);
                        }
                        session.Flush();
                        trans.Commit();

                        Notify(string.Format("{0} {1} has been deleted.", entityName, Monahrq.Sdk.Utilities.Inflector.Titleize(typeof(TEntity).Name)));
                    }
                    //catch (InvalidConstraintException)
                    //{
                    //    entity.IsDeleted = true;
                    //    session.Merge(entity);
                    //}
                    //catch (ConstraintException)
                    //{
                    //    entity.IsDeleted = true;
                    //    session.Merge(entity);
                    //}
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        LogEntityError(exc, typeof(TEntity), entityName);
                        //throw;
                    }
                }
            }
        }

        /// <summary>
        /// Called when [cancel selected item].
        /// </summary>
        /// <param name="obj">The object.</param>
        protected virtual void OnCancelSelectedItem(TEntity obj)
        {
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        session.Refresh(obj);
                        trans.Commit();

                        Notify(string.Format("Edit canceled for {0} {1}.", obj.Name,
                                                             typeof(TEntity).Name));
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        LogEntityError(exc, typeof(TEntity), obj.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Called when [save selected item].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="showSuccessConfirmation">if set to <c>true</c> [show success confirmation].</param>
        protected virtual void OnSaveSelectedItem(TEntity obj, bool showSuccessConfirmation = true)
        {
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        if (!obj.IsPersisted)
                            session.SaveOrUpdate(obj);
                        else
                            obj = session.Merge(obj);

                        trans.Commit();

                        if (showSuccessConfirmation)
                        {
                            Notify(string.Format("{0} {1} has been successfully saved.", obj.Name, typeof(TEntity).Name));
                        }
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();
                        LogEntityError(exc, typeof(TEntity), obj.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can save the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        ///   <c>true</c> if this instance can save the specified argument; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanSave(TEntity arg)
        {
            return true;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            InitCommands();
            InitProperties();
        }

        /// <summary>
        /// Called when [view sample].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private static void OnViewSample(string fileName)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Samples", fileName);

            if (string.IsNullOrEmpty(fileName) || !File.Exists(file))
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Sample file not found !", string.Format("MONAHRQ {0}", MonahrqContext.ApplicationVersion.SubStrBeforeLast(".")), MessageBoxButton.OK);
                return;
            }
            Process.Start(file);
        }

        /// <summary>
        /// Forces the load.
        /// </summary>
        public void ForceLoad()
        {
            OnLoad();
        }


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
        protected void LogEntityError(Exception exc, Type entityType, string entityName, string action = "unknown")
        {
            this.Logger.Write(exc, "Error acting on entity {0} of type {1}, action = {2}", entityName, typeof(TEntity).Name, action);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected virtual void InitCommands()
        {
            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);
            EditSelectedItemCommand = new DelegateCommand<TEntity>(OnEdit);
            AddNewItemCommand = new DelegateCommand<TEntity>(OnAddNewItem);
            DeleteSelectedItemCommand = new DelegateCommand<TEntity>(OnDelete);
            //SaveSelectedItemCommand = new DelegateCommand<TEntity>(OnSaveSelectedItem, CanSave);
            CancelSelectedItemCommand = new DelegateCommand<TEntity>(OnCancelSelectedItem);
            SaveSelectedItemCommand = new DelegateCommand<TEntity>(ExecuteSaveSelectedItem, CanSave);
            ViewImportSampleCommand = new DelegateCommand<string>(OnViewSample);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected virtual void InitProperties()
        {

        }

        /// <summary>
        /// Executes the save selected item.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void ExecuteSaveSelectedItem(TEntity entity)
        {
            OnSaveSelectedItem(entity);
        }

        /// <summary>
        /// Called when [Refresh].
        /// </summary>
        public virtual void OnRefresh()
        {
            //Clear CollectionItems and reload Items
        }

        /// <summary>
        /// Fetches this instance.
        /// </summary>
        public virtual void Fetch()
        {
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public abstract void OnNavigatedTo(NavigationContext navigationContext);

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
        public abstract void OnNavigatedFrom(NavigationContext navigationContext);

        #endregion
    }
}
