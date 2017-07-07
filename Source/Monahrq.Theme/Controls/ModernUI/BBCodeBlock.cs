using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Monahrq.Theme.Controls.ModernUI.BBCode;
using Monahrq.Theme.Controls.ModernUI.Presentation;

namespace Monahrq.Theme.Controls.ModernUI
{
    public interface ILinkNavigator
    {
        /// <summary>
        /// Gets or sets the navigable commands.
        /// </summary>
        CommandDictionary Commands { get; set; }
        /// <summary>
        /// Performs navigation to specified link.
        /// </summary>
        /// <param name="uri">The uri to navigate to.</param>
        /// <param name="source">The source element that triggers the navigation. Required for frame navigation.</param>
        /// <param name="parameter">An optional command parameter or navigation target.</param>
        void Navigate(Uri uri, FrameworkElement source, string parameter = null);
    }
    /// <summary>
    /// A lighweight control for displaying small amounts of rich formatted BBCode content.
    /// </summary>
    public class BbCodeBlock: TextBlock
    {
        /// <summary>
        /// Identifies the BBCode dependency property.
        /// </summary>
        public static DependencyProperty BbCodeProperty = DependencyProperty.Register("BBCode", typeof(string), typeof(BbCodeBlock), new PropertyMetadata(new PropertyChangedCallback(OnBBCodeChanged)));
        /// <summary>
        /// Identifies the LinkNavigator dependency property.
        /// </summary>
        public static DependencyProperty LinkNavigatorProperty = DependencyProperty.Register("LinkNavigator", typeof(ILinkNavigator), typeof(BbCodeBlock), new PropertyMetadata(new DefaultLinkNavigator(), OnLinkNavigatorChanged));

        private bool _dirty = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BbCodeBlock"/> class.
        /// </summary>
        public BbCodeBlock()
        {
            // ensures the implicit BBCodeBlock style is used
            this.DefaultStyleKey = typeof(BbCodeBlock);

            AddHandler(Hyperlink.LoadedEvent, new RoutedEventHandler(OnLoaded));
            AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnRequestNavigate));
        }

        private static void OnBBCodeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((BbCodeBlock)o).UpdateDirty();
        }

        private static void OnLinkNavigatorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                // null values disallowed
                throw new ArgumentNullException("LinkNavigator");
            }

            ((BbCodeBlock)o).UpdateDirty();
        }

        private void OnLoaded(object o, EventArgs e)
        {
            Update();
        }

        private void UpdateDirty()
        {
            this._dirty = true;
            Update();
        }

        private void Update()
        {
            if (!this.IsLoaded || !this._dirty)
            {
                return;
            }

            var bbcode = this.BBCode;

            this.Inlines.Clear();

            if (!string.IsNullOrWhiteSpace(bbcode))
            {
                Inline inline;
                try
                {
                    var parser = new BBCodeParser(bbcode, this)
                    {
                        Commands = this.LinkNavigator.Commands
                    };
                    inline = parser.Parse();
                }
                catch (Exception)
                {
                    // parsing failed, display BBCode value as-is
                    inline = new Run { Text = bbcode };
                }
                this.Inlines.Add(inline);
            }
            this._dirty = false;
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // perform navigation using the link navigator
                this.LinkNavigator.Navigate(e.Uri, this, e.Target);
            }
            catch (Exception error)
            {
                // display navigation failures
                MessageBox.Show((error.Message), "Navigation Failed", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Gets or sets the BB code.
        /// </summary>
        /// <value>The BB code.</value>
        public string BBCode
        {
            get { return (string)GetValue(BbCodeProperty); }
            set { SetValue(BbCodeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the link navigator.
        /// </summary>
        /// <value>The link navigator.</value>
        public ILinkNavigator LinkNavigator
        {
            get { return (ILinkNavigator)GetValue(LinkNavigatorProperty); }
            set { SetValue(LinkNavigatorProperty, value); }
        }
    }
}
