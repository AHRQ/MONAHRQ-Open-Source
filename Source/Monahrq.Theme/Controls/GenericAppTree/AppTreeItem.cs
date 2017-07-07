using System.Drawing;

namespace Monahrq.Theme.Controls.GenericAppTree
{
	/// <summary>
	/// A sub node of the AppTreeContainer control.  Holds items.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.GenericAppTree.AppTreeNode" />
	public class AppTreeItem : AppTreeNode
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="AppTreeItem"/> class.
		/// </summary>
		/// <param name="selectionGroup">The selection group.</param>
		/// <param name="header">The header.</param>
		/// <param name="dataContext">The data context.</param>
		public AppTreeItem(string selectionGroup, string header, object dataContext)
            : base(selectionGroup, header)
        {
            DataContext = dataContext;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="AppTreeItem"/> class.
		/// </summary>
		/// <param name="selectionGroup">The selection group.</param>
		/// <param name="header">The header.</param>
		/// <param name="dataContext">The data context.</param>
		/// <param name="icon">The icon.</param>
		public AppTreeItem(string selectionGroup, string header, object dataContext, Bitmap icon)
            : base(selectionGroup, header, icon)
        {
            DataContext = dataContext;
        }
    }
}