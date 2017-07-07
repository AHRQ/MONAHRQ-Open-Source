using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monahrq.Theme.Controls
{

	/// <summary>
	/// Individual TabItem Control for a TabControl.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.TabItem" />
	public class MonahrqTabItem : TabItem
    {
		/// <summary>
		/// Initializes the <see cref="MonahrqTabItem"/> class.
		/// </summary>
		static MonahrqTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonahrqTabItem), new FrameworkPropertyMetadata(typeof(MonahrqTabItem)));
        }

		#region RegionName

		/// <summary>
		/// RegionName Dependency Property
		/// </summary>
		public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.Register("RegionName", typeof(string), typeof(MonahrqTabItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnRegionNameChanged)));

		/// <summary>
		/// Gets or sets the RegionName property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The name of the region.
		/// </value>
		public string RegionName
        {
            get { return (string)GetValue(RegionNameProperty); }
            set { SetValue(RegionNameProperty, value); }
        }

		/// <summary>
		/// Handles changes to the RegionName property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MonahrqTabItem target = (MonahrqTabItem)d;
            string oldRegionName = (string)e.OldValue;
            string newRegionName = target.RegionName;
            //RegionManager.SetRegionName(target, newRegionName);
            target.OnRegionNameChanged(oldRegionName, newRegionName);
        }

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the RegionName property.
		/// </summary>
		/// <param name="oldRegionName">Old name of the region.</param>
		/// <param name="newRegionName">New name of the region.</param>
		protected virtual void OnRegionNameChanged(string oldRegionName, string newRegionName)
        {
        }

        #endregion




    }
}
