using System.Windows.Controls.Primitives;

namespace Monahrq.Theme.Controls.MultiSelect
{
    public interface IMultiSelectCollectionView
    {
        void AddControl(Selector selector);
        void RemoveControl(Selector selector);
        void SetSelectOnly(bool isSelectOnly);
        void DeselectAll();
        void SelectAll();
    }
}
