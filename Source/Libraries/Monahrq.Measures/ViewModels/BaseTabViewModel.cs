using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.ViewModels;
using Monahrq.Measures.Service;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Microsoft.Practices.ServiceLocation;
using System.ComponentModel.Composition;
using System.ComponentModel;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Class for measures Tab
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    /// <seealso cref="Monahrq.Measures.ViewModels.ITabViewModel" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [ImplementPropertyChanged]
    public abstract class BaseTabViewModel : BaseViewModel, ITabViewModel, IPartImportsSatisfiedNotification
    {
        #region Consts        
        /// <summary>
        /// The label text for manage measures
        /// </summary>
        public const string MANAGE_MEASURES = "Manage Measures";
        /// <summary>
        /// The label text for manage topics
        /// </summary>
        public const string MANAGE_TOPICS = "Manage Topics";


        /// <summary>
        /// Label text for All datasets
        /// </summary>
        public const string ALL_DATASETS = "All Datasets";
        /// <summary>
        /// The label text for cancel
        /// </summary>
        public const string CANCEL = "Cancel";
        /// <summary>
        /// The label text for save
        /// </summary>
        public const string SAVE = "SAVE";

        #endregion

        /// <summary>
        /// Gets or sets the base title.
        /// </summary>
        /// <value>
        /// The base title.
        /// </value>
        public string BaseTitle { get; set; }
        /// <summary>
        /// Gets the tab title.
        /// </summary>
        /// <value>
        /// The tab title.
        /// </value>
        public string TabTitle { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is tab selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tab selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsTabSelected { get; set; }
        /// <summary>
        /// Gets or sets the Measures UI.
        /// </summary>
        /// <value>
        /// The UI.
        /// </value>
        public MeasuresUIModel UI { get; set; }

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the measures service.
        /// </summary>
        /// <value>
        /// The measures service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IMeasureServiceSync MeasuresService { get; set; }


        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected abstract void InitCommands();
        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected abstract void InitProperties();
        /// <summary>
        /// Initializes the data.
        /// </summary>
        protected abstract void InitData();



        /// <summary>
        /// Called when imports satisfied that is when import is valid.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            UI = new MeasuresUIModel();
            InitCommands();
            InitProperties();
        }
    }

    //todo move all string to resource file
    public static class ModelPropertyFilterValues
    {
        public const string NONE = "None";
        public const string MEASURE_CODE = "Measure Code";
        public const string MEASURE_NAME = "Measure Name";
        public const string WEBSITE_NAME = "Website name";
        public const string TOPIC_NAME = "Topic name";
        public const string SUBTOPIC_NAME = "Subtopic name";
    }
}
