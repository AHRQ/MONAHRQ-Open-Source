using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Theme.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;

	/// <summary>
	/// Static class used to attach to wpf control
	/// </summary>
	public static class GridViewColumnResize
    {
		#region DependencyProperties

		/// <summary>
		/// The width property
		/// </summary>
		public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached("Width", typeof(string), typeof(GridViewColumnResize),
                                                new PropertyMetadata(OnSetWidthCallback));

		/// <summary>
		/// The grid view column resize behavior property
		/// </summary>
		public static readonly DependencyProperty GridViewColumnResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("GridViewColumnResizeBehavior",
                                                typeof(GridViewColumnResizeBehavior), typeof(GridViewColumnResize),
                                                null);

		/// <summary>
		/// The enabled property
		/// </summary>
		public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(GridViewColumnResize),
                                                new PropertyMetadata(OnSetEnabledCallback));

		/// <summary>
		/// The ListView resize behavior property
		/// </summary>
		public static readonly DependencyProperty ListViewResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("ListViewResizeBehaviorProperty",
                                                typeof(ListViewResizeBehavior), typeof(GridViewColumnResize), null);

		#endregion

		/// <summary>
		/// Gets the width.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static string GetWidth(DependencyObject obj)
        {
            return (string)obj.GetValue(WidthProperty);
        }

		/// <summary>
		/// Sets the width.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">The value.</param>
		public static void SetWidth(DependencyObject obj, string value)
        {
            obj.SetValue(WidthProperty, value);
        }

		/// <summary>
		/// Gets the enabled.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

		/// <summary>
		/// Sets the enabled.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }


		#region CallBack



		/// <summary>
		/// Called when [set width callback].
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnSetWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {

            var element = dependencyObject as GridViewColumn;

            if (element != null)
            {

                GridViewColumnResizeBehavior behavior = GetOrCreateBehavior(element);

                behavior.Width = e.NewValue as string;

            }

            else
            {

                Console.Error.WriteLine("Error: Expected type GridViewColumn but found " +

                                        dependencyObject.GetType().Name);

            }

        }



		/// <summary>
		/// Called when [set enabled callback].
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnSetEnabledCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {

            var element = dependencyObject as ListView;

            if (element != null)
            {

                ListViewResizeBehavior behavior = GetOrCreateBehavior(element);

                behavior.Enabled = (bool)e.NewValue;

            }

            else
            {

                Console.Error.WriteLine("Error: Expected type ListView but found " + dependencyObject.GetType().Name);

            }

        }





		/// <summary>
		/// Gets the or create behavior.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
		private static ListViewResizeBehavior GetOrCreateBehavior(ListView element)
        {

            var behavior = element.GetValue(GridViewColumnResizeBehaviorProperty) as ListViewResizeBehavior;

            if (behavior == null)
            {

                behavior = new ListViewResizeBehavior(element);

                element.SetValue(ListViewResizeBehaviorProperty, behavior);

            }



            return behavior;

        }



		/// <summary>
		/// Gets the or create behavior.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
		private static GridViewColumnResizeBehavior GetOrCreateBehavior(GridViewColumn element)
        {

            var behavior = element.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;

            if (behavior == null)
            {

                behavior = new GridViewColumnResizeBehavior(element);

                element.SetValue(GridViewColumnResizeBehaviorProperty, behavior);

            }



            return behavior;

        }



		#endregion



		#region Nested type: GridViewColumnResizeBehavior



		/// <summary>
		/// GridViewColumn class that gets attached to the GridViewColumn control
		/// </summary>

		public class GridViewColumnResizeBehavior
        {

			/// <summary>
			/// The element
			/// </summary>
			private readonly GridViewColumn _element;



			/// <summary>
			/// Initializes a new instance of the <see cref="GridViewColumnResizeBehavior"/> class.
			/// </summary>
			/// <param name="element">The element.</param>
			public GridViewColumnResizeBehavior(GridViewColumn element)
            {

                _element = element;

            }



			/// <summary>
			/// Gets or sets the width.
			/// </summary>
			/// <value>
			/// The width.
			/// </value>
			public string Width { get; set; }



			/// <summary>
			/// Gets a value indicating whether this instance is static.
			/// </summary>
			/// <value>
			///   <c>true</c> if this instance is static; otherwise, <c>false</c>.
			/// </value>
			public bool IsStatic
            {

                get { return StaticWidth >= 0; }

            }



			/// <summary>
			/// Gets the width of the static.
			/// </summary>
			/// <value>
			/// The width of the static.
			/// </value>
			public double StaticWidth
            {

                get
                {

                    double result;

                    return double.TryParse(Width, out result) ? result : -1;

                }

            }



			/// <summary>
			/// Gets the percentage.
			/// </summary>
			/// <value>
			/// The percentage.
			/// </value>
			public double Percentage
            {

                get
                {

                    if (!IsStatic)
                    {

                        return Mulitplier * 100;

                    }

                    return 0;

                }

            }



			/// <summary>
			/// Gets the mulitplier.
			/// </summary>
			/// <value>
			/// The mulitplier.
			/// </value>
			public double Mulitplier
            {

                get
                {

                    if (Width == "*" || Width == "1*") return 1;

                    if (Width.EndsWith("*"))
                    {

                        double perc;

                        if (double.TryParse(Width.Substring(0, Width.Length - 1), out perc))
                        {

                            return perc;

                        }

                    }

                    return 1;

                }

            }



			/// <summary>
			/// Sets the width.
			/// </summary>
			/// <param name="allowedSpace">The allowed space.</param>
			/// <param name="totalPercentage">The total percentage.</param>
			public void SetWidth(double allowedSpace, double totalPercentage)
            {

                if (IsStatic)
                {

                    _element.Width = StaticWidth;

                }

                else
                {

                    double width = allowedSpace * (Percentage / totalPercentage);

                    _element.Width = width;

                }

            }

        }



		#endregion



		#region Nested type: ListViewResizeBehavior



		/// <summary>
		/// ListViewResizeBehavior class that gets attached to the ListView control
		/// </summary>

		public class ListViewResizeBehavior
        {

			/// <summary>
			/// The margin
			/// </summary>
			private const int Margin = 25;

			/// <summary>
			/// The refresh time
			/// </summary>
			private const long RefreshTime = Timeout.Infinite;

			/// <summary>
			/// The delay
			/// </summary>
			private const long Delay = 500;



			/// <summary>
			/// The element
			/// </summary>
			private readonly ListView _element;

			/// <summary>
			/// The timer
			/// </summary>
			private readonly Timer _timer;



			/// <summary>
			/// Initializes a new instance of the <see cref="ListViewResizeBehavior"/> class.
			/// </summary>
			/// <param name="element">The element.</param>
			/// <exception cref="ArgumentNullException">element</exception>
			public ListViewResizeBehavior(ListView element)
            {

                if (element == null) throw new ArgumentNullException("element");

                _element = element;

                element.Loaded += OnLoaded;



                // Action for resizing and re-enable the size lookup

                // This stops the columns from constantly resizing to improve performance

                Action resizeAndEnableSize = () =>
                                                 {

                                                     Resize();

                                                     _element.SizeChanged += OnSizeChanged;

                                                 };

                _timer = new Timer(x => Application.Current.Dispatcher.BeginInvoke(resizeAndEnableSize), null, Delay,

                                   RefreshTime);

            }



			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="ListViewResizeBehavior"/> is enabled.
			/// </summary>
			/// <value>
			///   <c>true</c> if enabled; otherwise, <c>false</c>.
			/// </value>
			public bool Enabled { get; set; }





			/// <summary>
			/// Called when [loaded].
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
			private void OnLoaded(object sender, RoutedEventArgs e)
            {

                _element.SizeChanged += OnSizeChanged;

            }



			/// <summary>
			/// Called when [size changed].
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
			private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            {

                if (e.WidthChanged)
                {

                    _element.SizeChanged -= OnSizeChanged;

                    _timer.Change(Delay, RefreshTime);

                }

            }



			/// <summary>
			/// Resizes this instance.
			/// </summary>
			private void Resize()
            {

                if (Enabled)
                {

                    double totalWidth = _element.ActualWidth;

                    var gv = _element.View as GridView;

                    if (gv != null)
                    {

                        double allowedSpace = totalWidth - GetAllocatedSpace(gv);

                        allowedSpace = allowedSpace - Margin;

                        double totalPercentage = GridViewColumnResizeBehaviors(gv).Sum(x => x.Percentage);

                        foreach (GridViewColumnResizeBehavior behavior in GridViewColumnResizeBehaviors(gv))
                        {

                            behavior.SetWidth(allowedSpace, totalPercentage);

                        }

                    }

                }

            }



			/// <summary>
			/// Grids the view column resize behaviors.
			/// </summary>
			/// <param name="gv">The gv.</param>
			/// <returns></returns>
			private static IEnumerable<GridViewColumnResizeBehavior> GridViewColumnResizeBehaviors(GridView gv)
            {

                foreach (GridViewColumn t in gv.Columns)
                {

                    var gridViewColumnResizeBehavior =

                        t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;

                    if (gridViewColumnResizeBehavior != null)
                    {

                        yield return gridViewColumnResizeBehavior;

                    }

                }

            }



			/// <summary>
			/// Gets the allocated space.
			/// </summary>
			/// <param name="gv">The gv.</param>
			/// <returns></returns>
			private static double GetAllocatedSpace(GridView gv)
            {

                double totalWidth = 0;

                foreach (GridViewColumn t in gv.Columns)
                {

                    var gridViewColumnResizeBehavior =

                        t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;

                    if (gridViewColumnResizeBehavior != null)
                    {

                        if (gridViewColumnResizeBehavior.IsStatic)
                        {

                            totalWidth += gridViewColumnResizeBehavior.StaticWidth;

                        }

                    }

                    else
                    {

                        totalWidth += t.ActualWidth;

                    }

                }

                return totalWidth;

            }

        }



        #endregion

    }

}
