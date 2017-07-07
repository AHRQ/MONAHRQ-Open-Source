using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.Controls.Wizard.Helpers;

namespace Monahrq.Theme.Controls.Wizard.Models.Data
{

	/// <summary>
	/// Type to handle the 'NextEvent'.
	/// </summary>
	/// <param name="currentStep">The current step.</param>
	public delegate void NextEventHandler(object currentStep);

	/// <summary>
	/// Interface for DataWizardViewModels.
	/// </summary>
	/// <typeparam name="TWizardBusinessObject">The type of the wizard business object.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IWizardViewModel" />
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	public class DataWizardViewModel<TWizardBusinessObject> : IWizardViewModel, INotifyPropertyChanged where TWizardBusinessObject : IWizardDataContextObject, new()
    {
		/// <summary>
		/// Gets or sets the business object.
		/// </summary>
		/// <value>
		/// The business object.
		/// </value>
		TWizardBusinessObject BusinessObject
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the region MGR.
		/// </summary>
		/// <value>
		/// The region MGR.
		/// </value>
		protected IRegionManager RegionMgr { get; set; }

		/// <summary>
		/// The step manager
		/// </summary>
		private readonly StepManager<TWizardBusinessObject> _stepManager;
		/// <summary>
		/// The export data mapping command
		/// </summary>
		private RelayCommand _exportDataMappingCommand;
		/// <summary>
		/// The move next command
		/// </summary>
		private RelayCommand _moveNextCommand;
		/// <summary>
		/// The import complete command
		/// </summary>
		private RelayCommand _importCompleteCommand;
		/// <summary>
		/// The move previous command
		/// </summary>
		private RelayCommand _movePreviousCommand;
		/// <summary>
		/// The cancel command
		/// </summary>
		[SuppressMessage("ReSharper", "InconsistentNaming")] 
        protected RelayCommand _cancelCommand;

		//public event NextEventHandler NextEvent;
		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets the steps.
		/// </summary>
		/// <value>
		/// The steps.
		/// </value>
		public ReadOnlyCollection<CompleteStep<TWizardBusinessObject>> Steps
        {
            get
            {
                return new ReadOnlyCollection<CompleteStep<TWizardBusinessObject>>(_stepManager.Steps);
            }
        }



		/// <summary>
		/// Gets the event aggregator.
		/// </summary>
		/// <value>
		/// The event aggregator.
		/// </value>
		IEventAggregator EventAggregator { get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataWizardViewModel{TWizardBusinessObject}"/> class.
		/// </summary>
		public DataWizardViewModel()
        {
            _stepManager = new StepManager<TWizardBusinessObject>();
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            ImportCompleteEnabled = true;
        }

		#region Properties

		/// <summary>
		/// Guards the business object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onNull">The on null.</param>
		/// <param name="onElse">The on else.</param>
		/// <returns></returns>
		T GuardBusinessObject<T>(Func<T> onNull, Func<T> onElse)
        {
            if (BusinessObject == null)
            {
                return onNull();
            }
            return onElse();
        }

		/// <summary>
		/// Guards the business object.
		/// </summary>
		/// <param name="guarded">The guarded.</param>
		void GuardBusinessObject(Action guarded)
        {
            if (BusinessObject != null)
            {
                guarded();
            }
        }

		/// <summary>
		/// Gets the display name of the data type.
		/// </summary>
		/// <value>
		/// The display name of the data type.
		/// </value>
		public string DataTypeDisplayName
        {
            get
            {
                return GuardBusinessObject(() => string.Empty, () => BusinessObject.GetName());
            }
        }

		/// <summary>
		/// Gets the steps count.
		/// </summary>
		/// <value>
		/// The steps count.
		/// </value>
		public int StepsCount
        {
            get { return Steps.Count; }
        }

		/// <summary>
		/// The current index
		/// </summary>
		private int _currentIndex;
		/// <summary>
		/// Gets or sets the index of the current.
		/// </summary>
		/// <value>
		/// The index of the current.
		/// </value>
		public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                _currentIndex = value;
                OnPropertyChanged("CurrentIndex");
            }
        }

