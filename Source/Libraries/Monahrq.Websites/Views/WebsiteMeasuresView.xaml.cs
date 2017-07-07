using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Websites;
using System.Linq;

namespace Monahrq.Websites.Views
{
	/// <summary>
	/// Interaction logic for ManageMeasuresView.xaml
	/// </summary>
	[ViewExport(typeof(WebsiteMeasuresView), RegionName = RegionNames.WebsiteManageRegion)]
	public partial class WebsiteMeasuresView : IWebsiteMeasuresView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteMeasuresView"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		public WebsiteMeasuresView()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>
		/// The model.
		/// </value>
		[Import]
		public WebsiteMeasuresViewModel Model
		{
			get
			{
				return DataContext as WebsiteMeasuresViewModel;
			}
			set
			{
				DataContext = value;
			}
		}
        private int _lastCheckedItemIndex { get; set; }

        private void cbSelectionMeasures_KeyUp(object sender, KeyEventArgs e)
        {
            CheckBox cbSelectionMeasures = sender as CheckBox;
            WebsiteMeasure newCheckedItem = cbSelectionMeasures.DataContext as WebsiteMeasure;

            int newCheckedItemIndex = Model.AvailableMeasuresView.IndexOf(newCheckedItem);

            if ((System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightShift)) && newCheckedItemIndex != _lastCheckedItemIndex)
            {
                int start = System.Math.Min(_lastCheckedItemIndex, newCheckedItemIndex);
                int end = System.Math.Max(_lastCheckedItemIndex, newCheckedItemIndex);

                int countToTake = end - start;

                var itemsToCheck = new System.Collections.Generic.List<WebsiteMeasure>();

                foreach (var o in Model.AvailableMeasuresView.OfType<WebsiteMeasure>().Select((x, i) => new { x, i }))
                {
                    if (o.i > start && o.i < end)
                        itemsToCheck.Add(o.x);
                }

                foreach (WebsiteMeasure itemToCheck in itemsToCheck)
                {
                    itemToCheck.IsSelected = true;
                }
                _lastCheckedItemIndex = -1;
            }
            else
            {
                _lastCheckedItemIndex = newCheckedItemIndex;
            }
        }
    
    }
}
