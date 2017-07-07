using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;

namespace Monahrq.Theme.Controls.GenericAppTree
{
	/// <summary>
	/// AppTree control.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.GenericAppTree.AppTreeNode" />
	public class AppTreeContainer : AppTreeNode
    {
		/// <summary>
		/// The children
		/// </summary>
		private readonly ObservableCollection<AppTreeNode> _children;
		/// <summary>
		/// The enable preview
		/// </summary>
		private readonly bool _enablePreview;

		/// <summary>
		/// The preview child
		/// </summary>
		private AppTreeNode _previewChild;

		/// <summary>
		/// Initializes a new instance of the <see cref="AppTreeContainer"/> class.
		/// </summary>
		/// <param name="selectionGroup">The selection group.</param>
		/// <param name="header">The header.</param>
		/// <param name="enablePreview">if set to <c>true</c> [enable preview].</param>
		public AppTreeContainer(string selectionGroup, string header, bool enablePreview = false)
            : base(selectionGroup, header)
        {
            _children = new ObservableCollection<AppTreeNode>();
            _enablePreview = enablePreview;

            if(_enablePreview)
            {
                PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != "IsSelected") return;
                    
                    if(!IsSelected)
                    {
                        foreach (var item in Children)
                        {
                            item.IsSelected = false;
                        }
                    }

                    UpdatePreview();
                };
            }
        }

		/// <summary>
		/// Gets the container children.
		/// </summary>
		/// <value>
		/// The container children.
		/// </value>
		public IEnumerable<AppTreeContainer> ContainerChildren
        {
            get
            {
                return _children.OfType<AppTreeContainer>();
            }
        }

		/// <summary>
		/// Gets the item children.
		/// </summary>
		/// <value>
		/// The item children.
		/// </value>
		public IEnumerable<AppTreeItem> ItemChildren
        {
            get
            {
                return _children.OfType<AppTreeItem>();
            }
        }

		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <value>
		/// The children.
		/// </value>
		public IEnumerable<AppTreeNode> Children
        {
            get
            {
                return _children;
            }
        }

		/// <summary>
		/// Adds the child.
		/// </summary>
		/// <param name="item">The item.</param>
		public void AddChild(AppTreeNode item)
        {
            item.SelectionChanged += OnSelectionChanged;
            item.Parent = this;
            _children.Add(item);

            if (_enablePreview) UpdatePreview();
        }

		/// <summary>
		/// Removes the child.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool RemoveChild(AppTreeNode item)
        {
            item.SelectionChanged -= OnSelectionChanged; 
            item.Parent = null;
            var result = _children.Remove(item);

            if (_enablePreview) UpdatePreview();
            return result;
        }

		/// <summary>
		/// Updates the preview.
		/// </summary>
		private void UpdatePreview()
        {
            if (_previewChild != null) _previewChild.PropertyChanged -= PreviewPropertyChanged;
            
            if(_children.Count == 0)
            {
                Icon = null;
                OnPropertyChanged("Icon");

                return;
            }

            _previewChild = _children.First();
            _previewChild.PropertyChanged += PreviewPropertyChanged;

            if (IsSelected)
            {
                Icon = null;
                OnPropertyChanged("Icon");
            }
            else
            {
                Icon = _previewChild.Icon;
                OnPropertyChanged("Icon");
            }
        }

		/// <summary>
		/// Previews the property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
		private void PreviewPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Icon") UpdatePreview();
        }
    }
}