		/// <summary>
		/// The maximum progress
		/// </summary>
		private int _maxProgress;
		/// <summary>
		/// Gets or sets the maximum progress.
		/// </summary>
		/// <value>
		/// The maximum progress.
		/// </value>
		public int MaxProgress
        {
            get { return _stepManager.MaxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }
        }

		/// <summary>
		/// The maximum group progress
		/// </summary>
		private int _maxGroupProgress;
		/// <summary>
		/// Gets or sets the maximum group progress.
		/// </summary>
		/// <value>
		/// The maximum group progress.
		/// </value>
		public int MaxGroupProgress
        {
            get { return _maxGroupProgress; }
            set
            {
                _maxGroupProgress = value;
                OnPropertyChanged("MaxGroupProgress");
            }
        }

		/// <summary>
		/// The step groups
		/// </summary>
		private List<StepGroup> _stepGroups;
		/// <summary>
		/// Gets or sets the step groups.
		/// </summary>
		/// <value>
		/// The step groups.
		/// </value>
		public List<StepGroup> StepGroups
        {
            get { return _stepGroups; }
            set
            {
                _stepGroups = value;
                OnPropertyChanged("StepGroups");
            }
        }

		/// <summary>
		/// The current step group
		/// </summary>
		private StepGroup _currentStepGroup;
		/// <summary>
		/// Gets or sets the current step group.
		/// </summary>
		/// <value>
		/// The current step group.
		/// </value>
		public StepGroup CurrentStepGroup
        {
            get { return _currentStepGroup; }
            set
            {
                _currentStepGroup = value;
                OnPropertyChanged("CurrentStepGroup");
            }
        }

		/// <summary>
		/// The current group steps
		/// </summary>
		private List<CompleteStep<TWizardBusinessObject>> _currentGroupSteps;
		/// <summary>
		/// Gets or sets the current group steps.
		/// </summary>
		/// <value>
		/// The current group steps.
		/// </value>
		public List<CompleteStep<TWizardBusinessObject>> CurrentGroupSteps
        {
            get { return _currentGroupSteps; }
            set
            {
                _currentGroupSteps = value;
                OnPropertyChanged("CurrentGroupSteps");
            }
        }

		/// <summary>
		/// The current group index
		/// </summary>
		private int _currentGroupIndex;
		/// <summary>
		/// Gets or sets the index of the current group.
		/// </summary>
		/// <value>
		/// The index of the current group.
		/// </value>
		public int CurrentGroupIndex
        {
            get { return _currentGroupIndex; }
            set
            {
                _currentGroupIndex = value;
                OnPropertyChanged("CurrentGroupIndex");
            }
        }
		#endregion

		/// <summary>
		/// Gets the current linked list step.
		/// </summary>
		/// <value>
		/// The current linked list step.
		/// </value>
		public LinkedListNode<CompleteStep<TWizardBusinessObject>> CurrentLinkedListStep
        {
            get { return _stepManager.CurrentLinkedListStep; }
            private set
            {
                if (value == _stepManager.CurrentLinkedListStep)
                {
                    return;
                }

                ActionsOnCurrentLinkedListStep(value);

                OnPropertyChanged("CurrentLinkedListStep");
                OnPropertyChanged("IsOnLastStep");
            }
        }

		/// <summary>
		/// Provides the steps.
		/// </summary>
		/// <param name="stepCollection">The step collection.</param>
		public void ProvideSteps(StepCollection<TWizardBusinessObject> stepCollection)
        {
            BusinessObject = stepCollection.Context;

            _stepManager.ProvideSteps(stepCollection.GetAllSteps());

            StepGroups = stepCollection.Collection.Keys.ToList();
            MaxGroupProgress = StepGroups.Count * 2;

            ActionsOnCurrentLinkedListStep(_stepManager.FirstStep);
        }

