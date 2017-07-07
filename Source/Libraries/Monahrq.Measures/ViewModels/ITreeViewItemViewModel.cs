using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Measures.ViewModels
{

    /// <summary>
    /// Interface for Tree view Item model
    /// </summary>
    public interface ITreeViewItemViewModel 
    {
        /// <summary>
        /// Gets the children collection view.
        /// </summary>
        /// <value>
        /// The children collection view.
        /// </value>
        ObservableCollection<ITreeViewItemViewModel> ChildrenCollectionView { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        bool IsSelected { get; set; }
        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        /// <value>
        /// The visibility.
        /// </value>
        Visibility Visibility { get; set; }
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        ITreeViewItemViewModel Parent { get; }
        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        Topic Entity { get; }
        /// <summary>
        /// Gets the topic category.
        /// </summary>
        /// <value>
        /// The topic category.
        /// </value>
        TopicCategory TopicCategory { get; }
    }

}
