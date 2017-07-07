using System;
using PropertyChanged;

namespace Monahrq.Theme.Controls.Tab
{
	/// <summary>
	/// ViewModel TabItem controls.
	/// </summary>
	[ImplementPropertyChanged]
    public class TabItemViewModel
    {
		/// <summary>
		/// Gets or sets the base title.
		/// </summary>
		/// <value>
		/// The base title.
		/// </value>
		public string BaseTitle { get; set; }
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>
		/// The title.
		/// </value>
		public string Title { get; set; }
		/// <summary>
		/// Gets or sets the data context.
		/// </summary>
		/// <value>
		/// The data context.
		/// </value>
		public Object DataContext { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected { get; set; }
		/// <summary>
		/// Updates the title.
		/// </summary>
		/// <param name="count">The count.</param>
		public void UpdateTitle(int count)
        {
            Title = BaseTitle + " (" + count + ")";
        }
    }
}