		/// <summary>
		/// Actionses the on current linked list step.
		/// </summary>
		/// <param name="step">The step.</param>
		private void ActionsOnCurrentLinkedListStep(LinkedListNode<CompleteStep<TWizardBusinessObject>> step)
        {
            if (CurrentLinkedListStep != null)
            {
                CurrentLinkedListStep.Value.ViewModel.IsCurrentStep = false;
            }

            _stepManager.CurrentLinkedListStep = step;

            if (step != null)
            {
                step.Value.ViewModel.IsCurrentStep = true;
                step.Value.ViewModel.BeforeShow();

                foreach (var group in StepGroups)
                {
                    @group.IsCurrent = step.Value.GroupName == @group.DisplayName;
                }

                CurrentGroupIndex = (StepGroups.IndexOf(StepGroups.First(x => x.IsCurrent)) + 1) * 2 - 1;

                CurrentGroupSteps = Steps.Where(x => x.GroupName == step.Value.GroupName).ToList();
            }

            CurrentIndex = (_stepManager.CurrentIndex * 2) - 1; //(progress bar must stop in a center )
        }

		/// <summary>
		/// Cancels this instance.
		/// </summary>
		void Cancel()
        {
            var isCancelled = false;
            GuardBusinessObject(() => isCancelled = BusinessObject.Cancel());
            if (isCancelled)
            {
                RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView, UriKind.Relative));
            }
        }


		#region Commands
		/// <summary>
		/// Returns the command which, when executed, cancels the order and causes the Wizard to be removed from the user interface.
		/// </summary>
		/// <value>
		/// The cancel command.
		/// </value>
		public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); }
        }

		/// <summary>
		/// Returns the command which, when executed, causes the CurrentLinkedListStep
		/// property to reference the previous step in the workflow.
		/// </summary>
		/// <value>
		/// The move previous command.
		/// </value>
		public ICommand MovePreviousCommand
        {
            get {
                return _movePreviousCommand ??
                       (_movePreviousCommand =
                        new RelayCommand(MoveToPreviousStep, () => CanMoveToPreviousStep));
            }
        }

		/// <summary>
		/// Returns the command which, when executed, causes the CurrentLinkedListStep property to reference the next step in the workflow.  If the user
		/// is viewing the last step in the workflow, this causes the Wizard to finish and be removed from the user interface.
		/// </summary>
		/// <value>
		/// The move next command.
		/// </value>
		public ICommand MoveNextCommand
        {
            get
            {
                return _moveNextCommand ??
                       (_moveNextCommand = new RelayCommand(MoveToNextStep, () => CanMoveToNextStep));
            }
        }

		/// <summary>
		/// Gets the export data mapping command.
		/// </summary>
		/// <value>
		/// The export data mapping command.
		/// </value>
		public ICommand ExportDataMappingCommand
        {
            get
            {
                return _exportDataMappingCommand ??
                       (_exportDataMappingCommand = new RelayCommand(OnExportDataMapping, () => true));
            }
        }

		/// <summary>
		/// Called when [export data mapping].
		/// </summary>
		private void OnExportDataMapping()
        {
            var dataset = BusinessObject.GetDatasetItem();
            if (dataset == null || string.IsNullOrEmpty(dataset.SummaryData)) return;

            var mappingFileName = string.Format("{0}_Mapping_{1}.xml", dataset.ContentType.Name.Replace(" ", null),
                                                                       DateTime.Now.Date.ToShortDateString().Replace("/","_"));
            var tempPath = MonahrqContext.MappingFileExportDirPath;
            
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            var summaryDataDoc = new XmlDocument();
            summaryDataDoc.LoadXml(dataset.SummaryData);

            var mappingNode = summaryDataDoc.GetElementsByTagName("mappings").OfType<XmlNode>().FirstOrDefault();
            if (mappingNode == null || string.IsNullOrEmpty(mappingNode.OuterXml)) return;


            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = mappingFileName,
                AddExtension = true,
                DefaultExt = ".xml",
                InitialDirectory = tempPath
            };

            var mappingXml = new XmlDocument();
            mappingXml.LoadXml(mappingNode.OuterXml);

            if (saveFileDialog1.ShowDialog() == true)
            {
                Stream xmlStream;
                if ((xmlStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    var xr = XmlWriter.Create(xmlStream);
                    mappingXml.Save(xr);

                    xmlStream.Flush(); //Adjust this if you want read your data 
                    xmlStream.Position = 0;
                    xmlStream.Close();
                }
                xmlStream.Dispose();
            }
            saveFileDialog1 = null;
        }


		//TODO: ImportCompleteCommand change to WizardCompleteCommand
		/// <summary>
		/// Gets the import complete command.
		/// </summary>
		/// <value>
		/// The import complete command.
		/// </value>
		public ICommand ImportCompleteCommand
        {
            get
            {
                return _importCompleteCommand ?? 
                    (
                        _importCompleteCommand =
                            new RelayCommand(ImportCompleteStep,
                                () => CanMoveToImportCompleteStep) 
                    );
            }
        }

		/// <summary>
		/// Gets a value indicating whether this instance can move to previous step.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can move to previous step; otherwise, <c>false</c>.
		/// </value>
		bool CanMoveToPreviousStep
        {
            get { return CurrentLinkedListStep.Previous != null; }
        }

		/// <summary>
		/// Moves to previous step.
		/// </summary>
		void MoveToPreviousStep()
        {
            if (CanMoveToPreviousStep)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardBackEvent>().Publish(EventArgs.Empty);
                CurrentLinkedListStep = CurrentLinkedListStep.Previous;
                //CurrentLinkedListStep.Value.ViewModel.BeforeShow();
            }
        }

		/// <summary>
		/// Imports the complete step.
		/// </summary>
		void ImportCompleteStep()
        {
            // Load the Load Data view
            RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView, UriKind.Relative));
        }

		/// <summary>
		/// Gets a value indicating whether this instance can move to next step.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can move to next step; otherwise, <c>false</c>.
		/// </value>
		bool CanMoveToNextStep
        {
            get
            {
                var step = CurrentLinkedListStep;
                var result = (step != null) && (step.Value.ViewModel.IsValid()) && (step.Next != null);
                return result;
            }
        }

		/// <summary>
		/// Gets a value indicating whether this instance can move to import complete step.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can move to import complete step; otherwise, <c>false</c>.
		/// </value>
		public bool CanMoveToImportCompleteStep
        {
            get
            {
                var step = CurrentLinkedListStep;
                return (step != null) && (step.Value.ViewModel.IsValid()) && (step.Next == null);
            }
        }

		/// <summary>
		/// Gets the move next visible.
		/// </summary>
		/// <value>
		/// The move next visible.
		/// </value>
		public Visibility MoveNextVisible
        {
            get
            {
                var step = CurrentLinkedListStep;
                return ((step != null) && (step.Next != null)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

		/// <summary>
		/// Gets the import complete visible.
		/// </summary>
		/// <value>
		/// The import complete visible.
		/// </value>
		public Visibility ImportCompleteVisible
        {
            get
            {
                var step = CurrentLinkedListStep;
                return ((step != null) && (step.Next == null)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
		// ExportDataMappingCommand
		/// <summary>
		/// Gets the mapping enabled visible.
		/// </summary>
		/// <value>
		/// The mapping enabled visible.
		/// </value>
		public Visibility MappingEnabledVisible
        {
            get
            {
                var dataset = BusinessObject.GetDatasetItem();
                if(dataset == null || dataset.ContentType == null) return Visibility.Collapsed;

                if (dataset.ContentType.IsCustom)
                {
                    if (dataset.ContentType.ImportType == null)
                        dataset.ContentType = BusinessObject.RefreshTarget(dataset.ContentType);

                    if(ImportCompleteVisible == Visibility.Visible && dataset.ContentType.ImportType == DynamicStepTypeEnum.Simple)
                        return  Visibility.Collapsed;

                    if(ImportCompleteVisible == Visibility.Visible && dataset.ContentType.ImportType == DynamicStepTypeEnum.Full)
                        return Visibility.Visible;
                }

                if (ImportCompleteVisible == Visibility.Visible)
                {
                    return !dataset.ContentType.Name.In(new[] {"Inpatient Discharge", "ED Treat And Release"})
                                ? Visibility.Collapsed
                                : Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

		/// <summary>
		/// Gets the show back visibility.
		/// </summary>
		/// <value>
		/// The show back visibility.
		/// </value>
		public Visibility ShowBackVisibility
        {
            get
            {
                //return _stepManager.CurrentIndex != 1 ? Visibility.Visible : Visibility.Collapsed;
                var step = CurrentLinkedListStep;
                return ((step != null) && (step.Previous != null)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether [import complete enabled].
		/// </summary>
		/// <value>
		/// <c>true</c> if [import complete enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool ImportCompleteEnabled
        {
            get;
            set;
            //{
            //    //var dataset = BusinessObject.GetDatasetItem();

            //    //var result = ImportCompleteVisible != Visibility.Visible && CanMoveToImportCompleteStep;

            //    //return !CanMoveToImportCompleteStep;

            //    var step = CurrentLinkedListStep;
            //    return ((step != null) && step.Value.ViewModel.IsValid() && _stepManager.StepsLeft == 0) == false;
            //}
        }

		/// <summary>
		/// Note that currently, the step OnNext handler is only called when moving next; not when moving previous.
		/// </summary>
		void MoveToNextStep()
        {
            if (!CanMoveToNextStep) return;

            _stepManager.ReworkListBasedOn(CurrentLinkedListStep.Value.ViewModel.OnNext());
            CurrentLinkedListStep = CurrentLinkedListStep.Next;
            //CurrentLinkedListStep.Value.ViewModel.BeforeShow();
            CurrentLinkedListStep.Value.Visited = true;
        }

		#endregion


		/// <summary>
		/// Returns true if the user is currently viewing the last step in the workflow.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is on last step; otherwise, <c>false</c>.
		/// </value>
		public bool IsOnLastStep
        {
            get { return CurrentLinkedListStep.Next == null; }
        }

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

		/// <summary>
		/// The navigate request token
		/// </summary>
		private SubscriptionToken _navigateRequestToken;
		/// <summary>
		/// Attaches this instance.
		/// </summary>
		public void Attach()
        {
            Detach();
            _navigateRequestToken = EventAggregator
                .GetEvent<WizardRequestNavigateEvent<TWizardBusinessObject>>().Subscribe(NavigateTo);

            EventAggregator.GetEvent<DisableWizardButtons>().Subscribe(OnDisableWizardButtons);
        }

		/// <summary>
		/// Called when [disable wizard buttons].
		/// </summary>
		/// <param name="result">if set to <c>true</c> [result].</param>
		private void OnDisableWizardButtons(bool result)
        {
            ImportCompleteEnabled = !result;
            OnPropertyChanged("ImportCompleteEnabled");
        }

		/// <summary>
		/// Detaches this instance.
		/// </summary>
		public void Detach()
        {
            var token = Interlocked.Exchange(ref _navigateRequestToken, null);
            if (token != null)
            {
                EventAggregator
                    .GetEvent<WizardRequestNavigateEvent<TWizardBusinessObject>>().Unsubscribe(token);
            }
        }

		/// <summary>
		/// Navigates to.
		/// </summary>
		/// <param name="payload">The <see cref="WizardRequestNavigateEventArgs{TWizardBusinessObject}"/> instance containing the event data.</param>
		private void NavigateTo(WizardRequestNavigateEventArgs<TWizardBusinessObject> payload)
        {}
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Boolean}" />
	public class DisableWizardButtons : CompositePresentationEvent<bool> { }
}
