using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Annotations;
using Monahrq.DataSets.Events;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The data type details view model. This class holds the dataset types allowed in monahrq and the associated completed
    /// dataset meta data per the dataset type.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [Export]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DataTypeDetailsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        private IRegionManager RegionManager { get; set; }
        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        private IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the export mapping command.
        /// </summary>
        /// <value>
        /// The export mapping command.
        /// </value>
        public ICommand ExportMappingCommand { get; set; }
        /// <summary>
        /// Gets or sets the reimport mapping command.
        /// </summary>
        /// <value>
        /// The reimport mapping command.
        /// </value>
        public ICommand ReimportMappingCommand { get; set; }
        /// <summary>
        /// Gets or sets the delete record command.
        /// </summary>
        /// <value>
        /// The delete record command.
        /// </value>
        public ICommand DeleteRecordCommand { get; set; }
        /// <summary>
        /// Gets or sets the process record command.
        /// </summary>
        /// <value>
        /// The process record command.
        /// </value>
        public ICommand ProcessRecordCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeDetailsViewModel"/> class.
        /// </summary>
        public DataTypeDetailsViewModel()
        {
            RegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            DeleteRecordCommand = new DelegateCommand<DataTypeDetailsViewModel>(DeleteRecord, CanDelete);
            ProcessRecordCommand = new DelegateCommand<DataTypeDetailsViewModel>(ProcessDataserRecord, delegate { return true; });
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        /// <summary>
        /// Processes the dataser record.
        /// </summary>
        /// <param name="item">The item.</param>
        private void ProcessDataserRecord(DataTypeDetailsViewModel item)
        {
            if (item.Entry.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))
            {
                EventAggregator.GetEvent<ProcessDrgMdsDatasetInfoEvent>().Publish(item.Entry);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeDetailsViewModel"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public DataTypeDetailsViewModel(Dataset entry)
            : this()
        {
            Entry = entry;
            EnableGrouperProcessing = Entry.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Completed &&
                                      Entry.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.InProgress &&
                                      Entry.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Intializing;
        }

        #region Commands
        /// <summary>
        /// Determines whether this instance can delete the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified argument; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDelete(object arg)
        {
            return true;
        }

        /// <summary>
        /// Deletes the record.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DeleteRecord(DataTypeDetailsViewModel obj)
        {
            EventAggregator.GetEvent<DeleteEntryEvent>().Publish(new DeleteEntryEventArg()
            {
                Dataset = obj.Entry,
                ShowUserPrompt = true
            });
        }

        #endregion

        /// <summary>
        /// Gets or sets the entry.
        /// </summary>
        /// <value>
        /// The entry.
        /// </value>
        public Dataset Entry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable grouper processing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable grouper processing]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableGrouperProcessing { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is currently processing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is currently processing; otherwise, <c>false</c>.
        /// </value>
        public bool IsCurrentlyProcessing
        {
            get
            {
				//  TODO: Is this the right place for this?
				//  Or should be handed in XAML as a multibinding
				if (!Entry.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")) return false;

                return Entry.DRGMDCMappingStatus == DrgMdcMappingStatusEnum.InProgress ||
                       Entry.DRGMDCMappingStatus == DrgMdcMappingStatusEnum.Intializing;
            }
        }

        /// <summary>
        /// Gets the can process DRG.
        /// </summary>
        /// <value>
        /// The can process DRG.
        /// </value>
        public Visibility CanProcessDRG { get { return Entry.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge") ? Visibility.Visible : Visibility.Hidden; } }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }

}
