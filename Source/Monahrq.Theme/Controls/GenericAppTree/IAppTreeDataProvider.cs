using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Theme.Controls.GenericAppTree
{
	/// <summary>
	/// Interface for an AppTree data provider.
	/// </summary>
	public interface IAppTreeDataProvider
    {
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		string Name { get; }
		/// <summary>
		/// Gets the path.
		/// </summary>
		/// <value>
		/// The path.
		/// </value>
		string Path { get; }
		/// <summary>
		/// Gets the icon.
		/// </summary>
		/// <value>
		/// The icon.
		/// </value>
		Bitmap Icon { get; }

		/// <summary>
		/// Gets a value indicating whether this instance can be displayed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can be displayed; otherwise, <c>false</c>.
		/// </value>
		bool CanBeDisplayed { get; }
    }
}
