using System.Collections.ObjectModel;
using System.Windows;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Websites.ViewModels
{

    public interface ITreeViewItemViewModel 
    {
        ObservableCollection<ITreeViewItemViewModel> ChildrenCollectionView { get; }
        bool IsSelected { get; set; }
        Visibility Visibility { get; set; }
        ITreeViewItemViewModel Parent { get; }
        Topic Entity { get; }
        TopicCategory TopicCategory { get; }
    }

}
