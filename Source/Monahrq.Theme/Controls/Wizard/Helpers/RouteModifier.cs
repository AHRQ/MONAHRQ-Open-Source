using System;
using System.Collections.Generic;

namespace Monahrq.Theme.Controls.Wizard.Helpers
{
	/// <summary>
	/// Used to modify where the wizard routes to.
	/// </summary>
	public class RouteModifier
    {
		/// <summary>
		/// Gets or sets the exclude view types.
		/// </summary>
		/// <value>
		/// The exclude view types.
		/// </value>
		public List<Type> ExcludeViewTypes { get; set; }
		/// <summary>
		/// Gets or sets the include view types.
		/// </summary>
		/// <value>
		/// The include view types.
		/// </value>
		public List<Type> IncludeViewTypes { get; set; }

		/// <summary>
		/// Adds to exclude views.
		/// </summary>
		/// <param name="viewTypes">The view types.</param>
		public void AddToExcludeViews( List<Type> viewTypes )
        {
            if ( ExcludeViewTypes == null )
            {
                ExcludeViewTypes = viewTypes;
            }
            else
            {
                ExcludeViewTypes.AddRange( viewTypes );
            }
        }

		/// <summary>
		/// Adds to include views.
		/// </summary>
		/// <param name="viewTypes">The view types.</param>
		public void AddToIncludeViews( List<Type> viewTypes )
        {
            if ( IncludeViewTypes == null )
            {
                IncludeViewTypes = viewTypes;
            }
            else
            {
                IncludeViewTypes.AddRange( viewTypes );
            }
        }

    }
}
