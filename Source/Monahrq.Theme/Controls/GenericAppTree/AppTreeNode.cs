using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Monahrq.Theme.Controls.GenericAppTree
{
	/// <summary>
	/// A sub node of the AppTreeContainer control.
	/// </summary>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	public abstract class AppTreeNode : INotifyPropertyChanged
    {
		/// <summary>
		/// The group
		/// </summary>
		protected string _group;
		/// <summary>
		/// The is selected
		/// </summary>
		private bool _isSelected;

		/// <summary>
		/// Initializes a new instance of the <see cref="AppTreeNode"/> class.
		/// </summary>
		/// <param name="selectionGroup">The selection group.</param>
		/// <param name="header">The header.</param>
		public AppTreeNode(string selectionGroup, string header)
        {
            _group = selectionGroup;
            Header = header;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="AppTreeNode"/> class.
		/// </summary>
		/// <param name="selectionGroup">The selection group.</param>
		/// <param name="header">The header.</param>
		/// <param name="icon">The icon.</param>
		public AppTreeNode(string selectionGroup, string header, Image icon)
        {
            _group = selectionGroup;
            Header = header;

            if (icon == null) return;
            using (var memory = new MemoryStream())
            {
                icon.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = memory;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                Icon = bmi;
            }
        }

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>
		/// The icon.
		/// </value>
		public ImageSource Icon
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>
		/// The parent.
		/// </value>
		public AppTreeContainer Parent { get; set; }

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>
		/// The header.
		/// </value>
		public string Header { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected 
        { 
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");

                if (value) OnSelectionChanged(this);
            }
        }

		/// <summary>
		/// Occurs when [selection changed].
		/// </summary>
		public event Action<AppTreeNode> SelectionChanged;

		/// <summary>
		/// Gets or sets the data context.
		/// </summary>
		/// <value>
		/// The data context.
		/// </value>
		public object DataContext { get; set; }

		/// <summary>
		/// Gets the group.
		/// </summary>
		/// <value>
		/// The group.
		/// </value>
		public string Group
        {
            get
            {
                return _group;
            }
        }

		/// <summary>
		/// Selects this instance.
		/// </summary>
		public void Select()
        {
            IsSelected = true;
            OnSelectionChanged(this);

            if (Parent != null) Parent.Select();
        }

		/// <summary>
		/// Called when [selection changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		protected void OnSelectionChanged(AppTreeNode sender)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(sender);
            }
        }

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="name">The name.</param>
		protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
    }
}