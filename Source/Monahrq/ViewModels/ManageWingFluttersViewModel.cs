using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Dynamic;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.ViewModels;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using PropertyChanged;
using Monahrq.Sdk.Regions;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Flutters;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Domain.Wings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Runtime.Serialization;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model class for Manage Wing Flutters
    /// </summary>
    [Export(typeof(ManageWingFluttersViewModel)), ImplementPropertyChanged]
    [DataContract]
    [Serializable]
    [KnownType(typeof(DynamicMeasure))]
    [KnownType(typeof(DynamicTopic))]
    [KnownType(typeof(DynamicTarget))]
    public class ManageWingFluttersViewModel : SettingsViewModel
    {
        #region Fields

        [IgnoreDataMember] private readonly object _flutterItemsLock = new object();
        private bool _wingInstallPopupIsVisible;
        private bool _flutterInstallPopupIsVisible;
        [IgnoreDataMember] private readonly object _statusLogLock = new object();
        [IgnoreDataMember] private readonly object _wingItemsLock = new object();
        private bool _isActive;
        private string _originalHashValue;

        //[field: NonSerialized]
        //public event EventHandler IsActiveChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageWingFluttersViewModel"/> class.
        /// </summary>
        public ManageWingFluttersViewModel()
        {
            WingInstallPopupIsVisible = false;
            InstallationStatusText = null;
            //FlutterItems = new ObservableCollection<Flutter>();
            //BindingOperations.EnableCollectionSynchronization(FlutterItems, _flutterItemsLock);

            StatusLog = new ObservableCollection<string>();
            ApplicableReports = new ObservableCollection<Report>();
            BindingOperations.EnableCollectionSynchronization(StatusLog, _statusLogLock);
        }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IConfigurationService ConfigService { get; set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        [IgnoreDataMember]
        ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the data service provider.
        /// </summary>
        /// <value>
        /// The data service provider.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IDomainSessionFactoryProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the dynamic target service.
        /// </summary>
        /// <value>
        /// The dynamic target service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        protected IDynamicTargetService DynamicTargetService { get; set; }

        #endregion

        #region DelegateCommands

        /// <summary>
        /// Gets the show install wing pop up command.
        /// </summary>
        /// <value>
        /// The show install wing pop up command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<object> ShowInstallWingPopUpCommand { get; private set; }

        /// <summary>
        /// Gets the show install flutter pop up command.
        /// </summary>
        /// <value>
        /// The show install flutter pop up command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<object> ShowInstallFlutterPopUpCommand { get; private set; }

        /// <summary>
        /// Gets the hide install wing pop up command.
        /// </summary>
        /// <value>
        /// The hide install wing pop up command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<object> HideInstallWingPopUpCommand { get; private set; }

        /// <summary>
        /// Gets the hide install flutter pop up command.
        /// </summary>
        /// <value>
        /// The hide install flutter pop up command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<object> HideInstallFlutterPopUpCommand { get; private set; }

        /// <summary>
        /// Gets the close progress pop up command.
        /// </summary>
        /// <value>
        /// The close progress pop up command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<object> CloseProgressPopUpCommand { get; private set; }

        /// <summary>
        /// Gets the download template command.
        /// </summary>
        /// <value>
        /// The download template command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<Target> DownloadTemplateCommand { get; private set; }

        /// <summary>
        /// Gets or sets the install wing command.
        /// </summary>
        /// <value>
        /// The install wing command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand InstallWingCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand InstallFlutterCommand { get; private set; }

        /// <summary>
        /// Gets or sets the uninstall wing command.
        /// </summary>
        /// <value>
        /// The uninstall wing command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<Target> UninstallWingCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand<Flutter> UninstallFlutterCommand { get; private set; }

        /// <summary>
        /// Gets or sets the disable wing command.
        /// </summary>
        /// <value>
        /// The disable wing command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<Target> DisableWingCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand<Target> DisableFlutterCommand { get; private set; }

        /// <summary>
        /// Gets or sets the enable wing command.
        /// </summary>
        /// <value>
        /// The enable wing command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand<Target> EnableWingCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand<Target> EnableFlutterCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand SelectWingFileCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand SelectFlutterFileCommand { get; private set; }

        [IgnoreDataMember]
        public DelegateCommand CloseCommand { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [wing install popup is visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [wing install popup is visible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WingInstallPopupIsVisible
        {
            get { return _wingInstallPopupIsVisible; }
            set
            {
                StatusLog.Clear();
                _wingInstallPopupIsVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [flutter install popup is visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [flutter install popup is visible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool FlutterInstallPopupIsVisible
        {
            get { return _flutterInstallPopupIsVisible; }
            set
            {
                StatusLog.Clear();
                _flutterInstallPopupIsVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [progress popup visibility].
        /// </summary>
        /// <value>
        /// <c>true</c> if [progress popup visibility]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ProgressPopupVisibility { get; set; }

        /// <summary>
        /// Gets or sets the installation status text.
        /// </summary>
        /// <value>
        /// The installation status text.
        /// </value>
        [DataMember]
        public string InstallationStatusText { get; set; }

        /// <summary>
        /// Gets or sets the index of the current tab.
        /// </summary>
        /// <value>
        /// The index of the current tab.
        /// </value>
        [DataMember]
        public int CurrentTabIndex { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is wings view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is wings view; otherwise, <c>false</c>.
        /// </value>
        public bool IsWingsView { get { return CurrentTabIndex == 0; } }

        /// <summary>
        /// Gets a value indicating whether this instance is flutters view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is flutters view; otherwise, <c>false</c>.
        /// </value>
        public bool IsFluttersView { get { return CurrentTabIndex == 1; } }

        /// <summary>
        /// Gets or sets the status log.
        /// </summary>
        /// <value>
        /// The status log.
        /// </value>
        [DataMember]
        public ObservableCollection<string> StatusLog { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [DataMember]
        public ObservableCollection<Target> WingItems { get; set; }

        /// <summary>
        /// Gets or sets the flutter items.
        /// </summary>
        /// <value>
        /// The flutter items.
        /// </value>
        /// 
        [DataMember]
        public ObservableCollection<Flutter> FlutterItems { get; set; }

        /// <summary>
        /// Gets or sets the applicable reports.
        /// </summary>
        /// <value>
        /// The applicable reports.
        /// </value>
        [DataMember]
        public ObservableCollection<Report> ApplicableReports { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating whether the object is active.
        ///// </summary>
        ///// <value>
        ///// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        ///// </value>
        //[DataMember]
        //public bool IsActive
        //{
        //    get { return _isActive; }
        //    set
        //    {
        //        _isActive = value;

        //        if (_isActive)
        //            OnIsActiveChanged(this, new EventArgs());
        //    }
        //}

        /// <summary>
        /// Gets or sets the selected wing file.
        /// </summary>
        /// <value>
        /// The selected wing file.
        /// </value>
        [DataMember]
        public string SelectedWingFile { get; set; }

        /// <summary>
        /// Gets or sets the selected flutter file.
        /// </summary>
        /// <value>
        /// The selected flutter file.
        /// </value>
        [DataMember]
        public string SelectedFlutterFile { get; set; }

        /// <summary>
        /// Gets or sets the selected wing item.
        /// </summary>
        /// <value>
        /// The selected wing item.
        /// </value>
        [DataMember]
        public Target SelectedWingItem { get; set; }

        /// <summary>
        /// Gets or sets the selected flutter item.
        /// </summary>
        /// <value>
        /// The selected flutter item.
        /// </value>
        [DataMember]
        public Flutter SelectedFlutterItem { get; set; }

        /// <summary>
        /// Gets or sets the name of the unistall wing.
        /// </summary>
        /// <value>
        /// The name of the unistall wing.
        /// </value>
        [DataMember]
        public string UnistallWingName { get; set; }

        /// <summary>
        /// Gets or sets the name of the unistall flutter.
        /// </summary>
        /// <value>
        /// The name of the unistall flutter.
        /// </value>
        [DataMember]
        public string UnistallFlutterName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [wing installed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wing installed]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WingInstalled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [flutter installed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [flutter installed]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool FlutterInstalled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [uninstall in progress].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uninstall in progress]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UninstallInProgress { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        [IgnoreDataMember]
        public BaseViewModel Parent { get; set; }

        /// <summary>
        /// Gets or sets the original hash value.
        /// </summary>
        /// <value>
        /// The original hash value.
        /// </value>
        public override string OriginalHashValue
        {
            get { return _originalHashValue; }
            set
            {
                _originalHashValue = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [dowload template].
        /// </summary>
        /// <param name="target">The target.</param>
        public void OnDowloadTemplate(Target target)
        {
            //var target = objTarget as Target;
            if (target == null || string.IsNullOrEmpty(target.TemplateFileName)) return;

            var wingXmlPath = string.Format("{0}{1}", MonahrqContext.MyDocumentsApplicationDirPath, target.WingTargetXmlFilePath); // Path.Combine(DynamicTargetService.WingsFolder, target.WingTargetXmlFilePath);
            var templateDirectory = new FileInfo(wingXmlPath).Directory;

            if (templateDirectory != null)
            {
                var templateFilePath = Path.Combine(templateDirectory.FullName, target.TemplateFileName);
                var templateFileInfo = new FileInfo(templateFilePath);

                if (templateFileInfo == null || !templateFileInfo.Exists) return;

                using (var templateLocProc = new Process())
                {
                    templateLocProc.StartInfo.WorkingDirectory = templateDirectory.FullName;
                    templateLocProc.StartInfo.FileName = templateFileInfo.Name;

                    templateLocProc.StartInfo.CreateNoWindow = true;
                    templateLocProc.Start();
                    return;
                }
            }
        }

        /// <summary>
        /// Determines whether this instance [can download template] the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can download template] the specified target; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDownloadTemplate(Target target)
        {
            //var target = objTarget as Target;
            if (target == null || string.IsNullOrEmpty(target.TemplateFileName)) return false;

            var templatePath = Path.Combine(DynamicTargetService.WingsFolder, target.WingTargetXmlFilePath);
            var templateDirectory = new FileInfo(templatePath).Directory;

            if (templateDirectory == null) return false;

            return File.Exists(Path.Combine(templateDirectory.FullName, target.TemplateFileName));
        }

        /// <summary>
        /// Called when [load].
        /// </summary>
        public void OnLoad()
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                WingItems = session.CreateCriteria<Target>()
                               .Add(Restrictions.Eq("IsCustom", true))
                               .AddOrder(new Order("Name", true))
                               .SetResultTransformer(new DistinctRootEntityResultTransformer())
                               .Future<Target>()
                               .ToObservableCollection();
                BindingOperations.EnableCollectionSynchronization(WingItems, _wingItemsLock);

                FlutterItems = session.CreateCriteria<Flutter>()
                               .AddOrder(new Order("Name", true))
                               .SetResultTransformer(new DistinctRootEntityResultTransformer())
                               .Future<Flutter>()
                               .ToObservableCollection();
                BindingOperations.EnableCollectionSynchronization(FlutterItems, _flutterItemsLock);

            }
        }

        /// <summary>
        /// Called when [install wing].
        /// </summary>
        private async void OnInstallWing()
        {
            WingInstalled = false;

            StatusLog.Clear();

            if (string.IsNullOrEmpty(SelectedWingFile)) return;

            await Task.Run(() => StatusLog.Add("Copying files to Wing directory. Please, stand by. It may take a few seconds."));

            await Task.Delay(100);

            var targetsToInstall = await Task.Run(() => DynamicTargetService.CopyTargetFilesToDirectory(SelectedWingFile).ToList());

            using (var session = Provider.SessionFactory.OpenSession())
            {
                var wing = session.Query<Wing>().FirstOrDefault(w => w.WingGUID == new Guid(DynamicTargetWingConstants.WING_MODULE_GUID));
                if (wing == null)
                    throw new NotSupportedException("Dynamic wings module is not installed. XML Wings cannot be installed.");

                foreach (var dynamicTarget in targetsToInstall)
                {
                    StatusLog.Add(string.Format("Starting installation of wing \"{0}\".", dynamicTarget.Name));

                    #region "Asynchronous Aproach"
                    var result = await DynamicTargetService.InstallDynamicTargetAsync(dynamicTarget, wing, session, DynamicTargetService.Context.Value,
                        InstallWingProgress);
                    if (!result)
                        StatusLog.Add($"An error occurred while trying to install wing \"{dynamicTarget.Name}\". Please try again and if the error persists, please contact technical assistance for help.");
                    #endregion

                    #region synchronous install
                    //var result = DynamicTargetService.InstallDynamicTarget(dynamicTarget, wing, session);

                    //switch (result.Status)
                    //{
                    //    case OpenSourceInstallFlags.AlreadyExists:
                    //        uiContext.Send(x => StatusLog.Add(string.Format("Wing \"{0}\" has already been installed.", dynamicTarget.Name)), null);
                    //        continue;
                    //    case OpenSourceInstallFlags.Error:
                    //        // TODO: Add message for user.
                    //        uiContext.Send(x => StatusLog.Add(string.Format("An error occurred while trying to install wing \"{0}\". Please try again and if the error persists, please contact technical assistance for help.", dynamicTarget.Name)), null);
                    //        Logger.Write(result);
                    //        break;
                    //    case OpenSourceInstallFlags.Success:
                    //        uiContext.Send(x => StatusLog.Add(
                    //            string.Format(
                    //                "Start installation of associated measures, reports, wing dataset target tables for wing \"{0}\".",
                    //                dynamicTarget.Name)), null);

                    //        target = result.Target;

                    //        if (target != null)
                    //        {
                    //            // Thread.Sleep(500);

                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(string.Format("Start importing measures for wing \"{0}\".",
                    //                        dynamicTarget.Name)), null);
                    //            DynamicTargetService.ImportMeasures(target, dynamicTarget, session);
                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(string.Format("End importing measures for wing \"{0}\".",
                    //                        dynamicTarget.Name)), null);

                    //            // Thread.Sleep(500);
                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(string.Format("Start importing reports for wing \"{0}\".",
                    //                        dynamicTarget.Name)), null);
                    //            DynamicTargetService.ImportReports(dynamicTarget, session);
                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(string.Format("End importing reports for wing \"{0}\".",
                    //                        dynamicTarget.Name)), null);

                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(string.Format("Finalizing installation for wing \"{0}\".",
                    //                        dynamicTarget.Name)), null);
                    //            // Thread.Sleep(1000);
                    //            uiContext.Send(
                    //                x =>
                    //                    StatusLog.Add(
                    //                        string.Format("Installation for wing \"{0}\" successfully completed.",
                    //                            dynamicTarget.Name)), null);

                    //            if (target != null && !WingItems.Any(item => item.Name.EqualsIgnoreCase(target.Name)))
                    //                WingItems.Add(target);

                    //        }
                    //        break;
                    //}
                    #endregion
                }
            }
            WingInstalled = true;
        }

        /// <summary>
        /// Installs the wing progress.
        /// </summary>
        /// <param name="progressResult">The progress result.</param>
        public void InstallWingProgress(OpenSourceInstallResult progressResult)
        {
            switch (progressResult.Status)
            {
                case OpenSourceInstallFlags.AlreadyExists:
                    StatusLog.Add(progressResult.Message);

                    if (progressResult.Target != null && WingItems != null && !WingItems.Any(item => item != null && !string.IsNullOrEmpty(item.Name) && item.Name.EqualsIgnoreCase(progressResult.Target.Name)))
                        WingItems.Add(progressResult.Target);
                    break;
                case OpenSourceInstallFlags.StatusUpdate:
                    StatusLog.Add(progressResult.Message);
                    break;
                case OpenSourceInstallFlags.Success:
                    StatusLog.Add(progressResult.Message);

                    if (progressResult.Target != null && !WingItems.Any(wi => wi.Name.EqualsIgnoreCase(progressResult.Target.Name)))
                        WingItems.Add(progressResult.Target);

                    break;
            }
        }

        /// <summary>
        /// Called when [install flutter].
        /// </summary>
        private async void OnInstallFlutter()
        {
            FlutterInstalled = false;

            var uiContext = SynchronizationContext.Current;

            StatusLog.Clear();
            ApplicableReports.Clear();

            if (string.IsNullOrEmpty(SelectedFlutterFile)) return;


            OSFlutterInstallResult installResult = null;

            var cTokenSource = new CancellationTokenSource();
            var taskResult = await DynamicTargetService.InstallFlutterFiles(SelectedFlutterFile, cTokenSource.Token,
                result =>
                {
                    installResult = result;
                    if (result.Status == OSFlutterInstallFlags.StatusUpdate || result.Status == OSFlutterInstallFlags.UploadFileComplete)
                        uiContext.Send(x => StatusLog.Add(result.Message), null);

                    if (result.Status == OSFlutterInstallFlags.Success)
                    {
                        uiContext.Send(x => StatusLog.Add(result.Message), null);
                    }
                },
                exceptionResult =>
                {
                    if (exceptionResult.Status == OSFlutterInstallFlags.Error)
                    {
                        uiContext.Send(x => StatusLog.Add(exceptionResult.Message), null);
                        Logger.Warning(installResult.ToString());

                        installResult = exceptionResult;
                    }
                });

            if (installResult.Status == OSFlutterInstallFlags.Success && installResult.Flutter != null)
            {
                if (FlutterItems.All(f => !f.Name.EqualsIgnoreCase(installResult.Flutter.Name)))
                {
                    FlutterItems.Add(installResult.Flutter);
                }
            }
            FlutterInstalled = true;
        }

        /// <summary>
        /// Called when [uninstall wing].
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        private async void OnUninstallWing(Target dynamicTarget)
        {
            if (dynamicTarget == null) return;



            var uninstallConfirmation = MessageBox.Show(@"Are you sure you want to uninstall '" + dynamicTarget.DisplayName + "'?"
                + Environment.NewLine + "The Website(s) using this Wing will be deleted."
                , @"Uninstall Confirmation", MessageBoxButton.YesNo);

            if (uninstallConfirmation == MessageBoxResult.No) return;
            UninstallInProgress = true;
            StatusLog.Clear();
            ProgressPopupVisibility = true;

            await Task.Delay(500);

            UnistallWingName = dynamicTarget.DisplayName;
            var cancellationToken = new CancellationTokenSource();
            var uninstallResult = await DynamicTargetService.Uninstall(dynamicTarget, cancellationToken.Token, result =>
            {
                if (result.Status == OpenSourceUnInstallFlags.StatusUpdate)
                {
                    StatusLog.Add(result.Message);
                }
                if (result.Status == OpenSourceUnInstallFlags.Success)
                {
                    StatusLog.Add(string.Format("Uninstallation of wing \"{0}\" completed successfully.", dynamicTarget.Name));

                    if (WingItems.Contains(dynamicTarget))
                        WingItems.Remove(dynamicTarget);
                }
            },
                exceptionResult =>
                {
                    var statusMessage = string.Format("Uninstallation of wing \"{0}\" was unsuccessful. Please try again. If for any reason the" +
                                                      "the error still remains after trying to uninstall again. Please contact Monahrq technical assistance.",
                                                      dynamicTarget.Name);
                    StatusLog.Add(statusMessage);
                    Logger.Write(exceptionResult.Exception);
                });


            UninstallInProgress = false;


        }

        /// <summary>
        /// Called when [uninstall flutter].
        /// </summary>
        /// <param name="flutter">The flutter.</param>
        private async void OnUninstallFlutter(Flutter flutter)
        {
            var uiContext = SynchronizationContext.Current;

            var uninstallConfirmation = MessageBox.Show(@"Are you sure you want to uninstall '" + flutter.Name + "'?"
                , @"Uninstall Confirmation", MessageBoxButton.YesNo);

            if (uninstallConfirmation == MessageBoxResult.No) return;
            UninstallInProgress = true;
            StatusLog.Clear();
            ProgressPopupVisibility = true;

            var cancellationToken = new CancellationTokenSource();

            var uninstallResult = await DynamicTargetService.Uninstall(flutter, cancellationToken.Token,
                result =>
                {
                    if (result.Status == OpenSourceUnInstallFlags.StatusUpdate)
                    {
                        StatusLog.Add(result.Message);
                    }
                    if (result.Status == OpenSourceUnInstallFlags.Success)
                    {
                        StatusLog.Add(string.Format("Uninstallation of flutter \"{0}\" completed successfully.", flutter.Name));
                    }
                },
                errorCallback =>
                {
                    var statusMessage = string.Format("Uninstallation of flutter \"{0}\" was unsuccessful. Please try again. If for any reason the" +
                                                      "the error still remains after trying to uninstall again. Please contact Monahrq technical assistance.",
                                                     flutter.Name);
                    uiContext.Send(x => StatusLog.Add(statusMessage), null);
                    Logger.Write(errorCallback.Exception);

                    //if (FlutterItems.Contains(flutter))
                    //    FlutterItems.Remove(flutter);
                });
            if (FlutterItems.Contains(flutter))
                FlutterItems.Remove(flutter);
            UninstallInProgress = false;

        }

        /// <summary>
        /// Fakes the status.
        /// </summary>
        private void FakeStatus()
        {
            for (int i = 0; i < 50; i++)
            {
                StatusLog.Add(i + " aasdfasdfasdfasdfasdfasdfasdfasdfasd fasdfasdfasdfasdfasdfsadfasd dfasdfasd fasdfasdfasdfasdfasdfsadfasd");
                Task.Delay(100);
                StatusLog.Add(i + " zxcvzxcvzxcv zxcv zxcvz zxcv zxcv zxcvxz xzcv zxcv zxcv zxcv zxcv xcz");
                Task.Delay(100);
                StatusLog.Add(i + " qwerqwerwerq qwer qwer qwerwerwqerqwerq  qwer qwerqwerwerqwreqw ");
            }
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        /// <param name="target">The target.</param>
        private void OnEnable(Target target)
        {
            DisableEnableTarget(target, false);
        }

        /// <summary>
        /// Called when [disable].
        /// </summary>
        /// <param name="target">The target.</param>
        private void OnDisable(Target target)
        {
            DisableEnableTarget(target, true);
        }

        /// <summary>
        /// Disables the enable target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        private void DisableEnableTarget(Target target, bool disable)
        {
            if (target == null) return;

            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        target.IsDisabled = disable;
                        target = session.Merge(target);
                        trans.Commit();

                        var message = string.Format("Wing \"{0}\" has been successfully disabled.", target.Name);
                        EventAggregator.GetEvent<GenericNotificationEvent>().Publish(message);
                    }
                    catch (Exception exc)
                    {

                        Logger.Write((exc.InnerException ?? exc));
                        EventAggregator.GetEvent<ServiceErrorEvent>()
                                       .Publish(new ServiceErrorEventArgs((exc.InnerException ?? exc),
                                                                          typeof(Target).Name, target.Name));
                        trans.Rollback();
                    }
                }
            }
        }

        // Let the user browse for the file to import
        /// <summary>
        /// Called when [select wing file].
        /// </summary>
        private void OnSelectWingFile()
        {
            // Set filter for file extension and default file extension 

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.zip",
                InitialDirectory = ConfigService.LastDataFolder,
                Filter = "ZIP Files (*.zip)|*.zip|Xml Files (*.xml)|*.xml",
                //Filter = "Xml Files (*.xml)|*.xml", All Files (*.*)|*.*|
                FilterIndex = 1,
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            SelectedWingFile = dlg.FileName;
            ConfigService.LastDataFolder = Path.GetDirectoryName(SelectedWingFile);
        }

        /// <summary>
        /// Called when [select flutter file].
        /// </summary>
        private void OnSelectFlutterFile()
        {
            // Set filter for file extension and default file extension 

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.zip",
                InitialDirectory = ConfigService.LastDataFolder,
                Filter = "All Files (*.*)|*.*|ZIP Files (*.zip)|*.zip",
                //Filter = "All Files (*.*)|*.*|Xml Files (*.xml)|*.xml",
                FilterIndex = 1,
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            SelectedFlutterFile = dlg.FileName;
            ConfigService.LastDataFolder = Path.GetDirectoryName(SelectedFlutterFile);
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public override int Index { get { return 2; } set {} }

        /// <summary>
        /// Called when [save].
        /// </summary>
        public override void OnSave()
        {}

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {}

        /// <summary>
        /// Called when [reset].
        /// </summary>
        public override void OnReset()
        {}

        /// <summary>
        /// Called when [is active changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        public override void OnIsActiveChanged(object sender, EventArgs eventArgs)
        {
            OnReset();

            if (IsActive)
                OnLoad();
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {}

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {}

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            InstallWingCommand = new DelegateCommand(OnInstallWing, () => !string.IsNullOrEmpty(SelectedWingFile), this, new string[] { "SelectedWingFile" });
            UninstallWingCommand = new DelegateCommand<Target>(OnUninstallWing, t => true);
            InstallFlutterCommand = new DelegateCommand(OnInstallFlutter, () => !string.IsNullOrEmpty(SelectedFlutterFile), this, () => SelectedFlutterFile);
            UninstallFlutterCommand = new DelegateCommand<Flutter>(OnUninstallFlutter, t => true);
            DisableWingCommand = new DelegateCommand<Target>(OnDisable, t => true);
            EnableWingCommand = new DelegateCommand<Target>(OnEnable, t => true);
            SelectWingFileCommand = new DelegateCommand(OnSelectWingFile, () => true);
            SelectFlutterFileCommand = new DelegateCommand(OnSelectFlutterFile, () => true);
            ShowInstallWingPopUpCommand = new DelegateCommand<object>(OnToggleInstallWingPopupVisibility, t => true);
            ShowInstallFlutterPopUpCommand = new DelegateCommand<object>(OnToggleInstallFlutterPopupVisibility, t => true);
            HideInstallWingPopUpCommand = new DelegateCommand<object>(OnToggleInstallWingPopupVisibility, t => true);
            HideInstallFlutterPopUpCommand = new DelegateCommand<object>(OnToggleInstallFlutterPopupVisibility, t => true);
            CloseProgressPopUpCommand = new DelegateCommand<object>(OnCloseProgressPopup, t => true);
            CloseCommand = new DelegateCommand(OnClose, () => true);
            DownloadTemplateCommand = new DelegateCommand<Target>(OnDowloadTemplate, t => true); //CanDownloadTemplate
        }

        /// <summary>
        /// Called when [toggle install wing popup visibility].
        /// </summary>
        /// <param name="skipIfTrue">The skip if true.</param>
        private void OnToggleInstallWingPopupVisibility(object skipIfTrue)
        {
            if (skipIfTrue == null) skipIfTrue = false;

            var shouldSkip = bool.Parse(skipIfTrue.ToString());
            if (shouldSkip && WingInstallPopupIsVisible) return;

            WingInstallPopupIsVisible = (!WingInstallPopupIsVisible) ? true : false;

            if (WingInstallPopupIsVisible) SelectedWingFile = null;
        }

        /// <summary>
        /// Called when [toggle install flutter popup visibility].
        /// </summary>
        /// <param name="skipIfTrue">The skip if true.</param>
        private void OnToggleInstallFlutterPopupVisibility(object skipIfTrue)
        {
            if (skipIfTrue == null) skipIfTrue = false;

            var shouldSkip = bool.Parse(skipIfTrue.ToString());
            if (shouldSkip && FlutterInstallPopupIsVisible) return;

            FlutterInstallPopupIsVisible = (!FlutterInstallPopupIsVisible) ? true : false;

            if (FlutterInstallPopupIsVisible) SelectedFlutterFile = null;
        }

        /// <summary>
        /// Called when [close progress popup].
        /// </summary>
        /// <param name="popupArg">The popup argument.</param>
        public void OnCloseProgressPopup(object popupArg)
        {
            ProgressPopupVisibility = false;
        }

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        private void OnClose()
        {
            var isDirty = GetViewModelHashCode(this, GetType()) != OriginalHashValue;
            if (isDirty && MessageBox.Show("The data has been edited. Are you sure you want to leave before saving?", "Modification Verification", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri("WelcomeView", UriKind.Relative));
        }

        #endregion
    }
}
