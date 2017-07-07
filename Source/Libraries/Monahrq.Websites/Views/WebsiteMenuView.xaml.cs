using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using MenuItem = Monahrq.Infrastructure.Domain.Websites.Menu;
using Monahrq.Infrastructure.Domain.Websites;
using System.Collections.ObjectModel;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteMenuView.xaml
    /// </summary>
    public partial class WebsiteMenuView : UserControl
    {
        #region Fields and Constants

        private static string DefaultContainerWidth = "700";
        private static int MinimumWidth = 75;

        #endregion

        #region Dependency Properties

        public string ParentContainerWidth
        {
            get { return (string)GetValue(ContainerWidthProperty); }
            set { SetValue(ContainerWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContainerWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerWidthProperty =
            DependencyProperty.Register("ParentContainerWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata(DefaultContainerWidth, ParendContainerWidthChanged));

        public string MenuColumnWidth
        {
            get { return (string)GetValue(MenuColumnWidthPropertyProperty); }
            set { SetValue(MenuColumnWidthPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MenuColumnWidthProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MenuColumnWidthPropertyProperty =
            DependencyProperty.Register("MenuColumnWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata("450", MenuColumnWidthChanged));

        public string ButtonsColumnWidth
        {
            get { return (string)GetValue(ButtonsColumnWidthPropertyProperty); }
            set { SetValue(ButtonsColumnWidthPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonsColumnWidthProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsColumnWidthPropertyProperty =
            DependencyProperty.Register("ButtonsColumnWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata("150"));

        public string CheckBoxColumnWidth
        {
            get { return (string)GetValue(CheckBoxColumnWidthProperty); }
            set { SetValue(CheckBoxColumnWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckBoxColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckBoxColumnWidthProperty =
            DependencyProperty.Register("CheckBoxColumnWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata("150"));

        public string SubMenuColumnWidth
        {
            get { return (string)GetValue(SubMenuColumnWidthProperty); }
            set { SetValue(SubMenuColumnWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubMenuColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubMenuColumnWidthProperty =
            DependencyProperty.Register("SubMenuColumnWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata("433"));


        public string SubSubMenuColumnWidth
        {
            get { return (string)GetValue(SubSubMenuColumnWidthProperty); }
            set { SetValue(SubSubMenuColumnWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubSubMenuColumnWidthProperty =
            DependencyProperty.Register("SubSubMenuColumnWidth", typeof(string), typeof(WebsiteMenuView), new PropertyMetadata("418"));


        #endregion

        #region Constructor

        public WebsiteMenuView()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

            InitializeComponent();
        }

        #endregion

        #region Static Methods

        private static void MenuColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (WebsiteMenuView)d;
            if (control != null)
            {
                var menuColumnWidth = Convert.ToDouble(e.NewValue);
                control.SubMenuColumnWidth = string.Format("{0}", menuColumnWidth - 17);
                control.SubSubMenuColumnWidth = string.Format("{0}", menuColumnWidth - (17 * 2));
            }
        }

        private static void ParendContainerWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (WebsiteMenuView)d;
            if (control != null && e.NewValue != e.OldValue && !e.NewValue.ToString().Equals(DefaultContainerWidth))
            {
                var listViewWidth = Convert.ToDouble(e.NewValue);
                var minWidth = string.Format("{0}", Math.Ceiling(listViewWidth * (0.5 / 5)));
                if (Convert.ToDouble(minWidth) < MinimumWidth)
                {
                    control.MenuColumnWidth = string.Format("{0}", listViewWidth - (2 * MinimumWidth));
                    control.ButtonsColumnWidth = string.Format("{0}", MinimumWidth);
                    control.CheckBoxColumnWidth = string.Format("{0}", MinimumWidth);
                }
                else
                {
                    control.MenuColumnWidth = string.Format("{0}", Math.Ceiling(listViewWidth * (4.0 / 5)));
                    control.ButtonsColumnWidth = string.Format("{0}", Math.Ceiling(listViewWidth * (0.5 / 5)));
                    control.CheckBoxColumnWidth = string.Format("{0}", Math.Ceiling(listViewWidth * (0.5 / 5)));
                }
            }
        }

        #endregion
    }

    public class BorderBacgroundConverter : IValueConverter
    {
        private int _menuItemsCount;

        public int Count { get; set; }

        public int MenuItemsCount
        {
            get { return _menuItemsCount; }
            set
            {
                var prevVal = _menuItemsCount;
                _menuItemsCount = value;

                //if (prevVal != 0 && prevVal != _menuItemsCount)
                //{
                //    Count = Count - MenuItemsCount; ;
                //}
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var border = value as Border;
            System.Diagnostics.Debug.Print("Caller method :{0}", parameter);
            var context = (border.DataContext as MenuItem);
            if (context == null)
            {
                System.Diagnostics.Debug.Print("Context is not null {0} : {1}", parameter, border.Name);
                var items = new List<MenuItem>();
                foreach (var item in border.DataContext as ObservableCollection<MenuItem>)
                {
                    item.FindAllChildren(ref items);
                }
                MenuItemsCount = items.Count;
                if (Count >= MenuItemsCount)
                    Count = Count - MenuItemsCount;

            }
            if (MenuItemsCount != 0 && Count >= MenuItemsCount)
                Count = Count - MenuItemsCount;

            Count++;
            if (Count % 2 == 1) return border.FindResource("MGrey2");

            return border.Background;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class SubMenuVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var subMenus = value as ReadOnlyCollection<MenuItem>;

            if (subMenus == null || subMenus.Count == 0) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
