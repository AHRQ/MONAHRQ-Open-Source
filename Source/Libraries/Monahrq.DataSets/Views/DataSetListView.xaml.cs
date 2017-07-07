using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.ViewModels;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using System.Windows;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DataSetListView.xaml
    /// </summary>
    [ViewExport(typeof(DataSetListView), RegionName = RegionNames.DataSetsRegion)]
    public partial class DataSetListView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetListView"/> class.
        /// </summary>
        public DataSetListView()
		{
			InitializeComponent();
			this.DataContextChanged += (o, e) =>
			{
				Unsubscribe(e.OldValue as DataSetListViewModel);
				Subscribe(e.NewValue as DataSetListViewModel);
			};
		}

        /// <summary>
        /// Gets or sets the internal events.
        /// </summary>
        /// <value>
        /// The internal events.
        /// </value>
        [Import]
        IEventAggregator InternalEvents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the request current data type model event token.
        /// </summary>
        /// <value>
        /// The request current data type model event token.
        /// </value>
        SubscriptionToken RequestCurrentDataTypeModelEventToken { get; set; }

        /// <summary>
        /// Subscribes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Subscribe(DataSetListViewModel model)
        {
            if (model == null) return;

            Unsubscribe(null);
            RequestCurrentDataTypeModelEventToken =
                InternalEvents.GetEvent<Events.RequestCurrentDataTypeModelEvent>().Subscribe(
                request =>
                {
                    request.Data = Model.SelectedDataType;
                    request.DataId = Model.SelectedDataSet != null ? Model.SelectedDataSet.Dataset.Id : (int?) null;
                });

            model.Subscribe();
        }

        /// <summary>
        /// Unsubscribes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Unsubscribe(DataSetListViewModel model)
        {
            if (model == null) return;

            model.Unsubscribe();
            if (RequestCurrentDataTypeModelEventToken != null)
            {
                InternalEvents.GetEvent<Events.RequestCurrentDataTypeModelEvent>().Unsubscribe(RequestCurrentDataTypeModelEventToken);
            }
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        DataSetListViewModel Model
        {
            get
            {
                return DataContext as DataSetListViewModel;
            }
            set { DataContext = value; }
        }
    }
}
