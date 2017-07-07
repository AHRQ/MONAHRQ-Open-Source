using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Theme.Controls.Pagination
{
	/// <summary>
	/// Paging control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.Control" />
	[TemplatePart(Name = "PART_FirstPageButton", Type = typeof(Button)),
     TemplatePart(Name = "PART_PreviousPageButton", Type = typeof(Button)),
     TemplatePart(Name = "PART_NextPageButton", Type = typeof(Button)),
     TemplatePart(Name = "PART_LastPageButton", Type = typeof(Button))]
    public class PagingControl : Control
    {
		#region Constructors and Load/Unload events

		/// <summary>
		/// Initializes the <see cref="PagingControl"/> class.
		/// </summary>
		static PagingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PagingControl),
                new FrameworkPropertyMetadata(typeof(PagingControl)));
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="PagingControl"/> class.
		/// </summary>
		public PagingControl()
        {
            Loaded += PagingControl_Loaded;
            Unloaded += PagingControl_Unloaded;
        }

		/// <summary>
		/// Handles the Loaded event of the PagingControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		/// <exception cref="Exception">
		/// PagingControl template not assigned.
		/// or
		/// Paging DataGrid not assigned.
		/// </exception>
		private void PagingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Template == null) throw new Exception("PagingControl template not assigned.");
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (PagingDataGrid == null) throw new Exception("Paging DataGrid not assigned.");
            }

            RegisterButtonsEvents();
        }

		/// <summary>
		/// Handles the Unloaded event of the PagingControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void PagingControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UnRegisterButtonsEvents();
        }



		#endregion

		#region Button Events

		/// <summary>
		/// Registers the buttons events.
		/// </summary>
		private void RegisterButtonsEvents()
        {
            if (_firstPageBtn != null) _firstPageBtn.Click -= OnFirstPageBtnClicked;
            if (_firstPageBtn != null) _firstPageBtn.Click += OnFirstPageBtnClicked;
            if (_nextPageBtn != null) _nextPageBtn.Click -= OnNextPageBtnClicked;
            if (_nextPageBtn != null) _nextPageBtn.Click += OnNextPageBtnClicked;
            if (_previousPageBtn != null) _previousPageBtn.Click -= OnPreviousPageBtnClicked;
            if (_previousPageBtn != null) _previousPageBtn.Click += OnPreviousPageBtnClicked;
            if (_lastPageBtn != null) _lastPageBtn.Click -= OnLastPageBtnClicked;
            if (_lastPageBtn != null) _lastPageBtn.Click += OnLastPageBtnClicked;
        }


		/// <summary>
		/// Called when [first page BTN clicked].
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		public void OnFirstPageBtnClicked(object s, RoutedEventArgs e)
        {
            GetPage(PageRequest.FirstPage);
        }

		/// <summary>
		/// Called when [next page BTN clicked].
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		public void OnNextPageBtnClicked(object s, RoutedEventArgs e)
        {
            GetPage(PageRequest.NextPage);
        }

		/// <summary>
		/// Called when [previous page BTN clicked].
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		public void OnPreviousPageBtnClicked(object s, RoutedEventArgs e)
        {
            GetPage(PageRequest.PreviousPage);
        }

		/// <summary>
		/// Called when [last page BTN clicked].
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		public void OnLastPageBtnClicked(object s, RoutedEventArgs e)
        {
            GetPage(PageRequest.LastPage);
        }

		/// <summary>
		/// Gets the page.
		/// </summary>
		/// <param name="pageRequest">The page request.</param>
		public void GetPage(PageRequest pageRequest)
        {
            if (PagingArguments == null) return;
            switch (pageRequest)
            {
                case PageRequest.FirstPage:
                    PagingArguments.PageIndex = 1;
                    break;
                case PageRequest.NextPage:
                    PagingArguments.PageIndex = Math.Min(PagingArguments.PageIndex + 1,
                        PagingArguments.TotalPages);
                    break;
                case PageRequest.PreviousPage:
                    PagingArguments.PageIndex = Math.Max(PagingArguments.PageIndex - 1, 1);
                    break;
                case PageRequest.LastPage:
                    PagingArguments.PageIndex = PagingArguments.TotalPages;
                    break;
            }
        }

		/// <summary>
		/// Executes the pagination command.
		/// </summary>
		private void ExecutePaginationCommand()
        {
            var ipoc = PagingArguments;

            if (ipoc == null) return;

            ipoc.PagingFunction();
        }

		/// <summary>
		/// Uns the register buttons events.
		/// </summary>
		private void UnRegisterButtonsEvents()
        {
            if (_firstPageBtn != null) _firstPageBtn.Click -= OnFirstPageBtnClicked;
            if (_nextPageBtn != null) _nextPageBtn.Click -= OnNextPageBtnClicked;
            if (_previousPageBtn != null) _previousPageBtn.Click -= OnPreviousPageBtnClicked;
            if (_lastPageBtn != null) _lastPageBtn.Click -= OnLastPageBtnClicked;
        }

		#endregion

		#region get Paging parts

		/// <summary>
		/// The first page BTN
		/// </summary>
		private Button _firstPageBtn;
		/// <summary>
		/// The next page BTN
		/// </summary>
		private Button _nextPageBtn;
		/// <summary>
		/// The previous page BTN
		/// </summary>
		private Button _previousPageBtn;
		/// <summary>
		/// The last page BTN
		/// </summary>
		private Button _lastPageBtn;

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
		/// </summary>
		public override void OnApplyTemplate()
        {
            _firstPageBtn = Template.FindName("PART_FirstPageButton", this) as Button;
            _nextPageBtn = Template.FindName("PART_NextPageButton", this) as Button;
            _previousPageBtn = Template.FindName("PART_PreviousPageButton", this) as Button;
            _lastPageBtn = Template.FindName("PART_LastPageButton", this) as Button;

            base.OnApplyTemplate();
        }

		#endregion

		#region Dependency Properties and Commands

		#region PagingDataGrid

		/// <summary>
		/// PagingDataGrid Dependency Property
		/// </summary>
		public static readonly DependencyProperty PagingDataGridProperty =
            DependencyProperty.Register("PagingDataGrid", typeof(DataGrid), typeof(PagingControl),
                new FrameworkPropertyMetadata(null, OnPagingDataGridChanged));

		/// <summary>
		/// Gets or sets the PagingDataGrid property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The paging data grid.
		/// </value>
		public DataGrid PagingDataGrid
        {
            get { return (DataGrid)GetValue(PagingDataGridProperty); }
            set { SetValue(PagingDataGridProperty, value); }
        }

		/// <summary>
		/// Handles changes to the PagingDataGrid property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnPagingDataGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (PagingControl)d;
            var oldPagingDataGrid = (DataGrid)e.OldValue;
            var newPagingDataGrid = target.PagingDataGrid;
            target.OnPagingDataGridChanged(oldPagingDataGrid, newPagingDataGrid);
        }

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the PagingDataGrid property
		/// at object level.
		/// </summary>
		/// <param name="oldPagingDataGrid">The old paging data grid.</param>
		/// <param name="newPagingDataGrid">The new paging data grid.</param>
		protected virtual void OnPagingDataGridChanged(DataGrid oldPagingDataGrid, DataGrid newPagingDataGrid)
        {
            if (newPagingDataGrid == null) return;

            newPagingDataGrid.Sorting += newPagingDataGrid_Sorting;

            var itemsSourceDescription = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (itemsSourceDescription != null)
                itemsSourceDescription.AddValueChanged(newPagingDataGrid, DataGrid_ItemsSourceChanged);
        }

		/// <summary>
		/// Handles the Sorting event of the newPagingDataGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DataGridSortingEventArgs"/> instance containing the event data.</param>
		private void newPagingDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var dg = sender as DataGrid;
            if (PagingArguments == null) return;

            e.Handled = true; //no need to sort viewed records because of a new imminent fetch

            var dgsl = PagingArguments.SortingColumns ?? new PagingSortingColumns();

            if (dg != null) dgsl.SetSortList(dg.Columns, e.Column);
            ExecutePaginationCommand();
        }

		/// <summary>
		/// Res the apply sort.
		/// </summary>
		private void ReApplySort()
        {
            if (PagingArguments.SortingColumns == null) return;

            foreach (var sortProperty in PagingArguments.SortingColumns)
            {
                var sortingCol = PagingDataGrid.Columns.FirstOrDefault(c => (c.Header as string) == sortProperty.SortMemberPath);
                if (sortingCol == null) continue;
                sortingCol.SortDirection = sortProperty.SortDirection;
            }
        }

		/// <summary>
		/// Handles the ItemsSourceChanged event of the DataGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void DataGrid_ItemsSourceChanged(object sender, EventArgs e)
        {
            var dg = sender as DataGrid;
            if (dg != null)
            {
                var pagingArguments = dg.ItemsSource as IPagingArguments;

                PagingArguments = pagingArguments;

                if (pagingArguments == null) return;
            }
            //Sorting visual cues need to be re-applied because of reload of a new page
            ReApplySort();
        }


		#endregion

		#region PagingArguments

		/// <summary>
		/// PagingArguments Dependency Property
		/// </summary>
		public static readonly DependencyProperty PagingArgumentsProperty =
            DependencyProperty.Register("PagingArguments", typeof(IPagingArguments), typeof(PagingControl),
                new FrameworkPropertyMetadata(null, OnPagingArgumentsChanged));

		/// <summary>
		/// Gets or sets the PagingArguments property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The paging arguments.
		/// </value>
		public IPagingArguments PagingArguments
        {
            get { return (IPagingArguments)GetValue(PagingArgumentsProperty); }
            set { SetValue(PagingArgumentsProperty, value); }
        }

		/// <summary>
		/// Handles changes to the PagingArguments property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnPagingArgumentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagingControl target = (PagingControl)d;
            IPagingArguments oldPagingArguments = (IPagingArguments)e.OldValue;
            IPagingArguments newPagingArguments = target.PagingArguments;
            target.OnPagingArgumentsChanged(oldPagingArguments, newPagingArguments);
        }

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the PagingArguments property.
		/// </summary>
		/// <param name="oldPagingArguments">The old paging arguments.</param>
		/// <param name="newPagingArguments">The new paging arguments.</param>
		protected virtual void OnPagingArgumentsChanged(IPagingArguments oldPagingArguments,
            IPagingArguments newPagingArguments)
        {
            if (oldPagingArguments != null)
                oldPagingArguments.PropertyChanged -= newPagingArguments_PropertyChanged;
            if (newPagingArguments != null)
                newPagingArguments.PropertyChanged += newPagingArguments_PropertyChanged;
        }

		/// <summary>
		/// Handles the PropertyChanged event of the newPagingArguments control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
		private void newPagingArguments_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //var pagingArgs = sender as IPagingArguments;
            if (e.PropertyName == "PageSize") ExecutePaginationCommand();
            if (e.PropertyName == "PageIndex") ExecutePaginationCommand();
        }

        #endregion

        #endregion
    }

	/// <summary>
	/// 
	/// </summary>
	public enum PageRequest
    {
		/// <summary>
		/// The first page
		/// </summary>
		FirstPage,
		/// <summary>
		/// The next page
		/// </summary>
		NextPage,
		/// <summary>
		/// The previous page
		/// </summary>
		PreviousPage,
		/// <summary>
		/// The last page
		/// </summary>
		LastPage
	}
}
