using System.Collections.ObjectModel;
using Monahrq.Theme.Controls.Tab;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Interface for measires view model
    /// </summary>
    public interface IMeasuresViewModel
    {
        /// <summary>
        /// Loads the tab items.
        /// </summary>
        void LoadTabItems();
        /// <summary>
        /// Gets or sets the collection of tab items.
        /// </summary>
        /// <value>
        /// The collection of tab items.
        /// </value>
        ObservableCollection<TabItemViewModel> TabItems { get; set; }
    }
}