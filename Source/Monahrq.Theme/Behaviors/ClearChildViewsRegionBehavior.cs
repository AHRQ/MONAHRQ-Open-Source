using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior for clearing child view regions.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.RegionBehavior" />
	public class ClearChildViewsRegionBehavior : RegionBehavior
    {
		/// <summary>
		/// The behavior key
		/// </summary>
		public const string BEHAVIOR_KEY = "ClearChildViews";

		/// <summary>
		/// Override this method to perform the logic after the behavior has been attached.
		/// </summary>
		protected override void OnAttach()
        {
            Region.PropertyChanged += Region_PropertyChanged;
        }

		/// <summary>
		/// Handles the PropertyChanged event of the Region control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		void Region_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "RegionManager") return;

            if (Region.RegionManager != null) return;

            foreach (var dependencyObject in Region.Views.OfType<DependencyObject>().ToList())
                dependencyObject.ClearValue(RegionManager.RegionManagerProperty);
        }
    }
}